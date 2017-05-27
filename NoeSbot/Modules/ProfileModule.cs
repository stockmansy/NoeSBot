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
                if (Enum.IsDefined(typeof(ProfileEnum), fType))
                {
                    switch ((ProfileEnum)Enum.Parse(typeof(ProfileEnum), fType))
                    {
                        case ProfileEnum.Age:
                            if (int.TryParse(input, out int age)) 
                                await _profileService.AddProfileItem((long)Context.Guild.Id, (long)user.Id, (int)ProfileEnum.Age, age.ToString());
                            else { 
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
                    }

                    await ReplyAsync("Successfully updated your profile");
                } else
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
                            await _profileService.RemoveProfileItem((long)user.Guild.Id, (long)user.Id, (int)ProfileEnum.Age);
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
            var columnOneX = 150;
            var columnOneRowOneY = 15;
            var columnOneRowTwoY = 30;
            var columnOneRowThreeY = 55;
            var columnOneRowFourY = 70;

            var columnTwoX = 300;
            var columnTwoRowOneY = 15;
            var columnTwoRowTwoY = 30;
            var columnTwoRowThreeY = 55;
            var columnTwoRowFourY = 70;

            var profile = await _profileService.RetrieveProfileAsync((long)user.Guild.Id, (long)user.Id);

            string imageFilePath = @"Images\Profile\profile.jpg";
            using (var bitmap = new Bitmap(System.Drawing.Image.FromFile(imageFilePath)))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);

                    var username = !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;

                    var fontTitle = new Font("Arial", 12);
                    var fontValue = new Font("Arial", 10);
                    var brush = new SolidBrush(System.Drawing.Color.MintCream);

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
                    graphics.DrawString($"{username}", fontValue, brush, columnOneX, columnOneRowTwoY);

                    graphics.DrawString("Joined", fontTitle, brush, columnOneX, columnOneRowThreeY);
                    graphics.DrawString($"{user.JoinedAt?.ToString("dd/MM/yyyy HH:mm") ?? "/"}", fontValue, brush, columnOneX, columnOneRowFourY);

                    if (profile != null && profile.Items.Any())
                    {
                        var age = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Age);
                        var birthdate = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Birthdate);
                        var location = profile.Items.SingleOrDefault(x => x.ProfileItemTypeId == (int)ProfileEnum.Location);

                        if ((age != null || birthdate != null) && location != null)
                        {
                            if (birthdate != null) { 
                                graphics.DrawString("Birthdate", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                                graphics.DrawString(birthdate.Value, fontValue, brush, columnTwoX, columnTwoRowTwoY);
                            } else
                            {
                                graphics.DrawString("Age", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                                graphics.DrawString(age.Value, fontValue, brush, columnTwoX, columnTwoRowTwoY);
                            }

                            graphics.DrawString("Location", fontTitle, brush, columnTwoX, columnTwoRowThreeY);
                            graphics.DrawString(location.Value, fontValue, brush, columnTwoX, columnTwoRowFourY);
                        } else if (age != null || birthdate != null)
                        {
                            if (birthdate != null)
                            {
                                graphics.DrawString("Birthdate", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                                graphics.DrawString(birthdate.Value, fontValue, brush, columnTwoX, columnTwoRowTwoY);
                            }
                            else
                            {
                                graphics.DrawString("Age", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                                graphics.DrawString(age.Value, fontValue, brush, columnTwoX, columnTwoRowTwoY);
                            }
                        } else if (location != null)
                        {
                            graphics.DrawString("Location", fontTitle, brush, columnTwoX, columnTwoRowOneY);
                            graphics.DrawString(location.Value, fontValue, brush, columnTwoX, columnTwoRowTwoY);
                        }
                    }

                    if (user.AvatarId != null)
                    {
                        var avatarUrl = user.GetAvatarUrl(Discord.ImageFormat.Jpeg);
                        await LoadImage(new Uri(avatarUrl), tmpAvatarUrl);

                        if (File.Exists(tmpAvatarUrl)) {
                            using (var img = System.Drawing.Image.FromFile(tmpAvatarUrl))
                            {
                                graphics.DrawImage(img, 10, 10, 80, 80);
                            }

                            File.Delete(tmpAvatarUrl);
                        } else if (File.Exists(defaultAvatarUrl))
                        {
                            using (var img = System.Drawing.Image.FromFile(defaultAvatarUrl))
                            {
                                graphics.DrawImage(img, 10, 10, 80, 80);
                            }
                        }
                    }
                    else if (File.Exists(defaultAvatarUrl))
                    {
                        using (var img = System.Drawing.Image.FromFile(defaultAvatarUrl))
                        {
                            graphics.DrawImage(img, 10, 10, 80, 80);
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
