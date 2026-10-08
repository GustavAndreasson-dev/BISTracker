# WoW Forever och tre speclistor per karaktär

Statusdatum: 2026-10-08. Genomförbarhetsbedömningen följdes av implementation:
alla nio klasser/27 specs har aktiva Forever-kataloger för beta, nivå 30,
patch 1.60.1. De innehåller källbelagda guidealternativ med verifierade kompletta fraktionsset;
verkliga nivå 60-listor saknas. Nivå 60-import, katalogbyte och tre sparade
speclistor per katalogset är implementerade och verifierade.
Se [nivå 30-katalogerna](forever-level30-catalogs.md),
[katalogset och import](catalog-contexts.md) och
[kod, beteenden och verifiering](characters-and-loadouts.md).

## Roll inför beta 2

Starta i [beta2-handoff.md](beta2-handoff.md). Den här filen bevarar
genomförbarhetsbedömningen och hur dess beställda funktioner blev
implementation. Den är inte en ny lanserings- eller itemdatagranskning.
Chefens produktfunktioner för beta 2 är ännu inte beslutade.

Portabel **1.0.0** är verifierad med 49 beteendekontroller och
produktions-UI/sparning på utvecklingsdatorns Windows 11. Kod/runtime
kommer från `e25c134`; `043d859` dokumenterar ZIP-leveransen. Se
[leveransrapporten](data/portable-release-verification.json).
Ren dator, Windows 10 och verklig nivå 60-data är inte testade.
Behåll de 27 Forever nivå 30-katalogernas 54 verifierade fraktionsset,
delat ägande och separata spec-/nivåuppsättningar tills Chefen beslutar annat.

## Beställt scope

Chefen har bett att först föra över de saknade Boromir-ändringarna, därefter
undersöka Forever, vid genomförbarhet utöka till alla klasser/specs och möjliggöra
sammanlagt tre separata speclistor kopplade till samma karaktär.
Chefen har bekräftat att Forever avser Blizzards officiella produkt och
beställde därefter nivå 30-listor för alla specs, med importberedskap för
nivå 60 när källgranskad data blir tillgänglig.

Följande krav-ID:n finns i requirements.md efter integration av Boromirs
dokumentation. Tabellen skiljer verifierad funktion och nivå 30-underlag från
återstående verklig nivå 60-data.

| ID | Krav | Status |
| --- | --- | --- |
| REQ-005 | Stöd WoW Forever som separat valbar spelversion, med egen katalogkontext. | Implementerat/verifierat: separat beta/nivå 30 och förberedd nivå 60-import. |
| REQ-006 | Utöka till alla klasser och specs om Forever-stöd är genomförbart. | Nio klasser/27 specs med aktiva nivå 30-kataloger och 54 verifierade fraktionsset enligt REQ-010. |
| REQ-007 | Tre bestående, separata listor per karaktär, en per spec. | Implementerat/verifierat: gemensamt ägande, tre separata listor per katalogset och bevarade arkiverade nivåer. |

Överlämningens Classic-scope är fas 1 pre-raid, dungeons och quests.
Forever nivå 30 är ett separat beta-scope och visas som guidealternativ.
Normal Forever-app filtrerar till **dungeons, quests och crafting** enligt
Chefens senare godkännande (DEC-012); den tidigare D/Q-avgränsningen är
historisk för Forever. Övriga källtyper filtreras bort. Classic behåller
endast dungeons/quests. Lokal JSON används fortfarande; ingen databas har införts.

## Källbelagda fynd vid första granskningen 2026-10-08

Följande historiska fynd bevaras som beslutsunderlag. De är inte en ny kontroll
av lanseringsstatus eller av komplett nivå 60-itemdata.

- [Blizzards produktsida](https://worldofwarcraft.blizzard.com/en-us/forever)
  beskriver en separat spelupplevelse med klass- och specförändringar samt
  lansering 4 november 2026. Direktläsningen gav HTTP 403; officiellt indexerat
  sidinnehåll kunde läsas via sökverktyget. Lanseringsuppgiften är inte ett
  bevis för färdig itemdata.
- [Create the Hero You Want to Be](https://news.blizzard.com/en-us/article/24304075/create-the-hero-you-want-to-be-in-world-of-warcraft-forever)
  anger nio klasser: Druid, Hunter, Mage, Paladin, Priest, Rogue, Shaman,
  Warlock och Warrior. Race/class-tabellerna skiljer sig från Vanilla.
- [Deep Dive Panel Recap](https://news.blizzard.com/en-us/article/24303313/world-of-warcraft-forever-deep-dive-panel-recap)
  beskriver ändrade stats, omgranskade dungeon-drops, nya drops och utökade
  questbelöningar. Slutsats: Vanilla-katalogen kan inte användas som verifierad
  Forever-BiS. Artikeln beskriver också rulesets i stället för traditionellt
  realmval; ett lokalt karaktärs-ID bör därför inte kräva ett realmnamn.
- [Priest and Warrior Class Deep Dives](https://news.blizzard.com/en-gb/article/24301514/world-of-warcraft-forever-class-deep-dives-priest-and-warrior)
  bekräftar Priest-specs Discipline, Holy och Shadow samt Warrior-specs Arms,
  Fury och Protection. Blizzard anger att beta-talanger kan ändras och att
  Dual Specialization blir tillgänglig på level 40. Tre listor i trackerappen
  är ett eget planeringsbehov; de innebär inte tre samtidiga talentbyggen i spelet.

Vid första granskningen stödde källorna spelversion, klasser och förändrade
regler, men fastställde inte kompletta pre-raid-rankningar för alla specs.
[Icy Veins Forever-guide och specnavigation](https://www.icy-veins.com/wow-forever/fire-mage-ranged-dps-pve-guide/)
kontrollerades då för samtliga 27 specnamn. Gearsektionen avsåg nivå 30 i betan
och importerades inte som en nivå 60 pre-raid-katalog.

Efter Chefens nivå 30-beställning granskades ursprungliga nivå 30-guider och
Forever-itemmetadata. Samtliga 27 spec-kataloger har nu integrerats med
källreferenser och uttrycklig beta/nivå/patch-kontext. Luckor och ospecificerade
villkor dokumenteras, och saknade rekommendationer fylls inte med Vanilla-
eller SoD-data. Se [källurval och begränsningar](forever-level30-catalogs.md).

## Bedömning och föreslagna gränser

Alla tre funktioner är tekniskt möjliga i nuvarande DDD/SOLID-struktur.
Den stora datarisken är kvalitet och tillgänglighet för Forever-rekommendationer,
inte att WinUI eller JSON skulle förhindra fler listor.

Ursprungliga förslag före implementation, bevarade för spårbarhet. Faktiskt
genomförande och kvarstående gränser redovisas i characters-and-loadouts.md:

1. Katalogen identifieras med stabil spelversions-, klass- och specidentitet,
   samt uttrycklig build/patch, tillgänglighetskontext och urvalsgrund.
   Visningsnamn används inte som lagringsnyckel. Beta och launch skiljs åt.
2. En karaktär har ett stabilt lokalt ID och en klass i en bestämd spelversion.
   Tre speclistor hör till den karaktären. En spec kan inte tillhöra fel klass.
3. Varje lista behåller sin rekommendation, utrustningsuppsättning och sammanfattning
   när en annan spec väljs. Byte av spec ska inte skriva över någon annan lista.
4. Föreslaget ägande är gemensamt på karaktären för samma verkliga itemidentitet;
   utrustningsmarkeringar hör till respektive speclista. Detta har nu bekräftats
   av Chefen och implementerats. En utrustningsmarkering betyder att
   itemet används i listans sparade specuppsättning.
5. Itemidentitet skiljs från rekommendationsidentitet. Samma item kan rekommenderas
   i flera specs eller slots. Ringar/trinkets, två exemplar, unique-regler,
   tvåhandsvapen och dual wield måste hanteras efter klassens verifierade regler.
6. Framsteg isoleras mellan karaktärer och spelversioner. Gamla demo- och
   Holy Priest-framsteg bevaras. Eventuell migration måste vara versionsstyrd,
   kontrollerad och verifierad före användning av riktiga framstegsfiler.
7. UI behåller engelska etiketter, med korta val för version, karaktär och spec.
   Ingen extra instruktionstext införs för självklara interaktioner.

Punkt 1–4 och 6–7 är genomförda för det beställda nivå 30-scopet och importvägen.
Ägande delas inom karaktären, medan utrustning sparas separat för varje spec
och katalogset. Sparfel bevarar aktiva och arkiverade listor.

Punkt 5 har genomförda unique- och tvåhands/offhand-regler samt ett slotmål
med grupperade alternativ enligt REQ-010. Unique-equipped
begränsas efter basitem-ID oavsett suffix; ägande gäller fortfarande varje
item-ID/suffix-variant. Katalogernas handplaceringar bygger på verifierat
klass/spec-underlag. Antal exemplar av ett item modelleras inte.
54 kompletta lagliga fraktionsset är verifierade mot aktiva nivå 30-items;
detta bevisar genomförbarhet, inte optimal ranking eller lika starka alternativ.
Verkliga nivå 60-rekommendationer återstår.

## Implementation efter överföring

- Domain: spel/klass/spec- och katalogset-identitet, gemensamt variantägande
  samt unique- och tvåhands/offhand-regler.
- Application: karaktär/spec/set-val, tracking och bevarade arkiverade listor.
- Infrastructure: 27 nivå 30-kataloger, validerad import och säker JSON-lagring.
- Presentation: karaktär/spec/set-val, grupperade slotmål och katalogimport.
- Checks: 49 godkända konsolkontroller för bland annat kataloger, källpolicy,
  nivå 60-import, nivåisolering, sparfel, utrustningsregler och legacy-migration.
- Isolerad WinUI-kontroll: verkliga Classic- och Forever nivå 30-listor,
  nivåbyte och import med testdata utan att ändra spelarens framsteg.
- Docs: kravspårning, beslut, strukturkarta, datakällor och faktisk verifiering.

Acceptanskriterier: tre listor finns kvar efter omstart, specbyte bevarar dem,
fel klass/spec avvisas, karaktärer/versioner blandas inte och gamla framsteg
bevaras. Varje publicerad katalog måste ha verifierade items och källor för sin
uttryckliga spelkontext. Release-build, 49 konsolkontroller och isolerad
UI-verifiering är godkända för den aktuella leveransen. Importtesten använder
fiktiv nivå 60-data, inte en publicerad nivå 60-katalog. Se
[verifieringen](verification.md).

Den portabla normalappen har dessutom testats med isolerad diskbaserad
profil, faktiska checkboxklick, sparning/omstart och byte av programmapp.
Spelarens normala framsteg har inte ändrats av verifieringen. Det är ett
genomfört Windows 11-distributionstest, inte bevis för alla målmaskiner.

## Överföringsstatus

Boromirs originaländringar överfördes 2026-10-08. Lokal main fast-forwardades
vid överföringen till 8bf43b7; detta är överföringens revision, inte aktuellt
HEAD efter fortsatt utveckling. Appen fick då den riktiga 17-item-katalogen.
Git-bundlens checksumma, Release-build, 17 dåvarande beteendekontroller och
isolerad UI-verifiering kontrollerades lokalt.
Se [den historiska överföringsrapporten](local-transfer.md).

Boromir behövs inte för fortsatt implementation. REQ-005–007 har nu den
verifierade leverans som beskrivs ovan: listfunktion, val och alla 27 nivå 30-
kataloger är integrerade. Kompletta nivå 60-kataloger återstår.
Nya krav-ID:n börjar på REQ-005 eftersom Boromirs befintliga REQ-003 och REQ-004
redan gäller urvalsavgränsning respektive engelskt gränssnitt.
