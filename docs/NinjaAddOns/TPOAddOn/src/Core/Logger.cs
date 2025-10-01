using System;
using NinjaTrader.Code;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    public static class Logger
    {
        public static void Info(string s){ Safe("[INFO] " + s); }
        public static void Warn(string s){ Safe("[WARN] " + s); }
        public static void Error(string s){ Safe("[ERROR] " + s); }

        static void Safe(string s)
        {
            try { Output.Process(s, PrintTo.OutputTab1); } catch {}
        }
    }
}
