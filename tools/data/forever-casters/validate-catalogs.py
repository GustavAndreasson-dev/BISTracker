"""Verify shipped caster rows against separately collected item/quest facts."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
CATALOGS = ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30"
metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
quests = json.loads((HERE / "quest-metadata.json").read_text(encoding="utf-8"))
icons = {entry["url"]: entry["status"] for entry in json.loads((HERE / "icon-status.json").read_text(encoding="utf-8-sig"))}
specs = {"Mage": ["arcane", "fire", "frost"], "Priest": ["discipline", "holy", "shadow"],
         "Warlock": ["affliction", "demonology", "destruction"]}
rows = 0
identities = set()
report = []
for cls, names in specs.items():
    for spec in names:
        path = CATALOGS / f"{cls.lower()}-{spec}.json"
        catalog = json.loads(path.read_text(encoding="utf-8"))
        assert catalog["characterClass"] == cls and catalog["specializationId"] == spec
        assert catalog["gameVersion"] == "Forever" and catalog["levelCap"] == 30
        assert catalog["releaseStage"] == "Beta" and catalog["patch"] == "1.60.1"
        selected = [row for row in catalog["items"] if row["acquisitionType"] in ("Dungeon", "Quest")]
        assert len({row["slot"] for row in selected}) == 17, path
        for row in catalog["items"]:
            identity = row["id"]
            assert identity not in identities, identity
            identities.add(identity)
            evidence = metadata[str(row["itemId"])]
            assert row["name"] == evidence["name"], row
            assert row["itemUrl"] == evidence["itemUrl"], row
            assert row["requiredLevel"] == evidence["requiredLevel"] <= 30
            assert row["uniqueEquipped"] == evidence["uniqueEquipped"]
            assert row["iconUrl"] == evidence["iconUrl"] and icons[row["iconUrl"]] == 200
            assert row["recommendationUrl"].startswith(("https://www.wowhead.com/forever/guide/", "https://mobalytics.gg/wow-forever/classes/"))
            assert row["requiredSuffix"] is None
            if evidence["slotId"] == 17:
                assert row["slot"] == "MainHand" and row["weaponKind"] == "TwoHanded"
            if evidence["slotId"] == 11:
                assert row["slot"] in ("Finger1", "Finger2")
            if evidence["slotId"] == 12:
                assert row["slot"] in ("Trinket1", "Trinket2")
        rows += len(catalog["items"])
        report.append({"class": cls, "spec": spec, "items": len({row["itemId"] for row in catalog["items"]}),
            "placementRows": len(catalog["items"]), "dungeonQuestRows": len(selected), "dungeonQuestSlots": 17})
for quest in quests.values():
    assert quest["minimumLevel"] is not None and quest["minimumLevel"] <= 30, quest
result = {"reviewedOn": "2026-10-08", "catalogs": report, "placementRows": rows,
    "checkedItemPages": len(metadata), "checkedQuestPages": len(quests), "http200Icons": len(icons), "result": "PASS"}
(HERE / "validation-report.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(result, ensure_ascii=False))
