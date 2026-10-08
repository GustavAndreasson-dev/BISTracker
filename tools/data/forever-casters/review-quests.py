"""Check minimum level and faction on the guides' referenced quest pages."""
import concurrent.futures
import json
import re
import subprocess
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
    return str(quest_id), {"questId": quest_id, "name": data["name"],
        "minimumLevel": data.get("reqlevel"), "factionCode": data.get("side"),
        "itemChoices": data.get("itemchoices", []), "questUrl": url}

if __name__ == "__main__":
    ids = set()
    for path in HERE.glob("*-observed.json"):
        for row in json.loads(path.read_text(encoding="utf-8"))["rows"]:
            ids.update(int(value) for value in row["questIds"])
    target = HERE / "quest-metadata.json"
    data = json.loads(target.read_text(encoding="utf-8")) if target.exists() else {}
    missing = sorted(value for value in ids if str(value) not in data)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        for key, value in pool.map(collect, missing):
            data[key] = value
            target.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(len(data), "quest minimum levels and faction codes checked")
    print("Above beta level cap:", [(row['questId'], row['name'], row['minimumLevel']) for row in data.values() if (row['minimumLevel'] or 0) > 30])
