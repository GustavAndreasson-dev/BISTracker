"""Check whether ClassicDB boss pages list specific items in their loot.

Used for items whose ClassicDB item page has no "dropped by" data.

Usage: python -I check-boss-loot.py <cache-dir> <output.json> <npcId>:<itemId>[,<itemId>] ...
"""
import json
import re
import sys
import urllib.request

UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
      "(KHTML, like Gecko) Chrome/120 Safari/537.36")


def page(cache, npc_id):
    path = "%s/cdb-npc-%s.html" % (cache, npc_id)
    try:
        with open(path, encoding="utf-8") as handle:
            return handle.read()
    except FileNotFoundError:
        request = urllib.request.Request("https://classicdb.ch/?npc=%s" % npc_id, headers={"User-Agent": UA})
        with urllib.request.urlopen(request, timeout=30) as response:
            text = response.read().decode("utf-8", errors="replace")
        with open(path, "w", encoding="utf-8") as handle:
            handle.write(text)
        return text


def main():
    cache, output = sys.argv[1:3]
    results = []
    for arg in sys.argv[3:]:
        npc_id, items = arg.split(":")
        text = page(cache, npc_id)
        title = re.search(r"<title>([^<]*)", text)
        zone = re.search(r"location:\[(\d+)\]", text)
        for item_id in items.split(","):
            match = re.search(r"\{[^{}]*\bid:%s\b[^{}]*\}" % item_id, text)
            percent = re.search(r"percent:([\d.]+)", match.group(0)) if match else None
            results.append({"npcId": int(npc_id), "npcPage": title.group(1).strip() if title else None,
                            "itemId": int(item_id), "listedInLoot": bool(match),
                            "percent": float(percent.group(1)) if percent else None})
    with open(output, "w", encoding="utf-8") as handle:
        json.dump({"source": "https://classicdb.ch/?npc=<id> (drops tab)", "results": results}, handle, indent=2)
    for row in results:
        print(row)


if __name__ == "__main__":
    main()
