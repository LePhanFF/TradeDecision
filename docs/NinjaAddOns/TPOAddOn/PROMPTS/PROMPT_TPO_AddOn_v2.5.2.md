# TPO Add-On Prompt — v2.5.2 (FULL) — Nodes + B‑Day + Dual‑IB — aligned to TPO v9.4.11
Generated: 2025-09-30 04:24

> **Purpose**: Build a **NinjaTrader 8 Add-On** (C#) that subscribes to a *draw-plan JSON* and renders **TPO/Volume-Profile–driven levels**, **B‑Day node logic**, and **Dual‑IB (60m primary, 15m preview)** cues in real time. **No regressions** vs prior add-on specs.

---

## 0) Non-Negotiables (No Regression Checklist)
- Rendering primitives: horizontal lines (VAH/POC/VAL/IBH/IBL), price bands (zones), markers (targets), text/flags (annotations).
- Data contract: accept schema v1.2 draw-plan JSON; tolerate optional blocks (liquidity_pools, fvgs, ict_anchors, indicators).
- B‑Day logic: nodes + seam, edge-fade defaults, seam acceptance → B→Trend, seam fill → B→Balance.
- Dual-IB: primary 60m (Dalton) for classification; preview 15m close for early hints.
- dPOC path: paint 15‑minute dPOC breadcrumbs.
- Performance: UI-safe, non-blocking; target < 10ms per refresh, no memory leaks, safe dispose.
- Logging: ribbon + file logger; alert routing without UI-thread exceptions.
- Config: user toggles to enable/disable modules (VWAP hints, overlays) per day-type.
- Tests: load sample JSONs for NQ/ES/YM; verify lines/zones/alerts; regression fixtures.

---

## 1) Data Contracts (Parser Hints & Self-Checks)
- CSV Parsing (upstream): skip metadata row; tz-naive US/Eastern; use session_date; **RTH 09:30–16:00** for core; **ETH 18:00→09:30** only for **anchors**.
- Self-check banner: show “Parse contract OK (RTH core, ETH for anchors)” when JSON carries `data_filter` + `eth_for_anchors`.

---

## 2) Draw-Plan JSON — Schema v1.2
**Required**: vah, poc, val, ib_high, ib_low, hvn, lvn, zones[], lines[], targets[]  
**Optional**: liquidity_pools[], fvgs[], ict_anchors, indicators, data_filter, eth_for_anchors

### 2.1 Lines
- Examples: VAH/POC/VAL (dashed/solid/dotted), IBH/IBL (dashdot).

### 2.2 Zones (bands)
- Examples: short_edge, short_poc, short_val (and long mirrors).

### 2.3 Targets & Annotations
- Examples: T1/T2/T3 and text/flag helpers.

### 2.4 Alerts (IDs)
- `ALERT_PDAY_VWAP_FAIL`, `ALERT_BDAY_SEAM_BREAK`, `ALERT_BDAY_SEAM_FILL`, `ALERT_NEUTRAL_EXTREME`, `ALERT_DDT_PAUSE_BREAK`, `ALERT_GAP_FULLFILL`, `ALERT_EXCESS_HIGH/LOW`, `ALERT_POOR_HIGH/LOW`, `ALERT_POOL_SWEEP_REJECT`, `ALERT_POOL_RECLAIM_ACCEPT`, `ALERT_FVG_REJECT_MID`, `ALERT_FVG_INVERTED`.

---

## 3) Rendering & UX
- Price vertical, Time horizontal; mark VAH/POC/VAL on price axis.
- Layer order: zones < lines < annotations/targets.
- Active play highlight by day-type; theme-aware visuals; snap to tick.

---

## 4) State Machine (Day-Type & Morphs)
- Day-type badge; Dual‑IB badges; B‑acceptance bars.
- Morphs: B→Trend (seam acceptance + dPOC↑), B→Balance (seam fill time/traversals), Balance→Trend (1×30m TPO beyond VA), Neutral→NE (extreme close).

---

## 5) ICT Overlays
- Liquidity pools (scored, flags sweep_reject / reclaim_accept, next_target).  
- FVGs (status/rejection/inversion; overlap badges).  
- Anchors (ONH/ONL, London H/L, prev_day_eth H/L, midnight_open, weekly_open_gap).

---

## 6) Alerts → Ninja
- Route by category (Trend/Gap/Liquidity/FVG/Morph), de‑dupe, panel UI.

---

## 7) Architecture (C# NT8)
- DataBus (JSON→models), Renderer (WPF), Controller (state), AlertRouter; Dispatcher marshaling; IDisposable.
- Settings: module toggles, opacities, label density, alert categories, log verbosity.
- IO: file watcher + paste support.

---

## 8) Performance & Stability
- Diff drawing; reuse visuals; coalesce updates (~10Hz); cap lists; rotate logs.

---

## 9) Telemetry & Logging
- Ribbon: day-type, IB badges, seam state, dPOC drift, last alert.  
- File log: version/schema stamped; Debug overlay toggle.

---

## 10) Test Plan
- Fixture JSONs per day-type; assert visuals/alerts; theme toggle; no exceptions.

---

## 11) Deliverables
- Source under `NinjaTrader.NinjaScript.AddOns.TPOAddOn/`  
- README (install + schema table)  
- Examples/ (NQ/ES/YM)  
- QA checklist + screenshots

---

## 12) Acceptance Criteria
- Renders core refs + nodes/seam + dPOC trail; B-day defaults; Dual‑IB badges; overlays when present; alert routing; performance + tests pass.

---

## 13) Versioning
- Add‑On v2.5.2, schema v1.2; backward compatible when schema expands.

> End of prompt — build the NinjaTrader 8 Add-On to this spec.
