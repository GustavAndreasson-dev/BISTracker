# Forever: kataloger för alla 27 specs

Levererat och granskat 2026-10-08, REQ-008/009. Appen innehåller separata
kataloger för alla nio klasser och deras tre specs i **Forever beta, nivå 30,
patch 1.60.1**. Classic Holy Priests befintliga 17-item-lista bevaras.

## Urval och källspår

Katalogerna innehåller originalguidernas namngivna utrustningsalternativ,
med Forever-itemidentitet, anskaffning, villkor, itemlänk och rekommendationslänk.
Chefen har godkänt **dungeons, quests och crafting för Forever**, DEC-012.
Classic behåller dungeons/quests. Övriga källtyper filtreras bort.
Ingen egen ranking eller statviktning har införts. Kompletteringar kommer
från respektive specs ursprungliga Forever-guider, inklusive WOWTBC:s
orankade utrustningsalternativ, och individuella Forever-item-/quest-/receptposter.

1655 råa rekommendationsrader ger **1635 alternativa placeringsrader** efter
källfiltret. UI grupperar dem till ett mål per relevant slot, högst 17 och
16 för en vald tvåhandsuppsättning. Alternativ ökar inte antalet mål eller framsteg.
Varje slot kan bara ha ett utrustat val. Ägande delas mellan karaktärens specs
och katalogset; utrustningsuppsättningarna sparas separat.

`Reviewed` anger granskade guidealternativ; `Partial` anger att vidare
granskning behövs. Statusfältet är aldrig ensamt bevis på fullständighet.
[Releasekontrollen](data/forever-slot-release-audit.json) passerar för samtliga
27 kataloger, utan luckor eller okända questfraktioner. En oberoende
[integrationskontroll](data/forever-slot-witness-integration.json) matchar
54 konkreta lagliga exempelset mot aktiva produkt-ID:n, fraktion, distinkta
itemvarianter, unique, handkrav och granskade yrkeskrav.
Exempelseten bevisar genomförbarhet, inte optimal ranking eller likvärdig styrka.

| Klass | Specs i tabellordning | Aktiva alternativrader | Relevanta slots i releasekontrollens giltiga kombination |
| --- | --- | --- | --- |
| Druid | Balance / Feral / Restoration | 71 / 64 / 40 | 17 / 16 / 17 |
| Hunter | Beast Mastery / Marksmanship / Survival | 33 / 33 / 38 | 16 / 16 / 17 |
| Mage | Arcane / Fire / Frost | 106 / 106 / 111 | 17 / 17 / 17 |
| Paladin | Holy / Protection / Retribution | 34 / 30 / 49 | 17 / 17 / 16 |
| Priest | Discipline / Holy / Shadow | 67 / 67 / 64 | 17 / 17 / 17 |
| Rogue | Assassination / Combat / Subtlety | 41 / 52 / 52 | 17 / 17 / 17 |
| Shaman | Elemental / Enhancement / Restoration | 45 / 36 / 45 | 17 / 16 / 17 |
| Warlock | Affliction / Demonology / Destruction | 104 / 104 / 104 | 17 / 17 / 17 |
| Warrior | Arms / Fury / Protection | 40 / 48 / 51 | 16 / 17 / 17 |

Primärkällor, metadata- och questkontroller, uteslutna källfel samt konkreta
genomförbara exempelset redovisas per grupp:

- [Mage, Priest och Warlock](forever-level30-casters.md): nio kataloger,
  källbelagda wands/trinkets, receptkrav och uteslutna osäkra questitems.
- [Druid, Paladin och Shaman](forever-level30-hybrids.md): nio kataloger,
  craftingrelics, fraktionsbundna questvägar och receptkontroller.
- [Hunter, Rogue och Warrior](forever-level30-physical.md): nio kataloger,
  klassquests, suffix, crafting och källbelagda vapenupplägg.

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

Release-build passerar utan varningar/fel och samtliga 46 beteendekontroller
är godkända. Den isolerade WinUI-kontrollen använder verklig nivå 30-data,
verifierar nivåbyte och import som uppdaterar en redan vald tom lista. Dess
tydligt fiktiva nivå 60-prov används bara i TEMP och är inte produktdata.
[Verifiering och granskade bilder](verification.md).

Fyra arbetstrådar har använts: huvudtråden för modell, import, UI, integration
och commits; tre specialisttrådar för varsin grupp om nio specs, följt av
korsgranskning, regressionskontroller och dokumentgranskning.

Betaguider kan förändras, och hela questkedjor har inte spelats i en klient.
Ägandet registrerar ett exemplar per variant; samma variant kan inte fylla
två mål. Questkedjor, alternativa belöningar, nivåtak, bindning och aktiva
yrkeskrav har granskats mot publicerade poster. Ingen full genomspelning i
en klient har gjorts. Verklig nivå 60-data och distributionstest återstår.
