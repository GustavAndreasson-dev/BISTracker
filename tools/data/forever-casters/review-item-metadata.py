"""Read actual Forever item pages; save only reviewed factual tooltip metadata."""
import concurrent.futures
import html
import json
import re
import subprocess
from pathlib import Path

OUT = Path(__file__).resolve().parents[3] / "docs/data/forever-casters"

def gather(raw, kind):
    marker = f"WH.Gatherer.addData({kind}, 16, "
    combined = {}
    for match in re.finditer(re.escape(marker), raw):
        combined.update(json.JSONDecoder().raw_decode(raw[match.end():])[0])
    return combined

def fetch(url):
    return subprocess.check_output(["pwsh", "-NoProfile", "-Command",
        f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"], encoding="utf-8")

def collect(pair):
    item_id, url = pair
    raw = fetch(url)
    marker = f"g_items[{item_id}].tooltip_enus = "
    index = raw.find(marker)
    tooltip = json.JSONDecoder().raw_decode(raw[index + len(marker):])[0] if index >= 0 else ""
    data = gather(raw, 3)[str(item_id)]
    links = re.findall(r'https://wow\.zamimg\.com/images/wow/icons/large/[^"\s<>]+\.jpg', raw)
    icon = next((link for link in links if link.endswith("/" + data["icon"] + ".jpg")), None)
    plain = html.unescape(re.sub("<[^>]*>", " ", tooltip))
    return item_id, {"itemId": item_id, "itemUrl": url, "name": data["name_enus"],
        "iconUrl": icon, "uniqueEquipped": "Unique-Equipped" in tooltip or "<br>Unique<" in tooltip,
        "bindsWhenPickedUp": "Binds when picked up" in tooltip,
        "requiredLevel": data.get("jsonequip", {}).get("reqlevel", 0),
        "slotId": data.get("jsonequip", {}).get("slotbak"),
        "professionRequirements": [{"profession": profession, "skill": int(skill)}
            for profession, skill in re.findall(r"Requires ([A-Za-z ]+) \((\d+)\)", plain)],
        "uniqueProof": "Unique-Equipped" if "Unique-Equipped" in tooltip else "Unique" if "<br>Unique<" in tooltip else None}

if __name__ == "__main__":
    pairs = {}
    for path in OUT.glob("*-observed.json"):
        for row in json.loads(path.read_text(encoding="utf-8"))["rows"]:
            if row["slot"]:
                pairs[row["itemId"]] = row["itemUrl"]
    target = OUT / "item-metadata.json"
    existing = json.loads(target.read_text(encoding="utf-8")) if target.exists() else {}
    for data in existing.values():
        if "tooltipFacts" in data:
            data["professionRequirements"] = [{"profession": profession, "skill": int(skill)}
                for profession, skill in re.findall(r"Requires ([A-Za-z ]+) \((\d+)\)", data["tooltipFacts"])]
            data["uniqueProof"] = "Unique-Equipped or Unique in observed tooltip" if data["uniqueEquipped"] else None
            del data["tooltipFacts"]
    target.write_text(json.dumps(existing, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    pairs = {k: v for k, v in pairs.items() if str(k) not in existing}
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        for index, (item_id, data) in enumerate(pool.map(collect, pairs.items()), 1):
            existing[str(item_id)] = data
            target.write_text(json.dumps(existing, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            if index % 10 == 0 or index == len(pairs):
                print(index, "/", len(pairs), "new item pages verified", flush=True)
