"""Validate Classic phase 1 caster catalogs against the REQ-013 contract.

Usage:
  python -I validate-catalogs.py <catalog-dir> <item-metadata.json> <report.json> [extra-file ...]

Checks the schema-1 fields, enum names, unique row IDs, slot/weapon-hand
pairs, Quest factions, allowed acquisition types, item identity against the
collected Wowhead/ClassicDB metadata (name, phase 1, required level) and
whether a complete set is possible separately for Alliance and Horde.
Extra files (for example the Holy Priest proposal) get the row checks only.
"""
import itertools
import json
import os
import re
import sys

SLOTS = ["Head", "Neck", "Shoulder", "Back", "Chest", "Wrist", "Hands", "Waist", "Legs", "Feet",
         "Finger1", "Finger2", "Trinket1", "Trinket2", "MainHand", "OffHand", "Ranged"]
KINDS = {"None", "OneHanded", "MainHand", "OffHand", "TwoHanded"}
ALLOWED_ACQ = {"Dungeon", "Quest", "Crafting"}
FACTIONS = {"Alliance", "Horde"}
CLASS_SPECS = {"Mage": {"arcane", "fire", "frost"}, "Priest": {"discipline", "holy", "shadow"},
               "Warlock": {"affliction", "demonology", "destruction"}}
ROW_FIELDS = ["id", "slot", "itemId", "name", "acquisitionType", "source", "note", "itemUrl",
              "recommendationUrl", "weaponKind", "uniqueEquipped"]


def check_rows(rows, catalog_id, meta, errors, where):
    seen = set()
    for row in rows:
        tag = "%s/%s" % (where, row.get("id"))
        for name in ROW_FIELDS:
            if name not in row:
                errors.append("%s: missing %s" % (tag, name))
        if row.get("id") in seen:
            errors.append("%s: duplicate id" % tag)
        seen.add(row.get("id"))
        if catalog_id and not row["id"].startswith("%s-%s-%s" % (catalog_id, row["slot"].lower(), row["itemId"])):
            errors.append("%s: id does not follow <catalogId>-<slot>-<itemId>" % tag)
        if row["slot"] not in SLOTS:
            errors.append("%s: bad slot" % tag)
        kind = row["weaponKind"]
        if kind not in KINDS:
            errors.append("%s: bad weaponKind" % tag)
        ok = {"TwoHanded": row["slot"] == "MainHand", "MainHand": row["slot"] == "MainHand",
              "OffHand": row["slot"] == "OffHand", "OneHanded": row["slot"] in ("MainHand", "OffHand"),
              "None": row["slot"] not in ("MainHand", "OffHand")}
        if not ok.get(kind, False):
            errors.append("%s: weaponKind %s does not fit %s" % (tag, kind, row["slot"]))
        if row["acquisitionType"] not in ALLOWED_ACQ:
            errors.append("%s: acquisition %s not allowed" % (tag, row["acquisitionType"]))
        factions = row.get("availableFactions")
        if row["acquisitionType"] == "Quest" and not factions:
            errors.append("%s: Quest without availableFactions" % tag)
        if factions is not None and (not factions or len(set(factions)) != len(factions)
                                     or not set(factions) <= FACTIONS):
            errors.append("%s: invalid availableFactions" % tag)
        for url_field in ("itemUrl", "recommendationUrl", "iconUrl"):
            if row.get(url_field) is not None and not str(row[url_field]).startswith("https://"):
                errors.append("%s: %s must be https" % (tag, url_field))
        if row["itemUrl"] != "https://www.wowhead.com/classic/item=%d" % row["itemId"]:
            errors.append("%s: itemUrl mismatch" % tag)
        level = row.get("requiredLevel")
        if level is not None and not 0 <= level <= 60:
            errors.append("%s: requiredLevel out of range" % tag)
        if row["acquisitionType"] == "Crafting" and not re.search(r"(Tailoring|Leatherworking|Engineering|Blacksmithing)", row["source"]):
            errors.append("%s: crafting source lacks profession" % tag)
        item = meta.get(str(row["itemId"]))
        if not item:
            errors.append("%s: no collected metadata" % tag)
            continue
        if item["name"] != row["name"]:
            errors.append("%s: name differs from tooltip (%s)" % (tag, item["name"]))
        if item["classicdb"]["phase"] != 1:
            errors.append("%s: ClassicDB phase is %s" % (tag, item["classicdb"]["phase"]))
        if item["requiredLevel"] != level:
            errors.append("%s: requiredLevel differs from tooltip" % tag)
        if bool(item["unique"]) != row["uniqueEquipped"]:
            errors.append("%s: unique flag differs from tooltip" % tag)
        if row["acquisitionType"] == "Crafting" and item["binding"] == "BoP" and "Requires " not in row["note"]:
            errors.append("%s: BoP crafted item note lacks profession requirement" % tag)


def faction_set(rows, faction):
    usable = [r for r in rows if not r.get("availableFactions") or faction in r["availableFactions"]]
    by_slot = {s: [r for r in usable if r["slot"] == s] for s in SLOTS}
    chosen = {}
    missing = []
    for slot in ["Head", "Neck", "Shoulder", "Back", "Chest", "Wrist", "Hands", "Waist", "Legs", "Feet", "Ranged"]:
        if by_slot[slot]:
            chosen[slot] = by_slot[slot][0]["itemId"]
        else:
            missing.append(slot)
    for pair in (("Finger1", "Finger2"), ("Trinket1", "Trinket2")):
        ids = sorted({r["itemId"] for r in by_slot[pair[0]]} | {r["itemId"] for r in by_slot[pair[1]]})
        found = None
        for a, b in itertools.permutations(ids, 2):
            if any(r["itemId"] == a for r in by_slot[pair[0]]) and any(r["itemId"] == b for r in by_slot[pair[1]]):
                found = (a, b)
                break
        if found:
            chosen[pair[0]], chosen[pair[1]] = found
        elif ids:
            chosen[pair[0]] = ids[0]
            missing.append(pair[1])
        else:
            missing.extend(pair)
    two_hand = [r for r in by_slot["MainHand"] if r["weaponKind"] == "TwoHanded"]
    one_hand = [r for r in by_slot["MainHand"] if r["weaponKind"] != "TwoHanded"]
    if two_hand:
        chosen["MainHand"] = two_hand[0]["itemId"]
        chosen["OffHand"] = "covered by two-hander"
    elif one_hand and by_slot["OffHand"]:
        chosen["MainHand"] = one_hand[0]["itemId"]
        chosen["OffHand"] = by_slot["OffHand"][0]["itemId"]
    else:
        if not one_hand:
            missing.append("MainHand")
        if not by_slot["OffHand"]:
            missing.append("OffHand")
    return {"complete": not missing, "missingSlots": missing, "exampleSet": chosen}


def main():
    catalog_dir, meta_path, report_path = sys.argv[1:4]
    extras = sys.argv[4:]
    with open(meta_path, encoding="utf-8") as handle:
        meta = json.load(handle)["items"]
    report = {"catalogs": {}, "extraFiles": {}, "errors": []}
    for name in sorted(os.listdir(catalog_dir)):
        if not name.endswith(".json") or name == "priest-holy.json":
            continue
        with open(os.path.join(catalog_dir, name), encoding="utf-8") as handle:
            catalog = json.load(handle)
        errors = []
        cid = catalog.get("catalogId")
        expected = "classic-phase1-level60-%s-%s" % (catalog["characterClass"].lower(), catalog["specializationId"])
        if cid != expected or name != "%s-%s.json" % (catalog["characterClass"].lower(), catalog["specializationId"]):
            errors.append("catalogId/file name mismatch")
        fixed = {"schemaVersion": 1, "gameVersion": "Classic", "levelCap": 60, "releaseStage": "Classic",
                 "phase": "Phase 1 · pre-raid"}
        for key, value in fixed.items():
            if catalog.get(key) != value:
                errors.append("root %s must be %r" % (key, value))
        if catalog["specializationId"] not in CLASS_SPECS.get(catalog["characterClass"], ()):
            errors.append("unknown class/spec")
        if catalog.get("status") not in ("Reviewed", "Partial"):
            errors.append("bad status")
        if not re.fullmatch(r"\d{4}-\d{2}-\d{2}", catalog.get("reviewedOn", "")):
            errors.append("bad reviewedOn")
        for key in ("sourceUrl", "selectionMethod"):
            if not catalog.get(key):
                errors.append("missing %s" % key)
        if not catalog.get("items"):
            errors.append("no items")
        check_rows(catalog["items"], cid, meta, errors, name)
        filled = sorted({r["slot"] for r in catalog["items"]}, key=SLOTS.index)
        sets = {f: faction_set(catalog["items"], f) for f in ("Alliance", "Horde")}
        complete = all(s["complete"] for s in sets.values())
        if catalog["status"] == "Reviewed" and not complete:
            errors.append("status Reviewed but a faction set is incomplete")
        report["catalogs"][name] = {
            "status": catalog["status"], "rows": len(catalog["items"]),
            "distinctItems": len({r["itemId"] for r in catalog["items"]}),
            "byAcquisition": {a: sum(1 for r in catalog["items"] if r["acquisitionType"] == a) for a in sorted(ALLOWED_ACQ)},
            "slotsWithAlternatives": filled,
            "slotsWithoutAlternatives": [s for s in SLOTS if s not in filled],
            "factionSets": sets, "errors": errors,
        }
        report["errors"].extend("%s: %s" % (name, e) for e in errors)
    for path in extras:
        with open(path, encoding="utf-8") as handle:
            data = json.load(handle)
        errors = []
        check_rows(data["items"], "classic-phase1-level60-priest-holy", meta, errors, os.path.basename(path))
        report["extraFiles"][os.path.basename(path)] = {"rows": len(data["items"]), "errors": errors}
        report["errors"].extend(errors)
    report["result"] = "PASS" if not report["errors"] else "FAIL"
    with open(report_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(report, handle, indent=2, ensure_ascii=False)
        handle.write("\n")
    print(report["result"], len(report["errors"]))
    for error in report["errors"][:60]:
        print(" -", error)


if __name__ == "__main__":
    main()
