"""Cross-check reviewed feasibility witnesses against the actual level-30 packs.

This verifies source reviews against product data; it does not infer a ranking
or replace review of quest chains, recipe acquisition or game-client behavior.
"""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SLOTS = {"Head", "Neck", "Shoulder", "Back", "Chest", "Wrist", "Hands",
         "Waist", "Legs", "Feet", "Finger1", "Finger2", "Trinket1",
         "Trinket2", "MainHand", "OffHand", "Ranged"}
POLICY = {"Dungeon", "Quest", "Crafting"}


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def run(output):
    packs = {pack["catalogId"]: pack for pack in
             (read(path) for path in (ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30").glob("*.json"))}
    errors, rows, reviewed = [], [], set()
    metadata = {}
    for pack in packs.values():
        for item in pack["items"]:
            key = (item["itemId"], item.get("requiredSuffix"))
            facts = (item["name"], item.get("requiredLevel"), item["uniqueEquipped"])
            if key in metadata and metadata[key] != facts:
                errors.append(f"Inconsistent item metadata: {key} in {pack['catalogId']}")
            metadata[key] = facts
    for group in ("casters", "hybrids", "physical"):
        source = Path(f"docs/data/forever-{group}/slot-audit.json")
        audit = read(ROOT / source)
        for review in audit.get("catalogs", audit.get("perSpec", [])):
            catalog_id = review["catalogId"]
            if catalog_id not in packs or catalog_id in reviewed:
                errors.append(f"Unknown or duplicated reviewed catalog: {catalog_id}")
                continue
            reviewed.add(catalog_id)
            pack = packs[catalog_id]
            setup = pack.get("weaponSetup", "Flexible")
            active = {item["id"]: item for item in pack["items"] if item["acquisitionType"] in POLICY
                      and not (setup == "OneHandAndOffHand" and item["weaponKind"] == "TwoHanded")
                      and not (setup == "TwoHanded" and (item["slot"] == "OffHand"
                               or (item["slot"] == "MainHand" and item["weaponKind"] != "TwoHanded")))}
            factions = review.get("factions", [])
            if {faction.get("faction") for faction in factions} != {"Alliance", "Horde"} or len(factions) != 2:
                errors.append(f"Missing separate faction witnesses: {catalog_id}")
            for faction in factions:
                label = f"{catalog_id}/{faction['faction']}"
                failures, chosen = [], []
                for witness in faction.get("legalSetWitness", []):
                    item = active.get(witness.get("recommendationId"))
                    if item is None:
                        failures.append(f"Missing active recommendation: {witness.get('recommendationId')}")
                        continue
                    if item["slot"] != witness["slot"] or item["itemId"] != witness["itemId"]:
                        failures.append(f"Witness identity/slot mismatch: {witness}")
                    if faction["faction"] not in item.get("availableFactions", [] if item["acquisitionType"] == "Quest" else ["Alliance", "Horde"]):
                        failures.append(f"Wrong or unknown faction: {item['id']}")
                    chosen.append(item)
                required = SLOTS - {value["slot"] for value in pack.get("slotExemptions", [])}
                mains = [item for item in chosen if item["slot"] == "MainHand"]
                two_hand = len(mains) == 1 and mains[0]["weaponKind"] == "TwoHanded"
                if (setup == "TwoHanded" and not two_hand) or (setup == "OneHandAndOffHand" and two_hand):
                    failures.append("Witness contradicts published weapon setup")
                if two_hand:
                    required = required - {"OffHand"}
                slots = [item["slot"] for item in chosen]
                if set(slots) != required or len(slots) != len(required):
                    failures.append(f"Wrong slot set: missing {sorted(required-set(slots))}, duplicate/extra {sorted(set(slots)-required)}")
                variants = [(item["itemId"], item.get("requiredSuffix")) for item in chosen]
                if len(set(variants)) != len(variants):
                    failures.append("Same recorded physical variant used twice")
                unique_ids = {item["itemId"] for item in active.values() if item["uniqueEquipped"]}
                for item_id in unique_ids:
                    if sum(item["itemId"] == item_id for item in chosen) > 1:
                        failures.append(f"Unique base item used twice: {item_id}")
                professions = faction.get("requiredProfessions")
                if (faction.get("fullSetValidated") is not True or not isinstance(professions, list)
                        or len(professions) > 2 or faction.get("incompatibleProfessionRequirements")):
                    failures.append("Missing or incompatible active profession review")
                errors.extend(f"{label}: {failure}" for failure in failures)
                rows.append({"catalogId": catalog_id, "faction": faction["faction"],
                             "reviewSource": source.as_posix(), "verifiedSlots": len(chosen),
                             "requiredProfessions": professions, "result": "FAIL" if failures else "PASS"})
    if len(packs) != 27 or reviewed != set(packs) or len(rows) != 54:
        errors.append("Exactly 27 catalogs and 54 separate faction witnesses are required")
    report = {"auditType": "ForeverSlotWitnessIntegration", "reviewedOn": "2026-10-08",
              "result": "FAIL" if errors else "PASS", "checkedCatalogs": len(reviewed),
              "checkedFactionSets": len(rows), "errors": errors, "witnesses": rows,
              "scope": "Actual active product identities, slots, factions, physical variants, unique rules, weapon plans, reviewed profession count. Quest reward/chain and recipe accessibility evidence remains in the linked specialist reviews; no game-client playthrough."}
    if output.exists() and read(output).get("auditType") != report["auditType"]:
        raise ValueError("Unrelated existing output preserved")
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{report['result']}: {len(reviewed)} catalogs, {len(rows)} faction witnesses, {len(errors)} integration errors")
    for error in errors:
        print(error)
    return 1 if errors else 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("output", type=Path)
    raise SystemExit(run(parser.parse_args().output.resolve()))
