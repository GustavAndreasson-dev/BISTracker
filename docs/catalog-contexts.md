# Katalognivåer och säkert byte

Beställt av Chefen 2026-10-08: Forever nivå 30 nu och nivå 60-import när data finns.
Modell, katalogläsning och import är implementerade. Alla 27 beta-kataloger på
nivå 30 är aktiva. Verkliga nivå 60-kataloger inväntar källgranskad data.

`CatalogSet` skiljer Classic fas 1/nivå 60 från Forever beta/nivå 30 och kommande
Forever nivå 60. `ICharacterCatalog` kan erbjuda nya källgranskade set. Val av
ett set från fel spelversion avvisas. Spelversion, nivå och beta/launch anges
uttryckligt; beta-listor presenteras inte som slutliga pre-raid-rankningar.

Karaktären behåller gemensamt itemägande inom sin spelversion, men tre egna
utrustningslistor per katalogset. Vid nivåbyte arkiveras tidigare listor och
återställs när spelaren byter tillbaka. Det aktiva setet sparas. Borttaget
ägande rensar itemvariantens utrustning i alla specs och arkiverade nivåer.

JSON-schema 1 utökas med frivilliga `catalogSetId` och `archivedLoadouts`.
Äldre filer utan dem använder versionens ursprungliga set och bevarar
karaktärs-ID, ägande och utrustning. Okända eller borttagna sparade set
rapporteras som fel i stället för att framsteg raderas. Inget databasschema ändras.

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
filer och `ForeverCatalogReader` översätter JSON till domänens rekommendationer.
`CharacterCatalog` upptäcker importen i samma appinstans. Även en redan vald,
tidigare tom nivå 60-lista uppdateras. Underlaget ska först granskas enligt
projektreglerna och hämtas separat före import.

Packidentiteter är oföränderliga: en dubblett mot ett inbyggt pack, befintlig
import eller annan fil i batchen avvisas. Nivå 30 skrivs aldrig över vid
nivå 60-import. Import av reviderad data med samma kontext/ID ingår inte i
detta flöde och behöver en separat versions- och migrationslösning.

## JSON-kontrakt, schema 1

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

Läsaren validerar alla råa rader före dungeon/quest-filtret. Den verifierar
format och metadatarelationer, inte att en extern guide faktiskt stöder
rekommendationen. Källgranskningen är därför en nödvändig del före import.
Antal exemplar modelleras inte. Samtidiga importer från flera appinstanser
är inte samordnade; använd en appinstans för import och framstegsredigering.

## Verifiering

Release-build godkänd utan varningar/fel; samtliga 39 konsolscenarier passerar.
Sex `CatalogContextScenarios` verifierar nivåisolering, äldre JSON, delat ägande,
arkiverade markeringar, tvåhands-/unique-regler och oförändrat tillstånd efter
sparfel. Sju `ForeverCatalogScenarios` verifierar alla 27 produktpack, filtrering,
uppdatering av samma katalogprovider, dubbletter, ogiltiga/blandade pack och avbrott utan
filförändringar. Isolerad WinUI-kontroll verifierar riktiga nivåval och import;
dess fiktiva nivå 60-prov levereras aldrig som produktdata.
Se [katalograpport](forever-level30-catalogs.md) och [verifiering](verification.md).
