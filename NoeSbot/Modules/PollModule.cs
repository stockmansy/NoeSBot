using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NoeSbot.Database.Models;
using NoeSbot.Extensions;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Poll)]
    public class PollModule : ModuleBase
    {
        private const int _defaultDuration = 60;
        private readonly DiscordSocketClient _client;
        private readonly IMemoryCache _cache;
        private readonly Dictionary<ulong, PollModel> _polls;
        private readonly Dictionary<int, string> _reactions = new Dictionary<int, string>
                                                     {
                                                         { 1, "1⃣" },
                                                         { 2, "2⃣" },
                                                         { 3, "3⃣" },
                                                         { 4, "4⃣" },
                                                         { 5, "5⃣" },
                                                         { 6, "6⃣" },
                                                         { 7, "7⃣" },
                                                         { 8, "8⃣" },
                                                         { 9, "9⃣" },
                                                         { 10, "0⃣" },
                                                         { 11, "🇦" },
                                                         { 12, "🇧" },
                                                         { 13, "🇨" },
                                                         { 14, "🇩" },
                                                         { 15, "🇪" },
                                                         { 16, "🇫" },
                                                         { 17, "🇬" },
                                                         { 18, "🇭" },
                                                         { 19, "🇮" },
                                                         { 20, "🇯" }
                                                     };


        public PollModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
            
            _polls = new Dictionary<ulong, PollModel>();
        }

        #region Handlers

        protected async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null || !reaction.User.IsSpecified)
                return;

            var user = Context.User as SocketGuildUser;
            var userVoting = reaction.User.Value;
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !userVoting.IsBot && !userVoting.IsWebhook)
            {
                var success = _polls.TryGetValue(message.Id, out PollModel poll);
                if (!success)
                    return;

                var alreadyVoted = poll.UsersWhoVoted.TryGetValue(userVoting, out int votedOn);
                if (alreadyVoted)
                {
                    await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }
                    
                var chosenOption = _reactions.Where(pair => pair.Value == reaction.Emote.Name)
                                    .Select(pair => pair.Key)
                                    .FirstOrDefault();

                poll.Votes[chosenOption] += 1;
                poll.UsersWhoVoted.Add(userVoting, chosenOption);

                await message.ModifyAsync(x => x.Embed = GeneratePollEmbed(poll));
            }
        }

        #endregion

        #region Help text

        [Command("poll")]
        [Alias("enquete")]
        [Summary("Create a quick poll")]
        [MinPermissions(AccessLevel.User)]
        public async Task QuickPoll()
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can create polls using the poll command:"
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter 1: The subject";
                x.Value = "Provide a subject to vote on surrounded by \" \"";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Parameter 2 - 11: Up to 10 poll options";
                x.Value = "Provide your poll options surrounded by \" \" for each option";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Optional Parameter 1: Show usernames";
                x.Value = "This optional parameter can be put before the question parameter and will determine if usernames are shown in the vote.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Optional Parameter 2: Duration";
                x.Value = "This optional parameter can be put before the question parameter and will determine the duration of the poll.";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "How to vote?";
                x.Value = "A user can vote by clicking the reaction icons below the poll (only 1 per user)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example";
                x.Value = $"~poll \"Is W1klas a pussy?\" \"Yes\" \"Absolutely\" \"For sure\" \"Duh...\"{Environment.NewLine}(Default poll duration 1m, does not show usernames)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 2";
                x.Value = $"~poll true \"Is W1klas a pussy?\" \"Yes\" \"Absolutely\" \"For sure\" \"Duh...\"{Environment.NewLine}(Default poll duration, shows usernames)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 3";
                x.Value = $"~poll 2m \"Is W1klas a pussy?\" \"Yes\" \"Absolutely\" \"For sure\" \"Duh...\"{Environment.NewLine}(Duration of 2 minutes, does not show usernames)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 4";
                x.Value = $"~poll true 30s \"Is W1klas a pussy?\" \"Yes\" \"Absolutely\" \"For sure\" \"Duh...\"{Environment.NewLine}(Duration of 30 seconds, shows usernames)";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Commands

        [Command("poll")]
        [Alias("enquete")]
        [Summary("Create a quick poll")]
        [MinPermissions(AccessLevel.User)]
        public async Task QuickPoll([Remainder] string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook) { 
                var reg = new Regex("\".*?\"");
                var matches = reg.Matches(input);

                if (matches.Count < 3)
                {
                    await ReplyAsync("Not enough options");
                    await QuickPoll();
                    return;
                }

                var poll = new PollModel();

                // Get poll settings
                var index = input.IndexOf('"');
                if (index > 0) {
                    var options = input.Substring(0, index).Trim();
                    var split = options.Split(' ');

                    if (split.Length > 1)
                    {
                        var nDuration = CommonHelper.GetTimeInSeconds(split[1]);
                        if (nDuration > 0)
                            poll.Duration = nDuration;
                    }

                    switch (split[0].ToLowerInvariant())
                    {
                        case "1":
                        case "true":
                        case "yes":
                            poll.ShowWhoVoted = true;
                            break;
                        default:
                            var nDuration = CommonHelper.GetTimeInSeconds(split[0]);
                            if (nDuration > 0)
                                poll.Duration = nDuration;
                            break;
                    }                    
                }

                var count = 1;
                foreach (Match item in matches)
                {
                    if (string.IsNullOrWhiteSpace(poll.Question)) { 
                        poll.Question = item.Value;
                        continue;
                    }

                    if (count == 20) // Max questions
                        break;

                    poll.Options.Add(count, item.Value);
                    poll.Votes.Add(count, 0);
                    count++;
                }

                var message = await ReplyAsync("", false, GeneratePollStart(poll));

                for (var j = 1; j <= poll.Options.Count; j++)
                {
                    await message.AddReactionAsync(IconHelper.GetEmote(_reactions[j]));
                    await Task.Delay(1250);
                }

                await message.ModifyAsync(x => x.Embed = GeneratePollEmbed(poll));

                if (_polls.Count <= 0)
                    _client.ReactionAdded += OnReactionAdded;

                _polls.Add(message.Id, poll);

                Action endPoll = async () => await EndPoll(message.Id);
                endPoll.DelayFor(TimeSpan.FromSeconds(poll.Duration));
            }
        }

        #endregion

        #region Private

        private Embed GeneratePollStart(PollModel poll)
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Mention} created a quick poll. Duration: {CommonHelper.ToReadableString(TimeSpan.FromSeconds(poll.Duration))}{Environment.NewLine}You can vote by clicking on the the numbers below!"
            };

            builder.AddField(x =>
            {
                x.Name = "The question";
                x.Value = poll.Question;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "The options are currently being generated";
                x.Value = "Because discord doesn't allow reaction add spam, I need to build it up slowly...";
                x.IsInline = false;
            });

            return builder.Build();
        }

        private Embed GeneratePollEmbed(PollModel poll)
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Mention} created a quick poll. Duration: {CommonHelper.ToReadableString(TimeSpan.FromSeconds(poll.Duration))}{Environment.NewLine}You can vote by clicking on the the numbers below!"
            };

            builder.AddField(x =>
            {
                x.Name = "The question";
                x.Value = poll.Question;
                x.IsInline = false;
            });

            for (var i = 1; i <= poll.Options.Count; i++)
            {
                var text = poll.Options[i];
                
                if (poll.Votes[i] > 0)
                {
                    text += $"{Environment.NewLine}";
                    text += $"Current votes: {poll.Votes[i]}";
                    if (poll.ShowWhoVoted)
                    {                        
                        text += " (";
                        var usrs = poll.UsersWhoVoted.Where(x => x.Value == i).Distinct().ToList();
                        foreach (var userWhoVoted in usrs)
                            text += $"{userWhoVoted.Key.Username}, ";

                        text = text.Substring(0, text.Length - 2);
                        text += ")";
                    }
                }
                builder.AddField(x =>
                {
                    x.Name = $"Option {i}:";
                    x.Value = text;
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private Embed GeneratePollResultEmbed(PollModel poll)
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Mention}'s quick poll has ended!"
            };

            var mostVotes = poll.Votes.Where(x => x.Value == poll.Votes.Values.Max());

            if (!mostVotes.Any())
            {
                builder.AddField(x =>
                {
                    x.Name = "Error";
                    x.Value = "Nobody wins, blame the monkey";
                    x.IsInline = false;
                });
                return builder.Build();
            }
            
            if (mostVotes.Count() == 1)
            {
                var option = poll.Options[mostVotes.First().Key];

                builder.AddField(x =>
                {
                    x.Name = "The question";
                    x.Value = poll.Question;
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = $"Option {option} won with {poll.Votes.Values.Max()} votes!:";
                    x.Value = option;
                    x.IsInline = false;
                });

                return builder.Build();
            }

            builder.AddField(x =>
            {
                x.Name = "The question resulted in a tie";
                x.Value = poll.Question;
                x.IsInline = false;
            });

            foreach (var vote in mostVotes)
            {
                var option = poll.Options[vote.Key];

                builder.AddField(x =>
                {
                    x.Name = $"Option {option} tied with {poll.Votes.Values.Max()} votes!:";
                    x.Value = option;
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private async Task EndPoll(ulong msgId)
        {
            var success = _polls.TryGetValue(msgId, out PollModel poll);
            if (!success)
                return;

            if (_polls.Count == 1)
                _client.ReactionAdded -= OnReactionAdded;

            _polls.Remove(msgId);
            await ReplyAsync("", false, GeneratePollResultEmbed(poll));
        }

        #endregion
    }
}
