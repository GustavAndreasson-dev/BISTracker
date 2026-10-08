# Forever nivå 30: Hunter, Rogue och Warrior

Granskat 2026-10-08. Chefens nivå 30-beställning gäller Forever-betan,
patch 1.60.1. Nio separata JSON-kataloger har skapats med källbelagda
utrustningsalternativ. De är nu markerade **Reviewed**: samtliga relevanta
slots har källbelagda alternativ och en separat, möjlig uppsättning har
verifierats för Alliance och Horde. Ingen egen ranking eller likvärdighet
mellan alternativen har fastställts. De är inte nivå 60-listor.

Chefens senare beslut samma dag tillåter **Dungeon, Quest och Crafting** i
Forever. World drops ingår fortfarande inte i den aktiva policyn. Se även
[samlad katalogöversikt](forever-level30-catalogs.md) och
[katalogkontexter](catalog-contexts.md).

## Ursprungliga rekommendationskällor

Wowheads författade Forever-guider har både konkreta itemlänkar och
Forever-itemmetadata. Katalogerna följer deras utrustningstabeller och
uttryckliga klassquest-rekommendationer. Antalet placeringar inkluderar
alternativa giltiga slots; det anger inte antalet items som ska farmas.

| Klass/spec | Namngivna guideitems | Placeringar | Ursprunglig guide |
| --- | ---: | ---: | --- |
| Hunter / Beast Mastery | 30 | 37 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/beast-mastery/level-30-dps-overview) |
| Hunter / Marksmanship | 32 | 39 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/marksmanship/level-30-dps-overview) |
| Hunter / Survival | 36 | 44 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/survival/level-30-dps-overview) |
| Rogue / Assassination | 30 | 41 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/assassination/level-30-dps-overview) |
| Rogue / Combat | 36 | 52 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/combat/level-30-dps-overview) |
| Rogue / Subtlety | 36 | 52 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/subtlety/level-30-dps-overview) |
| Warrior / Arms | 30 | 40 | [shoop](https://www.wowhead.com/forever/guide/classes/warrior/arms/level-30-dps-overview) |
| Warrior / Fury | 37 | 50 | [Babyhoof](https://www.wowhead.com/forever/guide/classes/warrior/fury/level-30-dps-overview) |
| Warrior / Protection | 40 | 51 | [Riyani](https://www.wowhead.com/forever/guide/classes/warrior/protection/level-30-tank-overview) |

Totalt 406 råa rekommendationsplaceringar; 388 kan användas i respektive
beslutade vapenupplägg med Dungeon/Quest/Crafting. Placeringarna grupperas
inom en målrad per utrustningsslot. Antalet ovan anger därför inte antal
utrustningsmål eller antal kopior som ska farmas.

## Slotkomplettering och möjliga set

Vid den första granskningen 2026-10-08 innehöll de nio ursprungstabellerna
259 placeringsrader och betydande luckor under Dungeon/Quest-policyn.
Icy Veins namngivna Hunter-rekommendation för Armor Piercer samt Wowheads
uttryckliga Protection-klassquests gav ytterligare källbelagda alternativ.
Ruby-Adorned Blade togs bort från Assassination: författaren rekommenderar
svärdet för Combat/Subtlety och beskriver det som ett dåligt specval för
Assassination.

WOWTBC:s **egna Forever-listor för samma spec** kompletterar återstående
luckor. Exempel: [Beast Mastery](https://wowtbc.gg/warcraftforever/bis-list/beast-mastery-hunter/),
[Assassination](https://wowtbc.gg/warcraftforever/bis-list/assassination-rogue/),
[Arms](https://wowtbc.gg/warcraftforever/bis-list/arms-warrior/) och
[Fury](https://wowtbc.gg/warcraftforever/bis-list/fury-warrior/).
Källan anger uttryckligen att items inte är ordnade. Den observerade
offentliga Gatsby-`page-data.json` länkar spec, slot, item-ID och källa.
139 nya placeringsrader har tagits med efter separat Forever-kontroll av
itemmetadata och, för quests, belöning, fraktion och miniminivå.
Craftinggodkännandet gjorde exempelvis Barbaric Shoulders till ett giltigt,
namngivet Arms/Fury-alternativ utan att en egen statranking behövdes.

[Slotgranskningen](data/forever-physical/slot-audit.json) innehåller 18
`legalSetWitness`: en möjlig samtidig uppsättning per spec och fraktion.
Alla har tom `missingSlots` och `fullSetValidated: true`.

| Specs | Platser i exempelset | Egen profession i exempelset |
| --- | ---: | --- |
| Hunter, alla tre | 16; tvåhands melee-vapen upptar OffHand | Leatherworking 150 |
| Rogue, alla tre | 17; två enhandade vapen | Ingen |
| Arms Warrior | 16; tvåhandsvapen upptar OffHand | Blacksmithing 150 |
| Fury Warrior | 17; två enhandade vapen | Ingen |
| Protection Warrior | 17; enhandat vapen och shield | Blacksmithing 150 |

Exempelseten är tillgänglighetsbevis, inte ranking. De använder olika basitem-ID:n
i parade slots, kontrollerar unique-equipped och vapenhänder samt väljer aldrig
två alternativa valbelöningar från samma quest. Publicerade questkedjor är
kontrollerade till miniminivå högst 30. Antal ägda kopior modelleras fortfarande
inte. Hunter kan även välja dual wield; OffHand-undantaget ovan gäller den
verifierade tvåhandsuppsättningen, inte alla Hunter-listor permanent.

Samtliga 13 befintliga craftingitems har en individuellt observerad
`created-by-spell`-post med recept-ID och skill 95–175, Engineering 140.
[Blacksmithing](https://www.icy-veins.com/wow-forever/blacksmithing),
[Leatherworking](https://www.icy-veins.com/wow-forever/leatherworking) och
[Engineering](https://www.icy-veins.com/wow-forever/engineering) anger Expert 225
från karaktärsnivå 20. Bind-on-pickup behandlas som egen tillverkning och
itemets användningskrav räknas separat. Ingen uppsättning kräver fler än två
samtidiga professioner; uppsättningarna ovan kräver högst en.

## Metadata och kontroll

Guideinnehåll och `WH.Gatherer.addData` har lästs från respektive original-HTML.
Item-ID:n kommer från guidens länkar; namn, slot och nivåkrav från dess
Forever-data. Questens namn och fraktion kommer från samma guides questdata.
Namngivna professioner och dungeonområden följer tabellerna. Dungeonbossar
som tabellen inte anger har inte lagts till från Vanilla.

Den ursprungliga granskningen kontrollerade 78 olika item-ID:n mot Wowheads Forever-tooltip:
`https://nether.wowhead.com/tooltip/item/{id}?dataEnv=16&locale=0`.
Namn matchade. Alla uttryckliga nivåkrav är högst 30. Värdet 0 används där
guidens metadata inte anger nivåkrav; questtillgänglighet följer den
uttryckliga nivå 30-guiden. Vissa API-fält anger 1 trots att den synliga
tooltipsen saknar nivåkrav; dessa metadatafält har bevarats.

Ikonens namn kommer från samma Forever-tooltip. Ursprungligen kontrollerades 70 olika fullständiga
`wow.zamimg.com/images/wow/icons/large/*.jpg`-URL:er härleddes och kontrollerades
med HTTP HEAD: 200 och bildinnehåll. Det är nätverksverifierade ikonlänkar,
inte lokalt hämtade eller visuellt jämförda bilder.

Unique-equipped har endast markerats när den lästa tooltipsen uttryckligen
anger det, exempelvis 274152, 3456, 4381, 6692, 7682, 7686 och 9453.
Se exempelvis [Ironspine's Eye](https://www.wowhead.com/forever/item=7686).
Övriga items har inte fått påhittade unique-regler.

Den slutliga faktasamlingen innehåller 150 individuella itemmetadata-poster,
inklusive granskade men avvisade kandidater, 82 quest-/kedjeposter och 45
itemers egna reward-from-quest-tabeller. Alla 118 olika ikon-URL:er i den
slutliga faktasamlingen gav HTTP 200 och bildinnehåll. Kontrollunderlaget
finns i [data/forever-physical](data/forever-physical).

## Slots och särskilda villkor

- Ringar och trinkets har en rad per möjlig slot. Det innebär alternativ
  placering, inte rekommendation att äga två exemplar. Unique-equipped följer
  det fysiska itemet över placeringarna.
- Generella one-hand-vapen har alternativa MainHand/OffHand-rader för
  Rogue och Hunter. Hunter-kapaciteten stöds uttryckligen av nivå 30-avsnittet
  i [Survival-guiden](https://www.wowhead.com/forever/guide/classes/hunter/survival/level-30-dps-overview).
  Rogue använder två vapen i de författade guiderna. Protection Warriors
  one-hand-val visas enbart i MainHand, tillsammans med separata shieldval.
- One-Hand, Main Hand, Off Hand och Two-Hand hålls isär genom itemets faktiska
  metadata. Bows, guns och thrown ligger i Ranged och tar inte melee-vapnens
  slots. Whirlwind Axe kräver hjälp mot fiender över nivå 30; questvillkoret
  finns i dess rad.
- Marksmanship behåller Stonecutter Claymore och Infiltrator Cap **of the
  Falcon**. Survival behåller Emissary Cuffs **of the Tiger**. Suffixet
  ingår i namn och `requiredSuffix`; numeriska suffix-ID:n har inte hittats på.
- Faction-specifika questvägar finns i anskaffningstexten. Engineering 140
  och tio charges för Minor Recombobulator finns i dess villkor.

## Källbegränsningar och kontrollerade rättelser

Wowheads Fury-guide har en trasig BBCode-rad `item=6687]`. Den identifierar
ändå ett konkret item-ID och dungeon. Namn/slot/nivå för Corpsemaker
kontrollerades mot Arms-guidens korrekta Forever-referens och Forever-tooltip.

Hunters Brawler's Leather Helm (252512) ligger i dungeon-tabellen med
anskaffning **Multiple**. Detta har bevarats som ospecificerad dungeonkälla.
Full bosslista är inte fastställd. Itemet får inte blandas ihop med
Brawler's Leather Hood (252504), som är ett separat crafting-item.

Rogue-guidernas generella länk till en grupp Brawler's Leather Items har inte
expanderats till gissade bas-ID:n. Tabellen saknar individuella rekommendationer
för gruppen. Campingföremål, ammo och consumables ingår inte i utrustningsslots.

Icy Veins kontrollerades också. Dess Warrior-guider saknar ännu namngivna
nivå 30-listor, och Subtlety-guiden har tagit bort sin lista medan nya items
undersöks. Dessa luckor har inte fyllts med Classic/SoD-data. Wowheads
separata Forever-guider gav de konkreta rekommendationerna ovan.

Starving Arcane-itemet 279837 hade fraktionskod 0 utan verifierad questväg.
Det har uteslutits från aktiv Arms-data och bevarats i `upgrade-exclusions.json`.
Det gäller även andra nya kandidater vars itemnivåkrav är över 30, vars
questbelöning/tillgänglighet inte kunnat verifieras eller vars recept ännu
inte granskats. Inget sådant item har använts för att dölja en slotlucka.

Power Stones har fortfarande äldre item-ID:n i questens valbelöningstabell.
Forever-guiden och Charged Leather Bracers individuella itempost pekar
båda på samma quest. Den observerade reward-from-quest-kopplingen sparas
separat och betraktas konservativt som ett alternativt questval i exempelsetet.
Detta dokumenterade metadatafel innebär inte rätt att välja två belöningar.
Ingen spelklientgenomgång eller DPS-/healing-/tank-simulering har utförts.

## Researchverktyg och ansvar

`tools/data/forever-physical/Read-GuideResearch.ps1` hämtar och extraherar endast
de nio originalguiderna till en separat researchkatalog under TEMP.
`Write-ReviewedCatalogs.ps1` skriver de granskade nio katalogerna och verifierar
namn, nivåer och ikonlänkar. Det vägrar ersätta befintliga kataloger utan ett
uttryckligt regenereringsval och matchande källidentitet. Tooltips måste finnas
i researchkatalogens `tips`-mapp före körning.

`Complete-SlotAudit.ps1` kompletterar de ursprungliga klassquest-/Icy-fynden
och visar den historiska Dungeon/Quest-luckbilden. Kör därefter
`Read-SlotUpgrades.ps1`, `Read-UpgradeTooltips.ps1`, `Review-QuestMetadata.py`,
`Review-QuestItemSources.py` och `Complete-UpgradeAudit.py` för den beslutade
Dungeon/Quest/Crafting-policyn och den slutliga slotgranskningen.
`Review-Icons.ps1` verifierar ikonernas HTTP-status. Individuella crafting-HTML
som lästs av hybridtråden återanvänds för observerade receptfakta; full HTML
och tooltips hålls under TEMP, medan endast relevanta fakta ligger i repot.

Ingen spelares framstegsfil har lästs eller ändrats. Huvudtråden ansvarar för
gemensam loader, domänregler, appverifiering, slutlig källfiltrering och commits.
