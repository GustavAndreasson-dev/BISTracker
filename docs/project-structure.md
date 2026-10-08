# Mappstruktur i projekten

God mappstruktur är ett uttryckligt krav från Chefen från 2026-10-07.
Detta är den faktiska strukturkartan inför [beta 2-överlämningen](beta2-handoff.md),
granskad 2026-10-08. Levererad 1.0.0 bygger på kod `e25c134`, med verifierad
leveransdokumentation i `043d859`. Beta 2-funktioner är ännu inte beslutade.
Mappar skapas när de innehåller kod; huvudtråden verifierar ansvar vid integration.

```text
BISTracker/
├── CLAUDE.md                          Arbetsregler för huvudagent och underagenter
├── .claude/
│   └── agents/                        Underagenter med filansvar, se plan.md
├── README.md
├── BISTracker.slnx
├── docs/                              Krav, plan, struktur och beslut
│   ├── beta2-handoff.md               Samlad startpunkt för nästa agents arbete
│   ├── data/                          Research och verkliga release-/distributionsrapporter
│   └── previews/                      Verifierad UI-bild och resultat
├── tools/
│   ├── data/                          Metadatahämtning; forever-casters/hybrids/physical för research/import
│   └── distribution/                  Portabel publicering, PE-inventering och isolerade distributionsprov
├── BISTracker.Domain/
│   ├── Equipment/                     EquipmentSlot, EquipmentPlan, Recommendation och ItemDetails
│   ├── Game/                          GameContext, CatalogSet, CharacterDefinition, version/klass/spec
│   └── Tracking/                      CharacterLoadouts samt legacy CharacterProgress
├── BISTracker.Application/
│   ├── Catalog/                       BisCatalog, IBisCatalog, ICharacterCatalog och ICatalogPackImporter
│   ├── Characters/                    CharacterTrackerService, workspace och kontrakt
│   └── Tracking/                      Legacy TrackerService samt snapshot/entries för aktuell tjänst
├── BISTracker.Infrastructure/
│   ├── Catalog/                       CharacterCatalog, ForeverCatalogReader, CatalogPackImporter samt Classic/Sample
│   │   └── Data/                      Classic fas 1 och Forever/level30 med 27 inbyggda kataloger
│   └── Persistence/                   JsonWorkspaceRepository, legacy JSON och ProgressFilePaths
├── BISTracker.Presentation/
│   ├── App.xaml + MainWindow.xaml     Start, composition root och appens skal
│   ├── Common/                        Gemensam presentationsbas, ObservableObject
│   ├── Features/
│   │   ├── Characters/
│   │   │   └── Views/                 CharacterDialog (ny/namnbyte), DeleteCharacterDialog
│   │   └── Tracking/
│   │       └── ViewModels/            TrackerViewModel, TrackerSlotViewModel och TrackerItemViewModel
│   ├── Assets/                        Bilder och paketresurser
│   └── Properties/                    Startkonfiguration
└── BISTracker.Checks/
    ├── Program.cs                     Startar konsolkontroller
    └── Scenarios/                     56 beteendekontroller, releaseaudit och tre distributionsscenarier
```

## Regler

- Domain grupperas efter verksamhetsbegrepp, inte en stor Models-mapp.
- Application grupperas efter användningsfall och kontraktens ansvar.
- Infrastructure grupperas efter integration, datakälla eller lagringslösning.
- Presentation grupperas efter funktion; gemensam UI-infrastruktur ligger i Common.
- Appens skal kan ligga i projektroten. Nya funktioners vyer ska placeras under respektive Features-område när de införs.
- Scenarier grupperas efter det beteende de verifierar. Testverktyg ska inte bli produktkod.
- DistributionScenarios provar produktkataloger och riktig JSON-lagring. Distributionsverktygen publicerar till ignorerade artifacts-mappar och provar extraherad normal app med separat datamapp.
- Importverktyg ligger i tools/data och forskningsdata i docs/data. Dessa ska inte refereras av Domain eller läsas som en godkänd produktkatalog utan uttrycklig integration i Infrastructure/Catalog.
- Mappar är inte i sig DDD: kodens ansvar, invariants och beroenden måste också vara korrekta.
- Publika typer använder lagrets befintliga namespace. Mappindelning ska följa ansvar; det historiska draft-kontraktet beställer inga nya typer eller trådar.

Bin/obj och IDE-mappar ingår inte i den avsedda källkodsstrukturen.

## Levererad data och ansvar inför beta 2

`BISTracker.Infrastructure/Catalog/Data/Forever/level30` innehåller 27 produktkataloger
med Dungeon/Quest/Crafting och 54 granskade fraktionsset. Nivå60-Launch-data
finns ännu inte; fixtures ligger endast i isolerade kontroller. Ägande är
booleskt per itemvariant och tre utrustningslistor sparas separat per katalogset.

Spelarens schema1-JSON och importerade kataloger finns i
`%LOCALAPPDATA%\BISTracker`, eller i en uttrycklig absolut profil via
`BISTRACKER_DATA_DIRECTORY`. Normal profil och distributionsprovets TEMP-profil
ligger utanför källkod och programkatalog. Override tillåter en valfri absolut
sökväg; spelarens framsteg ska inte versionshanteras eller följa med i ZIP.
Build-utdata ligger i ignorerade bin/obj-mappar; publicerade paket och
distributionsrapporter ligger under ignorerade `artifacts`-mappar.

`tools/distribution` sköter untrimmed self-contained-publicering och provar
normal extraherad app med separat diskprofil. .NET och Windows App SDK/WinUI
följer med paketet. Det exakta leveransbeviset ligger i
[portable-release-verification.json](data/portable-release-verification.json).

Huvudtråden tilldelar nya filansvar och samordnar commits. Domain äger
invariants, Application användningsfall, Infrastructure lagring/import och
Presentation interaktion. Betydelsefulla kontroller hör hemma i Checks.
Mängdägande, schemaändringar eller migration av gamla dubbletter/borttagna
items kräver först ett beslutat beta 2-scope; befintliga fel bevarar filen.
