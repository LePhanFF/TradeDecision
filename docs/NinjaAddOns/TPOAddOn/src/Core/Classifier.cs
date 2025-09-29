
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class OpeningContext { public string Location="inRange"; public string Type="OA"; public string Dir="none"; public int GapTicks=0; }
    internal sealed class IbInfo { public double High=double.NaN, Low=double.NaN; public int ExtUpCount=0, ExtDownCount=0, SinceOpenMin=0; }
    internal struct OtfState { public bool Up; public bool Down; public string Frame; }
    internal sealed class CallDecision { public string Bias="neutral", DayType="Neutral"; public int Confidence=50; public List<string> Reasons=new List<string>(); public List<string> Warnings=new List<string>(); public List<string> DegradedBy=new List<string>(); }

    internal sealed class Classifier
    {
        private readonly double tick;
        public Classifier(double tickSize){ tick=tickSize; }

        public OpeningContext ClassifyOpening(double rthOpen,double priorVah,double priorVal,double priorHi,double priorLo,double prevClose,double firstOpen,double firstClose,double firstHi,double firstLo)
        {
            var ctx = new OpeningContext();
            double gap = firstOpen - prevClose;
            ctx.Dir = gap>0? "up" : (gap<0? "down" : "none");
            ctx.GapTicks = (int)Math.Round(Math.Abs(gap) / (tick > 0 ? tick : 0.25));

            if (!double.IsNaN(priorVah) && !double.IsNaN(priorVal))
                ctx.Location = (rthOpen>=priorVal && rthOpen<=priorVah) ? "inValue" : "outOfValue";

            double range = firstHi - firstLo;
            bool up = (firstClose-firstOpen)>0 && (firstClose-firstLo)>0.66*range;
            bool dn = (firstOpen-firstClose)>0 && (firstHi-firstClose)>0.66*range;
            ctx.Type = (up||dn) ? "OD" : "OA";
            return ctx;
        }

        public IbInfo UpdateIb(IbInfo ib, DateTime barEt, DateTime openEt, double h,double l,double close)
        {
            ib.SinceOpenMin = (int)(barEt - openEt).TotalMinutes;
            if (ib.SinceOpenMin < 60)
            {
                if (double.IsNaN(ib.High) || h > ib.High) ib.High = h;
                if (double.IsNaN(ib.Low)  || l < ib.Low)  ib.Low  = l;
            }
            else
            {
                if (!double.IsNaN(ib.High) && h > ib.High) ib.ExtUpCount++;
                if (!double.IsNaN(ib.Low)  && l < ib.Low)  ib.ExtDownCount++;
            }
            return ib;
        }

        public OtfState DetectOtf(List<Tuple<double,double>> s5)
        {
            var r = new OtfState(); r.Up=false; r.Down=false; r.Frame="5m";
            if (s5 == null || s5.Count < 2) return r;
            bool up = true, dn = true;
            for (int i=1;i<s5.Count;i++)
            {
                double ph = s5[i].Item1, pl = s5[i].Item2;
                double prevh = s5[i-1].Item1, prevl = s5[i-1].Item2;
                if (!(ph > prevh && pl > prevl)) up = false;
                if (!(ph < prevh && pl < prevl)) dn = false;
            }
            r.Up = up; r.Down = dn; return r;
        }

        public string ShapeFromProfile(ProfileSnapshot s)
        {
            if (s.TotalTpos == 0) return "Neutral";
            double mid = (s.Vah + s.Val) / 2.0;
            if (s.Poc >= mid) return "P";
            if (s.Poc <  mid) return "b";
            return "D";
        }

        public CallDecision ScoreCall(OpeningContext oc, IbInfo ib, ProfileSnapshot s, OtfState otf, Dictionary<string,bool> peers, string prev)
        {
            var c = new CallDecision();
            c.Bias = otf.Up ? "bullish" : (otf.Down ? "bearish" : "neutral");
            c.DayType = ShapeFromProfile(s);
            int sc = 50;
            if (oc.Type == "OD") sc += 10;
            if (ib.ExtUpCount>0 || ib.ExtDownCount>0) sc += 10;
            if (s.PoorHigh || s.PoorLow) sc -= 5;
            if (peers != null && peers.Count > 0)
            {
                int ok=0; foreach(var v in peers.Values) if (v) ok++;
                double coh = (double)ok / (double)peers.Count;
                if (coh > 0.66) sc += 5;
            }
            if (sc < 0) sc = 0; if (sc > 100) sc = 100;
            c.Confidence = sc;
            return c;
        }
    }
}
