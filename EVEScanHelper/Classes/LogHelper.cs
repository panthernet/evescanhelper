using System;
using System.IO;

namespace EVEScanHelper.Classes
{
    public static class LogHelper
    {
        private static string _logPath;
        
        public static void Log(string message)
        {
            try
            {
                _logPath = _logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                var file = Path.Combine(_logPath, "app.log");

                if (!Directory.Exists(_logPath))
                    Directory.CreateDirectory(_logPath);

                File.AppendAllText(file, $@"{DateTime.Now,-19}: {message}{Environment.NewLine}");
            }
            catch
            {
                // ignored
            }
        }

        public static void LogEx(string message, Exception exception)
        {
            try
            {
                _logPath = _logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                var file = Path.Combine(_logPath, "app.log");

                if (!Directory.Exists(_logPath))
                    Directory.CreateDirectory(_logPath);

                File.AppendAllText(file, $@"{DateTime.Now,-19}: {message} {Environment.NewLine}{exception}{exception.InnerException}{Environment.NewLine}");
            }
            catch
            {
                // ignored
            }
        }

    }
}
