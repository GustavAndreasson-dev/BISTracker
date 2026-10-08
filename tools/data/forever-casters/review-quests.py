"""Check minimum level and faction on the guides' referenced quest pages."""
import concurrent.futures
import json
import re
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parents[3] / "docs/data/forever-casters"

def collect(quest_id):
    url = f"https://www.wowhead.com/forever/quest={quest_id}"
    raw = subprocess.check_output(["pwsh", "-NoProfile", "-Command",
        f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"], encoding="utf-8")
    marker = f"$.extend(g_quests[{quest_id}], "
    index = raw.find(marker)
    if index < 0:
        raise ValueError(f"Missing quest data: {quest_id}")
    data = json.JSONDecoder().raw_decode(raw[index + len(marker):])[0]
    series = re.search(r'<table class="series">([\s\S]*?)</table>', raw)
    previous = series[1].split("<b>", 1)[0] if series else ""
    return str(quest_id), {"questId": quest_id, "name": data["name"],
        "minimumLevel": data.get("reqlevel"), "factionCode": data.get("side"),
        "itemChoices": data.get("itemchoices", []), "itemRewards": data.get("itemrewards", []),
        "publishedPrerequisiteQuestIds": sorted({int(value) for value in re.findall(r'/forever/quest=(\d+)', previous)}),
        "questUrl": url}

if __name__ == "__main__":
    ids = set()
    if "--extra-quests" in sys.argv:
        ids.update(int(value) for value in sys.argv[sys.argv.index("--extra-quests") + 1].split(","))
    for path in HERE.glob("*-observed.json"):
        for row in json.loads(path.read_text(encoding="utf-8"))["rows"]:
            ids.update(int(value) for value in row["questIds"])
    item_metadata = json.loads((HERE / "item-metadata.json").read_text(encoding="utf-8"))
    for item in item_metadata.values():
        ids.update(item.get("observedRewardQuestIds", []))
    target = HERE / "quest-metadata.json"
    data = json.loads(target.read_text(encoding="utf-8")) if target.exists() else {}
    missing = sorted(value for value in ids if str(value) not in data or "itemRewards" not in data[str(value)]
        or "--refresh" in sys.argv)
    while missing:
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
            for key, value in pool.map(collect, missing):
                data[key] = value
                target.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        missing = sorted({qid for quest in data.values() for qid in quest.get("publishedPrerequisiteQuestIds", [])
            if str(qid) not in data})
    print(len(data), "quest minimum levels and faction codes checked")
    print("Above beta level cap:", [(row['questId'], row['name'], row['minimumLevel']) for row in data.values() if (row['minimumLevel'] or 0) > 30])
