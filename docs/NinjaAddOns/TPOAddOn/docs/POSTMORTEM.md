# POSTMORTEM_2025-09-28_compile_fix.md
Recorded: 2025-09-29 02:59 

## Summary
NT8 compile failed with parser errors due to a mangled Start() in NinjaAddOnTPO.cs.

## Root Causes
- Duplicate dispatcher/UI creation blocks -> unmatched braces.
- Tuple generics -> parser bombs.
- Missing ClockEt helpers (ToEt, HM).
- NTWindow.MainWindow not valid in some builds.

## Fix Applied
- Rebuilt Start() canonical pattern using Application.Current.Dispatcher.Invoke.
- Added ClockEt.ToEt and HM.
- Removed tuple generics; enforced Core DTOs (Node, SinglePrint, DpocPoint, ProfileSnapshot).
- Removed NTWindow.MainWindow usage.

## Canonical Start() Template
```csharp
var disp = Application.Current != null ? Application.Current.Dispatcher : null;
if (disp != null)
{
    disp.Invoke(new Action(() =>
    {
        ui = new UiHostWindow();
        ui.Show();
    }));
}
else
{
    ui = new UiHostWindow();
    ui.Show();
}
```

## Guardrails
- No tuple generics, only DTOs.
- Only one ProfileSnapshot in Core.
- Dispatcher for UI create/update/close.
- Delete bin/obj before clean builds.
