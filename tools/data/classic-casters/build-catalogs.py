"""Write Classic phase 1 caster catalogs from the reviewed selection.

Usage:
  python -I build-catalogs.py <selection.json> <item-metadata.json> <catalog-dir> <holy-additions.json>

selection.json holds the reviewed decisions: shared item acquisition facts
("items") and, per spec, the guide-backed alternatives. Name, icon, slot,
weapon hand, unique flag and required level come from item-metadata.json
(Wowhead Classic tooltip). Nothing is ranked; alternatives keep guide order.
"""
import json
import os
import sys

SLOT_MAP = {
    "Head": ("Head", "None"), "Neck": ("Neck", "None"), "Shoulder": ("Shoulder", "None"),
    "Back": ("Back", "None"), "Chest": ("Chest", "None"), "Wrist": ("Wrist", "None"),
    "Hands": ("Hands", "None"), "Waist": ("Waist", "None"), "Legs": ("Legs", "None"),
    "Feet": ("Feet", "None"), "Wand": ("Ranged", "None"), "Ranged": ("Ranged", "None"),
    "One-Hand": ("MainHand", "OneHanded"), "Main Hand": ("MainHand", "MainHand"),
    "Two-Hand": ("MainHand", "TwoHanded"), "Off Hand": ("OffHand", "OffHand"),
    "Held In Off-hand": ("OffHand", "OffHand"),
}
DOUBLE = {"Finger": ("Finger1", "Finger2"), "Trinket": ("Trinket1", "Trinket2")}
ROOT = {
    "gameVersion": "Classic", "levelCap": 60, "releaseStage": "Classic",
    "phase": "Phase 1 · pre-raid",
}


def slots_for(meta):
    text = meta["slotText"]
    if text in DOUBLE:
        return [(slot, "None") for slot in DOUBLE[text]]
    if text not in SLOT_MAP:
        raise SystemExit("Unknown slot %r for item %s" % (text, meta["itemId"]))
    return [SLOT_MAP[text]]


def note_for(fact, meta, suffix):
    parts = []
    if suffix:
        parts.append("Requires the %s random suffix; other variants do not meet this recommendation." % suffix)
    if fact.get("note"):
        parts.append(fact["note"])
    if meta["unique"]:
        parts.append("Unique: one equipped copy.")
    return " ".join(parts)


def build_rows(catalog_id, entries, facts, meta):
    rows = []
    for entry in entries:
        item_id = str(entry["itemId"])
        fact = facts[item_id]
        item = meta[item_id]
        suffix = entry.get("requiredSuffix")
        for slot, kind in slots_for(item):
            suffix_key = "-" + suffix.lower().replace(" ", "-") if suffix else ""
            row = {
                "id": "%s-%s-%s%s" % (catalog_id, slot.lower(), item_id, suffix_key),
                "slot": slot,
                "itemId": int(item_id),
                "name": item["name"],
                "requiredSuffix": suffix,
                "acquisitionType": fact["acquisitionType"],
                "source": fact["source"],
                "note": note_for(fact, item, suffix),
                "iconUrl": "https://wow.zamimg.com/images/wow/icons/large/%s.jpg" % item["icon"],
                "itemUrl": "https://www.wowhead.com/classic/item=%s" % item_id,
                "recommendationUrl": entry["recommendationUrl"],
                "weaponKind": kind,
                "uniqueEquipped": bool(item["unique"]),
            }
            if item["requiredLevel"] is not None:
                row["requiredLevel"] = item["requiredLevel"]
            if fact.get("availableFactions"):
                row["availableFactions"] = fact["availableFactions"]
            rows.append(row)
    return rows


def main():
    selection_path, meta_path, out_dir, holy_path = sys.argv[1:5]
    with open(selection_path, encoding="utf-8") as handle:
        selection = json.load(handle)
    with open(meta_path, encoding="utf-8") as handle:
        meta = json.load(handle)["items"]
    facts = selection["items"]
    os.makedirs(out_dir, exist_ok=True)
    for key, spec in selection["specs"].items():
        catalog_id = "classic-phase1-level60-%s" % key
        catalog = {"schemaVersion": 1, "catalogId": catalog_id}
        catalog.update({"gameVersion": ROOT["gameVersion"], "characterClass": spec["characterClass"],
                        "specializationId": spec["specializationId"], "levelCap": ROOT["levelCap"],
                        "releaseStage": ROOT["releaseStage"], "phase": ROOT["phase"],
                        "sourceUrl": spec["sourceUrl"], "reviewedOn": selection["reviewedOn"],
                        "supplementarySourceUrls": spec["supplementarySourceUrls"],
                        "selectionMethod": spec["selectionMethod"] + "".join(
                            " Known gap: %s" % gap for gap in selection.get("gaps", {}).get(key, [])),
                        "status": spec["status"]})
        if spec.get("slotExemptions"):
            catalog["slotExemptions"] = spec["slotExemptions"]
        catalog["items"] = build_rows(catalog_id, spec["alternatives"], facts, meta)
        path = os.path.join(out_dir, "%s.json" % key)
        with open(path, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(catalog, handle, indent=2, ensure_ascii=False)
            handle.write("\n")
        print(path, len(catalog["items"]))
    holy = selection["holyAdditions"]
    holy_rows = build_rows("classic-phase1-level60-priest-holy", holy["alternatives"], facts, meta)
    with open(holy_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump({"purpose": holy["purpose"], "reviewedOn": selection["reviewedOn"],
                   "sourceUrls": holy["sourceUrls"], "items": holy_rows}, handle, indent=2, ensure_ascii=False)
        handle.write("\n")
    print(holy_path, len(holy_rows))


if __name__ == "__main__":
    main()
