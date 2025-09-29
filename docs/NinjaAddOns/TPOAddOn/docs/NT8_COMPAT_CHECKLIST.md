NT8_COMPAT_CHECKLIST.md
=======================

Use this checklist before every compile:

[ ] No tuple generics in any file (List<(double,double)> etc.).
[ ] No tuple .Add((a,b)) calls; use DTO objects instead.
[ ] Exactly one definition of: Node, SinglePrint, DpocPoint, ProfileSnapshot.
[ ] Files using DTOs include: using NinjaTrader.NinjaScript.AddOns.Core;
[ ] UiHostWindow is created on Application.Current.Dispatcher in Start().
[ ] UiHostWindow.UpdateUi only touches controls inside Dispatcher.Invoke/InvokeAsync.
[ ] Stop() closes UiHostWindow via ui.Dispatcher.BeginInvoke.
[ ] No unbalanced #region/#endregion markers.
[ ] No top-level stray code (everything inside a namespace/class).
[ ] ClockEt.cs is minimal and compiles without warnings.
[ ] JSON schemaVersion string updated if needed.
[ ] Prompt v9.4.4 multi-index logic verified (NQ primary; ES/YM observance; peer deltas).
