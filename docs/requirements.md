# Krav och koppling till kod

Statusdatum: 2026-10-08. Beslutade krav kommer från Chefen.
Acceptanskriterier och detaljer markerade som förslag behöver preciseras tillsammans.

## Beslutade produktkrav

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-001 | Visa pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla). | [Riktig katalog](../BISTracker.Infrastructure/Catalog/ClassicPhaseOneBisCatalog.cs) med 17 huvudval kopplad i App; [vyn](../BISTracker.Presentation/MainWindow.xaml) visar bilder, anskaffning, villkor och item-/guidelänkar. | Katalogkontroller och renderad UI-kontroll godkända. Guidebaserat fas 1-urval enligt phase1-items.md; full questkedjerevision och verifiering i spelet återstår. |
| REQ-002 | Tracka både erhållna och utrustade items mot pre-raid BiS. | [CharacterLoadouts](../BISTracker.Domain/Tracking/CharacterLoadouts.cs), [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs), [JSON-lagring](../BISTracker.Infrastructure/Persistence/JsonWorkspaceRepository.cs) och [ViewModel](../BISTracker.Presentation/Features/Tracking/ViewModels/TrackerViewModel.cs). | Tracking-/lagringskontroller, återläsning med riktig katalog, delat ägande, suffixidentitet och isolerad UI-kontroll godkända. Legacy- och demofiler bevaras. |
| REQ-003 | Använd fas 1 och endast dungeons/quests för BiS-urvalet. | [Produktkatalog](../BISTracker.Infrastructure/Catalog/Data/holy-priest-classic-phase1.json) och synlig kontext; endast Dungeon/Quest tillåts av katalogläsaren. | Källor och fasetiketter granskade. UBRS, of Healing och fraktions-/Raid-questvillkor visas. Kontroll av de 17 katalogmålen godkänd; inget crafting/world-drop-val från originalguiden ingår. |
| REQ-004 | Ha appens gränssnitt på engelska. | [Presentation](../BISTracker.Presentation/MainWindow.xaml), ViewModels, exempelmetadata och egna lagringsfel översatta. | Release-build, textgranskning och isolerad engelsk UI-preview godkända. Manuella tillgänglighets-/DPI-kontroller återstår. |

## Beställd nästa utökning

| ID | Krav | Implementation | Verifiering |
| --- | --- | --- | --- |
| REQ-005 | Lägg till WoW Forever som nästa separat valbara spelversion efter genomförbarhetsutredning. | [CharacterDialog](../BISTracker.Presentation/Features/Characters/Views/CharacterDialog.cs) väljer version; [CharacterCatalog](../BISTracker.Infrastructure/Catalog/CharacterCatalog.cs) isolerar kontext. **Versionsstöd implementerat; Forever-BiS återstår.** | Alla versions-/klass-/specval kontrollerade i [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs), Forever Mage i renderad UI. Ingen Vanilla-katalog används som Forever-data. |
| REQ-006 | Om Forever kan stödjas, utöka till alla klasser och specs. | [CharacterDefinition](../BISTracker.Domain/Game/CharacterDefinition.cs) anger nio klasser/27 specs; skapande och specval fungerar. **Struktur/UI implementerade; kompletta kataloger och utökade utrustningsregler återstår.** | 54 version/klass/spec-kombinationer kontrollerade. Specnamn och databegränsningar är källbelagda i [rapporten](characters-and-loadouts.md). |
| REQ-007 | Ha tre bestående, separata listor per karaktär, en per spec; gemensamt ägande och separat utrustning är bekräftat. | [CharacterLoadouts](../BISTracker.Domain/Tracking/CharacterLoadouts.cs), [CharacterTrackerService](../BISTracker.Application/Characters/CharacterTrackerService.cs), [JsonWorkspaceRepository](../BISTracker.Infrastructure/Persistence/JsonWorkspaceRepository.cs) och val i UI. **Listfunktionen implementerad**, även tomma listor när katalog saknas. | Tre olika listor återlästa efter omstart; delat ägande, utrustningsisolering, karaktärs-/versionsisolering, migration, sparfel och återställda UI-val verifierade. |
| REQ-008 | Skaffa Forever nivå 30-kataloger för alla 27 specs. | Beställt 2026-10-08. Tre datatrådar granskar varsin grupp om nio specs; import/UI kopplas efter granskning. | Pågående; inga nya kataloger markerade aktiverade ännu. |
| REQ-009 | Förbered nivå 60-import när källgranskad data blir tillgänglig, utan att skriva över nivå 30-listorna. | [CatalogSet](../BISTracker.Domain/Game/CatalogSet.cs) och arkiverade listor i CharacterTrackerService. Nivåval/import i UI återstår. | Tre nya [kontextscenarier](../BISTracker.Checks/Scenarios/CatalogContextScenarios.cs) godkända: nivåisolering, äldre JSON, gemensamt ägande, unique/tvåhandsregler. |

## Beslutade tekniska krav och riktning

| ID | Krav eller riktning | Implementation | Verifiering |
| --- | --- | --- | --- |
| ARC-001 | Använd DDD-struktur och en domänmodell för verksamhetens regler. | Fyra lager; CharacterLoadouts skyddar reglerna för tre listor. CharacterProgress validerar legacy-data. Se [arkitektur](architecture.md). | Projektberoenden och regler granskade; domän och användningsfall kontrolleras utan WinUI. |
| ARC-002 | Följ SOLID-principer. | Separata ansvar, ICharacterCatalog/IWorkspaceRepository och ICharacterTrackerService för aktuella gränser; legacy-kontrakt bevarade. | Katalog och repository utbytta i beteendekontroller. Fortsatt granskningskrav vid varje ändring. |
| ARC-003 | Håll Markdown-dokumentation som kan jämföras med koden. | AGENTS.md och docs, inklusive denna kodspårning. | Nuläge, projektgränser och källfiler har jämförts mot koden. |
| ARC-004 | Ha god mappstruktur inom varje projekt. | Kod indelad enligt [strukturkartan](project-structure.md). | Faktiska filplaceringar granskade; områden för Equipment, Game, Tracking, Catalog, Persistence och Features finns. |
| DIR-001 | Stöd fler expansioner senare. | Classic och Forever är separata val enligt REQ-005. | Versionsisolering verifierad; nya versioners data kräver egen granskning. |

## Föreslagna acceptanskriterier för första fungerande flödet

- REQ-001: spelaren kan se rekommenderad utrustning per utrustningsplats, med ursprungskälla för items och en förklarad rekommendationsgrund.
- REQ-002: spelaren kan markera ett rekommenderat item som erhållet eller utrustat och återställa markeringarna. Markeringarna finns kvar efter omstart. Utrustad innebär erhållen; att avmarkera erhållen tar också bort utrustningsmarkeringen.
- Kontexten för spelversion och tillgänglighet är synlig och entydig.

Dessa detaljer är förslag, inte ytterligare beslutade produktkrav.

## Uppdateringsregel

När implementation införs ska tabellerna länka till verkliga kodfiler och relevanta
tester eller dokumenterad manuell verifiering. Ange vad som faktiskt verifierats;
markera inte ett krav uppfyllt enbart för att en klass eller ett test har skapats.
