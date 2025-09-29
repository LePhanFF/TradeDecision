
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class PeersModel
    {
        private readonly Dictionary<string,double> priorClose = new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,bool> gapClosed = new Dictionary<string,bool>(StringComparer.OrdinalIgnoreCase);
        public void SetPriorRthClose(string sym,double price){ priorClose[sym]=price; gapClosed[sym]=false; }
        public void ObserveBar(string sym,double high,double low)
        {
            bool has; if (!priorClose.ContainsKey(sym)) return;
            if (!gapClosed.TryGetValue(sym, out has)) has=false;
            if (!has)
            {
                double p = priorClose[sym];
                if (high >= p && low <= p) gapClosed[sym] = true;
            }
        }
        public Dictionary<string,bool> SnapshotGapClosed(){ return new Dictionary<string,bool>(gapClosed); }
    }
}
