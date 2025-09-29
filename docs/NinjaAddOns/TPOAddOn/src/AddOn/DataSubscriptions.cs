using System;
using System.Globalization;

namespace NinjaTrader.NinjaScript.AddOns
{
    public static class DataSubscriptions
    {
        // Convert placeholders like "ES ##-##" to a front-month like "ES 12-25"
        public static string ResolveFrontMonth(string raw, DateTime now)
        {
            if (string.IsNullOrEmpty(raw)) return raw;

            // If it already has a month-year, return as-is
            if (raw.IndexOf("##-##") < 0 && raw.IndexOf("##/##") < 0) return raw;

            // Determine quarter month (03,06,09,12)
            int m = now.Month;
            int y = now.Year % 100;
            int q;
            if (m <= 3) q = 3;
            else if (m <= 6) q = 6;
            else if (m <= 9) q = 9;
            else q = 12;

            string mm = q.ToString("00", CultureInfo.InvariantCulture);
            string yy = y.ToString("00", CultureInfo.InvariantCulture);

            string repl = mm + "-" + yy;
            string sym = raw.Replace("##-##", repl).Replace("##/##", repl);
            return sym;
        }
    }
}
