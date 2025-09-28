
#region Using declarations
using System;
namespace NinjaTrader.NinjaScript.AddOns.Core
{
    internal static class ClockEt
    {
        private static readonly TimeZoneInfo EtTz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        public static DateTime ToEt(DateTime utc){ if(utc.Kind!=DateTimeKind.Utc) utc = DateTime.SpecifyKind(utc.ToUniversalTime(), DateTimeKind.Utc); return TimeZoneInfo.ConvertTimeFromUtc(utc, EtTz); }
        public static string HM(DateTime et) => et.ToString("HH:mm");
        public static string HMS(DateTime et) => et.ToString("HH:mm:ss");
        public static int GetEtOffsetMinutes(DateTime utc){ var et=ToEt(utc); return (int)(et-utc).TotalMinutes; }
        public static bool IsDst(DateTime utc){ var et=ToEt(utc); return EtTz.IsDaylightSavingTime(et); }
    }
}
