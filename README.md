# TradeDecision-Prompts

Public repository for the **TradeDecision project**.  
This repo is the **source of truth** for prompt specifications, playbook docs, changelogs, and published outputs.  
(Private repos hold sensitive trade/psychology logs.)

---

## 📑 Contents

### 1. Prompts
- **TPO Analysis**
  Auction Market Theory + TPO/Volume Profile analysis prompts.
  Versions: `v9.0`, `v9.1`

- **ICT Analysis**
  ICT-style liquidity setup analysis.
  Versions: `v1`.

- **Voice Tilt Analysis**
  Prompts for detecting trading tilt/emotional bias from transcripts/voice logs.
  Versions: `v1`.

- **Publish Manual**
  Prompts to generate guidebooks, quizzes, and educational content.
  Versions: `v1`.

- **MultiAccountDCA**
  Prompts to generate a NinjaTrader multi account virtual DCA
  Versions: `v1`.

- **NinjaDataExport**
  Prompts to generate NinjaTrader Future data feed
  Versions: `v1`.
---

### 2. Playbook
Living knowledge base built over trades:  
- **strategies/** → entry/exit rules, scaling, invalidation logic.  
- **evidence/** → annotated examples from real trading.

---

### 3. Changelogs
- **TPO.md** → evolution of Dalton TPO Analysis prompts.  
- **ICT.md** → ICT prompt changes.  
- **VoiceTilt.md** → tilt detection prompt changes.  

---

### 4. Docs
- **guidebook/** → published manuals (e.g. *Dalton TPO V10 Guidebook*).  
- **diagrams/** → TPO profile diagrams, day type schematics, auction theory visuals.

---

## 🔄 Workflow

1. Prompts are **immutable** once tagged (`v9.0`, `v9.1`, `v9.3`).  
2. Any upgrade → create new file (`v9.4.md`) + update changelog.  
3. Playbook grows continuously from live trading evidence.  
4. Guidebooks and diagrams exported into `/docs`.  

---

## ⚖️ License
This repo is public for **educational reference only**.  
Use prompts at your own discretion in live trading.

