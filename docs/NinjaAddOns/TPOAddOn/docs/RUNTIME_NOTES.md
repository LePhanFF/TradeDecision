RUNTIME_NOTES.md
=================
Updated: 2025-09-29 01:36

Symptoms observed
-----------------
1) Unhandled exception: "The calling thread cannot access this object because a different thread owns it."
2) Market data/tick subscription errors: "Symbol is inaccessible" for ES ##-##, NQ ##-##, YM ##-##.

Fixes in this patch
-------------------
- UpdateHeader now uses the **menu item's own Dispatcher**, with shutdown guards, to set the menu caption safely.
- Start() resolves instruments from a repo/config file (`instruments.txt`) and falls back to sane front-month defaults.
- Engine start skips unresolved instruments but logs a clear message.

How to override instruments
---------------------------
1) After first run, a folder is created at:
   <Documents>\NinjaTrader 8\NinjaAddOn\TPOAddon
2) Create a text file named **instruments.txt** in that folder with one instrument per line (no '##-##'). Example:

   NQ 12-25
   ES 12-25
   YM 12-25

Notes
-----
- The '##-##' generic symbols are convenient in UI, but **market data subscriptions** typically require a **real front-month** code (e.g., 'NQ 12-25').
- If your data provider uses different naming, edit instruments.txt accordingly.
- If NinjaTrader is closing or the main dispatcher is shutting down, UpdateHeader safely no-ops.
