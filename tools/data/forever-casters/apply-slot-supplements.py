"""Review missing-slot source facts; apply only the explicit --apply-reviewed option.

Read public original WOWTBC Forever spec lists and their observed page-data links.
The source says its listed upgrade options are unordered. No ranking is imported.
Four Worgenbane and two Tattered Mittens recommendations additionally have direct
original Icy Veins level-30 spec-guide evidence, recorded below from web review.
"""
import html
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
CATALOGS = ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30"
SPECS = {"Mage": ["arcane", "fire", "frost"], "Priest": ["discipline", "holy", "shadow"],
    "Warlock": ["affliction", "demonology", "destruction"]}

def fetch(url):
    return subprocess.check_output(["pwsh", "-NoProfile", "-Command",
        f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"], encoding="utf-8")

def collect():
    result = []
    for cls, specs in SPECS.items():
        for spec in specs:
            url = f"https://wowtbc.gg/warcraftforever/bis-list/{spec}-{cls.lower()}/"
            raw = fetch(url)
            assert "Items are not in order" in raw and "WoW Forever" in raw
            path = re.search(r'/page-data/warcraftforever/bis-list/[^" ]+/page-data.json', raw)[0]
            context = json.loads(fetch("https://wowtbc.gg" + path))["result"]["pageContext"]
            assert context["spec"] == f"{spec}-{cls.lower()}"
            wanted = [("trinket", 273643)]
            if cls == "Priest":
                wanted.append(("waist", 6392))
            if cls == "Priest" and spec == "shadow":
                wanted.append(("hands", 10654))
            for slot, item_id in wanted:
                listed = context["bisList"][slot]
                listed = listed.split() if isinstance(listed, str) else listed
                assert str(item_id) in [str(value) for value in listed]
                facts = next(row for row in context["gearData"] if row["id"] == item_id)
                result.append({"class": cls, "spec": spec, "itemId": item_id,
                    "recommendationUrl": url, "observedDataUrl": "https://wowtbc.gg" + path,
                    "sourceSelectionMethod": "Original Forever spec list of unordered valuable upgrade alternatives; no rank or equivalence claimed.",
                    "observedSlot": slot, "observedName": facts["name"], "observedSource": facts["source"],
                    "observedSourceType": facts["source_type"]})
    return result

def apply(evidence):
    metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
    quests = json.loads((HERE / "quest-metadata.json").read_text(encoding="utf-8"))
    supplement_path = HERE / "supplemental-recommendations.json"
    supplements = json.loads(supplement_path.read_text(encoding="utf-8"))
    def add_row(row):
        data = metadata[str(row["itemId"])]
        assert data["requiredLevel"] <= 30 and data["slotId"] == row["slotId"]
        for qid in row["questIds"]:
            quest = quests[qid]
            assert quest["minimumLevel"] <= 30
            assert row["itemId"] in [reward[0] for reward in quest["itemChoices"] + quest.get("itemRewards", [])]
        if not any((old["class"], old["spec"], old["itemId"]) == (row["class"], row["spec"], row["itemId"])
            for old in supplements["items"]):
            supplements["items"].append(row)
        path = CATALOGS / f'{row["class"].lower()}-{row["spec"]}.json'
        catalog = json.loads(path.read_text(encoding="utf-8"))
        changed = False
        for slot in (("Trinket1", "Trinket2") if row["slot"] == "Trinket1" else (row["slot"],)):
            if any(old["itemId"] == row["itemId"] and old["slot"] == slot for old in catalog["items"]):
                continue
            note = row["note"]
            if data["uniqueEquipped"]:
                note += " Unique-equipped: one equipped copy."
            source = row["source"] + (" (Horde)" if row["faction"] == "Horde" else "") + ", " + row["location"]
            item = {"id": f'{catalog["catalogId"]}-{slot.lower()}-{row["itemId"]}', "slot": slot, "itemId": row["itemId"],
                "name": row["name"], "requiredSuffix": None, "acquisitionType": row["acquisitionType"], "source": source,
                "note": note, "iconUrl": data["iconUrl"], "itemUrl": data["itemUrl"], "recommendationUrl": row["recommendationUrl"],
                "weaponKind": "None", "uniqueEquipped": data["uniqueEquipped"], "requiredLevel": data["requiredLevel"]}
            if row["acquisitionType"] == "Quest":
                item["availableFactions"] = [row["faction"]] if row["faction"] in ("Alliance", "Horde") else ["Alliance", "Horde"]
            catalog["items"].append(item)
            changed = True
        if changed:
            catalog["supplementarySourceUrls"] = sorted(set(catalog.get("supplementarySourceUrls", []) + [row["recommendationUrl"]]))
            if "Additional missing-slot alternatives" not in catalog["selectionMethod"]:
                catalog["selectionMethod"] += " Additional missing-slot alternatives are independently sourced from each original Forever spec guide/list; no ranking or equal-strength groups inferred."
            path.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    for proof in evidence:
        cls, spec, item_id = proof["class"], proof["spec"], proof["itemId"]
        data = metadata[str(item_id)]
        assert data["name"] == proof["observedName"] and data["requiredLevel"] <= 30
        row = {"class": cls, "spec": spec, "itemId": item_id, "name": data["name"],
            "slot": {"trinket": "Trinket1", "waist": "Waist", "hands": "Hands"}[proof["observedSlot"]], "slotId": data["slotId"],
            "acquisitionType": "Dungeon" if item_id in (273643, 6392) else "Quest",
            "source": proof["observedSource"], "location": "Shadowfang Keep" if item_id in (273643, 6392) else "Stonetalon Mountains",
            "faction": "Horde" if item_id == 10654 else "Both Factions", "questIds": ["3514"] if item_id == 10654 else [],
            "note": "Requires level 15; Horde quest." if item_id == 10654 else "",
            "modifiers": "", "itemUrl": data["itemUrl"], "recommendationUrl": proof["recommendationUrl"]}
        if (cls == "Mage" and spec in ("arcane", "fire")) or (cls == "Priest" and spec in ("holy", "discipline")):
            role = "ranged-dps" if cls == "Mage" else "healer"
            row["recommendationUrl"] = f"https://www.icy-veins.com/wow-forever/{spec}-{cls.lower()}-{role}-pve-guide/"
            row["note"] = ""
        add_row(row)
    for spec in ("discipline", "holy"):
        data = metadata["270030"]
        add_row({"class": "Priest", "spec": spec, "itemId": 270030, "name": data["name"], "slot": "Hands", "slotId": data["slotId"],
            "acquisitionType": "Quest", "source": "The Book of Ur", "location": "Shadowfang Keep", "faction": "Horde", "questIds": ["1013"],
            "note": "Requires level 16; Horde quest in Shadowfang Keep.",
            "modifiers": "", "itemUrl": data["itemUrl"],
            "recommendationUrl": f"https://www.icy-veins.com/wow-forever/{spec}-priest-healer-pve-guide/"})

    supplements["slotAuditRecommendationProof"] = "Original Icy Veins level-30 Arcane/Fire/Holy/Discipline pages list Worgenbane Talisman; Holy/Discipline list Horde Tattered Mittens. Other rows are independently observed in each own original WOWTBC Forever spec list/page-data; unordered upgrade alternatives. See slot-supplement-sources.json."
    supplement_path.write_text(json.dumps(supplements, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

if __name__ == "__main__":
    evidence_path = HERE / "slot-supplement-sources.json"
    if "--collect" in sys.argv:
        evidence = collect()
        evidence_path.write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(len(evidence), "independently observed spec/slot recommendations")
    if "--apply-reviewed" in sys.argv:
        apply(json.loads(evidence_path.read_text(encoding="utf-8")))
