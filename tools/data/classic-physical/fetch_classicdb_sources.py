"""Second source for item acquisition and quest faction facts: classicdb.ch (1.12 database).

Used because Wowhead Classic item/quest pages were rate limited (HTTP 403) during
the review; Wowhead's tooltip API stays the source for name/icon/level/unique.

Usage:
  python -I fetch_classicdb_sources.py <cache-dir> <out.json> items <id> [<id> ...]
  python -I fetch_classicdb_sources.py <cache-dir> <out.json> quests <id> [<id> ...]
The output file is merged (existing keys kept unless refetched).
"""
import html
import json
import re
import sys
import time
import urllib.request
from pathlib import Path

UA = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) BISTracker-data-review'}
ZONES = {1583: 'Blackrock Spire', 1584: 'Blackrock Depths', 2017: 'Stratholme', 2057: 'Scholomance',
         2100: 'Maraudon', 2557: 'Dire Maul', 2717: 'Molten Core', 2159: "Onyxia's Lair", 1337: 'Uldaman',
         1477: "The Temple of Atal'Hakkar", 139: 'Eastern Plaguelands', 28: 'Western Plaguelands', 47: 'The Hinterlands',
         51: 'Searing Gorge', 46: 'Burning Steppes', 8: 'Swamp of Sorrows'}


def get(url, path):
    if path.exists() and path.stat().st_size > 0:
        return path.read_text(encoding='utf-8')
    req = urllib.request.Request(url, headers=UA)
    with urllib.request.urlopen(req, timeout=60) as r:
        text = r.read().decode('utf-8', 'replace')
    path.write_text(text, encoding='utf-8')
    time.sleep(1.0)
    return text


def listview_objects(page, lv_id):
    m = re.search(r"id: ?'%s'.*?data: ?\[(.*?)\]\}\);" % re.escape(lv_id), page, re.S)
    if not m:
        return []
    objs = []
    for body in re.findall(r'\{([^{}]*)\}', m.group(1)):
        o = {}
        for key in ('itemchoices', 'itemrewards'):
            mm = re.search(key + r':\[((?:\[[^\]]*\],?)*)\]', body)
            if mm:
                o[key] = [int(x) for x in re.findall(r'\[(\d+),', mm.group(1))]
                body = body.replace(mm.group(0), '')
        for k, v in re.findall(r"(\w+):('(?:[^'\\]|\\.)*'|\[[^\]]*\]|-?[\d.]+)", body):
            if k in o:
                continue
            if v.startswith("'"):
                v = v[1:-1].replace("\\'", "'")
                o[k] = int(v) if re.fullmatch(r'-?\d+', v) else v
            elif v.startswith('['):
                o[k] = [int(x) for x in re.findall(r'-?\d+', v)]
            else:
                o[k] = float(v) if '.' in v else int(v)
        objs.append(o)
    return objs


def text_of(page):
    t = re.sub(r'<script.*?</script>', ' ', page, flags=re.S)
    t = re.sub(r'<[^>]+>', ' ', t)
    return re.sub(r'\s+', ' ', html.unescape(t))


def item_facts(item_id, cache):
    page = get('https://classicdb.ch/?item=%d' % item_id, cache / ('cdb-item%d.html' % item_id))
    def npcs(lv):
        return [{'npcId': o.get('id'), 'name': o.get('name'), 'zones': [ZONES.get(z, z) for z in (o.get('location') if isinstance(o.get('location'), list) else [o.get('location')] if o.get('location') is not None else [])],
                 'percent': o.get('percent')} for o in listview_objects(page, lv)[:10]]
    return {
        'droppedBy': npcs('dropped-by'),
        'soldBy': npcs('sold-by'),
        'rewardFrom': [{'questId': o.get('id'), 'name': o.get('name'), 'reqlevel': o.get('reqlevel'),
                        'side': {1: 'Alliance', 2: 'Horde', 3: 'Both'}.get(o.get('side'), o.get('side')),
                        'choice': item_id in o.get('itemchoices', []), 'fixedReward': item_id in o.get('itemrewards', []),
                        'otherChoices': [x for x in o.get('itemchoices', []) if x != item_id]}
                       for o in listview_objects(page, 'reward-of')],
        'createdBy': [{'spellId': o.get('id'), 'name': o.get('name'), 'learnedAt': o.get('learnedat'),
                       'skill': o.get('skill')} for o in listview_objects(page, 'created-by')],
        'containedIn': [{'objectId': o.get('id'), 'name': o.get('name'),
                         'zones': [ZONES.get(z, z) for z in (o.get('location') if isinstance(o.get('location'), list) else [o.get('location')] if o.get('location') is not None else [])]}
                        for o in listview_objects(page, 'contained-in-object')],
        'url': 'https://classicdb.ch/?item=%d' % item_id,
    }


def quest_facts(quest_id, cache):
    page = get('https://classicdb.ch/?quest=%d' % quest_id, cache / ('cdb-quest%d.html' % quest_id))
    t = text_of(page)
    side = re.search(r'Side: (Both|Alliance|Horde)', t)
    req = re.search(r'Requires level: (\d+)', t)
    lvl = re.search(r'Level: (\d+)', t)
    title = re.search(r'<title>([^<]*?) - Quest', page)
    choice = re.search(r'You can choose one of these rewards: (.*?)(?: You will also receive| Gains |$)', t)
    fixed = re.search(r'You will receive: (.*?)(?: Gains |$)', t)
    return {'name': html.unescape(title.group(1)) if title else None,
            'side': side.group(1) if side else None,
            'level': int(lvl.group(1)) if lvl else None,
            'requiredLevel': int(req.group(1)) if req else None,
            'choiceRewardsText': choice.group(1).strip() if choice else None,
            'fixedRewardsText': fixed.group(1).strip() if fixed else None,
            'url': 'https://classicdb.ch/?quest=%d' % quest_id}


def main():
    cache, out_path, kind = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
    cache.mkdir(parents=True, exist_ok=True)
    out = json.loads(out_path.read_text(encoding='utf-8')) if out_path.exists() else {'items': {}, 'quests': {}}
    for raw in sys.argv[4:]:
        i = int(raw)
        if kind == 'items':
            out['items'][str(i)] = item_facts(i, cache)
            print(i, json.dumps(out['items'][str(i)], ensure_ascii=False)[:300], flush=True)
        else:
            out['quests'][str(i)] = quest_facts(i, cache)
            print(i, json.dumps(out['quests'][str(i)], ensure_ascii=False)[:300], flush=True)
    out_path.write_text(json.dumps(out, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
