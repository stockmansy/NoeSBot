using System;
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

        #region Private methods

        private static FileInfo[] ProcessDirectory(string targetDirectory)
        {
            var d = new DirectoryInfo(targetDirectory);
            return d.GetFiles();
        }

        #endregion
    }
}
