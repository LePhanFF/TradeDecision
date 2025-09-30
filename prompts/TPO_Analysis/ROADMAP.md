# TPO Roadmap — v9.5.x Planning
Generated: 2025-09-29 20:38

## Goals
Deliver a complete Dalton-informed, real-time TPO assistant for NinjaTrader with **15-minute cadence**, correct **open-type filtering**, and **actionable triggers** (entries/stops/targets/alerts).

## Buckets & Features

### A) Day-Type Completeness
- **Neutral / Neutral Extreme** ✅ (added in 9.4.10; refine scoring & next-day bias tuning)
- **Non-Trend / Inside Day** ⏳ add module (fade IB edges; prepare breakout next session)
- **Normal Variation** ⏳ sub-variant under Balance (single clean extension beyond IB)
- **Double Distribution Trend (DDT)** ✅ (added in 9.4.10; add measured-move projection helper)

### B) Open Types (high priority for 15m cadence)
- **Opening Drive (OD)**: detection + “do not fade” banner + continuation entries
- **Open Test Drive (OTD)**: early test/reject logic
- **Open Auction In Range (OAIR)**: balance filter (fade edges)
- **Open Rejection Reverse (ORR)**: gap-fill play
- **Prompt rule:** classify by **10:00 ET**; re-evaluate at **10:30 ET**

### C) Overlays & Repairs
- **Poor High/Low** ✅ detection + same-day repair tracking + carry-forward
- **Excess** ✅ bias/stop anchor + carry-forward
- **Single Print Repairs** ⏳ intra-day “repair trade” module
- **Half-Back (50% range) test** ⏳ quick confluence marker in balance days
- **Weekly references** ⏳ prior week POC/VAH/VAL/HVN integration with alerts

### D) VWAP & Morph Intelligence
- **VWAP Integrity (Trend/DDT)** ✅ 0–1 touches filter
- **P/b Early Invalidation** ✅ (fail/reclaim VWAP + swing criteria)
- **B→D Seam Fill** ✅ seam_crosses & time_in_seam threshold
- **SMT divergence** ⏳ cross-index confirmation (ES/NQ/YM) at extremes
- **Imbalance edge tests** ⏳ ON single prints / imbalance highs/lows

### E) Gaps (expanded)
- **Gap & Go** ✅ rules + alert
- **Partial Fill → Balance** ✅
- **Full Gap Fill** ✅ (your RTH gap fill)
- **ORR Gap Fill** ✅
- **Next-day gap sequencing** ⏳ (gap after Normal/Inside day)

### F) Ninja Add‑On Integration
- **Schema extensions:** consider `schema_version 1.2` for open-type banners, measured-move projections, SMT/peer signals.  
- **Alert routing:** audible categories (Trend warning, Gap magnet, Morph).  
- **Layer ordering:** zones under lines under annotations; highlight active play.  
- **User toggles:** enable/disable modules (VWAP, overlays) per day-type.  

### G) QA/Regression
- **Fixture sessions**: curate 10 sessions per day-type for unit tests.  
- **Consistency checks**: seam fill metric reproducibility; VWAP touch counts per day-type.  
- **Latency budget**: render + compute < 1s per 15m run.

## Candidate Versions
- **v9.5.0**: Open Types, Inside Day, Normal Variation, Single Print Repair, Weekly references, measured-move helper for DDT.  
- **v9.5.1**: SMT divergence engine, imbalance edge tests, schema v1.2.  
- **v9.5.2**: Alert tuning & backtest stats, user toggles/presets.

