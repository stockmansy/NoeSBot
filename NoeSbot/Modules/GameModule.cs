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
using NoeSbot.Database.Models;
using System.IO;
using NoeSbot.Resources;
using System.Text.RegularExpressions;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Game)]
    public class GameModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private Random _random = new Random();

        public GameModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #region Commands

        #region Flip Coin

        [Command(Labels.Game_FlipCoin_Command)]
        [Alias(Labels.Game_FlipCoin_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task FlipCoin()
        {
            var result = _random.Next(2);
            if (result <= 0)
                await ReplyAsync("Heads");
            else
                await ReplyAsync("Tails");
        }

        #endregion

        #region Rock Paper Scissors

        [Command(Labels.Game_RockPaperScissors_Command)]
        [Alias(Labels.Game_RockPaperScissors_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RockPaperScissors()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Game_RockPaperScissors_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Game_RockPaperScissors_Command)]
        [Alias(Labels.Game_RockPaperScissors_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RockPaperScissors(string input)
        {
            var builder = new StringBuilder();

            var botChooses = _random.Next(3);

            var playerChoice = -1;
            bool? playerWins = null;

            switch (input.ToLowerInvariant())
            {
                case "scissors":
                case "s":
                case "scissor":
                    playerChoice = 0;
                    break;
                case "rocks":
                case "r":
                case "rock":
                    playerChoice = 1;
                    break;
                case "papers":
                case "p":
                case "paper":
                    playerChoice = 2;
                    break;
            }

            switch (botChooses)
            {
                case 0:
                    builder.AppendLine("I pick => Scissors!");
                    builder.AppendLine("```");
                    builder.AppendLine("   _       ,/'");
                    builder.AppendLine("  (_).  ,/'");
                    builder.AppendLine("   _  ::");
                    builder.AppendLine("  (_)'  `\\.");
                    builder.AppendLine("           `\\.");
                    builder.AppendLine("```");
                    if (playerChoice == 0) // Scissors
                        playerWins = null;
                    if (playerChoice == 1) // Rock
                        playerWins = true;
                    if (playerChoice == 2) // Paper
                        playerWins = false;
                    break;
                case 1:
                    builder.AppendLine("I pick => Rock!");
                    builder.AppendLine("```");
                    builder.AppendLine("  ____, O");
                    builder.AppendLine("   /   /M| ");
                    builder.AppendLine("  /|MMMMMMMM");
                    builder.AppendLine("  {| | // |}");
                    builder.AppendLine("-_}| |/ \\ |{_apx");
                    builder.AppendLine("");
                    builder.AppendLine("");
                    builder.AppendLine("  .Q             Q .");
                    builder.AppendLine(" /`M\\,          /W\\_\\");
                    builder.AppendLine("'| H           (`&}=|\"");
                    builder.AppendLine(" | |\\         ,'`}\\ |");
                    builder.AppendLine("/'\\l '       (_ / //'\\");
                    builder.AppendLine("```");
                    if (playerChoice == 0) // Scissors
                        playerWins = false;
                    if (playerChoice == 1) // Rock
                        playerWins = null;
                    if (playerChoice == 2) // Paper
                        playerWins = true;
                    break;
                case 2:
                    builder.AppendLine("I pick => Paper!");
                    builder.AppendLine("```");
                    builder.AppendLine("             _.____");
                    builder.AppendLine("          _.'      '_.");
                    builder.AppendLine("      _.-'       _.'  \\");
                    builder.AppendLine("    _'___     _.'      | ");
                    builder.AppendLine("  .'     '-.- '         L");
                    builder.AppendLine(" /          \\          | ");
                    builder.AppendLine("|     __     |         L");
                    builder.AppendLine("|   .x$$x.   L         | ");
                    builder.AppendLine("|   |%$$$|   |         | ");
                    builder.AppendLine("|   |%%$$|   L         | ");
                    builder.AppendLine("|   '%%%?'   |         .\\");
                    builder.AppendLine(" \\          /|      .- ");
                    builder.AppendLine("  '.__  __.' |   .- ");
                    builder.AppendLine("      ''      \\.-");
                    builder.AppendLine("```");
                    if (playerChoice == 0) // Scissors
                        playerWins = false;
                    if (playerChoice == 1) // Rock
                        playerWins = true;
                    if (playerChoice == 2) // Paper
                        playerWins = null;
                    break;
            }

            if (playerWins == null && playerChoice >= 0)
            {
                builder.AppendLine("We are:");
                builder.AppendLine("```");
                builder.AppendLine(" _   _          _ ");
                builder.AppendLine("| | (_)        | | ");
                builder.AppendLine("| |_ _  ___  __| | ");
                builder.AppendLine("| __| |/ _ \\/ _` | ");
                builder.AppendLine("| |_| |  __/ (_| |");
                builder.AppendLine(" \\__|_|\\___|\\__,_| ");
                builder.AppendLine("```");
            }
            else if (playerWins == true)
            {
                builder.AppendLine("I suffered:");
                builder.AppendLine("```");
                builder.AppendLine("     _       __           _   ");
                builder.AppendLine("    | |     / _|         | |  ");
                builder.AppendLine("  __| | ___| |_ ___  __ _| |_ ");
                builder.AppendLine(" / _` |/ _ \\  _/ _ \\/ _` | __| ");
                builder.AppendLine("| (_| |  __/ ||  __/ (_| | |_ ");
                builder.AppendLine(" \\__,_|\\___|_| \\___|\\__,_|\\__| ");
                builder.AppendLine("```");
            }
            else if (playerWins == false)
            {
                builder.AppendLine("I attained:");
                builder.AppendLine("```");
                builder.AppendLine("       _      _                   ");
                builder.AppendLine("      (_)    | | ");
                builder.AppendLine("__   ___  ___| |_ ___  _ __ _   _ ");
                builder.AppendLine("\\ \\ / / |/ __| __/ _ \\| '__| | | |");
                builder.AppendLine(" \\ V /| | (__| || (_) | |  | |_| |");
                builder.AppendLine("  \\_/ |_|\\___|\\__\\___/|_|   \\__, |");
                builder.AppendLine("                             __/ |");
                builder.AppendLine("                            |___/ ");
                builder.AppendLine("```");
            }

            if (playerChoice < 0)
                await ReplyAsync("Please choose something valid (rock,rocks,r; paper,papers,p; scissor, scissors,s)");
            else
                await ReplyAsync(builder.ToString());
        }

        #endregion

        #region 8 Ball

        [Command(Labels.Game_8Ball_Command)]
        [Alias(Labels.Game_8Ball_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task MagicBall()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Game_8Ball_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Game_8Ball_Command)]
        [Alias(Labels.Game_8Ball_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task MagicBall([Remainder] string input)
        {
            var result = "";

            var options = new List<EightBallModel>() {
                new EightBallModel
                {
                    Text = "Yes",
                    ImageName = "8BallYes.jpg"
                },
                new EightBallModel
                {
                    Text = "No",
                    ImageName = "8ballNo.jpg"
                },
                new EightBallModel
                {
                    Text = "Maybe",
                    ImageName = "8BallMaybe.jpg"
                },
                new EightBallModel
                {
                    Text = "Ask Mango",
                    ImageName = "8ballaskmango.jpg"
                },
                new EightBallModel
                {
                    Text = "How the fuck should I know?",
                    ImageName = "8Ballhtfsik.jpg"
                },
                 new EightBallModel
                {
                    Text = "I AM Err0r",
                    ImageName = "8Ballerror.jpg"
                }
            };

            var rndNr = _random.Next(options.Count);
            var randomElement = options.ElementAt(rndNr);
            result = randomElement?.Text;
            var imageName = randomElement?.ImageName;

            var fileExists = File.Exists(@"Images\MagicBall\" + imageName);

            //Check if the image exsits!!! then send it
            if (fileExists)
                await Context.Channel.SendFileAsync(@"Images\MagicBall\" + imageName, result);
            else
                await ReplyAsync(result);
        }

        #endregion

        #region Choose

        [Command(Labels.Game_Choose_Command)]
        [Alias(Labels.Game_Choose_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Choose()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Game_Choose_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Game_Choose_Command)]
        [Alias(Labels.Game_Choose_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Choose([Remainder] string input)
        {
            var inputs = input.Split(' ');
            if (inputs.Length < 2)
            {
                await ReplyAsync("Please provide at least 2 options");
                return;
            }

            var result = _random.Next(inputs.Length);

            await ReplyAsync(inputs[result]);
        }

        #endregion

        #region Blame

        [Command(Labels.Game_Blame_Command)]
        [Alias(Labels.Game_Blame_Alias_1, Labels.Game_Blame_Alias_2)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RandomlyBlame()
        {
            await Context.Message.DeleteAsync();
            var allUsers = await Context.Guild.GetUsersAsync();
            var onlineUsers = allUsers.Where(x => x.Status == UserStatus.Online && !x.IsBot && !x.IsWebhook).ToList();
            var rndUser = onlineUsers[_random.Next(onlineUsers.Count)];
            var name = !string.IsNullOrWhiteSpace(rndUser.Nickname) ? rndUser.Nickname : rndUser.Username;
            await ReplyAsync($"I blame {name}", true);
        }

        [Command(Labels.Game_Blame_Command)]
        [Alias(Labels.Game_Blame_Alias_1, Labels.Game_Blame_Alias_2)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RandomlyBlame([Summary("The user to be blamed")] SocketGuildUser user)
        {
            await Context.Message.DeleteAsync();
            var name = !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;
            await ReplyAsync($"I blame {name}", true);
        }

        #endregion

        #region Roll

        [Command(Labels.Game_Roll_Command)]
        [Alias(Labels.Game_Roll_Alias_1, Labels.Game_Roll_Alias_2, Labels.Game_Roll_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Roll()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Game_Roll_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Game_Roll_Command)]
        [Alias(Labels.Game_Roll_Alias_1, Labels.Game_Roll_Alias_2, Labels.Game_Roll_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Roll([Remainder]string input)
        {
            try
            {
                if (Regex.IsMatch(input, @"^d([0-9]).*$"))
                {
                    await Dnd(input.Replace("d", ""));
                    return;
                }

                var result = DiceHelper.GetDiceResult(input);
                await ReplyAsync($"{ IconHelper.Dice} {result}");
            }
            catch
            {
                await Context.User.SendMessageAsync("Invalid input, you have to specify a solid number (e.g. 6)");
            }
        }

        #endregion

        #region Dnd

        [Command(Labels.Game_Dnd_Command)]
        [Alias(Labels.Game_Dnd_Alias_1, Labels.Game_Dnd_Alias_2, Labels.Game_Dnd_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Dnd()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Game_Roll_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Game_Dnd_Command)]
        [Alias(Labels.Game_Dnd_Alias_1, Labels.Game_Dnd_Alias_2, Labels.Game_Dnd_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Dnd([Remainder]string input)
        {
            try
            {
                var result = DiceHelper.GetDndResult(input);
                await ReplyAsync($"{IconHelper.Dice} {result}");
            }
            catch
            {
                await Context.User.SendMessageAsync("Invalid input, you have to specify a solid number or expression (e.g. 6, 20 + 2, 20 x 5)");
            }
        }

        #endregion

        #endregion
    }
}
