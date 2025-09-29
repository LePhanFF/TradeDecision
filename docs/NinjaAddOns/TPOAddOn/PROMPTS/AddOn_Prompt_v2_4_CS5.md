# AddOn Prompt v2.4 (CS5 Safe)
Updated: 2025-09-29 04:08 

## Deltas vs v2.3
- Integrate logging via `TpoLogger` (Output tab + rolling file).
- Use `UiThread` helpers for all Dispatcher interactions.
- Add `Heartbeat` (30s) for liveness logging.
- Resolve instruments via `DataSubscriptions.ResolveFrontMonth()` for placeholders like `##-##`.
- **C# 5 Compatibility:** NO modern C# features.
  - No string interpolation (`$"..."`), use concatenation or `string.Format`.
  - No null-conditional (`?.`), use explicit null checks.
  - No expression-bodied members (`=>` on methods/properties).
  - No pattern matching (`is` with conditions), no local functions.
  - No `using var` declarations; only `using (...) { }` blocks.
  - Avoid `nameof()`. Avoid `ref readonly`, `in` params.

## Guardrails (retain from v2.3)
- Canonical Start/Stop (UiHostWindow created on Application.Current.Dispatcher via UiThread).
- Dispatcher-safe `UpdateHeader()` and all UI updates.
- Instruments from `instruments.txt` with fallback contracts.
- Safe logging; never crash on logging failures.
- Braces balanced; code inside namespaces only.

## Output expectations
- Files: `AddOns/NinjaAddOnTPO/*.cs` with the helpers and AddOn code.
- Code compiles on NT8's C# compiler without modern features.
