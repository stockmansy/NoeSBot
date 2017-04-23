using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;

namespace NoeSbot.Logic
{
    public class MediaProcessor
    {
        private CommandContext _context;
        private IDependencyMap _map;
        private IEnumerable<IMessage> _recentMediaMessages = null;

        public MediaProcessor(CommandContext context, IDependencyMap map)
        {
            _context = context;
            _map = map;
        }

        public async Task Process()
        {
            var matches = Regex.Matches(_context.Message.Content, @"(www.+|http.+)([\s]|$)");
            if (matches.Count <= 0)
                await Task.CompletedTask;

            var channels = await _context.Guild.GetChannelsAsync();
            var mediaChannel = channels.Where(x => x.Name.Equals(Configuration.Load(_context.Guild.Id).MediaChannel, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() as IMessageChannel;

            if (mediaChannel == null)
                return;


            //await CheckRecentMediaMessages();

            foreach (Match match in matches)
            {
                var isMedia = CheckForMedia(match.Value);
                if (isMedia)
                {
                    await mediaChannel.SendMessageAsync(match.Value);
                }
            }
        }

        #region Private

        //private async Task CheckRecentMediaMessages()
        //{
        //    if (_recentMediaMessages == null)
        //    {
        //        var messages = ((IMessageChannel)_client.GetChannel(302738686737514497)).GetMessagesAsync(100, CacheMode.CacheOnly);
        //        var flatten = await messages.Flatten();
        //        _recentMediaMessages = flatten;
        //    }

        //    await Task.CompletedTask;
        //}

        private bool CheckForMedia(string input)
        {
            var matches = Regex.Matches(input, @"(^https?://(www\.)?youtube\.com/.*v=\w+)|(^https?://youtu\.be/\w+)|(src=.https?://(www\.)?youtube\.com/v/\w+)|(src=.https?://(www\.)?youtube\.com/embed/\w+)", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
                return true;

            return false;
        }

        #endregion
    }
}
