# ROADMAP.md
NinjaTrader 8 AddOn + TPO Integration Roadmap
Status as of AddOn Prompt v2.3 (2025-09-29 03:28 )

---

## ✅ Phase 1: Stabilization
*Goal: Get the AddOn compiling and running safely.*

- [x] Remove tuple generics → replace with Core DTOs (`Node`, `SinglePrint`, `DpocPoint`, `ProfileSnapshot`).
- [x] Fix mangled `Start()` → canonical dispatcher-safe pattern.
- [x] Add back `ClockEt.ToEt` + `HM`.
- [x] Remove `NTWindow.MainWindow` dependency.
- [x] UpdateHeader uses `menuItem.Dispatcher` with shutdown guards.
- [x] UiHostWindow updates only on its Dispatcher.
- [x] SafeOut hardened.
- [x] Instrument resolution from `instruments.txt` (defaults: ES/NQ/YM front-month).
- [x] UI dark theme (background #1E1E1E, text #E0E0E0).
- [x] Fonts standardized (Segoe UI, Consolas in textboxes).
- [x] Professional accent colors (green/red/amber/orange).
- [x] Commentary/evidence styled for readability.
- [x] dPOC trail rendering (DeepSkyBlue stroke).
- [x] Postmortem + runtime notes documented.
- [x] Prompt guardrails (`PROMPT_CODEGEN_NT8` + `AddOn v2.3`).
- [x] Repo tools: `nt8_linter`, `nt8_autofix`, `nt8_postinstall_verify`.
- [x] Sample `instruments.txt` provided.
- [x] Optional git pre-commit hook.

**✅ Phase 1 Complete.**

---

## 🔜 Phase 2: Feature Expansion
*Goal: Move beyond stability to trader-facing functionality.*

- [ ] Multi-index support beyond ES/NQ/YM (support arbitrary list in config).
- [ ] Real-time playbook integration (active plays displayed in UI).
- [ ] Journaling hooks (auto-log trades/metrics to JSON/CSV).
- [ ] Tabbed UI layout (Summary, Metrics, Logs).
- [ ] Expandable evidence/detail panels.
- [ ] JSON/raw data viewer in UI for debugging.
- [ ] Distribution: installer/package flow for AddOn + config.
- [ ] Dashboard for health checks (connected symbols, peer coherence).

---

## 🛠️ Phase 3: Advanced Workflow
*Goal: Integrate AddOn with broader trading workflow + automation.*

- [ ] Signal integration: playbook outcomes trigger UI/log alerts.
- [ ] Export journaling to Notion/Google Drive.
- [ ] Hook external analysis (TPO v9.x, SMT scoring).
- [ ] Integrate with evaluation account dashboards (risk/scaling).

---

📌 **Current Status:**
- Phase 1 complete.
- Ready to begin **Phase 2 (feature expansion)**.
