
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class PriceBucket { public double Price; public int Count; }
    internal struct LevelScore { public double Price; public double Score; public LevelScore(double p,double s){ Price=p; Score=s; } }
    internal struct RangeBand { public double Start; public double End; public RangeBand(double a,double b){ Start=a; End=b; } }

    internal sealed class ProfileSnapshot
    {
        public double Poc; public double Vah; public double Val;
        public double SessionHigh; public double SessionLow;
        public int TotalTpos;
        public List<LevelScore> HVN = new List<LevelScore>();
        public List<LevelScore> LVN = new List<LevelScore>();
        public List<RangeBand> Singles = new List<RangeBand>();
        public bool PoorHigh; public bool PoorLow;
    }

    internal sealed class ProfileBuilder
    {
        private readonly double tick;
        private readonly SortedDictionary<double, PriceBucket> buckets = new SortedDictionary<double, PriceBucket>();
        private double hi = double.MinValue, lo = double.MaxValue;

        public ProfileBuilder(double tickSize) { tick = tickSize; }
        private double Snap(double p) { return Math.Round(p / tick) * tick; }
        public void Reset() { buckets.Clear(); hi = double.MinValue; lo = double.MaxValue; }

        public void AddBar(double o,double h,double l,double c)
        {
            if (h > hi) hi = h;
            if (l < lo) lo = l;
            double a = Snap(Math.Min(h,l)), b = Snap(Math.Max(h,l));
            int steps = (int)Math.Round((b - a) / tick);
            for (int i = 0; i <= steps; i++)
            {
                double px = Snap(a + i * tick);
                PriceBucket bk;
                if (!buckets.TryGetValue(px, out bk))
                {
                    bk = new PriceBucket { Price = px, Count = 0 };
                    buckets[px] = bk;
                }
                bk.Count++;
                buckets[px] = bk;
            }
        }

        public ProfileSnapshot Compute()
        {
            var s = new ProfileSnapshot();
            if (buckets.Count == 0) return s;

            var ordered = buckets.Values.OrderBy(b => b.Price).ToList();
            s.TotalTpos = ordered.Sum(b => b.Count);
            int maxCount = ordered.Max(b => b.Count);
            s.Poc = ordered.Where(b => b.Count == maxCount).OrderByDescending(b => b.Price).First().Price;

            int idx = -1;
            for (int i = 0; i < ordered.Count; i++) if (ordered[i].Price == s.Poc) { idx = i; break; }
            int iLow = idx, iHigh = idx;
            int cum = ordered[idx].Count;
            int target = (int)Math.Ceiling(s.TotalTpos * 0.70);
            while (cum < target && (iLow > 0 || iHigh < ordered.Count - 1))
            {
                int lc = iLow > 0 ? ordered[iLow - 1].Count : -1;
                int hc = iHigh < ordered.Count - 1 ? ordered[iHigh + 1].Count : -1;
                if (hc >= lc) { if (iHigh < ordered.Count - 1) { iHigh++; cum += ordered[iHigh].Count; } else if (iLow > 0) { iLow--; cum += ordered[iLow].Count; } else break; }
                else          { if (iLow  > 0)               { iLow--;  cum += ordered[iLow].Count; } else if (iHigh< ordered.Count - 1) { iHigh++; cum += ordered[iHigh].Count; } else break; }
            }
            s.Val = ordered[iLow].Price;
            s.Vah = ordered[iHigh].Price;
            s.SessionHigh = hi; s.SessionLow = lo;

            // HVN/LVN crude detection
            int[] counts = ordered.Select(b => b.Count).ToArray();
            var sorted = counts.OrderBy(x => x).ToArray();
            double p25 = sorted[(int)Math.Floor(0.25 * (sorted.Length - 1))];
            double p75 = sorted[(int)Math.Floor(0.75 * (sorted.Length - 1))];
            for (int i = 1; i < ordered.Count - 1; i++)
            {
                int c = ordered[i].Count;
                if (c > ordered[i-1].Count && c > ordered[i+1].Count && c >= p75)
                    s.HVN.Add(new LevelScore(ordered[i].Price, Math.Min(1.0, c / (double)maxCount)));
                if (c < ordered[i-1].Count && c < ordered[i+1].Count && c <= p25)
                    s.LVN.Add(new LevelScore(ordered[i].Price, Math.Max(0.0, 1.0 - c / (double)maxCount)));
            }
            s.PoorHigh = ordered[ordered.Count - 1].Count <= 1;
            s.PoorLow  = ordered[0].Count <= 1;
            return s;
        }
    }
}
