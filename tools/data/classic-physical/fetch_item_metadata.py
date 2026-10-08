"""Fetch Wowhead Classic item facts for the physical-group candidate items.

Usage:
  python -I fetch_item_metadata.py <candidates.json> <cache-dir> <out.json>

<candidates.json> is docs/data/classic-physical/guide-candidates.json. Every
distinct itemId in it is looked up on Wowhead Classic:
  * tooltip API  https://nether.wowhead.com/classic/tooltip/item/<id>
  * item page    https://www.wowhead.com/classic/item=<id>
Raw downloads are cached in <cache-dir> (keep it outside the repository).
The output keeps only facts used by the catalogs: name, icon, quality, binding,
unique, slot text, armor/weapon type, required level, random enchantment and
the dropped-by / reward-from / created-by / contained-in / sold-by listviews.
"""
import json
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

UA = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) BISTracker-data-review'}
ZONES = {1583: 'Blackrock Spire', 1584: 'Blackrock Depths', 2017: 'Stratholme', 2057: 'Scholomance',
         2100: 'Maraudon', 2557: 'Dire Maul', 2717: 'Molten Core', 2159: "Onyxia's Lair", 1337: 'Uldaman',
         1477: 'The Temple of Atal\'Hakkar', 1176: "Zul'Farrak", 19: "Zul'Gurub", 2677: 'Blackwing Lair',
         3428: "Ahn'Qiraj", 3429: "Ruins of Ahn'Qiraj", 3456: 'Naxxramas'}


def get(url, path):
    if path.exists() and path.stat().st_size > 0:
        return path.read_text(encoding='utf-8')
    text = None
    for attempt in range(2):
        try:
            req = urllib.request.Request(url, headers=UA)
            with urllib.request.urlopen(req, timeout=60) as r:
                text = r.read().decode('utf-8', 'replace')
            break
        except urllib.error.HTTPError:
            time.sleep(15)
    if text is None:
        return None  # rate limited; rerun later, cached pages are reused
    path.write_text(text, encoding='utf-8')
    time.sleep(1.5)
    return text


def listview(page, lv_id):
    i = page.find("id: '%s'" % lv_id)
    if i < 0:
        return None
    j = page.find('data: [', i)
    if j < 0:
        return None
    value, _ = json.JSONDecoder().raw_decode(page[j + 6:])
    return value


def tooltip_facts(tt):
    html = tt['tooltip']
    slot = re.search(r'<table width="100%"><tr><td>([^<]+)</td>(?:<th>(?:<!--[^>]*-->)?(?:<span[^>]*>)?([^<]*))?', html) or         re.search(r'<br>(Back|Neck|Finger|Trinket|Shirt|Tabard)<', html)
    req = re.search(r'Requires Level <!--rlvl-->(\d+)', html)
    return {
        'name': tt['name'], 'quality': tt['quality'], 'icon': tt['icon'],
        'bindsOnPickup': 'Binds when picked up' in html,
        'bindsOnEquip': 'Binds when equipped' in html,
        'unique': bool(re.search(r'<br>Unique(-Equipped)?<', html)),
        'uniqueText': (re.search(r'<br>(Unique(?:-Equipped)?)<', html) or [None, None])[1],
        'slotText': slot.group(1) if slot else None,
        'typeText': (slot.group(2) if slot and slot.re.groups > 1 else None),
        'requiredLevel': int(req.group(1)) if req else None,
        'randomEnchantment': '&lt;Random enchantment&gt;' in html,
        'requiresProfession': (re.search(r'Requires ((?:Leatherworking|Blacksmithing|Engineering|Tailoring|Dragonscale Leatherworking|Tribal Leatherworking|Elemental Leatherworking|Weaponsmith|Armorsmith|Master [A-Za-z]+smith)[^<]*)', html) or [None, None])[1],
    }


def main():
    candidates = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
    cache = Path(sys.argv[2])
    cache.mkdir(parents=True, exist_ok=True)
    ids = sorted({row['itemId'] for guide in candidates['guides'] for row in guide['items'] if row['itemId']})
    out = {}
    for item_id in ids:
        tt = json.loads(get('https://nether.wowhead.com/classic/tooltip/item/%d' % item_id, cache / ('tt%d.json' % item_id)))
        page = get('https://www.wowhead.com/classic/item=%d' % item_id, cache / ('item%d.html' % item_id))
        facts = tooltip_facts(tt)
        facts['pageUnavailable'] = page is None
        page = page or ''
        src = re.search(r'"id":%d,[^{}]*?"source":(\[[^\]]*\])(?:,"sourcemore":(\[[^\]]*\]))?' % item_id, page)
        facts['sourceCodes'] = json.loads(src.group(1)) if src else None
        facts['sourceMore'] = json.loads(src.group(2)) if src and src.group(2) else None
        drops = listview(page, 'dropped-by') or []
        facts['droppedBy'] = [{'npcId': d['id'], 'name': d['name'],
                               'zones': [ZONES.get(z, z) for z in d.get('location', [])],
                               'count': d.get('count'), 'outof': d.get('outof')} for d in drops[:12]]
        facts['droppedByTotal'] = len(drops)
        quests = listview(page, 'reward-from-q') or []
        facts['rewardFrom'] = [{'questId': q['id'], 'name': q['name'], 'side': {1: 'Alliance', 2: 'Horde', 3: 'Both'}.get(q.get('side'), q.get('side')),
                                'reqlevel': q.get('reqlevel'), 'level': q.get('level'),
                                'choice': any(c[0] == item_id for c in q.get('itemchoices', [])),
                                'fixedReward': any(c[0] == item_id for c in q.get('itemrewards', []))} for q in quests]
        spells = listview(page, 'created-by-spell') or []
        facts['createdBy'] = [{'spellId': s['id'], 'name': s['name'], 'skill': s.get('skill'), 'learnedAt': s.get('learnedat')} for s in spells]
        objs = listview(page, 'contained-in-object') or []
        facts['containedIn'] = [{'objectId': o['id'], 'name': o['name'], 'zones': [ZONES.get(z, z) for z in o.get('location', [])]} for o in objs[:8]]
        sold = listview(page, 'sold-by') or []
        facts['soldBy'] = [{'npcId': s['id'], 'name': s['name']} for s in sold[:5]]
        out[str(item_id)] = facts
        print(item_id, facts['name'], facts['slotText'], facts['typeText'], facts['requiredLevel'],
              'U' if facts['unique'] else '-', 'R' if facts['randomEnchantment'] else '-', flush=True)
    Path(sys.argv[3]).write_text(json.dumps(out, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
