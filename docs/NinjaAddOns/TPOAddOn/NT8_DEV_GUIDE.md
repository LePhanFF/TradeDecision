# NinjaTrader 8 — Global Dev Guide (Add-Ons, Indicators, Strategies)
**Version:** 1.0  
**Purpose:** Shared conventions and guardrails for NT8 development across projects.

---

## 0) Compatibility Baseline
NinjaTrader 8’s compiler lags modern C#. To avoid import/compile failures:
- ✅ Target classic C# 6/7 patterns.
- ❌ Do **not** use: tuples (`List<(... )>`), target‑typed `new()`, `^`/`..` (index/range), `record`, `with`, default interface methods, advanced pattern matching, `System.Text.Json` (prefer `StringBuilder`).
- Prefer simple structs/classes to carry grouped values.

### Allowed containers (instead of tuples)
```csharp
internal struct LevelScore { public double Price; public double Score; public LevelScore(double p,double s){ Price=p; Score=s; } }
internal struct RangeBand { public double Start; public double End; public RangeBand(double a,double b){ Start=a; End=b; } }
internal sealed class DpocPoint { public System.DateTime Et; public double Price; public DpocPoint(System.DateTime et,double p){ Et=et; Price=p; } }
```

---

## 1) Project Structure & Naming
- No version numbers in class/file/folder names. Keep names generic (e.g., `TPOAddon`, `MyIndicator`).
- Put the **current logic version** in code (`const string VersionTag = "vX.Y.Z"`) and in docs/JSON schema.
- Suggested repo layout:
```
src/
  AddOn/
  Indicators/
  Strategies/
docs/
tools/
PROMPTS/
```

---

## 2) Bars & Events (Add-Ons vs. Indicators)
**Add-Ons (data without attaching to a chart):** use `BarsRequest`.
```csharp
private void OnBarsUpdate(object sender, NinjaTrader.Data.BarsUpdateEventArgs e)
{
    var series = e.BarsSeries;
    int latestClosed = series.Count - 2; // closed bar only
    int start = System.Math.Max(e.MinIndex, lastProcessed + 1);
    int end   = System.Math.Min(e.MaxIndex, latestClosed);
    for (int i=start; i<=end; i++) {
        var o = series.GetOpen(i); var h = series.GetHigh(i); var l = series.GetLow(i); var c = series.GetClose(i);
        var utc = series.GetTime(i);
        // process...
    }
    lastProcessed = end;
}
```
**Indicators (chart‑attached):** override `OnBarUpdate` and add series via `AddDataSeries(...)` if needed. Always guard for `CurrentBar < required` and `IsFirstTickOfBar` if processing on bar close.

---

## 3) UI & Thread Safety
- Only touch WPF controls on the UI thread: `Dispatcher.BeginInvoke(...)`.
- Use `lock (stateLock)` around shared state; `lock (fileLock)` for file I/O.
- For “latest” JSON: write to `file.tmp`, then `File.Move(tmp, target)` to be atomic.
- On shutdown: unsubscribe event handlers, cancel/Dispose BarsRequest, close windows.

---

## 4) Packaging & Distribution
- **Development:** copy `.cs` under `Documents\NinjaTrader 8\bin\Custom\...` and compile in the NinjaScript Editor (F5).
- **Distribution:** from NinjaTrader use **Tools → Export → NinjaScript Add-On**. Hand‑built zips are often rejected.
- If you must zip locally, the archive root must be exactly `bin/` and include only `.cs` (docs are best excluded).

---

## 5) Logging & JSON
- Use `NinjaTrader.Code.Output.Process("message", PrintTo.OutputTab1)` for console logging.
- For files, use `StringBuilder` to build JSON (no JSON libs), and atomic writes for “latest”. Include timestamps in ET and UTC.
- Keep JSON verbose & stable; include `tickSize`, `pointValue`, and `schemaVersion` strings for consumers.

---

## 6) Performance Tips
- Minimize per‑bar allocations. Reuse `StringBuilder`, prefer arrays/Lists over LINQ in hot paths.
- Avoid heavy WPF visuals; batch UI updates on bar close.
- Guard dictionary growth; consider session resets.

---

## 7) Code Hygiene
- Keep shared models in a single file to avoid duplicate type errors.
- Add an internal **lint step** (see `tools\nt8-lint.ps1`) to block disallowed language features before commit/packaging.
- Use meaningful namespaces: `NinjaTrader.NinjaScript.AddOns.*` / `.Indicators.*`.

---

## 8) Global JSON & Versioning Conventions
- JSON root should carry:
  - `"schemaVersion": "tpo.v9_4_3"` (example)
  - `"type": "bar" | "session" | "signal"`
  - instrument identifiers, timestamps (`ts_utc`, `ts_et`), and core metrics.
- Keep version tags in one place in code and docs (no version in class/file names).

---

## 9) Troubleshooting Import/Compile
- Import fails with “older, incompatible…” → Use NT Export or install sources locally and compile in editor.
- Syntax errors on tuples/new()/`^1` → Replace with structs + explicit `new List<T>()` and index math.
- BarsRequest errors → Ensure handler is `BarsUpdateEventArgs` and you’re using `e.BarsSeries`.

---

## 10) Quick Checklist Before Packaging
- [ ] No tuples / no target‑typed `new()` / no `^1` / no `..` / no `record`.
- [ ] One definition per shared model (no duplicates).
- [ ] Bars processing only on closed bars.
- [ ] UI updates via Dispatcher only.
- [ ] Atomic latest JSON writes.
- [ ] If distributing: Export via NinjaTrader.
