# Classic fas 1-kataloger (beta 2)

Statusdatum: 2026-10-08. Kontrakt för [REQ-013](requirements.md#appens-beta-2)
enligt [DEC-016](decisions.md#dec-016--scope-för-appens-beta-2). Gäller
datagranskare, katalogläsaren och kontrollerna. Status: **under arbete**.

## Urval och källpolicy

- Spelversion: WoW Classic (Vanilla), fas 1, nivå 60, pre-raid.
- Tillåtna källor: `Dungeon`, `Quest` och `Crafting`. Raid (Molten Core,
  Onyxia), world drops, vendor, PvP och reputation-belöningar ingår inte.
- Fas 1-innehåll: alla 5-mannadungeons och UBRS utom Dire Maul (fas 2).
  Recept och questar som släpptes efter fas 1 ingår inte.
- Crafting: receptet ska kunna fås i fas 1. `source` anger yrke och
  skillnivå; `note` anger receptkälla och, för BoP, att yrket krävs.
- Urval: alternativ som uttryckligen rekommenderas av en namngiven
  Classic pre-raid BiS-guide för specen. Ingen egen ranking eller stat weights.
- Quests anger `availableFactions`. Fraktionsspecifika dungeon-/craftingvillkor
  anges också.

## JSON-format

Samma schema 1 som Forever-katalogerna (se [katalogkontexter](catalog-contexts.md)),
med dessa fasta värden:

| Fält | Värde |
| --- | --- |
| `catalogId` | `classic-phase1-level60-<klass gemener>-<specId>`, t.ex. `classic-phase1-level60-mage-frost` |
| `gameVersion` | `Classic` |
| `levelCap` | `60` |
| `releaseStage` | `Classic` |
| `phase` | `Phase 1 · pre-raid` |
| `status` | `Reviewed` eller `Partial` |
| `items[].id` | `<catalogId>-<slot gemener>-<itemId>`; unikt inom katalogen |
| `items[].recommendationId` | Valfritt. Endast Holy Priest: bevarar rad-ID:n från 1.0.0 (`classic-p1-item-<itemId>`, med `-of-healing` för suffixrader). |

Filer: `BISTracker.Infrastructure/Catalog/Data/Classic/phase1/<klass>-<spec>.json`.
Forskningsunderlag per klassgrupp: `docs/data/classic-<grupp>/`.

## Klassgrupper

| Grupp | Klasser |
| --- | --- |
| casters | Mage, Priest, Warlock |
| hybrids | Druid, Paladin, Shaman |
| physical | Hunter, Rogue, Warrior |
