# Holy Priest pre-raid: fas 1, dungeons och quests

Granskningsdatum: 2026-10-07. Forskningsunderlag för huvudtrådens katalogarbete;
ingen kod eller katalogimport ingår i denna fil.

De 17 huvudvalen är nu integrerade i appen. Denna fil bevarar researchen;
aktuell kodkoppling och UI-verifiering beskrivs i [catalog-integration.md](catalog-integration.md).

## Beslutad kontext och urvalsmetod

Chefen har valt vanliga WoW Classic (Vanilla), fas 1, Holy Priest och endast
anskaffning från dungeons och quests. Crafting, köp och world-drop BoE ingår
inte. Itemnamn och föreslagna slotnamn nedan är på engelska.

Utgångspunkt är fas 1-tabellen i [Icy Veins, Priest Healer Pre-Raid Gear](https://www.icy-veins.com/wow-classic/priest-healer-pre-raid-gear).
Head, Chest och Waist ersätts efter filtrering av otillåtna källor. Ersättarnas
rekommendationsstöd kommer från uttryckliga fas 1-guider hos Warcraft Tavern:
[Seebus, Holy Priest](https://www.warcrafttavern.com/wow-classic/guides/holy-priest/)
för Head och [Defcamp, Holy/Disc före Dire Maul](https://www.warcrafttavern.com/wow-classic/guides/pre-raid-bis-for-holy-disc-priest-in-phase-1-pre-dm-patch-of-classic/)
för Chest och Waist. Detta är ett guidebaserat urval efter källfiltrering,
inte en beräknad optimal kombination med nya statvikter.

Fas avser tillgänglighet i det ursprungliga Classic-upplägget. Blizzard
skiljer mellan införandet av nya items och ändrade stats på befintliga items;
Classic använder referensversion 1.12 för itemegenskaper. Ett äldre Vanilla-
patchnummer på en tooltip är därför inte ensamt ett skäl att flytta ett item
till en senare Classic-fas. Se [Blizzard, Itemization in WoW Classic, 2019-04-26](https://eu.forums.blizzard.com/en/wow/t/itemization-in-wow-classic/46655).

## Sammanhängande kandidatlista

Varje ID-länk går till den ClassicDB-post som granskats för identitet, slot
och märkningen **Added in content phase: 1**. Anskaffningen stöds av guiderna
och kontrollkällorna längre ned. Fasetiketten är sekundär databasuppgift,
inte en individuell bekräftelse från Blizzard. Samtliga valda basitems är
BoP i de granskade tooltipsen.

| Slot | Item och Classic-ID | Anskaffning | Rekommendationsstöd / villkor |
| --- | --- | --- | --- |
| Head | [Crimson Felt Hat — 18727](https://classicdb.ch/?item=18727) | Magistrate Barthilas, Stratholme (Undead) | Seebus fas 1 pre-raid; ersätter world-drop-valet. |
| Neck | [Animated Chain Necklace — 18723](https://classicdb.ch/?item=18723) | Ramstein the Gorger, Stratholme (Undead) | Icy Veins fas 1. |
| Shoulder | [Burial Shawl — 18681](https://classicdb.ch/?item=18681) | Delad bossloot i Scholomance | Icy Veins fas 1; Lord Alexei Barov är ett verifierat exempel. |
| Back | [Archivist Cape — 13386](https://classicdb.ch/?item=13386), **of Healing** | Archivist Galford, Stratholme (Live) | Icy Veins fas 1; kräver rätt slumpmässig bonus. |
| Chest | [Robes of the Exalted — 13346](https://classicdb.ch/?item=13346) | Baron Rivendare, Stratholme (Undead) | Defcamp fas 1; ersätter crafting-valet. |
| Wrist | [Flameweave Cuffs — 11766](https://classicdb.ch/?item=11766), **of Healing** | Lord Incendius, Blackrock Depths | Icy Veins fas 1; kräver rätt slumpmässig bonus. |
| Hands | [Atal'ai Gloves — 10787](https://classicdb.ch/?item=10787), **of Healing** | Trollminibossar, Sunken Temple | Icy Veins fas 1; kräver rätt slumpmässig bonus. |
| Waist | [Dustfeather Sash — 12589](https://classicdb.ch/?item=12589) | Solakar Flamewreath, Upper Blackrock Spire | Defcamp fas 1; ersätter den namnlösa BoE-raden. UBRS-villkor nedan. |
| Legs | [Senior Designer's Pantaloons — 11841](https://classicdb.ch/?item=11841) | Fineous Darkvire, Blackrock Depths | Icy Veins fas 1. |
| Feet | [Omnicast Boots — 11822](https://classicdb.ch/?item=11822) | Golem Lord Argelmach, Blackrock Depths | Icy Veins fas 1. |
| Ring 1 | [Fordring's Seal — 16058](https://classicdb.ch/?item=16058) | In Dreams, questkedja / Hearthglen | Icy Veins fas 1; båda fraktionerna. |
| Ring 2 | [Band of Rumination — 18103](https://classicdb.ch/?item=18103) | Warchief Rend Blackhand, Upper Blackrock Spire | Icy Veins fas 1; Unique. UBRS-villkor nedan. |
| Trinket 1 | [Briarwood Reed — 12930](https://classicdb.ch/?item=12930) | Jed Runewatcher, Upper Blackrock Spire | Icy Veins fas 1; sällsynt boss, Unique. UBRS-villkor nedan. |
| Trinket 2 | [Second Wind — 11819](https://classicdb.ch/?item=11819) | Golem Lord Argelmach, Blackrock Depths | Icy Veins fas 1; Unique. |
| Main Hand | [The Hammer of Grace — 11923](https://classicdb.ch/?item=11923) | Chest of The Seven, Blackrock Depths | Icy Veins fas 1; Main Hand-mace. |
| Off Hand | [Thaurissan's Royal Scepter — 11928](https://classicdb.ch/?item=11928) | Emperor Dagran Thaurissan, Blackrock Depths | Icy Veins fas 1; Held In Off-Hand. |
| Wand | [Stormrager — 16997](https://classicdb.ch/?item=16997) | Olika questkedjor för Alliance och Horde | Icy Veins fas 1; se fraktion och Raid-quest nedan. |

Ring 1/2 och Trinket 1/2 är visningspositioner, inte olika typer av itemslots.
Unique-märkningen måste beaktas om alternativ senare tillåter dubblering.
Main Hand och Off Hand utgör ett par; ett framtida tvåhandsalternativ ersätter
båda. Denna lista introducerar inget tvåhandsalternativ.

## Kontrollkällor för anskaffning

Följande länkar kompletterar rekommendationsguiderna. Vissa relaterade
loot-tabeller kunde läsas i sökverktygets indexerade sidutdrag men saknades
i den direkt öppnade sidans text. Det anges här för spårbarhet; inga
drop-procent har förts vidare.

| Item-ID | Separat kontroll | Vad som kontrollerats |
| --- | --- | --- |
| 18727 | [ClassicDB-item](https://classicdb.ch/?item=18727) | Tooltip och fas direkt; Barthilas i indexerad loot-tabell. |
| 18723 | [ClassicDB-item](https://classicdb.ch/?item=18723) | Tooltip och fas direkt; Ramstein i indexerad loot-tabell. |
| 18681 | [Lord Alexei Barov](https://classicdb.ch/?npc=10504) | Item i indexerad bossloot med fas 1. Ingen komplett förteckning över alla delade bossar verifierad. |
| 13386 | [Archivist Galford, Wowhead Classic](https://www.wowhead.com/classic/npc=10811/archivist-galford) | Galford och cape i Live-delen; även Defcamps fas 1-guide stöder anskaffningen. |
| 13346 | [Baron Rivendare](https://classicdb.ch/?npc=10440) | Item i indexerad bossloot med fas 1. |
| 11766 | [ClassicDB-item](https://classicdb.ch/?item=11766) | Tooltip/fas direkt; Incendius i indexerad loot-tabell. |
| 10787 | [Mijan](https://classicdb.ch/?npc=5717), [Zolo](https://classicdb.ch/?npc=5712) | Handskarna i indexerad bossloot; guiden beskriver gruppen av trollminibossar. |
| 12589 | [Solakar Flamewreath, JudgeHype Classic](https://wowclassic.judgehype.com/database/en/npc/10264/solakar-flamewreath/) | Item i bossens loot; Defcamp anger UBRS och egg-room-event. |
| 11841 | [Fineous Darkvire](https://classicdb.ch/?npc=9056) | Item i indexerad bossloot. |
| 11822, 11819 | [Golem Lord Argelmach](https://classicdb.ch/?npc=8983) | Båda items i indexerad bossloot med fas 1. |
| 16058 | [In Dreams — quest 5944](https://classicdb.ch/?quest=5944) | Belöning direkt; Group, båda fraktionerna, miniminivå 52 och föregående Scarlet Subterfuge. |
| 18103 | [Band of Rumination, WoW Classic Database](https://wowclassicdatabase.com/item/band-of-rumination) | Rend och Blackrock Spire direkt; denna sida visar inte ID eller fas, vilka kommer från ClassicDB-posten. |
| 12930 | [ClassicDB, itemversion 12930-2](https://classicdb.ch/?item=12930-2) | Jed i indexerad loot-tabell. URL-suffixet väljer databasens patchversion, inte itemets slumpmässiga enchant. |
| 11923 | [Defcamps fas 1-guide](https://www.warcrafttavern.com/wow-classic/guides/pre-raid-bis-for-holy-disc-priest-in-phase-1-pre-dm-patch-of-classic/) | Chest of The Seven som källa, oberoende av Icy Veins. Ingen separat läsbar chest-loot-tabell verifierad. |
| 11928 | [ClassicDB-item](https://classicdb.ch/?item=11928) | Emperor i indexerad loot-tabell. |
| 16997 | [Alliance quest 6187](https://classicdb.ch/?quest=6187), [Horde quest 6148](https://www.wowhead.com/classic/quest=6148/the-scarlet-oracle-demetria) | Questvägarna beskrivs nedan. |

## UBRS, slumpmässiga bonusar och quests

### UBRS som pre-raid-anskaffning

UBRS förekommer i samtliga använda pre-raid-guider, men körs med raidgrupp
upp till **10 spelare**. Det är inte en vanlig femmannainstans. Gruppens
tillträde förutsätter att någon har Seal of Ascension. Detta stöds av
[Wowheads ursprungliga Classic-guidepresentation från 2019](https://www.wowhead.com/classic/news/classic-guide-spotlight-upper-blackrock-spire-strategy-guide-292714).

Förslaget är att låta UBRS ingå som dungeon i produktens pre-raid-urval,
med tydlig information om tiomannagruppen. Detta behöver vara ett medvetet
scopeval i huvudtråden: utan UBRS faller Waist, Ring 2 och Trinket 1 i
tabellen ovan bort. Listan ska inte presentera dem som femmanna-dungeonloot.

### Of Healing är en del av rekommendationen

För 13386, 11766 och 10787 anger guiderekommendationen **of Healing**.
Basposterna visar slumpmässiga bonusar. Ett drop med samma bas-ID och annan
bonus uppfyller därför inte den angivna rekommendationen. Defcamps guide
förklarar också att healing-bonusen inte är garanterad.

Denna research verifierar inte numeriska suffix-ID:n, tillåtna bonusintervall
eller sannolikheten att få rätt variant. Dessa uppgifter får inte uppfinnas.
En manuellt använd tracker kan beskriva målet med fullständigt namn och
suffix; framtida automatisk matchning behöver identifiera varianten.

### Stormrager har två fraktionsvägar

- **Alliance:** [Order Must Be Restored, 6187](https://classicdb.ch/?quest=6187),
  från Highlord Bolvar Fordragon. Miniminivå 56, questtyp **Raid**, målet
  Nathanos Blightcaller i Eastern Plaguelands. Databasposten visar kedjan och
  Stormrager som valbar belöning. Detta är en utomhusquest, men den innebär
  en raidklassad strid och behöver tydlig information om anskaffningen.
- **Horde:** [The Scarlet Oracle, Demetria, 6148](https://www.wowhead.com/classic/quest=6148/the-scarlet-oracle-demetria),
  via Nathanos Blightcallers questkedja i Eastern Plaguelands. Wowheads
  indexerade Quick Facts anger Horde, miniminivå 56 och Elite. Defcamp
  beskriver samma fraktionsväg och belöning. ClassicDB visar felaktigt
  Both för denna quest; den uppgiften ska inte användas som bevis för en
  gemensam väg. En fullständig revision av varje förquest är inte gjord.

Om pre-raid även ska utesluta Raid-quests finns ett källstött dungeonalternativ:
[Bonecreeper Stylus — 13938](https://classicdb.ch/?item=13938), Darkmaster
Gandling i Scholomance. ID, Wand-slot, BoP och fas 1 är kontrollerade;
[Gandlings loot](https://classicdb.ch/?npc=1853) stöder anskaffningen.
Defcamp och Seebus rekommenderar den i sina fas 1 pre-raid-avsnitt. Valet
mellan denna och Stormrager ska beskrivas som guideurval, inte en beräknad ranking.

## Andra kontrollerade Head- och Waist-alternativ

Dessa är konkreta alternativa items, inte ytterligare beslutade huvudval.
Databasfakta nedan säger inte i sig vilket item som är BiS för ett Holy-bygge.

| Slot | Item-ID och anskaffning | Verifierat / begränsning |
| --- | --- | --- |
| Head | [Crown of the Penitent — 13216](https://classicdb.ch/?item=13216), [Houses of the Holy — 5243](https://classicdb.ch/?quest=5243) | Fas 1, BoP; 20 Intellect och 6 mana/5 sec. Dungeonquest för båda fraktionerna från nivå 55: samla 5 Stratholme Holy Water åt Leonid Barthalomew. Ingen separat Holy-BiS-ranking verifierad. |
| Head | [Devout Crown — 16693](https://classicdb.ch/?item=16693), [Darkmaster Gandling](https://classicdb.ch/?npc=1853), Scholomance | Fas 1, BoP; 24 Intellect, 15 Spirit, 13 Stamina. Ingen separat Holy-BiS-ranking verifierad. |
| Waist | [Thuzadin Sash — 18740](https://classicdb.ch/?item=18740), Nerub'enkan, Stratholme (Undead) | Fas 1, BoP; 12 Intellect, 11 Spirit, 11 Stamina, 11 damage/healing. Kandidat om UBRS utesluts; inte en verifierad fas 1-ranking framför Dustfeather. |
| Waist | [Ban'thok Sash — 11662](https://classicdb.ch/?item=11662), Ok'thor the Breaker, Blackrock Depths | Fas 1, BoP; arena-boss. 11 Intellect, 10 Stamina, 12 damage/healing och 1% spell hit. Spell-hit-bonusen innebär inte automatiskt ett högre värde för Holy. |
| Waist | [Devout Belt — 16696](https://classicdb.ch/?item=16696), Blackrock Spire-mobs | Fas 1, **BoE dungeon drop**, inte Scholomance. Defcamp anger bland annat Blackhand Summoner, Firebrand Dreadweaver och Smolderthorn Shadow Priest. Kan vara ett självfarmat dungeonalternativ om Chefen inkluderar sådana BoE; köp ingår fortfarande inte. |

## Uteslutna originalrader

- [Cassandra's Grace — 13102](https://classicdb.ch/?item=13102): world-drop BoE.
- [Truefaith Vestments — 14154](https://classicdb.ch/?item=14154): Tailoring.
- Icy Veins midjerad med namnlöst **of Healing**, BoE: saknar angivet basitem
  och ID. Den har inte tolkats som ett verkligt katalogitem.

Inga Dire Maul-items, reputation-/PvP-köp eller raidinstansdrops har lagts till.

## Observerade ikonlänkar

Följande fullständiga bildlänkar är observerade i Icy Veins fas 1-tabell.
Det är ikonmetadata, inte lokalt sparade bilder. Att länken observerats
betyder inte att bilden hämtats och visuellt jämförts med itemet.

| Item-ID | Direkt ikonlänk | Hämtning via webbverktyget |
| --- | --- | --- |
| 18723 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_jewelry_necklace_01.jpg) | Cache miss |
| 18681 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_shoulder_05.jpg) | Öppnades |
| 13386 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_misc_cape_19.jpg) | Öppnades |
| 11766 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_bracer_05.jpg) | Cache miss |
| 10787 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_bracer_18.jpg) | Öppnades |
| 11841 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_pants_09.jpg) | Cache miss |
| 11822 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_boots_05.jpg) | Öppnades |
| 16058 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_jewelry_ring_19.jpg) | Cache miss |
| 18103 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_jewelry_ring_16.jpg) | Cache miss |
| 12930 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_misc_root_02.jpg) | Cache miss |
| 11819 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_jewelry_talisman_06.jpg) | Cache miss |
| 11923 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_hammer_07.jpg) | Cache miss |
| 11928 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_mace_13.jpg) | Cache miss |
| 16997 | [ikon](https://static.icy-veins.com/images/classic/icons/inv_wand_05.jpg) | Cache miss |

För ersättarna 18727, 13346 och 12589 samt alternativen finns inga separat
verifierade direkta ikon-URL:er i denna rapport. Huvudtrådens item-API-arbete
behöver hämta och kontrollera dessa; URL:er ska inte konstrueras från minnet.

## Verifieringsnivå och kvarstående integration

- De 17 huvudraderna har verkliga bas-ID:n, slotar och individuellt lästa
  ClassicDB-fasetiketter. De tre bortfiltrerade platserna har namngivna
  ersättare med uttryckligt stöd i fas 1-healerguider.
- Icy Veins är uppdaterad 2024-11-18 och dess ändringslogg nämner Classic
  Anniversary. Den är därför inte ensam ett historiskt fasbevis. Äldre
  fas 1-guider och ClassicDB-etiketter kompletterar underlaget. Modern Era-
  tooltip eller namnmatchning har inte ensamt använts som fas 1-bevis.
- Warcraft Tavern-avsnitten har lästs via webbverktygets indexerade
  sidinnehåll; direktöppning fungerade inte. Flera databasers loot-tabeller
  har samma begränsning. Ingen verifiering i en fas 1-spelklient är gjord.
- UBRS-definition, Raid-quest för Alliance, slumpbonusar, fraktioner och
  ikonernas hämtbarhet behöver bevaras i katalogens presentation. Fulla
  questkedjor, suffix-ID:n och alla bildresurser är inte slutverifierade.
- Ingen kod, item-API-integration eller färdig UI har verifierats här.
  Huvudtråden ansvarar för gemensamma krav-/beslutsdokument, strukturkarta,
  implementation, tester och commits. Denna fil fastställer inte att
  REQ-001 är implementerat.
