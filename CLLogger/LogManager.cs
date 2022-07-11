using ChaseLabs.CLLogger.Events;
using ChaseLabs.CLLogger.Interfaces;
using System.Diagnostics;
using System.Security.AccessControl;
using static ChaseLabs.CLLogger.Lists;
using Timer = System.Timers.Timer;

namespace ChaseLabs.CLLogger
{
    /// <summary>
    /// The Method in which the log is dumped
    /// </summary>
    public enum DumpType
    {
        /// <summary>
        /// The log is dumped as soon as it is written to.
        /// </summary>
        NoBuffer,

        /// <summary>
        /// Dumps every X milliseconds
        /// </summary>
        Interval,

        /// <summary>
        /// The Log is never dumped to file.
        /// </summary>
        NoDump
    }

    /// <summary>
    /// <para>Author: Drew Chase</para>
    /// <para>Company: Chase Labs</para>
    /// </summary>
    public class LogManager : ILog
    {
        #region Fields

        /// <summary>
        /// </summary>
        public static LogManager Singleton = Singleton ??= new LogManager();

        private static LogManager? _singleton;

        private readonly List<string> MessageQueue = new List<string>();

        #endregion Fields

        #region Private Constructors

        private LogManager(string path = "./logs/", LogTypes _MinimumLogType = LogTypes.All, string _pattern_prefix = "[ %TYPE%: %DATE% ]: %MESSAGE%", DumpType dumpType = DumpType.NoDump, int dumpInterval = 10000)
        {
            _singleton = this;
            Path = Directory.CreateDirectory(System.IO.Path.GetFullPath(path)).FullName;
            MinimumLogType = _MinimumLogType;
            Pattern = _pattern_prefix;
            DumpType = dumpType;
            DumpInterval = dumpInterval;
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (DumpType == DumpType.Interval)
                {
                    MessageQueue = new();
                    DumpQueue(true);
                    SetDirectoryPermissions();
                }

                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
            }

            switch (MinimumLogType)
            {
                case LogTypes.All:
                    IsFatalEnabled = true; IsWarnEnabled = true; IsInfoEnabled = true; IsDebugEnabled = true; IsErrorEnabled = true;
                    break;

                case LogTypes.Debug:
                    IsFatalEnabled = true; IsWarnEnabled = true; IsInfoEnabled = true; IsDebugEnabled = true; IsErrorEnabled = true;
                    break;

                case LogTypes.Info:
                    IsFatalEnabled = true; IsWarnEnabled = true; IsInfoEnabled = true; IsDebugEnabled = false; IsErrorEnabled = true;
                    break;

                case LogTypes.Warn:
                    IsFatalEnabled = true; IsWarnEnabled = true; IsInfoEnabled = false; IsDebugEnabled = false; IsErrorEnabled = true;
                    break;

                case LogTypes.Error:
                    IsFatalEnabled = true; IsWarnEnabled = false; IsInfoEnabled = false; IsDebugEnabled = false; IsErrorEnabled = true;
                    break;

                case LogTypes.Fatal:
                    IsFatalEnabled = true; IsWarnEnabled = false; IsInfoEnabled = false; IsDebugEnabled = false; IsErrorEnabled = false;
                    break;

                default:
                    IsFatalEnabled = true; IsWarnEnabled = true; IsInfoEnabled = true; IsDebugEnabled = true; IsErrorEnabled = true;
                    MinimumLogType = LogTypes.All;
                    break;
            }
        }

        #endregion Private Constructors

        #region Delegates

        public delegate void LoggedMessageEventHandler(object sender, LogEventArgs args);

        #endregion Delegates

        #region Events

        public event LoggedMessageEventHandler LoggedMessage;

        #endregion Events

        #region Properties

        public int DumpInterval { get; private set; }

        /// <summary>
        /// The Method in which the log will dump
        /// </summary>
        public DumpType DumpType { get; private set; }

        /// <summary>
        /// Returns if Debug Messages will be Logged
        /// </summary>
        public bool IsDebugEnabled { get; private set; }

        /// <summary>
        /// Returns if Error Messages will be Logged
        /// </summary>
        public bool IsErrorEnabled { get; private set; }

        /// <summary>
        /// Returns if Fatal Messages will be Logged
        /// </summary>
        public bool IsFatalEnabled { get; private set; }

        /// <summary>
        /// Returns if Informational Messages will be Logged
        /// </summary>
        public bool IsInfoEnabled { get; private set; }

        /// <summary>
        /// Returns if Warning Messages will be Logged
        /// </summary>
        public bool IsWarnEnabled { get; private set; }

        public LogTypes MinimumLogType { get; private set; }

        /// <summary>
        /// Gets the Current Log File Path
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// <para> %DATE% - Current Date and Time </para>
        /// <para> %TYPE% - Log Type </para>
        /// <para> %MESSAGE% - The Message </para>
        /// Example
        /// <code>
        ///[ %TYPE%: %DATE% ]: %MESSAGE%
        /// </code>
        /// <para> [ ERROR: 01/01/1999/8:30:25 ] Example Message is Surprising </para>
        /// </summary>
        public string Pattern { get; private set; }

        private string LatestLogFilePath => System.IO.Path.Combine(Path, "latest.log");

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// <para>Default Path:
        /// <code>C:\Default-Log-File-Location(Please_Change)\latest.log</code>
        /// </para>
        /// <para>Default Minimum Log Type: All Logs</para>
        /// </summary>
        /// <returns>a Empty LogManager Object</returns>
        public static LogManager Empty()
        {
            return new LogManager();
        }

        /// <summary>
        /// Initializes an Empty LogManager
        /// <para>See <see cref="Empty"/></para>
        /// </summary>
        /// <returns></returns>
        public static LogManager Init()
        {
            return Empty();
        }

        private void Close(bool force_dump = true, int attempt = 0)
        {
            if (force_dump)
                DumpQueue(false);
            string newFile = $"{new FileInfo(LatestLogFilePath).CreationTime:(MM-dd-yyyy) hh-mm-ss-fftt} - {DateTime.Now.Ticks:(MM-dd-yyyy) hh-mm-ss-fftt}.log";
            if (File.Exists(LatestLogFilePath))
            {
                if (File.Exists(newFile))
                {
                    newFile = $"{new FileInfo(LatestLogFilePath).CreationTime:(MM-dd-yyyy) hh-mm-ss-fftt} - {DateTime.Now.Ticks:(MM-dd-yyyy) hh-mm-ss-fftt} ({new Random().Next(0, 1000)}).log";
                }
                try
                {
                    File.Move(LatestLogFilePath, System.IO.Path.Combine(Path, newFile));
                }
                catch
                {
                    if (attempt <= 3)
                    {
                        Thread.Sleep(500);
                        Close(force_dump, attempt + 1);
                    }
                }
            }
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled)
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string), LogTypes.Debug);
        }

        public void Debug(params object[] messages)
        {
            if (IsDebugEnabled)
                foreach (object message in messages)
                {
                    SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string), LogTypes.Debug);
                }
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "DEBUG").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} at line {new StackTrace(exception, true).GetFrame(0).GetFileLineNumber()}]:{Environment.NewLine}{exception.StackTrace}", LogTypes.Debug);
        }

        public void Error(object message)
        {
            if (IsErrorEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string), LogTypes.Error);
            }
        }

        public void Error(params object[] messages)
        {
            if (IsErrorEnabled)
            {
                foreach (object message in messages)
                {
                    SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string), LogTypes.Error);
                }
            }
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "ERROR").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} at line {new StackTrace(exception, true).GetFrame(0).GetFileLineNumber()}]:{Environment.NewLine}{exception.StackTrace}", LogTypes.Error);
            }
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "FATAL").Replace("%MESSAGE%", message as string), LogTypes.Fatal);
            }
        }

        public void Fatal(params object[] messages)
        {
            if (IsFatalEnabled)
            {
                foreach (object message in messages)
                {
                    SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "FATAL").Replace("%MESSAGE%", message as string), LogTypes.Fatal);
                }
            }
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "FATAL").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} at line {new StackTrace(exception, true).GetFrame(0).GetFileLineNumber()}]:{Environment.NewLine}{exception.StackTrace}", LogTypes.Fatal);
            }
        }

        public void Info(object message, Exception exception)
        {
            SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} at line {new StackTrace(exception, true).GetFrame(0).GetFileLineNumber()}]:{Environment.NewLine}{exception.StackTrace}", LogTypes.Info);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string), LogTypes.Info);
            }
        }

        public void Info(params object[] messages)
        {
            if (IsInfoEnabled)
            {
                foreach (object message in messages)
                {
                    SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "INFO").Replace("%MESSAGE%", message as string), LogTypes.Info);
                }
            }
        }

        /// <summary>
        /// Runs Every Time a Message is Logged
        /// </summary>
        public virtual void OnMessageLogged(string message)
        {
            LoggedMessage?.Invoke(this, new LogEventArgs() { Log = message });
        }

        /// <summary>
        /// Sets the dump method, with interval.
        /// </summary>
        /// <param name="type">DEFAULT CONTINOUS</param>
        /// <param name="interval">How often the log is dumped to file</param>
        /// <returns></returns>
        public LogManager SetDumpMethod(int interval)
        {
            return new LogManager(Path, MinimumLogType, Pattern, DumpType.Interval, interval);
        }

        /// <summary>
        /// Sets the dump method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LogManager SetDumpMethod(DumpType type)
        {
            return new LogManager(Path, MinimumLogType, Pattern, type, 10000);
        }

        /// <summary>
        /// Sets the Log File Path
        /// </summary>
        /// <param name="path">
        /// Log Path
        /// <para>Example</para>
        /// <code>path="c:\temp\"</code>
        /// </param>
        /// <returns></returns>
        public LogManager SetLogDirectory(string path)
        {
            return new LogManager(path, MinimumLogType, Pattern, DumpType, DumpInterval);
        }

        /// <summary>
        /// Sets the Log File Path
        /// </summary>
        /// <param name="path">
        /// Log Path
        /// <para>Example</para>
        /// <code>path="c:\temp\"</code>
        /// </param>
        /// <returns></returns>
        public LogManager SetLogDirectory(FileInfo path)
        {
            return new LogManager(path.FullName, MinimumLogType, Pattern, DumpType, DumpInterval);
        }

        /// <summary>
        /// Sets the Minimum Log Type
        /// </summary>
        /// <param name="MinimumLogType">See <see cref="LogTypes"/> Documentation for Sorting Order</param>
        /// <returns></returns>
        public LogManager SetMinimumLogType(LogTypes MinimumLogType)
        {
            return new LogManager(Path, MinimumLogType, Pattern, DumpType, DumpInterval);
        }

        /// <summary>
        /// <para> %DATE% - Current Date and Time </para>
        /// <para> %TYPE% - Log Type </para>
        /// <para> %MESSAGE% - The Message </para>
        /// Example
        /// <code>
        ///[ %TYPE%: %DATE% ]: %MESSAGE%
        /// </code>
        /// <para> [ ERROR: 01/01/1999/8:30:25 ] Example Message is Surprising </para>
        /// </summary>
        /// <returns> </returns>
        public LogManager SetPattern(string Pattern)
        {
            return new LogManager(Path, MinimumLogType, Pattern, DumpType, DumpInterval);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string), LogTypes.Warn);
            }
        }

        public void Warn(params object[] messages)
        {
            if (IsWarnEnabled)
            {
                foreach (object message in messages)
                {
                    SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string), LogTypes.Warn);
                }
            }
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                SendMessage(Pattern.Replace("%DATE%", DateTime.Now.ToString()).Replace("%TYPE%", "WARN").Replace("%MESSAGE%", message as string) + $" [Exception {exception.GetType().Name} at line {new StackTrace(exception, true).GetFrame(0).GetFileLineNumber()}]:{Environment.NewLine}{exception.StackTrace}", LogTypes.Warn);
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Manually Dumps Log Queue.
        /// </summary>
        internal void DumpQueue(bool repeat)
        {
            Task.Run(() =>
            {
                if (DumpType == DumpType.NoBuffer || MessageQueue.Any())
                {
                    try
                    {
                        string output = "";
                        string[] messages = MessageQueue.ToArray();
                        foreach (string message in messages)
                        {
                            output += message + "\n";
                        }
                        using (StreamWriter writer = new(LatestLogFilePath, true, System.Text.Encoding.UTF8))
                        {
                            writer.Write(output);
                        }
                        MessageQueue.Clear();
                        FileInfo info = new(LatestLogFilePath);
                        if (info.Length > 5 * Math.Pow(1024, 2))
                        {
                            Close(false);
                        }
                    }
                    catch (Exception e)
                    {
                        Fatal($"Unable to save log to file!", e);
                        Environment.Exit(2);
                    }
                }
                Thread.Sleep(DumpInterval);
            }).ContinueWith(e =>
            {
                if (repeat)
                    DumpQueue(true);
            });
        }

        #endregion Internal Methods

        #region Private Methods

        private void SendMessage(object message, LogTypes type)
        {
            if (message == null) return;
            if (DumpType == DumpType.Interval)
            {
                MessageQueue.Add(message.ToString());
            }
            else if (DumpType == DumpType.NoBuffer)
            {
                DumpQueue(false);
            }

            Console.ForegroundColor = type switch
            {
                LogTypes.Debug => ConsoleColor.White,
                LogTypes.Info => ConsoleColor.Green,
                LogTypes.Warn => ConsoleColor.DarkYellow,
                LogTypes.Error => ConsoleColor.Red,
                LogTypes.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            OnMessageLogged(message as string);
        }

        private void SetDirectoryPermissions()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    DirectoryInfo directoryInfo = new(Path);
                    DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                    var currentUserIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    FileSystemAccessRule fileSystemRule = new(currentUserIdentity.Name, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow);

                    directorySecurity.AddAccessRule(fileSystemRule);
                    directoryInfo.SetAccessControl(directorySecurity);
                }
                catch (Exception e)
                {
                    Fatal("Unable to set Directory Permissions", e);
                }
            }
        }

        #endregion Private Methods
    }
}