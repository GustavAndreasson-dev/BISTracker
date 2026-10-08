# WoW Forever och tre speclistor per karaktär

Granskningsdatum: 2026-10-08. Genomförbarhetsbedömning följd av implementation.
Version/klass/spec och tre sparade listor är nu implementerade; Forever-BiS-kataloger
återstår. Se [kod, beteenden och verifiering](characters-and-loadouts.md).

## Beställt scope

Användaren har bett att först föra över de saknade Boromir-ändringarna, därefter
undersöka Forever, vid genomförbarhet utöka till alla klasser/specs och möjliggöra
sammanlagt tre separata speclistor kopplade till samma karaktär.
Användaren har bekräftat att Forever avser Blizzards officiella produkt.

Följande krav-ID:n finns i requirements.md efter integration av Boromirs
dokumentation. Tabellen skiljer verifierad funktion från återstående itemdata.

| ID | Krav | Status |
| --- | --- | --- |
| REQ-005 | Stöd WoW Forever som separat valbar spelversion, med egen katalogkontext. | Versionsval implementerat/verifierat; katalogdata återstår. |
| REQ-006 | Utöka till alla klasser och specs om Forever-stöd är genomförbart. | Nio klasser/27 specs valbara; nya källgranskade listor återstår. |
| REQ-007 | Tre bestående, separata listor per karaktär, en per spec. | Gemensamt ägande och separat utrustning bekräftat; listfunktionen implementerad/verifierad. |

Befintlig avgränsning från överlämningen är pre-raid, dungeons och quests.
Detta är arbetsantagandet även för nya listor tills annat beslutas.
Ingen ny databas föreslås; lokal JSON är fortsatt utgångspunkt.

## Källbelagda fynd

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

Källorna stödjer spelversion, klasser och förändrade regler. De fastställer inte
kompletta pre-raid-rankningar för alla specs. Ingen sådan katalog har verifierats.
[Icy Veins Forever-guide och specnavigation](https://www.icy-veins.com/wow-forever/fire-mage-ranged-dps-pve-guide/)
har därefter kontrollerats för samtliga 27 specnamn. Gearsektionen avser nivå 30
i betan. Dessa rekommendationer har inte importerats som skarp pre-raid-katalog.

## Bedömning och föreslagna gränser

Alla tre funktioner är tekniskt möjliga i nuvarande DDD/SOLID-struktur.
Den stora datarisken är kvalitet och tillgänglighet för Forever-rekommendationer,
inte att WinUI eller JSON skulle förhindra fler listor.

Ursprungliga förslag före implementation (genomförande och kvarstående gränser
redovisas i characters-and-loadouts.md):

1. Katalogen identifieras med stabil spelversions-, klass- och specidentitet,
   samt uttrycklig build/patch, tillgänglighetskontext och urvalsgrund.
   Visningsnamn används inte som lagringsnyckel. Beta och launch skiljs åt.
2. En karaktär har ett stabilt lokalt ID och en klass i en bestämd spelversion.
   Tre speclistor hör till den karaktären. En spec kan inte tillhöra fel klass.
3. Varje lista behåller sin rekommendation, utrustningsuppsättning och sammanfattning
   när en annan spec väljs. Byte av spec ska inte skriva över någon annan lista.
4. Föreslaget ägande är gemensamt på karaktären för samma verkliga itemidentitet;
   utrustningsmarkeringar hör till respektive speclista. Detta har nu bekräftats
   av användaren och implementerats. En utrustningsmarkering betyder att
   itemet används i listans sparade specuppsättning.
5. Itemidentitet skiljs från rekommendationsidentitet. Samma item kan rekommenderas
   i flera specs eller slots. Ringar/trinkets, två exemplar, unique-regler,
   tvåhandsvapen och dual wield måste hanteras efter klassens verifierade regler.
6. Framsteg isoleras mellan karaktärer och spelversioner. Gamla demo- och
   Holy Priest-framsteg bevaras. Eventuell migration måste vara versionsstyrd,
   kontrollerad och verifierad före användning av riktiga framstegsfiler.
7. UI behåller engelska etiketter, med korta val för version, karaktär och spec.
   Ingen extra instruktionstext införs för självklara interaktioner.

Punkt 2–4 och 6–7 har genomförts inom beställt scope. Katalogmetadata och
utökade utrustningsregler i punkt 1 och 5 återstår inför nya items.
Fullständiga BiS-listor får inte fyllas med gissningar för att täcka alla specs.

## Implementation efter överföring

- Domain: spel/klass/spec-identitet, karaktär och regler för tre listor.
- Application: val av katalog och karaktär samt användningsfall för specbyte och tracking.
- Infrastructure: separata källgranskade kataloger och säker lagring av karaktärens listor.
- Presentation: version/klass/karaktär/spec-val och befintlig utrustningslista.
- Checks: klass/spec-validering, listisolering, gemensamt ägande om beslutat,
  utrustningskombinationer, återläsning, sparfel och bevarade äldre framsteg.
- Docs: kravspårning, beslut, strukturkarta, datakällor och faktisk verifiering.

Acceptanskriterier: tre listor finns kvar efter omstart, specbyte bevarar dem,
fel klass/spec avvisas, karaktärer/versioner blandas inte och gamla framsteg
bevaras. Varje publicerad katalog måste ha verifierade items och källor för sin
uttryckliga spelkontext. Kör relevant dotnet build och beteendekontroller samt
renderad UI-granskning efter implementation.

## Överföringsstatus

Boromirs originaländringar är överförda 2026-10-08. Lokal main är på 8bf43b7
och appen använder den riktiga 17-item-katalogen. Git-bundlens checksumma,
Release-build, 17 beteendekontroller och isolerad UI-verifiering är kontrollerade
lokalt. Se [överföringsrapporten](local-transfer.md).

Boromir behövs inte för fortsatt implementation. REQ-005–007 har påbörjats enligt
leveransen ovan: listfunktion och val är klara, nya BiS-kataloger återstår.
Nya krav-ID:n börjar på REQ-005 eftersom Boromirs befintliga REQ-003 och REQ-004
redan gäller urvalsavgränsning respektive engelskt gränssnitt.
