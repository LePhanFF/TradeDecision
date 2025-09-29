using System;
using System.IO;
using System.Text;
using System.Threading;
using NinjaTrader.Code;

namespace NinjaTrader.NinjaScript.AddOns
{
    public static class TpoLogger
    {
        private static readonly object _lock = new object();
        private static string _logDir;
        private static string _logPath;
        private static bool _initialized;

        public static void Init(string baseDir)
        {
            try
            {
                _logDir = Path.Combine(baseDir, "logs");
                Directory.CreateDirectory(_logDir);
                _logPath = Path.Combine(_logDir, "tpo_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                _initialized = true;
            }
            catch { _initialized = false; }
        }

        public static void Info(string msg) { Write("INFO", msg); }
        public static void Warn(string msg) { Write("WARN", msg); }
        public static void Error(string msg) { Write("ERROR", msg); }

        private static void Write(string level, string msg)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + level + "] " + msg;
            try { Output.Process(line, PrintTo.OutputTab1); } catch { }

            if (!_initialized) return;
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
