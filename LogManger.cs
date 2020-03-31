using ChaseLabs.CLLogger.Events;
using ChaseLabs.CLLogger.Interfaces;
using System;
using System.IO;
using static ChaseLabs.CLLogger.Lists;

namespace ChaseLabs.CLLogger
{
    /// <summary>
    /// <para>
    /// Author: Drew Chase
    /// </para>
    /// <para>
    /// Company: Chase Labs
    /// </para>
    /// </summary>
    public class LogManger : ILog
    {
        private string _pattern_prefix;
        private static string _path = "";
        private static LogTypes minLogType = LogTypes.All;
        private readonly bool fatal = false, warn = false, info = false, debug = false, error = false, logDefaultConsole = false;

        public static LogManger Init()
        {
            return Empty();
        }

        /// <summary>
        /// Sets the Minimum Log Type
        /// <remark>
        /// <para>
        /// See <see cref="LogTypes"/> Documentation for Sorting Order
        /// </para>
        /// </remark>
        /// <param name="minLogType"></param>
        /// <returns></returns>
        /// </summary>
        public LogManger SetMinLogType(LogTypes minLogType)
        {
            return new LogManger(Path, minLogType, Pattern, logDefaultConsole);
        }
        /// <summary>
        /// Sets the Log File Path
        /// <remark>
        /// <para>
        /// See <see cref="LogTypes"/> Documentation for Sorting Order
        /// </para>
        /// </remark>
        /// <example>
        /// Example:
        /// <code>
        /// SetLogDirectory("C:\Temp\TestLogFile.txt")
        /// </code>
        /// </example>
        /// <param name="minLogType"></param>
        /// <returns></returns>
        /// </summary>
        public LogManger SetLogDirectory(string path)
        {
            return new LogManger(path, minLogType, Pattern, logDefaultConsole);
        }
        /// <summary>
        /// Sets the Log File Path
        /// <remark>
        /// <para>
        /// See <see cref="LogTypes"/> Documentation for Sorting Order
        /// </para>
        /// </remark>
        /// <example>
        /// Example:
        /// <code>
        /// SetLogDirectory(new System.IO.FileInfo("C:\Temp\TestLogFile.txt"))
        /// </code>
        /// </example>
        /// <param name="minLogType"></param>
        /// <returns></returns>
        /// </summary>
        public LogManger SetLogDirectory(FileInfo path)
        {

            return new LogManger(path.FullName, minLogType, Pattern, logDefaultConsole);
        }
        /// <summary>
        /// Enables Logging for Default Console.
        /// </summary>
        /// <returns></returns>
        public LogManger EnableDefaultConsoleLogging()
        {
            return new LogManger(Path, minLogType, Pattern, true);
        }
        /// <summary>
        /// Disables Logging for Default Console.
        /// </summary>
        /// <returns></returns>
        public LogManger DisableDefaultConsoleLogging()
        {
            return new LogManger(Path, minLogType, Pattern, false);
        }

        /// <summary>
        /// <para>
        /// Default Path: <code>C:\Default-Log-File-Location(Please_Change)\latest.log</code>
        /// </para>
        /// <para>
        /// Default Minimum Log Type: All Logs
        /// </para>
        /// </summary>
        /// <returns>a Empty LogManager Object</returns>
        public static LogManger Empty()
        {
            return new LogManger();
        }

        private LogManger(string path = "", LogTypes _minLogType = LogTypes.All, string _pattern_prefix = "[ %TYPE%: %DATE% ]: %MESSAGE%", bool _logDefaultConsole = true)
        {
            _path = path;
            minLogType = _minLogType;
            Pattern = _pattern_prefix;
            logDefaultConsole = _logDefaultConsole;

            switch (minLogType)
            {
                case LogTypes.All:
                    fatal = true; warn = true; info = true; debug = true; error = true;
                    break;
                case LogTypes.Debug:
                    fatal = true; warn = true; info = true; debug = true; error = true;
                    break;
                case LogTypes.Info:
                    fatal = true; warn = true; info = true; debug = false; error = true;
                    break;
                case LogTypes.Warn:
                    fatal = true; warn = true; info = false; debug = false; error = true;
                    break;
                case LogTypes.Error:
                    fatal = true; warn = false; info = false; debug = false; error = true;
                    break;
                case LogTypes.Fatal:
                    fatal = true; warn = false; info = false; debug = false; error = false;
                    break;
                default:
                    fatal = true; warn = true; info = true; debug = true; error = true;
                    minLogType = LogTypes.All;
                    break;
            }

            AppDomain.CurrentDomain.ProcessExit += Close;
        }

        private void Close(object sender, EventArgs e)
        {
            if (File.Exists(Path))
            {
                File.Move(Path, System.IO.Path.Combine(Directory.GetParent(Path).FullName, DateTime.Now.ToString().Replace(":", "-").Replace("/", "-") + ".log"));
            }
        }

        public bool IsFatalEnabled => fatal;

        public bool IsWarnEnabled => warn;

        public bool IsInfoEnabled => info;

        public bool IsDebugEnabled => debug;

        public bool IsErrorEnabled => error;

        /// <summary>
        /// <para>%DATE% - Current Date</para>
        /// <para>%TYPE% - Log Type</para>
        /// <para>%MESSAGE% - The Message</para>
        /// Example
        /// <code>
        /// [ %TYPE%: %DATE% ]: %MESSAGE%
        /// </code>
        /// </summary>
        public string Pattern { get => _pattern_prefix; set => _pattern_prefix = value; }

        /// <summary>
        /// Gets the Current Log File Path
        /// </summary>
        public string Path => _path;

        public delegate void LoggedMessageEventHandler(object sender, LogEventArgs args);
        public event LoggedMessageEventHandler LoggedMessage;

        /// <summary>
        /// Runs Every Time a Message is Logged
        /// </summary>
        public virtual void OnMessageLogged(string message)
        {
            LoggedMessage?.Invoke(this, new LogEventArgs() { Log = message });
        }

        protected void SendMessage(object message)
        {
            if (!Directory.GetParent(_path).Exists)
            {
                Directory.CreateDirectory(Directory.GetParent(_path).FullName);
            }

            StreamWriter writer = new StreamWriter(_path, true);
            writer.WriteLine(message);
            writer.Flush();
            writer.Dispose();
            writer.Close();
            OnMessageLogged(message as string);
        }

        public void Debug(object message)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string));
        }

        public void Debug(params object[] messages)
        {
            foreach (object message in messages)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string));
            }
        }

        public void Debug(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} ]:{Environment.NewLine}{exception.StackTrace}");
        }

        public void Info(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} ]:{Environment.NewLine}{exception.StackTrace}");

        }

        public void Info(object message)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string));
        }

        public void Info(params object[] messages)
        {
            foreach (object message in messages)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string));
            }
        }

        public void Warn(object message)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string));
        }

        public void Warn(params object[] messages)
        {
            foreach (object message in messages)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string));
            }
        }

        public void Warn(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} ]:{Environment.NewLine}{exception.StackTrace}");

        }

        public void Error(object message)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string));
        }

        public void Error(params object[] messages)
        {
            foreach (object message in messages)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string));
            }
        }

        public void Error(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} ]:{Environment.NewLine}{exception.StackTrace}");
        }

        public void Fatal(object message)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "FATAL").Replace("%MESSAGE%", message as string));
        }

        public void Fatal(params object[] messages)
        {
            foreach (object message in messages)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "FATAL").Replace("%MESSAGE%", message as string));
            }
        }

        public void Fatal(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} ]:{Environment.NewLine}{exception.StackTrace}");
        }
    }
}
