using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using System;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Resources;
using Hangfire;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Utility)]
    public class UtilityModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private Random _random = new Random();

        #region Constructor

        public UtilityModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #endregion

        #region Commands

        #region User Info

        [Command(Labels.Utility_UserInfo_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UserInfo()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Utility_UserInfo_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }


        [Command(Labels.Utility_UserInfo_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UserInfo(SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor()
            };

            builder.AddField(x =>
            {
                x.Name = $"Username of {user.Nickname}";
                x.Value = $"{user.Username}";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Join date";
                x.Value = $"The user's joined {user.JoinedAt?.ToString("dd/MM/yyyy HH:mm")}";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "The user roles";
                x.Value = $"{string.Join(Environment.NewLine, user.Roles.Where(y => y.Id != y.Guild.EveryoneRole.Id).Select(y => y.Name))}";
                x.IsInline = false;
            });

            if (user.AvatarId != null)
                builder.WithThumbnailUrl(user.GetAvatarUrl());

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Random Member

        [Command(Labels.Utility_RandomMember_Command)]
        [Alias(Labels.Utility_RandomMember_Alias_1, Labels.Utility_RandomMember_Alias_2)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RandomMember()
        {
            await Context.Message.DeleteAsync();
            var allUsers = await Context.Guild.GetUsersAsync();
            var onlineUsers = allUsers.Where(x => x.Status == UserStatus.Online && !x.IsBot && !x.IsWebhook).ToList();
            var rndUser = onlineUsers[_random.Next(onlineUsers.Count)];
            var name = !string.IsNullOrWhiteSpace(rndUser.Nickname) ? rndUser.Nickname : rndUser.Username;
            await ReplyAsync($"Picked {name} at random");
        }

        #endregion

        #region

        [Command(Labels.Utility_RemindMe_Command)]
        [Alias(Labels.Utility_RemindMe_Alias_1, Labels.Utility_RemindMe_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemindMe()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Utility_RemindMe_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Utility_RemindMe_Command)]
        [Alias(Labels.Utility_RemindMe_Alias_1, Labels.Utility_RemindMe_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemindMe([Summary("Time input")] string timeDate)
        {
            await RemindMe(timeDate, string.Empty);
        }

        [Command(Labels.Utility_RemindMe_Command)]
        [Alias(Labels.Utility_RemindMe_Alias_1, Labels.Utility_RemindMe_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemindMe([Summary("Time input")] string timeDate, [Remainder, Summary("The reminder input")] string input)
        {
            var newMsg = default(IUserMessage);

            try
            {
                var user = Context.User as SocketGuildUser;
                var fullInput = $"{timeDate} {input}";

                var dateTimeParsed = CommonHelper.GetDateTimeWithinInput(fullInput);
                if (dateTimeParsed.DateTime == default)
                {
                    var dateTimeInput = CommonHelper.GetTimeInSeconds(timeDate, 0);
                    if (dateTimeInput <= 0)
                        throw new Exception("User input seems invalid");

                    dateTimeParsed.DateTime = DateTimeOffset.Now.AddSeconds(dateTimeInput);
                }

                if (dateTimeParsed.DateTime.Ticks <= DateTime.Now.Ticks) { 
                    await ReplyAsync("Please enter a date that's in the future.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(dateTimeParsed.DateString))
                    fullInput = fullInput.Replace(dateTimeParsed.DateString, string.Empty);

                newMsg = await ReplyAsync("", false, GetReminderEmbed(user, dateTimeParsed.DateTime, fullInput));

                BackgroundJob.Schedule(() => SendReminder(user.Id, user.GetColor(), Context.Message.CreatedAt, fullInput, $"https://discordapp.com/channels/{Context.Guild.Id}/{Context.Channel.Id}/{Context.Message.Id}"), dateTimeParsed.DateTime);
            }
            catch (Exception ex)
            {
                // In case the scheduler fails (Scheduler takes a bit to long imo, so don't want to let the user wait)
                if (newMsg != default)
                    await newMsg.DeleteAsync();

                await ReplyAsync("Could not properly add the reminder for you. Make sure your request is properly formatted.\rType the 'remindme' without any arguments for help.");
                LogHelper.LogWarning($"An error occurred while trying to parse the users input: {ex}");
            }
        }

        public async Task SendReminder(ulong userId, Color userColor, DateTimeOffset origDateTime, string message, string messageUrl)
        {
            var user = _client.GetUser(userId);

            var builder = new EmbedBuilder()
            {
                Color = userColor,
            };

            if (!string.IsNullOrWhiteSpace(message))
                builder.Description = $"Reminder of:\r{message}";

            builder.AddField(x =>
            {
                x.Name = $"Reminder was set on";
                x.Value = origDateTime.ToLocalTime();
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = $"Link to the message:";
                x.Value = messageUrl;
                x.IsInline = false;
            });

            var embed = builder.Build();
            await user.SendMessageAsync("", false, embed);
        }

        #endregion

        #endregion

        #region Private

        private Embed GetReminderEmbed(SocketGuildUser user, DateTimeOffset dateTime, string message)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
            };

            if (!string.IsNullOrWhiteSpace(message))
                builder.Description = $"You will be reminded with the message:\r{message}";

            builder.AddField(x =>
            {
                x.Name = $"Reminder date and time";
                x.Value = dateTime.ToLocalTime();
                x.IsInline = false;
            });

            return builder.Build();
        }

        #endregion
    }
}
