#!/usr/bin/env python3
# nt8_autofix.py - convert tuple generics to DTOs, wrap UiHostWindow in dispatcher

import re, pathlib, sys

def fix(p):
    t = p.read_text(errors="ignore")
    orig = t
    t = re.sub(r"List<\s*\(double, double\)\s*>", "List<Node>", t)
    t = re.sub(r"\.Add\(\(([^,]+),([^\)]+)\)\)", r".Add(new Node { Price=\1, Score=\2 })", t)
    if "new UiHostWindow()" in t and "Dispatcher.Invoke" not in t:
        t = t.replace("ui = new UiHostWindow();", "var d=Application.Current?.Dispatcher; if(d!=null) d.Invoke(new Action(()=>{ui=new UiHostWindow();ui.Show();})); else {ui=new UiHostWindow();ui.Show();}")
    if t!=orig:
        p.write_text(t)
        return True
    return False

if __name__=="__main__":
    root = pathlib.Path(sys.argv[1] if len(sys.argv)>1 else ".")
    for p in root.rglob("*.cs"):
        if fix(p): print("fixed", p)
