using log4net;
using System;

namespace NoeSbot.Helpers
{
    public static class LogHelper
    {
        private static ILog _log;

        public static void Initialize(Type type)
        {
            _log = LogManager.GetLogger(type);
        }

        public static void Log(string message, LogLevel level = LogLevel.Info, bool withConsole = true)
        {
            if (_log == null)
                throw new Exception("Logger not initialized yet");

            switch (level)
            {
                case LogLevel.Info:
                    _log.Info(message);
                    break;
                case LogLevel.Error:
                    _log.Error(message);
                    break;
                case LogLevel.Debug:
                    _log.Debug(message);
                    break;
                case LogLevel.Warning:
                    _log.Warn(message);
                    break;
            }

            if (withConsole)
                Console.WriteLine(message);
        }

        public static void LogInfo(string message, bool withConsole = true) => Log(message, LogLevel.Info, withConsole);
        public static void LogError(string message, bool withConsole = true) => Log(message, LogLevel.Error, withConsole);
        public static void LogDebug(string message, bool withConsole = true) => Log(message, LogLevel.Debug, withConsole);
        public static void LogWarning(string message, bool withConsole = true) => Log(message, LogLevel.Warning, withConsole);
    }

    public enum LogLevel
    {
        Info,
        Error,
        Debug,
        Warning
    }
}