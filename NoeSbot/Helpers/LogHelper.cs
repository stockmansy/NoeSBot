using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class LogHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public static void LogWithConsole(string message, LogLevel level = LogLevel.Info)
        {
            Log(message, level);
            Console.WriteLine(message);
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            switch (level)
            {
                case LogLevel.Info:
                    log.Info(message);
                    break;
                case LogLevel.Error:
                    log.Error(message);
                    break;
                case LogLevel.Debug:
                    log.Debug(message);
                    break;
                case LogLevel.Warning:
                    log.Warn(message);
                    break;
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Error,
        Debug,
        Warning
    }
}
