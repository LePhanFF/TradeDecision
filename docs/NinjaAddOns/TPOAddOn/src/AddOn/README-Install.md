# NinjaAddOn TPO — Patch Helpers (Dispatcher safety, logging, heartbeat, symbol resolver)

**Package:** NinjaAddOnTPO_Patch — built 20250929_031532

This zip provides *drop‑in helpers* for your TPO add‑on to:
- Prevent cross‑thread UI exceptions (WPF Dispatcher safety)
- Add a dual‑sink logger (Output window + rolling file)
- Add a lightweight UI heartbeat (updates “Last Updated (ET)” every second)
- Resolve continuous symbols like `ES ##-##` to an active front month (e.g., `ES 12-25`)

It does **not** replace your existing code. You copy these files into your project and
make a few small edits as described below.

---

## 1) Install

1. Close NinjaTrader.
2. Copy the **contents of this zip** into your Documents folder so that the path is:
   `Documents\NinjaTrader 8\bin\Custom\AddOns\NinjaAddOnTPO\*.cs`
3. Open NinjaTrader → **New → NinjaScript Editor** → press **F5** to compile.

If you prefer **Import NinjaScript**, keep this zip structure intact and import it.
It matches `bin/Custom/...` layout so NinjaTrader recognizes it.

---

## 2) Wire up logging

In your add‑on initialization (e.g., constructor, or when the window is created), add:

```csharp
NinjaTrader.NinjaScript.AddOns.TpoLogger.Init();
NinjaTrader.NinjaScript.AddOns.TpoLogger.Info("TPO starting…");
```

You’ll see log lines in the **Output** window and a file under:
`Documents\NinjaTrader 8\log\NinjaAddOnTPO_YYYYMMDD_HHMMSS.log`

---

## 3) Make UI updates thread‑safe

Wrap any UI work (setting label text, adding children, changing properties) with:

```csharp
NinjaTrader.NinjaScript.AddOns.UiThread.OnUI(() =>
{
    // your existing UI code here, e.g.:
    // headerLabel.Content = title;
    // statusTextBlock.Text = "Ready";
});
```

If you have direct `SetValue(...)` calls:

```csharp
NinjaTrader.NinjaScript.AddOns.UiThread.SafeSet(myControl, SomeControl.SomeProperty, value);
```

**Tip:** For whole methods like `UpdateHeader(...)`, wrap the **entire body** in `OnUI(() => {{ ... }})`.

---

## 4) Heartbeat (keeps "Last Updated (ET)" moving and proves the window is alive)

Create a field and start it when your window is ready (e.g., after controls are created):

```csharp
private NinjaTrader.NinjaScript.AddOns.Heartbeat _hb;

void StartHeartbeat()
{
    if (_hb != null) return;
    _hb = new NinjaTrader.NinjaScript.AddOns.Heartbeat(
        TimeSpan.FromSeconds(1),
        et =>
        {
            // Update your header label safely
            NinjaTrader.NinjaScript.AddOns.UiThread.OnUI(() =>
            {
                // Replace 'lastUpdatedTextBlock' with your actual control name
                lastUpdatedTextBlock.Text = et.ToString("HH:mm:ss");
            });
        });
    _hb.Start();
}
```

Call `_hb?.Stop();` when the window closes.

---

## 5) Subscribe to the correct (front‑month) instrument

When your code is about to subscribe to market data for a symbol that might be **continuous** (`##-##`), resolve it:

```csharp
string requested = "ES ##-##";    // or whatever you currently pass in
var instrument = NinjaTrader.NinjaScript.AddOns.DataSubscriptions
                    .GetInstrumentFromMaybeContinuous(requested);

TpoLogger.Info($"Subscribing to {{instrument?.MasterInstrument?.Name}} ({requested}→{{instrument?.FullName}})");

// Use 'instrument' for your MarketData/Bars subscriptions
```

If you only need the **resolved text**:
```csharp
string resolved = NinjaTrader.NinjaScript.AddOns.DataSubscriptions.ResolveFrontMonth("NQ ##-##");
```

---

## 6) Version mismatch tip

If your window title shows **v9.4.3** while the Add-On list shows **v9.4.4**, delete any
old DLLs in `Documents\NinjaTrader 8\bin\Custom` and recompile so the latest sources produce one `NinjaTrader.Custom.dll`.

---

## Files in this package

- `UiThread.cs` — UI dispatcher helpers (`OnUI`, `SafeSet`)
- `TpoLogger.cs` — Output + file logger
- `Heartbeat.cs` — Small DispatcherTimer that calls you back with ET time
- `TimeUtil.cs` — ET conversion helper
- `DataSubscriptions.cs` — Helpers to resolve `##-##` symbols to front month (and return an `Instrument`)

All code is internal to keep your namespace clean. Adjust visibility if you prefer.