using NoeSbot.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoeSbot
{
    public static class Globals
    {
        private static Dictionary<string, object> _cacheItems = new Dictionary<string, object>();
        private static object locker = new object();
        private static Random rnd = new Random();
        private static ConcurrentList<ulong> _nukedChannels = new ConcurrentList<ulong>();

        public static void LoadGlobals()
        {
            lock (locker)
            {
                _cacheItems.Add("PunishedImages", ProcessDirectory(@"Images\PunishModule\"));
            }
        }

        public static FileInfo[] PunishedImages
        {
            get
            {
                return (FileInfo[])_cacheItems["PunishedImages"];
            }
        }
        public static FileInfo RandomPunishedImage
        {
            get
            {
                int r = rnd.Next(PunishedImages.Length);
                return PunishedImages[r];
            }
        }

        public static ConcurrentList<ulong> NukedChannels
        {
            get
            {
                return _nukedChannels;
            }
        }

        public static void NukeChannel(ulong channel)
        {
            _nukedChannels.Add(channel);
        }

        public static void DeNukeChannel(ulong channel)
        {
            _nukedChannels.Remove(channel);
        }

        #region Private methods

        private static FileInfo[] ProcessDirectory(string targetDirectory)
        {
            var d = new DirectoryInfo(targetDirectory);
            return d.GetFiles();
        }

        #endregion
    }
}
