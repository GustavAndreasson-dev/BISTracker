"""Write the reviewed caster selection (docs/data/classic-casters/selection.json).

Usage: python -I make-selection.py <selection.json>

This file is the human review record in code form: acquisition facts per
item (verified against Wowhead Classic item pages and ClassicDB, see
wowhead-item-sources.json and item-metadata.json) and, per spec, the
alternatives named by the spec's guides after phase 1/source filtering.
Order inside a spec follows slot order and guide order; it is not a ranking.
"""
import json
import sys

REVIEWED_ON = "2026-10-08"

IVM = "https://www.icy-veins.com/wow-classic/mage-dps-pre-raid-gear"
ZKM = "https://www.zockify.com/classic/mage/dps/"
IVW = "https://www.icy-veins.com/wow-classic/warlock-dps-pre-raid-gear"
WTW = "https://www.warcrafttavern.com/wow-classic/guides/pve-warlock-pre-raid-best-in-slot/"
ZKW = "https://www.zockify.com/classic/warlock/dps/"
IVS = "https://www.icy-veins.com/wow-classic/priest-dps-pre-raid-gear"
ZKS = "https://www.zockify.com/classic/priest/shadow/"
IVH = "https://www.icy-veins.com/wow-classic/priest-healer-pre-raid-gear"
DEF = "https://www.warcrafttavern.com/wow-classic/guides/pre-raid-bis-for-holy-disc-priest-in-phase-1-pre-dm-patch-of-classic/"
SEE = "https://www.warcrafttavern.com/wow-classic/guides/holy-priest/"

UBRS = "UBRS uses a group of up to 10 players and requires the Seal of Ascension for access."
BOTH = ["Alliance", "Horde"]


def d(source, note=""):
    return {"acquisitionType": "Dungeon", "source": source, "note": note}


def q(source, factions, note=""):
    return {"acquisitionType": "Quest", "source": source, "note": note, "availableFactions": factions}


def c(source, note):
    return {"acquisitionType": "Crafting", "source": source, "note": note}


ITEMS = {
    # Head
    "18727": d("Magistrate Barthilas, Stratholme (Undead)"),
    "10504": c("Engineering (245)", "Recipe taught by an Engineering trainer. Requires Engineering (245) to equip. The crafted suffix is random."),
    "14111": c("Tailoring (290)", "Pattern: Felcloth Hood is a world drop."),
    # Neck
    "12103": d("Balnazzar, Stratholme (Live)"),
    "18691": d("Vectus, Scholomance"),
    "18723": d("Ramstein the Gorger, Stratholme (Undead)"),
    "13141": d("General Drakkisath, Upper Blackrock Spire", UBRS),
    "13960": d("Kirtonos the Herald, Scholomance"),
    # Shoulder
    "11782": d("Warder Stilgiss, Blackrock Depths"),
    "18681": d("Shared boss loot, Scholomance", "Several Scholomance bosses share this drop; Lord Alexei Barov is one verified source."),
    "14112": c("Tailoring (300)", "Pattern: Felcloth Shoulders is a world drop."),
    "11624": d("High Interrogator Gerstahn, Blackrock Depths"),
    "13185": d("Crystal Fang, Lower Blackrock Spire", "Rare spawn."),
    "16695": d("Solakar Flamewreath, Upper Blackrock Spire", UBRS),
    # Back
    "13386": d("Archivist Galford, Stratholme (Live)"),
    "11623": d("Houndmaster Grebmar, Blackrock Depths"),
    "12110": q("The Rise of the Machines, Badlands", ["Horde"], "Horde quest chain; reward quest minimum level 52."),
    # Chest
    "14152": c("Tailoring (300)", "Requires Tailoring. Bind on pickup; Mage only. Pattern: Robe of the Archmage drops from Firebrand Pyromancers in Blackrock Spire."),
    "14340": d("Ras Frostwhisper, Scholomance"),
    "14136": c("Tailoring (285)", "Pattern: Robe of Winter Night (bind on pickup) drops from Cobalt Mageweavers in Winterspring."),
    "14153": c("Tailoring (300)", "Requires Tailoring. Bind on pickup; Warlock only. Pattern: Robe of the Void drops from Darkmaster Gandling, Scholomance."),
    "14106": c("Tailoring (300)", "Pattern: Felcloth Robe is a world drop."),
    "14154": c("Tailoring (300)", "Requires Tailoring. Bind on pickup; Priest only. Pattern: Truefaith Vestments drops from Balnazzar, Stratholme (Live)."),
    "13346": d("Baron Rivendare, Stratholme (Undead)"),
    # Wrist
    "11766": d("Lord Incendius, Blackrock Depths"),
    "13409": d("The Unforgiven, Stratholme (Live)"),
    "16697": d("Scarlet and Thuzadin casters, Stratholme", "Bind on equip trash drop inside Stratholme."),
    "16683": d("Spellcasters in Blackrock Spire", "Bind on equip trash drop inside Lower and Upper Blackrock Spire."),
    # Hands
    "13253": d("Quartermaster Zigris, Lower Blackrock Spire"),
    "21318": q("Winterfall Activity, Winterspring", BOTH, "Quest minimum level 45."),
    "13870": c("Tailoring (265)", "Pattern: Frostweave Gloves is a world drop."),
    "10787": d("Troll mini-bosses, Sunken Temple"),
    "12554": d("Princess Moira Bronzebeard, Blackrock Depths"),
    # Waist
    "11662": d("Ok'thor the Breaker, Blackrock Depths"),
    "13956": d("Kirtonos the Herald, Scholomance"),
    "18740": d("Nerub'enkan, Stratholme (Undead)"),
    "12589": d("Solakar Flamewreath, Upper Blackrock Spire", UBRS),
    "14143": c("Tailoring (265)", "Pattern: Ghostweave Belt drops from Lingering Highborne in Azshara."),
    "16696": d("Spellcasters in Blackrock Spire", "Bind on equip trash drop inside Lower and Upper Blackrock Spire."),
    # Legs
    "13170": d("Highlord Omokk, Lower Blackrock Spire"),
    "9484": d("Random drop, Zul'Farrak", "Rare bind on equip drop in Zul'Farrak."),
    "11841": d("Fineous Darkvire, Blackrock Depths"),
    "14144": c("Tailoring (290)", "Pattern: Ghostweave Pants drops from Spectral Citizens in Stratholme."),
    "12965": d("The Beast, Upper Blackrock Spire", UBRS),
    # Feet
    "11822": d("Golem Lord Argelmach, Blackrock Depths"),
    "18102": d("Warchief Rend Blackhand, Upper Blackrock Spire", UBRS),
    "13369": d("Balnazzar, Stratholme (Live)"),
    "18735": d("Maleki the Pallid, Stratholme (Undead)"),
    "13282": d("Spirestone Lord Magus, Lower Blackrock Spire", "Rare spawn."),
    "13391": d("Postmaster Malown, Stratholme (Undead)"),
    # Rings
    "12543": q("The Princess's Surprise, Blackrock Depths", ["Alliance"], "Alliance reward; Horde gets Eye of Orgrimmar from The Princess Saved?."),
    "12545": q("The Princess Saved?, Blackrock Depths", ["Horde"], "Horde reward; Alliance gets Songstone of Ironforge from The Princess's Surprise."),
    "12926": d("Pyroguard Emberseer, Upper Blackrock Spire", UBRS),
    "16058": q("In Dreams, Western Plaguelands", BOTH, "Group quest at the end of the Tirion Fordring chain; minimum level 52."),
    "18103": d("Warchief Rend Blackhand, Upper Blackrock Spire", UBRS),
    "13178": d("Urok Doomhowl, Lower Blackrock Spire"),
    # Trinkets
    "12930": d("Jed Runewatcher, Upper Blackrock Spire", "Rare spawn. " + UBRS),
    "13968": q("General Drakkisath's Demise / For The Horde!", BOTH, "Alliance: General Drakkisath's Demise. Horde: For The Horde!. Minimum level 55."),
    "11832": d("Ambassador Flamelash, Blackrock Depths"),
    "11819": d("Golem Lord Argelmach, Blackrock Depths"),
    # Main hand / two-hand
    "13964": d("Darkmaster Gandling, Scholomance"),
    "17780": d("Princess Theradras, Maraudon"),
    "17719": d("Tinkerer Gizlock, Maraudon"),
    "13349": d("Baron Rivendare, Stratholme (Undead)"),
    "10828": d("Shade of Eranikus, Sunken Temple"),
    "11923": d("Chest of The Seven, Blackrock Depths", "Loot from the Chest of The Seven after the Seven Dwarves encounter."),
    "11932": d("Emperor Dagran Thaurissan, Blackrock Depths"),
    # Off hand
    "10796": d("Green dragon mini-bosses, Sunken Temple"),
    "11904": q("It's Dangerous to Go Alone, Un'Goro Crater", BOTH, "End of the Linken quest chain; minimum level 47."),
    "11928": d("Emperor Dagran Thaurissan, Blackrock Depths"),
    # Ranged
    "13938": d("Darkmaster Gandling, Scholomance"),
    "11748": d("Pyromancer Loregrain, Blackrock Depths"),
    "13396": d("Skul, Stratholme (Live)", "Rare spawn."),
    "16997": q("Order Must Be Restored / The Scarlet Oracle, Demetria", BOTH, "Alliance: Order Must Be Restored, an outdoor raid quest. Horde: The Scarlet Oracle, Demetria, an elite quest. Minimum level 56."),
}


def alt(item_id, url, suffix=None):
    return {"itemId": item_id, "recommendationUrl": url, "requiredSuffix": suffix}


FROZEN, FIERY, SHADOW, HEALING = "of Frozen Wrath", "of Fiery Wrath", "of Shadow Wrath", "of Healing"

FROST_LIST = [
    alt(18727, IVM), alt(10504, IVM, FROZEN),
    alt(12103, IVM),
    alt(11782, IVM), alt(18681, IVM),
    alt(13386, IVM, FROZEN), alt(11623, IVM),
    alt(14152, IVM), alt(14340, IVM), alt(14136, IVM),
    alt(11766, IVM, FROZEN), alt(13409, IVM, FROZEN),
    alt(13253, IVM), alt(21318, IVM), alt(13870, ZKM),
    alt(11662, IVM), alt(13956, IVM), alt(18740, IVM),
    alt(13170, IVM), alt(9484, IVM),
    alt(11822, IVM), alt(18102, IVM),
    alt(12543, IVM), alt(12545, IVM),
    alt(12930, IVM), alt(13968, IVM), alt(11832, ZKM),
    alt(13964, IVM), alt(17780, IVM), alt(17719, IVM),
    alt(10796, IVM, FROZEN), alt(11904, IVM),
    alt(13938, IVM),
]

FIRE_LIST = [
    alt(18727, IVM), alt(10504, IVM, FIERY),
    alt(12103, IVM),
    alt(18681, IVM),
    alt(13386, IVM, FIERY), alt(11623, IVM),
    alt(14152, IVM),
    alt(11766, IVM, FIERY), alt(13409, IVM, FIERY),
    alt(13253, IVM), alt(21318, IVM),
    alt(11662, IVM), alt(13956, IVM), alt(18740, IVM),
    alt(13170, IVM), alt(9484, IVM),
    alt(13369, IVM), alt(11822, IVM), alt(18102, IVM),
    alt(12926, IVM), alt(12543, IVM), alt(12545, IVM),
    alt(12930, IVM), alt(13968, IVM),
    alt(13964, IVM), alt(17780, IVM), alt(17719, IVM),
    alt(10796, IVM, FIERY), alt(11904, IVM),
    alt(13938, IVM), alt(11748, IVM),
]

WARLOCK_LIST = [
    alt(18727, WTW), alt(10504, ZKW, SHADOW), alt(14111, ZKW),
    alt(18691, IVW), alt(12103, ZKW),
    alt(14112, WTW), alt(18681, WTW),
    alt(13386, ZKW, SHADOW), alt(11623, WTW),
    alt(14153, IVW), alt(14136, WTW), alt(14106, ZKW),
    alt(11766, ZKW, SHADOW), alt(13409, ZKW, SHADOW),
    alt(13253, IVW), alt(10787, IVW, SHADOW), alt(21318, ZKW),
    alt(11662, IVW), alt(13956, ZKW), alt(18740, WTW), alt(12589, WTW),
    alt(13170, IVW),
    alt(18735, IVW), alt(11822, WTW), alt(18102, WTW),
    alt(12543, IVW), alt(12545, WTW),
    alt(12930, IVW), alt(13968, IVW), alt(11832, ZKW),
    alt(13964, ZKW), alt(10828, ZKW, SHADOW),
    alt(10796, IVW, SHADOW), alt(11904, ZKW),
    alt(13396, IVW), alt(13938, WTW),
]

SHADOW_LIST = [
    alt(18727, ZKS), alt(10504, ZKS, SHADOW), alt(14111, ZKS),
    alt(18691, IVS),
    alt(14112, IVS), alt(18681, ZKS),
    alt(13386, IVS, SHADOW), alt(11623, ZKS),
    alt(14136, IVS), alt(14106, ZKS),
    alt(11766, IVS, SHADOW), alt(13409, ZKS, SHADOW),
    alt(13253, IVS), alt(21318, ZKS),
    alt(11662, IVS), alt(13956, ZKS), alt(18740, ZKS),
    alt(13170, IVS),
    alt(18735, IVS), alt(11822, ZKS),
    alt(12543, IVS), alt(12545, ZKS),
    alt(12930, IVS), alt(11819, IVS), alt(11832, ZKS),
    alt(13349, IVS), alt(10828, ZKS, SHADOW), alt(13964, ZKS),
    alt(10796, IVS, SHADOW), alt(11904, ZKS),
    alt(13396, IVS), alt(13938, ZKS),
]

DISC_LIST = [
    alt(18723, IVH), alt(13141, DEF),
    alt(18681, IVH), alt(11624, DEF), alt(13185, DEF), alt(16695, DEF),
    alt(13386, IVH, HEALING), alt(12110, DEF),
    alt(14154, IVH), alt(13346, DEF),
    alt(11766, IVH, HEALING), alt(16697, DEF), alt(16683, DEF),
    alt(10787, IVH, HEALING), alt(12554, DEF),
    alt(14143, DEF), alt(12589, DEF), alt(16696, DEF),
    alt(11841, IVH), alt(13170, DEF), alt(14144, DEF), alt(12965, DEF),
    alt(11822, IVH), alt(13282, DEF),
    alt(16058, IVH), alt(18103, IVH), alt(13178, DEF), alt(12543, DEF), alt(12545, DEF),
    alt(12930, IVH), alt(11819, IVH),
    alt(11923, IVH), alt(11932, DEF),
    alt(11928, IVH), alt(11904, DEF),
    alt(16997, IVH), alt(13938, DEF),
]

# Holy Priest: proposed additions to the 1.0.0 catalog (17 rows stay as-is).
HOLY_ADDITIONS = [
    alt(13141, DEF), alt(13960, SEE),
    alt(11624, DEF), alt(13185, DEF), alt(16695, DEF),
    alt(12110, DEF),
    alt(14154, IVH),
    alt(16697, DEF), alt(16683, DEF),
    alt(12554, DEF), alt(13253, SEE),
    alt(14143, DEF), alt(16696, DEF),
    alt(13170, DEF), alt(14144, DEF), alt(12965, DEF),
    alt(13282, DEF), alt(13391, SEE),
    alt(13178, DEF), alt(12543, DEF), alt(12545, DEF),
    alt(11932, DEF),
    alt(11904, DEF),
    alt(13938, DEF),
]

MAGE_METHOD = ("Named alternatives from the Icy Veins Classic Mage DPS pre-raid lists and the Zockify Classic Mage pre-BiS list. "
               "Items are kept only when available in Classic phase 1 from dungeons (not Dire Maul or raids), quests or crafting with a phase 1 recipe; "
               "world drops, vendor, PvP, reputation and later-phase items are excluded. Random-suffix rows require the suffix the guide names. No independent ranking.")
SPEC_NOTE = {
    "frost": " Icy Veins states its phase 1-4 lists emphasize Frost damage because raids in those phases are played as Frost.",
    "arcane": " Neither guide publishes a separate Arcane list; this uses the class-wide phase 1 Mage DPS list (Frost-oriented per Icy Veins).",
    "fire": " Uses the Icy Veins phase 5-6 lists, which the guide states are Fire-oriented, filtered to items available in phase 1.",
}

SPECS = {
    "mage-arcane": {"characterClass": "Mage", "specializationId": "arcane", "sourceUrl": IVM,
                    "supplementarySourceUrls": [ZKM], "status": "Partial",
                    "selectionMethod": MAGE_METHOD + SPEC_NOTE["arcane"], "alternatives": FROST_LIST},
    "mage-fire": {"characterClass": "Mage", "specializationId": "fire", "sourceUrl": IVM,
                  "supplementarySourceUrls": [], "status": "Reviewed",
                  "selectionMethod": MAGE_METHOD.replace(" and the Zockify Classic Mage pre-BiS list", "") + SPEC_NOTE["fire"],
                  "alternatives": FIRE_LIST},
    "mage-frost": {"characterClass": "Mage", "specializationId": "frost", "sourceUrl": IVM,
                   "supplementarySourceUrls": [ZKM], "status": "Partial",
                   "selectionMethod": MAGE_METHOD + SPEC_NOTE["frost"], "alternatives": FROST_LIST},
    "priest-discipline": {"characterClass": "Priest", "specializationId": "discipline", "sourceUrl": IVH,
                          "supplementarySourceUrls": [DEF], "status": "Partial",
                          "selectionMethod": ("Named alternatives from the Icy Veins Classic Priest Healer pre-raid lists and the Warcraft Tavern "
                                              "Holy/Discipline phase 1 (pre Dire Maul) list by Defcamp. Kept only when available in phase 1 from dungeons "
                                              "(not Dire Maul or raids), quests or crafting with a phase 1 recipe; world drops, vendor, PvP, reputation and "
                                              "later-phase items are excluded. Random-suffix rows require the suffix the guide names. No independent ranking."),
                          "alternatives": DISC_LIST},
    "priest-shadow": {"characterClass": "Priest", "specializationId": "shadow", "sourceUrl": IVS,
                      "supplementarySourceUrls": [ZKS], "status": "Partial",
                      "selectionMethod": ("Named alternatives from the Icy Veins Classic Shadow Priest pre-raid lists and the Zockify Classic Shadow "
                                          "Priest pre-BiS list. Kept only when available in phase 1 from dungeons (not Dire Maul or raids), quests or "
                                          "crafting with a phase 1 recipe; world drops, vendor, PvP, reputation and later-phase items are excluded. "
                                          "Random-suffix rows require the suffix the guide names. No independent ranking."),
                      "alternatives": SHADOW_LIST},
}
WARLOCK_METHOD = ("Named alternatives from the Icy Veins Classic Warlock DPS pre-raid lists, the Warcraft Tavern Warlock pre-raid list by Zephan "
                  "and the Zockify Classic Warlock pre-BiS list. These are class-wide Warlock lists without separate spec lists. Kept only when "
                  "available in phase 1 from dungeons (not Dire Maul or raids), quests or crafting with a phase 1 recipe; world drops, vendor, PvP, "
                  "reputation and later-phase items are excluded. Random-suffix rows require the suffix the guide names. No independent ranking.")
for spec in ("affliction", "demonology", "destruction"):
    SPECS["warlock-" + spec] = {"characterClass": "Warlock", "specializationId": spec, "sourceUrl": IVW,
                                "supplementarySourceUrls": [WTW, ZKW], "status": "Partial",
                                "selectionMethod": WARLOCK_METHOD, "alternatives": WARLOCK_LIST}

GAP_NOTES = {
    "mage-arcane": ["Finger2: only one phase 1 dungeon/quest ring per faction is named (Songstone of Ironforge / Eye of Orgrimmar); the other named rings are world drops or later phases."],
    "mage-frost": ["Finger2: only one phase 1 dungeon/quest ring per faction is named (Songstone of Ironforge / Eye of Orgrimmar); the other named rings are world drops or later phases."],
    "priest-discipline": ["Head: the only named phase 1 head (Cassandra's Grace) is a world drop."],
    "priest-shadow": ["Finger2: only one phase 1 dungeon/quest ring per faction is named (Songstone of Ironforge / Eye of Orgrimmar)."],
}
for spec in ("affliction", "demonology", "destruction"):
    GAP_NOTES["warlock-" + spec] = ["Finger2: only one phase 1 dungeon/quest ring per faction is named (Songstone of Ironforge / Eye of Orgrimmar); Maiden's Circle and Underworld Band are world drops."]

used = {a["itemId"] for s in SPECS.values() for a in s["alternatives"]} | {a["itemId"] for a in HOLY_ADDITIONS}
missing = sorted(i for i in used if str(i) not in ITEMS)
if missing:
    raise SystemExit("No acquisition facts for %s" % missing)

with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as handle:
    json.dump({"reviewedOn": REVIEWED_ON, "items": ITEMS, "specs": SPECS, "gaps": GAP_NOTES,
               "holyAdditions": {"purpose": ("Proposed Holy Priest Classic phase 1 alternatives to merge into the existing catalog. "
                                             "The 17 existing 1.0.0 rows are not repeated and keep their row IDs. Row IDs here follow the "
                                             "new contract and may be changed by the main agent when merging."),
                                 "sourceUrls": [IVH, DEF, SEE], "alternatives": HOLY_ADDITIONS}},
              handle, indent=2, ensure_ascii=False)
    handle.write("\n")
print("items", len(ITEMS), "specs", len(SPECS))
