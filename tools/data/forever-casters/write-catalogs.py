"""Write reviewed caster alternatives from original Forever guide evidence."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
TARGET = ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30"
WEAPONS = {13: "OneHanded", 17: "TwoHanded", 21: "MainHand", 22: "OffHand", 23: "OffHand", 14: "OffHand"}

def produce(path, metadata):
    observed = json.loads(path.read_text(encoding="utf-8"))
    cls, spec, guide = observed["class"], observed["spec"], observed["sourceUrl"]
    supplementary = [row for row in json.loads((HERE / "supplemental-recommendations.json").read_text(encoding="utf-8"))["items"]
        if row["class"] == cls and row["spec"] == spec]
    collected = {}
    excluded = []
    unresolved = {row["itemId"]: row for row in json.loads((HERE / "excluded-research.json").read_text(encoding="utf-8"))["items"]}
    recipe_metadata = json.loads((HERE / "crafting-metadata.json").read_text(encoding="utf-8"))
    for row in observed["rows"] + supplementary:
        if row["itemId"] in unresolved:
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": unresolved[row["itemId"]]["reason"], "researchEvidenceFile": "excluded-research.json"})
            continue
        if not row["slot"]:
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": "Consumable or recipe; not equipment."})
            continue
        data = metadata[str(row["itemId"])]
        if row["acquisitionType"] == "Crafting" and not recipe_metadata.get(str(row["itemId"]), {}).get("recipeWithinLevel30ExpertCap", False):
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": "No independently verified Forever recipe skill within the level-30 Expert cap."})
            continue
        if data["requiredLevel"] > 30:
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": "Required level above 30."})
            continue
        requirements = [(requirement["profession"], requirement["skill"]) for requirement in data["professionRequirements"]]
        if any(profession == "Engineering" and skill > 225 for profession, skill in requirements):
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": "Engineering above the level-30 beta skill cap (225)."})
            continue
        if row["modifiers"]:
            excluded.append({"itemId": row["itemId"], "name": row["name"], "reason": "Guide requests a bonus variant that has not been mapped to a verified suffix."})
            continue
        if row["name"] != data["name"] or row["slotId"] != data["slotId"]:
            raise ValueError(f"Guide/item page identity mismatch: {row['itemId']}")
        source = row["source"]
        if row["acquisitionType"] == "Dungeon":
            source = f"{source}, {row['location']}"
        elif row["acquisitionType"] == "Quest":
            if row["faction"] in ("Alliance", "Horde"):
                source += f" ({row['faction']})"
            if row["location"] and row["location"] != "Notable Open World Quest Rewards:":
                source += f", {row['location']}"
        else:
            source += " crafting"
        notes = []
        if row["note"] and not row["note"].startswith(("Named ", "Recommended in ")) and row["note"] not in ("(requires )", "(requires Merchant's Favor)"):
            notes.append(row["note"])
        if "(requires " in row["note"]:
            notes.append("Recipe requires Merchant's Favor.")
        if row["acquisitionType"] == "Crafting" and data["bindsWhenPickedUp"]:
            notes.append("Bind on pickup; crafted for personal use.")
        for profession, skill in requirements:
            notes.append(f"Requires {profession} {skill}.")
        if data["uniqueEquipped"]:
            notes.append("Unique-equipped: one equipped copy.")
        key = (row["itemId"], row["slot"])
        if key in collected:
            old = collected[key]
            if source not in old["source"]:
                old["source"] += " / " + source
            continue
        collected[key] = {"id": "", "slot": row["slot"], "itemId": row["itemId"],
            "name": row["name"], "requiredSuffix": None,
            "acquisitionType": row["acquisitionType"], "source": source,
            "note": " ".join(notes), "iconUrl": data["iconUrl"],
            "itemUrl": row["itemUrl"], "recommendationUrl": row.get("recommendationUrl", guide),
            "weaponKind": WEAPONS.get(row["slotId"], "None"),
            "uniqueEquipped": data["uniqueEquipped"], "requiredLevel": data["requiredLevel"]}
    items = []
    for item in collected.values():
        placements = (item["slot"],)
        if item["slot"] == "Finger1":
            placements = ("Finger1", "Finger2")
        elif item["slot"] == "Trinket1":
            placements = ("Trinket1", "Trinket2")
        for slot in placements:
            result = dict(item)
            result["slot"] = slot
            result["id"] = f"forever-beta-level30-{cls.lower()}-{spec}-{slot.lower()}-{item['itemId']}"
            items.append(result)
    catalogue = {"schemaVersion": 1,
        "catalogId": f"forever-beta-level30-{cls.lower()}-{spec}",
        "gameVersion": "Forever", "characterClass": cls,
        "specializationId": spec, "levelCap": 30, "releaseStage": "Beta",
        "phase": "Level 30 beta", "patch": "1.60.1", "sourceUrl": guide, "reviewedOn": "2026-10-08",
        "supplementarySourceUrls": sorted({row["recommendationUrl"] for row in supplementary}),
        "selectionMethod": "Named equipment alternatives in the original Wowhead level-30 Forever guide. "
            + ("Missing-slot alternatives independently sourced from original level-30 beta guides for the same spec; see each recommendationUrl. " if supplementary else "")
            + "No independent ranking; consumables, unverified bonus variants and unavailable profession requirements excluded.",
        "status": "Reviewed", "items": items}
    TARGET.mkdir(parents=True, exist_ok=True)
    (TARGET / f"{cls.lower()}-{spec}.json").write_text(json.dumps(catalogue, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (HERE / f"{cls.lower()}-{spec}-excluded.json").write_text(json.dumps(excluded, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(cls, spec, len(collected), "distinct items,", len(items), "placement rows", "types:", sorted({item['acquisitionType'] for item in items}))
    print("Run audit-slots.py --annotate-reviewed-factions before validation/publication; regenerated rows require current faction and recipe annotations.")

if __name__ == "__main__":
    if "--regenerate-reviewed-catalogs" not in sys.argv and any(
        path.name.startswith(("mage-", "priest-", "warlock-")) for path in TARGET.glob("*.json")):
        raise SystemExit("Existing catalogs preserved. Review changes before using --regenerate-reviewed-catalogs.")
    metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
    for path in sorted(HERE.glob("*-observed.json")):
        produce(path, metadata)
