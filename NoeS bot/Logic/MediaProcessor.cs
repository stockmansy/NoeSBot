using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using Discord.WebSocket;
using NoeSbot.Helpers;

namespace NoeSbot.Logic
{
    public class MediaProcessor
    {
        private CommandContext _context;
        private IDependencyMap _map;

        public MediaProcessor(CommandContext context, IDependencyMap map)
        {
            _context = context;
            _map = map;
        }

        public async Task Process()
        {
            if (!_context.Channel.Name.Equals(Configuration.Load(_context.Guild.Id).GeneralChannel))
                return;

            var matches = Regex.Matches(_context.Message.Content, @"(www.+|http.+)([\s]|$)");
            if (matches.Count <= 0 && !_context.Message.Attachments.Any())
                return;

            var channels = await _context.Guild.GetChannelsAsync();
            var mediaChannel = channels.Where(x => x.Name.Equals(Configuration.Load(_context.Guild.Id).MediaChannel, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() as IMessageChannel;

            var messages = mediaChannel.GetMessagesAsync(100, CacheMode.AllowDownload);
            var flatten = await messages.Flatten();

            var user = _context.User as SocketGuildUser;

            if (_context.Message.Attachments.Any())
            {
                foreach (var attach in _context.Message.Attachments)
                {
                    await ProcessMediaAsync(flatten, attach.Url, user, mediaChannel);
                }
            }

            if (matches.Count <= 0 || mediaChannel == null)
                return;

            foreach (Match match in matches)
            {
                if (CheckForEmbed())//CheckForMedia(match.Value))            
                    await ProcessMediaAsync(flatten, match.Value, user, mediaChannel);
            }
        }

        #region Private

        private async Task ProcessMediaAsync(IEnumerable<IMessage> flatten, string input, SocketGuildUser user, IMessageChannel mediaChannel)
        {
            var builder = new StringBuilder();

            var isRepost = false;
            foreach (var f in flatten)
            {
                var flattenMsg = Regex.Matches(f.Content, @"(www.+|http.+)([\s]|$)");
                if (flattenMsg.Count <= 0)
                    continue;

                var mentionedUser = f.Author.Username;
                var indexOf = f.Content.IndexOf("posted new");
                if (indexOf > -1)
                    mentionedUser = f.Content.Substring(0, indexOf).Trim();

                foreach (Match fm in flattenMsg)
                    if (fm.Value.Equals(input, StringComparison.OrdinalIgnoreCase))
                    {
                        isRepost = true;
                        break;
                    }

                if (isRepost)
                {
                    builder.AppendLine("Repost!");
                    builder.AppendLine($"{mentionedUser} posted the following media on {f.CreatedAt.ToString("dd-MM-yyyy hh:mm")}");
                    builder.AppendLine("```");
                    builder.AppendLine($"{input}");
                    builder.AppendLine("```");
                    break;
                }
            }

            if (!isRepost)
            {
                builder.AppendLine($"{user.Username} posted new media:");
                builder.AppendLine($"{input}");
            }

            await mediaChannel.SendMessageAsync(builder.ToString());
        }

        private bool CheckForMedia(string input)
        {
            var matches = Regex.Matches(input, @"(^https?://(www\.)?youtube\.com/.*v=\w+)|(^https?://youtu\.be/\w+)|(src=.https?://(www\.)?youtube\.com/v/\w+)|(src=.https?://(www\.)?youtube\.com/embed/\w+)", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
                return true;

            return false;
        }

        private bool CheckForEmbed()
        {
            if (_context.Message.Embeds.Any())
                return true;
            return false;
        }

        #endregion
    }
}
