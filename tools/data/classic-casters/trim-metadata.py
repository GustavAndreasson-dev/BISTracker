"""Keep the collected metadata in the repo without bulky tooltip text.

Usage: python -I trim-metadata.py <item-metadata-all.json> <selection.json> <output.json>

Selected items keep their full tooltip text as identity evidence; other
candidates keep the structured fields used for exclusion decisions.
"""
import json
import sys

with open(sys.argv[1], encoding="utf-8") as handle:
    data = json.load(handle)
with open(sys.argv[2], encoding="utf-8") as handle:
    selected = set(json.load(handle)["items"])
for item_id, entry in data["items"].items():
    if item_id not in selected:
        entry.pop("tooltipText", None)
with open(sys.argv[3], "w", encoding="utf-8", newline="\n") as handle:
    json.dump(data, handle, indent=1, ensure_ascii=False)
    handle.write("\n")
print(len(data["items"]), "items,", len(selected), "selected")
