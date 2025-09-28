
#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal sealed class BarComputedEventArgs : EventArgs {
        public string Symbol, FullName, SessionId;
        public DateTime Utc, Et; public double O,H,L,C;
        public ProfileSnapshot Profile; public OpeningContext Opening; public IbInfo IB;
        public (bool up,bool dn,string frame) Otf; public (string risk,string from,string to, System.Collections.Generic.List<string> evidence) Morph;
        public System.Collections.Generic.Dictionary<string,bool> PeersGapClosed;
        public CallDecision Call; public System.Collections.Generic.List<(DateTime et,double price)> DPocTrail;
    }
    internal sealed class SessionClosedEventArgs : EventArgs { public string Symbol, FullName; public DateTime EtSessionDate; public ProfileSnapshot Profile; public IbInfo IB; public CallDecision Call; public System.Collections.Generic.Dictionary<string,bool> PeersGapClosed; public double RthClosePrice,RthHigh,RthLow; }
    internal sealed class SessionState {
        public Instrument Inst; public string Symbol, FullName; public double Tick=0.25; public DateTime RthOpenEt, RthCloseEt, SessionDateEt; public double PriorRthClose=double.NaN, PriorVah=double.NaN, PriorVal=double.NaN, PriorHi=double.NaN, PriorLo=double.NaN;
        public ProfileBuilder Prof; public Classifier Clf; public IbInfo IB=new IbInfo(); public System.Collections.Generic.List<(DateTime et,double price)> DPocTrail=new(); public DateTime NextTrailEt; public double LastClose=double.NaN;
        public System.Collections.Generic.List<(double h,double l)> S5=new();
    }
    internal sealed class DataEngine
    {
        private readonly object stateLock=new object();
        private readonly System.Collections.Generic.Dictionary<string,BarsRequest> req=new(System.StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.Dictionary<string,int> lastIdx=new(System.StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.Dictionary<string,SessionState> sessions=new(System.StringComparer.OrdinalIgnoreCase);

        public event EventHandler<BarComputedEventArgs> BarComputed;
        public event EventHandler<SessionClosedEventArgs> SessionClosed;

        private static DateTime RthOpen(DateTime etDate)=> etDate.Date.AddHours(9).AddMinutes(30);
        private static DateTime RthClose(DateTime etDate)=> etDate.Date.AddHours(16);

        public void Start(System.Collections.Generic.IEnumerable<Instrument> instruments){
            lock(stateLock){
                foreach(var inst in instruments){
                    if(req.ContainsKey(inst.FullName)) continue;
                    var br=new BarsRequest(inst, 300){ BarsPeriod=new BarsPeriod{ BarsPeriodType=BarsPeriodType.Minute, Value=5}, TradingHours=TradingHours.Get("CME US Index Futures ETH") ?? TradingHours.Get("Default 24 x 5")};
                    br.Update += OnBarsUpdate;
                    br.Request((bars,ec,em)=>{ if(ec==ErrorCode.NoError) lastIdx[inst.FullName]=System.Math.Max(-1, bars.Bars.Count-1); });
                    req[inst.FullName]=br;
                    var st=new SessionState{ Inst=inst, Symbol=inst.MasterInstrument.Name, FullName=inst.FullName, Tick=inst.MasterInstrument.TickSize };
                    sessions[inst.FullName]=st;
                }
            }
        }
        public void Stop(){ lock(stateLock){ foreach(var kv in req){ try{ kv.Value.Update-=OnBarsUpdate; kv.Value.Cancel(); kv.Value.Dispose(); } catch{} } req.Clear(); lastIdx.Clear(); sessions.Clear(); } }

        private void EnsureSession(SessionState st, DateTime etOpen){
            var rOpen=RthOpen(etOpen), rClose=RthClose(etOpen);
            if(st.Prof==null || st.SessionDateEt!=rOpen.Date){
                st.SessionDateEt=rOpen.Date; st.RthOpenEt=rOpen; st.RthCloseEt=rClose;
                st.Prof=new ProfileBuilder(st.Tick, st.RthOpenEt); st.Clf=new Classifier(st.Tick); st.IB=new IbInfo(); st.DPocTrail.Clear(); st.NextTrailEt=rOpen.AddMinutes(15); st.S5.Clear();
            }
        }

        private void OnBarsUpdate(object sender, BarsUpdateEventArgs e){
            var series=e.BarsSeries; if(series==null) return; var inst=series.Instrument; if(inst==null) return;
            SessionState st; int last=-1; lock(stateLock){ if(!sessions.TryGetValue(inst.FullName, out st)) return; if(!lastIdx.TryGetValue(inst.FullName, out last)) last=-1; }
            int count=series.Count; if(count<2) return; int latestClosed=count-2; int start=System.Math.Max(e.MinIndex, last+1), end=System.Math.Min(e.MaxIndex, latestClosed); if(start> end) return;

            for(int i=start;i<=end;i++){
                var openUtc=series.GetTime(i); var openEt=ClockEt.ToEt(openUtc);
                EnsureSession(st, openEt);
                if(openEt<st.RthOpenEt || openEt>=st.RthCloseEt){ st.LastClose=series.GetClose(i); continue; }

                double o=series.GetOpen(i), h=series.GetHigh(i), l=series.GetLow(i), c=series.GetClose(i);
                st.Prof.AddBar(o,h,l,c, openEt); st.LastClose=c; st.IB=st.Clf.UpdateIb(st.IB, openEt, st.RthOpenEt, h,l,c); st.S5.Add((h,l));
                var snap=st.Prof.Compute();
                if(openEt>=st.NextTrailEt){ st.DPocTrail.Add((openEt, snap.Poc)); st.NextTrailEt=st.NextTrailEt.AddMinutes(15); }
                var opening=st.Clf.ClassifyOpening(o, st.PriorVah, st.PriorVal, st.PriorHi, st.PriorLo, double.IsNaN(st.PriorRthClose)?o:st.PriorRthClose, o, c, h, l);
                var otf=st.Clf.DetectOtf(st.S5); var morph=("low","", "", new System.Collections.Generic.List<string>());

                BarComputed?.Invoke(this, new BarComputedEventArgs{
                    Symbol=st.Symbol, FullName=st.FullName, SessionId=openEt.ToString("yyyy-MM-dd") + "-RTH",
                    Utc=openUtc, Et=openEt, O=o,H=h,L=l,C=c, Profile=snap, Opening=opening, IB=st.IB, Otf=otf, Morph=morph, PeersGapClosed=null, Call=new CallDecision(), DPocTrail=new System.Collections.Generic.List<(DateTime,double)>(st.DPocTrail)
                });
            }
            lock(stateLock){ lastIdx[inst.FullName]=end; }
        }
    }
}
