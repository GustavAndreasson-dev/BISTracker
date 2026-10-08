"""Record every item-linked guide row with its section heading.

Usage:
  python -I extract-guide-rows.py <output.json> <guideKey>=<url>=<text-file> [...]

Text files come from guide-to-text.py. The result is the observed guide
evidence; recommendation and filtering decisions are made separately.
"""
import json
import re
import sys


def rows(path):
    heading = ""
    found = []
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            line = line.strip()
            if line.startswith("## "):
                heading = line[3:].strip()
                continue
            ids = [int(x) for x in re.findall(r"\[#(\d+)\]", line)]
            if ids:
                found.append({"section": heading, "itemIds": ids, "text": line[:400]})
    return found


def main():
    output = sys.argv[1]
    result = {}
    candidates = set()
    for arg in sys.argv[2:]:
        key, rest = arg.split("=", 1)
        url, path = rest.rsplit("=", 1)
        found = rows(path)
        result[key] = {"url": url, "rows": found}
        for row in found:
            candidates.update(row["itemIds"])
    with open(output, "w", encoding="utf-8") as handle:
        json.dump(result, handle, indent=2, ensure_ascii=False)
    print(json.dumps({"items": sorted(candidates)}))


if __name__ == "__main__":
    main()
