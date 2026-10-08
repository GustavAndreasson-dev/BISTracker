# Krav och koppling till kod

Statusdatum: 2026-10-08. Beslutade krav kommer från Chefen.
Acceptanskriterier och detaljer markerade som förslag behöver preciseras tillsammans.

Aktuell bas är appversion 1.0.0 med 49 godkända beteendekontroller och lokalt
verifierat Windows x64-ZIP. Börja med [beta 2-överlämningen](beta2-handoff.md).
Beta 2-kraven REQ-013–REQ-015 är beslutade; REQ-014 och REQ-015 är implementerade
och verifierade lokalt, REQ-013 är inte implementerat;
versionsnumret är ännu inte beslutat.

## Beslutade produktkrav

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-001 | Visa pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla). | [Riktig katalog](../BISTracker.Infrastructure/Catalog/ClassicPhaseOneBisCatalog.cs) med 17 huvudval kopplad i App; [vyn](../BISTracker.Presentation/MainWindow.xaml) visar bilder, anskaffning, villkor och item-/guidelänkar. | Katalogkontroller och renderad UI-kontroll godkända. Guidebaserat fas 1-urval enligt phase1-items.md; full questkedjerevision och verifiering i spelet återstår. |
| REQ-002 | Tracka både erhållna och utrustade items mot pre-raid BiS. | [CharacterLoadouts](../BISTracker.Domain/Tracking/CharacterLoadouts.cs), [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs), [JSON-lagring](../BISTracker.Infrastructure/Persistence/JsonWorkspaceRepository.cs) och [ViewModel](../BISTracker.Presentation/Features/Tracking/ViewModels/TrackerViewModel.cs). | Tracking-/lagringskontroller, återläsning med riktig katalog, delat ägande, suffixidentitet och isolerad UI-kontroll godkända. Legacy- och demofiler bevaras. |
| REQ-003 | Använd fas 1 och endast dungeons/quests för BiS-urvalet. | [Produktkatalog](../BISTracker.Infrastructure/Catalog/Data/holy-priest-classic-phase1.json) och synlig kontext; endast Dungeon/Quest tillåts av katalogläsaren. | Källor och fasetiketter granskade. UBRS, of Healing och fraktions-/Raid-questvillkor visas. Kontroll av de 17 katalogmålen godkänd; inget crafting/world-drop-val från originalguiden ingår. |
| REQ-004 | Ha appens gränssnitt på engelska. | [Presentation](../BISTracker.Presentation/MainWindow.xaml), ViewModels, exempelmetadata och egna lagringsfel översatta. | Release-build, textgranskning och isolerad engelsk UI-preview godkända. Manuella tillgänglighets-/DPI-kontroller återstår. |

## Forever och separata speclistor

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-005 | Lägg till WoW Forever som nästa separat valbara spelversion efter genomförbarhetsutredning. | [CharacterDialog](../BISTracker.Presentation/Features/Characters/Views/CharacterDialog.cs) väljer version; [CharacterCatalog](../BISTracker.Infrastructure/Catalog/CharacterCatalog.cs) laddar separata Forever beta nivå 30-kataloger för alla 27 specs. **Implementerat för nivå 30.** | Versionsisolering och alla 27 kataloger verifierade i [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs) och [ForeverCatalogScenarios](../BISTracker.Checks/Scenarios/ForeverCatalogScenarios.cs), samt Forever-katalog och nivåval i renderad UI. Ingen Vanilla-katalog används som Forever-data. |
| REQ-006 | Om Forever kan stödjas, utöka till alla klasser och specs. | [CharacterDefinition](../BISTracker.Domain/Game/CharacterDefinition.cs) anger nio klasser/27 specs; alla har aktiva nivå 30-kataloger. [CharacterLoadouts](../BISTracker.Domain/Tracking/CharacterLoadouts.cs) skyddar unique- och tvåhandsregler. **Implementerat med källbelagda guidealternativ**, inte en fullständigt optimerad ranking av ett helt utrustningsset. | 54 version/klass/spec-kombinationer och alla 27 Forever-kataloger kontrollerade. Källunderlag, täckning och luckor redovisas i [katalograpporten](forever-level30-catalogs.md); utrustningsregler verifierade i [CatalogContextScenarios](../BISTracker.Checks/Scenarios/CatalogContextScenarios.cs). |
| REQ-007 | Ha tre bestående, separata listor per karaktär, en per spec; gemensamt ägande och separat utrustning är bekräftat. | [CharacterLoadouts](../BISTracker.Domain/Tracking/CharacterLoadouts.cs), [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs), [JsonWorkspaceRepository](../BISTracker.Infrastructure/Persistence/JsonWorkspaceRepository.cs) och val i UI. **Listfunktionen implementerad**, även tomma listor när katalog saknas. | Tre olika listor återlästa efter omstart; delat ägande, utrustningsisolering, karaktärs-/versionsisolering, migration, sparfel och återställda UI-val verifierade. |
| REQ-008 | Skaffa Forever nivå 30-kataloger för alla 27 specs. | [27 inbyggda JSON-kataloger](../BISTracker.Infrastructure/Catalog/Data/Forever/level30) läses av [ForeverCatalogReader](../BISTracker.Infrastructure/Catalog/ForeverCatalogReader.cs) och visas via CharacterCatalog. 1 655 råa alternativrader, 1 635 efter godkänd Dungeon/Quest/Crafting-policy och handkrav. **Aktiverat och slotgranskat.** | Alla 27 kataloger och 54 fraktionsset verifierade; [katalograpporten](forever-level30-catalogs.md), [releasekontrollen](data/forever-slot-release-audit.json) och [vittneskontrollen](data/forever-slot-witness-integration.json). Alternativen är inte en egen optimal ranking. |
| REQ-009 | Förbered nivå 60-import när källgranskad data blir tillgänglig, utan att skriva över nivå 30-listorna. | [ICatalogPackImporter](../BISTracker.Application/Catalog/ICatalogPackImporter.cs), [CatalogPackImporter](../BISTracker.Infrastructure/Catalog/CatalogPackImporter.cs), ForeverCatalogReader, CatalogSet, CharacterTrackerService och ViewModel ger atomisk import, nivåval och separata utrustningslistor. CharacterCatalog upptäcker import utan omstart. **Implementerat; verklig nivå 60-data saknas.** | 49 beteendekontroller och renderad UI-kontroll godkända, inklusive faktisk Rogue-diskpersistens över tre specs och nivå 30/60. Importbatcher, dubbletter, avbrott, bevarade original, nivåisolering och sparfel verifieras. Ett glest testpack förblir synligt ofullständigt; ändrad metadata uppdateras även med samma rad-ID:n. Testdata ingår inte i produkten; [importkontraktet](catalog-contexts.md). |

## Appens beta 2

Beställt av Chefen 2026-10-08 enligt [DEC-016](decisions.md#dec-016--scope-för-appens-beta-2).

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-013 | Visa pre-raid BiS i WoW Classic (Vanilla) fas 1, nivå 60, för alla nio klasser och 27 specs. Källpolicy: Dungeon, Quest och Crafting. Holy Priest utökas och behåller sina 17 befintliga rad-ID:n. | **Ej påbörjat.** | Acceptans: varje spec har en källbelagd katalog eller visas uttryckligen som ofullständig; inga påhittade items; slot- och fraktionsgranskning som för Forever; en sparfil från 1.0.0 med Holy Priest-framsteg laddas oförändrad. |
| REQ-014 | Spelaren kan byta namn på en karaktär. Version och klass kan inte ändras. | `RenameAsync` i [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs) via [ICharacterTrackerService](../BISTracker.Application/Characters/ICharacterTrackerService.cs): samma `ValidateName` som skapande före mutation, endast `Name` ändras. [CharacterDialog](../BISTracker.Presentation/Features/Characters/Views/CharacterDialog.cs) i namnbytesläge med låst version/klass, menyn "Rename…" i [MainWindow](../BISTracker.Presentation/MainWindow.xaml) och [TrackerViewModel](../BISTracker.Presentation/Features/Tracking/ViewModels/TrackerViewModel.cs). **Implementerat.** | [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs): namnbyte trimmas, återläses efter omstart och lämnar ID, version, klass, spec, ägande, utrustning, aktivt val och andra karaktärer byte-för-byte oförändrade i JSON; tomma, för långa (41) och kontrolltecken-namn samt okänt ID avvisas utan filändring; 40 tecken godtas; sparfel lämnar fil och returnerat tillstånd oförändrat. Isolerad UI-preview ([rapport](previews/beta2-characters-ui-checks.txt)): dialog, namnbyte med bevarat framsteg och väljare, sparfel behåller namn och visar fel. Menyklick och dialogknappar är inte automatiskt körda. |
| REQ-015 | Spelaren kan ta bort en karaktär permanent efter en bekräftelse, även den sista. Utan karaktärer visar appen ett tomt läge med möjlighet att skapa en karaktär. | `DeleteAsync` i [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs); aktiv ersätts av första kvarvarande, annars tomt ID. Schema 1 tillåter noll karaktärer i [JsonWorkspaceRepository](../BISTracker.Infrastructure/Persistence/JsonWorkspaceRepository.cs); legacy-import endast när filen saknas. Tomt snapshot `TrackerSnapshot.NoCharacters` i [TrackerSnapshot](../BISTracker.Application/Tracking/TrackerSnapshot.cs). [DeleteCharacterDialog](../BISTracker.Presentation/Features/Characters/Views/DeleteCharacterDialog.cs), menyn "Delete…" och tomt läge med "Create character" i [MainWindow](../BISTracker.Presentation/MainWindow.xaml). **Implementerat.** | [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs): borttagning av inaktiv och aktiv karaktär med övriga karaktärers JSON oförändrad och återläsning; sista karaktären ger schema 1 med tom lista och tomt ID, återläses utan att legacy läses eller “My Priest” återskapas; operationer i tomt läge avvisas utan filändring; skapande från tomt läge; sparfel vid borttagning (även sista) lämnar fil och tillstånd oförändrat. [PersistenceScenarios](../BISTracker.Checks/Scenarios/PersistenceScenarios.cs): tom workspace kräver tomt aktivt ID, ogiltiga tomma filer bevaras, schema 1-filer i 1.0.0- och äldre form läses oförändrade. Isolerad UI-preview: bekräftelsedialog, sparfel behåller listan, borttagning av inaktiv/aktiv/sista, [tomt läge](previews/beta2-characters-empty.png), återläsning och skapande från tomt läge. Menyklick, dialogknappar och normal app med diskprofil är inte körda; app 1.0.0 mot tom fil är inte provad. |

## Beslutade tekniska krav och riktning

REQ-010, beställt 2026-10-08: varje relevant utrustningsslot ska vara ett mål
med källbelagda itemalternativ. Dubbletter och alternativ får inte räknas som
extra slots. Samtliga specs ska kontrolleras för en möjlig komplett kombination
före delning. Implementationsgräns: [EquipmentPlan](../BISTracker.Domain/Equipment/EquipmentPlan.cs),
[EquipmentPlanScenarios](../BISTracker.Checks/Scenarios/EquipmentPlanScenarios.cs)
och [releasegranskning](slot-catalog-correctness.md). **Implementerat och verifierat**:
27 kataloger, 54 kompletta fraktionsset, 49 beteendekontroller och renderad UI.
Rogue visar 17 mål; två alternativ i samma slot ger ett fyllt mål. Unique,
fysisk variantkapacitet, fraktion och handplan förhindrar falsk fullständighet.
Chefens godkännande omfattar crafting i Forever; Classic bevarar sitt urval.
Lokalt distributionstest för portabel Windows x64 är godkänt; prov på ren
mottagardator återstår enligt [distributionsrapporten](distribution.md).

REQ-011, beställt 2026-10-08: paketera version 1 och kör distributionsprov.
[Publiceringsverktyget](../tools/distribution/Publish-PortableRelease.ps1)
levererar en komplett portabel Windows x64-programkatalog med båda runtimes.
[Distributionsprovet](../tools/distribution/Test-PortableRelease.ps1) verifierar
ZIP-manifest, samma lager-DLL:er i checkhostet, 49 beteendekontroller,
27 kataloger och verklig UI-sparning/omstart i isolerad profil.
Lokal verifiering är genomförd. Ren dator, äldre Windows och signering är
inte verifierade; slutpaketets metadata redovisas i distributionsrapporten.

REQ-012, beställt 2026-10-08: aktualisera samtliga projektspecifika
Markdown-filer så en annan agent kan fortsätta inför appens beta 2.
[Överlämningen](beta2-handoff.md) samlar verifierad bas, rapporter,
kodgränser, körkommandon, data-/katalogregler och öppna frågor. README och
AGENTS (nu CLAUDE.md) leder dit; områdesdokument anger sin aktuella respektive historiska
roll. Acceptans: alla tidigare 23 Markdown-filer ska vara granskade och
uppdaterade, lokala länkar giltiga och ingen kod/katalog/spelardata ändrad.
**Verifierat:** 23 befintliga dokument uppdaterade och 24 Markdown-filer
kontrollerade inklusive överlämningen; 270 lokala länkar utan fel.
Paketets metadata och checksumma stämmer med den sparade PASS-rapporten.
Dokumentgranskningen ersätter inte nya beta 2-tester.

| ID | Krav eller riktning | Implementation | Verifiering |
| --- | --- | --- | --- |
| ARC-001 | Använd DDD-struktur och en domänmodell för verksamhetens regler. | Fyra lager; CharacterLoadouts skyddar reglerna för tre listor. CharacterProgress validerar legacy-data. Se [arkitektur](architecture.md). | Projektberoenden och regler granskade; domän och användningsfall kontrolleras utan WinUI. |
| ARC-002 | Följ SOLID-principer. | Separata ansvar och små applikationskontrakt: ICharacterCatalog, IWorkspaceRepository, ICharacterTrackerService och [ICatalogPackImporter](../BISTracker.Application/Catalog/ICatalogPackImporter.cs). JSON-/källmetadata valideras i Infrastructure; domänens utrustningsregler kräver inget filsystem. Appens composition root kopplar importimplementationen. Legacy-kontrakt bevarade. | Katalog och repository utbytta i beteendekontroller; importbatcher, fel och levande kataloguppdatering verifierade i ForeverCatalogScenarios. Fortsatt granskningskrav vid varje ändring. |
| ARC-003 | Håll Markdown-dokumentation som kan jämföras med koden. | CLAUDE.md och docs, inklusive denna kodspårning. | Nuläge, projektgränser och källfiler har jämförts mot koden. |
| ARC-004 | Ha god mappstruktur inom varje projekt. | Kod indelad enligt [strukturkartan](project-structure.md). | Faktiska filplaceringar granskade; områden för Equipment, Game, Tracking, Catalog, Persistence och Features finns. |
| DIR-001 | Stöd fler expansioner senare. | Classic och Forever är separata val enligt REQ-005. | Versionsisolering verifierad; nya versioners data kräver egen granskning. |

## Verifierade beteenden i första fungerande flödet

- REQ-001: spelaren kan se rekommenderad utrustning per utrustningsplats, med ursprungskälla för items och en förklarad rekommendationsgrund.
- REQ-002: spelaren kan markera ett rekommenderat item som erhållet eller utrustat och återställa markeringarna. Markeringarna finns kvar efter omstart. Utrustad innebär erhållen; att avmarkera erhållen tar också bort utrustningsmarkeringen.
- Kontexten för spelversion och tillgänglighet är synlig och entydig.

Dessa beteenden är implementerade inom de befintliga kraven och kontrolleras
av konsolscenarier samt UI-/distributionsprov enligt [verifiering](verification.md).
De innebär inga nya produktfunktioner för beta 2.

## Uppdateringsregel

När implementation införs ska tabellerna länka till verkliga kodfiler och relevanta
tester eller dokumenterad manuell verifiering. Ange vad som faktiskt verifierats;
markera inte ett krav uppfyllt enbart för att en klass eller ett test har skapats.
