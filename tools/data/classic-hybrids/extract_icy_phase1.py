"""Extract the pre-raid slot table from Icy Veins WoW Classic pre-raid gear pages.

Usage: python -I tools/data/classic-hybrids/extract_icy_phase1.py <out.json> <name=page.html>...

Pages with phase tabs use the table whose heading names Phase 1 (Paladin) or
"MC/Ony" (Shaman, the guide's "Phase One (MC)" tab). Pages without
phase tabs (Druid/Shaman) have one general pre-raid table; it is recorded
unchanged and must be filtered to phase 1 and allowed sources afterwards.
Each slot row keeps the guide's item links (ID and text, including any
"of X" suffix) and source texts in guide order. No ranking happens here.
"""
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))  # own helper only; -I drops script dir
from strip_guide import strip  # noqa: E402

ITEM = re.compile(r"\[(\d+)\]([^\[]*)")


def section(lines):
    heads = [i for i, line in enumerate(lines) if re.match(r"^\d+\.\d+\.\s+Phases? 1\b.*Pre-raid", line, re.I)]
    heads = heads or [i for i, line in enumerate(lines) if re.match(r"^\d+\.\s+MC/Ony .*Pre-Raid", line, re.I)]
    if heads:
        start = heads[0]
        end = next((i for i in range(start + 1, len(lines))
                    if re.match(r"^\d+(\.\d+)*\.\s", lines[i]) or lines[i] == "Guide Author"), len(lines))
        return lines[start], lines[start + 1:end]
    start = [i for i, line in enumerate(lines) if "Pre-Raid Best in Slot (BiS) List" in line][-1]
    end = next(i for i in range(start, len(lines)) if lines[i] == "Guide Author")
    return "General pre-raid list (no phase tabs)", lines[start + 1:end]


def parse(body):
    # Rows start at a "<Slot> |" line; within a row, item lines come before source lines.
    rows, row = [], None
    table_started = False
    for raw in body:
        line = raw.strip()
        if line in {"Slot |", "Item |", "Source |"}:
            table_started = True
            continue
        if not table_started:
            continue
        if line.endswith("|") and line[:-1].strip() and not ITEM.search(line) and row is None or (
                line.endswith("|") and not ITEM.search(line) and row is not None and row["stage"] == "sources-done"):
            row = {"guideSlot": line[:-1].strip(), "items": [], "sources": [], "stage": "items"}
            rows.append(row)
            continue
        if row is None:
            continue
        if line == "|":
            row["stage"] = "sources" if row["stage"] == "items" else "sources-done"
            continue
        content = line.rstrip("|").strip()
        ends_cell = line.endswith("|")
        found = ITEM.findall(content)
        if row["stage"] == "items" and found:
            row["items"].append({"line": content, "links": [{"itemId": int(i), "text": t.strip()} for i, t in found]})
            if ends_cell:
                row["stage"] = "sources"
        elif row["stage"] == "sources" and content:
            row["sources"].append(content)
            if ends_cell:
                row["stage"] = "sources-done"
        elif content:
            row.setdefault("unparsed", []).append(content)
    for row in rows:
        row.pop("stage")
    return rows


if __name__ == "__main__":
    result = {}
    for arg in sys.argv[2:]:
        name, path = arg.split("=", 1)
        text = strip(open(path, encoding="utf-8", errors="replace").read())
        lines = [line.strip() for line in text.splitlines()]
        heading, body = section(lines)
        updated = next((line for line in lines if line.startswith("Last Updated")), None)
        result[name] = {"heading": heading, "lastUpdated": updated, "rows": parse(body)}
    with open(sys.argv[1], "w", encoding="utf-8") as out:
        json.dump(result, out, ensure_ascii=False, indent=1)
