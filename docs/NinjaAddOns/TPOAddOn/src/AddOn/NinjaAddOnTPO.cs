// NinjaAddOnTPO.cs - clean Start()/Stop() and stable OnBar JSON builder
using System;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.AddOns.Core;
using NinjaTrader.NinjaScript.AddOns.Ui;
using NinjaTrader.Core;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class NinjaAddOnTPO : AddOnBase
    {
        private const string VersionTag = "v9.4.4";
        private NTMenuItem menuItem;
        private bool isRunning = false;

        private DataEngine engine;
        private PeersModel peers;
        private JsonStore store;
        private Journal journal;
        private UiHostWindow ui;

        private System.Windows.Threading.Dispatcher Disp
        {
            get { return Application.Current != null ? Application.Current.Dispatcher : null; }
        }

        private readonly string[] instrumentNames = new string[] { "ES ##-##", "NQ ##-##", "YM ##-##" };
        private readonly string baseDir = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "NinjaAddOn", "TPOAddon");

        protected override void OnWindowCreated(Window w)
        {
            var cc = w as ControlCenter;
            if (cc == null) return;
            var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
            if (newMenu == null) return;

            if (menuItem == null)
            {
                menuItem = new NTMenuItem
                {
                    Header = "NinjaAddOn TPO " + VersionTag,
                    Style = Application.Current.TryFindResource("MainMenuItem") as Style
                };
                menuItem.Click += (s, e) =>
                {
                    if (!isRunning) Start();
                    else Stop();
                };
                newMenu.Items.Add(menuItem);
            }
        }

        protected override void OnWindowDestroyed(Window w)
        {
            var cc = w as ControlCenter;
            if (cc == null) return;

            if (menuItem != null)
            {
                var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
                if (newMenu != null && newMenu.Items.Contains(menuItem))
                    newMenu.Items.Remove(menuItem);
                menuItem = null;
            }

            if (isRunning) Stop();
        }

        private void Start()
        {
            try
            {
                System.IO.Directory.CreateDirectory(baseDir);
                store = new JsonStore(baseDir);
                journal = new Journal(baseDir);
                peers = new PeersModel();

                // Create UI window on WPF UI thread
                var disp = Application.Current != null ? Application.Current.Dispatcher : null;
                if (disp != null)
                {
                    disp.Invoke(new Action(() =>
                    {
                        ui = new UiHostWindow();
                        ui.Show();
                    }));
                }
                else
                {
                    ui = new UiHostWindow();
                    ui.Show();
                }

                engine = new DataEngine();
                engine.BarComputed += OnBar;

                var list = new List<Instrument>();
                for (int i = 0; i < instrumentNames.Length; i++)
                {
                    var inst = Instrument.GetInstrument(instrumentNames[i]);
                    if (inst != null) list.Add(inst);
                    else SafeOut("[TPO] Missing instrument: " + instrumentNames[i]);
                }
                engine.Start(list);

                isRunning = true;
                UpdateHeader();
                SafeOut("[TPO] Started " + VersionTag);
            }
            catch (Exception ex)
            {
                SafeOut("[TPO] Start exception: " + ex.Message);
            }
        }

        private void Stop()
        {
            try
            {
                isRunning = false;
                UpdateHeader();

                if (engine != null)
                {
                    engine.BarComputed -= OnBar;
                    engine.Stop();
                    engine = null;
                }

                if (ui != null)
                {
                    // Close on the window's dispatcher
                    ui.Dispatcher.BeginInvoke(new Action(() => { try { ui.Close(); } catch { } }));
                    ui = null;
                }

                SafeOut("[TPO] Stopped.");
            }
            catch (Exception ex)
            {
                SafeOut("[TPO] Stop exception: " + ex.Message);
            }
        }

        private void UpdateHeader()
        {
            string h = isRunning ? "Stop NinjaAddOn TPO " + VersionTag : "NinjaAddOn TPO " + VersionTag;
            var d = Disp;
            if (d != null && !d.CheckAccess())
                d.BeginInvoke(new Action(() => { if (menuItem != null) menuItem.Header = h; }));
            else if (menuItem != null) menuItem.Header = h;
        }

        private void OnBar(object sender, BarComputedEventArgs e)
        {
            try
            {
                // Cross-index peer tracking
                peers.ObserveBar(e.Symbol, e.H, e.L);
                Dictionary<string, bool> peersClosed = peers.SnapshotGapClosed();

                // Score
                Classifier clf = new Classifier(0.25);
                CallDecision call = clf.ScoreCall(e.Opening, e.IB, e.Profile, e.Otf, peersClosed, "");

                // Journal (safe)
                journal.Append(
                    e.Symbol,
                    e.Et,
                    string.Format(
                        "{0} {1} Bias={2} Day={3} Conf={4} IB[{5:0.00}/{6:0.00}] dPOC={7:0.00} VA[{8:0.00}/{9:0.00}/{10:0.00}] Singles:{11} OTF:{12}({13}) Morph:low",
                        e.Et.ToString("HH:mm"),
                        e.Symbol,
                        call.Bias,
                        call.DayType,
                        call.Confidence,
                        e.IB.High,
                        e.IB.Low,
                        e.Profile.Poc,
                        e.Profile.Val,  // VAL
                        e.Profile.Poc,  // POC in middle for display consistency
                        e.Profile.Vah,  // VAH
                        (e.Profile.Singles != null ? e.Profile.Singles.Count : 0),
                        (e.Otf.Up ? "Up" : (e.Otf.Down ? "Down" : "-")),
                        e.Otf.Frame
                    )
                );

                var inv = System.Globalization.CultureInfo.InvariantCulture;
                var json = new StringBuilder(4096);

                json.Append("{\"schemaVersion\":\"tpo.v9_4_4\",\"type\":\"bar\"");
                json.Append(",\"ts_utc\":\"").Append(e.Utc.ToString("yyyy-MM-ddTHH:mm:00Z")).Append("\"");
                json.Append(",\"ts_et\":\"").Append(e.Et.ToString("yyyy-MM-ddTHH:mm:00")).Append("\"");
                json.Append(",\"sessionId\":\"").Append(e.SessionId).Append("\"");
                json.Append(",\"instrument\":{\"symbol\":\"").Append(e.Symbol).Append("\",\"fullName\":\"").Append(e.FullName).Append("\"}");
                json.Append(",\"bar\":{\"o\":").Append(e.O.ToString(inv)).Append(",\"h\":").Append(e.H.ToString(inv)).Append(",\"l\":").Append(e.L.ToString(inv)).Append(",\"c\":").Append(e.C.ToString(inv)).Append("}");
                json.Append(",\"context\":{\"rthPhase\":\"RTH\",\"sinceOpenMin\":").Append(e.IB.SinceOpenMin)
                    .Append(",\"gap\":{\"dir\":\"").Append(e.Opening.Dir).Append("\",\"ticks\":").Append(e.Opening.GapTicks).Append("}")
                    .Append(",\"opening\":{\"location\":\"").Append(e.Opening.Location).Append("\",\"type\":\"").Append(e.Opening.Type).Append("\"}}");

                // IB
                json.Append(",\"ib\":{\"high\":").Append((double.IsNaN(e.IB.High) ? 0 : e.IB.High).ToString(inv))
                    .Append(",\"low\":").Append((double.IsNaN(e.IB.Low) ? 0 : e.IB.Low).ToString(inv))
                    .Append(",\"extensions\":{\"up\":").Append(e.IB.ExtUpCount).Append(",\"down\":").Append(e.IB.ExtDownCount).Append("}}");

                // Profile (TPO / HVN / LVN / Singles / Poor)
                json.Append(",\"profile\":{\"tpo\":{\"poc\":").Append(e.Profile.Poc.ToString(inv))
                    .Append(",\"vah\":").Append(e.Profile.Vah.ToString(inv))
                    .Append(",\"val\":").Append(e.Profile.Val.ToString(inv))
                    .Append(",\"totalTpos\":").Append(e.Profile.TotalTpos).Append("}");

                // HVN
                json.Append(",\"hvn\":[");
                if (e.Profile.HVN != null)
                {
                    for (int i = 0; i < e.Profile.HVN.Count; i++)
                    {
                        if (i > 0) json.Append(',');
                        json.Append("{\"price\":").Append(e.Profile.HVN[i].Price.ToString(inv))
                            .Append(",\"score\":").Append(e.Profile.HVN[i].Score.ToString(inv)).Append("}");
                    }
                }
                json.Append("]");

                // LVN
                json.Append(",\"lvn\":[");
                if (e.Profile.LVN != null)
                {
                    for (int i = 0; i < e.Profile.LVN.Count; i++)
                    {
                        if (i > 0) json.Append(',');
                        json.Append("{\"price\":").Append(e.Profile.LVN[i].Price.ToString(inv))
                            .Append(",\"score\":").Append(e.Profile.LVN[i].Score.ToString(inv)).Append("}");
                    }
                }
                json.Append("]");

                // Singles
                json.Append(",\"singles\":[");
                if (e.Profile.Singles != null)
                {
                    for (int i = 0; i < e.Profile.Singles.Count; i++)
                    {
                        if (i > 0) json.Append(',');
                        json.Append("{\"start\":").Append(e.Profile.Singles[i].Start.ToString(inv))
                            .Append(",\"end\":").Append(e.Profile.Singles[i].End.ToString(inv)).Append("}");
                    }
                }
                json.Append("]");

                // Poor
                json.Append(",\"poor\":{\"high\":")
                    .Append(e.Profile.PoorHigh ? "true" : "false")
                    .Append(",\"low\":")
                    .Append(e.Profile.PoorLow ? "true" : "false")
                    .Append("}"); // end poor

                json.Append("}"); // end profile

                // Developing (dPOC + trail placeholder)
                json.Append(",\"developing\":{\"dPoc\":").Append(e.Profile.Poc.ToString(inv)).Append(",\"dPocTrail\":[]}");

                // Structure
                json.Append(",\"structure\":{\"oneTimeframing\":{\"up\":")
                    .Append(e.Otf.Up ? "true" : "false")
                    .Append(",\"down\":")
                    .Append(e.Otf.Down ? "true" : "false")
                    .Append(",\"frame\":\"").Append(e.Otf.Frame).Append("\"}")
                    .Append(",\"shape\":\"").Append(new Classifier(0.25).ShapeFromProfile(e.Profile)).Append("\"")
                    .Append(",\"morph\":{\"risk\":\"low\",\"from\":\"\",\"to\":\"\",\"evidence\":[]}}");

                // Peers coherence
                double coh = 0.0;
                if (peersClosed != null && peersClosed.Count > 0)
                {
                    int ok = 0;
                    foreach (var v in peersClosed.Values) if (v) ok++;
                    coh = (double)ok / (double)peersClosed.Count;
                }
                json.Append(",\"peers\":{\"coherenceScore\":").Append(coh.ToString(inv)).Append("}");

                // Call decision
                json.Append(",\"call\":{\"bias\":\"").Append(call.Bias)
                    .Append("\",\"dayType\":\"").Append(call.DayType)
                    .Append("\",\"confidence\":").Append(call.Confidence).Append(",\"reasons\":[],\"warnings\":[]}");

                json.Append("}"); // end root

                string js = json.ToString();
                store.AppendBar(e.Symbol, e.Et, js);
                store.WriteLatest(e.Symbol, js);

                if (ui != null)
                {
                    ui.UpdateUi(
                        e.Et,
                        e.Symbol,
                        call.Bias,
                        call.DayType,
                        call.Confidence,
                        "low",
                        new string[] { "POC " + e.Profile.Poc.ToString("0.00") + ", VA [" + e.Profile.Val.ToString("0.00") + "/" + e.Profile.Vah.ToString("0.00") + "]" },
                        new string[] { "Opening " + e.Opening.Type + ", IB[" + e.IB.High.ToString("0.00") + "/" + e.IB.Low.ToString("0.00") + "]" },
                        e.DPocTrail
                    );
                }
            }
            catch (Exception ex)
            {
                SafeOut("[TPO] OnBar exception: " + ex.Message);
            }
        }

        private void SafeOut(string s)
        {
            var d = Disp;
            if (d != null && !d.CheckAccess())
                d.BeginInvoke(new Action(() => { NinjaTrader.Code.Output.Process(s, PrintTo.OutputTab1); }));
            else NinjaTrader.Code.Output.Process(s, PrintTo.OutputTab1);
        }
    }
}
