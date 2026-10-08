"""Print one review line per candidate item (metadata + guide mentions).

Usage: python -I summarize-candidates.py <guide-rows-observed.json> <item-metadata.json> [guidePrefix]
"""
import json
import sys

with open(sys.argv[1], encoding="utf-8") as handle:
    guides = json.load(handle)
with open(sys.argv[2], encoding="utf-8") as handle:
    meta = json.load(handle)["items"]
prefix = sys.argv[3] if len(sys.argv) > 3 else ""

mentions = {}
for key, guide in guides.items():
    if not key.startswith(prefix):
        continue
    for row in guide["rows"]:
        for item_id in row["itemIds"]:
            mentions.setdefault(str(item_id), set()).add("%s:%s" % (key.split("-", 1)[1][:14], row["section"][:22]))

for item_id in sorted(mentions, key=int):
    entry = meta.get(item_id)
    if not entry:
        continue
    cdb = entry["classicdb"]
    drops = "; ".join("%s@%s(%s%%)" % (d["name"], "/".join(str(z) for z in d["zones"]), d["percent"])
                      for d in cdb["droppedBy"][:3])
    quests = "; ".join("Q%s %s rl%s %s" % (q["questId"], q["name"], q["requiredLevel"], q["classicdbSide"])
                       for q in cdb["rewardOf"])
    craft = "; ".join("%s %s [%s]" % (c["profession"], c["skill"], ", ".join(
        ("R%s ph%s %s drop:%s vend:%d quest:%d" % (r["recipeItemId"], r["phase"], r["binding"],
                                                  "/".join(d["name"] for d in r["droppedBy"][:2]) + ("+%d" % r["droppedByCount"] if r["droppedByCount"] > 2 else ""),
                                                  len(r["soldBy"]), len(r["rewardOf"])))
        if "recipeItemId" in r else "trainer" for r in c["recipes"])) for c in cdb["createdBy"])
    chest = "; ".join(o["name"] for o in cdb["containedInObject"])
    print("%s %s | ph%s %s%s q%s rl%s %s | drops(%d): %s | quest: %s | craft: %s | obj: %s | vend:%d | %s" % (
        item_id, entry["name"], cdb["phase"], entry["binding"], " U" if entry["unique"] else "",
        entry["quality"], entry["requiredLevel"], entry["slotText"], cdb["droppedByCount"], drops, quests,
        craft, chest, len(cdb["soldBy"]), " ".join(sorted(mentions[item_id]))))
