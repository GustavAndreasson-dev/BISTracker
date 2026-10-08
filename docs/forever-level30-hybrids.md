# Forever beta level 30: Druid, Paladin och Shaman

Överlämning inför appens beta 2: [beta2-handoff.md](beta2-handoff.md).
Denna granskning ingår i levererad 1.0.0 från kod `e25c134` med
leveransdokumentation `043d859`. Nya beta 2-funktioner är ännu inte beslutade.

Slutgranskat 2026-10-08. De nio katalogerna gäller Forever **1.60.1**, beta och
nivåtak **30**. Chefens godkända anskaffningspolicy är **Dungeon, Quest och
Crafting**. Nivå 60 får senare separat källunderlag och katalogidentitet.

## Kataloger och originalkällor

| Klass/spec | Råa placeringsrader | Aktiva D/Q/Crafting | Dungeons/quests | Originalguide |
| --- | ---: | ---: | ---: | --- |
| Druid Balance | 71 | 71 | 49 | [Wowhead](https://www.wowhead.com/forever/guide/classes/druid/balance/level-30-dps-overview) |
| Druid Feral | 64 | 64 | 47 | [DPS](https://www.wowhead.com/forever/guide/classes/druid/feral/level-30-dps-overview), [tank](https://www.wowhead.com/forever/guide/classes/druid/feral/level-30-tank-overview) |
| Druid Restoration | 42 | 40 | 32 | [Wowhead](https://www.wowhead.com/forever/guide/classes/druid/restoration/level-30-healer-overview) |
| Paladin Holy | 34 | 34 | 29 | [Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/holy/level-30-healer-overview) |
| Paladin Protection | 30 | 30 | 24 | [Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/protection/level-30-tank-overview) |
| Paladin Retribution | 49 | 49 | 31 | [Wowhead](https://www.wowhead.com/forever/guide/classes/paladin/retribution/level-30-dps-overview) |
| Shaman Elemental | 45 | 45 | 37 | [Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/elemental/level-30-dps-overview) |
| Shaman Enhancement | 36 | 36 | 29 | [Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/enhancement/level-30-dps-overview) |
| Shaman Restoration | 45 | 45 | 37 | [Wowhead](https://www.wowhead.com/forever/guide/classes/shaman/restoration/level-30-healer-overview) |

Totalt **416 råa och 414 aktiva placeringsrader**: 315 dungeon/quest och 99
crafting. Restoration Druids två reputationplaceringar är endast rådata.
Placeringsrader är inte utrustningsslots eller en ranking. Alternativen grupperas
under en målplats per slot; ringar och trinkets har två möjliga placeringar utan
att två exemplar rekommenderas. Feral är en spec med markerade DPS-/tankalternativ.

Luckorna kompletterades med uttryckliga same-spec-rekommendationer från
WOWTBC:s egna Forever-listor:
[Balance](https://wowtbc.gg/warcraftforever/bis-list/balance-druid/),
[Feral DPS](https://wowtbc.gg/warcraftforever/bis-list/feral-dps-druid/),
[Restoration Druid](https://wowtbc.gg/warcraftforever/bis-list/restoration-druid/),
[Holy](https://wowtbc.gg/warcraftforever/bis-list/holy-paladin/),
[Protection](https://wowtbc.gg/warcraftforever/bis-list/protection-paladin/),
[Retribution](https://wowtbc.gg/warcraftforever/bis-list/retribution-paladin/),
[Elemental](https://wowtbc.gg/warcraftforever/bis-list/elemental-shaman/),
[Enhancement](https://wowtbc.gg/warcraftforever/bis-list/enhancement-shaman/) och
[Restoration Shaman](https://wowtbc.gg/warcraftforever/bis-list/restoration-shaman/).
Författaren beskriver dessa som värdefulla alternativ utan inbördes ordning.
Rekommendations-ID:t måste förekomma i specens verkliga slotlista; förekomst i
sidans gemensamma itemmetadata räcker inte som rekommendation.

Icy Veins ursprungliga Forever-guider kompletterar
[Feral](https://www.icy-veins.com/wow-forever/feral-druid-melee-dps-and-tank-pve-guide/),
[Retribution](https://www.icy-veins.com/wow-forever/retribution-paladin-melee-dps-pve-guide/) och
[Enhancement](https://www.icy-veins.com/wow-forever/enhancement-shaman-melee-dps-pve-guide).
Varje rekommendation har egen källkontroll. Classic/SoD-tabeller, godtyckliga
statvikter och automatiskt scorade urval har inte använts för att fylla slots.

## Faktiskt möjliga uppsättningar

[Slotgranskningen](data/forever-hybrids/slot-audit.json) innehåller **18 separata
legalSetWitness**, en per spec och fraktion. Varje vittne anger verkligt
rekommendations-ID, item, slot, källa och vald quest. Det är ett bevis på en
möjlig uppsättning, inte ett påstående om optimalt BiS-set eller likvärdighet.

- Alla relevanta slots täcks separat för Alliance och Horde. Feral,
  Retribution och Enhancement har källbelagd tvåhandsplan; OffHand upptas då
  av huvudvapnet. Övriga vittnen har ett giltigt enhand/offhand-par.
- Vittnena använder olika fysiska ringar och trinkets. Unique-items dubbleras
  inte och två alternativa belöningar från samma quest väljs aldrig tillsammans.
- Alla valda questitems har observerad Forever-belöningsrelation och verifierad
  questminiminivå högst 30. Alla 116 questplaceringsrader har källbelagda
  availableFactions; okända fraktioner finns inte i aktiva kataloger.
- Feral Alliance använder Defender's Leather Helm, egen Leatherworking **130**.
  Alla Paladin-vittnen använder Tenets of the Silver Hand, egen Enchanting
  **130**, i relicslotten. Övriga vittnen behöver inget craftingyrke.
  Ingen uppsättning kräver fler än två samtidiga professioner.

Samtliga **76 individuella craftingitems** har observerad Forever-receptrelation
med spell-ID, yrke, skill, bindning, eventuellt equipyrke och ingredienser i
craftingReviews. Receptskill är högst 225. Forever-guidernas ranktabeller
bekräftar Journeyman från nivå 10 och Expert från nivå 20:
[Enchanting](https://www.icy-veins.com/wow-forever/enchanting),
[Leatherworking](https://www.icy-veins.com/wow-forever/leatherworking),
[Blacksmithing](https://www.icy-veins.com/wow-forever/blacksmithing),
[Tailoring](https://www.icy-veins.com/wow-forever/tailoring) och
[Engineering](https://www.icy-veins.com/wow-forever/engineering).
Flera BoP-professionsalternativ innebär inte att alla ska bäras samtidigt;
vittnena väljer en kompatibel kombination.

[Extra receptåtkomstkontroll](data/forever-hybrids/crafting-accessibility.json)
visar att Paladins Tenets-formula säljs för 45 Merchant's Favor på båda
fraktionernas läger och kräver vanlig Runed Silver Rod, ingen specialforge.
Mystic Mushroom och Polished Driftwood Icon har samma individuellt verifierade
formulakostnad/försäljare; villkoret finns i alla relevanta produktanteckningar
och i writern. Det har inte antagits enbart från Tenets.
För Defender's Leather Helm stöds åtkomsten av originalguidernas uttryckliga
nivå 30-rekommendation och aktuell receptrelation; en separat trainer-/pattern-
tabell saknas på spell-sidan. Ingen oberoende trainerväg har därför påståtts.

## Metadata, korrigeringar och begränsningar

Forever-guideinnehåll och WH.Gatherer.addData(...,16,...) kopplar ID till
namn och slot. Forever-tooltiparna kontrollerar miniminivå, vapenhand,
tvåhandsvapen, Unique och bindning; ikonlänkar gav HTTP 200.
[Tidigare itemkontroller](data/forever-hybrids/item-verification.json) omfattar
203 identiteter och [tidigare questkontroller](data/forever-hybrids/quest-verification.json)
45 quests. Slotgranskningen tillför 91 kontrollerade spec/slot-rekommendationer
samt aktuella faction-, recipe- och uppsättningsbevis.

Electromagnetic Gigaflux Reactivator (9492) kräver 31 och är uteslutet.
Fallen Guard's Pendant (279837) är borttaget eftersom Starving Arcane saknar
verifierad fraktion. Healer's Staff (271767) och Magistrate's Pantaloons (270036)
är uteslutna tills Alliance-fix respektive motstridiga questbelönings-ID:n är
lösta. Vittnena beror inte på dessa items.

En tidigare slutsats om Defender's Leather Helm (252455) har korrigerats:
det är både craftat och registrerat som dungeon-drop i den aktuella
[Forever-itemkällan](https://www.wowhead.com/forever/item=252455).
Att ett item är craftat bevisar inte att dess dropdata är fel.
[Uteslutningsrapporten](data/forever-hybrids/excluded-records.json) behåller
det tidigare fyndet med korrigerad status och bevis.

Idol of Shifting Tides och Totem of Charged Flames har flyttats från
Meddlesome Mages till Aquatic Form respektive Call of Fire under betan.
Aktuella itemkällor visar båda fraktionernas class quests. Redan genomförda
quests kan hindra äldre betakaraktärer från att få belöningen; detta står i
anskaffningsvillkoren. Alla Druid-/Shamankataloger har även ett källbelagt
Enchanting-relicalternativ. Hela förquestkedjor och materialanskaffning har
inte spelats igenom i klienten. Katalogernas Partial-status anger preliminärt
betaunderlag, inte kvarvarande hål i de verifierade uppsättningarna.

## Import och researchverktyg

Nivåtak, stage, patch, klass/spec och catalogId är uttryckliga kontextfält.
Nivå 30 ometiketteras inte till nivå 60. Huvudtråden äger loader, UI,
kontextisolering, verifiering och commits.

tools/data/forever-hybrids/review-slot-supplements.py använder uttryckligt
nedladdat originalunderlag i en temporär mapp. Verktyget kontrollerar
same-spec-rekommendationer, metadata, faction, recipe och konkreta uppsättningar
innan --apply får skriva egna kataloger. Befintliga arbetsfiler säkerhetskopieras.
Fulla originalsidor och tooltips versionshanteras inte. Specialistbidraget
integrerades av huvudtråden; dokumentet reserverar inga nya agentuppdrag.

## Leveransstatus och ansvar inför beta 2

Hela Forever30-leveransen har **27 kataloger och 54 granskade fraktionsset**;
denna rapport täcker nio kataloger och 18 av vittnena. Samtliga **49 kontroller**,
inklusive tre DistributionScenarios, passerade mot den portabla 1.0.0-versionens
kärn-DLL:er. [Leveransrapporten](data/portable-release-verification.json)
visar även faktisk normal-UI-sparning/omstart och byte av programmapp.
PublishTrimmed är false och .NET/Windows App SDK följer med. Normal profil
är LocalAppData; en absolut BISTRACKER_DATA_DIRECTORY ger isolerad testlagring.

Ägande är ett boolvärde för ett exemplar per item-ID/suffix. Utrustning är
separat för tre specs och varje katalogset, medan ägande delas inom karaktären.
Schema1-JSON är bevarat. Gamla dubblerade varianter eller referenser till
uteslutna items ger fel med filen bevarad, utan tyst datarensning.

Researchunderlaget behöver ny uttrycklig källgranskning om betans itemdata
ändras. Huvudtråden äger beslut, loader/import, UI, integration och commits;
specgranskning får inte ensam ersätta befintliga katalog-ID:n eller reparera
sparade framsteg. Verklig nivå60-Launch-data finns ännu inte. Mängdmodell,
reparationsmigration och andra beta 2-funktioner är inte beslutade genom rapporten.
