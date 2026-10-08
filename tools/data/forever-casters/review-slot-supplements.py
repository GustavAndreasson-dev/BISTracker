"""Collect only named missing-slot alternatives from original Forever sources.

Review the resulting evidence before applying it to the shipped catalogues.
Earlier level-20 beta recommendations are identified explicitly, never relabelled
as a level-30 ranking. Current Forever item pages independently verify usability.
"""
import html
import importlib.util
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[3]
HERE = ROOT / "docs/data/forever-casters"
module = importlib.util.spec_from_file_location("caster_items", Path(__file__).with_name("review-item-metadata.py"))
items = importlib.util.module_from_spec(module)
module.loader.exec_module(items)

def clean(value):
    return html.unescape(re.sub(r"\[[^\]]*\]|<[^>]*>", " ", value)).strip()

def wowhead_twenty(cls, spec, wanted):
    url = f"https://www.wowhead.com/forever/guide/classes/{cls.lower()}/{spec}/level-20-dps-overview"
    raw = items.fetch(url)
    body = next(json.loads(match[0]) for match in re.finditer(r'"(?:[^"\\]|\\.)*"', raw) if '[h2 toc=' in match[0])
    result = []
    for match in re.finditer(r"\[tr[^\]]*\]([\s\S]*?)\[/tr\]", body):
        for item_id in wanted:
            if re.search(r"\[item=" + str(item_id) + r"(?:\]| )", match[1]):
                heading = list(re.finditer(r"\[h2[^\]]*\]([\s\S]*?)\[/h2\]", body[:match.start()]))[-1][1]
                if "Dungeon" in heading or "Quest" in heading:
                    result.append({"class": cls, "spec": spec, "itemId": item_id,
                        "recommendationUrl": url, "guideLevelCap": 20,
                        "section": clean(heading), "observedRecommendation": clean(match[1])})
    return result

if __name__ == "__main__":
    observed = []
    for cls, spec, wanted in [("Mage", "frost", [273298]),
        ("Warlock", "affliction", [273298]), ("Warlock", "demonology", [273298]),
        ("Warlock", "destruction", [273298]), ("Priest", "shadow", [270030, 10654])]:
        rows = wowhead_twenty(cls, spec, wanted)
        observed.extend(rows)
        print(cls, spec, [(row["itemId"], row["observedRecommendation"]) for row in rows], flush=True)
    (HERE / "earlier-beta-slot-sources.json").write_text(json.dumps(observed, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
    for item_id, slug in [(273298, "lookies-spyglass"), (273643, "worgenbane-talisman"), (3456, "dog-whistle")]:
        key, data = items.collect((item_id, f"https://www.wowhead.com/forever/item={item_id}/{slug}"))
        metadata[str(key)] = data
        print(item_id, data["name"], data["slotId"], data["requiredLevel"], data["uniqueProof"], flush=True)
    for item_id in (281635, 281265, 3461, 271740, 270036):
        data = metadata[str(item_id)]
        raw = items.fetch(data["itemUrl"])
        related = re.search(r"id: 'reward-from-q',[\s\S]*?data: (\[[\s\S]*?\]),\s*\}\);", raw)
        assert related, item_id
        source_quests = json.loads(related[1])
        data["observedRewardQuestIds"] = [quest["id"] for quest in source_quests]
        data["rewardSourceProof"] = "Observed reward-from-q table on this individual Forever item page."
        print(item_id, "individual item reward quests", data["observedRewardQuestIds"], flush=True)
    (HERE / "item-metadata.json").write_text(json.dumps(metadata, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
