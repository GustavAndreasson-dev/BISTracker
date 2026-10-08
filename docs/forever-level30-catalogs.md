# Forever: kataloger för alla 27 specs

Levererat och granskat 2026-10-08, REQ-008/009. Appen innehåller separata
kataloger för alla nio klasser och deras tre specs i **Forever beta, nivå 30,
patch 1.60.1**. Classic Holy Priests befintliga 17-item-lista bevaras.

## Urval och källspår

Katalogerna innehåller originalguidernas namngivna utrustningsalternativ,
med Forever-itemidentitet, anskaffning, villkor, itemlänk och rekommendationslänk.
Appen visar endast dungeons och quests enligt den tidigare avgränsningen.
Övriga granskade guidekällor bevaras i råkatalogerna inför ett möjligt ändrat
beslut. Ingen egen ranking eller statviktning har införts.

1394 råa rekommendationsrader ger **1020 synliga placeringsrader** efter
dungeon/quest-filtret. Antalet räknar alternativ och möjliga ring-/trinket-/
vapenplaceringar; det är inte antalet items att skaffa till en färdig uppsättning.
Varje slot kan bara ha ett utrustat val. Ägande delas mellan karaktärens specs
och katalogset; utrustningsuppsättningarna sparas separat.

`Reviewed` anger granskade guidealternativ; `Partial` anger dessutom att
guidernas betaurval har täckningsluckor. Ingen status garanterar ett optimalt
eller komplett rangordnat set. Tabellen visar täckning efter filtret.
En tvåhandsuppsättning behöver ingen OffHand, och vissa klasser använder relic
i Ranged-platsen; 17 slots är därför inte ett universellt fullständighetskrav.

| Klass | Specs i tabellordning | Råa rader | Dungeons/quests | Täckt antal slots |
| --- | --- | --- | --- | --- |
| Druid | Balance / Feral / Restoration | 63 / 57 / 36 | 41 / 41 / 27 | 15 / 14 / 14 |
| Hunter | Beast Mastery / Marksmanship / Survival | 31 / 33 / 35 | 25 / 25 / 25 | 14 / 14 / 13 |
| Mage | Arcane / Fire / Frost | 105 / 105 / 110 | 76 / 76 / 76 | 17 / 17 / 17 |
| Paladin | Holy / Protection / Retribution | 14 / 16 / 41 | 10 / 10 / 23 | 10 / 8 / 12 |
| Priest | Discipline / Holy / Shadow | 65 / 65 / 62 | 52 / 52 / 47 | 17 / 17 / 17 |
| Rogue | Assassination / Combat / Subtlety | 33 / 42 / 42 | 30 / 39 / 39 | 14 / 14 / 14 |
| Shaman | Elemental / Enhancement / Restoration | 26 / 32 / 26 | 18 / 25 / 18 | 9 / 14 / 9 |
| Warlock | Affliction / Demonology / Destruction | 104 / 104 / 104 | 72 / 72 / 72 | 17 / 17 / 17 |
| Warrior | Arms / Fury / Protection | 15 / 9 / 19 | 11 / 7 / 11 | 8 / 6 / 8 |

Primärkällor, metadata- och questkontroller, uteslutna källfel samt konkreta
luckor redovisas per grupp:

- [Mage, Priest och Warlock](forever-level30-casters.md): nio kataloger,
  824 rader; källbelagda Priest-wands och uteslutna engineering-items vars
  skillkrav inte nås på nivå 30.
- [Druid, Paladin och Shaman](forever-level30-hybrids.md): nio kataloger,
  311 rader; uteslutet level 31-item och felaktig dungeonmappning.
- [Hunter, Rogue och Warrior](forever-level30-physical.md): nio kataloger,
  259 rader; suffix och tillåtna vapenplaceringar granskade separat.

Produktfiler ligger i `BISTracker.Infrastructure/Catalog/Data/Forever/level30/`.
Forskningsfakta ligger i `docs/data/forever-casters/` och `docs/data/forever-hybrids/`;
verktygen ligger i motsvarande grupper under `tools/data/`, inklusive
`tools/data/forever-physical/`. Full HTML och fullständiga tooltips versionshanteras
inte. Programstart läser JSON-kataloger och hämtar inga guidetabeller.

## Nivå 60 och verifiering

Nivå 60 väljs som ett separat katalogset. Fram tills källgranskade kataloger
finns visas en tom lista med datastatus. **Import catalogs** kopierar validerade
JSON-pack från en vald mapp; ingen omkompilering behövs. Både betans och
lanseringens nivå 60 stöds av formatet. Nivå 30-data och dess sparade utrustning
bevaras. [Importformat och regler](catalog-contexts.md).

Release-build passerar utan varningar/fel och samtliga 39 beteendekontroller
är godkända. Den isolerade WinUI-kontrollen använder verklig nivå 30-data,
verifierar nivåbyte och import som uppdaterar en redan vald tom lista. Dess
tydligt fiktiva nivå 60-prov används bara i TEMP och är inte produktdata.
[Verifiering och granskade bilder](verification.md).

Fyra arbetstrådar har använts: huvudtråden för modell, import, UI, integration
och commits; tre specialisttrådar för varsin grupp om nio specs, följt av
korsgranskning, regressionskontroller och dokumentgranskning.

Betaguider kan förändras, och hela questkedjor har inte spelats i en klient.
Antal identiska exemplar modelleras inte; placering av ett icke-unikt item i
två slots bevisar därför inte att spelaren äger två exemplar. Verklig nivå
60-data och kompletta urval för guidernas täckningsluckor återstår.
