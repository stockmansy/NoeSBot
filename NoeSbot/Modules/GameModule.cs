﻿using Discord;
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

        #region Help text

        [Command("rockpaperscissors")]
        [Alias("rps")]
        [Summary("Play rock paper scissors")]
        public async Task RockPaperScissors()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: input");
            builder.AppendLine("Provide a rock paper scissors input");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        #endregion

        #region Commands

        [Command("flipcoin")]
        [Alias("flip")]
        [Summary("Flip a coin")]
        [MinPermissions(AccessLevel.User)]
        public async Task FlipCoin()
        {
            var result = _random.Next(2);
            if (result <= 0)
                await ReplyAsync("Heads");
            else
                await ReplyAsync("Tails");
        }

        [Command("rockpaperscissors")]
        [Alias("rps")]
        [Summary("Play rock paper scissors")]
        [MinPermissions(AccessLevel.User)]
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

        [Command("8ball")]
        [Alias("8b")]
        [Summary("Play rock paper scissors")]
        [MinPermissions(AccessLevel.User)]
        public async Task MagicBall([Remainder] string input)
        {
            var result = "";

            var options = new List<EightBallModel>() {
                new EightBallModel
                {
                    Text = "Wiklas is a pussy",
                    ImageName = "8Ballwiklaspussy.jpg"
                },
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
    }
}