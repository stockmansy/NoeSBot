using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
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
        }

        #endregion

        #region Handlers

       

        #endregion

        #region Commands

        

        #endregion

        #region Private

        

        #endregion
    }
}
