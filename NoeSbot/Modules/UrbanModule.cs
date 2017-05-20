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

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Urban)]
    public class UrbanModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private const string apiBaseUrl = "http://api.urbandictionary.com";
        private const string apiQueryPath = "/v0/define?term=";
        private readonly Dictionary<ulong, UrbanMain> _urbans;
        private Dictionary<ulong, string> _messagesBeingAdjusted;

        #region Constructor

        public UrbanModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;

            _urbans = new Dictionary<ulong, UrbanMain>();
            _messagesBeingAdjusted = new Dictionary<ulong, string>();
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

                if (_messagesBeingAdjusted.TryGetValue(message.Id, out string author))
                    return;

                _messagesBeingAdjusted.Add(message.Id, userAdjusting.Username);

                var name = reaction.Emoji.Name;

                await message.RemoveReactionAsync(reaction.Emoji.Name, reaction.User.Value);

                if (name.Equals(IconHelper.ArrowDown))
                {
                    var current = urbanMain.CurrentDef;
                    var next = current + 1;
                    if (urbanMain.DefinitionCount < next)
                        return;

                    SetCurrentDefinition(urbanMain, next);

                } else if (name.Equals(IconHelper.ArrowUp))
                {
                    var current = urbanMain.CurrentDef;
                    var previous = current - 1;
                    if (previous < 1)
                        return;

                    SetCurrentDefinition(urbanMain, previous);
                } else if (name.Equals(IconHelper.ArrowLeft))
                {
                    var current = urbanMain.CurrentPage;
                    var previous = current - 1;
                    if (previous < 1)
                        return;

                    SetCurrentPage(urbanMain, previous);
                } else if (name.Equals(IconHelper.ArrowRight))
                {
                    var current = urbanMain.CurrentPage;
                    var next = current + 1;
                    if (urbanMain.PagesCount < next)
                        return;

                    SetCurrentPage(urbanMain, next);
                }

                await message.ModifyAsync(x => x.Embed = GenerateUrban(urbanMain, author: userAdjusting.Username));

                _messagesBeingAdjusted.Remove(message.Id);
            }
        }

        #endregion

        #region Help text

        [Command("urban")]
        [Alias("lookup")]
        [Summary("Create a quick poll")]
        [MinPermissions(AccessLevel.User)]
        public async Task Urban()
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can retrieve an urban dictionary item:"
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter: The subject";
                x.Value = "Provide a subject to learn more about from the urban dictionary.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Navigation";
                x.Value = $"You can navigate to other urban dictionary entries with the arrow icons. {Environment.NewLine}Discord has a 2000 character limit so some entries will be paginated.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example";
                x.Value = $"~urban mango";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Commands

        [Command("urban")]
        [Alias("lookup")]
        [Summary("Create a quick poll")]
        [MinPermissions(AccessLevel.User)]
        public async Task Urban([Remainder] string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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

                var iconsToAdd = new List<string>();
                Embed embed = null;
                bool multiplePages = false;

                if (urbanMain.List.Count > 1)
                {
                    iconsToAdd.Add(IconHelper.ArrowUp);
                    iconsToAdd.Add(IconHelper.ArrowDown);
                }

                urbanMain = SetCurrentDefinition(urbanMain, 1);

                if (urbanMain.Pages.Length > 1)
                    multiplePages = true;

                embed = GenerateUrban(urbanMain, true);

                if (multiplePages)
                {
                    iconsToAdd.Add(IconHelper.ArrowLeft);
                    iconsToAdd.Add(IconHelper.ArrowRight);
                }

                if (embed == null)
                {
                    await ReplyAsync("Could not generate the urban dictionary item.");
                    return;
                }

                var message = await ReplyAsync("", false, embed);

                _urbans.Add(message.Id, urbanMain);

                foreach (var icon in iconsToAdd) {
                    await message.AddReactionAsync(icon);
                    await Task.Delay(1250);
                }

                if (urbanMain.DefinitionCount > 1)
                    _client.ReactionAdded += OnReactionAdded;

                await message.ModifyAsync(x => x.Embed = GenerateUrban(urbanMain));
            }
        }

        #endregion

        #region Private

        private Embed GenerateUrban(UrbanMain urbanMain, bool initializing = false, string author = "")
        {
            var user = Context.User as SocketGuildUser;
            var hasAuthor = !string.IsNullOrWhiteSpace(author);
            var footer = "Click the arrows for more definitions";

            if (hasAuthor)
                footer = $"{footer} (adjusted by {author})";

            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = "https://www.melhorcambio.com/images/question-mark.png",
                    Text = footer
                }
            };

            if (initializing) {
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

        #endregion
    }
}
