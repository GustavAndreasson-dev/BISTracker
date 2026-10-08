"""Collect original Forever level-30 guide rows for manual catalogue review.

The guide is BBCode embedded in public Wowhead HTML, accompanied by the site's
item and quest metadata. This script is research tooling, not runtime scraping.
"""
import html
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "docs/data/forever-casters"
SPECS = {
    "Mage": ("arcane", "fire", "frost"),
    "Priest": ("discipline", "holy", "shadow"),
    "Warlock": ("affliction", "demonology", "destruction"),
}
SLOTS = {1: "Head", 2: "Neck", 3: "Shoulder", 5: "Chest", 6: "Waist",
         7: "Legs", 8: "Feet", 9: "Wrist", 10: "Hands", 11: "Finger1",
         12: "Trinket1", 13: "MainHand", 14: "OffHand", 15: "Ranged",
         16: "Back", 17: "MainHand", 20: "Chest", 21: "MainHand",
         22: "OffHand", 23: "OffHand", 26: "Ranged"}
SKILLS = {197: "Tailoring", 333: "Enchanting", 202: "Engineering", 171: "Alchemy", 185: "Cooking"}

def gather(raw, kind):
    marker = f"WH.Gatherer.addData({kind}, 16, "
    index = raw.find(marker)
    return json.JSONDecoder().raw_decode(raw[index + len(marker):])[0] if index >= 0 else {}

def strip(text, quests):
    text = re.sub(r"\[quest=(\d+)[^\]]*\]", lambda m: quests.get(m[1], {}).get("name_enus", f"quest {m[1]}"), text)
    # Name is observed in WH.Gatherer.addData(17, 16, ...) on these guides.
    text = text.replace("[currency=3402]", "Merchant's Favor")
    return html.unescape(re.sub(r"\[[^\]]*\]", "", text)).strip()

def collect(cls, spec):
    role = "healer" if cls == "Priest" and spec != "shadow" else "dps"
    url = f"https://www.wowhead.com/forever/guide/classes/{cls.lower()}/{spec}/level-30-{role}-overview"
    raw = subprocess.check_output(["pwsh", "-NoProfile", "-Command",
        f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"], encoding="utf-8")
    items, quests = gather(raw, 3), gather(raw, 5)
    body = next(json.loads(m[0]) for m in re.finditer(r'"(?:[^"\\]|\\.)*"', raw) if '[h2 toc=' in m[0])
    rows = []
    parts = re.split(r"(\[h2[^\]]*\][\s\S]*?\[/h2\])", body)
    for ix in range(1, len(parts), 2):
        heading, content = strip(parts[ix], quests), parts[ix + 1]
        if "Level 30" not in heading or not any(word in heading for word in ("Dungeon Drops", "Quest Rewards", "Crafted Gear")):
            continue
        kind = "Dungeon" if "Dungeon Drops" in heading else "Quest" if "Quest Rewards" in heading else "Crafting"
        blocks = list(re.finditer(r"\[table[^\]]*\]([\s\S]*?)\[/table\]", content))
        for table in blocks:
            pre = content[:table.start()]
            bold = list(re.finditer(r"\[b\]([\s\S]*?)\[/b\]", pre))
            group = strip(bold[-1][1], quests) if bold else ""
            tabs = list(re.finditer(r'\[tab name="([^"]+)"\]', pre))
            if tabs:
                group = tabs[-1][1]
            if group in ("Boss", "Quest", "Profession", "Item", "Reward", "Encounter"):
                group = ""
            location = group
            faction = ""
            profession = ""
            table_note = ""
            for row in re.finditer(r"\[tr[^\]]*\]([\s\S]*?)\[/tr\]", table[1]):
                cells = re.findall(r"\[td[^\]]*\]([\s\S]*?)\[/td\]", row[1])
                if len(cells) < 2:
                    if cells:
                        label = strip(cells[0], quests).strip(":")
                        skill = re.search(r"\[skill=(\d+)\]", cells[0])
                        if skill:
                            profession = SKILLS[int(skill[1])]
                            table_note = ""
                            continue
                        if not label:
                            continue
                        if label in ("Alliance", "Horde", "Both Factions"):
                            faction = label
                        else:
                            if len(label) > 50:
                                table_note = label
                            else:
                                location = label
                    continue
                source = strip(cells[0], quests)
                qids = re.findall(r"\[quest=(\d+)", cells[0])
                sides = {quests[qid].get("_side") for qid in qids if qid in quests}
                rowfaction = "Alliance" if sides == {1} else "Horde" if sides == {2} else faction
                source = re.sub(r"\s+", " ", source)
                for target in re.finditer(r"\[item=(\d+)([^\]]*)\]", cells[1]):
                    itemid, modifiers = target[1], target[2].strip()
                    data = items[itemid]
                    equip = data.get("jsonequip", {})
                    slotid = equip.get("slotbak")
                    link = re.search(r'href="(/forever/item=' + itemid + r'[^" ]*)"', raw)
                    if not link:
                        raise ValueError(f"No observed item link for {itemid}")
                    between = cells[1][target.end():]
                    between = between.split("[item=", 1)[0]
                    note = re.sub(r"\s+", " ", strip(between, quests)).strip()
                    if table_note and data.get("name_enus", "").startswith(("Pearly", "Filigreed Pearly")):
                        note = table_note + " " + note
                    rows.append({"itemId": int(itemid), "name": data["name_enus"],
                                 "slot": SLOTS.get(slotid), "slotId": slotid,
                                 "requiredLevel": equip.get("reqlevel", 0),
                                 "itemUrl": "https://www.wowhead.com" + html.unescape(link[1]),
                                 "source": profession or source if kind == "Crafting" else source, "location": location,
                                 "faction": rowfaction, "questIds": qids,
                                 "acquisitionType": kind, "note": note,
                                 "modifiers": modifiers, "iconName": data.get("icon"),
                                 "rawMetadata": equip})
    if cls == "Mage":
        # The guide explicitly chooses one reward for this spec after listing
        # all three choices. Keep that authored choice rather than infer one.
        chosen = re.search(r"For " + spec.capitalize() + r",[^\n]*pick \[item=(\d+)\]", body)
        if not chosen:
            raise ValueError(f"Missing explicit wand reward choice for {spec}")
        itemid = chosen[1]
        data = items[itemid]
        equip = data["jsonequip"]
        link = re.search(r'href="(/forever/item=' + itemid + r'[^" ]*)"', raw)
        rows.append({"itemId": int(itemid), "name": data["name_enus"],
            "slot": SLOTS[equip["slotbak"]], "slotId": equip["slotbak"],
            "requiredLevel": equip.get("reqlevel", 0), "itemUrl": "https://www.wowhead.com" + html.unescape(link[1]),
            "source": "Mage's Wand", "location": "Mage class quest chain",
            "faction": "Both Factions", "questIds": ["1952"], "acquisitionType": "Quest",
            "note": "Quest chain starts at level 30 and requires Scarlet Monastery: Library; challenging at the beta cap.",
            "modifiers": "", "iconName": data.get("icon"), "rawMetadata": equip})
    return {"class": cls, "spec": spec, "sourceUrl": url, "rows": rows}

if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    for cls, specs in SPECS.items():
        for spec in specs:
            result = collect(cls, spec)
            (OUT / f"{cls.lower()}-{spec}-observed.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            valid = [row for row in result["rows"] if row["slot"] and row["requiredLevel"] <= 30]
            print(cls, spec, len(result["rows"]), "observed;", len(valid), "level-eligible")
