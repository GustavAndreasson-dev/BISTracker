# Forever nivå 30: Hunter, Rogue och Warrior

Granskat 2026-10-08. Chefens nivå 30-beställning gäller Forever-betan,
patch 1.60.1. Nio separata JSON-kataloger har skapats med källbelagda
utrustningsalternativ. De är markerade **Partial**: guiderna täcker inte ett
fullständigt, rangordnat set för alla slots. De är inte nivå 60-listor.

## Ursprungliga rekommendationskällor

Wowheads författade Forever-guider har både konkreta itemlänkar och
Forever-itemmetadata. Katalogerna följer deras utrustningstabeller och
uttryckliga klassquest-rekommendationer. Antalet placeringar inkluderar
alternativa giltiga slots; det anger inte antalet items som ska farmas.

| Klass/spec | Namngivna guideitems | Placeringar | Ursprunglig guide |
| --- | ---: | ---: | --- |
| Hunter / Beast Mastery | 24 | 31 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/beast-mastery/level-30-dps-overview) |
| Hunter / Marksmanship | 26 | 33 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/marksmanship/level-30-dps-overview) |
| Hunter / Survival | 27 | 35 | [Docoda, granskad av Neteyes](https://www.wowhead.com/forever/guide/classes/hunter/survival/level-30-dps-overview) |
| Rogue / Assassination | 25 | 33 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/assassination/level-30-dps-overview) |
| Rogue / Combat | 30 | 42 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/combat/level-30-dps-overview) |
| Rogue / Subtlety | 30 | 42 | [warstry, granskad av Simonize](https://www.wowhead.com/forever/guide/classes/rogue/subtlety/level-30-dps-overview) |
| Warrior / Arms | 12 | 15 | [shoop](https://www.wowhead.com/forever/guide/classes/warrior/arms/level-30-dps-overview) |
| Warrior / Fury | 8 | 9 | [Babyhoof](https://www.wowhead.com/forever/guide/classes/warrior/fury/level-30-dps-overview) |
| Warrior / Protection | 17 | 19 | [Riyani](https://www.wowhead.com/forever/guide/classes/warrior/protection/level-30-tank-overview) |

Totalt 259 rekommendationsplaceringar. Ingen egen simulering eller omrankning
har gjorts. Alla anskaffningstyper i dessa tabeller har samlats in för att
huvudtråden ska kunna tillämpa Chefens slutliga källavgränsning.

## Metadata och kontroll

Guideinnehåll och `WH.Gatherer.addData` har lästs från respektive original-HTML.
Item-ID:n kommer från guidens länkar; namn, slot och nivåkrav från dess
Forever-data. Questens namn och fraktion kommer från samma guides questdata.
Namngivna professioner och dungeonområden följer tabellerna. Dungeonbossar
som tabellen inte anger har inte lagts till från Vanilla.

78 olika item-ID:n kontrollerades mot Wowheads Forever-tooltip:
`https://nether.wowhead.com/tooltip/item/{id}?dataEnv=16&locale=0`.
Namn matchade. Alla uttryckliga nivåkrav är högst 30. Värdet 0 används där
guidens metadata inte anger nivåkrav; questtillgänglighet följer den
uttryckliga nivå 30-guiden. Vissa API-fält anger 1 trots att den synliga
tooltipsen saknar nivåkrav; dessa metadatafält har bevarats.

Ikonens namn kommer från samma Forever-tooltip. 70 olika fullständiga
`wow.zamimg.com/images/wow/icons/large/*.jpg`-URL:er härleddes och kontrollerades
med HTTP HEAD: 200 och bildinnehåll. Det är nätverksverifierade ikonlänkar,
inte lokalt hämtade eller visuellt jämförda bilder.

Unique-equipped har endast markerats när den lästa tooltipsen uttryckligen
anger det: 274152, 3456, 4381, 6692, 7682, 7686 och 9453.
Se exempelvis [Ironspine's Eye](https://www.wowhead.com/forever/item=7686).
Övriga items har inte fått påhittade unique-regler.

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

## Researchverktyg och ansvar

`tools/data/forever-physical/Read-GuideResearch.ps1` hämtar och extraherar endast
de nio originalguiderna till en separat researchkatalog under TEMP.
`Write-ReviewedCatalogs.ps1` skriver de granskade nio katalogerna och verifierar
namn, nivåer och ikonlänkar. Det vägrar ersätta befintliga kataloger utan ett
uttryckligt regenereringsval och matchande källidentitet. Tooltips måste finnas
i researchkatalogens `tips`-mapp före körning.

Ingen spelares framstegsfil har lästs eller ändrats. Huvudtråden ansvarar för
gemensam loader, domänregler, appverifiering, slutlig källfiltrering och commits.
