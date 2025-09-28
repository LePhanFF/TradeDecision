# TPOv943 NinjaAddon Development Roadmap v1.0

## 📍 Current Stage
- Prototype foundation (**Phase 1**) complete: core metrics, JSON/Journal outputs, minimal UI.  
- Testing in progress (user validation).  

---

## 🚦 Roadmap Phases

### **Phase 1 — Prototype foundation (✅ Completed)**
- [x] Live data ingestion (5m bars, ES/NQ/YM, BarsRequest).  
- [x] Session management (RTH segmentation 09:30–16:00 ET).  
- [x] Auction metrics core (POC, VAH/VAL, HVN/LVN, IB, singles, poor highs/lows).  
- [x] Context: Opening type, gap logic.  
- [x] Developing metrics: dPOC tracking + 15m trail.  
- [x] One-timeframing (5m).  
- [x] JSON + human-readable journal outputs.  
- [x] UI: bias flag, day-type, evidence box, dPOC graph, morph warning.  

---

### **Phase 2 — Metric completeness (⏳ Next Up)**
- [ ] Full dynamic confidence scoring (weighted 100-pt system).  
- [ ] Morphology engine: initial → current → evidence state machine.  
- [ ] One-timeframing detection (30m).  
- [ ] Volume profile hooks (optional).  
- [ ] Carry-forward references (prior-day VAH/VAL/POC, settlement).  

---

### **Phase 3 — Playbooks & education**
- [ ] PlaybookEvaluator module:  
  - Gap-and-Go  
  - IB breakout continuation/failure  
  - Failed Auction  
  - Double Distribution (B-day continuation)  
- [ ] Playbook outputs → JSON `{playbooks:[...]}`.  
- [ ] Commentary lines in journal + UI.  
- [ ] Educational snippets (Dalton coaching notes).  

---

### **Phase 4 — Visualization polish**
- [ ] UI: Playbook panel, confidence gauge, session summary panel.  
- [ ] Indicator: file watcher overlay for VAH/VAL/POC/IB.  
- [ ] Exportable profile charts (ASCII/heatmap).  

---

### **Phase 5 — Ecosystem integration**
- [ ] JSON consumption by NT indicators + TradingView dashboards.  
- [ ] Alerts: morph shift, playbook triggers.  
- [ ] Historical backfill/replay support.  

---

### **Phase 6 — Refinement & scaling**
- [ ] Performance optimizations (incremental building).  
- [ ] Configurable settings (symbols, templates, toggles).  
- [ ] Advanced analytics (weekly/monthly composites).  
- [ ] Packaging: polished Add-On distribution (installer + docs).  

---

## 🔖 Versioning
- **v1.0**: Baseline roadmap.  
- Each roadmap update will increment (v1.1, v1.2, …) with new scope, reprioritization, or completed items moved forward.  
