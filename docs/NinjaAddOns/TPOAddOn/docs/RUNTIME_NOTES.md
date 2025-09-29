# RUNTIME_NOTES.md
Updated: 2025-09-29 02:59 

## Issues observed
- Unhandled cross-thread exceptions in UpdateHeader.
- Market data subscription errors for ES/NQ/YM ##-##.

## Fixes
- UpdateHeader now uses menuItem.Dispatcher with shutdown guards.
- Instruments resolved from instruments.txt (user config) or fallback to NQ/ES/YM 12-25.

## Usage
- Create Documents/NinjaTrader 8/NinjaAddOn/TPOAddon/instruments.txt
- One instrument per line, e.g.:
```
NQ 12-25
ES 12-25
YM 12-25
```
