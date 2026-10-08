"""Write docs/data/classic-physical/selection.json: the kept guide rows per catalog.

Usage: python -I write_selection.py <out.json>

Each kept row is a guide recommendation that passed the phase 1 / source
filter documented in filter-decisions.json. Rings and trinkets get both
placements; weapons keep the hand(s) the guide lists them for, except a
guide 'pair' column without hand labels (Rogue daggers), where a one-hand
item gets both hands.
"""
import json
import sys
from pathlib import Path

IVR = 'https://www.icy-veins.com/wow-classic/rogue-dps-pre-raid-gear'
IVH = 'https://www.icy-veins.com/wow-classic/hunter-dps-pre-raid-gear'
IVW = 'https://www.icy-veins.com/wow-classic/warrior-dps-pre-raid-gear'
IVT = 'https://www.icy-veins.com/wow-classic/warrior-tank-pre-raid-gear'
WTT = 'https://www.warcrafttavern.com/wow-classic/guides/phase-1-warrior-prot-pre-raid-bis-list/'
IVWT = 'https://www.icy-veins.com/wow-classic/warrior-dps-pve-spec-builds-talents'


def both(slot_a, slot_b, *ids, **kw):
    return [dict({'slot': s, 'itemId': i}, **kw) for i in ids for s in (slot_a, slot_b)]


def one(slot, *ids, **kw):
    return [dict({'slot': slot, 'itemId': i}, **kw) for i in ids]


SET_NOTE_BEAST = 'Beaststalker set piece; the guide recommends it when you use four set pieces.'
SET_NOTE_BDS = 'Black Dragonscale set piece; recommended for the set bonus.'
SHADOWCRAFT = 'Shadowcraft set piece.'

rogue_rows = (one('Head', 13404) + one('Neck', 15411) + one('Shoulder', 12927) + one('Back', 13340)
              + one('Chest', 14637) + one('Wrist', 16710, note=SHADOWCRAFT)
              + one('Hands', 15063) + one('Hands', 16712, note=SHADOWCRAFT)
              + one('Waist', 13252) + one('Waist', 16713, note=SHADOWCRAFT)
              + one('Legs', 15062) + one('Legs', 16709, note=SHADOWCRAFT)
              + one('Feet', 12553) + one('Feet', 16711, note=SHADOWCRAFT)
              + both('Finger1', 'Finger2', 13098, 17713) + both('Trinket1', 'Trinket2', 11815, 13965)
              + one('MainHand', 12940) + one('OffHand', 12939) + both('MainHand', 'OffHand', 12783)
              + one('Ranged', 12651))

hunter_rows = (one('Head', 13404) + one('Neck', 15411, 11933) + one('Shoulder', 12927, 15051)
               + one('Back', 13340, 13397)
               + one('Chest', 11726, 13944, 11926, 14637) + one('Chest', 15050, note=SET_NOTE_BDS)
               + one('Chest', 16674, note=SET_NOTE_BEAST)
               + one('Wrist', 13211) + one('Wrist', 16681, note=SET_NOTE_BEAST)
               + one('Hands', 15063, 13255)
               + one('Waist', 16680, note=SET_NOTE_BEAST) + one('Waist', 14636)
               + one('Legs', 15062) + one('Legs', 15052, note=SET_NOTE_BDS) + one('Legs', 16678, note=SET_NOTE_BEAST)
               + one('Feet', 13967) + one('Feet', 16675, note=SET_NOTE_BEAST) + one('Feet', 12553)
               + both('Finger1', 'Finger2', 17713, 13098) + both('Trinket1', 'Trinket2', 13965, 19120, 11815)
               + one('MainHand', 18725) + one('Ranged', 18738, 12651, 18680))

warrior_dps_rows = (one('Head', 12640, 13404, 12587) + one('Neck', 15411, 17044, 11933)
                    + one('Shoulder', 12927, 12082) + one('Shoulder', 16733, note='Defensive option.')
                    + one('Back', 13340, 11626, 13397) + one('Chest', 11726, 14637, 13944)
                    + one('Wrist', 12936, 13400, 13211, 12966) + one('Hands', 15063, 13957)
                    + one('Waist', 13142, 13959, 13502) + one('Legs', 15062, 16732)
                    + one('Feet', 14616, 12555, 13967)
                    + both('Finger1', 'Finger2', 13098, 17713, 12548, 13217)
                    + both('Trinket1', 'Trinket2', 11815, 13965, 19120)
                    + one('MainHand', 11684, 12940) + one('OffHand', 12590, 12939, 15806)
                    + one('MainHand', 12592, 12784, 11931, 13348) + one('Ranged', 12653, 12651))

tank_ivt = (one('Head', 11746) + one('Neck', 11755) + one('Shoulder', 16733) + one('Back', 13397, 11930, 11626)
            + one('Chest', 14624, 15413, 11678) + one('Wrist', 16735) + one('Hands', 14525)
            + one('Hands', 13963, note='Threat option.') + one('Waist', 13142, 14620) + one('Legs', 12935)
            + one('Feet', 13381, 13259) + both('Finger1', 'Finger2', 13373, 11669, 15855)
            + both('Trinket1', 'Trinket2', 17774) + both('Trinket1', 'Trinket2', 11815, note='Threat option.')
            + both('Trinket1', 'Trinket2', 11810, note='AoE tanking option.')
            + one('MainHand', 15806) + one('OffHand', 12602) + one('Ranged', 12651, 16996))
tank_wt = [dict(r, recommendationUrl=WTT) for r in (
    one('Shoulder', 13405) + one('Chest', 13394) + one('Wrist', 13951)
    + one('Wrist', 12966, note='Threat option.') + one('Hands', 14622) + one('Waist', 12115)
    + one('Legs', 14623, 11927, 19124) + one('Feet', 14621) + one('Back', 12967)
    + both('Finger1', 'Finger2', 17713, 13098, note='Damage-focused option.')
    + both('Trinket1', 'Trinket2', 10779, 13965) + both('Trinket1', 'Trinket2', 13515, note='AoE tanking option.')
    + one('MainHand', 11684) + one('OffHand', 18696, 13529) + one('Ranged', 18738, 18680))]

ROGUE_METHOD = ('Named items from the Icy Veins Rogue DPS pre-raid list (one list for all Rogue specs). The Phase 1 table '
                'is primary; Phase 2-4 table items are kept only when obtainable in phase 1. Dungeon, quest and '
                'phase 1 crafting only. Unranked alternatives; no equivalent-value claim.')
HUNTER_METHOD = ('Named items from the Icy Veins Hunter DPS pre-raid list (one list for all Hunter specs), filtered to '
                 'phase 1 dungeons, quests and crafting. Dire Maul, Dungeon Set 2, world drops and reputation removed. '
                 'Unranked alternatives; no equivalent-value claim.')
WARRIOR_DPS_METHOD = ('Named items from the Icy Veins MC/Ony DPS Warrior pre-raid list (one DPS list for Arms and Fury), '
                      'filtered to phase 1 dungeons, quests and crafting. One-hand and two-hand options are both kept '
                      'because the guide does not tie a weapon setup to Arms or Fury. Unranked alternatives.')
TANK_METHOD = ('Named items from the Icy Veins MC/Ony Tank Warrior pre-raid list and the Warcraft Tavern phase 1 Warrior '
               'Prot pre-raid list, filtered to phase 1 dungeons, quests and crafting. Random-enchantment items without '
               'a named suffix are left out. Unranked alternatives; no equivalent-value claim.')

catalogs = []
for spec in ('assassination', 'combat', 'subtlety'):
    catalogs.append({'characterClass': 'Rogue', 'specializationId': spec, 'sourceUrl': IVR, 'status': 'Reviewed',
                     'selectionMethod': ROGUE_METHOD, 'weaponSetup': 'OneHandAndOffHand',
                     'weaponSetupSourceUrl': IVR, 'rows': rogue_rows})
for spec in ('beast-mastery', 'marksmanship', 'survival'):
    catalogs.append({'characterClass': 'Hunter', 'specializationId': spec, 'sourceUrl': IVH, 'status': 'Reviewed',
                     'selectionMethod': HUNTER_METHOD, 'weaponSetup': 'Flexible', 'rows': hunter_rows})
for spec in ('arms', 'fury'):
    catalogs.append({'characterClass': 'Warrior', 'specializationId': spec, 'sourceUrl': IVW, 'status': 'Reviewed',
                     'selectionMethod': WARRIOR_DPS_METHOD, 'weaponSetup': 'Flexible', 'rows': warrior_dps_rows})
catalogs.append({'characterClass': 'Warrior', 'specializationId': 'protection', 'sourceUrl': IVT, 'status': 'Reviewed',
                 'selectionMethod': TANK_METHOD, 'weaponSetup': 'OneHandAndOffHand', 'weaponSetupSourceUrl': IVT,
                 'rows': tank_ivt + tank_wt})

for c in catalogs:
    seen = set()
    for r in c['rows']:
        key = (r['slot'], r['itemId'])
        assert key not in seen, (c['specializationId'], key)
        seen.add(key)

doc = {'reviewedOn': '2026-10-08', 'weaponSetupNotes': {
    'Rogue': 'Rogues cannot use two-handed weapons; the guide lists sword and dagger pairs (OneHandAndOffHand).',
    'Warrior DPS': 'Flexible: the guide lists Main-Hand/Off-Hand and Two-Hand options; its talent page (%s) '
                   'describes both dual-wield and two-hander DPS builds without assigning them to Arms or Fury.' % IVWT,
    'Warrior Protection': 'OneHandAndOffHand: both tank guides list a one-hand weapon plus a shield.',
    'Hunter': 'Flexible: the only kept melee weapon is the two-handed Peacemaker.'},
    'catalogs': catalogs}
Path(sys.argv[1]).write_text(json.dumps(doc, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
print(len(catalogs), 'catalogs;', sum(len(c['rows']) for c in catalogs), 'rows')
