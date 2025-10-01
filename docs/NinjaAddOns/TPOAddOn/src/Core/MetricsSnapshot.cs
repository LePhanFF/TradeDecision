using System;
using System.Collections.Generic;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    // Additive-only DTO; safe for C# 5 and NinjaScript.
    public class MetricsSnapshot
    {
        public string SchemaVersion = "tpo.metrics.v9_4_11";
        public string Symbol;
        public string Role;        // Primary / Observer
        public DateTime TsEt;
        public DateTime TsUtc;
        public double Last;

        public SessionMeta Session = new SessionMeta();
        public Codes Codes = new Codes();
        public Levels Levels = new Levels();

        public IbDual IbDual = new IbDual();
        public Acceptance Acceptance = new Acceptance();
        public IbBreakSet Ib = new IbBreakSet();

        public ValueBlock Value = new ValueBlock();
        public ProfileBlock Profile = new ProfileBlock();
        public DPocBlock DPoc = new DPocBlock();

        public BDayBlock BDay = new BDayBlock();
        public DrawPlan DrawPlan = new DrawPlan();
    }

    public class SessionMeta { public string RthPhase="RTH"; public int SinceOpenMin; public double TickSize; public double Adr20; }
    public class Codes { public int OpeningType=-1; public int ReDir=0; public int DayType=0; public int Shape=-1; }

    public class Levels
    {
        public LevelPair Ib = new LevelPair();
        public ValueLevels Value = new ValueLevels();
        public DpocLevel Dpoc = new DpocLevel();
        public BalanceLevels Balance = new BalanceLevels();
    }
    public class LevelPair { public double High; public double Low; }
    public class ValueLevels { public double Vah; public double Poc; public double Val; }
    public class DpocLevel { public double Current; }
    public class BalanceLevels { public double Top; public double Bottom; }

    public class IbDual { public IbSide Primary = new IbSide(); public IbSide Preview = new IbSide(); }
    public class IbSide { public int Minutes; public TpoIbMetrics Ib = new TpoIbMetrics(); }
    public class TpoIbMetrics { public double High; public double Low; public double Range; public int SizeTicks; }

    public class Acceptance { public List<AcceptanceEvent> Timeline = new List<AcceptanceEvent>(); }
    public class AcceptanceEvent { public string Edge; public string Tier; public string Et; public string Status; }

    public class IbBreakSet { public List<IbBreak> Breaks = new List<IbBreak>(); }
    public class IbBreak { public string Dir; public string Et; public string Hold; public string Retest; }

    public class ValueBlock { public TpoValue Tpo = new TpoValue(); }
    public class TpoValue { public double Vah; public double Poc; public double Val; public int TotalTpos; }

    public class ProfileBlock { public ProfileNodes Nodes = new ProfileNodes(); public SinglesBlock Singles = new SinglesBlock(); }
    public class ProfileNodes { public HvnList Hvn = new HvnList(); public LvnList Lvn = new LvnList(); public int NodeCount; public Seam Seam=new Seam(); public double Top2SeparationRatio; }
    public class HvnList { public List<NodeItem> Top3 = new List<NodeItem>(); }
    public class LvnList { public List<NodeItem> UpTo5 = new List<NodeItem>(); }
    public class NodeItem { public double Price; public double Prominence; public string Method; public int DistanceTicks; }
    public class Seam { public double Price; public int WidthTicks; }
    public class SinglesBlock { public List<SingleSpan> Levels = new List<SingleSpan>(); }
    public class SingleSpan { public double Start; public double End; }

    public class DPocBlock
    {
        public double Current;
        public double Start;
        public List<DpocPathPoint> Path15m = new List<DpocPathPoint>();
        public double MigrationTicks;
        public double MigrationSpeedTicksPerHr;
        public double StabilityRatioLast4;
        public string Jump;
    }
    public class DpocPathPoint { public string Et; public double Price; }

    public class BDayBlock
    {
        public int TimeInLowerBars;
        public int TimeInBetweenBars;
        public int TimeInUpperBars;
        public int SeamTraversals;
        public bool SeamAcceptance;
        public double SeamFillRatio;
        public BDayMorph Morph = new BDayMorph();
    }
    public class BDayMorph { public string ToTrendTime; public string ToBalanceTime; }

    public class DrawPlan { public List<LineSpec> Lines = new List<LineSpec>(); public List<ZoneSpec> Zones = new List<ZoneSpec>(); public List<TargetSpec> Targets = new List<TargetSpec>(); }
    public class LineSpec { public string Role; public double Price; public string Style; public string Ttl; public string Label; }
    public class ZoneSpec { public string Role; public double Top; public double Bottom; public double Alpha; public string Ttl; public string Label; }
    public class TargetSpec { public string Role; public double Price; public string Label; public double Confidence; }
}
