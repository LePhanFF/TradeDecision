
using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;

using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    public sealed class DpocPoint { public DateTime Et; public double Price; public DpocPoint(DateTime et,double p){ Et=et; Price=p; } }

    internal sealed class BarComputedEventArgs : EventArgs
    {
        public string Symbol; public string FullName; public string SessionId;
        public DateTime Utc; public DateTime Et; public double O; public double H; public double L; public double C;
        public ProfileSnapshot Profile; public OpeningContext Opening; public IbInfo IB;
        public OtfState Otf;
        public Dictionary<string,bool> PeersGapClosed;
        public CallDecision Call; public List<DpocPoint> DPocTrail;
    }

    internal sealed class SessionState
    {
        public Instrument Inst; public string Symbol; public string FullName; public double Tick=0.25;
        public DateTime RthOpenEt; public DateTime RthCloseEt; public DateTime SessionDateEt;
        public double PriorRthClose = double.NaN, PriorVah=double.NaN, PriorVal=double.NaN, PriorHi=double.NaN, PriorLo=double.NaN;
        public ProfileBuilder Prof; public Classifier Clf; public IbInfo IB = new IbInfo();
        public List<Tuple<double,double>> S5 = new List<Tuple<double,double>>();
        public List<DpocPoint> Trail = new List<DpocPoint>();
        public DateTime NextTrailEt;
        public double LastClose = double.NaN;
    }

    internal sealed class DataEngine
    {
        private readonly object stateLock = new object();
        private readonly Dictionary<string,BarsRequest> req = new Dictionary<string,BarsRequest>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,int> lastIdx = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,SessionState> sessions = new Dictionary<string,SessionState>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<BarComputedEventArgs> BarComputed;

        private static DateTime RthOpen(DateTime etDate){ return etDate.Date.AddHours(9).AddMinutes(30); }
        private static DateTime RthClose(DateTime etDate){ return etDate.Date.AddHours(16); }

        public void Start(IEnumerable<Instrument> instruments)
        {
            lock(stateLock)
            {
                foreach(var inst in instruments)
                {
                    if (req.ContainsKey(inst.FullName)) continue;
                    var br = new BarsRequest(inst, 300);
                    br.BarsPeriod = new BarsPeriod{ BarsPeriodType = BarsPeriodType.Minute, Value = 5 };
                    br.TradingHours = TradingHours.Get("CME US Index Futures ETH") ?? TradingHours.Get("Default 24 x 5");
                    br.Update += OnBarUpdate;
                    br.Request(delegate(BarsRequest bars, ErrorCode ec, string em)
                    {
                        if (ec == ErrorCode.NoError)
                            lastIdx[inst.FullName] = Math.Max(-1, bars.Bars.Count - 2);
                    });
                    req[inst.FullName] = br;
                    var st = new SessionState{ Inst=inst, Symbol=inst.MasterInstrument.Name, FullName=inst.FullName, Tick=inst.MasterInstrument.TickSize };
                    sessions[inst.FullName] = st;
                }
            }
        }

        public void Stop()
        {
            lock(stateLock)
            {
                foreach(var kv in req)
                {
                    try { 
						    kv.Value.Update -= OnBarUpdate;
    						kv.Value.Dispose();
					} catch {}
                }
                req.Clear(); lastIdx.Clear(); sessions.Clear();
            }
        }

        private void EnsureSession(SessionState st, DateTime etOpen)
        {
            DateTime rOpen = RthOpen(etOpen);
            DateTime rClose = RthClose(etOpen);
            if (st.Prof == null || st.SessionDateEt != rOpen.Date)
            {
                st.SessionDateEt = rOpen.Date; st.RthOpenEt = rOpen; st.RthCloseEt = rClose;
                st.Prof = new ProfileBuilder(st.Tick); st.Prof.Reset(); st.Clf = new Classifier(st.Tick);
                st.IB = new IbInfo(); st.S5.Clear(); st.Trail.Clear(); st.NextTrailEt = rOpen.AddMinutes(15);
            }
        }

        private void OnBarUpdate(object sender, BarsUpdateEventArgs e)
        {
            var series = e.BarsSeries; if (series == null) return;
            var inst = series.Instrument; if (inst == null) return;

            SessionState st; int last = -1;
            lock(stateLock)
            {
                if (!sessions.TryGetValue(inst.FullName, out st)) return;
                if (!lastIdx.TryGetValue(inst.FullName, out last)) last = -1;
            }

            int count = series.Count;
            if (count < 2) return;
            int latestClosed = count - 2;
            int start = Math.Max(e.MinIndex, last + 1);
            int end = Math.Min(e.MaxIndex, latestClosed);
            if (start > end) return;

            for (int i = start; i <= end; i++)
            {
                DateTime openUtc = series.GetTime(i);
                DateTime openEt = NinjaTrader.NinjaScript.AddOns.Core.ClockEt.ToEt(openUtc);

                EnsureSession(st, openEt);
                if (openEt < st.RthOpenEt || openEt >= st.RthCloseEt)
                {
                    st.LastClose = series.GetClose(i);
                    continue;
                }

                double o = series.GetOpen(i), h = series.GetHigh(i), l = series.GetLow(i), c = series.GetClose(i);
                st.Prof.AddBar(o,h,l,c); st.LastClose = c;
                st.IB = st.Clf.UpdateIb(st.IB, openEt, st.RthOpenEt, h,l,c);
                st.S5.Add(new Tuple<double,double>(h,l));
                var snap = st.Prof.Compute();
                if (openEt >= st.NextTrailEt) { st.Trail.Add(new DpocPoint(openEt, snap.Poc)); st.NextTrailEt = st.NextTrailEt.AddMinutes(15); }
                var opening = st.Clf.ClassifyOpening(o, st.PriorVah, st.PriorVal, st.PriorHi, st.PriorLo, double.IsNaN(st.PriorRthClose)?o:st.PriorRthClose, o,c,h,l);
                var otf = st.Clf.DetectOtf(st.S5);

                var args = new BarComputedEventArgs();
                args.Symbol = st.Symbol; args.FullName = st.FullName; args.SessionId = openEt.ToString("yyyy-MM-dd") + "-RTH";
                args.Utc = openUtc; args.Et = openEt; args.O=o; args.H=h; args.L=l; args.C=c;
                args.Profile = snap; args.Opening = opening; args.IB = st.IB; args.Otf = otf; args.PeersGapClosed = null; args.Call = new CallDecision(); args.DPocTrail = new List<DpocPoint>(st.Trail);

                var handler = BarComputed; if (handler != null) handler(this, args);
            }
            lock(stateLock){ lastIdx[inst.FullName] = end; }
        }
    }
}
