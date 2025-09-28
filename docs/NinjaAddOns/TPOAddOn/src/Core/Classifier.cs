
#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class OpeningContext { public string Location="inRange"; public string Type="OA"; public string Dir="none"; public int GapTicks=0; }
    internal sealed class IbInfo { public double High=double.NaN, Low=double.NaN; public int ExtUpCount=0, ExtDownCount=0, SinceOpenMin=0; }
    internal sealed class ProfileSnapshot { public double Poc,Vah,Val,SessionHigh,SessionLow; public int TotalTpos; public System.Collections.Generic.List<(double price,double score)> HVN=new(); public System.Collections.Generic.List<(double price,double score)> LVN=new(); public System.Collections.Generic.List<(double start,double end)> Singles=new(); public bool PoorHigh, PoorLow; }
    internal sealed class CallDecision { public string Bias="neutral", DayType="Neutral"; public int Confidence=50; public List<string> Reasons=new(); public List<string> Warnings=new(); public List<string> DegradedBy=new(); }
    internal sealed class Classifier
    {
        private readonly double tick;
        public Classifier(double tickSize){ tick=tickSize; }
        public OpeningContext ClassifyOpening(double rthOpen,double priorVah,double priorVal,double priorHi,double priorLo,double prevClose,double firstOpen,double firstClose,double firstHi,double firstLo){
            var ctx=new OpeningContext(); var gap=firstOpen-prevClose; ctx.Dir = gap>0?"up":(gap<0?"down":"none"); ctx.GapTicks=(int)Math.Round(Math.Abs(gap)/Math.Max(0.01,tick));
            if (!double.IsNaN(priorVah) && !double.IsNaN(priorVal)){
                ctx.Location = (rthOpen>=priorVal && rthOpen<=priorVah) ? "inValue" : "outOfValue";
            }
            // OD heuristic
            double range=firstHi-firstLo;
            bool up=(firstClose-firstOpen)>0 && (firstClose-firstLo)>0.66*range;
            bool dn=(firstOpen-firstClose)>0 && (firstHi-firstClose)>0.66*range;
            ctx.Type = (up||dn)?"OD":"OA";
            return ctx;
        }
        public IbInfo UpdateIb(IbInfo ib, DateTime barEt, DateTime openEt, double h,double l,double close){
            ib.SinceOpenMin=(int)(barEt-openEt).TotalMinutes;
            if(ib.SinceOpenMin<60){ if(double.IsNaN(ib.High)||h>ib.High) ib.High=h; if(double.IsNaN(ib.Low)||l<ib.Low) ib.Low=l; }
            else { if(!double.IsNaN(ib.High)&&h>ib.High) ib.ExtUpCount++; if(!double.IsNaN(ib.Low)&&l<ib.Low) ib.ExtDownCount++; }
            return ib;
        }
        public (bool up,bool dn,string frame) DetectOtf(System.Collections.Generic.List<(double h,double l)> s5){
            bool up5=false,dn5=false; if(s5.Count>=2){ up5=s5.Zip(s5.Skip(1),(a,b)=>b.h>a.h && b.l>a.l).All(x=>x); dn5=s5.Zip(s5.Skip(1),(a,b)=>b.h<a.h && b.l<a.l).All(x=>x); }
            return (up5,dn5,"5m");
        }
        public string ShapeFromProfile(ProfileSnapshot s){ if(s.TotalTpos==0) return "Neutral"; double mid=(s.Vah+s.Val)/2.0; if(s.Poc>=mid) return "P"; if(s.Poc<mid) return "b"; return "D"; }
        public CallDecision ScoreCall(OpeningContext oc, IbInfo ib, ProfileSnapshot s, (bool up,bool dn,string frame) otf, System.Collections.Generic.Dictionary<string,bool> peers, string prev){
            var c=new CallDecision(); c.Bias= otf.up? "bullish" : otf.dn? "bearish":"neutral"; c.DayType=ShapeFromProfile(s); int sc=50; if(oc.Type=="OD") sc+=10; if(ib.ExtUpCount>0||ib.ExtDownCount>0) sc+=10; if(s.PoorHigh||s.PoorLow) sc-=5; c.Confidence=Math.Max(0,Math.Min(100,sc)); return c;
        }
    }
}
