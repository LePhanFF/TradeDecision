
# TPO Add-On Prompt — v2.5.2 (Nodes + B-Day Acceptance) — aligned to v9.4.11
Generated: 2025-09-30 02:39 UTC

## Scope of this sprint
Implement the following, preserving all prior features (no regressions):
1) Dual-IB: primary = 60 minutes (Dalton standard), preview = 15 minutes (early alerts). Compute and emit both.
2) Acceptance timeline: per edge (IBH/IBL) record accept/reject events with tier (15m/30m/proxy) and timestamps.
3) Profile nodes enhancement: hvn.top3[], lvn.upTo5[], nodeCount, seam.price, seam.width, top2SeparationRatio.
4) B-Day acceptance metrics: timeInLowerBars, timeInBetweenBars, timeInUpperBars, seamTraversals, seamAcceptance, seamFillRatio, morph.toTrendTime, morph.toBalanceTime.
5) Extend drawPlan minimally to include seam and node edges for plotting.
6) Add regression guard rails and compile/thread-safety guards.

## Non-negotiable constraints (regression guard rails)
- Do not remove or rename existing JSON keys. Additive only.
- Schema version bump: "tpo.metrics.v9_4_11". Keep all v9_4_4/v9_4_3 keys intact.
- Five-minute snapshots must continue to publish to MetricsBus and write per-symbol JSON + metrics_latest.json.
- C# 5 only (NinjaScript): no string interpolation ($""), no null-conditional (?.), no expression-bodied members (=>).
- All UI updates via Dispatcher helpers (UiThread.BeginInvoke/Invoke). No cross-thread access.
- All file IO under Documents/NinjaTrader 8/NinjaAddOn/TPOAddon/; create directories if missing.
- On missing dependencies, populate fields with null/default and proceed. Never throw from snapshot build.
- Keep existing ScoreEngine and MetricsReporter behavior unchanged apart from additive fields.
- Keep existing nodes.hvns/lvns arrays, dpoc.path15m, levels, codes, and RTH gap intact.

## Inputs (assume already available)
- BarComputedEventArgs provides: symbol, Et/Utc, O/H/L/C, IB info (SinceOpenMin, High/Low, Ext counts), Opening info, OTF flags, Profile (POC/VAH/VAL/TotalTpos, HVN/LVN arrays, Singles), dPOC trail.
- Tick size map: NQ=0.25, ES=0.25, YM=1.0 (from config).
- Peers model for gap-close summary (ES/NQ/YM).

## Feature 1: Dual-IB (primary 60m, preview 15m)
- Add new block: ibDual
  - primary.minutes, preview.minutes
  - primary.ib: high, low, range, sizeTicks
  - preview.ib: high, low, range, sizeTicks
- Acceptance rules (as per v9.4.11):
  - primary: >= 1 x 30m TPO beyond edge (Dalton acceptance)
  - preview: >= 1 x 15m close beyond edge (early signal)
  - proxy: >= 2 x 5m closes beyond level (alternative proxy)
- Emit acceptance.timeline[] entries: { edge: "IBH"|"IBL", tier: "15m"|"30m"|"proxy", et: "HH:mm", status: "accept"|"reject" }.

## Feature 2: Acceptance timeline & IB breaks
- Extend ib.breaks[]: { dir:"Up"|"Down", et:"HH:mm", hold:"success"|"fail", retest:"pass"|"fail" }.
- If an edge is broken and price returns inside within the same 15m, mark hold="fail".
- Record first break time for reporting compatibility (ib_break_time).

## Feature 3: Profile nodes enhancement
- Compute hvn.top3[] using prominence_ratio=0.6 and spacing to avoid near-duplicates.
- Compute lvn.upTo5[] similarly for minima.
- nodeCount: total HVNs detected.
- seam.price: LVN valley between the top two HVNs; seam.width: contiguous thin span width in ticks.
- top2SeparationRatio: distance between top two HVNs divided by VA width (0..1).

## Feature 4: B-Day acceptance metrics
- Classify P/b/B based on TPO shape and node structure; when B-Day:
  - Count 5m bars by region:
    - lower_node: below seam toward lower HVN
    - between_nodes: around seam band
    - upper_node: above seam toward upper HVN
  - timeInLowerBars/timeInBetweenBars/timeInUpperBars = integer bar counts.
  - seamTraversals: number of times price crosses seam band from one node to the other.
  - seamAcceptance: true if >=2 closes beyond seam into a node.
  - seamFillRatio: proportion of 5m closes that occur inside the seam band.
  - morph.toTrendTime: first timestamp when seamAcceptance=true AND dPOC migrates into the destination node.
  - morph.toBalanceTime: time when seamFillRatio>=0.2 AND >=2 traversals.
- Emit under bDay.* and profile.nodes.seam.*.

## Feature 5: Draw-Plan minimal
- drawPlan.lines: add seam line { role:"seam", price, style:"dash", ttl:"session", label:"LVN seam" }.
- drawPlan.zones: optional thin zone for seam width { role:"seam_zone", top, bottom, alpha:0.08, ttl:"session" }.
- drawPlan.targets: upper/lower node edge magnets { role:"node_edge", price, label:"Upper/Lower Node" }.

## JSON schema additions (additive)
- ibDual.*
- acceptance.timeline[]
- ib.breaks[]
- profile.nodes: hvn.top3[], lvn.upTo5[], nodeCount, seam.price, seam.width, top2SeparationRatio
- bDay.* (fields listed above)
- drawPlan (lines/zones/targets) minimal, in addition to existing levels/codes.

## Code touch points (C# 5 safe)
- MetricsSnapshot.cs: add DTO fields for the new blocks (strings, doubles, ints, lists).
- MetricsAggregator.cs: compute and populate new fields using available profile histogram and price series.
- MetricsReporter.cs: serialize new fields in JSON; keep existing "levels" and "codes" stable.
- NinjaAddOnTPO.cs: no breaking changes; ensure acceptance events are captured as timeline entries.
- UiHostWindow.cs: no mandatory changes; optional: append seam/node summary lines to commentary.
- ScoreEngine.cs: do not change scoring yet (v2.5.2 focuses on metrics).

## Unit/regression tests
- JSON shape test: ensure ibDual, acceptance.timeline, profile.nodes.seam, bDay fields appear (even if null).
- Enum domain checks: acceptance.timeline.tier in {15m,30m,proxy}; ib.breaks.dir in {Up,Down}.
- Range checks: seam.width >= 0; top2SeparationRatio in [0, 1].
- Non-throwing: BuildFromBar never throws on missing upstream pieces.

## Outputs must still include (regression guard)
- levels (ib/value/dpoc/balance), codes (openingType, reDir, dayType, shape), nodes.hvns/lvns arrays, dpoc.path15m, rthGap, peers, profile.singles.

## Acceptance checklist for v2.5.2
- Compiles in NinjaScript (C# 5 only).
- metrics_latest.json shows ibDual.*, acceptance.timeline[], profile.nodes.seam.*, bDay.* and minimal drawPlan additions.
- Indicators can render seam line from drawPlan.lines and continue to render IB/VA/POC/dPOC from levels.
- No crashes when profile nodes are sparse or missing.
