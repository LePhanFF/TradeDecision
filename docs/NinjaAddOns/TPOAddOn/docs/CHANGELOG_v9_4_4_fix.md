CHANGELOG_v9_4_4_fix.md
=======================
Date: 2025-09-28 23:57:45

- Fixed WPF UI threading: create UiHostWindow on GUI dispatcher; close on window dispatcher.
- Removed C# tuple generic usages; replaced with Core DTOs (Node, SinglePrint).
- Normalized ProfileSnapshot to hold DTOs; ensured single definition in Core.
- Added/cleaned Core/ClockEt.cs (minimal, parser-safe).
- Ensured files that use DTOs include 'using NinjaTrader.NinjaScript.AddOns.Core;'.
- Verified no #region/#endregion mismatches remain.
- Optional: cosmetic JSON schemaVersion remains "tpo.v9_4_3" (log-only).

Notes:
- v9.4.4 peer weighting lives in prompt; UI can optionally display peer deltas in a later update.
