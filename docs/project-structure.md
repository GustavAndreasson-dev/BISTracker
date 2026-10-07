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
├── BISTracker.Domain/
│   ├── Equipment/                     EquipmentSlot och Recommendation
│   ├── Game/                          GameContext
│   └── Tracking/                      CharacterProgress och dess regler
├── BISTracker.Application/
│   ├── Catalog/                       BisCatalog och IBisCatalog
│   └── Tracking/                      TrackerService, tillstånd, snapshot och lagringskontrakt
├── BISTracker.Infrastructure/
│   ├── Catalog/                       SampleBisCatalog; framtida verifierade datakällor
│   └── Persistence/                   JsonProgressRepository
├── BISTracker.Presentation/
│   ├── App.xaml + MainWindow.xaml     Start, composition root och appens skal
│   ├── Common/                        Gemensam presentationsbas, ObservableObject
│   ├── Features/
│   │   └── Tracking/
│   │       └── ViewModels/            TrackerViewModel och TrackerItemViewModel
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
- Mappar är inte i sig DDD: kodens ansvar, invariants och beroenden måste också vara korrekta.
- Namespace kan behållas som lagrets publika namespace i utkastets kontrakt. Mappindelningen förändrar inte kontraktet mellan trådarna.

Bin/obj och IDE-mappar ingår inte i den avsedda källkodsstrukturen.
