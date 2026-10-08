"""Write the nine Classic phase 1 physical catalogs from the reviewed selection.

Usage: python -I build_catalogs.py <repo-root>

Inputs (docs/data/classic-physical/):
  selection.json      per catalog: guide, weapon setup and the kept rows (slot + itemId)
  item-sources.json   reviewed acquisition facts per item (type, source text, factions, note)
  item-metadata.json  Wowhead Classic facts (name, icon, level, unique, slot)
Item name, required level, unique and icon always come from item-metadata.json.
"""
import json
import sys
from pathlib import Path

ROOT = Path(sys.argv[1])
FACTS = ROOT / 'docs/data/classic-physical'
OUT = ROOT / 'BISTracker.Infrastructure/Catalog/Data/Classic/phase1'
SLOT_ORDER = ['Head', 'Neck', 'Shoulder', 'Back', 'Chest', 'Wrist', 'Hands', 'Waist', 'Legs', 'Feet',
              'Finger1', 'Finger2', 'Trinket1', 'Trinket2', 'MainHand', 'OffHand', 'Ranged']
KIND = {'Main Hand': 'MainHand', 'One-Hand': 'OneHanded', 'Two-Hand': 'TwoHanded', 'Off Hand': 'OffHand'}
PAIRED = {'Finger1': 'Finger2', 'Finger2': 'Finger1', 'Trinket1': 'Trinket2', 'Trinket2': 'Trinket1',
          'MainHand': 'OffHand', 'OffHand': 'MainHand'}

read = lambda name: json.loads((FACTS / name).read_text(encoding='utf-8'))
selection = read('selection.json')
sources = read('item-sources.json')
meta = read('item-metadata.json')
OUT.mkdir(parents=True, exist_ok=True)

for cat in selection['catalogs']:
    cls, spec = cat['characterClass'], cat['specializationId']
    catalog_id = 'classic-phase1-level60-%s-%s' % (cls.lower(), spec)
    rows = []
    slots_of = {}
    for sel in cat['rows']:
        slots_of.setdefault(sel['itemId'], []).append(sel['slot'])
    for sel in sorted(cat['rows'], key=lambda r: (SLOT_ORDER.index(r['slot']), cat['rows'].index(r))):
        iid = sel['itemId']
        m, s = meta[str(iid)], sources[str(iid)]
        slot = sel['slot']
        kind = KIND.get(m['slotText'], 'None') if slot in ('MainHand', 'OffHand') else 'None'
        notes = []
        if s.get('note'):
            notes.append(s['note'])
        if sel.get('note'):
            notes.append(sel['note'])
        if PAIRED.get(slot) in slots_of[iid]:
            notes.append('Alternative placement; choose one slot per owned copy.')
        if m['unique']:
            notes.append('Unique.')
        row = {
            'id': '%s-%s-%d' % (catalog_id, slot.lower(), iid),
            'slot': slot,
            'itemId': iid,
            'name': m['name'],
            'requiredSuffix': None,
            'requiredLevel': m['requiredLevel'],
            'acquisitionType': s['acquisitionType'],
            'source': s['source'],
            'note': ' '.join(notes),
            'iconUrl': 'https://wow.zamimg.com/images/wow/icons/large/%s.jpg' % m['icon'],
            'itemUrl': 'https://www.wowhead.com/classic/item=%d' % iid,
            'recommendationUrl': sel.get('recommendationUrl', cat['sourceUrl']),
            'weaponKind': kind,
            'uniqueEquipped': m['unique'],
        }
        if s.get('availableFactions'):
            row['availableFactions'] = s['availableFactions']
        rows.append(row)
    doc = {
        'schemaVersion': 1,
        'catalogId': catalog_id,
        'gameVersion': 'Classic',
        'characterClass': cls,
        'specializationId': spec,
        'levelCap': 60,
        'releaseStage': 'Classic',
        'phase': 'Phase 1 · pre-raid',
        'sourceUrl': cat['sourceUrl'],
        'reviewedOn': selection['reviewedOn'],
        'selectionMethod': cat['selectionMethod'],
        'status': cat['status'],
        'weaponSetup': cat['weaponSetup'],
    }
    if cat['weaponSetup'] != 'Flexible':
        doc['weaponSetupSourceUrl'] = cat['weaponSetupSourceUrl']
    doc['items'] = rows
    path = OUT / ('%s-%s.json' % (cls.lower(), spec))
    path.write_text(json.dumps(doc, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(path.name, len(rows), 'rows')
