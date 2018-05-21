using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using NoeSbot.Models;
using Newtonsoft.Json;
using Discord;
using NoeSbot.Helpers;
using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using NoeSbot.Resources;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Urban)]
    public class UrbanModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private const string apiBaseUrl = "http://api.urbandictionary.com";
        private const string apiQueryPath = "/v0/define?term=";
        private readonly LimitedDictionary<ulong, UrbanMain> _urbans;

        #region Constructor

        public UrbanModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;

            _urbans = new LimitedDictionary<ulong, UrbanMain>();
        }

        #endregion

        #region Handlers

        protected async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null || !reaction.User.IsSpecified)
                return;

            var user = Context.User as SocketGuildUser;
            var userAdjusting = reaction.User.Value;

            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !userAdjusting.IsBot && !userAdjusting.IsWebhook)
            {
                var success = _urbans.TryGetValue(message.Id, out UrbanMain urbanMain);
                if (!success)
                    return;

                var name = reaction.Emote.Name;

                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                if (urbanMain.AuthorId != userAdjusting.Id)
                    return;

                if (urbanMain.InHelpMode && !name.Equals(IconHelper.Question))
                {
                    await message.ModifyAsync(x => x.Embed = GenerateUrban(urbanMain, author: userAdjusting.Username));
                    urbanMain.InHelpMode = false;
                    return;
                }

                new Thread(async () =>
                {
                    if (name.Equals(IconHelper.ArrowDown))
                    {
                        var current = urbanMain.CurrentDef;
                        var next = current + 1;
                        if (urbanMain.DefinitionCount < next)
                            return;

                        SetCurrentDefinition(urbanMain, next);
                        await SetPagingAsync(message, urbanMain);

                    }
                    else if (name.Equals(IconHelper.ArrowUp))
                    {
                        var current = urbanMain.CurrentDef;
                        var previous = current - 1;
                        if (previous < 1)
                            return;

                        SetCurrentDefinition(urbanMain, previous);
                        await SetPagingAsync(message, urbanMain);
                    }
                    else if (name.Equals(IconHelper.ArrowLeft))
                    {
                        var current = urbanMain.CurrentPage;
                        var previous = current - 1;
                        if (previous < 1)
                            return;

                        SetCurrentPage(urbanMain, previous);
                        await SetPagingAsync(message, urbanMain);
                    }
                    else if (name.Equals(IconHelper.ArrowRight))
                    {
                        var current = urbanMain.CurrentPage;
                        var next = current + 1;
                        if (urbanMain.PagesCount < next)
                            return;

                        SetCurrentPage(urbanMain, next);
                        await SetPagingAsync(message, urbanMain);
                    }
                    else if (name.Equals(IconHelper.Question))
                    {
                        await message.ModifyAsync(x => x.Embed = GenerateHelpUrban(userAdjusting.Username));
                        urbanMain.InHelpMode = true;
                        return;
                    }

                    await message.ModifyAsync(x => x.Embed = GenerateUrban(urbanMain, author: userAdjusting.Username));
                }).Start();
            }
        }

        #endregion

        #region Commands

        #region Urban

        [Command(Labels.Urban_Urban_Command)]
        [Alias(Labels.Urban_Urban_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Urban()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Urban_Urban_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }

        [Command(Labels.Urban_Urban_Command)]
        [Alias(Labels.Urban_Urban_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Urban([Remainder] string input)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(apiBaseUrl)
            };

            UrbanMain urbanMain = null;
            using (var cl = client)
            {
                var json = await cl.GetStringAsync($"{apiBaseUrl}{apiQueryPath}{input}");
                urbanMain = JsonConvert.DeserializeObject<UrbanMain>(json);
            }

            if (urbanMain == null || urbanMain.List == null || !urbanMain.List.Any())
            {
                await ReplyAsync("Could not retrieve the requested term.");
                return;
            }

            urbanMain.AuthorId = Context.Message.Author.Id;

            var iconsToAdd = new List<string>();
            Embed embed = null;
            bool multiplePages = false;

            if (urbanMain.List.Count > 1)
            {
                iconsToAdd.Add(IconHelper.ArrowUp);
                iconsToAdd.Add(IconHelper.ArrowDown);
                iconsToAdd.Add(IconHelper.Question);
            }

            urbanMain = SetCurrentDefinition(urbanMain, 1);

            if (urbanMain.Pages.Length > 1)
                multiplePages = true;

            embed = GenerateUrban(urbanMain, true);

            if (multiplePages)
            {
                iconsToAdd.Add(IconHelper.ArrowLeft);
                iconsToAdd.Add(IconHelper.ArrowRight);
                urbanMain.PageIconsSet = true;
            }

            if (embed == null)
            {
                await ReplyAsync("Could not generate the urban dictionary item.");
                return;
            }

            var message = await ReplyAsync("", false, embed);

            _urbans.Add(message.Id, urbanMain);

            new Thread(async () =>
            {
                foreach (var icon in iconsToAdd)
                {
                    var emote = IconHelper.GetEmote(icon);
                    await message.AddReactionAsync(emote);
                    Thread.Sleep(1300);
                }

                if (urbanMain.DefinitionCount > 1)
                    _client.ReactionAdded += OnReactionAdded;

                await message.ModifyAsync(x => x.Embed = GenerateUrban(urbanMain));
            }).Start();
        }

        #endregion

        #endregion

        #region Private

        private Embed GenerateUrban(UrbanMain urbanMain, bool initializing = false, string author = "")
        {
            var user = Context.User as SocketGuildUser;
            var hasAuthor = !string.IsNullOrWhiteSpace(author);
            var footer = "Click the arrows for more definitions (only the author can switch pages)";

            if (hasAuthor)
                footer = $"{footer} ({author})";

            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = "https://www.melhorcambio.com/images/question-mark.png",
                    Text = footer
                }
            };

            if (initializing)
            {
                builder.Description = "Loading...";
                builder.ThumbnailUrl = "https://cdn2.iconfinder.com/data/icons/loading-3/100/load01-256.png";
            }

            var definition = urbanMain.Pages[urbanMain.CurrentPage - 1];

            if (urbanMain.CurrentPage > 1)
                definition = $"... {definition}";

            if (urbanMain.CurrentPage < urbanMain.PagesCount)
                definition = $"{definition} ...";

            if (urbanMain.PagesCount > 1)
                definition = $"{definition}{Environment.NewLine}{Environment.NewLine}Page {urbanMain.CurrentPage} of {urbanMain.PagesCount} ";

            var curDef = urbanMain.DefinitionCount > 1 ? $"{urbanMain.CurrentDef} of {urbanMain.DefinitionCount}" : $"{urbanMain.DefinitionCount}";

            builder.AddField(x =>
            {
                x.Name = $"Definition {curDef}";
                x.Value = definition ?? "";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Link";
                x.Value = urbanMain.CurrentItem?.Permalink ?? "";
                x.IsInline = false;
            });

            return builder.Build();
        }

        private Embed GenerateHelpUrban(string author = "")
        {
            var user = Context.User as SocketGuildUser;
            var hasAuthor = !string.IsNullOrWhiteSpace(author);
            var footer = "Click the arrows for more definitions";

            if (hasAuthor)
                footer = $"{footer} ({author})";

            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                ThumbnailUrl = "http://icons.veryicon.com/256/Application/isabi/Help.png",
                Description = "Help",
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = "https://www.melhorcambio.com/images/question-mark.png",
                    Text = footer
                }
            };

            builder.AddField(x =>
            {
                x.Name = "How to use";
                x.Value = "Use the up and down arrows to switch between the various definitions.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "How to switch pages";
                x.Value = "Use the left and right arrows to switch between pages.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Why use...";
                x.Value = $"Discord has a 2000 character limit for each message, so some urban dictionary entries need to be split up.{Environment.NewLine}The arrows are added slowly because of the rate limit discord set on reactions (Yes, odd)";
                x.IsInline = false;
            });

            return builder.Build();
        }

        private UrbanMain SetCurrentDefinition(UrbanMain urbanMain, int defId)
        {
            var definition = urbanMain.List[(defId - 1)];
            var pages = CommonHelper.GetSplitIntoPages(definition.Definition, 1000);

            urbanMain.Pages = pages;
            urbanMain.CurrentDef = defId;
            urbanMain.CurrentPage = 1;

            return urbanMain;
        }

        private UrbanMain SetCurrentPage(UrbanMain urbanMain, int pageId)
        {
            urbanMain.CurrentPage = pageId;

            return urbanMain;
        }

        private async Task SetPagingAsync(IUserMessage message, UrbanMain urbanMain)
        {
            if (urbanMain.PageIconsSet && urbanMain.PagesCount <= 1)
            {
                await message.RemoveReactionAsync(IconHelper.GetEmote(IconHelper.ArrowLeft), message.Author);
                await message.RemoveReactionAsync(IconHelper.GetEmote(IconHelper.ArrowRight), message.Author);
                urbanMain.PageIconsSet = false;
            }

            if (!urbanMain.PageIconsSet && urbanMain.PagesCount > 1)
            {
                await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.ArrowLeft));
                await Task.Delay(1300);
                await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.ArrowRight));
                await Task.Delay(1300);
                urbanMain.PageIconsSet = true;
            }
        }

        #endregion
    }
}
