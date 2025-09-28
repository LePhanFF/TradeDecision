
#region Using declarations
using System;
using System.Collections.Generic;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class PeersModel { private readonly Dictionary<string,double> priorClose=new(StringComparer.OrdinalIgnoreCase); private readonly Dictionary<string,bool> gapClosed=new(StringComparer.OrdinalIgnoreCase);
        public void SetPriorRthClose(string sym,double price){ priorClose[sym]=price; gapClosed[sym]=false; }
        public void ObserveBar(string sym,double high,double low){ if(priorClose.ContainsKey(sym) && !gapClosed[sym]){ var p=priorClose[sym]; if(high>=p && low<=p) gapClosed[sym]=true; } }
        public Dictionary<string,bool> SnapshotGapClosed(){ return new Dictionary<string,bool>(gapClosed); }
    }
}
