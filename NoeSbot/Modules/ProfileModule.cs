using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using System.Threading.Tasks;
using Discord;
using NoeSbot.Helpers;
using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net.Http;
using NoeSbot.Database.Services;
using NoeSbot.Resources;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Profile)]
    public class ProfileModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private const string _tempDir = @"Temp";
        private IProfileService _profileService;

        #region Constructor

        public ProfileModule(DiscordSocketClient client, IMemoryCache memoryCache, IProfileService profileService)
        {
            _client = client;
            _cache = memoryCache;
            _profileService = profileService;
        }

        #endregion

        #region Handlers



        #endregion

        #region Commands

        #region Profile

        [Command(Labels.Profile_Profile_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Profile()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Profile_Profile_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Profile_Profile_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Profile(SocketGuildUser user)
        {
            var getProfileImage = await GetProfileImageAsync(user);
            await Context.Channel.SendFileAsync(getProfileImage);
            DeleteTmpImage(user.Id);
        }

        #endregion

        #region Profile Item

        [Command(Labels.Profile_ProfileItem_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task ProfileItem()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Profile_ProfileItem_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Profile_ProfileItem_Command)]
        [Alias(Labels.Profile_ProfileItem_Alias_1, Labels.Profile_ProfileItem_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task ProfileItem(string type, [Remainder]string input)
        {
            var user = Context.User as SocketGuildUser;
            var fType = CommonHelper.FirstLetterToUpper(type);

            if (input.Length > 255)
                input = input.Substring(0, 255);

            switch (fType)
            {
                case nameof(ProfileEnum.Age):
                case "Old":
                    if (int.TryParse(input, out int age))
                        await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Age, age.ToString());
                    else
                    {
                        await ReplyAsync("The age you provided is incorrect (Try something like 28)");
                        return;
                    }
                    break;
                case nameof(ProfileEnum.Birthdate):
                    if (DateTime.TryParse(input, out DateTime birthDate))
                        await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Birthdate, birthDate.ToString("dd-MM-yyyy"));
                    else
                    {
                        await ReplyAsync("The birthdate you provided is incorrect (Try something like 1988-11-30)");
                        return;
                    }
                    break;
                case nameof(ProfileEnum.Location):
                    await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Location, input);
                    break;
                case nameof(ProfileEnum.Game):
                    await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Game, input);
                    break;
                case nameof(ProfileEnum.Streaming):
                case "Stream":
                case "Twitch":
                case "Youtube":
                    await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Streaming, input);
                    break;
                case nameof(ProfileEnum.Summary):
                case "Description":
                    var lines = CommonHelper.SplitToLines(input, 50);
                    var res = "";
                    foreach (var line in lines)
                        res += $"{line}<br>";
                    await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Summary, res);
                    break;
                default:
                    await ReplyAsync("Please provide a correct type (Age or Birthdate, Location, ...)");
                    break;
            }

            await ReplyAsync("Successfully updated your profile");
        }

        #endregion

        #region Remove Profile Item

        [Command(Labels.Profile_RemoveProfileItem_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveProfileItem()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Profile_RemoveProfileItem_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Profile_RemoveProfileItem_Command)]
        [Alias(Labels.Profile_RemoveProfileItem_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveProfileItem(string type)
        {
            var user = Context.User as SocketGuildUser;
            var fType = CommonHelper.FirstLetterToUpper(type);
            if (Enum.IsDefined(typeof(ProfileEnum), fType))
            {
                switch ((ProfileEnum)Enum.Parse(typeof(ProfileEnum), fType))
                {
                    case ProfileEnum.Age:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Age);
                        break;
                    case ProfileEnum.Birthdate:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Birthdate);
                        break;
                    case ProfileEnum.Location:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Location);
                        break;
                    case ProfileEnum.Game:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Game);
                        break;
                    case ProfileEnum.Streaming:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Streaming);
                        break;
                    case ProfileEnum.Summary:
                        await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Summary);
                        break;
                }

                await ReplyAsync("Successfully removed the item from your profile");
            }
            else
            {
                await ReplyAsync("Please provide a correct type (Age, Location, ...)");
            }
        }

        #endregion

        #endregion

        #region Private

        private async Task<string> GetProfileImageAsync(SocketGuildUser user)
        {
            //TODO: Clean this up

            var tmpUrl = $"{_tempDir}\\Profile{user.Id}.jpg";
            var defaultAvatarUrl = @"Images\Profile\default.png";
            var tmpAvatarUrl = $"{_tempDir}\\Avatar{user.Id}.jpg";
            const int quality = 75;
            const int rowDiff = 20;
            const int columnDiff = 240;
            var columnOneX = 130;
            var columnOneRowOneY = 25;
            var columnOneRowTwoY = 65;
            var columnOneRowThreeY = 105;
            var columnOneRowFourY = 145;
            var columnOneRowFiveY = 185;

            var columnTwoX = columnOneX + columnDiff;
            var columnTwoRowOneY = columnOneRowOneY;
            var columnTwoRowTwoY = columnOneRowTwoY;
            var columnTwoRowThreeY = columnOneRowThreeY;

            var profile = await _profileService.RetrieveProfileAsync((long)user.Guild.Id, (long)user.Id);

            var imageFilePath = @"Images\Profile\profile_bg.jpg";
            var imageCorePath = @"Images\Profile\profile_core.png";

            var age = "n/a";
            string birthdate = null;
            var location = "n/a";
            var favGame = "n/a";
            var streaming = "n/a";
            var summary = "n/a";
            Bitmap bitmap = null;

            if (profile != null && profile.Items.Any())
            {
                var fGame = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Game);

                age = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Age)?.Value ?? "n/a";
                birthdate = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Birthdate)?.Value;
                location = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Location)?.Value ?? "n/a";
                favGame = fGame?.Value ?? "n/a";
                streaming = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Streaming)?.Value ?? "n/a";
                summary = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Summary)?.Value ?? "n/a";


                var custom = await _profileService.RetrieveProfileBackground((long)user.Guild.Id, (long)user.Id, favGame);
                if (custom != null)
                    bitmap = await DownloadHelper.DownloadBitmapImage(custom.Value);

                imageFilePath = GetDefaultBackgrounds(favGame, imageFilePath);
            }

            if (bitmap == null)
                bitmap = new Bitmap(System.Drawing.Image.FromFile(imageFilePath));

            using (bitmap)
            {
                using (var bitmapCore = new Bitmap(System.Drawing.Image.FromFile(imageCorePath)))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                        graphics.DrawImage(bitmapCore, 0, 0, bitmapCore.Width, bitmapCore.Height);

                        var username = !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;

                        var fontTitle = new Font("Arial", 12);
                        var fontValue = new Font("Arial", 10);
                        var brush = new SolidBrush(System.Drawing.Color.Black);
                        var lightbrush = new SolidBrush(System.Drawing.Color.DarkSlateGray);

                        //var p = new GraphicsPath();
                        //p.AddString(
                        //   "Username",
                        //    FontFamily.GenericSansSerif, 
                        //    (int)FontStyle.Regular,  
                        //    graphics.DpiY * 14 / 72, 
                        //    new Point(110, 15),
                        //    new StringFormat());
                        //graphics.DrawPath(Pens.Black, p);
                        //graphics.FillPath(brush, p);

                        graphics.DrawString("Username", fontTitle, brush, columnOneX, columnOneRowOneY);
                        graphics.DrawString($"{username}", fontValue, lightbrush, columnOneX, columnOneRowOneY + rowDiff);

                        graphics.DrawString("Joined", fontTitle, brush, columnOneX, columnOneRowTwoY);
                        graphics.DrawString($"{user.JoinedAt?.ToString("dd/MM/yyyy HH:mm") ?? "/"}", fontValue, lightbrush, columnOneX, columnOneRowTwoY + rowDiff);

                        if (birthdate != null)
                        {
                            graphics.DrawString("Birthdate", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                            graphics.DrawString(birthdate, fontValue, lightbrush, columnTwoX, columnTwoRowOneY + rowDiff);
                        }
                        else
                        {
                            graphics.DrawString("Age", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                            graphics.DrawString(age, fontValue, lightbrush, columnTwoX, columnTwoRowOneY + rowDiff);
                        }

                        graphics.DrawString("Location", fontTitle, brush, columnTwoX, columnTwoRowTwoY);
                        graphics.DrawString(location, fontValue, lightbrush, columnTwoX, columnTwoRowTwoY + rowDiff);

                        graphics.DrawString("Favorite game", fontTitle, brush, columnOneX, columnOneRowThreeY);
                        graphics.DrawString(favGame, fontValue, lightbrush, columnOneX, columnOneRowThreeY + rowDiff);

                        graphics.DrawString("Stream (Twitch/Youtube/etc)", fontTitle, brush, columnOneX, columnOneRowFourY);
                        graphics.DrawString(streaming, fontValue, lightbrush, columnOneX, columnOneRowFourY + rowDiff);

                        graphics.DrawString("Summary (max 255)", fontTitle, brush, columnOneX, columnOneRowFiveY);
                        graphics.DrawString(summary.Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine), fontValue, lightbrush, columnOneX, columnOneRowFiveY + rowDiff);

                        if (user.AvatarId != null)
                        {
                            var avatarUrl = user.GetAvatarUrl(Discord.ImageFormat.Jpeg);
                            await LoadImage(new Uri(avatarUrl), tmpAvatarUrl);

                            if (File.Exists(tmpAvatarUrl))
                            {
                                using (var img = System.Drawing.Image.FromFile(tmpAvatarUrl))
                                {
                                    graphics.DrawImage(img, 19, 21, 98, 98);
                                }

                                File.Delete(tmpAvatarUrl);
                            }
                            else if (File.Exists(defaultAvatarUrl))
                            {
                                using (var img = System.Drawing.Image.FromFile(defaultAvatarUrl))
                                {
                                    graphics.DrawImage(img, 19, 21, 98, 98);
                                }
                            }
                        }
                        else if (File.Exists(defaultAvatarUrl))
                        {
                            using (var img = System.Drawing.Image.FromFile(defaultAvatarUrl))
                            {
                                graphics.DrawImage(img, 19, 21, 98, 98);
                            }
                        }

                        using (var output = File.Open(tmpUrl, FileMode.Create))
                        {
                            var qualityParamId = Encoder.Quality;
                            var encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                            var codec = ImageCodecInfo.GetImageDecoders()
                                .FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                            bitmap.Save(output, codec, encoderParameters);
                        }
                    }
                }
            }

            return tmpUrl;
        }

        private string GetDefaultBackgrounds(string favGame, string imageFilePath)
        {
            var input = favGame.ToLowerInvariant().Replace(" ", "");
            var keys = new string[] { "dota", "dota2", "defenseoftheancients", "defenseoftheancients2", "darksouls", "darksouls2", "darksouls3", "bloodborne", "demonsouls", "gta", "gta5", "grandtheftauto", "grandtheftauto5", "pubg",
                                                  "battlegrounds", "playerunknownbattlegrounds", "tf2", "tf", "teamfortress", "teamfortress2", "witcher", "witcher3", "gwent", "gwentsimulator", "gwentsimulator3" };
            var sKeyResult = keys.FirstOrDefault(s => input.Contains(s));

            switch (sKeyResult)
            {
                case "dota":
                case "dota2":
                case "defenseoftheancients":
                case "defenseoftheancients2":
                    imageFilePath = @"Images\Profile\profile_bg_dota.jpg";
                    break;
                case "darksouls":
                case "darksouls2":
                case "darksouls3":
                case "bloodborne":
                case "demonsouls":
                    imageFilePath = @"Images\Profile\profile_bg_darksouls.jpg";
                    break;
                case "gta":
                case "gta5":
                case "grandtheftauto":
                case "grandtheftauto5":
                    imageFilePath = @"Images\Profile\profile_bg_gta.jpg";
                    break;
                case "pubg":
                case "battlegrounds":
                case "playerunknownbattlegrounds":
                    imageFilePath = @"Images\Profile\profile_bg_pubg.jpg";
                    break;
                case "tf2":
                case "tf":
                case "teamfortress":
                case "teamfortress2":
                    imageFilePath = @"Images\Profile\profile_bg_tf2.jpg";
                    break;
                case "witcher":
                case "witcher3":
                case "gwent":
                case "gwentsimulator":
                case "gwentsimulator3":
                    imageFilePath = @"Images\Profile\profile_bg_witcher.jpg";
                    break;
                default:
                    if (File.Exists(@"Images\Profile\profile_bg_" + $"{favGame.ToLowerInvariant()}.jpg"))
                        imageFilePath = @"Images\Profile\profile_bg_" + $"{favGame.ToLowerInvariant()}.jpg";
                    break;
            }
            return imageFilePath;
        }

        private void DeleteTmpImage(ulong userId)
        {
            var tmpUrl = $"{_tempDir}\\Profile{userId}.jpg";
            File.Delete(tmpUrl);
        }

        private async Task LoadImage(Uri uri, string filename)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
            }
        }

        #endregion

    }
}
