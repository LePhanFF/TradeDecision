using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.AddOns.Core;
using NinjaTrader.NinjaScript.AddOns.Ui;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class NinjaAddOnTPO : AddOnBase
    {
        private const string VersionTag = "v2.4-CS5";
        private NTMenuItem menuItem;
        private bool isRunning = false;

        private DataEngine engine;
        private PeersModel peers;
        private JsonStore store;
        private Journal journal;
        private UiHostWindow ui;
        private Heartbeat hb;

        private string BaseDir { get { return System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "NinjaAddOn", "TPOAddon"); } }
        private string InstrumentsCfg { get { return System.IO.Path.Combine(BaseDir, "instruments.txt"); } }

        protected override void OnWindowCreated(Window w)
        {
            ControlCenter cc = w as ControlCenter; if (cc == null) return;
            NTMenuItem newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if (newMenu == null) return;

            if (menuItem == null)
            {
                menuItem = new NTMenuItem();
                menuItem.Header = "NinjaAddOn TPO " + VersionTag;
                menuItem.Style = Application.Current != null ? Application.Current.TryFindResource("MainMenuItem") as Style : null;
                menuItem.Click += delegate(object s, RoutedEventArgs e) { if (!isRunning) Start(); else Stop(); };
                newMenu.Items.Add(menuItem);
            }
        }

        protected override void OnWindowDestroyed(Window w)
        {
            ControlCenter cc = w as ControlCenter; if (cc == null) return;
            if (menuItem != null)
            {
                NTMenuItem newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
                if (newMenu != null && newMenu.Items.Contains(menuItem)) newMenu.Items.Remove(menuItem);
                menuItem = null;
            }
            if (isRunning) Stop();
        }

        private void Start()
        {
            try
            {
                Directory.CreateDirectory(BaseDir);
                TpoLogger.Init(BaseDir);

                store   = new JsonStore(BaseDir);
                journal = new Journal(BaseDir);
                peers   = new PeersModel();

                UiThread.Invoke(delegate()
                {
                    ui = new UiHostWindow();
                    ui.Show();
                });

                hb = new Heartbeat(TimeSpan.FromSeconds(30), delegate() { TpoLogger.Info("hb"); });
                hb.Start();

                engine = new DataEngine();
                engine.BarComputed += OnBar;

                List<Instrument> instruments = ResolveInstruments();
                if (instruments.Count == 0) TpoLogger.Warn("No instruments resolved. Provide 'instruments.txt'.");
                else
                {
                    engine.Start(instruments);
                    try
                    {
                        List<string> names = new List<string>();
                        for (int i = 0; i < instruments.Count; i++) names.Add(instruments[i].FullName);
                        TpoLogger.Info("Engine started for: " + string.Join(", ", names.ToArray()));
                    }
                    catch { }
                }

                isRunning = true;
                UpdateHeader();
                TpoLogger.Info("AddOn Started " + VersionTag);
            }
            catch (Exception ex)
            {
                TpoLogger.Error("Start exception: " + ex.Message);
            }
        }

        private void Stop()
        {
            try
            {
                isRunning = false;
                UpdateHeader();

                if (engine != null) { engine.BarComputed -= OnBar; engine.Stop(); engine = null; }

                if (hb != null) { hb.Dispose(); hb = null; }

                if (ui != null)
                {
                    UiThread.BeginInvoke(delegate() { try { ui.Close(); } catch { } });
                    ui = null;
                }

                TpoLogger.Info("AddOn Stopped.");
            }
            catch (Exception ex)
            {
                TpoLogger.Error("Stop exception: " + ex.Message);
            }
        }

        private void UpdateHeader()
        {
            string h = isRunning ? "Stop NinjaAddOn TPO " + VersionTag : "NinjaAddOn TPO " + VersionTag;
            NTMenuItem mi = menuItem; if (mi == null) return;
            UiThread.BeginInvoke(delegate() { try { if (menuItem != null) menuItem.Header = h; } catch { } });
        }

        private List<Instrument> ResolveInstruments()
        {
            List<Instrument> list = new List<Instrument>();
            string[] names = ReadInstrumentNames();
            for (int i = 0; i < names.Length; i++)
            {
                string raw = names[i];
                try
                {
                    string sym = DataSubscriptions.ResolveFrontMonth(raw, DateTime.UtcNow);
                    Instrument inst = Instrument.GetInstrument(sym);
                    if (inst != null) list.Add(inst); else TpoLogger.Warn("Instrument not found: " + sym);
                }
                catch (Exception ex)
                {
                    TpoLogger.Warn("Resolve instrument failed for " + raw + ": " + ex.Message);
                }
            }
            return list;
        }

        private string[] ReadInstrumentNames()
        {
            string[] defaults = new string[] { "NQ 12-25", "ES 12-25", "YM 12-25" };
            try
            {
                if (File.Exists(InstrumentsCfg))
                {
                    string[] raw = File.ReadAllLines(InstrumentsCfg);
                    List<string> cleaned = new List<string>();
                    for (int i = 0; i < raw.Length; i++)
                    {
                        string s = (raw[i] ?? string.Empty).Trim();
                        if (s.Length == 0) continue;
                        if (s.StartsWith("#")) continue;
                        if (!cleaned.Contains(s)) cleaned.Add(s);
                    }
                    if (cleaned.Count > 0) return cleaned.ToArray();
                }
            }
            catch (Exception ex)
            {
                TpoLogger.Warn("Could not read instruments.txt: " + ex.Message);
            }
            return defaults;
        }

        private void OnBar(object sender, BarComputedEventArgs e)
        {
            try
            {
                peers.ObserveBar(e.Symbol, e.H, e.L);
                Dictionary<string,bool> peersClosed = peers.SnapshotGapClosed();

                Classifier clf = new Classifier(0.25);
                CallDecision call = clf.ScoreCall(e.Opening, e.IB, e.Profile, e.Otf, peersClosed, "");

                journal.Append(
                    e.Symbol, e.Et,
                    string.Format(
                        "{0} {1} Bias={2} Day={3} Conf={4} IB[{5:0.00}/{6:0.00}] dPOC={7:0.00} VA[{8:0.00}/{9:0.00}/{10:0.00}] Singles:{11} OTF:{12}({13})",
                        e.Et.ToString("HH:mm"),
                        e.Symbol,
                        call.Bias,
                        call.DayType,
                        call.Confidence,
                        e.IB.High, e.IB.Low, e.Profile.Poc, e.Profile.Val, e.Profile.Poc, e.Profile.Vah,
                        (e.Profile.Singles != null ? e.Profile.Singles.Count : 0),
                        (e.Otf.Up ? "Up" : (e.Otf.Down ? "Down" : "-")),
                        e.Otf.Frame
                    )
                );

                UiThread.BeginInvoke(delegate()
                {
                    if (ui != null)
                    {
                        ui.UpdateUi(
                            e.Et, e.Symbol, call.Bias, call.DayType, call.Confidence, "low",
                            new string[] { "POC " + e.Profile.Poc.ToString("0.00") + ", VA [" + e.Profile.Val.ToString("0.00") + "/" + e.Profile.Vah.ToString("0.00") + "]" },
                            new string[] { "Opening " + e.Opening.Type + ", IB[" + e.IB.High.ToString("0.00") + "/" + e.IB.Low.ToString("0.00") + "]" },
                            e.DPocTrail
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                TpoLogger.Error("OnBar exception: " + ex.Message);
            }
        }
    }
}
