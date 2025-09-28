
#region Using declarations
using System;
using System.IO;
using System.Text;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class JsonStore
    {
        private readonly object fileLock = new object();
        private readonly string baseDir;
        public JsonStore(string baseDir){ this.baseDir = baseDir; }
        private string DailyDir(string symbol, DateTime et){
            var day = et.ToString("yyyy-MM-dd");
            var path = Path.Combine(baseDir, symbol, "daily", day);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(baseDir, symbol, "latest"));
            Directory.CreateDirectory(Path.Combine(baseDir, symbol, "logs"));
            return path;
        }
        public void AppendBar(string symbol, DateTime et, string jsonLine){
            try{ lock(fileLock){ var p = Path.Combine(DailyDir(symbol, et), "bars.jsonl"); File.AppendAllText(p, jsonLine + "\n", Encoding.UTF8); } } catch {}
        }
        public void WriteLatest(string symbol, string json){
            try{ lock(fileLock){ var p = Path.Combine(baseDir, symbol, "latest", symbol + "_latest.json"); var tmp = p + ".tmp"; File.WriteAllText(tmp, json, Encoding.UTF8); if (File.Exists(p)) File.Delete(p); File.Move(tmp, p); } } catch {}
        }
        public void WriteSession(string symbol, DateTime et, string json){
            try{ lock(fileLock){ var p = Path.Combine(DailyDir(symbol, et), "session_summary.json"); File.WriteAllText(p, json, Encoding.UTF8); } } catch {}
        }
    }
}
