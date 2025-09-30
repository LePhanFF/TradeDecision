# ROADMAP.md
NinjaTrader 8 AddOn — Project Roadmap
Status updated: 2025-09-29 04:23 

---

## ✅ Phase 1 — Stabilization (v2.3)
- [x] Compile stability (no tuples; DTOs only).
- [x] Canonical Start/Stop (dispatcher-safe).
- [x] Runtime safety (UpdateHeader/UiHostWindow via Dispatcher).
- [x] Safe logging wrapper.
- [x] Instruments via `instruments.txt` (fallback contracts).
- [x] Professional dark theme UI.

## ✅ Phase 2 (Internal Enhancements) — v2.4
- [x] Logging infrastructure (`TpoLogger`) → Output tab + rolling file.
- [x] UI thread helpers (`UiThread`) → no direct `Application.Current.Dispatcher` in app code.
- [x] Heartbeat (30s liveness log).
- [x] Auto front-month resolution (`DataSubscriptions` for `##-##`).
- [x] CS5 compatibility enforced.

---

## 🚧 Phase 2 — v2.5 (Metrics Foundation + Multi-Index)
**Goal:** Build the **foundation**: multi-index wiring and **complete TPO metrics** (v9.4.4 superset) with periodic JSON snapshots and on-screen summaries.

### Scope
- [ ] **Multi-index config**
  - [ ] `instruments.txt` supports: `primary=NQ 12-25`, `observers=ES 12-25,YM 12-25` (comma-separated).
  - [ ] If only a list is provided, treat **first** as primary; else default `primary=NQ` and best-effort resolve others.
  - [ ] Resolve `##-##` placeholders to current front month.

- [ ] **Metrics engine (v9.4.4 superset)**
  - [ ] Opening context (location vs prior value/range; OpeningType OD/OTD/ORR/OA).
  - [ ] Gap module (type, size in ticks/%ADR, fill %, time to 50% fill, completed?).
  - [ ] Overnight inventory (Too Long/Too Short/Balanced; correction risk flag).
  - [ ] IB/RE (IB H/L/size; RE dir; acceptance: 15m provisional → 30m confirmed).
  - [ ] Value areas & POC (TPO; Volume if available) + relation vs prior day (Higher/Lower/Overlapping/Inside/Outside).
  - [ ] Profile features: Day Types (Trend, DD, Normal, Normal-Variation, Neutral, Neutral-Extreme, D/Balance).
  - [ ] Shapes: P, b, D + morphs (P→B, b→B, D→P, D→b, Normal→Trend, Neutral→Extreme, Trend→DD).
  - [ ] Excess & Poor High/Low; Spike & Spike Base; One-timeframing; Prominent POC; Revisit magnets.
  - [ ] dPOC tracking (15m path since RTH open; migration speed; stability; jump up/down).
  - [ ] HVN/LVN arrays (multiple nodes above/below with prominence & distance).
  - [ ] RTH Gap Close (dir/size/close time/elapsed; peers status).
  - [ ] Balance box (top/bottom; inside flag).
  - [ ] 15m overlay (early accept/reject at IB extremes; provisional→confirmed upgrade path).
  - [ ] Cross-index evidence (peer gap close; relative strength).

- [ ] **Snapshots & UI**
  - [ ] Every **5 minutes** (aligned to clock), write `metrics_{{YYYYMMDD_HHmm}}.json` per instrument and a combined `metrics_latest.json`.
  - [ ] Append a human-readable **one-line summary** per instrument to the UI commentary every 5 minutes (key fields: OpeningType, IB/RE state, Shape/DayType, dPOC state, HVN/LVN nearest, GapClose status).

- [ ] **Relative strength / peer context**
  - [ ] For each 5‑min snapshot: compute primary (NQ) **relative return since RTH open** vs ES/YM; rank RS (1 strongest → 3 weakest).
  - [ ] Peer gap-close status array and earliest close time.
  - [ ] Flags for divergence (e.g., NQ up while ES/YM down within last 15 min).

### Acceptance criteria
- [ ] Compile on NT8 (CS5 safe).
- [ ] Metrics JSON validates against schema; includes **all fields** listed above (or null if unavailable).
- [ ] UI commentary shows **fresh line every 5 min** per instrument.
- [ ] Combined snapshot includes primary + observers; RS rank present.
- [ ] No blocking calls on UI thread; timers safe to stop on AddOn Stop().

---

## 📌 Phase 3 — v3.0 (Playbook & Alerts)
- [ ] Simple **YAML playbook** (conditions → labels, bias, triggers) referencing v9.4.4 fields.
- [ ] Engine to evaluate conditions and produce active play list.
- [ ] UI “Active Plays” panel + optional alerts.

## 🧭 Later
- [ ] Journaling auto-export (CSV/JSON; Notion/Drive adapters).
- [ ] Tabbed UI (Summary • Metrics • Logs), expandable sections.
- [ ] Dashboard (connections, peer coherence, latencies).
