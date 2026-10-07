# Hämtning av riktig itemdata

Kontrolldatum: 2026-10-07. Scope: Holy Priest, Vanilla Classic, fas 1,
endast dungeons och quests. Detta är ett genomfört importprov och ett förslag
till integration; appen använder fortfarande sin fiktiva exempelkatalog.

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
| Fasindelade guider | Rekommendationsgrund och dungeon-/questförslag | Fas 1-tabeller och kompletterande item-/questkällor enligt phase1-items.md. Direkthämtning av Icy Veins via script stoppades av webbplatsens åtkomstskydd; ingen automatisk guidescraper införs. |
| Wowhead Classic tooltip | Engelskt namn, kvalitet, ikonnyckel och tooltip | 18 anrop till `nether.wowhead.com/classic/tooltip/item/{id}?dataEnv=4&locale=0` lyckades. Detta är en observerad, odokumenterad endpoint utan garanterat stabilt kontrakt. HTML sparas inte. |
| Wowheads ikon-CDN | Bild via ikonnyckeln | 18 HEAD-anrop till `wow.zamimg.com/images/wow/icons/large/{icon}.jpg` lyckades. Tillgänglighet kontrolleras vid import; URL:er kan förändras. |
| WarcraftDB | Alternativ tooltip med namn, kvalitet, ikon och HTML | Egen [API-dokumentation](https://classic.warcraftdb.com/tool/tooltip) och ett lyckat prov mot `/api/v1/tooltip/item/13102`. Inte inkopplat som fallback; ikonfältets format skiljer sig från Wowheads. |
| Blizzard Game Data API | Officiella item- och mediaresurser | [Blizzards endpoint-annons](https://us.forums.blizzard.com/en/blizzard/t/world-of-warcraft-classic-api-endpoints/346/) listar item/media. Ingen autentiserad hämtning utförd; aktuell namespace och Era-täckning återstår att verifiera. Blizzards [OAuth-exempel](https://github.com/Blizzard/node-signature-generator) använder client ID/secret. Hemligheter ska hållas utanför desktopklienten om detta införs. |

## Föreslagen integration i DDD-strukturen

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

Detta är ett integrationsförslag, inte implementerade produktfunktioner.

## Viktiga katalogvillkor

- Archivist Cape, Flameweave Cuffs och Atal'ai Gloves kräver **of Healing**.
  Bas-ID identifierar inte suffixet. Numeriska suffix-ID:n har inte verifierats.
- UBRS ingår i guideförslaget som pre-raid-dungeon med upp till 10 spelare.
  Informationen ska visas; det är inte en femmannainstans.
- Stormrager har olika questvägar per fraktion. Alliance-vägen är en
  Raid-quest utomhus. Bonecreeper Stylus från Scholomance finns som alternativ.
- Unika ringar/trinkets och framtida tvåhandsval behöver riktiga
  domänregler. Aktuell demomodell verifierar inte dessa itemegenskaper.
- Guideurvalet är inte en beräkning med egna statvikter. Fullständiga
  questförkrav och kontroll i en fas 1-spelklient återstår.

## Återskapa provet

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
