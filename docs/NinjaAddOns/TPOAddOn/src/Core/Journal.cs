
using System;
using System.IO;
using System.Text;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class Journal
    {
        private readonly object fileLock = new object();
        private readonly string baseDir;
        public Journal(string baseDir) { this.baseDir = baseDir; }
        private string LogPath(string symbol, DateTime et)
        {
            string dir = Path.Combine(baseDir, symbol, "logs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, et.ToString("yyyy-MM-dd") + ".log");
        }
        public void Append(string symbol, DateTime et, string line)
        {
            try { lock (fileLock) { File.AppendAllText(LogPath(symbol, et), "[" + ClockEt.HM(et) + "] " + line + "\n", Encoding.UTF8); } } catch {}
        }
    }
}
