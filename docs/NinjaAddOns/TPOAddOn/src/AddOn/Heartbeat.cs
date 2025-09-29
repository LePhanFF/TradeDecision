using System;
using System.Threading;

namespace NinjaTrader.NinjaScript.AddOns
{
    public sealed class Heartbeat : IDisposable
    {
        private Timer _timer;
        private TimeSpan _interval;
        private Action _onTick;

        public Heartbeat(TimeSpan interval, Action onTick)
        {
            _interval = interval;
            _onTick = onTick;
        }

        public void Start()
        {
            try
            {
                // dueTime = interval, period = interval
                _timer = new Timer(new TimerCallback(Tick), null, _interval, _interval);
            }
            catch (Exception ex)
            {
                TpoLogger.Warn("Heartbeat start failed: " + ex.Message);
            }
        }

        private void Tick(object state)
        {
            try { if (_onTick != null) _onTick(); } catch { }
        }

        public void Dispose()
        {
            try
            {
                if (_timer != null) { _timer.Dispose(); _timer = null; }
            }
            catch { }
        }
    }
}
