using System.Collections.Generic;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    public static class MetricsBus
    {
        static readonly Dictionary<string, MetricsSnapshot> latest = new Dictionary<string, MetricsSnapshot>();
        public static void Put(MetricsSnapshot s){ if (s==null || string.IsNullOrEmpty(s.Symbol)) return; latest[s.Symbol]=s; }
        public static bool TryGet(string symbol, out MetricsSnapshot snap){ return latest.TryGetValue(symbol, out snap); }
        public static List<MetricsSnapshot> All()
        {
            var list = new List<MetricsSnapshot>();
            foreach (var kv in latest) list.Add(kv.Value);
            return list;
        }
    }
}
