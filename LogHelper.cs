using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ChaseLabs.Logging
{
    /// <summary>
    /// <list type="table">
    /// <item>
    /// Author: Drew Chase
    /// </item>
    /// <item>
    /// Company: Chase Labs
    /// </item>
    /// </list>
    /// </summary>
    public class LogHelper
    {

        private static LogHelper _singleton;
        public static LogHelper Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new LogHelper();
                }

                return _singleton;
            }
        }

        private StreamWriter writer;
        private string log_path;

        protected LogHelper() { }

        public void Init(string log_path)
        {
            try
            {
                this.log_path = log_path;
                writer = new StreamWriter(log_path);
                writer.AutoFlush = true;
                Console.SetOut(writer);
            }
            catch { }
        }

        public void Close()
        {
            StreamWriter stdOut = new StreamWriter(Console.OpenStandardOutput());
            stdOut.AutoFlush = true;
            Console.SetOut(stdOut);
            writer.Dispose();
            System.Threading.Thread.Sleep(1000);
            File.Move(log_path, Path.Combine(Directory.GetParent(log_path).FullName, DateTime.Now.ToString().Replace(":", "-").Replace("/", "-") + ".log"));
        }

        public static log4net.ILog GetLogger([CallerFilePath]string filename = "")
        {
            return log4net.LogManager.GetLogger(filename);
        }
    }
}
