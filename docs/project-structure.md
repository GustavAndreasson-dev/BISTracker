# Mappstruktur i projekten

God mappstruktur är ett uttryckligt användarkrav från 2026-10-07.
Detta är strukturkartan för första utkastet. Mappar skapas när de innehåller kod;
trådarna ska följa kartan och huvudtråden verifierar den vid integration.

```text
BISTracker/
├── AGENTS.md
├── README.md
├── BISTracker.slnx
├── docs/                              Krav, plan, struktur och beslut
│   ├── data/                          Kandidatmanifest och hämtad itemmetadata (forskningsdata)
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
│   └── Tracking/                      Legacy TrackerService, ITrackerService, snapshot och kontrakt
├── BISTracker.Infrastructure/
│   ├── Catalog/                       CharacterCatalog, ForeverCatalogReader, CatalogPackImporter samt Classic/Sample
│   │   └── Data/                      Classic fas 1 och Forever/level30 med 27 inbyggda kataloger
│   └── Persistence/                   JsonWorkspaceRepository, legacy JSON och ProgressFilePaths
├── BISTracker.Presentation/
│   ├── App.xaml + MainWindow.xaml     Start, composition root och appens skal
│   ├── Common/                        Gemensam presentationsbas, ObservableObject
│   ├── Features/
│   │   ├── Characters/
│   │   │   └── Views/                 CharacterDialog för ny karaktär
│   │   └── Tracking/
│   │       └── ViewModels/            TrackerViewModel, TrackerSlotViewModel och TrackerItemViewModel
│   ├── Assets/                        Bilder och paketresurser
│   └── Properties/                    Startkonfiguration
└── BISTracker.Checks/
    ├── Program.cs                     Startar konsolkontroller
    └── Scenarios/                     Domän-, användningsfalls- och lagringskontroller
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
- Namespace kan behållas som lagrets publika namespace i utkastets kontrakt. Mappindelningen förändrar inte kontraktet mellan trådarna.

Bin/obj och IDE-mappar ingår inte i den avsedda källkodsstrukturen.
