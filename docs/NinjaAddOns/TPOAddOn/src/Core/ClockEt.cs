using System;

using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Core
{
    public static class ClockEt
    {
        public static DateTime NowEt()
        {
            try
            {
                return TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            }
            catch
            {
                return DateTime.Now;
            }
        }
    
        public static DateTime ToEt(DateTime utc)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                if (utc.Kind == DateTimeKind.Local)
                    return TimeZoneInfo.ConvertTime(utc, tz);
                if (utc.Kind == DateTimeKind.Unspecified)
                    return TimeZoneInfo.ConvertTime(utc, tz);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            }
            catch
            {
                return utc.ToLocalTime();
            }
        }

        public static string HM(DateTime t)
        {
            try { return t.ToString("HH:mm"); }
            catch { return ""; }
        }
    
}
}
