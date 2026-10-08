"""Collect Classic item metadata for the caster pre-raid review.

Usage:
  python -I fetch-item-metadata.py <cache-dir> <candidates.json> <output.json>

candidates.json: {"items": [itemId, ...]}.

Sources per item (raw responses are cached in <cache-dir>):
- Wowhead Classic tooltip API (name, icon, quality, binding, unique, slot,
  item type, required level, class restriction).
- ClassicDB item page (content phase, dropped-by NPCs with zone ids,
  quest rewards, container objects, vendors, created-by spells).
- For crafted items: ClassicDB spell page (required skill, recipe items) and
  the recipe item page (recipe phase, drops, vendors, quests, binding).

The output is research evidence. It does not decide recommendations.
"""
import html
import json
import os
import re
import sys
import time
import urllib.request

UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
      "(KHTML, like Gecko) Chrome/120 Safari/537.36")

ZONES = {
    1583: "Blackrock Spire", 1584: "Blackrock Depths", 2017: "Stratholme",
    2057: "Scholomance", 1477: "Sunken Temple", 2100: "Maraudon",
    2557: "Dire Maul", 1176: "Zul'Farrak", 1337: "Uldaman",
    2717: "Molten Core", 2159: "Onyxia's Lair", 1977: "Zul'Gurub",
    796: "Scarlet Monastery", 722: "Razorfen Downs", 491: "Razorfen Kraul",
    209: "Shadowfang Keep", 719: "Blackfathom Deeps", 721: "Gnomeregan",
    1581: "The Deadmines", 717: "The Stockade", 718: "Wailing Caverns",
    2437: "Ragefire Chasm", 139: "Eastern Plaguelands", 28: "Western Plaguelands",
    618: "Winterspring", 490: "Un'Goro Crater", 361: "Felwood",
    16: "Azshara", 3: "Badlands", 46: "Burning Steppes", 51: "Searing Gorge",
    1377: "Silithus", 3428: "Ahn'Qiraj", 3429: "Ruins of Ahn'Qiraj",
    2677: "Blackwing Lair", 3456: "Naxxramas",
}
SKILLS = {197: "Tailoring", 165: "Leatherworking", 164: "Blacksmithing",
          202: "Engineering", 333: "Enchanting", 171: "Alchemy",
          755: "Jewelcrafting"}


def fetch(cache, key, url):
    path = os.path.join(cache, key)
    if os.path.exists(path):
        with open(path, encoding="utf-8", errors="replace") as handle:
            return handle.read()
    request = urllib.request.Request(url, headers={"User-Agent": UA,
                                                   "Accept-Language": "en-US"})
    for attempt in range(3):
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                text = response.read().decode("utf-8", errors="replace")
            break
        except Exception as error:  # network retry, evidence is cached
            if attempt == 2:
                raise RuntimeError("%s: %s" % (url, error))
            time.sleep(2)
    with open(path, "w", encoding="utf-8") as handle:
        handle.write(text)
    time.sleep(0.4)
    return text


def listviews(page):
    views = {}
    for match in re.finditer(r"new Listview\(\{(.*?)\}\);", page, re.S):
        body = match.group(1)
        ident = re.search(r"id:\s*'([\w-]+)'", body)
        data = re.search(r"data:\s*(\[.*\])\s*$", body, re.S)
        if not ident or not data:
            continue
        objects = re.findall(r"\{[^{}]*\}", data.group(1))
        views[ident.group(1)] = objects
    return views


def field(obj, name):
    match = re.search(r"(?:^|[{,])\s*%s:\s*('(?:[^'\\]|\\.)*'|\[[^\]]*\]|[^,}]+)" % name, obj)
    if not match:
        return None
    value = match.group(1).strip()
    if value.startswith("'"):
        return value[1:-1].replace("\\'", "'")
    if value.startswith("["):
        return [int(x) for x in re.findall(r"-?\d+", value)]
    try:
        return float(value) if "." in value else int(value)
    except ValueError:
        return value


def strip_quality(name):
    return re.sub(r"^\d", "", name or "")


def parse_tooltip(raw):
    data = json.loads(raw)
    tip = data.get("tooltip", "")
    text = html.unescape(re.sub(r"<br\s*/?>", "\n", tip))
    text = re.sub(r"<[^>]+>", " ", text)
    text = re.sub(r"[ \t]+", " ", text)
    lines = [line.strip() for line in text.split("\n") if line.strip()]
    flat = " | ".join(lines)
    name = data.get("name") or ""
    # The item name comes first; skip it so names such as "Dragon Finger"
    # or "Lunar Wand" are not read as the equipment slot.
    body = flat[len(name):] if flat.startswith(name) else flat
    slot_match = re.search(r"\b(Head|Neck|Shoulder|Back|Chest|Wrist|Hands|Waist|Legs|Feet|Finger|Trinket|"
                           r"One-Hand|Two-Hand|Main Hand|Off Hand|Held In Off-hand|Ranged|Wand|Relic)\b", body)
    level = re.search(r"Requires Level (\d+)", flat)
    classes = re.search(r"Classes: ([A-Za-z, ]+?)(?= Requires| \||$)", flat)
    skill = re.search(r"Requires (Engineering|Tailoring|Leatherworking|Blacksmithing) \((\d+)\)", flat)
    binding = ("BoP" if "Binds when picked up" in flat else
               "BoE" if "Binds when equipped" in flat else
               "Quest" if "Quest Item" in flat else "None")
    return {
        "name": data.get("name"),
        "quality": data.get("quality"),
        "icon": data.get("icon"),
        "binding": binding,
        "unique": "Unique" in flat,
        "slotText": slot_match.group(1) if slot_match else None,
        "requiredLevel": int(level.group(1)) if level else None,
        "classes": classes.group(1).strip() if classes else None,
        "equipSkill": "%s %s" % (skill.group(1), skill.group(2)) if skill else None,
        "tooltipText": flat,
    }


def npc_entry(obj):
    location = field(obj, "location") or []
    if isinstance(location, int):
        location = [location]
    return {
        "npcId": field(obj, "id"),
        "name": field(obj, "name"),
        "zones": [ZONES.get(z, z) for z in location],
        "classification": field(obj, "classification"),
        "percent": field(obj, "percent"),
        "react": field(obj, "react"),
    }


def quest_entry(obj):
    return {
        "questId": int(field(obj, "id")),
        "name": field(obj, "name"),
        "requiredLevel": field(obj, "reqlevel"),
        "classicdbSide": {1: "Alliance", 2: "Horde", 3: "Both"}.get(
            int(field(obj, "side") or 0), field(obj, "side")),
        "type": field(obj, "type"),
    }


def classicdb_item(cache, item_id):
    page = fetch(cache, "cdb-item-%d.html" % item_id, "https://classicdb.ch/?item=%d" % item_id)
    phase = re.search(r"Added in content phase: (\d)", page)
    views = listviews(page)
    result = {
        "phase": int(phase.group(1)) if phase else None,
        "droppedBy": [npc_entry(o) for o in views.get("dropped-by", [])],
        "rewardOf": [quest_entry(o) for o in views.get("reward-of", [])],
        "containedInObject": [{"objectId": field(o, "id"), "name": field(o, "name"),
                               "percent": field(o, "percent")}
                              for o in views.get("contained-in-object", [])],
        "soldBy": [npc_entry(o) for o in views.get("sold-by", [])],
        "createdBy": [],
    }
    for obj in views.get("created-by", []):
        spell_id = int(field(obj, "id"))
        skills = field(obj, "skill") or []
        colors = field(obj, "colors") or []
        result["createdBy"].append({
            "spellId": spell_id,
            "spell": strip_quality(field(obj, "name")),
            "profession": SKILLS.get(skills[0], skills[0]) if skills else None,
            "skill": colors[0] if colors else None,
            "recipes": classicdb_spell(cache, spell_id),
        })
    result["droppedBy"].sort(key=lambda n: -(n["percent"] or 0))
    return result


def classicdb_spell(cache, spell_id):
    page = fetch(cache, "cdb-spell-%d.html" % spell_id, "https://classicdb.ch/?spell=%d" % spell_id)
    views = listviews(page)
    recipes = []
    for obj in views.get("taught-by-item", []):
        recipe_id = int(field(obj, "id"))
        recipe_page = fetch(cache, "cdb-item-%d.html" % recipe_id,
                            "https://classicdb.ch/?item=%d" % recipe_id)
        recipe_views = listviews(recipe_page)
        phase = re.search(r"Added in content phase: (\d)", recipe_page)
        tip = parse_tooltip(fetch(cache, "wh-tip-%d.json" % recipe_id,
                                  "https://nether.wowhead.com/classic/tooltip/item/%d?dataEnv=4&locale=0" % recipe_id))
        drops = [npc_entry(o) for o in recipe_views.get("dropped-by", [])]
        drops.sort(key=lambda n: -(n["percent"] or 0))
        recipes.append({
            "recipeItemId": recipe_id,
            "name": strip_quality(field(obj, "name")),
            "binding": tip["binding"],
            "phase": int(phase.group(1)) if phase else None,
            "droppedBy": drops[:12],
            "droppedByCount": len(drops),
            "soldBy": [npc_entry(o) for o in recipe_views.get("sold-by", [])],
            "rewardOf": [quest_entry(o) for o in recipe_views.get("reward-of", [])],
            "containedInObject": [{"objectId": field(o, "id"), "name": field(o, "name")}
                                  for o in recipe_views.get("contained-in-object", [])],
        })
    trainers = [npc_entry(o) for o in views.get("taught-by-npc", [])]
    if trainers:
        recipes.append({"trainer": True, "trainers": trainers[:5]})
    return recipes


def main():
    cache, candidates, output = sys.argv[1:4]
    os.makedirs(cache, exist_ok=True)
    with open(candidates, encoding="utf-8") as handle:
        ids = sorted(set(json.load(handle)["items"]))
    def one(item_id):
        tip = parse_tooltip(fetch(cache, "wh-tip-%d.json" % item_id,
                                  "https://nether.wowhead.com/classic/tooltip/item/%d?dataEnv=4&locale=0" % item_id))
        entry = {"itemId": item_id}
        entry.update(tip)
        entry["classicdb"] = classicdb_item(cache, item_id)
        drops = entry["classicdb"]["droppedBy"]
        entry["classicdb"]["droppedByCount"] = len(drops)
        entry["classicdb"]["droppedBy"] = drops[:12]
        print(item_id, entry["name"], "phase", entry["classicdb"]["phase"], file=sys.stderr, flush=True)
        return entry

    from concurrent.futures import ThreadPoolExecutor
    with ThreadPoolExecutor(max_workers=4) as pool:
        entries = list(pool.map(one, ids))
    result = {str(e["itemId"]): e for e in entries}
    with open(output, "w", encoding="utf-8") as handle:
        json.dump({"retrievedOn": time.strftime("%Y-%m-%d"),
                   "sources": ["https://nether.wowhead.com/classic/tooltip/item/<id>",
                               "https://classicdb.ch/?item=<id>",
                               "https://classicdb.ch/?spell=<id>"],
                   "items": result}, handle, indent=2, ensure_ascii=False)


if __name__ == "__main__":
    main()
