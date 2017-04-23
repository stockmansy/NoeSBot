using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Database;
using System.Threading;
using System.Net;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Media)]
    public class MediaModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private static IEnumerable<IMessage> _recentMediaMessages = null;

        #region Constructor

        public MediaModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
            
            //_client.MessageReceived += HandleCommand;
        }

        #endregion

        #region Handlers

        public async Task MessageReceivedHandler(SocketMessage messageParam)
        {
            if (!messageParam.Author.IsBot && !messageParam.Author.IsWebhook)
            {
                await ReplyAsync("here");
                //    await CheckRecentMediaMessages();
                //    if (messageParam.Attachments.Any())
                //        await ReplyAsync("ismedia");

                //    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.youtu.be/Ddn4MGaS3N4");
                //    request.Method = "HEAD";
                //    using (var response = await request.GetResponseAsync())
                //    {
                //        Console.WriteLine("Does this resolve to youtube?: {0}", response.ResponseUri.ToString().Contains("youtube.com") ? "Yes" : "No");
                //    }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Commands

        [Command("med")]
        [Summary("Make the bot say ...")]
        public async Task Med([Remainder] string input)
        {
            await ReplyAsync(input);
        }

        #endregion

        #region Private

        private async Task CheckRecentMediaMessages()
        {
            if (_recentMediaMessages == null)
            {
                var messages = ((IMessageChannel)_client.GetChannel(305231577040945164)).GetMessagesAsync(100, CacheMode.CacheOnly);
                var flatten = await messages.Flatten();
                _recentMediaMessages = flatten;
            }
        }

        #endregion
    }
}
