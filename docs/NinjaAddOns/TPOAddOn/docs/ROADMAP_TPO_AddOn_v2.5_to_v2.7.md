
# TPO Add-On Roadmap (v2.5 -> v2.7) aligned to TPO Analysis v9.4.11
Generated: 2025-09-30 02:39 UTC

## Principles
- Backward-compatible superset: nothing dropped from v9.4.3/v9.4.4. Only additive changes for v9.4.11.
- Dalton fidelity: 30-minute TPO is the standard bracket. Initial Balance (IB) = first hour (2 x 30m TPOs).
- v9.4.11 dual-IB: primary IB = 60m (Dalton), preview IB = 15m (early alerts). Both are computed and emitted.
- One data contract for both live (in-memory bus) and audit/replay (JSON on disk).
- Five-minute cadence for snapshots; 30m structures for classification; 15m overlay for early detection.

## Schema version
- JSON/BUS schema: "tpo.metrics.v9_4_11" (additive superset of v9_4_4).

## Delivery artifacts
- In-memory: MetricsBus.Publish(MetricsSnapshot) per symbol.
- Files: Documents/NinjaTrader 8/NinjaAddOn/TPOAddon/metrics/
  - metrics_{SYMBOL}_{YYYYMMDD_HHmm}.json (every 5 minutes)
  - metrics_latest.json (combined primary + observers)

## Phase plan

### v2.5.0 - Contract and Registry
- Freeze schema to tpo.metrics.v9_4_11 (additive).
- Add specs/registry (list every key with type, units, dependencies, default).
- Unit tests for JSON shape and enums/ranges.

### v2.5.1 - Dual-IB and Acceptance Timeline
- Config: primaryIBMinutes=60 (default), previewIBMinutes=15 (default).
- Compute and emit: ibDual.primary and ibDual.preview, both with high/low/range/sizeTicks.
- acceptance.timeline[]: per edge (IBH/IBL) record events with tier (15m/30m/proxy) and accept/reject status.
- ib.breaks[]: direction, time, hold success/failure, retest result.

### v2.5.2 - Nodes + B-Day Acceptance Metrics (THIS SPRINT)
- Profile nodes enhancement: hvn.top3[], lvn.upTo5[], nodeCount, seam.price, seam.width, top2SeparationRatio.
- B-Day metrics: timeInLowerBars, timeInBetweenBars, timeInUpperBars, seamTraversals, seamAcceptance, seamFillRatio, morph.toTrendTime, morph.toBalanceTime.
- Extend drawPlan (lines/zones/targets) minimally to cover seam and node edges.
- Regression guard rails and compile guards (C# 5 safe, no unsafe WPF calls).

### v2.5.3 - VWAP and P/b-specific Logic
- VWAP + sd bands + slope.
- P-Day: bounce1/2 retrace pct.
- b-Day: reclaim time, closesAbovePocCount.
- vwap.touches, firstTouchEt, maxExcursion.

### v2.5.4 - Gaps Expansion + Peer Summary
- Classify gap type: gap_and_go, partial_fill_balance, full_gap_fill, orr_gap_fill.
- gapPercentFilled, gapFillEt, magnetTarget.
- peers gap summary (ES/NQ/YM).

### v2.5.5 - ICT Anchors
- London high/low, Midnight open, PM open 14:30 ET.
- Prev ETH H/L, weekly/monthly/quarterly H/L/Open, weekly_open_gap_pts.

### v2.5.6 - Acceptance/Reject, Single Prints, Overlays
- closesAboveVahCount, closesBelowValCount, seamClosesBeyondCount.
- oneTimeframing30mCount, hourlyDirectionalCandlesCount.
- singlePrints levels/unrepairedCount/filledTimes; overlays poor/excess + repair ETs.

### v2.5.7 - Liquidity Pools + FVGs + Scoring
- Liquidity pools across TFs with overlaps and scoring.
- FVGs across TFs with status/rejection/inversion and scoring.
- Scoring breakdown: structure, triggerQuality, confluence, flow, total, rationale[].

### v2.5.8 - Alerts + Morph Log + Draw-Plan
- Map alert catalog to metrics; alerts.fired[] timeline.
- morph.log[] structure transitions.
- drawPlan fully populated for indicator zero-logic rendering.

## Regression guard rails (always-on)
- Must not remove or rename existing keys. Add only.
- Must continue to emit: levels, codes, nodes.hvns/lvns arrays, dpoc.path15m, peer gap summary, RTH gap, profile.singles.
- Must preserve five-minute cadence and publish to both bus and disk.
- C# 5 only: no interpolated strings, no null-conditional, no expression-bodied members.
- WPF Dispatcher for any UI access; no cross-thread violations.
- File paths: under Documents/NinjaTrader 8/NinjaAddOn/TPOAddon/; create folders if missing.
- On missing dependencies, set fields to null/default; never throw from snapshot build.

## Acceptance criteria per phase
- Compiles clean on fresh NinjaScript.
- metrics_latest.json contains all required keys for the phase and passes registry tests.
- Indicator can render IB/VA/POC/dPOC/seam/HVN/LVN and basic drawPlan without schema changes.
- Unit tests for enums and ranges green.

## Next
- v2.5.2 prompt will instruct the generator to implement nodes + B-Day metrics + guard rails.
