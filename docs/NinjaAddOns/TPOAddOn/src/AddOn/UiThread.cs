using System;
using System.Windows;
using System.Windows.Threading;

namespace NinjaTrader.NinjaScript.AddOns
{
    public static class UiThread
    {
        public static void Invoke(Action action)
        {
            if (action == null) return;
            try
            {
                var app = Application.Current;
                if (app != null)
                {
                    Dispatcher d = app.Dispatcher;
                    if (d != null)
                    {
                        if (d.CheckAccess()) action();
                        else d.Invoke(action);
                        return;
                    }
                }
                action();
            }
            catch { }
        }

        public static void BeginInvoke(Action action)
        {
            if (action == null) return;
            try
            {
                var app = Application.Current;
                if (app != null)
                {
                    Dispatcher d = app.Dispatcher;
                    if (d != null)
                    {
                        try { d.BeginInvoke(action); return; } catch { }
                    }
                }
                // Fallback
                action();
            }
            catch { }
        }
    }
}
