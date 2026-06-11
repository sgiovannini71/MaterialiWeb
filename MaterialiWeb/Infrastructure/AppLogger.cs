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
                    + "[user=" + ResolveCurrentUser() + "] "
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

        private static string ResolveCurrentUser()
        {
            try
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    return "n/a";
                }

                var sessionUsername = context.Session != null && context.Session["Username"] != null
                    ? context.Session["Username"].ToString()
                    : string.Empty;

                var windowsUser = context.User != null &&
                    context.User.Identity != null &&
                    !string.IsNullOrWhiteSpace(context.User.Identity.Name)
                        ? context.User.Identity.Name
                        : string.Empty;

                if (!string.IsNullOrWhiteSpace(sessionUsername) && !string.IsNullOrWhiteSpace(windowsUser))
                {
                    return sessionUsername + " (" + windowsUser + ")";
                }

                if (!string.IsNullOrWhiteSpace(sessionUsername))
                {
                    return sessionUsername;
                }

                if (!string.IsNullOrWhiteSpace(windowsUser))
                {
                    return windowsUser;
                }
            }
            catch
            {
            }

            return "n/a";
        }
    }
}
