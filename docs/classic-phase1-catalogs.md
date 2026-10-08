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
| `items[].id` | `<catalogId>-<slot gemener>-<itemId>`, valfritt följt av `-<gemener/siffror>` för att skilja t.ex. suffixvarianter; unikt inom katalogen |
| `items[].recommendationId` | Valfritt. Endast Holy Priest: bevarar rad-ID:n från 1.0.0 (`classic-p1-item-<itemId>`, med `-of-healing` för suffixrader). |

Filer: `BISTracker.Infrastructure/Catalog/Data/Classic/phase1/<klass>-<spec>.json`.
Forskningsunderlag per klassgrupp: `docs/data/classic-<grupp>/`.

## Klassgrupper

| Grupp | Klasser |
| --- | --- |
| casters | Mage, Priest, Warlock |
| hybrids | Druid, Paladin, Shaman |
| physical | Hunter, Rogue, Warrior |

## Implementationsstatus

Infrastrukturen är implementerad 2026-10-08; data för de övriga 26 specs
levereras separat av dataagenterna.

- `ReviewedCatalogReader` (tidigare `ForeverCatalogReader`) läser både Forever
  och Classic. Classic kräver `gameVersion` `Classic`, `levelCap` 60,
  `releaseStage` `Classic`, `phase` exakt `Phase 1 · pre-raid` och
  `catalogId` enligt tabellen; paketet får `CatalogSet.ClassicPhaseOne`.
  `patch` är frivilligt och läggs i så fall till fasetiketten som för Forever.
- Källpolicy: Dungeon, Quest och Crafting visas; övriga råa rader valideras men
  filtreras bort. Urvalsetiketten är versionsneutral.
- Rad-ID: Classic-radens `id` används oförändrat som rekommendations-ID (det
  innehåller redan katalog-ID:t). Läsaren kräver mönstret
  `<catalogId>-<slot>-<itemId>` med samma slot och item-ID som raden, valfritt
  följt av `-<gemener/siffror>`. Råa rad-ID:n måste vara unika.
- `recommendationId`: tillåts endast i `classic-phase1-level60-priest-holy` och
  måste vara exakt `classic-p1-item-<itemId>` för basitem eller
  `classic-p1-item-<itemId>-of-healing` när `requiredSuffix` är `of Healing`.
  Andra suffix, andra kataloger och Forever avvisas. Dubbletter avvisas.
- `CharacterCatalog` läser inbyggda resurser under `Catalog/Data/Classic/` och
  `Catalog/Data/Forever/`; en resurs under fel versionsmapp avvisas. Spec utan
  paket visar "No reviewed BiS list available yet.". Classic-pack kan inte
  importeras (`CatalogPackImporter` avvisar dem uttryckligen).
- Holy Priest: `priest-holy.json` innehåller samma 17 rader som 1.0.0 med
  oförändrade rekommendations-ID:n via `recommendationId`. Nivåkrav, Unique och
  vapenhand är kontrollerade mot Wowhead Classic-tooltips 2026-10-08 (Fordring's
  Seal och Stormrager saknar nivåkrav i tooltipen och anges därför inte).
  Båda questraderna gäller båda fraktionerna enligt phase1-items.md. Status är
  `Partial` tills crafting-alternativ enligt DEC-016 är granskade.
  Den gamla filen `holy-priest-classic-phase1.json` är borttagen;
  `ClassicPhaseOneBisCatalog` är nu en vy över paketets 17 rader med 1.0.0-ID:n
  för den äldre `TrackerService` och tidigaste framstegsfilen.
- Releasekontroll: `--catalog-release-audit <fil> [Classic|Forever]` granskar
  standardsetet för båda versionerna (eller en vald) med samma slot- och
  fraktionskontroll och rapporterar `catalogStatus` (`Reviewed`, `Partial`,
  `Missing`). `Missing` blockerar; `Partial` rapporteras.
  `tools/data/verify-forever-slot-witnesses.py` är fortsatt endast Forever.
- Kontroller: fem `ClassicCatalogScenarios`; totalt 54 beteendekontroller.
