"""HEAD-check every icon URL used by the caster catalogs and Holy proposal.

Usage: python -I check-icons.py <output.json> <json-file> [...]
"""
import json
import sys
import urllib.request

urls = set()
for path in sys.argv[2:]:
    with open(path, encoding="utf-8") as handle:
        urls.update(row["iconUrl"] for row in json.load(handle)["items"] if row.get("iconUrl"))
status = {}
for url in sorted(urls):
    request = urllib.request.Request(url, method="HEAD", headers={"User-Agent": "Mozilla/5.0"})
    try:
        with urllib.request.urlopen(request, timeout=20) as response:
            status[url] = response.status
    except Exception as error:
        status[url] = str(error)
with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as handle:
    json.dump(status, handle, indent=2)
    handle.write("\n")
bad = {u: s for u, s in status.items() if s != 200}
print(len(status), "icons,", len(bad), "not HTTP 200", bad)
