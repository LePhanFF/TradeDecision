BUILD_NOTES_NT8.md
==================
Updated: 2025-09-28 23:57:45

Purpose
-------
This document records the hard-won lessons while stabilizing the TPO AddOn for NinjaTrader 8 (NT8).
It should live in the repo so anyone can rebuild or refactor without re-introducing the same errors.


1) WPF UI Thread Ownership (critical)
-------------------------------------
Symptom:
  "Unhandled exception: The calling thread cannot access this object because a different thread owns it."

Cause:
  Creating or touching WPF UI elements (Windows, Controls) from a non-UI thread.

Rules:
  - Create windows (UiHostWindow) on the GUI dispatcher thread.
  - Update controls only via Dispatcher on that window (Invoke / InvokeAsync).
  - Close windows on their dispatcher.

Example (AddOn Start - CORRECT):
  var d = Application.Current != null ? Application.Current.Dispatcher : null;
  if (d != null) d.Invoke(new Action(() =>
  {
      ui = new UiHostWindow();
      ui.Owner = NTWindow.MainWindow; // optional
      ui.Show();
  }));

Example (AddOn Stop - CORRECT):
  if (ui != null) ui.Dispatcher.BeginInvoke(new Action(() => ui.Close()));

Inside UiHostWindow.UpdateUi (CORRECT):
  this.Dispatcher.InvokeAsync(() => { /* touch controls here */ });


2) Avoid C# 7 tuple syntax in NT8
---------------------------------
Symptoms in compile logs:
  - "Type expected"
  - "Invalid token '(' in class struct or interface member declaration"
  - "'double' is a keyword"
  - "Expected class delegate enum interface or struct"

Cause:
  NT8 compiler is picky about C# 7 tuple syntax (List<(double,double)>, deconstruction, etc.).
  When it hits such constructs high in the file, the parser derails and reports nonsense errors later.

Policy:
  - Do NOT use tuple generics or tuple literals in collection Adds.
  - Use small DTO classes instead.

DTOs used:
  - Node { double Price, double Score }
  - SinglePrint { double Start, double End }
  - DpocPoint { DateTime Et, double Price }
  - ProfileSnapshot { Poc, Vah, Val, TotalTpos, List<Node> HVN, List<Node> LVN, List<SinglePrint> Singles, PoorHigh, PoorLow }

One definition only:
  Ensure there is a single definition of ProfileSnapshot and the DTOs in Core.
  Do not duplicate them in multiple files/namespaces.


3) Namespaces and 'using' hygiene
---------------------------------
- Files that reference DTOs must have:
    using NinjaTrader.NinjaScript.AddOns.Core;
- All classes must live inside a namespace (no stray top-level code).
- Keep '#region' / '#endregion' balanced; if in doubt, remove them.


4) Minimal, safe Clock (ET)
---------------------------
- Keep Core/ClockEt.cs simple to avoid parser traps.
  public static class ClockEt
  {
      public static DateTime NowEt()
      {
          try
          {
              return TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                  TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
          }
          catch { return DateTime.Now; }
      }
  }


5) JSON schema stability
------------------------
- OnBar JSON currently uses "schemaVersion":"tpo.v9_4_3".
- You may bump to "tpo.v9_4_4" for bookkeeping; this is cosmetic for logging.


6) v9.4.4 Peer Rule (prompt logic)
----------------------------------
- The "analyze all three (NQ primary; ES/YM observance) + peer-confidence deltas" rule lives in the analysis prompt.
- The AddOn may display peer adjustments later, but prompt/analysis does the scoring logic today.


7) Pre-commit checks (recommended)
----------------------------------
- Run tools/nt8_linter.py before committing. It flags tuple usage, UI creation off dispatcher, duplicate DTOs, and region mismatches.
- If it reports autofixable patterns, run tools/nt8_autofix.py (optional) or patch by hand.
