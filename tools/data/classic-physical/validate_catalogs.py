"""Validate the nine Classic phase 1 physical catalogs against docs/classic-phase1-catalogs.md.

Usage: python -I validate_catalogs.py <repo-root> <report.json>

Checks the schema-1 contract (root fields, fixed Classic values, catalog/item
ID patterns, enum names from EquipmentSlot.cs and ItemDetails.cs, HTTPS links,
weapon kind vs slot, quest factions), cross-checks every row against the saved
Wowhead metadata (name, required level, unique, slot family) and builds one
faction-feasible full set per faction (distinct physical items, unique and
one-reward-per-quest-choice respected, at most two professions).
"""
import json
import re
import sys
from itertools import product
from pathlib import Path

ROOT = Path(sys.argv[1])
DATA = ROOT / 'BISTracker.Infrastructure/Catalog/Data/Classic/phase1'
FACTS = ROOT / 'docs/data/classic-physical'
SPECS = {'Hunter': ['beast-mastery', 'marksmanship', 'survival'], 'Rogue': ['assassination', 'combat', 'subtlety'],
         'Warrior': ['arms', 'fury', 'protection']}


def enum_names(path, enum):
    text = (ROOT / path).read_text(encoding='utf-8')
    body = re.search(r'enum %s\s*\{([^}]*)\}' % enum, text).group(1)
    return [x.strip() for x in body.replace('\n', ',').split(',') if x.strip()]


SLOTS = enum_names('BISTracker.Domain/Equipment/EquipmentSlot.cs', 'EquipmentSlot')
ACQ = enum_names('BISTracker.Domain/Equipment/ItemDetails.cs', 'AcquisitionType')
WEAPON = enum_names('BISTracker.Domain/Equipment/ItemDetails.cs', 'WeaponKind')
FACTIONS = enum_names('BISTracker.Domain/Equipment/ItemDetails.cs', 'CharacterFaction')
ALLOWED_ACQ = {'Dungeon', 'Quest', 'Crafting'}
SLOT_TEXT = {'Head': {'Head'}, 'Neck': {'Neck'}, 'Shoulder': {'Shoulder'}, 'Back': {'Back'}, 'Chest': {'Chest'},
             'Wrist': {'Wrist'}, 'Hands': {'Hands'}, 'Waist': {'Waist'}, 'Legs': {'Legs'}, 'Feet': {'Feet'},
             'Finger1': {'Finger'}, 'Finger2': {'Finger'}, 'Trinket1': {'Trinket'}, 'Trinket2': {'Trinket'},
             'MainHand': {'Main Hand', 'One-Hand', 'Two-Hand'}, 'OffHand': {'Off Hand', 'One-Hand'},
             'Ranged': {'Ranged', 'Thrown'}}
KIND_TEXT = {'Main Hand': 'MainHand', 'One-Hand': 'OneHanded', 'Two-Hand': 'TwoHanded', 'Off Hand': 'OffHand'}
ROOT_FIELDS = ['schemaVersion', 'catalogId', 'gameVersion', 'characterClass', 'specializationId', 'levelCap',
               'releaseStage', 'phase', 'sourceUrl', 'reviewedOn', 'selectionMethod', 'status', 'items']
ITEM_FIELDS = ['id', 'slot', 'itemId', 'name', 'acquisitionType', 'source', 'note', 'itemUrl', 'recommendationUrl',
               'weaponKind', 'uniqueEquipped']

meta = json.loads((FACTS / 'item-metadata.json').read_text(encoding='utf-8'))
errors, summary = [], []


def err(cat, msg):
    errors.append('%s: %s' % (cat, msg))


def https(v):
    return isinstance(v, str) and v.startswith('https://')


def faction_set(cat, rows, faction):
    """Greedy-with-backtracking search for one legal full set for a faction."""
    usable = [r for r in rows if r['acquisitionType'] != 'Quest' or faction in (r.get('availableFactions') or [])]
    setup = cat.get('weaponSetup', 'Flexible')
    exempt = {e['slot'] for e in cat.get('slotExemptions', [])}
    targets = [s for s in SLOTS if s not in exempt]
    by_slot = {s: [r for r in usable if r['slot'] == s] for s in targets}
    order = sorted(targets, key=lambda s: len(by_slot[s]))
    best = {'missing': None}

    def quest_key(r):
        return r.get('_questChoice', {}).get(faction)

    def search(i, chosen, used_items, quest_used, professions):
        if i == len(order):
            return dict(chosen)
        slot = order[i]
        main = chosen.get('MainHand')
        if slot == 'OffHand' and main and main['weaponKind'] == 'TwoHanded':
            return search(i + 1, chosen, used_items, quest_used, professions)
        options = by_slot[slot]
        if slot == 'MainHand':
            if setup == 'TwoHanded':
                options = [r for r in options if r['weaponKind'] == 'TwoHanded']
            elif setup == 'OneHandAndOffHand':
                options = [r for r in options if r['weaponKind'] != 'TwoHanded']
        for r in options:
            if r['itemId'] in used_items:
                continue
            qk = quest_key(r)
            if qk and qk in quest_used:
                continue
            prof = r.get('_profession')
            new_prof = professions | ({prof} if prof and r.get('_requiresProfession') else set())
            if len(new_prof) > 2:
                continue
            chosen[slot] = r
            res = search(i + 1, chosen, used_items | {r['itemId']}, quest_used | ({qk} if qk else set()), new_prof)
            if res:
                return res
            del chosen[slot]
        if best['missing'] is None:
            best['missing'] = slot
        return None

    # order MainHand before OffHand so the two-hand rule can be applied
    order.sort(key=lambda s: (s == 'OffHand', len(by_slot[s])))
    res = search(0, {}, frozenset(), frozenset(), frozenset())
    if not res:
        return None, best['missing']
    return {s: '%s (%d)' % (r['name'], r['itemId']) for s, r in res.items()}, None


quest_choices = json.loads((FACTS / 'quest-reward-choices.json').read_text(encoding='utf-8'))
for cls, specs in SPECS.items():
    for spec in specs:
        path = DATA / ('%s-%s.json' % (cls.lower(), spec))
        if not path.exists():
            err(path.name, 'missing file')
            continue
        cat = json.loads(path.read_text(encoding='utf-8-sig'))
        name = path.name
        for f in ROOT_FIELDS:
            if f not in cat:
                err(name, 'missing root field ' + f)
        expect = {'schemaVersion': 1, 'catalogId': 'classic-phase1-level60-%s-%s' % (cls.lower(), spec),
                  'gameVersion': 'Classic', 'characterClass': cls, 'specializationId': spec, 'levelCap': 60,
                  'releaseStage': 'Classic', 'phase': 'Phase 1 · pre-raid'}
        for k, v in expect.items():
            if cat.get(k) != v:
                err(name, '%s is %r, expected %r' % (k, cat.get(k), v))
        if cat.get('status') not in ('Reviewed', 'Partial'):
            err(name, 'bad status')
        if not https(cat.get('sourceUrl')):
            err(name, 'sourceUrl must be HTTPS')
        if not re.fullmatch(r'\d{4}-\d{2}-\d{2}', cat.get('reviewedOn', '')):
            err(name, 'reviewedOn format')
        setup = cat.get('weaponSetup', 'Flexible')
        if setup not in ('Flexible', 'TwoHanded', 'OneHandAndOffHand'):
            err(name, 'bad weaponSetup')
        if setup != 'Flexible' and not https(cat.get('weaponSetupSourceUrl')):
            err(name, 'weaponSetup needs weaponSetupSourceUrl')
        ids = set()
        rows = cat.get('items', [])
        if not rows:
            err(name, 'no items')
        for r in rows:
            rid = r.get('id')
            for f in ITEM_FIELDS:
                if f not in r:
                    err(name, '%s missing %s' % (rid, f))
            if rid in ids:
                err(name, 'duplicate id ' + str(rid))
            ids.add(rid)
            pattern = '%s-%s-%d' % (cat['catalogId'], r['slot'].lower(), r['itemId'])
            if r.get('requiredSuffix'):
                pattern += '-' + r['requiredSuffix'].lower().replace(' ', '-')
            if rid != pattern:
                err(name, 'id %s does not match %s' % (rid, pattern))
            if r['slot'] not in SLOTS:
                err(name, rid + ' bad slot')
            if r['acquisitionType'] not in ACQ or r['acquisitionType'] not in ALLOWED_ACQ:
                err(name, rid + ' acquisitionType not allowed: ' + r['acquisitionType'])
            if r['weaponKind'] not in WEAPON:
                err(name, rid + ' bad weaponKind')
            for f in ('itemUrl', 'recommendationUrl'):
                if not https(r.get(f)):
                    err(name, rid + ' non-HTTPS ' + f)
            if r.get('iconUrl') is not None and not https(r['iconUrl']):
                err(name, rid + ' non-HTTPS iconUrl')
            if r['itemUrl'] != 'https://www.wowhead.com/classic/item=%d' % r['itemId']:
                err(name, rid + ' itemUrl pattern')
            lvl = r.get('requiredLevel')
            if lvl is not None and not (0 <= lvl <= 60):
                err(name, rid + ' requiredLevel out of range')
            if r['acquisitionType'] == 'Quest':
                fac = r.get('availableFactions')
                if not fac or any(f not in FACTIONS for f in fac) or len(set(fac)) != len(fac):
                    err(name, rid + ' quest without valid availableFactions')
            elif r.get('availableFactions') is not None:
                fac = r['availableFactions']
                if any(f not in FACTIONS for f in fac) or len(set(fac)) != len(fac):
                    err(name, rid + ' invalid availableFactions')
            if r['acquisitionType'] == 'Crafting' and not r['note']:
                err(name, rid + ' crafting row needs recipe note')
            m = meta.get(str(r['itemId']))
            if not m:
                err(name, rid + ' no saved Wowhead metadata')
                continue
            expected_name = m['name'] + (' ' + r['requiredSuffix'] if r.get('requiredSuffix') else '')
            if r['name'] != expected_name:
                err(name, '%s name %r != Wowhead %r' % (rid, r['name'], expected_name))
            if r.get('requiredLevel') != m['requiredLevel']:
                err(name, '%s requiredLevel %r != Wowhead %r' % (rid, r.get('requiredLevel'), m['requiredLevel']))
            if r['uniqueEquipped'] != m['unique']:
                err(name, '%s uniqueEquipped %r != Wowhead %r' % (rid, r['uniqueEquipped'], m['unique']))
            if m['slotText'] not in SLOT_TEXT[r['slot']]:
                err(name, '%s slot %s incompatible with Wowhead %s' % (rid, r['slot'], m['slotText']))
            if r['slot'] in ('MainHand', 'OffHand'):
                kind = KIND_TEXT.get(m['slotText'])  # shields are 'Off Hand' -> OffHand, as in Forever packs
                if r['weaponKind'] != kind:
                    err(name, '%s weaponKind %s != %s' % (rid, r['weaponKind'], kind))
            elif r['weaponKind'] != 'None':
                err(name, rid + ' non-weapon slot must use weaponKind None')
            if m['randomEnchantment'] and not r.get('requiredSuffix'):
                err(name, rid + ' random-enchantment item without requiredSuffix')
            if r.get('iconUrl') and r['iconUrl'] != 'https://wow.zamimg.com/images/wow/icons/large/%s.jpg' % m['icon']:
                err(name, rid + ' icon mismatch')
            # annotations for the set search
            r['_questChoice'] = {f: quest_choices.get(str(r['itemId']), {}).get(f) for f in FACTIONS}
            if r['acquisitionType'] == 'Crafting':
                r['_profession'] = r['source'].split(' ')[0]
                r['_requiresProfession'] = 'Requires ' in r['note']
        for ex in cat.get('slotExemptions', []):
            if any(r['slot'] == ex['slot'] for r in rows):
                err(name, 'slotExemption contradicts item rows: ' + ex['slot'])
        sets = {}
        for f in FACTIONS:
            witness, missing = faction_set(cat, rows, f)
            sets[f] = {'complete': witness is not None, 'witness': witness, 'firstUnfilledSlot': missing}
        covered = sorted({r['slot'] for r in rows}, key=SLOTS.index)
        uncovered = [s for s in SLOTS if s not in covered and s not in {e['slot'] for e in cat.get('slotExemptions', [])}]
        if cat['status'] == 'Reviewed' and (uncovered or not all(v['complete'] for v in sets.values())):
            err(name, 'status Reviewed but slots or faction sets incomplete')
        summary.append({'catalogId': cat['catalogId'], 'status': cat['status'], 'rows': len(rows),
                        'distinctItems': len({(r['itemId'], r.get('requiredSuffix')) for r in rows}),
                        'weaponSetup': setup, 'uncoveredSlots': uncovered, 'factionSets': sets,
                        'acquisition': {a: sum(r['acquisitionType'] == a for r in rows) for a in sorted(ALLOWED_ACQ)}})

report = {'reviewedOn': '2026-10-08', 'errors': errors, 'catalogs': summary,
          'scope': 'Contract fields/enums/IDs, saved Wowhead metadata, quest factions and one legal set per faction. '
                   'The C# Classic reader and BISTracker.Checks audits were not run by this script.'}
Path(sys.argv[2]).write_text(json.dumps(report, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
for s in summary:
    print(s['catalogId'], s['status'], s['rows'], 'rows', 'uncovered', s['uncoveredSlots'],
          {f: v['complete'] for f, v in s['factionSets'].items()})
print(len(errors), 'errors')
for e in errors:
    print(' ', e)
sys.exit(1 if errors else 0)
