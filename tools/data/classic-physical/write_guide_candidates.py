"""Record the guide rows (before any filtering) as docs/data/classic-physical/guide-candidates.json.

Usage: python -I write_guide_candidates.py <out.json>
Rows are transcribed from the saved guide pages (rendered with html_to_text.py):
item, guide slot label and the guide's own source text. Rows without an item
link in the guide (e.g. Tarnished Elven Ring) keep itemId null.
"""
import json
import sys
from pathlib import Path

IVR = 'https://www.icy-veins.com/wow-classic/rogue-dps-pre-raid-gear'
IVH = 'https://www.icy-veins.com/wow-classic/hunter-dps-pre-raid-gear'
IVW = 'https://www.icy-veins.com/wow-classic/warrior-dps-pre-raid-gear'
IVT = 'https://www.icy-veins.com/wow-classic/warrior-tank-pre-raid-gear'
WTT = 'https://www.warcrafttavern.com/wow-classic/guides/phase-1-warrior-prot-pre-raid-bis-list/'


def rows(spec):
    out = []
    for line in spec.strip().splitlines():
        slot, iid, name, src = [x.strip() for x in line.split('|')]
        out.append({'guideSlot': slot, 'itemId': int(iid) if iid.isdigit() else None, 'name': name, 'guideSource': src})
    return out


GUIDES = [
    {'key': 'icy-rogue', 'url': IVR, 'title': 'Rogue DPS Pre-Raid Gear (Icy Veins, Sellin)',
     'lastUpdated': 'Page header 2025-03-08; changelog 2025-05-01 Phase 4 of Classic Anniversary',
     'appliesTo': ['rogue-assassination', 'rogue-combat', 'rogue-subtlety'],
     'scope': 'One Rogue DPS list with separate Phase 1-4 tables (Classic Anniversary). The Phase 1 table is primary; '
              'rows from the Phase 2-4 tables are kept only when the item itself is obtainable in phase 1.',
     'items': rows('''
Helm (P1-P3) | 13404 | Mask of the Unforgiven | The Unforgiven in Stratholme
Helm (P4) | 22005 | Darkmantle Cap | quest 9010 Saving the Best for Last
Neck | 15411 | Mark of Fordring | Reward from quest 5944 In Dreams
Shoulders (P1-P3) | 12927 | Truestrike Shoulders | Pyroguard Emberseer in Upper Blackrock Spire
Shoulders (P4) | 22008 | Darkmantle Spaulders | Anthion's Parting Words
Back | 13340 | Cape of the Black Baron | Baron Rivendare in Stratholme
Chest (P1-P3) | 14637 | Cadaverous Armor | Multiple bosses in Scholomance
Chest (P4) | 22009 | Darkmantle Tunic | quest 9010 Saving the Best for Last
Wrists (P1) | 13120 | Deepfury Bracers | World BoE drop
Wrists (P2-P3) | 16710 | Shadowcraft Bracers | Trash in Scholomance
Wrists (P4) | 22004 | Darkmantle Bracers | quest 8907 An Earnest Proposition
Hands (P1, P4) | 15063 | Devilsaur Gauntlets | Leatherworking
Hands (P2-P3) | 16712 | Shadowcraft Gloves | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Waist (P1) | 13252 | Cloudrunner Girdle | Quartermaster Zigris in Lower Blackrock Spire
Waist (P2-P3) | 16713 | Shadowcraft Belt | Trash in Lower Blackrock Spire
Waist (P4) | 22002 | Darkmantle Belt | Just Compensation
Legs (P1, P4) | 15062 | Devilsaur Leggings | Leatherworking
Legs (P2-P3) | 16709 | Shadowcraft Pants | Baron Rivendare in Stratholme
Feet (P1) | 12553 | Swiftwalker Boots | Princess Moira Bronzebeard in Blackrock Depths
Feet (P2-P3) | 16711 | Shadowcraft Boots | Rattlegore in Scholomance
Feet (P4) | 22003 | Darkmantle Boots | Anthion's Parting Words
Rings (P1) | 13098 | Painweaver Band | General Drakkisath in Upper Blackrock Spire
Rings (P1) | 17713 | Blackstone Ring | Princess Theradras in Maraudon
Rings (P2-P4) | - | Tarnished Elven Ring | Mizzle the Crafty in Dire Maul North
Trinkets | 11815 | Hand of Justice | General Angerforge in Blackrock Depths
Trinkets | 13965 | Blackhand's Breadth | Reward from quest 5102 General Drakkisath's Demise / 4974 For The Horde!
Swords | 12940 | Dal'Rend's Sacred Charge | Warchief Rend Blackhand in Upper Blackrock Spire
Swords | 12939 | Dal'Rend's Tribal Guardian | Warchief Rend Blackhand in Upper Blackrock Spire
Daggers | 12783 | Heartseeker | Blacksmithing
Daggers | 14555 | Alcor's Sunrazor | World BoE drop
Ranged Weapon | 12651 | Blackcrow | Shadow Hunter Vosh'gajin in Lower Blackrock Spire''')},
    {'key': 'icy-hunter', 'url': IVH, 'title': 'Hunter DPS Pre-Raid Gear (Icy Veins, Impakt)',
     'lastUpdated': '2024-11-17 (Classic re-release)',
     'appliesTo': ['hunter-beast-mastery', 'hunter-marksmanship', 'hunter-survival'],
     'scope': "One Hunter DPS list for gear before Molten Core or Onyxia's Lair; it mixes phase 1 dungeons with "
              'Dire Maul and later content and is filtered here.',
     'items': rows('''
Helm | 18421 | Backwood Helm | Quest 7877 The Treasure of the Shen'dralar in Dire Maul
Helm | 13404 | Mask of the Unforgiven | The Unforgiven in Stratholme
Neck | 15411 | Mark of Fordring | Quest 5944 In Dreams
Neck | 22340 | Pendant of Celerity | Lord Valthalak in Upper Blackrock Spire
Neck | 11933 | Imperial Jewel | Emperor Dagran Thaurissan in Blackrock Depths
Shoulders | 12927 | Truestrike Shoulders | Pyroguard Emberseer in Upper Blackrock Spire
Shoulders | 15051 | Black Dragonscale Shoulders | Leatherworking
Shoulders | - | Any high Agility shoulders | Bind on equip items from the AH
Back | 13340 | Cape of the Black Baron | Baron Rivendare in Stratholme
Back | 13397 | Stoneskin Gargoyle Cape | Stonespine in Stratholme
Chest | 11726 | Savage Gladiator Chain | Gorosh the Dervish in Blackrock Depths
Chest | 13944 | Tombstone Breastplate | Darkmaster Gandling in Scholomance
Chest | 11926 | Deathdealer Breastplate | The Chest of Seven in Blackrock Depths
Chest | 14637 | Cadaverous Armor | Mini-bosses in Scholomance
Chest | 15050 | Black Dragonscale Breastplate | Leatherworking (for the set bonuses)
Chest | 16674 | Beaststalker's Tunic | General Drakkisath in Upper Blackrock Spire (4-piece set condition)
Wrists | 18375 | Bracers of the Eclipse | Prince Tortheldrin in Dire Maul West
Wrists | 13211 | Slashclaw Bracers | Halycon in Lower Blackrock Spire
Wrists | 16681 | Beaststalker's Bindings | Zone drop from Stratholme (4-piece set condition)
Hands | 15063 | Devilsaur Gauntlets | Leatherworking
Hands | 13255 | Trueaim Gauntlets | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Waist | 18393 | Warpwood Binding | Tendris Warpwood in Dire Maul West
Waist | 16680 | Beaststalker's Belt | Zone drop from Blackrock Spire (4-piece set condition)
Waist | 14636 | Cadaverous Belt | Bosses in Scholomance
Legs | 15062 | Devilsaur Leggings | Leatherworking
Legs | 15052 | Black Dragonscale Leggings | Leatherworking (for the set bonuses)
Legs | 16678 | Beaststalker's Pants | Baron Rivendare in Stratholme (4-piece set condition)
Feet | 13967 | Windreaver Greaves | Quest 5384 Kirtonos the Herald in Scholomance
Feet | 16675 | Beaststalker's Boots | Nerub'enkan in Stratholme (4-piece set condition)
Feet | 12553 | Swiftwalker Boots | High Priestess of Thaurissan and Princess Moira Bronzebeard in Blackrock Depths
Rings | - | Tarnished Elven Ring | King Gordok in Dire Maul North
Rings | 17713 | Blackstone Ring | Princess Theradras in Maraudon
Rings | 13098 | Painweaver Band | General Drakkisath in Upper Blackrock Spire
Trinkets | 18473 | Royal Seal of Eldre'Thalas | Quest 7503 The Greatest Race of Hunters in Dire Maul
Trinkets | 13965 | Blackhand's Breadth | Quest 5102 (A) / 4974 (H) in Upper Blackrock Spire
Trinkets | 19120 | Rune of the Guard Captain | (H) Quest 7862 Job Opening: Guard Captain of Revantusk Village
Trinkets | 11815 | Hand of Justice | (A) General Angerforge in Blackrock Depths
Melee Weapons | 18520 | Barbarous Blade | King Gordok in Dire Maul North
Melee Weapons | 18725 | Peacemaker | Magistrate Barthilas in Stratholme
Melee Weapons | 13052 | Warmonger | World drop
Ranged Weapon | 18738 | Carapace Spine Crossbow | Nerub'enkan in Stratholme
Ranged Weapon | 12651 | Blackcrow | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Ranged Weapon | 18680 | Ancient Bone Bow | Multiple bosses in Scholomance
Ranged Weapon | 2099 | Dwarven Hand Cannon | World drop
Quiver | 2662 | Ribbly's Quiver | Ribbly Screwspigot in Blackrock Depths
Arrows | 12654 | Doomshot | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Arrows | 15997 | Thorium Shells | Engineering
Arrows | 18042 | Thorium Headed Arrow | Engineering''')},
    {'key': 'icy-warrior-dps', 'url': IVW, 'title': 'Warrior DPS Pre-Raid Gear, MC/Ony table (Icy Veins, Seksixeny)',
     'lastUpdated': '2024-11-18 (Classic Anniversary)',
     'appliesTo': ['warrior-arms', 'warrior-fury'],
     'scope': 'The "MC/Ony DPS Warrior Pre-Raid BiS" table (gear before Molten Core/Onyxia). The separate Naxxramas '
              'pre-raid table is not used.',
     'items': rows('''
Helm | 12640 | Lionheart Helm | Blacksmithing
Helm | 13404 | Mask of the Unforgiven | The Unforgiven in Stratholme
Helm | 12587 | Eye of Rend | Warchief Rend Blackhand in Upper Blackrock Spire
Neck | 15411 | Mark of Fordring | Quest 5944 In Dreams
Neck | 17044 | Will of the Martyr | Quest 5125 Aurius' Reckoning in Stratholme
Neck | 11933 | Imperial Jewel | Emperor Dagran Thaurissan in Blackrock Depths
Shoulder | 12927 | Truestrike Shoulders | Pyroguard Emberseer in Upper Blackrock Spire
Shoulder | 12082 | Wyrmhide Spaulders | Quest 4024 A Taste of Flame in Blackrock Depths
Shoulder | 16733 | Spaulders of Valor | Warchief Rend Blackhand in Upper Blackrock Spire (defensive / PvP)
Cloak | 13340 | Cape of the Black Baron | Baron Rivendare in Stratholme
Cloak | 11626 | Blackveil Cape | High Interrogator Gerstahn in Blackrock Depths
Cloak | 13397 | Stoneskin Gargoyle Cape | Stonespine in Stratholme
Chest | 11726 | Savage Gladiator Chain | Gorosh the Dervish in Blackrock Depths
Chest | 14637 | Cadaverous Armor | Side bosses in Scholomance
Chest | 13944 | Tombstone Breastplate | Darkmaster Gandling in Scholomance
Wrists | 12936 | Battleborn Armbraces | Warchief Rend Blackhand in Upper Blackrock Spire
Wrists | 13400 | Vambraces of the Sadist | Timmy the Cruel in Stratholme
Wrists | 13211 | Slashclaw Bracers | Halycon in Lower Blackrock Spire
Wrists | 12966 | Blackmist Armguards | The Beast in Upper Blackrock Spire
Gloves | 14551 | Edgemaster's Handguards | World drop BoE
Gloves | 15063 | Devilsaur Gauntlets | Tribal Leatherworking
Gloves | 13957 | Gargoyle Slashers | Quest 5384 Kirtonos the Herald in Scholomance
Belt | 13142 | Brigam Girdle | General Drakkisath in Upper Blackrock Spire
Belt | 13959 | Omokk's Girth Restrainer | Quest 5081 Maxwell's Mission / 4903 Warlord's Command in Lower Blackrock Spire
Belt | 13502 | Handcrafted Mastersmith Girdle | Goraluk Anvilcrack in Upper Blackrock Spire
Legs | 14554 | Cloudkeeper Legplates | World drop BoE
Legs | 15062 | Devilsaur Leggings | Tribal Leatherworking
Legs | 18380 | Eldritch Reinforced Legplates | Prince Tortheldrin in Dire Maul West
Legs | 16732 | Legplates of Valor | Baron Rivendare in Stratholme
Boots | 14616 | Bloodmail Boots | Side bosses in Scholomance
Boots | 12555 | Battlechaser's Greaves | Blackrock Spire BoE drop
Boots | 13967 | Windreaver Greaves | Quest 5384 Kirtonos the Herald in Scholomance
Rings | 13098 | Painweaver Band | General Drakkisath in Upper Blackrock Spire
Rings | - | Tarnished Elven Ring | Gordok Tribute in Dire Maul North
Rings | 17713 | Blackstone Ring | Princess Theradras in Maraudon
Rings | 2246 | Myrmidon's Signet | World drop BoE
Rings | 12548 | Magni's Will | Quest 4363 The Princess's Surprise in Blackrock Depths (Alliance only)
Rings | 13217 | Band of the Penitent | Quest 5243 Houses of the Holy in Stratholme
Trinkets | 11815 | Hand of Justice | Emperor Dagran Thaurissan in Blackrock Depths
Trinkets | 13965 | Blackhand's Breadth | Quest 5102 / 4974 in Upper Blackrock Spire
Trinkets | 19120 | Rune of the Guard Captain | Quest 7862 in The Hinterlands
Main-Hand | 11684 | Ironfoe | Emperor Dagran Thaurissan in Blackrock Depths
Main-Hand | 12940 | Dal'Rend's Sacred Charge | Warchief Rend Blackhand in Upper Blackrock Spire
Main-Hand | 811 | Axe of the Deep Woods | World drop BoE (Orc)
Main-Hand | 18348 | Quel'Serrar | Quest 7509 The Forging of Quel'Serrar (book looted in Dire Maul)
Off-Hand | 12590 | Felstriker | Warchief Rend Blackhand in Upper Blackrock Spire
Off-Hand | 12939 | Dal'Rend's Tribal Guardian | Warchief Rend Blackhand in Upper Blackrock Spire
Off-Hand | 871 | Flurry Axe | World drop BoE (Orc)
Off-Hand | 15806 | Mirah's Song | Quest 5384 Kirtonos the Herald in Scholomance
Off-Hand | 13015 | Serathil | World drop BoE
Two-Hand | 18538 | Treant's Bane | Gordok Tribute in Dire Maul North
Two-Hand | 12592 | Blackblade of Shahram | General Drakkisath in Upper Blackrock Spire
Two-Hand | 12784 | Arcanite Reaper | Blacksmithing
Two-Hand | 11931 | Dreadforge Retaliator | Emperor Dagran Thaurissan in Blackrock Depths
Two-Hand | 13348 | Demonshear | Balnazzar in Stratholme
Ranged | 18323 | Satyr's Bow | Zevrim Thornhoof in Dire Maul East
Ranged | 12653 | Riphook | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Ranged | 12651 | Blackcrow | Shadow Hunter Vosh'gajin in Lower Blackrock Spire''')},
    {'key': 'icy-warrior-tank', 'url': IVT, 'title': 'Warrior Tank Pre-Raid Gear, MC/Ony table (Icy Veins, Seksixeny)',
     'lastUpdated': '2024-11-18 (Classic Anniversary)',
     'appliesTo': ['warrior-protection'],
     'scope': 'The "MC/Ony Tank Warrior Pre-Raid BiS" table. The Naxxramas pre-raid table is not used.',
     'items': rows('''
Helm | 12952 | Gyth's Skull | Gyth in Upper Blackrock Spire
Helm | 11746 | Golem Skull Helm | Phalanx in Blackrock Depths
Neck | 13091 | Medallion of Grand Marshal Morris | World drop BoE
Neck | 11755 | Verek's Collar | Verek in Blackrock Depths
Shoulder | 14552 | Stockade Pauldrons | World drop BoE
Shoulder | 16733 | Spaulders of Valor | Warchief Rend Blackhand in Upper Blackrock Spire
Cloak | 13397 | Stoneskin Gargoyle Cape | Stonespine in Stratholme
Cloak | 11930 | The Emperor's New Cape | Emperor Dagran Thaurissan in Blackrock Depths
Cloak | 11626 | Blackveil Cape | High Interrogator Gerstahn in Blackrock Depths
Chest | 14624 | Deathbone Chestplate | Side bosses in Scholomance
Chest | 15413 | Ornate Adamantium Breastplate | Quest 5944 In Dreams
Chest | 11678 | Carapace of Anub'shiah | Anub'shiah in Blackrock Depths
Wrists | 16735 | Bracers of Valor | Blackrock Spire drop BoE
Wrists | 12550 | Runed Golem Shackles | World drop BoE
Gloves | 13072 | Stonegrip Gauntlets | World drop BoE
Gloves | 14525 | Boneclenched Gauntlets | Ras Frostwhisper in Scholomance
Gloves | 13963 | Voone's Vice Grips | Quest 5081 Maxwell's Mission / 4903 Warlord's Command (threat)
Belt | 13142 | Brigam Girdle | General Drakkisath in Upper Blackrock Spire
Belt | 14620 | Deathbone Girdle | Side bosses in Scholomance
Legs | 14554 | Cloudkeeper Legplates | World drop BoE
Legs | 12935 | Warmaster Legguards | Warchief Rend Blackhand in Upper Blackrock Spire
Boots | 13381 | Master Cannoneer Boots | Cannon Master Willey in Stratholme
Boots | 13259 | Ribsteel Footguards | Urok Doomhowl in Lower Blackrock Spire
Ring #1 | 2246 | Myrmidon's Signet | World drop BoE
Ring #1 | 13373 | Band of Flesh | Ramstein the Gorger in Stratholme
Ring #2 | 11669 | Naglering | Golem Lord Argelmach in Blackrock Depths
Ring #2 | 15855 | Ring of Protection | Quest 5942 Hidden Treasures in Eastern Plaguelands
Trinket 1 | 17774 | Mark of the Chosen | Quest 7067 The Pariah's Instructions in Maraudon
Trinket 2 | 11815 | Hand of Justice | Emperor Dagran Thaurissan in Blackrock Depths (threat)
Trinket 2 | 11810 | Force of Will | General Angerforge in Blackrock Depths (AoE tanking)
One-Hand | 15806 | Mirah's Song | Quest 5384 Kirtonos the Herald in Scholomance
One-Hand | 811 | Axe of the Deep Woods | World drop BoE
Shield | 12602 | Draconian Deflector | General Drakkisath in Upper Blackrock Spire
Shield | 1168 | Skullflame Shield | World drop BoE
Ranged | 12651 | Blackcrow | Shadow Hunter Vosh'gajin in Lower Blackrock Spire
Ranged | 16996 | Gorewood Bow | Quest 6187 Order Must Be Restored / 6148 The Scarlet Oracle, Demetria''')},
    {'key': 'wt-prot-p1', 'url': WTT, 'title': 'Phase 1 Warrior Prot Pre Raid BiS List (Warcraft Tavern, thermophile)',
     'lastUpdated': 'not shown on the saved page',
     'appliesTo': ['warrior-protection'],
     'scope': 'Explicit Classic phase 1 warrior tank pre-raid list in text form.',
     'items': rows('''
Helm | 12952 | Gyth's Skull | Gyth in UBRS (random elemental resistances)
Helm | 11746 | Golem Skull Helm | Magmus in BRD
Shoulders | 14552 | Stockade Pauldrons | Random world drop
Shoulders | 16733 | Spaulders of Valor | Rend in UBRS
Shoulders | 13405 | Wailing Nightbane Pauldrons | The Unforgiven in Stratholme
Chestplate | 14624 | Deathbone Chestplate | Scholomance (defense-cap fights)
Chestplate | 15413 | Ornate Adamantium Breastplate | Quest In Dreams
Chestplate | 13394 | Skul's Cold Embrace | Skul in Stratholme
Bracers | 13951 | Vigorsteel Vambraces | Darkmaster Gandling in Scholomance
Bracers | 16735 | Bracers of Valor | Random drop from mobs in Blackrock Spire
Bracers | 12966 | Blackmist Armguards | The Beast in UBRS (TPS alternative)
Gloves | 13072 | Stonegrip Gauntlets | Random world drop
Gloves | 14622 | Deathbone Gauntlets | Scholomance
Gloves | 9410 | Cragfists | Ancient Stone Keeper in Uldaman (random stats: bear, monkey or stam)
Gloves | 13963 | Voone's Vice Grips | Quest Warlord's Command / Maxwell's Mission (TPS alternative)
Belt | 14620 | Deathbone Girdle | Scholomance
Belt | 12115 | Stalwart Clutch | Quest 3907 Disharmony of Fire (H) / 4263 Incendius! (A)
Belt | 13142 | Brigam Girdle | General Drakkisath in UBRS (TPS alternative)
Legplates | 14623 | Deathbone Legguards | Scholomance
Legplates | 11927 | Legplates of the Eternal Guardian | Chest in BRD
Legplates | 19124 | Slagplate Leggings | Quest 7728 STOLEN: Smithing Tuyere and Lookout's Spyglass
Boots | 14621 | Deathbone Sabatons | Scholomance
Boots | 14549 | Boots of Avoidance | Random world drop
Rings | 11669 | Naglering | Golem Lord Argelmach in BRD
Rings | 10795 | Drakeclaw Band | Sunken Temple dragon mini-bosses (random stats: bear, monkey or stam)
Rings | 2246 | Myrmidon's Signet | Random world drop
Rings | 13373 | Band of Flesh | Ramstein the Gorger in Stratholme
Rings | 18674 | Hardened Stone Band | Random world drop
Rings | 17713 | Blackstone Ring | Princess Theradras in Maraudon (DPS-focused)
Rings | 13098 | Painweaver Band | General Drakkisath in UBRS (DPS-focused)
Necklace | 13091 | Medallion of Grand Marshal Morris | Random world drop
Trinket | 11810 | Force of Will | General Angerforge in BRD
Trinket | 10779 | Demon's Blood | Quest chain You Are Rakh'likh, Demon
Trinket | 17774 | Mark of the Chosen | Quest 7067 The Pariah's Instructions
Trinket | 13515 | Ramstein's Lightning Bolts | Ramstein the Gorger in Stratholme (AoE tanking)
Trinket | 11815 | Hand of Justice | Emperor Dagran Thaurissan in BRD
Trinket | 13965 | Blackhand's Breadth | Quest 4974 For The Horde! (H) / 5102 General Drakkisath's Demise (A)
Cloak | 12967 | Bloodmoon Cloak | The Beast in UBRS
Cloak | 13397 | Stoneskin Gargoyle Cape | Stonespine in Stratholme
Cloak | 11930 | The Emperor's New Cape | Emperor Dagran Thaurissan in BRD
Shield | 12602 | Draconian Deflector | General Drakkisath in UBRS
Shield | 18696 | Intricately Runed Shield | Ras Frostwhisper in Scholomance
Shield | 13529 | Husk of Nerub'enkan | Nerub'enkan in Stratholme
Shield | 1168 | Skullflame Shield | Random world drop
Weapon | 11684 | Ironfoe | Emperor Dagran Thaurissan in BRD
Weapon | 12798 | Annihilator | Master Axesmith only; recommended only if the armor debuff stacks with Sunder Armor
Ranged | 16996 | Gorewood Bow | Quest chain in Eastern Plaguelands
Ranged | 18738 | Carapace Spine Crossbow | Nerub'enkan in Stratholme
Ranged | 18680 | Ancient Bone Bow | Scholomance
Ranged | 12651 | Blackcrow | Shadow Hunter Vosh'gajin in LBRS''')},
]

doc = {'reviewedOn': '2026-10-08',
       'note': 'Guide rows as published (item, guide slot label, guide source text) before phase 1 and source filtering.',
       'guides': GUIDES}
Path(sys.argv[1]).write_text(json.dumps(doc, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
print(sum(len(g['items']) for g in GUIDES), 'rows;',
      len({r['itemId'] for g in GUIDES for r in g['items'] if r['itemId']}), 'distinct item IDs')
