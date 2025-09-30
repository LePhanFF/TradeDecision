# TPO Playbook — v9.4.10 (Consolidated)
Generated: 2025-09-29 20:38

## What’s in this version
- **Day-types:** Balance (**D/Normal**), **P**, **b**, **B**, **Trend**, **Double Distribution Trend (DDT)**, **Neutral / Neutral Extreme**.
- **Overlays:** **Poor High/Low** (unfinished auction), **Excess** (secure extreme).
- **Morph logic:** P→B, b→B, **B→Trend** (seam acceptance + dPOC migration), **B→D** (seam fills; time in seam), **Balance→Trend** (acceptance outside VA), **Neutral Extreme** bias next session.
- **VWAP usage map:** *Use for Trend & P/b invalidation; do **not** use for B or Balance (D).* 
- **RTH gap play integration:** Full/partial gap fill, Gap & Go, ORR gap-fill with morph ties.
- **Ninja Add-on JSON schema v1.1**: zones/lines/targets/annotations/alerts with clear IDs.
- **Auto-display rules:** Print relevant playbook **at open** and again **post-morph** with updated entries/exits and draw-plan.

---

## 1) Day-Type Modules

### 1.1 Balance (D) — includes **Normal** subtype
**Detection:** POC ~ center of VA; symmetric rotations; IB often contains most trading.  
**Normal subtype:** DayRange ≤ ~1.1× IBRange; shallow LVN pokes quickly rejected.  
**Setups:** Fade **VAH/VAL** on rejection → **T1 POC**, **T2 opposite edge**.  
**Invalidation:** ≥2 closes **outside VA** with time building (morph **Balance→Trend**); or twin nodes with seam (morph **Balance→B**).  
**Notes:** For Normal subtype, add **carry-forward: breakout risk next session**.

### 1.2 P (upper skew)
**Detection:** POC in lower half of VA; VA skewed up; IBH extension dominates.  
**Setups:**  
- **Upper-edge fade (short):** Rejection at **VAH/IBH/upper HVN** → **T1 POC**, **T2 VAL**.  
- **Acceptance continuation (long):** ≥2 closes **above VAH** + dPOC up → buy pullback holding above edge.  
**Early invalidation (VWAP module):**  
- 1st VWAP test = healthy bounce; **2nd weak bounce** (≤20–30% swing) = **warning**;  
- **Close < VWAP & below last swing** = **invalidate P; morph risk to B/trend down**.

### 1.3 b (lower skew)
**Detection:** POC in upper half of VA; VA skewed down; IBL extension dominates.  
**Setups:**  
- **Lower-edge fade (long):** Rejection at **VAL/IBL/lower HVN** → **T1 POC**, **T2 VAH**.  
- **Acceptance continuation (short):** ≥2 closes **below VAL** + dPOC down → sell pullback failing below edge.  
**Early invalidation (VWAP module):**  
- If **close > VWAP & > last swing high** (reclaim) = **warning**;  
- **2 closes > VWAP & > POC + dPOC up** = **invalidate b; morph risk to B/trend up**.

### 1.4 B (double distribution)
**Detection:** ≥2 HVNs separated by LVN seam (≥20% VA width separation) or skew fallback.  
**Setups (fade logic):** Short **upper node edge** on rejection; Long **lower node edge** on rejection. **T1:** seam/POC; **T2:** opposite node edge.  
**Morphs:**  
- **B→Trend:** Seam breaks with acceptance (**≥2 closes** beyond) + **dPOC migration** into breakout node.  
- **B→D (Balance):** **Seam fills** (≥2 traversals + **time in seam ≥20%** of session) → nodes merge → trade as Balance.  
**VWAP:** Not reliable in B (often sits in seam). Focus on **nodes & seam**.

### 1.5 Trend Day (up or down)
**Detection:** IB break **holds**; single prints stack; 30m **one‑timeframing** (HH/HL up or LH/LL down); **dPOC migrates** with price; 0–1 VWAP touches.  
**Setups:**  
- **IB Break Entry:** trade with breakout; stop just inside IB.  
- **Pullback Add‑on:** shallow pullback to single prints or VWAP; stop beyond pullback swing.  
**Invalidation:** Single prints fill fast; **2 closes back inside IB**; dPOC stalls away from move.  
**Warning banner:** *“Trend Day risk — avoid fading VAH/VAL; continuation only.”*

### 1.6 Double Distribution Trend (DDT)
**Detection:** Morning trend leg breaks IB; **midday pause node** forms; **afternoon seam break** resumes trend.  
**Setups:**  
- **Morning:** IB breakout as in Trend.  
- **Afternoon:** Enter **pause‑node seam break** in trend direction; stop inside pause node; **target = leg‑1 projection**.  
**Invalidation:** Afternoon breakout fails back into pause node; dPOC anchors inside pause node → downgrade to Balance.

### 1.7 Neutral / Neutral Extreme
**Detection:** **Both IBH & IBL** broken.  
- **Neutral (classic):** close near mid → **fade extremes to POC**.  
- **Neutral Extreme:** close near an extreme → **align with close side** (carry-forward continuation bias).  
**Invalidation:** Only one IB side broken → not Neutral; or close drifts back to mid = Neutral classic.

---

## 2) Overlays

### 2.1 Poor High / Poor Low (unfinished auction)
**Detect:** ≥2 TPOs at session high/low; no excess tail.  
**Trade:** Expect **repair** (retest, often exceed) same day or next.  
**State:** mark `repaired_same_day` or `carry_forward` at close.

### 2.2 Excess (secure extreme)
**Detect:** clear single‑print tail (≥2 prices) at extreme.  
**Trade:** **Trade away** from excess; use extreme as **natural stop**; carry forward as bias if unretested into close.

---

## 3) VWAP Usage Map
- **Trend / DDT:** **Integrity filter** (0–1 touches).  
- **P / b:** **Early invalidation** (defense fail/reclaim).  
- **B / Balance (D):** **Do not use**; rely on nodes, seam, VA edges.

---

## 4) Gap Play Integration
- **Gap & Go:** open outside prior VA/range; no return → Trend bias.  
- **Partial fill → Balance:** fill to ON POC or VA edge, stall.  
- **Full gap fill (your RTH rule):** opening drive fails → rotate to **prior close**.  
- **ORR gap fill:** gap open, push fails, **reversal back inside** → magnet is prior close.  
**Morph ties:** P→B or b→B transitions frequently **target gap fill**.

---

## 5) Execution (real‑time triggers)
- **Fail‑back rule:** act only on **rejection** (poke + 1–2 bar close back) or **acceptance** (≥2 closes beyond).  
- **Stops:** structure‑based (beyond rejection wick/swing).  
- **Targets:** POC/seam first; then VA edge / opposite node / gap close.  
- **Post‑morph:** Immediately print **new playbook** section and **refresh draw‑plan**.

---

## 6) Ninja Add‑On JSON schema (v1.1) — summary
```json
{
  "schema_version":"1.1",
  "version":"v9.4.10",
  "session_date":"YYYY-MM-DD",
  "instrument":"SYMBOL",
  "vah":0.0, "poc":0.0, "val":0.0,
  "ib_high":0.0, "ib_low":0.0,
  "hvn":[{"price":0.0}], "lvn":[{"price":0.0}],
  "zones":[
    {"id":"short_edge","type":"zone","price_min":0.0,"price_max":0.0,"label":"Upper HVN/IBH fade (reject only)","priority":2},
    {"id":"short_poc","type":"zone","price_min":0.0,"price_max":0.0,"label":"POC fail-back","priority":1},
    {"id":"short_val","type":"zone","price_min":0.0,"price_max":0.0,"label":"VAL break-retest","priority":0},
    {"id":"long_edge","type":"zone","price_min":0.0,"price_max":0.0,"label":"Lower HVN/IBL fade (reject only)","priority":2},
    {"id":"long_poc","type":"zone","price_min":0.0,"price_max":0.0,"label":"POC fail-back (long)","priority":1},
    {"id":"long_vah","type":"zone","price_min":0.0,"price_max":0.0,"label":"VAH break-retest (long)","priority":0}
  ],
  "lines":[
    {"type":"line","price":0.0,"style":"dashed","label":"VAH"},
    {"type":"line","price":0.0,"style":"solid","label":"POC"},
    {"type":"line","price":0.0,"style":"dotted","label":"VAL"},
    {"type":"line","price":0.0,"style":"dashdot","label":"IBH"},
    {"type":"line","price":0.0,"style":"dashdot","label":"IBL"}
  ],
  "targets":[
    {"id":"T1","price":0.0,"label":"First scale"},
    {"id":"T2","price":0.0,"label":"Second scale"},
    {"id":"T3","price":0.0,"label":"Stretch"}
  ],
  "annotations":[
    {"shape":"text","price":0.0,"label":"Wait for fail-back confirmation"},
    {"shape":"flag","price":0.0,"label":"Invalidate if 2 closes beyond"}
  ],
  "alerts":[
    {"id":"ALERT_PDAY_VWAP_FAIL","when":"close<VWAP & close<last_swing","message":"P failing — morph risk to B/trend"},
    {"id":"ALERT_BDAY_SEAM_BREAK","when":"acceptance_beyond_seam & dPOC_migrate","message":"B→Trend risk"},
    {"id":"ALERT_BDAY_SEAM_FILL","when":"seam_crosses>=2 & time_in_seam>=0.2","message":"B→Balance (seam filling)"},
    {"id":"ALERT_NEUTRAL_EXTREME","when":"ib_high_broken & ib_low_broken & close_near_extreme","message":"Neutral Extreme — continuation bias"},
    {"id":"ALERT_DDT_PAUSE_BREAK","when":"afternoon_break_in_trend & seam_confirmed","message":"DDT continuation"},
    {"id":"ALERT_GAP_FULLFILL","when":"gap_open & open_failed & price_to_prior_close","message":"Gap fill magnet active"},
    {"id":"ALERT_EXCESS_HIGH","when":"tail_length>=2 & location=high","message":"Excess high — short bias back to POC"},
    {"id":"ALERT_POOR_HIGH","when":"flat_top_TPOs>=2","message":"Poor High — repair watch"}
  ]
}
```

---

## 7) Changelog (v9.4.10)
- Added **Trend** and **DDT** modules with execution rules and alerts.
- Added **Neutral / Neutral Extreme** module.
- Integrated **VWAP early invalidation** for **P** and **b**; added **VWAP usage map** (skip for B & Balance).
- Added **B→D seam-fill** rule and alert; clarified **B→Trend** rule (seam + dPOC).
- Unified naming: **Balance (D)**; removed duplicate D entry.
- Expanded **Gap Playbook** with **Gap & Go**, **Partial**, **Full Fill**, **ORR**.
- Added **Poor High/Low** & **Excess** overlays with carry-forward flags.
- Ensured **auto-display at open** and **post-morph** refresh with draw-plan JSON.

---

## 8) Display Rules
- **At 09:30–10:00 ET:** Print **day-type hypothesis** and matching playbook.  
- **On morph:** Immediately append **new playbook section** + emit **updated draw-plan JSON**.  
- **Every 15 min run:** Update **morph log**, **gap status**, **VWAP modules (only for Trend/P/b)**, **peer index notes**.

