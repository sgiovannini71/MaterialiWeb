using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web;

namespace MaterialiGestioneWeb.Infrastructure
{
    public static class AppLogger
    {
        private static readonly object SyncRoot = new object();

        public static void Info(string source, string message)
        {
            Write("INFO", source, message, null);
        }

        public static void Error(string source, string message, Exception exception)
        {
            Write("ERROR", source, message, exception);
        }

        private static void Write(string level, string source, string message, Exception exception)
        {
            try
            {
                var directory = ResolveLogDirectory();
                Directory.CreateDirectory(directory);

                var logFilePath = Path.Combine(
                    directory,
                    "materiali-it-" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");

                var line =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
                    + " [" + level + "] "
                    + source
                    + " - "
                    + message;

                if (exception != null)
                {
                    line += Environment.NewLine + exception;
                }

                lock (SyncRoot)
                {
                    File.AppendAllText(logFilePath, line + Environment.NewLine, System.Text.Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        private static string ResolveLogDirectory()
        {
            var configuredPath = ConfigurationManager.AppSettings["AppLogDirectory"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                configuredPath = @"App_Data\Logs";
            }

            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            var basePath = HttpRuntime.AppDomainAppPath ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, configuredPath);
        }
    }
}
