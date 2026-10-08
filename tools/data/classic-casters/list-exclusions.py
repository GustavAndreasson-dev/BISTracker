"""List guide items that were not selected, with the reason.

Usage: python -I list-exclusions.py <selection.json> <guide-rows-observed.json> <item-metadata.json> <output.json>

Rules (in order): manual decisions below; no equipment slot (consumable,
enchant, recipe, quest item); ClassicDB content phase other than 1;
Dire Maul source (phase 2); PvP/reputation/vendor reward; world drop.
Anything left is reported as "not named for this spec in an allowed section"
because a guide row elsewhere (for example a later-phase table of another
spec page) was the only mention.
"""
import json
import sys

MANUAL = {
    14146: "Crafting recipe only drops from world bosses and raid bosses (ClassicDB pattern 14511); raid source not allowed.",
    15802: "Recipe comes from the Timbermaw quest Sacred Cloth (reputation-gated per Defcamp); recipe source not verified in collected data.",
    18720: "Drop source not verifiable: ClassicDB lists no dropper and Wowhead returned HTTP 403 during review.",
    12546: "Guides label it a BoE world drop (Defcamp, Seebus).",
    18510: "Pattern drops in Dire Maul (phase 2).",
    18407: "Pattern drops in Dire Maul (phase 2).",
    19165: "Recipe requires Thorium Brotherhood reputation and is a later phase per ClassicDB.",
    18467: "Reward of the Dire Maul book quest Harnessing Shadows (phase 2).",
    20601: "Silithus/Abyssal quest container (later phase).",
    9484: None,
}
SLOTLESS = {None}


def main():
    selection_path, guides_path, meta_path, out_path = sys.argv[1:5]
    with open(selection_path, encoding="utf-8") as handle:
        selection = json.load(handle)
    with open(guides_path, encoding="utf-8") as handle:
        guides = json.load(handle)
    with open(meta_path, encoding="utf-8") as handle:
        meta = json.load(handle)["items"]
    selected = set(int(i) for i in selection["items"])
    mentioned = {}
    for key, guide in guides.items():
        for row in guide["rows"]:
            for item_id in row["itemIds"]:
                mentioned.setdefault(item_id, set()).add(key)
    result = []
    for item_id in sorted(mentioned):
        if item_id in selected or str(item_id) not in meta:
            continue
        entry = meta[str(item_id)]
        cdb = entry["classicdb"]
        zones = {z for d in cdb["droppedBy"] for z in d["zones"]}
        names = entry["name"] or ""
        if MANUAL.get(item_id):
            reason = MANUAL[item_id]
        elif entry["slotText"] in SLOTLESS or entry["binding"] == "Quest":
            reason = "No equipment slot (consumable, enchant, recipe or quest item)."
        elif cdb["phase"] != 1:
            reason = "Later content phase (ClassicDB phase %s)." % cdb["phase"]
        elif cdb["droppedByCount"] > 20:
            reason = "World drop (bind on equip, %d droppers in ClassicDB)." % cdb["droppedByCount"]
        elif "Dire Maul" in zones or any("Gordok" in o["name"] for o in cdb["containedInObject"]):
            reason = "Dire Maul source (phase 2)."
        elif any(w in names for w in ("Marshal", "Warlord", "General", "Champion", "Legionnaire", "Knight",
                                       "Blood Guard", "Lieutenant", "Frostwolf", "Stormpike", "Defiler",
                                       "Highlander", "Sentinel", "Outrider", "Dryad", "Advisor", "Lorekeeper")) or cdb["soldBy"]:
            reason = "PvP, battleground reputation or vendor reward."
        elif cdb["droppedByCount"] > 20 or entry["binding"] == "BoE" and not cdb["createdBy"]:
            reason = "World drop (bind on equip, %d droppers in ClassicDB)." % cdb["droppedByCount"]
        else:
            reason = "Only named outside the allowed sections for these specs (later-phase table or another spec)."
        result.append({"itemId": item_id, "name": entry["name"], "guides": sorted(mentioned[item_id]),
                       "classicdbPhase": cdb["phase"], "reason": reason})
    with open(out_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump({"note": "Guide-linked items not used in the caster catalogs or Holy proposal.", "items": result},
                  handle, indent=1, ensure_ascii=False)
        handle.write("\n")
    counts = {}
    for row in result:
        counts[row["reason"].split(" (")[0][:50]] = counts.get(row["reason"].split(" (")[0][:50], 0) + 1
    print(len(result), counts)


if __name__ == "__main__":
    main()
