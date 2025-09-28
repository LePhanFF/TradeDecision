# Role
You are an expert Market Profile educator and editor.

# Objective
Generate a SINGLE plain-text README handbook for **TPO Analysis v9.4.3 (FULL)**. 
This is a complete textbook-style document for beginners (8th-grade readability), covering:
- Key Dalton concepts
- ALL v9.4.3 metrics (no omissions)
- Full day-type taxonomy
- Dynamic confidence scoring (every component)
- EVERY playbook (each as a full scenario card, one by one)
- JSON/Human output explanation
- Footnotes with Dalton page ranges

# Output Requirements (very important)
- Output **one continuous .txt** document (no images, no tables, no PDFs, no code fencing in the final content).
- **Fully expand EVERYTHING** (no outlines, no summaries, no “see above”).
- Plain English, mentor/coach tone, verbose & educational, with whitespace and clear headers.
- **Cite James Dalton’s _Markets in Profile_ by PAGE RANGES** whenever referencing concepts (e.g., “pp. 70–92”).
- ASCII-safe characters only (no smart quotes, em dashes, bullet glyphs that render as black boxes).

# Document Structure (use these section headers verbatim)
1) TPO ANALYSIS v9.4.3 — FULL TEXT HANDBOOK (NO REGRESSIONS VS v9.4.2)
   - Purpose paragraph and “learn more here” resource line (marketprofile.com)
2) Key Dalton Concepts (one paragraph EACH, with page-range citations)
   - Auction Theory (pp. 6–45)
   - Acceptance (pp. 70–92, 165–225)
   - Rejection (pp. 103–157)
   - Time vs Price vs Volume (pp. 45–92)
   - Value Migration (pp. 70–92, 165–225)
   - Market Generated Information (pp. 6–45)
   - Initial Balance Importance (pp. 70–92)
   - Excess (pp. 103–157)
   - Single Prints (pp. 45–92)
   - Poor Highs / Poor Lows (pp. 103–157)
   - One-Timeframing (pp. 199–225)
   - Day-Type Recognition (pp. 70–205)
   - Context (pp. 6–45, 165–225)
   - Confidence (pp. 192–202)
   - Inventory (pp. 178–205)
3) Metrics (v9.4.3 Superset) — repeat the following FOUR sub-bullets for EACH metric:
   - Definition (simple words)
   - Why it matters (Dalton theory + page ranges)
   - How it is computed (step by step)
   - How to use it in the playbook (trade implications)
   Metrics to cover (all, no omissions):
   - Opening Types (OD/OTD/ORR/OA)
   - Gap Module
   - Overnight Inventory
   - Initial Balance & Range Extension (IB/RE)
   - Value Areas & POC (TPO + Volume)
   - Shapes (P, b, D)
   - Excess & Poor High/Low
   - Spikes & Spike Base
   - One-Timeframing
   - Prominent POC
   - Revisit Magnets (single prints, b/p loops)
   - Developing POC (15m tracking)
   - HVN/LVN arrays (multiple nodes above/below)
   - RTH Gap Close
   - Balance Box & LAB/LBF
   - 15m Overlay (early acceptance/rejection)
   - Peer Gap Close (cross-index awareness)
4) Day-Type Taxonomy (pp. 70–205) — for EACH type include:
   - Definition
   - Who is in control (auction participants)
   - Why it matters (with page-range cite)
   - Trade implications
   Types:
   - Trend Day (Up/Down), Double Distribution Trend Day, Normal Day, Normal-Variation Day,
     Neutral Day, Neutral-Extreme Day, Non-Trend / Balance (D-Day),
     plus P-shape (short covering) and b-shape (long liquidation),
     and Morphs (P→B, b→B, D→P, D→b, Normal→Trend, Neutral→Extreme, Trend→DD)
5) Dynamic Confidence Scoring (0–100) — for EACH component include:
   - Definition
   - Why it matters (with page-range cite)
   - How it is computed (points/thresholds)
   - How to use it in playbooks
   Components (all):
   - Opening Type; RE Status; Value vs Yesterday; Volume Alignment; Day-Type Clarity;
     One-Timeframing; 30m vs 15m Confluence; Prominent POC; Spike Acceptance;
     Gap Context; dPOC; HVN/LVN; Peer Gap Close
6) Playbooks — Scenario Cards (ALL plays, one by one; each fully expanded)
   For EACH card include exactly these labeled lines:
   - What is the play (Context)
   - Why it matters (Dalton page ranges)
   - Metrics used for confirmation
   - Bias & Confidence (Strong/Moderate/Weak)
   - Ideal Entry
   - Risk / Invalidation
   - Sizing Guidance
   - Targets
   - Conviction (explain strength and why)
   Playbooks to include (no omissions):
   - Openings: Open-Drive (OD), Open-Test-Drive (OTD), Open-Rejection-Reverse (ORR), Open-Auction (OA)
   - IB/RE: IB Breakout Success; IB Breakout Failure → Balance
   - Gaps: Gap & Go Up; Gap & Go Down; Gap Fade Down from Gap Up; Gap Fade Up from Gap Down
   - RTH Gap Close: Fade Down (Gap Up); Fade Up (Gap Down)
   - Spikes: Next Day After Late Spike Up; Next Day After Late Spike Down
   - Shapes: P-day; b-day; D-day Balance
   - Morphs: P→B; b→B; D→P; D→b; Normal→Trend; Neutral→Extreme; Trend→DD
   - Extreme Tests: Look Above and Fail (LAB); Look Below and Fail (LBF)
   - HVN/LVN Plays: LVN Rejection → HVN; LVN Break & Accept → Next Node; HVN Rejection → Back to Balance
   - Risk Caution: Prominent POC Magnet
   - dPOC Relocations: dPOC Relocation Up; dPOC Relocation Down
7) JSON/Human Output — explain:
   - What JSON fields the system outputs (all metrics, sub-scores, playbooks, evidence)
   - How to read the Human Table (Opening, IB/RE, Value vs Yesterday, Shape, dPOC, HVN/LVN,
