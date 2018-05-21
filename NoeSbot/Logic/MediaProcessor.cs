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
        public async Task Process(ICommandContext context)
        {
            if (!context.Channel.Name.Equals(Configuration.Load(context.Guild.Id).GeneralChannel))
                return;

            var matches = Regex.Matches(context.Message.Content, @"(www.+|http.+)([\s]|$)");
            if (matches.Count <= 0 && !context.Message.Attachments.Any())
                return;

            var channels = await context.Guild.GetChannelsAsync();
            var mediaChannel = channels.Where(x => x.Name != null && x.Name.Equals(Configuration.Load(context.Guild.Id).MediaChannel, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() as IMessageChannel;

            var messages = await mediaChannel.GetMessagesAsync(80).Flatten();

            var user = context.User as SocketGuildUser;

            if (context.Message.Attachments.Any())
            {
                foreach (var attach in context.Message.Attachments)
                {
                    await ProcessMediaAsync(messages, attach.Url, user, mediaChannel);
                }
            }

            if (matches.Count <= 0 || mediaChannel == null)
                return;
            
            foreach (Match match in matches)
            {
                if (context.Message.Embeds.Any())//CheckForMedia(match.Value))            
                    await ProcessMediaAsync(messages, match.Value, user, mediaChannel);
            }
        }

        #region Private

        private async Task ProcessMediaAsync(IEnumerable<IMessage> messages, string input, SocketGuildUser user, IMessageChannel mediaChannel)
        {
            var builder = new StringBuilder();

            var isRepost = false;
            foreach (var f in messages)
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

        #endregion
    }
}
