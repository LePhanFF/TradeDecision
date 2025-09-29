using System;

namespace NinjaTrader.NinjaScript.AddOns
{
    public static class TimeUtil
    {
        // Converts UTC -> ET using fixed US Eastern offset approximation.
        // For exact conversion, prefer Ninja's session manager if available.
        public static DateTime ToEt(DateTime utc)
        {
            // Simple placeholder: assume ET = UTC-5, adjust DST manually if desired.
            return utc.AddHours(-5);
        }

        public static string HM(DateTime dt)
        {
            return dt.ToString("HH:mm");
        }
    }
}
