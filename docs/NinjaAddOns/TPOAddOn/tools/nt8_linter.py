#!/usr/bin/env python3
# nt8_linter.py - flag tuple generics, Add((a,b)), UI creation issues, duplicates

import re, pathlib, sys

DTO_CLASSES = ["Node","SinglePrint","DpocPoint","ProfileSnapshot"]

def scan(root):
    root = pathlib.Path(root)
    results = {"tuple":[], "addtuple":[], "duplicates":{}, "bad_ui":[]}
    for cls in DTO_CLASSES: results["duplicates"][cls] = []
    for p in root.rglob("*.cs"):
        t = p.read_text(errors="ignore")
        if re.search(r"List<\s*\(", t): results["tuple"].append(str(p))
        if re.search(r"\.Add\(\(", t): results["addtuple"].append(str(p))
        if "new UiHostWindow()" in t and "Dispatcher.Invoke" not in t:
            results["bad_ui"].append(str(p))
        for cls in DTO_CLASSES:
            if re.search(rf"class\s+{cls}\b", t):
                results["duplicates"][cls].append(str(p))
    return results

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv)>1 else "."
    res = scan(root)
    print(res)
