# Forever nivå 30: Mage, Priest och Warlock

Granskat 2026-10-08. Nio kataloger har skrivits under
`BISTracker.Infrastructure/Catalog/Data/Forever/level30/`.
Kontexten är **WoW Forever, beta, patch 1.60.1, level cap 30**.
Detta är guidernas namngivna utrustningsalternativ, inte en självberäknad
rangordning eller ett påstående om en färdig optimal uppsättning.

## Originalkällor och omfattning

Wowheads Forever-guider beskriver eget spel i betan och skiljer dungeon-drops,
questbelöningar och crafting. Varje katalog använder sin egen specguide.
En likadan rad i två specs har kontrollerats i båda guiderna; den har inte
kopierats mellan specs utifrån ett antagande om gemensamma statvikter.

| Spec och rekommendationskälla | Unika basitems | Placeringsrader | Rader från dungeons/quests |
| --- | ---: | ---: | ---: |
| [Arcane Mage](https://www.wowhead.com/forever/guide/classes/mage/arcane/level-30-dps-overview) | 94 | 106 | 77 |
| [Fire Mage](https://www.wowhead.com/forever/guide/classes/mage/fire/level-30-dps-overview) | 94 | 106 | 77 |
| [Frost Mage](https://www.wowhead.com/forever/guide/classes/mage/frost/level-30-dps-overview) | 99 | 111 | 77 |
| [Discipline Priest](https://www.wowhead.com/forever/guide/classes/priest/discipline/level-30-healer-overview) | 55 | 67 | 54 |
| [Holy Priest](https://www.wowhead.com/forever/guide/classes/priest/holy/level-30-healer-overview) | 55 | 67 | 54 |
| [Shadow Priest](https://www.wowhead.com/forever/guide/classes/priest/shadow/level-30-dps-overview) | 52 | 64 | 49 |
| [Affliction Warlock](https://www.wowhead.com/forever/guide/classes/warlock/affliction/level-30-dps-overview) | 95 | 104 | 73 |
| [Demonology Warlock](https://www.wowhead.com/forever/guide/classes/warlock/demonology/level-30-dps-overview) | 95 | 104 | 73 |
| [Destruction Warlock](https://www.wowhead.com/forever/guide/classes/warlock/destruction/level-30-dps-overview) | 95 | 104 | 73 |

Chefen har godkänt **dungeons, quests och crafting för Forever**. Alla 833
placeringsrader ingår i den policyn; dungeons/quests-kolumnen visar delmängden
som även ger kompletta utrustningskombinationer utan professionskrav.
Mage-guiderna rekommenderar uttryckligen Nether Force Wand,
Ragefire Wand respektive Icefury Wand från Mage's Wand-kedjan; dessa har
tagits med för rätt spec. Kedjan börjar på level 30 och kräver Scarlet
Monastery: Library, vilket dokumenteras som ett svårt mål vid betans nivåtak.

Priest-tabellerna saknar quest-wands. De tre ursprungliga Mobalytics-guiderna
för [Discipline](https://mobalytics.gg/wow-forever/classes/discipline-priest-guide),
[Holy](https://mobalytics.gg/wow-forever/classes/holy-priest-guide) och
[Shadow](https://mobalytics.gg/wow-forever/classes/shadow-priest-guide)
rekommenderar uttryckligen Gravestone Scepter i respektive level 30-betaavsnitt.
Den raden kompletterar varje Priest-katalog med separat rekommendationslänk.
Forever-itemposten och båda Blackfathom Villainy-questposterna bekräftar item
7001, Ranged, Alliance/Horde-belöning och questens miniminivå 18.

## Kompletta kombinationer och korrigerade källluckor

Den första kontrollen räknade namngivna slotar. Det bevisade inte att två
olika trinkets kunde väljas samtidigt. Alla nio specs hade bara ett distinkt
dungeon/quest-trinket och Priest saknade Horde-val för Hands/Waist.
Granskningen kompletterar därför rekommendationerna med 24 placeringsrader:

- Worgenbane Talisman (273643), Commander Springvale i Shadowfang Keep,
  ger ett andra distinkt trinket i samtliga nio specs. Arcane/Fire och
  Holy/Discipline har uttryckliga rekommendationer i sina egna Icy Veins-
  nivå 30-guider; de övriga fem har itemet i sina egna WOWTBC Forever-listor.
- Belt of Arugal (6392) fyller Priest Waist för båda fraktionerna. Holy/
  Discipline rekommenderar det i egna Icy Veins-guider och Shadow i egen
  WOWTBC Forever-lista.
- Tattered Mittens (270030) från The Book of Ur, Horde, minimum level 16,
  fyller Holy/Discipline Hands. Guidernas platsangivelse Hall of Thanes
  korrigeras till Shadowfang Keep med questpostens anskaffningsbevis.
  Shadow får Jutebraid Gloves (10654) från Horde Presence, minimum level 15,
  som uttryckligen finns i dess egen WOWTBC Forever-lista.

[WOWTBC Forever](https://wowtbc.gg/warcraftforever/bis-list/shadow-priest/)
beskriver listorna som alternativa uppgraderingar per slot utan inbördes
ordning. Sidornas observerade `page-data` innehåller även alternativ som
döljs i den korta tabellen. Exakta URL:er, spec, slot, item-ID, namn och
anskaffning sparas i `slot-supplement-sources.json`; varje produktitem har
sin egen `recommendationUrl`. Ingen Classic-lista, egen statvikt eller
likvärdighetsgrupp har använts för kompletteringarna.

`slot-audit.json` innehåller **18 separata lagliga exempelset**, ett för varje
spec och fraktion. Varje set har 17 faktiska positioner, två olika ringar,
två olika trinkets, enhand plus offhand och inga konkurrerande belöningsval
från samma quest. `missingSlots` och `exemptSlots` är tomma; samtliga
`fullSetValidated` är sanna. Exemplen väljs genom en deterministisk sökning
på item-ID för att visa möjligheten, inte för att rangordna utrustningen.
Alla exempel klarar sig med dungeons/quests och kräver därför inga
professioner; craftingalternativen innebär ingen obligatorisk konflikt.

## Kontroller och viktiga villkor

- Item-ID, engelskt namn, slot och nivåkrav kommer från Forever-guidens
  itemreferenser och dess itemmetadata. Därefter har 165 olika Forever-itemposter
  lästs individuellt. Item- och ikonlänkar observerades på dessa sidor.
  Inga Vanilla-/SoD-tabeller används som rekommendationsbevis.
- 104 individuella Forever-questposter, inklusive publicerade förquests och
  gemensamt forskningsunderlag till andra klassgrupper, har kontrollerats för
  miniminivå, fraktion och belöningar. Ingen har miniminivå över 30.
  Ett lågt item-nivåkrav har alltså inte ensamt använts som questtillgänglighet.
- 110 olika observerade ikon-URL:er gav HTTP 200 vid HEAD-kontroll.
  Hämtbarhet ersätter inte verifiering i en spelklient.
- Ett betaitem kan ha ett gammalt numeriskt ID men ändrade egenskaper.
  Forever-namnrymden används därför konsekvent även för äldre ID:n.
- Tvåhandsvapen placeras i MainHand med `weaponKind: TwoHanded` och ersätter
  OffHand. Guidernas alternativ med enhand och off-hand ligger kvar som
  alternativa kombinationer. Ranged-vapen är wands i de här nio katalogerna.
- Ringar och trinkets finns som alternativ i båda visningspositionerna, med
  olika rekommendations-ID men samma fysiska item-ID. Det rekommenderar inte
  två exemplar. `uniqueEquipped` är sann bara när den individuella Forever-
  tooltipen anger Unique eller Unique-Equipped. Exempel är Dark Horde Band,
  Electrocutioner Lagnut, Agamaggan's Clutch och Roogug's Severed Head.
- Alla Quest-rader har verifierad `availableFactions`. Questmetadata och
  publicerade kedjor används tillsammans med guidens uttryckliga fraktions-
  uppgift när questposten saknar numerisk fraktionskod. Road of Confidence
  (281265) korrigeras från guidens Both-tabell till Horde enligt Watching the
  Roads. Knife of Polishing (271740) använder uttryckligt Horde-villkor från
  originalguidens The Open Maw och individuell questpost för nivå/belöning.
  Magistrate's Pantaloons (270036) har uteslutits ur samtliga tre Priest-
  kataloger: guidens/itemets questlänk och The Fury Runs Deeps aktuella
  belöningstabell ger motstridiga item-ID:n. Fullseten behöver inte itemet.
  Appen behöver fortsatt visa fraktionsvillkoren.
- Bind on pickup och verifierade professionskrav följer med i villkoren för
  crafting. Crafter-only-varianter ska inte tolkas som köpbara BoE-items.
- Alla 68 distinkta craftingalternativ som finns kvar har individuellt
  verifierade Forever-recept med skillkrav mellan 50 och 200. Receptbevis
  sparas i `crafting-metadata.json`; varje rad visar receptets profession och
  skillkrav. [Tailoring](https://www.icy-veins.com/wow-forever/tailoring),
  [Enchanting](https://www.icy-veins.com/wow-forever/enchanting),
  [Engineering](https://www.icy-veins.com/wow-forever/engineering) och
  [Leatherworking](https://www.icy-veins.com/wow-forever/leatherworking)
  har Expert-cap 225 från karaktärsnivå 20. BoP kräver egen tillverkning;
  aktivt professionskrav för användning redovisas separat från receptkravet.
  Boots of Darkness (7027) utesluts ur Warlocks tre kataloger: Forever-posten
  hänvisar till ett recept men saknar verifierbar skilluppgift. Classic-
  receptregler används inte för att fylla den luckan. Recept-/materialanskaffning
  har inte provspelats; guidernas uttryckliga Merchant's Favor-villkor kvarstår.
- Consumables, oljor och recept utan utrustningsslot har inte importerats.
  Green Lens och Spellpower Goggles Xtreme Plus har uteslutits: deras
  tooltips kräver Engineering 245 respektive 270. Den ursprungliga
  [Forever Engineering-guiden](https://www.icy-veins.com/wow-forever/engineering)
  anger Artisan först på level 35 och Expert som skill cap 225.
  Green Lens har dessutom en bonusvariant som inte får gissas till suffix.
- Icy Veins äldre Holy/Discipline-guide angav att Healer's Staff var låst
  av en questbugg. De nyare Wowhead-guiderna inkluderar den. På
  [Earthen Echo](https://www.wowhead.com/forever/quest=98823/earthen-echo)
  finns direkta spelarrapporter om rättningen 2026-10-04. Itemet utesluts
  ur alla nio aktiva caster-kataloger tills Alliance-vägen också har
  verifierats. Ingen egen spelklientkontroll har utförts. Båda osäkra
  questitems bevaras med originalrader, metadata och källkonflikter i
  `excluded-research.json`; regenerering och validering blockerar dem.

## Återskapning och granskningsspår

Forskningsfilerna ligger under `docs/data/forever-casters/` och verktygen under
`tools/data/forever-casters/`:

1. `review-casters.py` läser de nio offentliga guiderna via PowerShell och
   sparar enbart tabellernas item- och anskaffningsfakta i `*-observed.json`.
2. `review-item-metadata.py` läser individuella Forever-itemposter och sparar
   identitet, verklig ikon-URL, Unique-villkor, bindning och professionskrav
   i `item-metadata.json`. Ny hämtning hoppar över redan sparade poster.
3. `write-catalogs.py` validerar identitet/slot, utesluter ogiltiga mål och
   skriver kataloger plus tydliga `*-excluded.json`-listor. Befintliga caster-
   kataloger bevaras om inte `--regenerate-reviewed-catalogs` uttryckligen ges.
4. `review-quests.py` sparar individuella questers miniminivå, fraktion,
   garanterade/valbara belöningar och publicerade förquests i `quest-metadata.json`.
   `supplemental-recommendations.json` innehåller Priest-wands och nya slot-
   kompletteringar. `review-slot-supplements.py` och `apply-slot-supplements.py`
   samlar kompletteringsbevis; nivå 20-spår är enbart research och importeras
   inte i produktlistorna. `icon-status.json`
   bevarar HTTP-kontrollen.
5. `review-crafting.py` läser individuella Forever-itemposters skapande-
   recept och skillkrav. `audit-slots.py --annotate-reviewed-factions`
   annoterar questfraktioner/receptvillkor och skriver `slot-audit.json`.
   Detta steg måste köras efter en uttrycklig regenerering av katalogerna.
6. `validate-catalogs.py` jämför katalogerna med de separat insamlade fakta-
   och HTTP-filerna. Senaste `validation-report.json` är PASS: nio kataloger,
   833 placeringsrader, 68 verifierade craftingalternativ och 18 kompletta
   fraktionsspecifika exempelset. Varje Quest-rads verifierade fraktion,
   receptens skillkrav och exempelsetens distinkta item-ID:n kontrolleras.

Guiderna kan ändras. Ett nytt forskningsprov behöver granskas innan dess
utdata blir produktkatalog. Programstart hämtar aldrig guidetabeller.
`schemaVersion`, `catalogId`, version, klass, spec, level cap, release stage,
patch, rekommendationskälla, granskningsdatum och urvalsmetod är uttryckliga
rotfält. Därmed kan level 60 få egna kataloger och villkor utan att skriva
över level 30-källspåret eller återanvända dess rekommendations-ID:n.

Datagranskningen fastställer riktiga guidealternativ. Huvudtråden verifierar
loader, urvalsfilter, domänregler, UI, sparning och commits separat.
