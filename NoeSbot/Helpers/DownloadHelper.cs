using Discord;
using Discord.WebSocket;
using NoeSbot.Enums;
using NoeSbot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Helpers
{
    internal static class DownloadHelper
    {
        private static readonly string _tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        private static readonly string _youtubeRegex = @"(https ?://(www\.)?youtube\.com/.*v=\w+.*)|(https?://youtu\.be/\w+.*)|(.*src=.https?://(www\.)?youtube\.com/v/\w+.*)|(.*src=.https?://(www\.)?youtube\.com/embed/\w+.*)";
        private static Dictionary<ulong, int> _fileCount = new Dictionary<ulong, int>();

        public static Dictionary<ulong, int> FileCount { get => _fileCount; set => _fileCount = value; }

        public static async Task<string> Download(string url, string name)
        {
            if (IfValidUrl(url)) 
                return await DownloadFile(url, name);
            else
                throw new Exception("Invalid Url!");
        }

        public static async Task<List<string>> GetItems(string url)
        {
            if (IfValidUrl(url))
                return await GetNumberOfItems(url);
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

                    while (!youtubedl.StandardOutput.EndOfStream)
                    {
                        string line = youtubedl.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }

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

        private static async Task<List<string>> GetNumberOfItems(string url, int max = 10)
        {
            var tcs = new TaskCompletionSource<List<string>>();

            new Thread(() =>
            {
                var items = new List<string>();

                try
                {
                    // Get info
                    // -s => simulate (do not download)
                    // -e => get title
                    // other ones are pretty clear ;)
                    var youtubedlGetTitle = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s --playlist-start 1 --playlist-end {max} --get-id {url}",
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
                var youtubedl = Process.Start(youtubedlDownload);

                while (!youtubedl.StandardOutput.EndOfStream)
                {
                    string line = youtubedl.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }

                youtubedl.WaitForExit(1000 * 60 * 5);

                // Safeguard
                //Thread.Sleep(1000);

                var fullFileName = File.Exists(file) ? file : null;
                tcs.SetResult(fullFileName);
            }).Start();

            string result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl failed to download the file!");

            return result.Replace("\n", "").Replace(Environment.NewLine, "");
        }

        #endregion
    }
}
