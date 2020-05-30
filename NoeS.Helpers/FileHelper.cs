using System.IO;

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
