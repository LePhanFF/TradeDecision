
#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class PriceBucket{ public double Price; public int Count; }
    internal sealed class ProfileSnapshot { public double Poc, Vah, Val, SessionHigh, SessionLow; public int TotalTpos; public List<(double price,double score)> HVN=new(); public List<(double price,double score)> LVN=new(); public List<(double start,double end)> Singles=new(); public bool PoorHigh, PoorLow; }
    internal sealed class ProfileBuilder
    {
        private readonly double tick; private readonly SortedDictionary<double,PriceBucket> buckets = new();
        private double hi = double.MinValue, lo = double.MaxValue; private DateTime rthOpenEt;
        public ProfileBuilder(double tickSize, DateTime rthOpenEt){ tick=tickSize; this.rthOpenEt=rthOpenEt; }
        private double Snap(double p)=> Math.Round(p/tick)*tick;
        public void Reset(DateTime newOpen){ buckets.Clear(); hi=double.MinValue; lo=double.MaxValue; rthOpenEt=newOpen; }
        public void AddBar(double o,double h,double l,double c, DateTime et){
            hi=Math.Max(hi,h); lo=Math.Min(lo,l);
            double a=Snap(Math.Min(h,l)), b=Snap(Math.Max(h,l));
            int steps=(int)Math.Round((b-a)/tick);
            for(int i=0;i<=steps;i++){ double px=Snap(a+i*tick); if(!buckets.TryGetValue(px, out var bk)){ bk=new PriceBucket{Price=px}; buckets[px]=bk; } bk.Count++; }
        }
        public ProfileSnapshot Compute(){
            var s=new ProfileSnapshot(); if(buckets.Count==0) return s;
            s.TotalTpos=buckets.Values.Sum(b=>b.Count);
            var max=buckets.Values.Max(b=>b.Count);
            s.Poc=buckets.Values.Where(b=>b.Count==max).OrderByDescending(b=>b.Price).First().Price;
            var ordered=buckets.Values.OrderBy(b=>b.Price).ToList();
            int idx=ordered.FindIndex(b=>b.Price==s.Poc), iLow=idx,iHigh=idx,cum=ordered[idx].Count, target=(int)Math.Ceiling(s.TotalTpos*0.70);
            while(cum<target && (iLow>0 || iHigh<ordered.Count-1)){
                int lc=iLow>0?ordered[iLow-1].Count:-1, hc=iHigh<ordered.Count-1?ordered[iHigh+1].Count:-1;
                if(hc>=lc){ if(iHigh<ordered.Count-1){ iHigh++; cum+=ordered[iHigh].Count; } else if(iLow>0){ iLow--; cum+=ordered[iLow].Count; } else break; }
                else{ if(iLow>0){ iLow--; cum+=ordered[iLow].Count; } else if(iHigh<ordered.Count-1){ iHigh++; cum+=ordered[iHigh].Count; } else break; }
            }
            s.Val=ordered[iLow].Price; s.Vah=ordered[iHigh].Price; s.SessionHigh=hi; s.SessionLow=lo;
            // crude HVN/LVN
            var counts=ordered.Select(b=>b.Count).ToArray(); var sorted=counts.OrderBy(x=>x).ToArray();
            double p25=sorted[(int)Math.Floor(0.25*(sorted.Length-1))], p75=sorted[(int)Math.Floor(0.75*(sorted.Length-1))];
            for(int i=1;i<ordered.Count-1;i++){ if(ordered[i].Count>ordered[i-1].Count && ordered[i].Count>ordered[i+1].Count && ordered[i].Count>=p75) s.HVN.Add((ordered[i].Price, Math.Min(1.0, ordered[i].Count/(double)max))); if(ordered[i].Count<ordered[i-1].Count && ordered[i].Count<ordered[i+1].Count && ordered[i].Count<=p25) s.LVN.Add((ordered[i].Price, 1.0-ordered[i].Count/(double)max)); }
            s.PoorHigh = ordered[^1].Count<=1; s.PoorLow = ordered[0].Count<=1;
            return s;
        }
    }
}
