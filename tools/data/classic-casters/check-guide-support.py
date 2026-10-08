"""Check that every selected alternative is named by its cited guide section.

Usage: python -I check-guide-support.py <selection.json> <guide-rows-observed.json> <report.json>

Allowed sections per spec: Icy Veins Mage phase 1-4 lists (Frost/Arcane) or
phase 5-6 lists (Fire); Zockify "Pre-BiS" sections; all Icy Veins
Warlock/Priest pre-raid tables; whole Warcraft Tavern Warlock and Defcamp
lists; Seebus' first (phase 1) gear block. A suffix row also needs the suffix
text in at least one supporting row.
"""
import json
import sys

with open(sys.argv[1], encoding="utf-8") as handle:
    selection = json.load(handle)
with open(sys.argv[2], encoding="utf-8") as handle:
    guides = json.load(handle)
by_url = {g["url"]: (k, g["rows"]) for k, g in guides.items()}


def seebus_phase1(rows):
    kept, heads, previous = [], 0, None
    for row in rows:
        if row["section"] == "Head" and previous != "Head":
            heads += 1
        previous = row["section"]
        if heads >= 2:
            break
        if heads == 1:
            kept.append(row)
    return kept


def allowed(key, rows, spec):
    if key == "icyveins-mage":
        phases = ("Phase 5", "Phase 6") if spec == "mage-fire" else ("Phase 1", "Phase 2", "Phase 3", "Phase 4")
        return [r for r in rows if any(p in r["section"] for p in phases)]
    if key.startswith("zockify"):
        return [r for r in rows if "Pre-BiS" in r["section"]]
    if key == "warcrafttavern-holy-seebus":
        return seebus_phase1(rows)
    return rows


problems, checked = [], 0
targets = [(name, spec["alternatives"]) for name, spec in selection["specs"].items()]
targets.append(("priest-holy-additions", selection["holyAdditions"]["alternatives"]))
evidence = {}
for name, alternatives in targets:
    for alt in alternatives:
        checked += 1
        key, rows = by_url[alt["recommendationUrl"]]
        support = [r for r in allowed(key, rows, name) if alt["itemId"] in r["itemIds"]]
        if not support:
            problems.append("%s %s: not named in %s allowed sections" % (name, alt["itemId"], key))
            continue
        suffix = alt.get("requiredSuffix")
        if suffix and not any(suffix.lower().replace("of ", "") in r["text"].lower() for r in support):
            problems.append("%s %s: suffix %s not in supporting rows" % (name, alt["itemId"], suffix))
        evidence.setdefault(name, []).append({"itemId": alt["itemId"], "guide": key,
                                              "sections": sorted({r["section"] for r in support})})

with open(sys.argv[3], "w", encoding="utf-8", newline="\n") as handle:
    json.dump({"checked": checked, "problems": problems, "evidence": evidence}, handle, indent=2, ensure_ascii=False)
    handle.write("\n")
print("checked", checked, "problems", len(problems))
for problem in problems:
    print(" -", problem)
