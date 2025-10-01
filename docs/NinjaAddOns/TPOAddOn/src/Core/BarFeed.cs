using System;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    // Minimal bar context interface; AddOn wires engine bars into this form and calls MetricsAggregator.
    public interface IBarEmitter
    {
        void Emit(TpoBar bar);
    }

    public struct TpoBar
    {
        public string Symbol;
        public DateTime Et;
        public DateTime Utc;
        public int SinceOpenMin;
        public double O,H,L,C;

        // TPO/Value
        public double Vah, Poc, Val;
        public int TotalTpos;

        // IB
        public double IbHigh, IbLow;
        public int ExtUpCount, ExtDownCount;

        // Nodes/Singles — optional
        public System.Collections.Generic.List<Node> Hvns;
        public System.Collections.Generic.List<Node> Lvns;
        public System.Collections.Generic.List<Span> Singles;

        // Optional tags
        public string OpeningType;
        public string DayType;
    }

    public struct Node { public double Price; public double Prominence; } // Prominence 0..1
    public struct Span { public double Start; public double End; }
}
