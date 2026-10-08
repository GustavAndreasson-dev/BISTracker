# Källresearch för Holy Priest pre-raid

Ursprungligt granskningsdatum: 2026-10-07. Uppdaterat för överlämning
2026-10-08. Rapporten nedan bevarar den första researchen för REQ-001;
vid den granskningen fanns ännu ingen riktig itemkatalog. Anvisningen om
exempeldata i [draft-contract.md](draft-contract.md) gällde det historiska utkastet.

## Aktuell roll inför beta 2

Starta nästa agents arbete i [beta2-handoff.md](beta2-handoff.md). Den här
filen förklarar varför spelkontext, rekommendation och itemmetadata behöver
separata källor; den är inte en instruktion att återgå till demoitems.
Classic Holy Priest har nu 17 aktiva fas 1-mål från dungeons/quests enligt
[phase1-items.md](phase1-items.md) och [catalog-integration.md](catalog-integration.md).
Forever har 27 egna nivå 30-kataloger med godkänd crafting och 54 verifierade
fraktionsset; Classic-källor används inte som Forever-rekommendationer.

Den verifierade baslinjen är portabel version **1.0.0**, kod/runtime från
`e25c134` och leveransdokumentation från `043d859`: 49 beteendekontroller och
distributionstest på utvecklingsdatorns Windows 11. Se
[leveransrapport](data/portable-release-verification.json). Ren dator,
Windows 10 och verklig nivå 60-data är inte verifierade. Beta 2-funktioner
är ännu inte beslutade av Chefen; gamla öppna frågor nedan är inte nya krav.

## Källbelagda observationer

Denna första rapport bevaras som historiskt underlag. Chefen har därefter
beslutat fas 1 och endast dungeons/quests. Aktuellt urval finns i
[phase1-items.md](phase1-items.md) och metadataimporten i
[data-import.md](data-import.md). Öppna scopefrågor nedan är den ursprungliga
granskningens frågor, inte återkallade beslut.

| Källa och direktlänk | Spel och tillgänglighetskontext | Användning och begränsning |
| --- | --- | --- |
| [Icy Veins: Priest Healer Pre-Raid Gear](https://www.icy-veins.com/wow-classic/priest-healer-pre-raid-gear), Abide, uppdaterad 2024-11-18 | WoW Classic; uttryckligen Holy och Discipline. Separata tabeller för fas 1, fas 2–4 och fas 5–6. | Kandidatkälla för rekommendationer med fasindelning. Innehåller dungeonloot, crafting, BoE, quests samt PvP-rank och reputation. Inte enbart dungeonutrustning. |
| [Wowhead: Priest Healing Pre-Raid BiS — Classic Era](https://www.wowhead.com/classic/guide/priest-healing-pre-raid-best-in-slot-bis-gear-wow-classic), uppdaterad 2023-10-08 | Uttryckligen Classic Era och Priest Healing; ingen separat fas 1-lista på sidan. | Kandidatkälla med alternativ och motiveringar. Guiden framhåller att val beror på spelstil och raidmiljö. Healing Priest är bredare än ett specificerat Holy-bygge. |
| [Blizzard: WoW Classic FAQ](https://news.blizzard.com/en-gb/article/23090136/world-of-warcraft-classic-faq-what-you-need-to-know) | Historisk plan inför lanseringen 2019, med sex innehållsfaser. Fas 5 omfattar ändrad dungeonloot och Tier 0.5. | Primärkälla för ursprunglig tillgänglighetsplan, inte en BiS-ranking eller bevis för dagens Era-regler. |
| [Blizzard: Anniversary Phase 2](https://worldofwarcraft.blizzard.com/en-gb/news/24167661/wow-classic-20th-anniversary-edition-phase-2-arrives-january-9) | 20th Anniversary Edition; Dire Maul, honor och battlegrounds i fas 2, med separat tidplan och initial rankgräns. | Visar att fasnummer måste knytas till spelvariant. Anniversary-data får inte automatiskt användas som Era-data. |

### Konkreta skillnader och oklarheter

- **Olika urval:** Icy Veins rekommenderar Burial Shawl även i fas 5–6;
  Wowheads Era-guide väljer Mantle of Lost Hope och listar Burial Shawl som
  alternativ. Detta är en observerad skillnad mellan guiderna, inte ett avgörande
  om vilket item appen ska välja. Se respektive guide ovan.
- **Vad betyder pre-raid?** Icy Veins inkluderar rank 14-vapen och Alterac
  Valley-reputation. Wowheads Era-guide inkluderar Desert Bloom Gloves från
  en quest vars strid enligt guiden kräver minst 20 personer. Avsaknad av loot
  från en raidinstans innebär alltså inte automatiskt ett krav på små grupper.
  Se respektive guide ovan.
- **Källfel måste kunna upptäckas:** Icy Veins anger Royal Seal of Eldre'Thalas
  som loot från Jed Runewatcher i UBRS. [Wowheads Classic-itempost för Priest,
  ID 18469](https://www.wowhead.com/classic/item=18469/royal-seal-of-eldrethalas)
  beskriver i stället belöningen från bokquesten Holy Bologna i Dire Maul.
  Slutsats: importera inte guidens anskaffningskolumn utan separat kontroll.
  Samma itemnamn räcker inte heller som identitet för klassvarianter.

## Historiskt metodförslag från första granskningen

1. Lås en uttrycklig kontext: Vanilla Classic-variant, tillgänglighetsläge/fas,
   referensdatum och Holy-bygge. Använd inte enbart etiketten ”Classic”.
2. Besluta tillåtna källor. Föreslagen grund är att utesluta raidloot och
   raidberoende anskaffningskedjor; quests, crafting, BoE och reputation behöver
   uttryckliga val. Hantera UBRS, PvP och gruppquests separat i beslutet.
3. Välj en huvudguide som rekommendationsgrund och använd den andra för
   kontroll. Bevara olikheter och motivera avvikelser; slå inte ihop rankningar
   eller uppfinn gemensamma statvikter.
4. För varje framtida katalogpost: spara stabilt Classic-item-ID, eventuell
   slumpmässig suffixvariant, slot, klass-/faktions-/professionskrav, anskaffning
   med förkrav och tillgänglighet. Håll länken till itemfakta skild från länken
   till rekommendationen. Registrera källans datum, kontrollens datum och
   rekommendationsmotivering.
5. Granska hela urvalet mot beslutad kontext före publicering. Verifiera
   särskilt questkedjor, craftingmaterial, unika items, två ring-/trinketplatser
   och kombinationer av tvåhandsvapen respektive main-hand/off-hand. Dessa är
   kontrollpunkter för nästa datasteg, inte verifierade regler i denna rapport.

## Produktbeslut vid den första granskningen

- Classic Era med sent Vanilla-innehåll, eller en bestämd historisk fas? Om
  historisk fas: vilken tidpunkt och vilka innehållsupplåsningar?
- Tillåts crafting/BoE via köp, PvP-rank, battleground-reputation, UBRS och
  stora gruppquests? Ska raidberoende material eller questförkrav uteslutas?
- Vilket Holy-bygge och vilken rekommendationsgrund ska styra? Ska appen visa
  en huvudrekommendation eller även praktiska alternativ?
- Ska katalogen täcka båda fraktionerna och professionsberoende alternativ,
  eller anpassas till en viss karaktär?

## Den ursprungliga granskningens verifiering och begränsningar

Projektets README, arbetsregler och relevanta docs är lästa. Guiderna och
Blizzards FAQ har öppnats och relevanta avsnitt jämförts; Anniversary-artikeln
har granskats via sökverktygets sidinnehåll. Itemposten för 18469 har öppnats som
stickprov. Ingen fullständig item-/quest-/fasrevision eller verifiering i spelet
har gjorts i den ursprungliga granskningen. Denna research är därför inte
ensam implementationsbevis för REQ-001. Senare katalog-, lagrings-, UI- och
distributionstester redovisas i [verification.md](verification.md).
