
#region Using declarations
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
#endregion
namespace NinjaTrader.NinjaScript.Indicators
{
    public class MyMinimalIndicator : Indicator
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "My Minimal Indicator";
                Calculate = Calculate.OnBarClose;
            }
        }
        protected override void OnBarUpdate()
        {
            // no-op; compile test only
        }
    }
}
