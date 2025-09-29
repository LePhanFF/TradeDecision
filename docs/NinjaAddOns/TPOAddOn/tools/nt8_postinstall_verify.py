#!/usr/bin/env python3
# nt8_postinstall_verify.py - check installed Custom folder for duplicates, tuples, bad UI creation

import pathlib, re, sys

def main(root):
    root = pathlib.Path(root)
    errs=[]
    ninjas = list(root.rglob("NinjaAddOnTPO.cs"))
    if len(ninjas)!=1: errs.append(f"Expected 1 NinjaAddOnTPO.cs, found {len(ninjas)}")
    for p in root.rglob("*.cs"):
        t=p.read_text(errors="ignore")
        if "NTWindow.MainWindow" in t: errs.append(f"Bad NTWindow.MainWindow in {p}")
        if re.search(r"List<\s*\(", t): errs.append(f"Tuple generic in {p}")
    print("Errors:",errs)

if __name__=="__main__":
    main(sys.argv[1] if len(sys.argv)>1 else ".")
