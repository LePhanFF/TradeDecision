#!/usr/bin/env python3
"""
nt8_autofix.py
Best-effort regex-based autofixer to convert tuple usages to DTOs and wrap UiHostWindow creation in Dispatcher.
Review diffs before committing.
"""
import re, argparse, pathlib

def fix_file(p: pathlib.Path):
    text = p.read_text(encoding="utf-8", errors="ignore")
    orig = text

    # Ensure using for Core DTOs
    if "namespace NinjaTrader" in text and "using NinjaTrader.NinjaScript.AddOns.Core;" not in text:
        text = text.replace("namespace NinjaTrader", "using NinjaTrader.NinjaScript.AddOns.Core;\n\nnamespace NinjaTrader", 1)

    # Replace tuple generics
    patterns = [
        (r"List<\s*\(\s*double\s*,\s*double\s*\)\s*>", "List<Node>"),
        (r"IList<\s*\(\s*double\s*,\s*double\s*\)\s*>", "IList<Node>"),
        (r"IEnumerable<\s*\(\s*double\s*,\s*double\s*\)\s*>", "IEnumerable<Node>"),
        (r"List<\s*\(\s*double\s+Price\s*,\s*double\s+Score\s*\)\s*>", "List<Node>"),
        (r"List<\s*\(\s*double\s+Start\s*,\s*double\s+End\s*\)\s*>", "List<SinglePrint>"),
    ]
    for pat, repl in patterns:
        text = re.sub(pat, repl, text)

    # Replace .Add((a,b))
    def repl_add(m):
        head = m.group(1)  # e.g. HVN.Add(
        a = m.group(2)
        b = m.group(3)
        which = m.group(4)
        if which == "Singles":
            return f"{head}new SinglePrint {{ Start = {a}, End = {b} }}"
        return f"{head}new Node {{ Price = {a}, Score = {b} }}"
    text = re.sub(r"((HVN|LVN|Singles)\s*\.\s*Add\s*\()\(\s*([^,]+?)\s*,\s*([^)]+?)\s*\)", repl_add, text)

    # Wrap UiHostWindow creation in dispatcher
    if "new UiHostWindow()" in text and "Dispatcher.Invoke" not in text:
        text = text.replace("ui = new UiHostWindow();", 
            "var d = Application.Current != null ? Application.Current.Dispatcher : null;\nif (d != null) d.Invoke(new Action(() => { ui = new UiHostWindow(); ui.Owner = NTWindow.MainWindow; ui.Show(); }));\nelse { ui = new UiHostWindow(); ui.Owner = NTWindow.MainWindow; ui.Show(); }")

    if text != orig:
        p.write_text(text, encoding="utf-8")
        return True
    return False

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("repo", nargs="?", default=".")
    args = ap.parse_args()

    repo = pathlib.Path(args.repo)
    changed = []
    for p in repo.rglob("*.cs"):
        if fix_file(p):
            changed.append(str(p))

    print("Autofix changed files:")
    for f in changed:
        print(" -", f)

if __name__ == "__main__":
    main()
