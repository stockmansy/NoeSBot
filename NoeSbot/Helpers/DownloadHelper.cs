using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using NoeSbot.Converters;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Helpers
{
    internal static class DownloadHelper
    {
        private static readonly string _tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        private static readonly string _youtubeRegex = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";

        public static async Task<string> Download(string url, string name)
        {
            if (IfValidUrl(url))
                return await DownloadFile(url, name);
            else
                throw new Exception("Invalid Url!");
        }

        public static async Task<List<string>> GetItems(string url, bool ignorePlaylist = false)
        {
            if (IfValidUrl(url))
                return await GetNumberOfItems(url, ignorePlaylist: ignorePlaylist);
            else
                throw new Exception("Invalid Url!");
        }

        public static async Task<AudioInfo> GetInfo(string url)
        {
            if (IfValidUrl(url))
                return await GetInfoOfUrl(url);
            else
                throw new Exception("Invalid Url!");
        }

        public static bool IsValidUrl(string input)
        {
            return IfValidUrl(input);
        }

        #region Private

        private static bool IfValidUrl(string url)
        {
            if (Regex.Matches(url, _youtubeRegex).Count > 0)
                return true;
            else
                return false;
        }

        private static async Task<AudioInfo> GetInfoOfUrl(string url)
        {
            var tcs = new TaskCompletionSource<AudioInfo>();

            new Thread(() =>
            {
                var info = new AudioInfo()
                {
                    Url = url,
                    Title = "Not found",
                    Description = "Not found",
                    Duration = "0m"
                };

                try
                {
                    // Get info
                    // -s => simulate (do not download)
                    // -e => get title
                    // other ones are pretty clear ;)
                    var youtubedlGetTitle = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s -e --get-duration --get-description --no-playlist {url}",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    };

                    var youtubedl = Process.Start(youtubedlGetTitle);

                    // In case of debug:
                    //while (!youtubedl.StandardOutput.EndOfStream)
                    //{
                    //    string line = youtubedl.StandardOutput.ReadLine();
                    //    Console.WriteLine(line);
                    //}

                    youtubedl.WaitForExit();


                    // Get the actual info
                    string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    if (lines.Length >= 3)
                    {
                        info.Title = lines[0];

                        var builder = new StringBuilder();

                        var description = lines[2];
                        var duration = lines[1];

                        builder.AppendLine(description);
                        for (var i = 2; i < lines.Length; i++)
                        {
                            if (lines[i].IndexOf(':') > -1)
                                duration = lines[i];
                            builder.AppendLine(lines[i]);
                        }

                        info.Description = builder.ToString().Replace(duration, "");
                        info.Duration = duration;
                    }

                    tcs.SetResult(info);
                }
                catch
                {
                    tcs.SetResult(info);
                }
            }).Start();

            AudioInfo result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl failed to retrieve the info!");

            return result;
        }

        private static async Task<List<string>> GetNumberOfItems(string url, int max = 10, bool ignorePlaylist = false)
        {
            var tcs = new TaskCompletionSource<List<string>>();

            new Thread(() =>
            {
                var items = new List<string>();
                var playlistargs = ignorePlaylist ? "--no-playlist" : $"--playlist-start 1 --playlist-end {max}";

                try
                {
                    // Get info
                    // -s => simulate (do not download)
                    // -e => get title
                    // other ones are pretty clear ;)
                    var youtubedlGetTitle = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s -i {playlistargs} --get-id {url}",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    };

                    var youtubedl = Process.Start(youtubedlGetTitle);

                    while (!youtubedl.StandardOutput.EndOfStream)
                    {
                        string line = youtubedl.StandardOutput.ReadLine();
                        items.Add($"https://www.youtube.com/watch?v={line}");
                        Console.WriteLine(line);
                    }

                    youtubedl.WaitForExit();

                    tcs.SetResult(items);
                }
                catch
                {
                    tcs.SetResult(items);
                }
            }).Start();

            List<string> result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl failed to retrieve the info!");

            return result;
        }

        private static async Task<string> DownloadFile(string url, string name)
        {
            var tcs = new TaskCompletionSource<string>();

            new Thread(() =>
            {
                var file = Path.Combine(_tempFolder, $"{name}.mp3");
                if (File.Exists(file))
                    File.Delete(file);

                var youtubedlDownload = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-x --audio-format mp3 -o \"{file}\" \"{url}\"",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                try
                {
                    var youtubedl = Process.Start(youtubedlDownload);

                    while (!youtubedl.StandardOutput.EndOfStream)
                    {
                        string line = youtubedl.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }

                    youtubedl.WaitForExit(1000 * 60 * 5);
                    // Safeguard
                    Thread.Sleep(1000);

                    var fullFileName = File.Exists(file) ? file : null;
                    tcs.SetResult(fullFileName);
                }
                catch
                {
                    tcs.SetResult(null);
                }
            }).Start();

            string result = await tcs.Task;
            if (result == null)
                return "";// throw new Exception("youtube-dl failed to download the file!");

            return result.Replace("\n", "").Replace(Environment.NewLine, "");
        }

        internal static async Task<Bitmap> DownloadBitmapImage(string url)
        {
            try
            {
                return await GetScaledImage(url);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<Bitmap> GetScaledImage(string url)
        {
            var file = await DownloadFile(url);
            Bitmap bitmap = null;

            using (var ms = new MemoryStream(file))
            {
                float width = 500;
                float height = 340;

                var brush = new SolidBrush(System.Drawing.Color.Transparent);
                var rawImage = System.Drawing.Image.FromStream(ms);
                float scale = Math.Min(width / rawImage.Width, height / rawImage.Height);
                var scaleWidth = (int)(rawImage.Width * scale);
                var scaleHeight = (int)(rawImage.Height * scale);
                var scaledBitmap = new Bitmap((int)width, (int)height);
                Graphics graph = Graphics.FromImage(scaledBitmap);
                graph.InterpolationMode = InterpolationMode.High;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.SmoothingMode = SmoothingMode.AntiAlias;
                graph.FillRectangle(brush, new RectangleF(0, 0, width, height));

                var stw = 0;
                var sth = 0;

                if (scaleWidth < width)
                    stw = ((int)width - scaleWidth) / 2;

                if (scaleHeight < height)
                    sth = ((int)height - scaleHeight) / 2;

                graph.DrawImage(rawImage, new Rectangle(stw, sth, scaleWidth, scaleHeight));

                bitmap =  scaledBitmap;
            }

            return bitmap;
        }

        private static async Task<byte[]> DownloadFile(string url)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                }
            }
            return null;
        }

        #endregion
    }
}
