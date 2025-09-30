# AddOn Prompt v2.5 — Phase 2 Foundation (CS5-safe)
Updated: 2025-09-29 04:23 

## Objective
Implement **multi-index configuration** and the **complete v9.4.4 TPO metrics foundation** with periodic JSON snapshots and on-screen summaries. NQ is **primary**; ES/YM are **observers** for divergence/relative strength. This is **foundation only**; playbooks will be added in Phase 3.

---

## Non-negotiable Guardrails (inherit v2.3/v2.4)
- **Language level:** C# 5 only. **NO** string interpolation (`$"..."`), **NO** null-conditional (`?.`), **NO** expression-bodied members, **NO** pattern matching, **NO** `using var`.
- **Dispatcher safety:** All UI updates via helpers (`UiThread.Invoke/BeginInvoke`). No direct usage of `Application.Current.Dispatcher` outside helpers.
- **Start/Stop:** Create `UiHostWindow` on UI thread; close via its own dispatcher. Stop timers/heartbeats on Stop().
- **File system:** Write under `Documents\NinjaTrader 8\NinjaAddOn\TPOAddon\` in dedicated subfolders.
- **Namespace & braces:** Everything inside namespaces; balanced braces; no top-level code.
- **Do not drop** any existing features from v2.4 (logging, heartbeat, instrument resolution).

---

## 1) Multi-index Configuration
- **Config file:** `instruments.txt` in the AddOn data folder. Support 3 patterns:
  1. Keyed format (preferred):
     ```
     primary=NQ 12-25
     observers=ES 12-25,YM 12-25
     ```
  2. List format (fallback): first line is **primary**, rest are observers.
  3. Placeholders: support `##-##` → resolve to front-month via `DataSubscriptions.ResolveFrontMonth()`.

- **Runtime rules:**
  - If primary not present → default to `NQ` (front-month).
  - Unknown instruments → log warning and skip (do not crash).
  - Provide `InstrumentRole` enum: `Primary`, `Observer`.

---

## 2) Metrics Engine (v9.4.4 superset)
Create a **MetricsEngine** that subscribes to 5‑minute bars (RTH, optional ON if available) per instrument and constructs a **MetricsSnapshot** DTO. Include all fields below (populate with values; if unavailable, set null/defaults).

### 2.1 Session/Context
- `rthPhase` (RTH/ON), `sinceOpenMin`, `tickSize`, `adr20`.
- Prior-day refs (H/L, TPO VAH/POC/VAL; Volume VAH/POC/VAL if available).

### 2.2 Opening Context
- `opening.location` vs prior day (outside value/range/inside).
- `opening.type` ∈ {{OD, OTD, ORR, OA}}.

### 2.3 Gap Module
- `gap.type` ∈ {{Outside_Prior_Range, Into_Prior_Range, Within_Prior_Value}}.
- `gap.sizeTicks`, `gap.sizePctAdr`, `gap.fillPct`, `gap.timeToFirst50FillMin`, `gap.fillCompleted` (bool).

### 2.4 Overnight Inventory
- `onInventory.state` ∈ {{Too_Long, Too_Short, Balanced}}.
- `onInventory.correctionRisk` (bool).

### 2.5 IB / RE / Acceptance
- `ib.high`, `ib.low`, `ib.sizeTicks`.
- `re.direction` ∈ {{Up, Down, None}}, `re.countUp`, `re.countDown`.
- `acceptance`:
  - `tier` ∈ {{Provisional15m, Confirmed30m, None}}.
  - Evidence: `elongation`, `valueDrift` (bool).

### 2.6 Value Areas (TPO & Volume)
- `tpo.vah`, `tpo.poc`, `tpo.val`, `tpo.totalTpos`.
- `vol.vah`, `vol.poc`, `vol.val` (nullable if not computed).
- `valueVsYesterday` ∈ {{Higher, Lower, Overlapping, Inside, Outside}}.

### 2.7 Profile Features
- `dayType` ∈ {{TrendUp, TrendDown, DoubleDistribution, Normal, NormalVariation, Neutral, NeutralExtreme, Balance}}.
- `shape` ∈ {{P, b, D}}.
- `morph` (from,to) for: P→B, b→B, D→P, D→b, Normal→Trend, Neutral→Extreme, Trend→DD.

### 2.8 Excess / Tails
- `excess.poorHigh` (bool), `excess.poorLow` (bool).
- `spike.present` (bool), `spike.base` (price), `spike.dir` ∈ {{Up, Down}}.
- `otf.up` (bool), `otf.down` (bool), `otf.frame` ("30m"/"15m").
- `prominentPoc` (bool), `revisitMagnets` (array of levels).

### 2.9 dPOC (15‑minute tracking from RTH open)
- `dpoc.current`, `dpoc.start`.
- `dpoc.path15m` = array of `[timestamp, price]` (use ET strings).
- `dpoc.migrationTicks`, `dpoc.migrationSpeedTicksPerHr`, `dpoc.stabilityRatioLast4`.
- `dpoc.jump` ∈ {{None, Up, Down}}.

### 2.10 HVN/LVN arrays
- `hvns.above[]` / `hvns.below[]` (price, prominence, method=TPO/Volume, distanceTicks).
- `lvns.above[]` / `lvns.below[]` (same fields). Identify **multiple** nodes with spacing + prominence thresholds.

### 2.11 RTH Gap Close
- `rthGap.dir` ∈ {{Up, Down, None}}, `rthGap.sizeTicks`.
- `rthGap.closeTimeEt` (nullable), `rthGap.closeElapsedMin`.
- `peers.gapClosed[]` (per observer) + earliest peer close time.

### 2.12 Balance Box & 15m Overlay
- `balance.top`, `balance.bottom`, `balance.inside` (bool).
- `overlay15.acceptEarlyAtIb` (bool), `overlay15.rejectEarlyAtIb` (bool).

### 2.13 Cross-Index Evidence / Relative Strength
- `rs.sinceOpenPct` for NQ/ES/YM, `rs.rank` (1 best → 3 worst).
- `divergence.flags` (e.g., `nqUpPeersDown15m`, `nqDownPeersUp15m`).

---

## 3) JSON Snapshot Spec
- **Directory:** `...\TPOAddon\metrics\`
- **Per-instrument file:** `metrics_{{SYMBOL}}_{{YYYYMMDD_HHmm}}.json`
- **Combined latest:** `metrics_latest.json` containing `{{ primary: <snap>, observers: [<snap>, ...], generatedAtEt: "..." }}`
- **Cadence:** every **5 minutes** aligned to the clock.
- **Retention:** keep per-day files; overwrite `metrics_latest.json` each cycle.
- **Encoding:** UTF‑8; one JSON object per file (no trailing commas).

**Schema (abridged; include all fields from §2):**
```json
{{
  "schemaVersion": "tpo.metrics.v9_4_4",
  "symbol": "NQ 12-25",
  "role": "Primary",
  "tsUtc": "2025-09-29T16:35:00Z",
  "tsEt": "2025-09-29T12:35:00",
  "session": {{ "rthPhase": "RTH", "sinceOpenMin": 125, "tickSize": 0.25, "adr20": 300.0 }},
  "opening": {{ "location": "OutsideValue", "type": "OTD" }},
  "gap": {{ "type": "Outside_Prior_Range", "sizeTicks": 48, "sizePctAdr": 0.16, "fillPct": 60, "timeToFirst50FillMin": 22, "fillCompleted": false }},
  "onInventory": {{ "state": "Too_Short", "correctionRisk": true }},
  "ib": {{ "high": 15875.25, "low": 15798.00, "sizeTicks": 309, "re": {{ "direction": "Up", "countUp": 2, "countDown": 0 }},
           "acceptance": {{ "tier": "Provisional15m", "elongation": true, "valueDrift": true }} }},
  "value": {{
    "tpo": {{ "vah": 15862.00, "poc": 15840.00, "val": 15810.00, "totalTpos": 214 }},
    "vol": {{ "vah": null, "poc": null, "val": null }},
    "vsYesterday": "Higher"
  }},
  "profile": {{ "dayType": "NormalVariation", "shape": "P", "morph": {{ "from": "", "to": "" }},
               "excess": {{ "poorHigh": false, "poorLow": true }}, "spike": {{ "present": false, "base": null, "dir": null }},
               "otf": {{ "up": true, "down": false, "frame": "30m" }}, "prominentPoc": false, "revisitMagnets": [] }},
  "dpoc": {{ "current": 15840.00, "start": 15790.00,
            "path15m": [["2025-09-29T10:00:00","15805.00"],["2025-09-29T10:15:00","15812.00"]],
            "migrationTicks": 200, "migrationSpeedTicksPerHr": 48, "stabilityRatioLast4": 0.75, "jump": "Up" }},
  "nodes": {{
    "hvns": {{ "above": [{{"price":15890.00,"prominence":0.8,"method":"TPO","distanceTicks":200}}], "below": [] }},
    "lvns": {{ "above": [], "below": [{{"price":15820.00,"prominence":0.6,"method":"TPO","distanceTicks":80}}] }}
  }},
  "rthGap": {{ "dir": "Up", "sizeTicks": 48, "closeTimeEt": null, "closeElapsedMin": 22,
              "peers": {{ "gapClosed": [{{"symbol":"ES 12-25","closed":true,"timeEt":"12:12"}},{{"symbol":"YM 12-25","closed":false}}] }} }},
  "balance": {{ "top": 15870.00, "bottom": 15815.00, "inside": true }},
  "overlay15": {{ "acceptEarlyAtIb": true, "rejectEarlyAtIb": false }},
  "rs": {{ "sinceOpenPct": {{"NQ": 0.62, "ES": 0.18, "YM": -0.05}}, "rank": 1, "divergence": ["nqUpPeersDown15m"] }}
}}
```

---

## 4) UI (minimal for v2.5)
- Append a **5‑minute summary line** per instrument to commentary, e.g.:
  - `12:35 NQ | Open:OTD | IB:Up (prov) | Shape:P | dPOC↑ stable | Nearest HVN 15900 | GapClose: peers ES closed @ 12:12`
- No new tabs yet (will arrive in v2.6).

---

## 5) Scheduler & Lifecycle
- Add `MetricsSampler` that triggers every 5 minutes aligned to clock edges (e.g., 09:35, 09:40, …).
- On Stop(): dispose timers, flush any pending file writes, close rolling logs cleanly.

---

## 6) Performance & Safety
- Avoid heavy allocations inside the 5‑minute tick; reuse DTOs where possible.
- File writes should handle IO exceptions and continue.
- If a metric depends on missing refs (e.g., prior volume VA), write `null` and include a warning in log; do not fail the whole snapshot.

---

## 7) Deliverables
- `MetricsEngine.cs`, `MetricsSnapshot.cs` (DTOs), `MetricsSampler.cs`.
- Extensions to existing `DataEngine` if needed to expose bars/states to MetricsEngine.
- JSON output as specified; UI commentary lines every 5 minutes.
- **Unit-test stubs** (lightweight) for HVN/LVN scan, dPOC migration calc, gap fill calculations (pure functions).

---

## 8) Notes
- Preserve all v2.4 logging/heartbeat behaviors.
- Maintain CS5 safety in **all** new code.
- Do not implement Playbook in this version (placeholder YAML allowed but unused).
