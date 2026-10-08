"""Pick candidate item IDs from the observed guide rows.

Usage: python -I select-candidates.py <guide-rows-observed.json> <candidates.json>

Only pre-raid sections are used. Zockify pages also contain raid phase lists,
so only their "Pre-BiS" section is read. Seebus' Holy guide lists Phase 1
(pre-raid) first; later blocks repeat slot headings and are raid lists, so
only rows before the second "Head" section are read.
"""
import json
import sys

with open(sys.argv[1], encoding="utf-8") as handle:
    guides = json.load(handle)

items = set()
for key, guide in guides.items():
    rows = guide["rows"]
    if key.startswith("zockify"):
        rows = [r for r in rows if "Pre-BiS" in r["section"]]
    if key == "warcrafttavern-holy-seebus":
        kept, heads, previous = [], 0, None
        for row in rows:
            if row["section"] == "Head" and previous != "Head":
                heads += 1
            previous = row["section"]
            if heads >= 2:
                break
            if heads == 1:
                kept.append(row)
        rows = kept
    for row in rows:
        items.update(row["itemIds"])

with open(sys.argv[2], "w", encoding="utf-8") as handle:
    json.dump({"items": sorted(items)}, handle)
print(len(items))
