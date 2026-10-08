"""Fetch crafting facts for crafted candidate items from Wowhead Classic.

Usage:
  python -I fetch_recipe_sources.py <item-metadata.json> <cache-dir> <out.json> <itemId> [<itemId> ...]

For each crafted item: the creating spell (skill line, learned-at rank,
specialisation requirement text) and the recipe item(s) that teach it
(taught-by-item listview) with their own drop/vendor/quest/object sources.
"""
import json
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

UA = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) BISTracker-data-review'}
SKILLS = {165: 'Leatherworking', 164: 'Blacksmithing', 202: 'Engineering', 197: 'Tailoring'}


def get(url, path):
    if path.exists() and path.stat().st_size > 0:
        return path.read_text(encoding='utf-8')
    for attempt in range(4):
        try:
            req = urllib.request.Request(url, headers=UA)
            with urllib.request.urlopen(req, timeout=60) as r:
                text = r.read().decode('utf-8', 'replace')
            break
        except urllib.error.HTTPError:
            if attempt == 3:
                raise
            time.sleep(30 * (attempt + 1))
    path.write_text(text, encoding='utf-8')
    time.sleep(1.5)
    return text


def listview(page, lv_id):
    i = page.find("id: '%s'" % lv_id)
    if i < 0:
        return []
    j = page.find('data: [', i)
    value, _ = json.JSONDecoder().raw_decode(page[j + 6:])
    return value


def main():
    meta = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
    cache = Path(sys.argv[2])
    cache.mkdir(parents=True, exist_ok=True)
    out = {}
    for item_id in sys.argv[4:]:
        facts = meta[item_id]
        rows = []
        for spell in facts['createdBy']:
            sp = get('https://www.wowhead.com/classic/spell=%d' % spell['spellId'], cache / ('spell%d.html' % spell['spellId']))
            spec = re.findall(r'Requires (Dragonscale Leatherworking|Tribal Leatherworking|Elemental Leatherworking|'
                              r'Weaponsmith|Armorsmith|Master Axesmith|Master Swordsmith|Master Hammersmith)', sp)
            recipes = []
            for rec in listview(sp, 'taught-by-item'):
                rp = get('https://www.wowhead.com/classic/item=%d' % rec['id'], cache / ('item%d.html' % rec['id']))
                drops = listview(rp, 'dropped-by')
                recipes.append({
                    'recipeItemId': rec['id'], 'name': rec['name'],
                    'droppedByCount': len(drops),
                    'droppedBySample': [{'name': d['name'], 'location': d.get('location')} for d in drops[:6]],
                    'soldBy': [{'name': s['name'], 'location': s.get('location'), 'react': s.get('react')} for s in listview(rp, 'sold-by')[:6]],
                    'rewardFrom': [{'questId': q['id'], 'name': q['name'], 'side': q.get('side')} for q in listview(rp, 'reward-from-q')[:6]],
                    'containedIn': [{'name': o['name'], 'location': o.get('location')} for o in listview(rp, 'contained-in-object')[:6]],
                })
            trainers = listview(sp, 'taught-by-npc')
            rows.append({'spellId': spell['spellId'], 'name': spell['name'],
                         'profession': SKILLS.get((spell.get('skill') or [None])[0]), 'learnedAt': spell['learnedAt'],
                         'specialisationRequirementText': sorted(set(spec)),
                         'taughtByTrainer': [t['name'] for t in trainers[:6]],
                         'recipes': recipes})
        out[item_id] = {'name': facts['name'], 'bindsOnPickup': facts['bindsOnPickup'], 'spells': rows}
        print(item_id, facts['name'], json.dumps(rows, ensure_ascii=False)[:400], flush=True)
    Path(sys.argv[3]).write_text(json.dumps(out, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
