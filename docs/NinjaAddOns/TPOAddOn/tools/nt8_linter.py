#!/usr/bin/env python3
"""
nt8_linter.py
Quick pre-commit linter for NinjaTrader 8 code bases.

- Flags C# tuple generics (List<(double,double)>, etc.)
- Flags .Add((a,b)) tuple adds
- Warns if UiHostWindow is created without dispatcher
- Checks for duplicate DTO class definitions
- Checks for unbalanced #region/#endregion
- Writes a short report to stdout and tools/nt8_lint_report.txt
"""
import sys, re, os, argparse, pathlib

DTO_CLASSES = ["Node", "SinglePrint", "DpocPoint", "ProfileSnapshot"]

def scan(path):
    path = pathlib.Path(path)
    results = {
        "tuple_generics": [],
        "tuple_adds": [],
        "dispatcher_window": [],
        "duplicates": {cls: [] for cls in DTO_CLASSES},
        "region_mismatch": [],
    }

    for p in path.rglob("*.cs"):
        text = p.read_text(encoding="utf-8", errors="ignore")

        # tuple generics
        if re.search(r"\b(List|IList|IEnumerable)\s*<\s*\(", text):
            results["tuple_generics"].append(str(p))

        # tuple Add((a,b))
        if re.search(r"\.\s*Add\s*\(\s*\(", text):
            results["tuple_adds"].append(str(p))

        # UiHostWindow creation without dispatcher
        if "new UiHostWindow()" in text and "Dispatcher.Invoke" not in text:
            results["dispatcher_window"].append(str(p))

        # DTO duplicates
        for cls in DTO_CLASSES:
            if re.search(rf"\bclass\s+{cls}\b", text):
                results["duplicates"][cls].append(str(p))

        # region mismatch
        regs = text.count("#region")
        endregs = text.count("#endregion")
        if regs != endregs:
            results["region_mismatch"].append((str(p), regs, endregs))

    return results

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("repo", nargs="?", default=".")
    args = ap.parse_args()

    res = scan(args.repo)

    report = []
    report.append("NT8 Lint Report")
    report.append("Repo: %s" % pathlib.Path(args.repo).resolve())
    report.append("="*60)

    report.append("\nTuple generics found in:")
    for f in res["tuple_generics"]:
        report.append("  - " + f)

    report.append("\nTuple Add((a,b)) found in:")
    for f in res["tuple_adds"]:
        report.append("  - " + f)

    report.append("\nUiHostWindow created without Dispatcher in:")
    for f in res["dispatcher_window"]:
        report.append("  - " + f)

    report.append("\nDTO duplicate definitions:")
    for cls, files in res["duplicates"].items():
        if len(files) > 1:
            report.append("  %s:" % cls)
            for f in files:
                report.append("    - " + f)

    report.append("\n#region/#endregion mismatches:")
    for f, a, b in res["region_mismatch"]:
        report.append("  - %s (region=%d, endregion=%d)" % (f, a, b))

    out = "\n".join(report)
    print(out)

    # write to tools/nt8_lint_report.txt
    out_path = pathlib.Path("tools") / "nt8_lint_report.txt"
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(out, encoding="utf-8")

if __name__ == "__main__":
    main()
