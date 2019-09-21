using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class FileHelper
    {
        public static FileInfo[] ProcessDirectory(string targetDirectory)
        {
            var d = new DirectoryInfo(targetDirectory);

            if (!d.Exists)
                d.Create();

            return d.GetFiles();
        }
    }
}
