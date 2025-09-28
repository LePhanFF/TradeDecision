# Prompt: “Markets in Profile (Dalton) — 100+ MCQ Test → test”

**Role:** You are an expert Market Profile educator and assessment designer.

**Goal:** Generate a **single plain-text file** named `test` containing **≥100 multiple-choice questions** based on James Dalton’s *Markets in Profile: Profiting from the Auction Process*.

---

## Inputs (load first)
- Primary source PDF (if available): `/mnt/data/James_Dalton-Markets_in_Profile-EN.pdf`
- Use the PDF’s visible page numbers for citations. If front matter uses Roman numerals, cite the page number as displayed in the PDF viewer.

---

## Coverage & Counts
- Total items: **≥100** (target 100–120).  
- Difficulty mix: ~30% Easy, ~50% Medium, ~20% Hard.
- Distribute questions across the book’s core themes, for example:
  - Market-generated information; value vs. price; fair value construction
  - Value area (≈70% of TPOs), POC, VAH/VAL; migration of value/POC
  - Timeframes (scalper, day, short, intermediate, long) and interactions
  - Auction states: balance vs. trend; transitions; excess; poor highs/lows
  - Initial Balance (IB), Range Extension (RE), Attempted Direction
  - Day types (P, b, D, B) and one-timeframing
  - Open types: Open-Drive (OD), Open-Test-Drive (OTD), Open Rejection Reverse (ORR), Open Auction (OA)
  - Inventory imbalance; short covering vs. long liquidation
  - Bracketing mechanics; breakout/failed breakout context
  - Preparation: top-down vs. bottom-up, reference mapping, context building
  - Symmetry, single prints, HVN/LVN, acceptance vs. rejection

---

## Item Format (apply to every question)
- **ID:** `B-###` (zero-padded, e.g., `B-001`).
- **Difficulty:** `Easy | Medium | Hard`.
- **Stem:** concise, precise, no absolutes like “always/never” unless the source is explicit.
- **Choices:** A–D, **four plausible options**; **randomize order** for each item.
- **Correct:** show the **letter + exact text** of the right option.
- **Why:** 1–3 lines explaining the reasoning grounded in Dalton’s framework.
- **Evidence:** Footnote citing the book:  
  `[MiP — Chapter/Section, page #, brief supporting quote or paraphrase]`  
  - Keep quotes short (≤25 words) or paraphrase faithfully.

---

## Style & Quality Rules
- Use common confusions for distractors (e.g., IB vs. full range; excess vs. poor highs; momentum vs. acceptance; price vs. value).
- Avoid “All of the above/None of the above” except in rare, justified cases.
- Maintain even distribution of correct-answer positions across A–D.
- Prefer practical, context-driven stems over rote definitions when possible.
- No tables/YAML/JSON in output; plain text only.

---

## Output Contract
- Produce **one** block only, delimited exactly like this:

```
BEGIN FILE: test
[ID] B-001 (Easy)
Q: ...
A) ...
B) ...
C) ...
D) ...
Correct: B) ...
Why: ...
Evidence: [MiP — ..., p. ...]

[ID] B-002 (Medium)
...

... (continue until ≥100 items) ...
END FILE
```

- Do **not** output anything outside the `BEGIN FILE: test` … `END FILE` block.

---

## Example (one item only; do not include this header in the final file)
```
[ID] B-001 (Easy)
Q: In Dalton’s framework, “value area” is best described as:
A) The first hour’s range
B) The range containing approximately 70% of TPOs around the POC  ← CORRECT
C) The session’s midpoint only
D) The highest volume price
Correct: B) The range containing approximately 70% of TPOs around the POC
Why: Value is defined by acceptance; the ~70% TPO band around POC distinguishes value from mere price movement.
Evidence: [MiP — Market Profile fundamentals, p. 23–26]
```

---

**Ready.** When you run this, it will emit a standalone `test` file with ≥100 MCQs, each with randomized choices, the correct answer, a brief rationale, and a proper *Markets in Profile* footnote.
