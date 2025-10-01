using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.AddOns.TpoV25.Core;




namespace NinjaTrader.NinjaScript.AddOns.TpoV25
{



    public class TpoAddOn : AddOnBase
    {
		
	    // TEST MODE — synthetic bars (toggle off when not needed)
	    private System.Threading.Timer test;
	    private bool testMode = false;   // <- set false to disable
	    private int testMinute = 0;
	    private double testPrice = 15800.00;		
        private const string VersionTag = "v2.5.2-metrics-only+hb";
        private NTMenuItem menuItem;
        private bool isRunning = false;

        private MetricsReporter reporter;
        private readonly string baseDir = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "NinjaAddOn", "TPOAddon");

        // simple NQ/ES/YM watch list; adapt later to instruments.txt
        private readonly string[] watch = new string[] { "NQ 12-25", "ES 12-25", "YM 12-25" };

        private readonly Dictionary<string, MetricsAggregator> aggs = new Dictionary<string, MetricsAggregator>();

        // NEW: heartbeat + bar count
        private Timer hb;
        private int barCount;

        // NEW: global accessor so other code can call EmitBar(...)
        public static TpoAddOn Current;

        protected override void OnWindowCreated(Window w)
        {
            var cc = w as ControlCenter; if (cc == null) return;
            var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if (newMenu == null) return;

            if (menuItem == null)
            {
                menuItem = new NTMenuItem
                {
                    Header = "TPO Metrics " + VersionTag,
                    Style = Application.Current.TryFindResource("MainMenuItem") as Style
                };
                menuItem.Click += delegate (object s, RoutedEventArgs e) { if (!isRunning) Start(); else Stop(); };
                newMenu.Items.Add(menuItem);
            }
        }

        protected override void OnWindowDestroyed(Window w)
        {
            var cc = w as ControlCenter; if (cc == null) return;
            if (menuItem != null)
            {
                var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
                if (newMenu != null && newMenu.Items.Contains(menuItem)) newMenu.Items.Remove(menuItem);
                menuItem = null;
            }
            if (isRunning) Stop();
        }

        private void Start()
        {



            try
            {
                System.IO.Directory.CreateDirectory(baseDir);
                Logger.Info("[TPO] Start " + VersionTag);

                reporter = new MetricsReporter(baseDir);

                // init aggregators per symbol (use default tick sizes: YM=1.0 else 0.25)
                aggs.Clear();
                for (int i = 0; i < watch.Length; i++)
                {
                    var sym = watch[i];
                    double tick = sym.StartsWith("YM") ? 1.0 : 0.25;
                    aggs[sym] = new MetricsAggregator(sym, tick);
                }

                // heartbeat every ~30s so you can see activity in Output tab
                hb = new Timer(_ =>
                {
                    Logger.Info("[TPO] hb — running " + VersionTag);
                }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));

                barCount = 0;
                isRunning = true;
                Current = this;

                // --- TEST EMITTER: fake bar every 5s so you can see JSON + logs without engine
                if (testMode)
                {
                    test = new System.Threading.Timer(_ => {
                        try { GenerateTestBar(); } catch (Exception ex1) { Logger.Error("[TPO] Start exception: " + ex1.Message);   }
                    }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                }       

                UpdateHeader();


         
            }
            catch (Exception ex) { Logger.Error("[TPO] Start exception: " + ex.Message); }
        }

        private void Stop()
        {
            try
            {

                if (test != null) { try { test.Dispose(); } catch {} test = null; }

                isRunning = false;
                UpdateHeader();

                if (hb != null) { try { hb.Dispose(); } catch { } hb = null; }
                Current = null;

                Logger.Info("[TPO] Stopped.");
            }
            catch (Exception ex) { Logger.Error("[TPO] Stop exception: " + ex.Message); }
        }

        // SAFE: always marshal via the owning control's Dispatcher; add bar counter to make activity visible
        private void UpdateHeader()
        {
            var mi = menuItem; if (mi == null) return;

            string h = (isRunning ? "Stop TPO Metrics " : "TPO Metrics ") + VersionTag + "  • bars: " + barCount;

            var d = mi.Dispatcher;
            if (d == null || d.HasShutdownStarted || d.HasShutdownFinished) return;

            if (d.CheckAccess())
            {
                mi.Header = h;
            }
            else
            {
                d.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (menuItem != null &&
                            menuItem.Dispatcher != null &&
                            !menuItem.Dispatcher.HasShutdownStarted &&
                            !menuItem.Dispatcher.HasShutdownFinished)
                        {
                            menuItem.Header = h;
                        }
                    }
                    catch { /* swallow to keep add-on alive */ }
                }));
            }
        }

        // Feed bars into the add-on (call from your engine handler). Emits snapshot + JSON + logs.
        public void EmitBar(TpoBar b)
        {
            try
            {
                Logger.Info("[TPO] EmitBar called for " + (b.Symbol ?? "?"));

                if (!isRunning) { Logger.Warn("[TPO] EmitBar ignored — not running"); return; }
                if (b.Symbol == null) { Logger.Warn("[TPO] EmitBar ignored — null symbol"); return; }
                if (!aggs.ContainsKey(b.Symbol)) { Logger.Warn("[TPO] EmitBar ignored — no aggregator for " + b.Symbol); return; }

                string role = b.Symbol.StartsWith("NQ") ? "Primary" : "Observer";

                var s = aggs[b.Symbol].Build(b, role);
                if (s == null) { Logger.Warn("[TPO] Aggregator returned null snapshot"); return; }

                MetricsBus.Put(s);
                reporter.Write(s);

                barCount++;
                Logger.Info("[TPO] " + b.Symbol + " bar " + barCount + " @ " + b.Et.ToString("HH:mm"));

                UpdateHeader();
            }
            catch (Exception ex) { Logger.Warn("[TPO] EmitBar exception: " + ex.Message); }
        }

		
        private void GenerateTestBar()
        {
            Logger.Info("[TPO] GenerateTestBar() firing...");

            testMinute += 5;
            if (testMinute > 360) testMinute = 5;

            testPrice += (testMinute % 10 == 0 ? 4 : -3);

            var et = DateTime.Now;

            var b = new NinjaTrader.NinjaScript.AddOns.TpoV25.Core.TpoBar
            {
                Symbol       = "NQ 12-25",
                Et           = et,
                Utc          = DateTime.UtcNow,
                SinceOpenMin = testMinute,
                O = testPrice - 2,
                H = testPrice + 3,
                L = testPrice - 4,
                C = testPrice + 1,
                Val       = testPrice - 10,
                Poc       = testPrice,
                Vah       = testPrice + 10,
                TotalTpos = 120,
                IbHigh      = testPrice + 12,
                IbLow       = testPrice - 12,
                ExtUpCount  = (testMinute % 15 == 0 ? 1 : 0),
                ExtDownCount= 0,
                OpeningType = "OTD",
                DayType     = "Normal Variation"
            };

            EmitBar(b); // push through aggregator -> JSON -> logs
        }


		
    }
}
