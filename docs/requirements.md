# Krav och koppling till kod

Statusdatum: 2026-10-07. Beslutade krav kommer från användaren.
Acceptanskriterier och detaljer markerade som förslag behöver preciseras tillsammans.

## Beslutade produktkrav

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-001 | Visa pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla). | [UI-utkast](../BISTracker.Presentation/MainWindow.xaml) och [exempelkatalog](../BISTracker.Infrastructure/Catalog/SampleBisCatalog.cs) finns. Riktig BiS-data saknas. | UI byggt och renderat; kravet är delvis implementerat. Fas, datakälla och urvalsmetod återstår. |
| REQ-002 | Tracka både erhållna och utrustade items mot pre-raid BiS. | [CharacterProgress](../BISTracker.Domain/Tracking/CharacterProgress.cs), [TrackerService](../BISTracker.Application/Tracking/TrackerService.cs), [JSON-lagring](../BISTracker.Infrastructure/Persistence/JsonProgressRepository.cs) och [ViewModel](../BISTracker.Presentation/Features/Tracking/ViewModels/TrackerViewModel.cs). | [Trackingkontroller](../BISTracker.Checks/Scenarios/TrackingScenarios.cs), [lagringskontroller](../BISTracker.Checks/Scenarios/PersistenceScenarios.cs) och isolerad UI-kontroll godkända. Verifierat för demoprofil och exempeldata; riktig katalog återstår. |

## Beslutade tekniska krav och riktning

| ID | Krav eller riktning | Implementation | Verifiering |
| --- | --- | --- | --- |
| ARC-001 | Använd DDD-struktur och en domänmodell för verksamhetens regler. | Fyra lager; CharacterProgress skyddar invariants. Se [arkitektur](architecture.md). | Projektberoenden och regler granskade; domän och användningsfall kontrolleras utan WinUI. |
| ARC-002 | Följ SOLID-principer. | Separata ansvar och IBisCatalog/IProgressRepository för externa beroenden. | Katalog och repository utbytta i beteendekontroller. Fortsatt granskningskrav vid varje ändring. |
| ARC-003 | Håll Markdown-dokumentation som kan jämföras med koden. | AGENTS.md och docs, inklusive denna kodspårning. | Nuläge, projektgränser och källfiler har jämförts mot koden. |
| ARC-004 | Ha god mappstruktur inom varje projekt. | Kod indelad enligt [strukturkartan](project-structure.md). | Faktiska filplaceringar granskade; områden för Equipment, Game, Tracking, Catalog, Persistence och Features finns. |
| DIR-001 | Stöd fler expansioner senare. | Ingen expansion utöver första målversionen är implementerad. | Planerad riktning; ännu inga beslutade funktionella acceptanskriterier. |

## Föreslagna acceptanskriterier för första fungerande flödet

- REQ-001: användaren kan se rekommenderad utrustning per utrustningsplats, med ursprungskälla för items och en förklarad rekommendationsgrund.
- REQ-002: användaren kan markera ett rekommenderat item som erhållet eller utrustat och återställa markeringarna. Markeringarna finns kvar efter omstart. Utrustad innebär erhållen; att avmarkera erhållen tar också bort utrustningsmarkeringen.
- Kontexten för spelversion och tillgänglighet är synlig och entydig.

Dessa detaljer är förslag, inte ytterligare beslutade produktkrav.

## Uppdateringsregel

När implementation införs ska tabellerna länka till verkliga kodfiler och relevanta
tester eller dokumenterad manuell verifiering. Ange vad som faktiskt verifierats;
markera inte ett krav uppfyllt enbart för att en klass eller ett test har skapats.
