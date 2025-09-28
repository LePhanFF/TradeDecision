
#region Using declarations
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.AddOns.Core;
using NinjaTrader.NinjaScript.AddOns.Ui;
#endregion
namespace NinjaTrader.NinjaScript.AddOns
{
    public class NinjaAddOnTPO : AddOnBase
    {
        private const string VersionTag = "v9.4.3";
        private NTMenuItem menuItem; private bool isRunning=false;
        private DataEngine engine; private PeersModel peers; private JsonStore store; private Journal journal; private UiHostWindow ui;
        private System.Windows.Threading.Dispatcher Disp => Application.Current?.Dispatcher;
        private readonly string[] instrumentNames = new[]{ "ES ##-##", "NQ ##-##", "YM ##-##" };
        private readonly string baseDir = System.IO.Path.Combine(Core.Globals.UserDataDir, "NinjaAddOn", "TPOAddon");
        protected override void OnWindowCreated(Window w){
            var cc = w as ControlCenter; if(cc==null) return;
            var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if(newMenu==null) return;
            if(menuItem==null){ menuItem=new NTMenuItem{ Header="NinjaAddOn TPO " + VersionTag, Style=Application.Current.TryFindResource("MainMenuItem") as Style }; menuItem.Click += (s,e)=>{ if(!isRunning) Start(); else Stop(); }; newMenu.Items.Add(menuItem); }
        }
        protected override void OnWindowDestroyed(Window w){
            var cc = w as ControlCenter; if(cc==null) return;
            if(menuItem!=null){ var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if(newMenu!=null && newMenu.Items.Contains(menuItem)) newMenu.Items.Remove(menuItem); menuItem=null; }
            if(isRunning) Stop();
        }
        private void Start(){
            try{
                System.IO.Directory.CreateDirectory(baseDir);
                store=new JsonStore(baseDir); journal=new Journal(baseDir); peers=new PeersModel();
                ui=new UiHostWindow(); ui.Show();
                engine=new DataEngine(); engine.BarComputed+=OnBar; engine.SessionClosed+=OnSessionClosed;

                var list=new List<Instrument>();
                foreach(var name in instrumentNames){ var inst=Instrument.GetInstrument(name); if(inst!=null){ list.Add(inst); } else SafeOut("[TPO] Missing instrument: "+name); }
                engine.Start(list);
                isRunning=true; UpdateHeader(); SafeOut("[TPO] Started "+VersionTag);
            } catch(Exception ex){ SafeOut("[TPO] Start exception: " + ex.Message); }
        }
        private void Stop(){
            try{
                isRunning=false; UpdateHeader();
                if(engine!=null){ engine.BarComputed-=OnBar; engine.SessionClosed-=OnSessionClosed; engine.Stop(); engine=null; }
                if(ui!=null){ ui.Dispatcher.BeginInvoke(new Action(()=>ui.Close())); ui=null; }
                SafeOut("[TPO] Stopped.");
            } catch(Exception ex){ SafeOut("[TPO] Stop exception: " + ex.Message); }
        }
        private void UpdateHeader(){ var h = isRunning? "Stop NinjaAddOn TPO " + VersionTag : "NinjaAddOn TPO " + VersionTag; var d=Disp; if(d!=null && !d.CheckAccess()) d.BeginInvoke(new Action(()=>{ if(menuItem!=null) menuItem.Header=h; })); else if(menuItem!=null) menuItem.Header=h; }

        private void OnBar(object sender, BarComputedEventArgs e){
            try{
                peers.ObserveBar(e.Symbol, e.H, e.L);
                var peersClosed = peers.SnapshotGapClosed();
                var clf = new Classifier(0.25);
                var call = clf.ScoreCall(e.Opening, e.IB, e.Profile, e.Otf, peersClosed, "");
                // journal
                journal.Append(e.Symbol, e.Et, $"{e.Et:HH:mm}  {e.Symbol}  Bias={call.Bias}  Day={call.DayType}  Conf={call.Confidence}  IB[{e.IB.High:0.00}/{e.IB.Low:0.00}]  dPOC={e.Profile.Poc:0.00}  VA[{e.Profile.Val:0.00}/{e.Profile.Poc:0.00}/{e.Profile.Vah:0.00}]  Singles:{(e.Profile.Singles!=null?e.Profile.Singles.Count:0)}  OTF:{(e.Otf.up? "Up": e.Otf.dn? "Down":"-")}({e.Otf.frame})  Morph:low");
                // json
                var json = new StringBuilder(2048);
                json.Append("{\"schemaVersion\":\"tpo.v9_4_3\",\"type\":\"bar\"");
                json.Append(",\"ts_utc\":\"").Append(e.Utc.ToString("yyyy-MM-ddTHH:mm:00Z")).Append("\"");
                json.Append(",\"ts_et\":\"").Append(e.Et.ToString("yyyy-MM-ddTHH:mm:00")).Append("\"");
                json.Append(",\"sessionId\":\"").Append(e.SessionId).Append("\"");
                json.Append(",\"instrument\":{\"symbol\":\"").Append(e.Symbol).Append("\",\"fullName\":\"").Append(e.FullName).Append("\"}");
                json.Append(",\"bar\":{\"o\":").Append(e.O.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"h\":").Append(e.H.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"l\":").Append(e.L.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"c\":").Append(e.C.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}");
                json.Append(",\"context\":{\"rthPhase\":\"RTH\",\"sinceOpenMin\":").Append(e.IB.SinceOpenMin).Append(",\"gap\":{\"dir\":\"").Append(e.Opening.Dir).Append("\",\"ticks\":").Append(e.Opening.GapTicks).Append("},\"opening\":{\"location\":\"").Append(e.Opening.Location).Append("\",\"type\":\"").Append(e.Opening.Type).Append("\"}}");
                json.Append(",\"ib\":{\"high\":").Append((double.IsNaN(e.IB.High)?0:e.IB.High).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"low\":").Append((double.IsNaN(e.IB.Low)?0:e.IB.Low).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"extensions\":{\"up\":").Append(e.IB.ExtUpCount).Append(",\"down\":").Append(e.IB.ExtDownCount).Append("}}");
                json.Append(",\"profile\":{\"tpo\":{\"poc\":").Append(e.Profile.Poc.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"vah\":").Append(e.Profile.Vah.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"val\":").Append(e.Profile.Val.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"totalTpos\":").Append(e.Profile.TotalTpos).Append("},");
                // HVN/LVN
                json.Append("\"hvn\":[");
                if(e.Profile.HVN!=null){ for(int i=0;i<e.Profile.HVN.Count;i++){ if(i>0) json.Append(','); json.Append("{\"price\":").Append(e.Profile.HVN[i].price.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"score\":").Append(e.Profile.HVN[i].score.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}"); } }
                json.Append("],\"lvn\":[");
                if(e.Profile.LVN!=null){ for(int i=0;i<e.Profile.LVN.Count;i++){ if(i>0) json.Append(','); json.Append("{\"price\":").Append(e.Profile.LVN[i].price.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"score\":").Append(e.Profile.LVN[i].score.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}"); } }
                json.Append("],\"singles\":[");
                if(e.Profile.Singles!=null){ for(int i=0;i<e.Profile.Singles.Count;i++){ if(i>0) json.Append(','); json.Append("{\"start\":").Append(e.Profile.Singles[i].start.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"end\":").Append(e.Profile.Singles[i].end.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}"); } }
                json.Append("],\"poor\":{\"high\":").Append(e.Profile.PoorHigh ? "true":"false").Append(",\"low\":").Append(e.Profile.PoorLow ? "true":"false").Append("}}");
                // developing + structure
                json.Append(",\"developing\":{\"dPoc\":").Append(e.Profile.Poc.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"dPocTrail\":[]}");
                json.Append(",\"structure\":{\"oneTimeframing\":{\"up\":").Append(e.Otf.up?"true":"false").Append(",\"down\":").Append(e.Otf.dn?"true":"false").Append(",\"frame\":\"").Append(e.Otf.frame).Append("\"},\"shape\":\"").Append(new Classifier(0.25).ShapeFromProfile(e.Profile)).Append("\",\"morph\":{\"risk\":\"low\",\"from\":\"\",\"to\":\"\",\"evidence\":[]}}");
                // peers + call
                var coh = peersClosed!=null && peersClosed.Count>0 ? (double)System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(peersClosed.Values, v=>v==true)) / peersClosed.Count : 0.0;
                json.Append(",\"peers\":{\"coherenceScore\":").Append(coh.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}");
                json.Append(",\"call\":{\"bias\":\"").Append(call.Bias).Append("\",\"dayType\":\"").Append(call.DayType).Append("\",\"confidence\":").Append(call.Confidence).Append(",\"reasons\":[],\"warnings\":[]}");
                json.Append("}");
                var js = json.ToString();
                store.AppendBar(e.Symbol, e.Et, js);
                store.WriteLatest(e.Symbol, js);

                // UI
                ui?.UpdateUi(e.Et, e.Symbol, call.Bias, call.DayType, call.Confidence, "low", new[]{ $"POC {e.Profile.Poc:0.00}, VA [{e.Profile.Val:0.00}/{e.Profile.Vah:0.00}]" }, new[]{ $"Opening {e.Opening.Type}, IB[{e.IB.High:0.00}/{e.IB.Low:0.00}]" }, e.DPocTrail);
            } catch(Exception ex){ SafeOut("[TPO] OnBar exception: " + ex.Message); }
        }

        private void OnSessionClosed(object sender, SessionClosedEventArgs e){
            try{
                peers.SetPriorRthClose(e.Symbol, e.RthClosePrice);
                var json = new StringBuilder(512);
                json.Append("{\"schemaVersion\":\"tpo.v9_4_3\",\"type\":\"session\"");
                json.Append(",\"sessionId\":\"").Append(e.EtSessionDate.ToString("yyyy-MM-dd")).Append("-RTH\"");
                json.Append(",\"instrument\":{\"symbol\":\"").Append(e.Symbol).Append("\"}");
                json.Append(",\"ranges\":{\"high\":").Append(e.RthHigh.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"low\":").Append(e.RthLow.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}");
                json.Append(",\"tpo\":{\"poc\":").Append(e.Profile.Poc.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"vah\":").Append(e.Profile.Vah.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",\"val\":").Append(e.Profile.Val.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("}");
                json.Append("}");
                store.WriteSession(e.Symbol, e.EtSessionDate, json.ToString());
            } catch(Exception ex){ SafeOut("[TPO] OnSessionClosed exception: " + ex.Message); }
        }

        private void SafeOut(string text){
            var d=Disp;
            if(d!=null && !d.CheckAccess()) d.BeginInvoke(new Action(()=> NinjaTrader.Code.Output.Process(text, PrintTo.OutputTab1)));
            else NinjaTrader.Code.Output.Process(text, PrintTo.OutputTab1);
        }
    }
}
