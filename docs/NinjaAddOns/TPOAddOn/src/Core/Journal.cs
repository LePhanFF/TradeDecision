
#region Using declarations
using System;
using System.IO;
using System.Text;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class Journal
    {
        private readonly object fileLock = new object();
        private readonly string baseDir;
        public Journal(string baseDir){ this.baseDir = baseDir; }
        private string LogPath(string symbol, DateTime et) {
            var dir = Path.Combine(baseDir, symbol, "logs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, et.ToString("yyyy-MM-dd") + ".log");
        }
        public void Append(string symbol, DateTime et, string line){
            try{ lock(fileLock){ File.AppendAllText(LogPath(symbol, et), $"[{ClockEt.HM(et)}] {line}\n", Encoding.UTF8); } } catch {}
        }
        public void Header(string symbol, DateTime et){
            Append(symbol, et, "Time   Sym  Bias   Day  Conf  IB[H/L]      dPOC     VA[VAL/POC/VAH]  Singles  OTF  Morph");
        }
    }
}
