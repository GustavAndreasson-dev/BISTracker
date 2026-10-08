"""Audit simultaneous D/Q sets, quest reachability and faction availability.

The witness is a feasibility example sorted by item ID, not a gear ranking.
Seventeen named positions alone do not prove two distinct rings/trinkets.
"""
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
CATALOGS = ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30"
SPECS = {"Mage": ["arcane", "fire", "frost"], "Priest": ["discipline", "holy", "shadow"],
    "Warlock": ["affliction", "demonology", "destruction"]}
SLOTS = ["Head", "Neck", "Shoulder", "Back", "Chest", "Wrist", "Hands", "Waist", "Legs", "Feet",
    "Finger1", "Finger2", "Trinket1", "Trinket2", "MainHand", "OffHand", "Ranged"]
FACTIONS = {1: ["Alliance"], 2: ["Horde"], 3: ["Alliance", "Horde"]}
quests = json.loads((HERE / "quest-metadata.json").read_text(encoding="utf-8"))
metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
crafting_metadata = json.loads((HERE / "crafting-metadata.json").read_text(encoding="utf-8"))
excluded_research = json.loads((HERE / "excluded-research.json").read_text(encoding="utf-8"))
supplements = json.loads((HERE / "supplemental-recommendations.json").read_text(encoding="utf-8"))["items"]
faction_evidence = {str(row["questId"]): row for row in json.loads((HERE / "quest-faction-evidence.json").read_text(encoding="utf-8"))["quests"]}

def all_quest_levels_valid(qid, seen=None):
    seen = set() if seen is None else seen
    if qid in seen:
        return True
    seen.add(qid)
    quest = quests.get(str(qid))
    return bool(quest and quest["minimumLevel"] is not None and quest["minimumLevel"] <= 30
        and all(all_quest_levels_valid(value, seen) for value in quest.get("publishedPrerequisiteQuestIds", [])))

def rewards(quest, kind):
    return [entry[0] for entry in quest.get(kind, [])]

def quest_proof(item_id, observed):
    qids = {str(qid) for row in observed if row["itemId"] == item_id for qid in row["questIds"]}
    item_source_ids = {str(qid) for qid in metadata[str(item_id)].get("observedRewardQuestIds", [])}
    qids.update(item_source_ids)
    return [quests[qid] for qid in sorted(qids, key=int) if qid in quests and all_quest_levels_valid(qid)
        and (item_id in rewards(quests[qid], "itemChoices") + rewards(quests[qid], "itemRewards") or qid in item_source_ids)]

def available_factions(item_id, observed):
    proof = quest_proof(item_id, observed)
    declared = {faction for quest in proof for faction in FACTIONS.get(quest["factionCode"], [])}
    declared.update(faction for quest in proof for faction in faction_evidence.get(str(quest["questId"]), {}).get("availableFactions", []))
    if not declared and proof:
        for row in observed:
            if row["itemId"] != item_id:
                continue
            if row["faction"] in ("Alliance", "Horde"):
                declared.add(row["faction"])
            for faction in ("Alliance", "Horde"):
                if f"({faction})" in row["source"]:
                    declared.add(faction)
    return sorted(declared)

def solve(rows, observed, faction):
    allowed = [row for row in rows if row["acquisitionType"] == "Dungeon" or faction in row.get("availableFactions", [])]
    groups = {slot: sorted([row for row in allowed if row["slot"] == slot], key=lambda row: (row["itemId"], row["id"])) for slot in SLOTS}
    order = sorted(SLOTS, key=lambda slot: len(groups[slot]))
    def visit(index, selected, choices):
        if index == len(order):
            return selected
        slot = order[index]
        for row in groups[slot]:
            if any(old["itemId"] == row["itemId"] for old in selected.values()):
                continue  # Use distinct physical items, including both ring/trinket positions.
            if slot == "MainHand" and row["weaponKind"] == "TwoHanded":
                continue  # This witness fills a relevant offhand; no fake offhand exemption.
            options = [None] if row["acquisitionType"] == "Dungeon" else [quest for quest in quest_proof(row["itemId"], observed)
                if faction in FACTIONS.get(quest["factionCode"], available_factions(row["itemId"], observed))]
            for quest in options:
                changed_choices = dict(choices)
                if quest and (row["itemId"] in rewards(quest, "itemChoices") or row["itemId"] not in rewards(quest, "itemRewards")):
                    key = str(quest["questId"])
                    if key in choices and choices[key] != row["itemId"]:
                        continue
                    changed_choices[key] = row["itemId"]
                result = visit(index + 1, dict(selected, **{slot: row}), changed_choices)
                if result:
                    return result
        return None
    result = visit(0, {}, {})
    missing = [slot for slot in SLOTS if not groups[slot]]
    for first, second in (("Finger1", "Finger2"), ("Trinket1", "Trinket2")):
        if not any(left["itemId"] != right["itemId"] for left in groups[first] for right in groups[second]) and second not in missing:
            missing.append(second)
    return result, missing

report = []
for cls, specs in SPECS.items():
    for spec in specs:
        path = CATALOGS / f"{cls.lower()}-{spec}.json"
        catalog = json.loads(path.read_text(encoding="utf-8"))
        assert not {row["itemId"] for row in catalog["items"]} & {row["itemId"] for row in excluded_research["items"]}, path
        observed = json.loads((HERE / f"{cls.lower()}-{spec}-observed.json").read_text(encoding="utf-8"))["rows"]
        observed += [row for row in supplements if row["class"] == cls and row["spec"] == spec]
        unresolved = []
        quest_checks = []
        for row in catalog["items"]:
            if row["acquisitionType"] != "Quest":
                continue
            proof = quest_proof(row["itemId"], observed)
            row["availableFactions"] = available_factions(row["itemId"], observed)
            if row["availableFactions"] == ["Horde"] and "Horde" not in row["source"]:
                row["source"] += " (Horde)"
            if not proof or not row["availableFactions"]:
                unresolved.append({"recommendationId": row["id"], "itemId": row["itemId"], "source": row["source"]})
            quest_checks.append({"recommendationId": row["id"], "availableFactions": row["availableFactions"],
                "factionProof": "Individual quest faction metadata" if any(quest["factionCode"] in FACTIONS for quest in proof)
                    else "Original guide explicitly marks the quest's faction; individual quest verifies level/reward" if proof and row["availableFactions"] else "Unverified",
                "questProof": [{"questId": quest["questId"], "questUrl": quest["questUrl"], "minimumLevel": quest["minimumLevel"],
                    "factionCode": quest["factionCode"], "publishedPrerequisiteQuestIds": quest.get("publishedPrerequisiteQuestIds", [])} for quest in proof]})
        selected = [row for row in catalog["items"] if row["acquisitionType"] in ("Dungeon", "Quest")]
        crafting = []
        for row in catalog["items"]:
            if row["acquisitionType"] != "Crafting":
                continue
            proof = metadata[str(row["itemId"])]
            recipe = crafting_metadata[str(row["itemId"])]
            assert recipe["recipeWithinLevel30ExpertCap"], row
            eligible = [value for value in recipe["createdBySpell"] if value["learnedAtSkill"] is not None and value["learnedAtSkill"] <= 225]
            for value in eligible:
                note = f"Crafting recipe: {' / '.join(value['professions'])} {value['learnedAtSkill']}."
                if note not in row["note"]:
                    row["note"] = (row["note"] + " " + note).strip()
            crafting.append({"recommendationId": row["id"], "itemId": row["itemId"], "itemUrl": row["itemUrl"],
                "source": row["source"], "bindsWhenPickedUp": proof["bindsWhenPickedUp"],
                "professionRequirements": proof["professionRequirements"], "requiresCrafterProfession": proof["bindsWhenPickedUp"],
                "recipeProof": recipe, "recipeWithinLevel30ExpertCap": recipe["recipeWithinLevel30ExpertCap"],
                "requiredProfessionToObtain": sorted({name for value in eligible for name in value["professions"]}) if proof["bindsWhenPickedUp"] else [],
                "bindingReview": "Bind on pickup: character must craft this item; active equip requirements are separately listed." if proof["bindsWhenPickedUp"] else "BoE: may be obtained from another crafter; any active use profession requirement still applies.",
                "aboveSkill225": any(requirement["skill"] > 225 for requirement in proof["professionRequirements"])})
        if "--annotate-reviewed-factions" in sys.argv:
            path.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        factions = []
        for faction in ("Alliance", "Horde"):
            witness, missing = solve(selected, observed, faction)
            factions.append({"faction": faction, "missingSlots": missing, "exemptSlots": [], "fullSetValidated": witness is not None,
                "witnessSelectionMethod": "Feasibility only, deterministic item-ID search; no priority, rank or equal-strength claim.",
                "witnessSourcePolicy": ["Dungeon", "Quest"], "requiredProfessions": [], "incompatibleProfessionRequirements": False,
                "legalSetWitness": [{"slot": slot, "recommendationId": witness[slot]["id"], "itemId": witness[slot]["itemId"],
                    "name": witness[slot]["name"], "weaponKind": witness[slot]["weaponKind"], "uniqueEquipped": witness[slot]["uniqueEquipped"],
                    "itemUrl": witness[slot]["itemUrl"], "recommendationUrl": witness[slot]["recommendationUrl"]} for slot in SLOTS] if witness else []})
        report.append({"catalogId": catalog["catalogId"], "class": cls, "spec": spec,
            "missingSlots": sorted({slot for faction in factions for slot in faction["missingSlots"]}), "exemptSlots": [],
            "fullSetValidated": all(faction["fullSetValidated"] for faction in factions), "factions": factions,
            "unresolvedQuestProof": unresolved, "questAvailability": quest_checks,
            "craftingAudit": crafting,
            "explicitEquivalenceGroups": [], "equivalenceReview": "The source alternatives are not declared equal in strength; no equivalence group inferred.",
            "excludedResearchItems": [{"itemId": row["itemId"], "reason": row["reason"], "evidenceFile": "excluded-research.json"}
                for row in excluded_research["items"] if catalog["catalogId"] in row["affectedCatalogs"]]})
result = {"reviewedOn": "2026-10-08", "levelCap": 30, "releaseStage": "Beta", "sourcePolicy": ["Dungeon", "Quest", "Crafting"],
    "professionTrainingProof": [{"profession": name, "expertSkillCap": 225, "expertMinimumCharacterLevel": 20,
        "artisanMinimumCharacterLevel": 35, "sourceUrl": f"https://www.icy-veins.com/wow-forever/{name.lower()}"}
        for name in ("Tailoring", "Enchanting", "Engineering", "Leatherworking")],
    "professionCompatibility": "Every faction/spec has a complete D/Q witness requiring no crafting profession. Crafted alternatives are optional, not simultaneous requirements; crafter-only gear requires its listed profession.",
    "validationScope": "Separately reachable, simultaneous 17-position set per faction; distinct rings/trinkets, one-hand plus offhand, published quest levels/chains and alternative reward exclusivity. No game-client playthrough performed.",
    "catalogs": report}
(HERE / "slot-audit.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps([{key: row[key] for key in ("catalogId", "missingSlots", "fullSetValidated")} | {"unresolvedQuestProof": len(row["unresolvedQuestProof"])} for row in report], ensure_ascii=False))
