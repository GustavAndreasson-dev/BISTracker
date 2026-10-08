# Hämtning av riktig itemdata

Ursprungligt kontrolldatum: 2026-10-07. Uppdaterat för överlämning 2026-10-08.
Importprovets scope: Holy Priest, Vanilla Classic, fas 1,
endast dungeons och quests. Detta är ett genomfört importprov och underlaget
till den nu [integrerade produktkatalogen](catalog-integration.md).

## Aktuell roll inför beta 2

Startpunkt: [beta2-handoff.md](beta2-handoff.md). Den här filen beskriver
det historiska onlineprovet för Classic-metadata. Det är ett separat
researchverktyg, inte appens runtimeimport av Forever-katalogpack.
Classic har 17 aktiva Holy Priest-mål och endast dungeons/quests. Forever
har 27 separata beta nivå 30-kataloger, godkänd crafting och 54 verifierade
fraktionsset. [Forever-katalogernas underlag](forever-level30-catalogs.md)
och [importkontraktet](catalog-contexts.md) beskriver den senare integrationen.

Version **1.0.0** är portabel och verifierad på Windows 11 med 49
beteendekontroller; kod/runtime är `e25c134`, leveransdokumentationen
`043d859`. [Rapport](data/portable-release-verification.json). Ren dator,
Windows 10 och import av verklig nivå 60-data är inte testade. Beta 2:s
produktfunktioner är ännu inte beslutade av Chefen.

## Resultat

Ja, onlinekällor kan ge både rekommendationer och itemmetadata. De fyller
olika roller: guider föreslår BiS, databaser identifierar items och bilder.
En tooltip bevisar varken BiS-ranking, historisk fas eller anskaffning.

[Källgranskningen](phase1-items.md) innehåller 17 huvudkandidater och
ett dungeonalternativ till wand. Head, Chest och Waist har ersatts efter
filtrering av world-drop BoE och crafting. Urvalet bygger på fas 1-avsnitt
hos [Icy Veins](https://www.icy-veins.com/wow-classic/priest-healer-pre-raid-gear)
och [Warcraft Tavern](https://www.warcrafttavern.com/wow-classic/guides/pre-raid-bis-for-holy-disc-priest-in-phase-1-pre-dm-patch-of-classic/).
Item-ID, anskaffning och fasgranskning är dokumenterade med separata källor.

Importprovet har hämtat **18 riktiga itemposter**, matchat alla engelska
namn mot förväntat ID och kontrollerat att samtliga 18 ikon-URL:er svarar
HTTP 200 med bildtyp. Se [kandidatmanifest](data/phase1-candidates.json),
[hämtad metadata](data/phase1-item-metadata.json) och
[PowerShell-script](../tools/data/Get-ItemMetadata.ps1).
Bilderna har inte laddats ned eller jämförts visuellt; JSON innehåller
ikonlänkar, inte bildfiler. Anskaffning kopieras från det granskade manifestet,
inte från tooltip-API:t.

## Källor och praktiska alternativ

| Källa | Vad den kan bidra med | Vad som faktiskt kontrollerats |
| --- | --- | --- |
| Fasindelade guider | Rekommendationsgrund och dungeon-/questförslag | Fas 1-tabeller och kompletterande item-/questkällor enligt phase1-items.md. Direkthämtning av Icy Veins via detta Classic-script stoppades av webbplatsens åtkomstskydd. Appen inför ingen livehämtning av guider; Forever har separata researchverktyg. |
| Wowhead Classic tooltip | Engelskt namn, kvalitet, ikonnyckel och tooltip | 18 anrop till `nether.wowhead.com/classic/tooltip/item/{id}?dataEnv=4&locale=0` lyckades. Detta är en observerad, odokumenterad endpoint utan garanterat stabilt kontrakt. HTML sparas inte. |
| Wowheads ikon-CDN | Bild via ikonnyckeln | 18 HEAD-anrop till `wow.zamimg.com/images/wow/icons/large/{icon}.jpg` lyckades. Tillgänglighet kontrolleras vid import; URL:er kan förändras. |
| WarcraftDB | Alternativ tooltip med namn, kvalitet, ikon och HTML | Egen [API-dokumentation](https://classic.warcraftdb.com/tool/tooltip) och ett lyckat prov mot `/api/v1/tooltip/item/13102`. Inte inkopplat som fallback; ikonfältets format skiljer sig från Wowheads. |
| Blizzard Game Data API | Officiella item- och mediaresurser | [Blizzards endpoint-annons](https://us.forums.blizzard.com/en/blizzard/t/world-of-warcraft-classic-api-endpoints/346/) listar item/media. Ingen autentiserad hämtning utförd; aktuell namespace och Era-täckning återstår att verifiera. Blizzards [OAuth-exempel](https://github.com/Blizzard/node-signature-generator) använder client ID/secret. Hemligheter ska hållas utanför desktopklienten om detta införs. |

## Historiskt integrationsförslag och vad som genomförts

Punkterna var förslag 2026-10-07. Källgranskad produktdata, infrastruktur-
läsare, ItemDetails, engelska villkor och bevarad migration är nu införda:

1. Behåll urvalet som en versionshanterad, källgranskad katalog. Registrera
   spelversion, fas, rekommendationskälla, item-ID, anskaffning och kontrolltid.
2. Kör metadatahämtning som ett separat verktyg under `tools/data/`.
   Ingen guidescraping eller nätverkshämtning behövs när appen startar.
3. Inför en riktig katalogimplementation under Infrastructure/Catalog bakom
   befintliga IBisCatalog. Application fortsätter att läsa katalogkontraktet;
   Domain ska inte bero på HTTP, JSON eller webbplatser.
4. Utöka kontraktet efter konkret behov med ikon, källänkar och villkor.
   Presentation visar engelska namn, anskaffning och den nödvändiga varianten.
   Behåll en läsbar platsmarkering om extern bild inte går att visa.
5. Byt från demokatalog först när tracking-ID, variantregler och framstegsfilens
   migration är hanterade. `demo-*` får inte tyst tolkas som riktiga items.

Appen använder nu en separat inbyggd katalog med de 17 huvudvalen,
villkor och bild-/källänkar. Infrastrukturimplementation och variantidentitet
är införda. Den första separata fas 1-framstegsfilen är nu legacy-underlag
för engångsmigration till karaktärernas `characters-v1.json`, enligt
[catalog-integration.md](catalog-integration.md).
Researchens 18-posters JSON läses inte direkt av appen. Ett generellt
automatiserat flöde från guide till godkänd produktkatalog är fortfarande ett förslag.

Normal app använder `ICharacterCatalog`/`CharacterCatalog`, som väljer rätt
spelversion, klass, spec och katalogset. `ICatalogPackImporter`/
`CatalogPackImporter` kopierar förgranskade Forever-JSON-pack atomiskt till
appens datakatalog och uppdaterar den levande providern. Det hämtar inte
guidesidor, och ett befintligt katalog-ID får inte ersättas. Ägande delas
per karaktär; sparad utrustning skiljs per spec och katalogset.

## Viktiga katalogvillkor

- Archivist Cape, Flameweave Cuffs och Atal'ai Gloves kräver **of Healing**.
  Bas-ID identifierar inte suffixet. Numeriska suffix-ID:n har inte verifierats.
- UBRS ingår i den aktiva Classic-katalogen som pre-raid-dungeon med upp till 10 spelare.
  Informationen ska visas; det är inte en femmannainstans.
- Stormrager har olika questvägar per fraktion. Alliance-vägen är en
  Raid-quest utomhus. Bonecreeper Stylus från Scholomance finns som alternativ.
- Den fasta Classic-katalogen innehåller varje unikt item en gång och bara
  ett main-hand/off-hand-par. Gemensamma regler för Unique över basitem-ID,
  en registrerad kopia per item/suffix, ring-/trinketkapacitet och tvåhands-
  vapen/offhand är nu införda för alternativa katalogmål. Metadata från
  Classic-provet ska inte automatiskt användas för Forever.
- Guideurvalet är inte en beräkning med egna statvikter. Fullständiga
  questförkrav och kontroll i en fas 1-spelklient återstår.

## Återskapa det historiska Classic-provet

Kräver PowerShell 7 och nätverksåtkomst. Från projektroten:

```powershell
./tools/data/Get-ItemMetadata.ps1
```

Scriptet kontrollerar manifestversion, positiva ID:n, tillåtna källtyper,
namnmatchning och ikonernas HTTP-svar. Det skriver JSON först när alla
poster passerat; ett fel ska bevara tidigare utdata. Importen fastställer
inte historisk fastillgänglighet eller slutlig BiS-ranking.

Kontroll av negativt item-ID och otillåten Crafting-källa gav förväntade
fel utan att tidigare utdata ändrades. Den sparade datamängden kontrollerades
för 18 unika ID:n och lyckade ikon-HTTP-svar.

Inför beta 2 ska en ny researchkörning granskas före uppdatering av
produktkatalogen. Befintliga 2026-10-07-resultat är en sparad ögonblicksbild,
inte ett löfte om att webbendpointen eller ikonernas HTTP-status är oförändrade.
Denna dokumentuppdatering kör inte nätprov och skapar ingen ny itemdata.
