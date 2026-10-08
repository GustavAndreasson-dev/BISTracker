# Arkitektur och domänspråk

Status inför beta 2, 2026-10-08: portabel version **1.0.0** är levererad från
kodcommit `e25c134`; leveransdokumentationen finns i `043d859`.
Nästa agents startpunkt är [överlämningen inför beta 2](beta2-handoff.md).
Beta 2-funktioner är ännu inte beslutade. Classic-katalogen, Forever beta nivå 30 för alla 27 specs,
separata katalogset och katalogimport är implementerade. Verklig nivå 60-data
saknas. DDD och SOLID är beslutade principer.
Mappindelningen inom lagren ska följa [strukturkartan](project-structure.md).
WinUI-presentationen använder C# och .NET 8, med separata domän-, applikations- och infrastrukturprojekt.

## Implementerade lager

| Projekt | Ansvar | Tillåtna beroenden |
| --- | --- | --- |
| BISTracker.Domain | Domänbegrepp, värdeobjekt, invariants och regler. | Inga andra BISTracker-lager. |
| BISTracker.Application | Användningsfall och kontrakt för lagring eller dataåtkomst som dessa behöver. | Domain. |
| BISTracker.Infrastructure | Implementationer av lagring och eventuella dataintegrationer. | Application och Domain. |
| BISTracker.Presentation | WinUI-vyer, presentationslogik och koppling till användningsfall. | Application; Infrastructure i appens composition root för att koppla implementationer. |

Presentation kan använda domäntyper när det är motiverat, men får inte duplicera
domänregler. Filformat och externa datamodeller översätts vid infrastrukturgränsen.
Presentation använder MVVM för listan och filter. [App.xaml.cs](../BISTracker.Presentation/App.xaml.cs)
är composition root och kopplar CharacterCatalog, JsonWorkspaceRepository och
CharacterTrackerService till TrackerViewModel genom ICharacterTrackerService.
Den kopplar också CatalogPackImporter genom applikationskontraktet
[ICatalogPackImporter](../BISTracker.Application/Catalog/ICatalogPackImporter.cs).
Katalogpack lagras separat från spelarens framsteg under appens lokala `Catalogs`-mapp.

## Domängränser

- Utrustningskatalog: itemidentitet, utrustningsplats och var ett item kan erhållas.
- BiS-rekommendationer: vilka items som rekommenderas för en given klass, specialisering och spelkontext, samt rekommendationens källa.
- Framsteg: spelarens registrerade framsteg mot en vald rekommendation.

Detta är logiska gränser inom samma app. CharacterLoadouts är aggregatet som
skyddar gemensamt ägande och tre separata utrustningslistor per karaktär.
CharacterProgress finns kvar för legacy-validering. Katalogen är referensdata. Djupare rekommendationsregler
utvecklas när riktiga källor och urvalsmetod är beslutade.

### Invariants och användningsfall

REQ-010 korrigerar skillnaden mellan rekommendationsrader och slotmål.
`EquipmentPlan` grupperar alternativ per slot och kontrollerar en möjlig
kombination med itemkapacitet, unique, handkrav och separat fraktionstillgång.
En enda fysisk variant räknas inte som två ägda eller utrustade exemplar.
Se [korrekthetsregler och releasekontroll](slot-catalog-correctness.md).

- Ägande är booleskt och representerar ett exemplar per item-ID/suffix inom karaktären; antal exemplar modelleras inte.
- Utrustad innebär erhållen; avmarkering av erhållen tar bort variantens utrustning i alla specs och aktiva/arkiverade katalogset.
- Avmarkering av utrustad behåller erhållen.
- Ett item per EquipmentSlot kan vara utrustat. Ring- och trinketplatser är separata slotvärden; en rekommendation på båda platserna kan uttrycka alternativa placeringar.
- Unique-regeln gäller basitem-ID oavsett suffix inom en spec. Att utrusta det i en annan plats flyttar utrustningen; separat variantägande och andra specs bevaras.
- Tvåhandsvapen och offhand kan inte vara utrustade samtidigt i en spec. Ett byte rensar den oförenliga platsen, men bevarar ägande och andra specs.
- Okända katalog-ID:n och inkonsekvent återställt tillstånd avvisas. Äldre dubbelutrustade varianter och referenser till borttagna items är fel; originalfilen bevaras utan tyst reparation.
- CharacterTrackerService serialiserar läs/ändra/spara, validerar alla karaktärer och returnerar snapshot först efter lyckad sparning.
- JSON-lagringen skriver temporär fil och ersätter målet. Korrupt data rapporteras och bevaras.
- UI visar fel vid läs- eller skrivproblem och uppdaterar endast framsteg efter lyckat användningsfall.

## Gemensamt språk

| Begrepp | Arbetsdefinition |
| --- | --- |
| Item | Ett identifierbart utrustningsföremål. |
| Equipment slot | Utrustningsplats; ringar och trinkets har två uttryckliga slotvärden. |
| BiS recommendation | En källbelagd rekommendation för en definierad spelkontext, inte en universell ranking. |
| Pre-raid | Avgränsning vars tillåtna itemkällor ska bestämmas innan listan tas fram. |
| Game context | Spelvariant eller expansion samt den fas/tillgänglighet som styr rekommendationen. |
| Catalog set | Separat kontext med spelversion, nivågräns och utgivningsstadium; har egna sparade utrustningslistor. |
| Progress | Spelarens erhållna och utrustade items mot en rekommendation. |

## Utbyggnad till fler expansioner

Spelkontext ska vara uttrycklig i data och rekommendationer. Framtida expansioners
regler ska kunna skiljas från Vanilla-regler. Implementera endast de variationer
som behövs nu; generalisera vid konkreta nya krav.

## Verifiering av arkitekturen

Vid kodgranskning kontrolleras projektberoenden, placeringen av domänregler och
att användningsfall kan verifieras utan WinUI. Kritiska invariants testas i
domänen; lagringskontrakt verifieras mot den valda implementationen.

## Riktig katalog och variantidentitet

CharacterCatalog läser inbyggda Classic fas 1-paket (`Catalog/Data/Classic/phase1`),
de 27 inbyggda Forever-katalogerna och importerade Forever-pack genom
ReviewedCatalogReader. ClassicPhaseOneBisCatalog är en äldre enkellistvy av
Holy Priest-paketets 17 mål från 1.0.0, för den gamla framstegsfilen.
JSON-format, källmetadata och kontextvalidering stannar i Infrastructure.
ItemDetails i Domain beskriver item-ID, suffix, anskaffningstyp, vapenhand,
unique-villkor och referenslänkar. Kataloger och användningsfall kräver ingen
HTTP-hämtning av guider vid körning.

De tre of Healing-målen har variant i både namn och tracking-ID. Domain
avvisar okända ID:n; ett basitem eller demo-ID kan inte räknas som den
nödvändiga varianten. Presentation visar fakta och hämtar externa ikoner
med fallback. Separata framstegsfiler skyddar övergången från fiktiva items.
Se [integrationen](catalog-integration.md) för filansvar och begränsningar.

Forever beta nivå 30 visar Dungeon/Quest/Crafting enligt DEC-012;
Classic fas 1 använder samma policy enligt DEC-016. Antal alternativ, slotgrupper och giltiga
fraktionsset redovisas av [releasegranskningen](data/forever-slot-release-audit.json).
Katalogerna innehåller källbelagda guidealternativ, inte en
beräknad optimal ranking av ett helt utrustningsset. En ring på två platser har
två rekommendations-ID:n och samma fysiska itemidentitet; detta innebär inte
automatiskt en rekommendation av två exemplar. Källor och luckor redovisas i
[Forever-katalograpporten](forever-level30-catalogs.md).

ReviewedCatalogReader prefixar varje rått Forever-rekommendations-ID med katalogens
version/stadium/nivå/klass/spec-identitet. Classic-radernas ID:n innehåller redan
katalog-ID:t och används oförändrade; Holy Priest behåller sina 1.0.0-ID:n via `recommendationId`. Därmed kan samma rå-ID återkomma i
andra katalogset utan att utrustningsval blandas ihop. Gemensamt ägande använder
basitem-ID plus uttryckligt suffix inom karaktären; unique-regeln använder
basitem-ID utan suffix.

## Katalogimport och nivåkontext

[CatalogPackImporter](../BISTracker.Infrastructure/Catalog/CatalogPackImporter.cs)
implementerar ICatalogPackImporter. Den läser JSON-filer i en vald mapp och
undermappar och låter [ReviewedCatalogReader](../BISTracker.Infrastructure/Catalog/ReviewedCatalogReader.cs)
validera hela batchen före publicering. Classic-pack avvisas vid import. Läsaren kontrollerar schema, Forever-kontext,
giltig klass/spec, nivå 30 eller 60, Beta eller Launch, granskningsdatum,
urvalsmetod, HTTPS-källänkar och itemmetadata. Ett angivet itemnivåkrav får inte
överstiga katalogens nivågräns; vapenhand måste stämma med slot. Datainsamlingens
källgranskning ansvarar för rekommendationernas riktighet och faktisk anskaffning;
importören gör ingen ny webbrevision av guideinnehåll.

Duplicerade katalogidentiteter inom batchen eller mot inbyggda/tidigare importerade
pack avvisas. Validerade originalbytes kopieras till en separat stagingmapp och
hela mappen publiceras genom en enda mappflytt. Källfiler, befintliga kataloger
och framstegsdata skrivs inte över. Fel eller avbrott före publicering lämnar ingen
delvis importerad katalogbatch.

CharacterCatalog upptäcker ändrade importfiler genom en fingerprint av sökväg,
filstorlek och ändringstid och läser då om packen. Samma levande provider ger
tillgång till ett importerat katalogset utan omstart. TrackerViewModel anropar
importkontraktet och laddar sedan om snapshoten. Nivåval och import finns i UI.
Format och arbetsflöde finns i [katalogkontexter och import](catalog-contexts.md).

Nivå 60 visas som väntande tills verklig granskad data importeras. Importvägen
har verifierats med tydligt märkta testfixturer; dessa är inte produktdata.

## Karaktärer och speclistor

En karaktär har stabilt ID, version och klass. CharacterDefinition anger exakt
tre giltiga spec-ID:n för klassen. CharacterLoadouts skiljer gemensamt ägande
(item-ID/suffix inom karaktärens version) från utrustning (rekommendations-ID i
varje spec). Borttaget ägande rensar motsvarande utrustning i alla tre specs.
Listorna och ägandet isoleras mellan karaktärer. Katalogkontraktet väljer uttrycklig
version/katalogset/klass/spec och kan ange att granskad data saknas. Vid nivåbyte
arkiverar CharacterTrackerService det tidigare setets tre utrustningslistor och
återställer det valda setets listor. Gemensamt ägande gäller även mellan nivåerna;
borttaget ägande rensar motsvarande utrustning i både aktiva och arkiverade listor.

Application orkestrerar migration och tillståndsbyten genom IWorkspaceRepository.
Infrastructure sköter JSON och atomisk filersättning. Gamla framsteg kopieras en
gång utan att originalet ändras. Reglerna kräver varken UI eller filsystem för
att verifieras. Detaljer och begränsningar finns i [listmodellen](characters-and-loadouts.md).

Projektets **49 beteendekontroller** och lokala distributionsprov är godkända.
[ForeverCatalogScenarios](../BISTracker.Checks/Scenarios/ForeverCatalogScenarios.cs)
verifierar alla 27 kataloger, importvalidering, batchpublicering, duplicerade pack,
avbrott och uppdatering utan omstart.
[CatalogContextScenarios](../BISTracker.Checks/Scenarios/CatalogContextScenarios.cs)
verifierar nivåisolering, suffix/unique- och tvåhandsregler, återläsning samt att
sparfel bevarar aktiva och arkiverade listor.

De tre [DistributionScenarios](../BISTracker.Checks/Scenarios/DistributionScenarios.cs)
provar absoluta isolerade datavägar, verklig Classic-start och faktisk JSON-
återläsning samt Forever Rogue med tre specs och separata nivå30/60-listor.
Nivå60-fixturer är endast testdata. Alla 27 Forever30-kataloger har separata
granskade fraktionsvittnen, totalt **54 möjliga set**; detta är ingen optimal ranking.

## Leveransgräns och ansvar inför beta 2

Normal WinUI-start väljer `CharacterTrackerService` och riktig JSON-lagring.
`BISTRACKER_DATA_DIRECTORY` kan ange en absolut isolerad profil; utan override
används `%LOCALAPPDATA%\BISTracker`. Relativa profiler avvisas. Framsteg och
importerade pack ligger utanför programmappen, så nytt extraherat program inte
ersätter spelarens filer.

Publiceringen använder `PublishTrimmed=false` och inkluderar .NET samt Windows
App SDK/WinUI. [Portabel leveransverifiering](data/portable-release-verification.json)
kopplar exakt ZIP till 49 kontroller, identiska Domain/Application/Infrastructure-
DLL:er i kontrollhosten och verklig produktions-UI-sparning/omstart med isolerad
diskprofil. Ren mottagardator och äldre Windows har inte verifierats.

Huvudtråden samordnar nya krav, gränser, integration och commits. Domänregler
ska fortsatt ligga i Domain, användningsfall i Application, filformat och
import i Infrastructure och UI i Presentation. Datakällors riktighet kräver
källgranskning utöver formatvalideringen. Ingen mängdmodell, reparationsmigration,
databas eller annan beta 2-funktion är beställd genom detta dokument.
