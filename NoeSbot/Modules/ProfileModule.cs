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

        #region Help

        [Command("profile")]
        [Summary("Get the profile of a user")]
        [MinPermissions(AccessLevel.User)]
        public async Task Profile()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "You can get the profile of a user."
                };

                builder.AddField(x =>
                {
                    x.Name = "Parameter: The user";
                    x.Value = "Provide a user.";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example";
                    x.Value = $"{prefix}profile @MensAap";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("profileitem")]
        [Alias("addprofileitem", "addprofile")]
        [Summary("Adjust a profileItem of yourself")]
        [MinPermissions(AccessLevel.User)]
        public async Task ProfileItem()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "Change a profile item of yourself."
                };

                builder.AddField(x =>
                {
                    x.Name = "Parameter: The item to change";
                    x.Value = "Provide an item type like Age or Location.";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Parameter 2: The item value";
                    x.Value = "Provide an item value.";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example";
                    x.Value = $"{prefix}profileitem Birthdate 1988-11-30";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example 2";
                    x.Value = $"{prefix}profileitem Age 28";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example 3";
                    x.Value = $"{prefix}profileitem Location Belgium";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("removeprofileitem")]
        [Alias("deleteprofileitem")]
        [Summary("Remove a profileItem of yourself")]
        [MinPermissions(AccessLevel.User)]
        public async Task RemoveProfileItem()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "Remove a profile item of yourself."
                };

                builder.AddField(x =>
                {
                    x.Name = "Parameter: The item to remove";
                    x.Value = "Provide an item type like Age or Location.";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example";
                    x.Value = $"{prefix}removeprofileitem Age";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Commands

        [Command("profile")]
        [Summary("Get the profile of a user")]
        [MinPermissions(AccessLevel.User)]
        public async Task Profile(SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var getProfileImage = await GetProfileImageAsync(user);
                await Context.Channel.SendFileAsync(getProfileImage);
                DeleteTmpImage(user.Id);
            }
        }

        [Command("profileitem")]
        [Alias("addprofileitem", "addprofile")]
        [Summary("Adjust a profileItem of yourself")]
        [MinPermissions(AccessLevel.User)]
        public async Task ProfileItem(string type, [Remainder]string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var fType = CommonHelper.FirstLetterToUpper(type);

                if (input.Length > 255)
                    input = input.Substring(0, 255);

                if (Enum.IsDefined(typeof(ProfileEnum), fType))
                {
                    switch ((ProfileEnum)Enum.Parse(typeof(ProfileEnum), fType))
                    {
                        case ProfileEnum.Age:
                            if (int.TryParse(input, out int age))
                                await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Age, age.ToString());
                            else
                            {
                                await ReplyAsync("The age you provided is incorrect (Try something like 28)");
                                return;
                            }
                            break;
                        case ProfileEnum.Birthdate:
                            if (DateTime.TryParse(input, out DateTime birthDate))
                                await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Birthdate, birthDate.ToString("dd-MM-yyyy"));
                            else
                            {
                                await ReplyAsync("The birthdate you provided is incorrect (Try something like 1988-11-30)");
                                return;
                            }
                            break;
                        case ProfileEnum.Location:
                            await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Location, input);
                            break;
                        case ProfileEnum.Game:
                            await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Game, input);
                            break;
                        case ProfileEnum.Streaming:
                            await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Streaming, input);
                            break;
                        case ProfileEnum.Summary:
                            var lines = CommonHelper.SplitToLines(input, 50);
                            var res = "";
                            foreach (var line in lines)
                                res += $"{line}<br>";
                            await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Summary, res);
                            break;
                    }

                    await ReplyAsync("Successfully updated your profile");
                }
                else
                {
                    await ReplyAsync("Please provide a correct type (Age or Birthdate, Location, ...)");
                }
            }
        }

        [Command("removeprofileitem")]
        [Alias("deleteprofileitem")]
        [Summary("Remove a profileItem of yourself")]
        [MinPermissions(AccessLevel.User)]
        public async Task RemoveProfileItem(string type)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }

        #endregion

        #region Private

        private async Task<string> GetProfileImageAsync(SocketGuildUser user)
        {
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

            if (profile != null && profile.Items.Any())
            {
                age = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Age)?.Value ?? "n/a";
                birthdate = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Birthdate)?.Value;
                location = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Location)?.Value ?? "n/a";
                favGame = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Game)?.Value ?? "n/a";
                streaming = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Streaming)?.Value ?? "n/a";
                summary = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Summary)?.Value ?? "n/a";

                // Should probly do this better :')
                switch (favGame.ToLowerInvariant())
                {
                    case "dota":
                    case "dota 2":
                    case "dota2":
                    case "defense of the ancients":
                    case "defense of the ancients 2":
                        imageFilePath = @"Images\Profile\profile_bg_dota.jpg";
                        break;
                    case "darksouls":
                    case "dark souls":
                    case "dark souls 2":
                    case "bloodborne":
                    case "demon souls":
                        imageFilePath = @"Images\Profile\profile_bg_darksouls.jpg";
                        break;
                    case "gta":
                    case "gta5":
                    case "gta 5":
                    case "grand theft auto":
                    case "grand theft auto 5":
                    case "grand theft auto5":
                        imageFilePath = @"Images\Profile\profile_bg_gta.jpg";
                        break;
                    case "pubg":
                    case "battlegrounds":
                    case "playerunknown battlegrounds":
                    case "player unknown battlegrounds":
                    case "player unknown battle grounds":
                        imageFilePath = @"Images\Profile\profile_bg_pubg.jpg";
                        break;
                    case "tf2":
                    case "tf":
                    case "team fortress":
                    case "team fortress 2":
                    case "team fortress2":
                        imageFilePath = @"Images\Profile\profile_bg_tf2.jpg";
                        break;
                    case "witcher":
                    case "witcher 3":
                    case "witcher3":
                    case "gwent":
                    case "gwent simulator":
                    case "gwent simulator 3":
                        imageFilePath = @"Images\Profile\profile_bg_witcher.jpg";
                        break;
                    default:
                        if (File.Exists(@"Images\Profile\profile_bg_" + $"{favGame.ToLowerInvariant()}.jpg"))
                            imageFilePath = @"Images\Profile\profile_bg_" + $"{favGame.ToLowerInvariant()}.jpg";
                        break;
                }
            }

            using (var bitmap = new Bitmap(System.Drawing.Image.FromFile(imageFilePath)))
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

                        if (profile != null && profile.Items.Any())
                        {
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
                        }



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
