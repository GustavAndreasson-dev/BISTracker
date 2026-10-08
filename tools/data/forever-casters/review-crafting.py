"""Verify actual Forever crafting spells, recipe skill and item binding."""
import concurrent.futures
import importlib.util
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
spec = importlib.util.spec_from_file_location("metadata", Path(__file__).with_name("review-item-metadata.py"))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
PROFESSIONS = {197: "Tailoring", 333: "Enchanting", 202: "Engineering", 165: "Leatherworking", 164: "Blacksmithing"}

def collect(pair):
    item_id, url = pair
    raw = module.fetch(url)
    marker = re.search(r"id:\s*['\"]created-by-spell['\"]", raw)
    recipes = []
    if marker:
        data = re.search(r"\bdata:\s*", raw[marker.end():])
        values = json.JSONDecoder().raw_decode(raw[marker.end() + data.end():])[0]
        for value in values:
            if value.get("creates", [None])[0] == item_id:
                recipes.append({"spellId": value["id"], "name": value["displayName"],
                    "learnedAtSkill": value.get("learnedat"), "professionIds": value.get("skill", []),
                    "professions": [PROFESSIONS.get(key, str(key)) for key in value.get("skill", [])],
                    "spellUrl": f"https://www.wowhead.com/forever/spell={value['id']}",
                    "recipeSourceTypes": value.get("source", [])})
    return str(item_id), {"itemId": item_id, "itemUrl": url, "createdBySpell": recipes,
        "minimumVerifiedRecipeSkill": min((recipe["learnedAtSkill"] for recipe in recipes if recipe["learnedAtSkill"] is not None), default=None),
        "recipeWithinLevel30ExpertCap": any(recipe["learnedAtSkill"] is not None and recipe["learnedAtSkill"] <= 225 for recipe in recipes)}

if __name__ == "__main__":
    pairs = {row["itemId"]: row["itemUrl"] for path in (ROOT / "BISTracker.Infrastructure/Catalog/Data/Forever/level30").glob("*.json")
        if path.name.startswith(("mage-", "priest-", "warlock-"))
        for row in json.loads(path.read_text(encoding="utf-8"))["items"] if row["acquisitionType"] == "Crafting"}
    for item_id in sys.argv[1:]:
        pairs[int(item_id)] = f"https://www.wowhead.com/forever/item={int(item_id)}"
    target = HERE / "crafting-metadata.json"
    existing = json.loads(target.read_text(encoding="utf-8")) if target.exists() else {}
    pairs = {key: value for key, value in pairs.items() if str(key) not in existing}
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        for index, (key, result) in enumerate(pool.map(collect, pairs.items()), 1):
            existing[key] = result
            target.write_text(json.dumps(existing, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            if index % 10 == 0 or index == len(pairs):
                print(index, "/", len(pairs), "crafting recipe tables verified", flush=True)
