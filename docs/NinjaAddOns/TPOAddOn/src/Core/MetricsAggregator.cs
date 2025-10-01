using System;
using System.Collections.Generic;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    public class MetricsAggregator
    {
        readonly string symbol;
        readonly double tickSize;

        IbWindow ib60 = new IbWindow(60);
        IbWindow ib15 = new IbWindow(15);

        public MetricsAggregator(string symbol, double tickSize)
        {
            this.symbol = symbol; this.tickSize = tickSize;
        }

        public MetricsSnapshot Build(TpoBar b, string role)
        {
            // freeze IBH/L once window duration passes
            UpdateIb(ib60, 60, b);
            UpdateIb(ib15, 15, b);

            var s = new MetricsSnapshot();
            s.Symbol = b.Symbol; s.Role = role; s.TsEt = b.Et; s.TsUtc = b.Utc; s.Last = b.C;
            s.Session.SinceOpenMin = b.SinceOpenMin; s.Session.TickSize = tickSize;

            // Levels & value
            s.Levels.Ib.High = ib60.High; s.Levels.Ib.Low = ib60.Low;
            s.Levels.Value.Vah = b.Vah; s.Levels.Value.Poc = b.Poc; s.Levels.Value.Val = b.Val;
            s.Levels.Dpoc.Current = b.Poc;

            // ibDual
            s.IbDual.Primary.Minutes = 60; s.IbDual.Primary.Ib.High = ib60.High; s.IbDual.Primary.Ib.Low = ib60.Low;
            s.IbDual.Primary.Ib.Range = Math.Abs(ib60.High - ib60.Low);
            s.IbDual.Primary.Ib.SizeTicks = (int)Math.Round(s.IbDual.Primary.Ib.Range / Math.Max(tickSize,1e-9));

            s.IbDual.Preview.Minutes = 15; s.IbDual.Preview.Ib.High = ib15.High; s.IbDual.Preview.Ib.Low = ib15.Low;
            s.IbDual.Preview.Ib.Range = Math.Abs(ib15.High - ib15.Low);
            s.IbDual.Preview.Ib.SizeTicks = (int)Math.Round(s.IbDual.Preview.Ib.Range / Math.Max(tickSize,1e-9));

            // Value block
            s.Value.Tpo.Vah = b.Vah; s.Value.Tpo.Poc = b.Poc; s.Value.Tpo.Val = b.Val; s.Value.Tpo.TotalTpos = b.TotalTpos;

            // Nodes/Singles passthrough with small transforms
            if (b.Hvns != null) for (int i=0; i<Math.Min(3,b.Hvns.Count); i++)
                s.Profile.Nodes.Hvn.Top3.Add(new NodeItem{ Price=b.Hvns[i].Price, Prominence=b.Hvns[i].Prominence, Method="TPO", DistanceTicks=(int)Math.Round(Math.Abs(b.Poc - b.Hvns[i].Price)/Math.Max(tickSize,1e-9)) });
            if (b.Lvns != null) for (int i=0; i<Math.Min(5,b.Lvns.Count); i++)
                s.Profile.Nodes.Lvn.UpTo5.Add(new NodeItem{ Price=b.Lvns[i].Price, Prominence=b.Lvns[i].Prominence, Method="TPO", DistanceTicks=(int)Math.Round(Math.Abs(b.Poc - b.Lvns[i].Price)/Math.Max(tickSize,1e-9)) });
            s.Profile.Nodes.NodeCount = (b.Hvns!=null? b.Hvns.Count:0);

            if (b.Singles != null) for (int i=0;i<b.Singles.Count;i++)
                s.Profile.Singles.Levels.Add(new SingleSpan{ Start=b.Singles[i].Start, End=b.Singles[i].End });

            return s;
        }
		void UpdateIb(IbWindow w, int minutes, TpoBar b)
		{
		    if (w.Frozen) return;
		    if (b.SinceOpenMin < minutes)
		    {
		        if (w.Count == 0)
		        {
		            w.High = b.H;            // was b.High
		            w.Low  = b.L;            // was b.Low
		            w.Count = 1;
		        }
		        else
		        {
		            if (b.H > w.High) w.High = b.H;   // was b.High
		            if (b.L < w.Low)  w.Low  = b.L;   // was b.Low
		            w.Count++;
		        }
		    }
		    else w.Frozen = true;
		}


        class IbWindow { public int Count; public double High; public double Low; public bool Frozen; public IbWindow(int m){} }
    }
}
