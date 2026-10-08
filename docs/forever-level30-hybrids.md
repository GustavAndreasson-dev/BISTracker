# Forever beta level 30: Druid, Paladin och Shaman

Granskat 2026-10-08. Chefens beställning gäller alla specs och senare separat
nivå 60-import. Dessa nio JSON-kataloger innehåller ursprungliga Forever-
guiders uttryckliga utrustningsalternativ. Patchkontexten är **1.60.1**, betan
och nivåtaket **30**. De är inte Vanilla-, SoD- eller framtida nivå 60-listor.

## Kataloger och originalkällor

| Klass/spec | Rekommendationsrader | Dungeons/quests | Originalguide |
| --- | ---: | ---: | --- |
| Druid Balance | 63 | 41 | [DrWonder, Wowhead](https://www.wowhead.com/forever/guide/classes/druid/balance/level-30-dps-overview) |
| Druid Feral | 57 | 41 | [Lexolas, DPS](https://www.wowhead.com/forever/guide/classes/druid/feral/level-30-dps-overview) och [tank](https://www.wowhead.com/forever/guide/classes/druid/feral/level-30-tank-overview) |
| Druid Restoration | 36 | 27 | [Frankensteak, Wowhead](https://www.wowhead.com/forever/guide/classes/druid/restoration/level-30-healer-overview) |
| Paladin Holy | 14 | 10 | [Harreks, Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/holy/level-30-healer-overview) |
| Paladin Protection | 16 | 10 | [Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/protection/level-30-tank-overview) |
| Paladin Retribution | 41 | 23 | [Surveillant, Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/retribution/level-30-dps-overview) |
| Shaman Elemental | 26 | 18 | [Lucenia, Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/elemental/level-30-dps-overview) |
| Shaman Enhancement | 32 | 25 | [Yebb, Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/enhancement/level-30-dps-overview) |
| Shaman Restoration | 26 | 18 | [Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/restoration/level-30-healer-overview) |

Totalt 311 rekommendationsrader. Antalet är större än antalet verkliga items:
alternativ till samma slot förekommer och ringar/trinkets har två möjliga
placeringar. Det innebär inte att guiden rekommenderar två exemplar av ett
unikt item. Feral förblir en spec; anteckningarna anger tank-/DPS-alternativ.

Enhancement kompletteras från [Wordups ursprungliga Forever-guide](https://www.icy-veins.com/wow-forever/enhancement-shaman-melee-dps-pve-guide).
Retributions Reedknot Ring rekommenderas uttryckligen i [Meyras Forever-guide](https://www.icy-veins.com/wow-forever/retribution-paladin-melee-dps-pve-guide/).
Itemmetadata för ett gemensamt item kan återanvändas, men klassens/specens
rekommendationskälla följer varje katalograd och har kontrollerats separat.

## Urval och verifiering

- Wowheads Forever-guideinnehåll och `WH.Gatherer.addData(..., 16, ...)`
  kopplar respektive tabellrad till positivt item-ID, engelskt namn och slot.
  Metadata har inte hämtats från Classic-namespace.
- Forever-tooltiparna kontrollerar namn, faktisk miniminivå, vapenhand,
  tvåhandsvapen, Unique, professionkrav och bindning. Ikonlänkar har gett
  HTTP 200. [Itemkontroller](data/forever-hybrids/item-verification.json)
  omfattar 203 kontrollerade identiteter, inklusive sådana som senare uteslutits.
- 45 berörda quests kontrollerades separat i Forever-namespace; alla hade
  verifierad miniminivå högst 30. [Questkontroller](data/forever-hybrids/quest-verification.json).
  Fraktionsvillkor kommer från Forever-questmetadata och guiderna. Hela
  förquestkedjor har inte spelats igenom i klienten.
- Katalogerna behåller Crafting och en Reputation-källa som granskningsdata.
  Appens beslutade anskaffningsfilter avgör vilka rader som visas. Guidernas
  alternativ ersätts inte med självberäknade statvikter efter filtrering.
- Konsumtionsvaror, enchants och generiska itemfamiljer utan ett individuellt
  utrustnings-ID har inte importerats som utrustning.
- Inget suffix har gissats. Vapen, unikhet och itemägande använder verifierad
  physical item-identitet; rekommendations-ID:n skiljer alternativa placeringar.

## Upptäckta källfel och kvarstående hål

`Partial` innebär att originalguidernas betaurval är preliminärt och har hål;
det innebär inte att katalogerna är tomma. En tom slot fylls inte med en
oprovad ersättare. Vapenpar och tvåhandsalternativ är alternativa uppsättningar,
så OffHand saknas avsiktligt när ett relevant tvåhandsval används.

- Electromagnetic Gigaflux Reactivator (9492) förekommer i Shamanguidernas
  nivå 30-tabeller men Forever-metadata kräver **31**. Det har uteslutits.
- Retributions Wowhead-tabell länkar crafted Defender's Leather Helm (252455)
  som flera dungeonbossars drops. Den felaktiga dungeonmappningen har uteslutits.
- Balance-tabellrader med flera quests och fler/färre belöningar behölls bara
  när belöningens questrelation kunde fastställas. Övriga är dokumenterade i
  [uteslutningsrapporten](data/forever-hybrids/excluded-records.json).
- Originalguidernas kvarvarande täckningshål: Feral och Restoration Druid saknar
  relic i detta urval; Holy Paladin saknar Neck, Hands, Feet, OffHand och relic;
  Protection Paladin saknar Back, Wrist och Waist; Elemental/Restoration Shaman
  saknar Back, Neck och trinkets; Enhancement saknar Back. Dungeons/quests-
  filtrering kan skapa ytterligare hål när enda källa är crafting.

Icy Veins Balance/Restoration Druid samt Holy/Protection Paladin hade ännu inga
konkreta nivå 30-tabeller vid kontrollen. Wowheads ursprungliga Forever-guider
gav separat underlag för samtliga. Alternativa communitysidor granskades också
(ForeverChanges, WoW Forever Tools och wow-forever.gg), men deras bredare
datamined/scorade urval har inte använts för att fylla hålen.

## Importgräns inför nivå 60

Nivåtak, Beta/Launch, patch, klass/spec, sourceUrl och catalogId är uttryckliga
rootfält. Nivå 60 får en separat katalogidentitet och egna källgranskade data;
ingen nivå 30-rekommendation får automatiskt etiketteras om till level 60.
Huvudtråden äger loader, kontextisolering, UI och integrationstester.

Researchverktygen i `tools/data/forever-hybrids/` läser explicit nedladdat
guideunderlag ur en temporär mapp. Originalsidornas HTML och fulla tooltips
versionshanteras inte. Importverktyget vägrar ersätta befintliga kataloger
utan ett uttryckligt arbetsimportargument; tidigare filer säkerhetskopierades
före granskningens korrigeringar. `verify-working-import.py` kräver uttryckligt
`--update-working-catalogs` för sin eftergranskning av arbetskatalogerna.
Verktygen är inte en nätverksfunktion i appen.

Ingen commit eller staging har utförts av specialisttråden. Huvudtråden
verifierar och samordnar commits enligt Chefens uppdrag.
