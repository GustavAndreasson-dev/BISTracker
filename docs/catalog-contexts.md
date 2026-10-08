# Katalognivåer och säkert byte

Aktuell överlämning 2026-10-08: [beta 2-handoff](beta2-handoff.md).
Portabel 1.0.0 är verifierad från kod `e25c134`, dokumenterad i `043d859`;
beta 2-funktioner är inte beslutade.

Beställt av Chefen 2026-10-08: Forever nivå 30 nu och nivå 60-import när data finns.
Modell, katalogläsning och import är implementerade. Alla 27 beta-kataloger på
nivå 30 är aktiva. Verkliga nivå 60-kataloger inväntar källgranskad data.

`CatalogSet` skiljer Classic fas 1/nivå 60 från Forever beta/nivå 30 och kommande
Forever nivå 60. `ICharacterCatalog` kan erbjuda nya källgranskade set. Val av
ett set från fel spelversion avvisas. Spelversion, nivå och beta/launch anges
uttryckligt; beta-listor presenteras inte som slutliga pre-raid-rankningar.

De levererade standard-ID:na är `classic-phase1-level60`,
`forever-beta-level30` och **`forever-launch-level60`**. Det väntande nivå60-valet
är Launch, inte Beta. Läsaren tillåter även separat källgranskad Beta60-import,
men en sådan import fyller inte automatiskt den valda Launch60-kontexten.

Karaktären behåller gemensamt itemägande inom sin spelversion, men tre egna
utrustningslistor per katalogset. Vid nivåbyte arkiveras tidigare listor och
återställs när spelaren byter tillbaka. Det aktiva setet sparas. Borttaget
ägande rensar itemvariantens utrustning i alla specs och arkiverade nivåer.

JSON-schema 1 har bakåtkompatibla frivilliga `catalogSetId` och `archivedLoadouts`.
Äldre filer utan dem använder versionens ursprungliga set och bevarar
karaktärs-ID, ägande och utrustning. Okända eller borttagna sparade set
rapporteras som fel i stället för att framsteg raderas. Inget databasschema ändras.

Ägande är booleskt: ett exemplar per fysisk itemvariant, identifierad av
item-ID/suffix. Gamla dubbelutrustningar och sparade referenser till uteslutna
items avvisas med filen bevarad. Ingen automatisk reparations- eller mängdmigration
ingår i den levererade katalogbytesfunktionen.

Domänen hanterar källbelagda tvåhands/offhand-konflikter och unika items.
Utrusta tvåhandsvapen avmarkerar vald specs offhand; utrusta offhand avmarkerar
dess tvåhandsvapen. Ägandet bevaras. Ett unikt basitem flyttas mellan möjliga
ring-/trinketplaceringar och kan inte återställas som dubbelutrustat, även när
två rekommendationer har olika suffix. Ägande använder fortfarande item-ID
plus suffix; unique-regeln jämför basitem-ID.
Katalogen måste bara erbjuda vapenplaceringar som verifierats för klass/spec;
inga generella nya dual-wield-talanger antas.

## Import av nya pack

Välj **Import catalogs** och mappen med de källgranskade JSON-filerna.
Filer i undermappar ingår också; välj därför en katalogmapp med enbart pack,
inte en researchmapp med andra JSON-filer. Importen kopierar originalen till
`%LOCALAPPDATA%/BISTracker/Catalogs/import-{batchId}/`. Varje pack valideras
innan hela batchen publiceras genom en katalogflytt. Originalfiler, äldre
pack och spelarframsteg bevaras. Ett ogiltigt pack avvisar hela batchen.

`ICatalogPackImporter` är applikationskontraktet; `CatalogPackImporter` sköter
filer och `ReviewedCatalogReader` översätter JSON till domänens rekommendationer.
Endast Forever-pack kan importeras. Ett giltigt Classic-pack avvisas av importen,
och ett Classic-pack som ligger i importmappen gör katalogladdningen ogiltig;
Classic fas 1-paket är alltid inbyggda.
`CharacterCatalog` upptäcker importen i samma appinstans. Även en redan vald,
tidigare tom nivå 60-lista uppdateras. Underlaget ska först granskas enligt
projektreglerna och hämtas separat före import.

Det gäller när importens stage/nivå/klass/spec matchar det valda setet.
Verkliga nivå60-Launch-guider/items finns ännu inte i produkten; verifierade
importprov använder tydligt märkta TEST ONLY-fixturer, aldrig produktpack.
En absolut `BISTRACKER_DATA_DIRECTORY`-override flyttar både framsteg och
`Catalogs` till den isolerade profilen. Utan override behålls LocalAppData.

Packidentiteter är oföränderliga: en dubblett mot ett inbyggt pack, befintlig
import eller annan fil i batchen avvisas. Nivå 30 skrivs aldrig över vid
nivå 60-import. Import av reviderad data med samma kontext/ID ingår inte i
detta flöde och behöver en separat versions- och migrationslösning.

## JSON-kontrakt, schema 1

Samma läsare (`ReviewedCatalogReader`) och schema gäller Forever och de inbyggda
Classic fas 1-paketen. Classic-specifika värden och ID-regler finns i
[Classic fas 1-kataloger](classic-phase1-catalogs.md); tabellen nedan beskriver Forever.

Varje klass/spec har ett pack. Obligatoriska rotfält:

| Fält | Tillåtet innehåll |
| --- | --- |
| `schemaVersion` | `1` |
| `catalogId` | `forever-{beta eller launch}-level{30 eller 60}-{klass med små bokstäver}-{specId}` |
| `gameVersion` | `Forever` |
| `characterClass`, `specializationId` | Giltig klass och dess stabila spec-ID enligt CharacterDefinition |
| `levelCap`, `releaseStage` | `30` eller `60`; `Beta` eller `Launch` |
| `phase` | Uttrycklig tillgänglighetskontext |
| `sourceUrl`, `reviewedOn` | HTTPS-källa och granskningsdatum `yyyy-MM-dd` |
| `selectionMethod`, `status` | Beskriven urvalsmetod; `Reviewed` eller `Partial` |
| `items` | Minst en granskad rekommendationsrad |

`patch` är frivilligt i läsformatet; alla levererade nivå 30-pack anger 1.60.1.
En framtida katalog ska fortfarande ha uttrycklig spelversion och patch/fas
enligt datagranskningsreglerna. Exempel på faktisk struktur finns i
[Mage Fire](../BISTracker.Infrastructure/Catalog/Data/Forever/level30/mage-fire.json).

Varje itemrad kräver `id`, `slot`, positivt `itemId`, `name`, `acquisitionType`,
`source`, `note`, `itemUrl`, `recommendationUrl`, `weaponKind` och
`uniqueEquipped`. Slot, anskaffning och vapenhand använder domänens enum-namn,
inte heltal. HTTPS krävs för referenslänkar och eventuell `iconUrl`.
`requiredSuffix`, `iconUrl` och `requiredLevel` kan saknas; en angiven nivå
får inte vara negativ eller över packets nivåtak. Vapenhand måste passa slot.
Råa rad-ID:n får inte dubbleras inom paketet; läsaren prefixar dem med
`catalogId` så rekommendations-ID:n skiljer nivå 30 från 60.

Frivilliga tillgänglighetsfält:

- `availableFactions` per item: `Alliance` och/eller `Horde`. En Quest-rad
  utan verifierad fraktion räknas inte i något fraktionsset och blockerar
  releasekontrollen. Ogiltiga eller dubblerade fraktionsvärden avvisas.
- `weaponSetup`: `Flexible` (standard), `TwoHanded` eller `OneHandAndOffHand`.
Ett uttryckligt handkrav behöver HTTPS-källan `weaponSetupSourceUrl`.
- `slotExemptions`: lista av `slot`, `reason`, `sourceUrl` för en styrkt
  oanvändbar plats. Undantaget får inte motsäga katalogens itemrader.

Läsaren validerar alla råa rader före Dungeon/Quest/Crafting-filtret, som
sedan DEC-016 gäller både Forever och Classic fas 1. Den verifierar
format och metadatarelationer, inte att en extern guide faktiskt stöder
rekommendationen. Källgranskningen är därför en nödvändig del före import.
Antal exemplar modelleras inte. Samtidiga importer från flera appinstanser
är inte samordnade; använd en appinstans för import och framstegsredigering.

## Verifiering

Release-build godkänd utan varningar/fel; samtliga **54** konsolkontroller passerar
(49 i 1.0.0 plus fem `ClassicCatalogScenarios` för beta 2, se nedan).
Sex `CatalogContextScenarios` verifierar nivåisolering, äldre JSON, delat ägande,
arkiverade markeringar, tvåhands-/unique-regler och oförändrat tillstånd efter
sparfel. Sju `ForeverCatalogScenarios` verifierar alla 27 produktpack, filtrering,
uppdatering av samma katalogprovider, dubbletter, ogiltiga/blandade pack och avbrott utan
filförändringar. Isolerad WinUI-kontroll verifierar riktiga nivåval och import;
dess fiktiva nivå 60-prov levereras aldrig som produktdata.
Fem `ClassicCatalogScenarios` verifierar Classic-läsarens kontext och källpolicy,
`recommendationId`-reglerna, att Classic-pack inte kan importeras, Holy Priests
17 rad-ID:n från 1.0.0 samt att en schema 1-sparfil från 1.0.0 med Holy Priest-
ägande och utrustning laddas oförändrad.
Se [katalograpport](forever-level30-catalogs.md) och [verifiering](verification.md).

De tre distributionskontrollerna omfattar faktisk diskpersistens och Rogue
med tre specs över nivå30/60; kontrollhostens kärn-DLL:er matchar det exakta
1.0.0-paketet enligt [leveransrapporten](data/portable-release-verification.json).
Alla 27 Forever30-kataloger använder godkänd Dungeon/Quest/Crafting-policy
och har 54 granskade fraktionsset. Uppdatering av befintliga pack-ID:n,
gamla ogiltiga framsteg och nya beta 2-funktioner behöver Chefens beslut;
denna överlämning beställer ingen sådan implementation.
