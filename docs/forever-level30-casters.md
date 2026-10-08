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
| [Arcane Mage](https://www.wowhead.com/forever/guide/classes/mage/arcane/level-30-dps-overview) | 94 | 105 | 76 |
| [Fire Mage](https://www.wowhead.com/forever/guide/classes/mage/fire/level-30-dps-overview) | 94 | 105 | 76 |
| [Frost Mage](https://www.wowhead.com/forever/guide/classes/mage/frost/level-30-dps-overview) | 99 | 110 | 76 |
| [Discipline Priest](https://www.wowhead.com/forever/guide/classes/priest/discipline/level-30-healer-overview) | 54 | 65 | 52 |
| [Holy Priest](https://www.wowhead.com/forever/guide/classes/priest/holy/level-30-healer-overview) | 54 | 65 | 52 |
| [Shadow Priest](https://www.wowhead.com/forever/guide/classes/priest/shadow/level-30-dps-overview) | 51 | 62 | 47 |
| [Affliction Warlock](https://www.wowhead.com/forever/guide/classes/warlock/affliction/level-30-dps-overview) | 96 | 104 | 72 |
| [Demonology Warlock](https://www.wowhead.com/forever/guide/classes/warlock/demonology/level-30-dps-overview) | 96 | 104 | 72 |
| [Destruction Warlock](https://www.wowhead.com/forever/guide/classes/warlock/destruction/level-30-dps-overview) | 96 | 104 | 72 |

Crafting finns i forskningskatalogerna för att urvalspolicyn ska kunna filtreras
utan ny scraping. Kolumnen dungeons/quests redovisar den tidigare beslutade
avgränsningen. Mage-guiderna rekommenderar uttryckligen Nether Force Wand,
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
7001, Ranged, Alliance/Horde-belöning och questens miniminivå 18. Alla nio
casterkataloger har därmed alternativ för samtliga 17 utrustningspositioner
även när bara dungeons och quests väljs.

## Kontroller och viktiga villkor

- Item-ID, engelskt namn, slot och nivåkrav kommer från Forever-guidens
  itemreferenser och dess itemmetadata. Därefter har 162 olika Forever-itemposter
  lästs individuellt. Item- och ikonlänkar observerades på dessa sidor.
  Inga Vanilla-/SoD-tabeller används som rekommendationsbevis.
- 46 olika refererade Forever-quests har kontrollerats på sina individuella
  sidor för miniminivå, fraktion och belöningar. Ingen har miniminivå över 30.
  Ett lågt item-nivåkrav har alltså inte ensamt använts som questtillgänglighet.
- 109 olika observerade ikon-URL:er gav HTTP 200 vid HEAD-kontroll.
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
- Faction anges där guiden eller dess questmetadata uttryckligen anger
  Alliance/Horde. Gemensamma questbelöningar slås samman till en itemrad;
  exempel är Healer's Staff från Prehistoric Prism respektive Earthen Echo.
  Appen behöver fortsatt visa dessa villkor; den känner inte karaktärens race.
- Bind on pickup och verifierade professionskrav följer med i villkoren för
  crafting. Crafter-only-varianter ska inte tolkas som köpbara BoE-items.
- Consumables, oljor och recept utan utrustningsslot har inte importerats.
  Green Lens och Spellpower Goggles Xtreme Plus har uteslutits: deras
  tooltips kräver Engineering 245 respektive 270. Den ursprungliga
  [Forever Engineering-guiden](https://www.icy-veins.com/wow-forever/engineering)
  anger Artisan först på level 35 och Expert som skill cap 225.
  Green Lens har dessutom en bonusvariant som inte får gissas till suffix.
- Icy Veins äldre Holy/Discipline-guide angav att Healer's Staff var låst
  av en questbugg. De nyare Wowhead-guiderna inkluderar den. På
  [Earthen Echo](https://www.wowhead.com/forever/quest=98823/earthen-echo)
  finns direkta spelarrapporter om rättningen 2026-10-04. Katalograden
  behåller en daterad betabuggsnotering; någon egen spelklientkontroll har
  inte utförts, och Alliance-vägens rättning har inte separat bekräftats.

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
4. `review-quests.py` sparar individuella questers miniminivå och fraktion
   i `quest-metadata.json`. `supplemental-recommendations.json` innehåller
   de tre källbelagda Priest-wandrekommendationerna. `icon-status.json`
   bevarar HTTP-kontrollen.
5. `validate-catalogs.py` jämför katalogerna med de separat insamlade fakta-
   och HTTP-filerna. Senaste `validation-report.json` är PASS: nio kataloger,
   824 placeringsrader och 17 dungeon/quest-slotar per spec.

Guiderna kan ändras. Ett nytt forskningsprov behöver granskas innan dess
utdata blir produktkatalog. Programstart hämtar aldrig guidetabeller.
`schemaVersion`, `catalogId`, version, klass, spec, level cap, release stage,
patch, rekommendationskälla, granskningsdatum och urvalsmetod är uttryckliga
rotfält. Därmed kan level 60 få egna kataloger och villkor utan att skriva
över level 30-källspåret eller återanvända dess rekommendations-ID:n.

Datagranskningen fastställer riktiga guidealternativ. Huvudtråden verifierar
loader, urvalsfilter, domänregler, UI, sparning och commits separat.
