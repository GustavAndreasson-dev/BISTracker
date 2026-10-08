# Classic fas 1: Mage, Priest och Warlock (casters)

Granskat 2026-10-08 för [REQ-013](requirements.md#appens-beta-2) enligt
[kontraktet för Classic fas 1-kataloger](classic-phase1-catalogs.md) och
[DEC-016](decisions.md#dec-016--scope-för-appens-beta-2). Åtta kataloger har
skrivits under `BISTracker.Infrastructure/Catalog/Data/Classic/phase1/`.
Holy Priest konverteras av en annan agent; föreslagna tillägg ligger i
`docs/data/classic-casters/priest-holy-additions.json` för huvudagenten.

Kontext: **WoW Classic (Vanilla), fas 1, nivå 60, pre-raid**. Tillåtet är
Dungeon (alla 5-mannadungeons och UBRS utom Dire Maul), Quest och Crafting
med fas 1-recept. Detta är guidernas namngivna alternativ efter filtrering,
inte en egen rangordning. Status: datan är granskad men **inte inläst av
appens Classic-läsare**, som skrivs parallellt.

## Källor per spec

| Spec | Rekommendationskällor | Status | Rader | Unika items | Dungeon / Quest / Crafting |
| --- | --- | --- | ---: | ---: | --- |
| Arcane Mage | [Icy Veins Mage DPS pre-raid](https://www.icy-veins.com/wow-classic/mage-dps-pre-raid-gear) fas 1–4, [Zockify Mage pre-BiS](https://www.zockify.com/classic/mage/dps/) | Partial | 38 | 33 | 26 / 8 / 4 |
| Fire Mage | Icy Veins Mage DPS pre-raid fas 5–6, filtrerat till fas 1-items | Reviewed | 36 | 31 | 26 / 8 / 2 |
| Frost Mage | Icy Veins fas 1–4, Zockify pre-BiS | Partial | 38 | 33 | 26 / 8 / 4 |
| Discipline Priest | [Icy Veins Priest Healer pre-raid](https://www.icy-veins.com/wow-classic/priest-healer-pre-raid-gear), [Warcraft Tavern/Defcamp Holy/Disc fas 1](https://www.warcrafttavern.com/wow-classic/guides/pre-raid-bis-for-holy-disc-priest-in-phase-1-pre-dm-patch-of-classic/) | Partial | 44 | 37 | 32 / 9 / 3 |
| Shadow Priest | [Icy Veins Priest DPS pre-raid](https://www.icy-veins.com/wow-classic/priest-dps-pre-raid-gear), [Zockify Shadow Priest pre-BiS](https://www.zockify.com/classic/priest/shadow/) | Partial | 37 | 32 | 26 / 6 / 5 |
| Affliction/Demonology/Destruction Warlock | [Icy Veins Warlock DPS pre-raid](https://www.icy-veins.com/wow-classic/warlock-dps-pre-raid-gear), [Warcraft Tavern/Zephan](https://www.warcrafttavern.com/wow-classic/guides/pve-warlock-pre-raid-best-in-slot/), [Zockify Warlock pre-BiS](https://www.zockify.com/classic/warlock/dps/) | Partial | 41 var | 36 var | 27 / 8 / 6 |

Rader räknar ringar och trinkets två gånger (Finger1/Finger2, Trinket1/Trinket2)
med samma item-ID, som i Forever-katalogerna.

Specfördelning som måste granskas av Chefen/huvudagenten:

- **Mage:** Ingen granskad Classic-guide har separata pre-raid-listor per spec.
  Icy Veins anger att fas 1–4-listorna betonar Frost eftersom raiderna spelas
  som Frost, och att fas 5–6 är Fire-listor. Frost använder fas 1–4 och
  Zockifys pre-BiS-lista (`of Frozen Wrath`). Fire använder fas 5–6-listorna
  filtrerade till items som finns i fas 1 (`of Fiery Wrath`). **Arcane har
  ingen egen källa**; katalogen återanvänder den klassgemensamma Frost-
  orienterade fas 1-listan och är därför markerad Partial. Det är ett
  källbeslut, inte en statvikt, men det är den största osäkerheten.
- **Warlock:** Alla tre källorna är klassgemensamma Warlock-listor utan
  specuppdelning. De tre katalogerna har därför samma items.
- **Discipline:** Defcamps lista gäller uttryckligen Holy eller Discipline.
  Seebus (Warcraft Tavern) gäller Holy och används bara för Holy-förslaget.

Uteslutna källor: noobtoboss.com (felaktiga item-ID:n och källor, t.ex. fel
ID för Hands of Power och Skyshroud Leggings), Wowheads Classic-guider
(innehållet renderas med skript och kunde inte läsas), Zockifys och Seebus'
raidfaslistor.

## Urval och filtrering

1. Guidernas HTML sparades lokalt och omvandlades till text med item-ID
   (`guide-to-text.py`). Alla rader med item-länk och rubrik ligger i
   `guide-rows-observed.json` (10 guider).
2. Kandidater (249 item-ID) togs från pre-raid-avsnitten. Metadata hämtades
   från Wowhead Classic tooltip-API (namn, ikon, slot, bindning, Unique,
   nivåkrav, klass) och ClassicDB (innehållsfas, droppar, quests, recept):
   `item-metadata.json`. Wowheads itemsidor lästes för 75 valda items i
   webbläsaren (droppar, questfraktion, recepts skill): `wowhead-item-sources.json`.
3. Uteslutet (175 kandidater, `excluded.json`): senare fas (131), world drops
   (22), Dire Maul inklusive mönster och bokquest (13), PvP/rykte/vendor,
   samt enskilda beslut:
   - Gloves of Spell Mastery: mönstret droppar bara från world bosses/raidbossar.
   - Mooncloth Boots: receptet kommer från Timbermaw-questen Sacred Cloth
     (ryktesgränsad), receptkällan är inte verifierad.
   - Shroud of the Nathrezim (Holy): droppkällan kunde inte verifieras.
   - Aristocratic Cuffs: guiderna kallar den world drop.
   - Cassandra's Grace, Maiden's Circle, Freezing Band, Master's/Eternal-items,
     Elemental Mage Staff m.fl.: world drops.
   ClassicDB markerar Dire Maul-items som fas 1; de utesluts ändå enligt kontraktet.
4. `selection.json` (skrivs av `make-selection.py`) innehåller anskaffnings-
   fakta per item och varje specs alternativ med rekommendationslänk och suffix.
   `check-guide-support.py` bekräftar att alla 298 alternativ finns i den
   citerade guidens tillåtna avsnitt och att suffixet står i guidens rad
   (`guide-support.json`, 0 problem).
5. `build-catalogs.py` skriver katalogerna; `validate-catalogs.py` kontrollerar
   kontraktet: rotfält, enum-namn, unika rad-ID:n, slot/vapenhand, Quest-
   fraktioner, Dungeon/Quest/Crafting, namn/nivå/Unique mot tooltip,
   ClassicDB-fas 1, BoP-crafting med yrkeskrav och fraktionsvisa set.
   Senaste `validation-report.json`: **PASS**. 66 ikon-URL:er gav HTTP 200
   (`icon-status.json`).

## Villkor i datan

- Slumpsuffix: Archivist Cape, Flameweave Cuffs, Tearfall Bracers, Drakestone,
  Green Lens, Atal'ai Gloves och Dire Nail kräver guidens suffix
  (`of Frozen Wrath`, `of Fiery Wrath`, `of Shadow Wrath`, `of Healing`).
- Crafting: källan anger yrke och receptets skill (Wowheads första färgvärde).
  BoP-robes (Robe of the Archmage, Robe of the Void, Truefaith Vestments)
  har "Requires Tailoring" och klassbegränsning i noten. Green Lens kräver
  Engineering (245) för att bäras och har slumpat suffix vid tillverkning.
- Quests med fraktion enligt Wowhead: Songstone of Ironforge (Alliance),
  Eye of Orgrimmar (Horde), Raincaster Drape (Horde). Eye of the Beast och
  Stormrager finns för båda via olika quests, vilket står i noten.
  In Dreams, It's Dangerous to Go Alone och Winterfall Activity är för båda.
  ClassicDB:s fraktionsfält var fel för For The Horde!; Wowhead används.
- Dungeon-trash som är BoE (Devout Bracers/Belt, Magister's Bindings,
  Spellshock Leggings) har tagits med som Dungeon eftersom de bara droppar i
  en dungeon. **Huvudagenten bör bekräfta** att det stämmer med Chefens
  policy mot BoE-items; annars tas de bort (påverkar inga fullständiga set
  utom Mage Legs-alternativ och Disc Wrist/Waist-alternativ, som har andra val).
- UBRS-rader har villkoret om grupp om upp till 10 och Seal of Ascension.

## Fraktionsset och luckor

| Katalog | Alliance | Horde | Lucka |
| --- | --- | --- | --- |
| Fire Mage | komplett | komplett | – |
| Arcane, Frost Mage | saknar Finger2 | saknar Finger2 | Endast Songstone/Eye of Orgrimmar är fas 1-dungeon/quest-ringar i guiderna |
| Shadow Priest | saknar Finger2 | saknar Finger2 | Samma ring-lucka |
| Warlock ×3 | saknar Finger2 | saknar Finger2 | Samma; Maiden's Circle och Underworld Band är world drops |
| Discipline Priest | saknar Head | saknar Head | Enda namngivna fas 1-huvudet är Cassandra's Grace (world drop) |

Luckorna står också i varje katalogs `selectionMethod` ("Known gap"). Inga
`slotExemptions` har lagts till: en lucka i en guide är inte ett styrkt
undantag. Ett set räknas komplett med enhand + offhand; inga tvåhandsvapen
återstår för Mage, Shadow eller Warlock (Elemental Mage Staff är world drop).

## Holy Priest-förslag

`priest-holy-additions.json` har 27 rader (24 items) som inte finns bland de
17 befintliga raderna: bland annat Truefaith Vestments (Tailoring, BoP),
Tooth of Gnarr, Heart of the Fiend, Hands of the Exalted Herald, Hands of
Power, Ghostweave Belt/Pants, Spiritshroud Leggings, The Postmaster's Treads,
Rosewine Circle, Songstone/Eye of Orgrimmar, Guiding Stave of Wisdom,
Spirit of Aquementas, Bonecreeper Stylus och Raincaster Drape (Horde).
Källor: Icy Veins Priest Healer, Defcamp och Seebus fas 1. Rad-ID:n följer
det nya kontraktet och kan ändras vid sammanslagning; `recommendationId`
saknas medvetet eftersom det bara gäller de 17 gamla raderna.

## Inte verifierat

- Ingen spelklientkontroll; drop-, quest- och receptuppgifter kommer från
  Wowhead och ClassicDB, inte från egen spelning.
- Wowhead blockerade (HTTP 403) vidare läsning efter 75 itemsidor; recept-
  källor bygger därför på ClassicDB. Wowheads guider kunde inte läsas.
- Fas 1-tillgänglighet bygger på ClassicDB:s fasmärkning plus manuell
  uteslutning av Dire Maul. Earth Warder's Gloves (item 21318, questen
  Winterfall Activity) är märkt fas 1 i ClassicDB och står i Icy Veins fas 1-
  lista men har ett högt item-ID; kontrollera om tveksamhet uppstår.
- Arcane Mage-mappningen (se ovan) är ett källbeslut som bör bekräftas.
- Katalogerna har inte lästs av appens Classic-läsare eller
  `CatalogReleaseAudit`; `dotnet build` validerar inte dessa filer ännu.

## Återskapning

Verktyg i `tools/data/classic-casters/` (kör med `python -I`):
`guide-to-text.py` → `extract-guide-rows.py` → `select-candidates.py` →
`fetch-item-metadata.py` (cache i en separat katalog) → `make-selection.py`
→ `check-guide-support.py` → `trim-metadata.py` → `build-catalogs.py` →
`validate-catalogs.py`, `check-icons.py` och `list-exclusions.py`.
`summarize-candidates.py` skriver en granskningsrad per kandidat och
`check-boss-loot.py` visade att ClassicDB:s bossidor saknar vissa bossdroppar
(`boss-loot-checks.json`), vilket är skälet till Wowhead-kontrollen.
