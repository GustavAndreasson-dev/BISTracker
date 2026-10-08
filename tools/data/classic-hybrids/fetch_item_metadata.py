"""Collect review metadata for Classic items from two observed public sources.

Usage: python -I tools/data/classic-hybrids/fetch_item_metadata.py <cacheDir> <out.json> <itemId>...

1. Wowhead Classic tooltip endpoint (nether.wowhead.com, the same observed
   endpoint as tools/data/Get-ItemMetadata.ps1): name, quality, icon and
   tooltip facts (slot, type, required level, binding, Unique, random
   enchantment, profession requirement).
2. classicdb.ch item page (Vanilla 1.12 database): "phase" field, dropped-by
   NPCs with zone IDs, quest rewards with side (1 Alliance, 2 Horde, 3 both)
   and quest required level, vendors, and creating spells. For a creating
   spell the teaching recipe item and its own sources/phase are followed.

Neither endpoint is a stable API. Raw responses are cached under cacheDir.
The result is review input; catalog decisions are made separately.
"""
import json
import re
import sys
import time
import urllib.request
from pathlib import Path

TOOLTIP = "https://nether.wowhead.com/classic/tooltip/item/{}?dataEnv=4&locale=0"
CLASSICDB = "https://classicdb.ch/?{}={}"
ZONES = {2017: "Stratholme", 2057: "Scholomance", 1584: "Blackrock Depths", 1583: "Blackrock Spire",
         1477: "Sunken Temple", 2100: "Maraudon", 2557: "Dire Maul", 1176: "Zul'Farrak",
         721: "Gnomeregan", 796: "Scarlet Monastery", 1337: "Uldaman", 491: "Razorfen Kraul",
         722: "Razorfen Downs", 209: "Shadowfang Keep", 717: "The Stockade", 719: "Blackfathom Deeps",
         718: "Wailing Caverns", 1581: "The Deadmines", 2437: "Ragefire Chasm", 2717: "Molten Core",
         2159: "Onyxia's Lair", 2677: "Blackwing Lair", 1977: "Zul'Gurub", 3428: "Ahn'Qiraj Temple",
         3429: "Ruins of Ahn'Qiraj", 3456: "Naxxramas", 2597: "Alterac Valley", 3277: "Warsong Gulch",
         3358: "Arathi Basin"}
SKILLS = {164: "Blacksmithing", 165: "Leatherworking", 197: "Tailoring", 202: "Engineering",
          333: "Enchanting", 171: "Alchemy"}
SIDES = {"1": ["Alliance"], "2": ["Horde"], "3": ["Alliance", "Horde"]}


def get(cache, name, url):
    path = cache / name
    for attempt in range(5):
        if path.exists() and path.stat().st_size > 500:
            return path.read_text(encoding="utf-8", errors="replace")
        request = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
        try:
            with urllib.request.urlopen(request, timeout=40) as response:
                path.write_bytes(response.read())
        except Exception:  # transient network/rate errors; retried below
            time.sleep(5 * (attempt + 1))
        time.sleep(0.7)
    raise RuntimeError(f"Could not fetch {url}")


def listview(page, view_id):
    match = re.search(r"id:'" + re.escape(view_id) + r"'.*?data:\[(.*?)\]\}\);", page, re.S)
    return match.group(1) if match else ""


def records(data):
    # Records are flat JS objects separated by "},{"; nested arrays stay inside the record text.
    return [chunk for chunk in re.split(r"\},\s*\{", data.strip().strip("{}")) if chunk] if data else []


def field(record, key):
    match = re.search(r"\b" + key + r":('([^']*)'|\[([^\]]*)\]|(-?[\d.]+))", record)
    if not match:
        return None
    return match.group(2) if match.group(2) is not None else (match.group(3) if match.group(3) is not None
                                                              else match.group(4))


def classicdb_item(cache, item_id):
    page = get(cache, f"cdb-item-{item_id}.html", CLASSICDB.format("item", item_id))
    phase = re.search(r"phase: (\d+)", page)
    drops = [{"npc": field(r, "name"), "npcId": int(field(r, "id")),
              "zones": [ZONES.get(int(z), f"zone {z}") for z in (field(r, "location") or "").split(",") if z],
              "percent": float(field(r, "percent") or 0)}
             for r in records(listview(page, "dropped-by"))]
    quests = [{"questId": int(field(r, "id")), "name": field(r, "name"),
               "factions": SIDES.get(field(r, "side"), ["unknown side " + str(field(r, "side"))]),
               "questLevel": field(r, "level"), "requiredLevel": field(r, "reqlevel")}
              for r in records(listview(page, "reward-of"))]
    vendors = [field(r, "name") for r in records(listview(page, "sold-by"))]
    objects = [field(r, "name") for r in records(listview(page, "contained-in-object"))]
    spells = [{"spellId": int(field(r, "id")),
               "skill": SKILLS.get(int((field(r, "skill") or "0").split(",")[0] or 0), field(r, "skill")),
               "colors": field(r, "colors")} for r in records(listview(page, "created-by"))]
    return {"phase": int(phase.group(1)) if phase else None, "droppedBy": drops, "rewardOf": quests,
            "soldBy": vendors, "containedIn": objects, "createdBy": spells}


def recipes(cache, spell):
    page = get(cache, f"cdb-spell-{spell['spellId']}.html", CLASSICDB.format("spell", spell["spellId"]))
    taught = [{"recipeItemId": int(field(r, "id")), "name": field(r, "name").lstrip("0123456789")}
              for r in records(listview(page, "taught-by-item"))]
    trainers = [field(r, "name") for r in records(listview(page, "taught-by-npc"))]
    for recipe in taught:
        facts = classicdb_item(cache, recipe["recipeItemId"])
        recipe_tooltip = tooltip(cache, recipe["recipeItemId"])
        recipe.update({"requirements": recipe_tooltip["requirements"],
                       "bindsOnPickup": recipe_tooltip["bindsOnPickup"], "phase": facts["phase"], "soldBy": facts["soldBy"], "rewardOf": facts["rewardOf"],
                       "dropCount": len(facts["droppedBy"]),
                       "dropZones": sorted({z for d in facts["droppedBy"] for z in d["zones"]}),
                       "topDrops": sorted(facts["droppedBy"], key=lambda d: -d["percent"])[:5]})
    return {"taughtByItem": taught, "taughtByTrainer": trainers}


def tooltip(cache, item_id):
    data = json.loads(get(cache, f"tt-{item_id}.json", TOOLTIP.format(item_id)))
    html = data.get("tooltip", "")
    text = re.sub(r"\s+", " ", re.sub(r"<[^>]+>", " ", html.replace("<br>", " | "))).strip()
    slot = re.search(r"<table width=\"100%\"><tr><td>([^<]+)</td>(?:<th>(?:<!--[^>]*-->)?(?:<span[^>]*>)?([^<]*))?",
                     html)
    level = re.search(r"Requires level (\d+)", text, re.I)
    profession = re.search(r"Requires ((?:Tribal |Dragonscale |Elemental )?Leatherworking|Tailoring|"
                           r"Blacksmithing|Engineering|Enchanting|Weaponsmith|Armorsmith|Master \w+smith)"
                           r"(?: \((\d+)\))?", text)
    return {"name": data.get("name"), "quality": data.get("quality"), "icon": data.get("icon"),
            "tooltipSlot": slot.group(1).strip() if slot else None,
            "tooltipType": (slot.group(2) or "").strip() if slot else None,
            "requirements": re.findall(r"Requires [^|<]+?(?= Requires | \||$| Use:| Equip:)", text),
            "requiredLevel": int(level.group(1)) if level else 0,
            "bindsOnPickup": "Binds when picked up" in text, "bindsOnEquip": "Binds when equipped" in text,
            "unique": bool(re.search(r"\bUnique\b", text)),
            "randomEnchantment": "Random enchantment" in text or "Random Bonuses" in text,
            "requiresProfession": profession.group(0) if profession else None,
            "tooltipText": text}


if __name__ == "__main__":
    cache = Path(sys.argv[1])
    cache.mkdir(parents=True, exist_ok=True)
    out = Path(sys.argv[2])
    result = json.loads(out.read_text(encoding="utf-8")) if out.exists() else {}
    for raw in sys.argv[3:]:
        item_id = int(raw)
        entry = {"itemId": item_id, **tooltip(cache, item_id), "classicdb": classicdb_item(cache, item_id)}
        for spell in entry["classicdb"]["createdBy"]:
            spell.update(recipes(cache, spell))
        entry["sources"] = {"tooltip": TOOLTIP.format(item_id), "classicdb": CLASSICDB.format("item", item_id)}
        result[str(item_id)] = entry
        print(item_id, entry["name"], entry["tooltipSlot"], "phase", entry["classicdb"]["phase"])
    out.write_text(json.dumps(dict(sorted(result.items(), key=lambda kv: int(kv[0]))),
                              ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
