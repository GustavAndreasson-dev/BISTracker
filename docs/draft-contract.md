# Historiskt första utkast — arbetsfördelning och kontrakt

**Historiskt dokument från 2026-10-07, inte nya arbetsuppgifter.** Inför beta 2
är [beta2-handoff.md](beta2-handoff.md) startpunkten. Den tidigare filfördelningen
och signaturlistan nedan får inte användas för att återskapa utkastet eller
återuppta gamla agentuppdrag. Nya beta 2-funktioner är ännu inte beslutade.

Datum: 2026-10-07. Chefen har beställt plan, separata Codex-trådar med
instruktioner och ett första apputkast. Tracking omfattar både erhållna och
utrustade items.

Detta var kontraktet för första utkastet. SampleBisCatalog och den äldre
TrackerService/CharacterProgress-vägen finns kvar för regressioner och
legacy-validering; de styr inte normal produktions-UI.

Levererad 1.0.0 från kod `e25c134` och dokumentation `043d859` använder
CharacterCatalog, CharacterTrackerService och JsonWorkspaceRepository:
Classic Holy Priest och 27 Forever30-kataloger med crafting, 54 granskade
fraktionsset, gemensamt boolägande per itemvariant och tre separata speclistor
per katalogset. Schema1-filen är characters-v1.json i LocalAppData, eller en
absolut BISTRACKER_DATA_DIRECTORY-testprofil. Gamla dubbletter och borttagna
itemreferenser ger fel med originalfilen bevarad. Verklig nivå60-Launch-data
saknas; importen är verifierad med isolerade TEST ONLY-fixturer.
Aktuellt kontrakt finns i [arkitekturen](architecture.md),
[listmodellen](characters-and-loadouts.md) och [katalogkontexterna](catalog-contexts.md).

## Historisk omfattning

Utkastet byggdes i den befintliga WinUI-appen med utrustningsplatser, sökning,
sammanfattning, erhållen/utrustad och lokal JSON för en demokaraktär.
Exempeldata markerades uttryckligen. Därefter fastställdes Classic fas1 med
endast dungeons/quests och ersattes produktens data med granskade kataloger.

Arbetsregel för utkastet: utrustad innebär erhållen, en rekommendation per
utrustningsplats kan vara utrustad, och avmarkering av erhållen tar bort dess
utrustningsmarkering. Avmarkering av utrustad behåller erhållen.

## Historiska filansvar

Denna fördelning gällde utkastets genomförande. Huvudtråden tilldelar nya
ansvar enligt aktuellt beslutat scope; inga filer reserveras genom listan nedan.

- Domäntråd: BISTracker.Domain/ och BISTracker.Application/.
- Infrastrukturtråd: BISTracker.Infrastructure/ och BISTracker.Checks/.
- Datatråd: endast docs/data-research.md.
- Huvudtråd: BISTracker.Presentation/, BISTracker.slnx och gemensam dokumentation.

Arbetet utfördes i den delade lokala projektmappen med avgränsade filansvar.
Huvudtråden integrerade bidrag och samordnade granskning och Git.

## Historiska C#-kontrakt

Alla projekt utom Presentation riktade sig mot net8.0 med nullable och implicit usings.
Följande signaturer var utkastets integrationskontrakt. De är historiska;
Recommendation/BisCatalog/TrackerSnapshot har senare utökats och dagens kod
är auktoritativ för exakt API. [Strukturkartan](project-structure.md) är aktuell.

### BISTracker.Domain

- enum EquipmentSlot: Head, Neck, Shoulder, Back, Chest, Wrist, Hands, Waist, Legs,
  Feet, Finger1, Finger2, Trinket1, Trinket2, MainHand, OffHand, Ranged.
- record GameContext(string Version, string Specialization, string Phase).
- record Recommendation(string Id, EquipmentSlot Slot, string Name, string Source, string Note).
- CharacterProgress är aggregatet för ägande/utrustning. Konkreta interna API:n
  bestäms av domäntråden. Invariants ska vara i Domain, inte dupliceras i UI.

### BISTracker.Application

- record BisCatalog(GameContext Context, IReadOnlyList<Recommendation> Items, bool IsSample).
- record ProgressState(string[] OwnedItemIds, Dictionary<EquipmentSlot, string> EquippedItems).
- record TrackerEntry(Recommendation Item, bool IsOwned, bool IsEquipped).
- record TrackerSnapshot(BisCatalog Catalog, IReadOnlyList<TrackerEntry> Entries).
- IBisCatalog: Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default).
- IProgressRepository: Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default)
  och Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default).
- TrackerService(IBisCatalog catalog, IProgressRepository progress):
  Task<TrackerSnapshot> LoadAsync(CancellationToken cancellationToken = default),
  Task<TrackerSnapshot> SetOwnedAsync(string itemId, bool owned, CancellationToken cancellationToken = default),
  Task<TrackerSnapshot> SetEquippedAsync(string itemId, bool equipped, CancellationToken cancellationToken = default).

TrackerService läser katalog och sparat tillstånd, låter aggregatet utföra ändringen,
sparar och returnerar en ny snapshot. Okända item-ID:n ska avvisas vid mutation.
Persistensfel ska nå UI; en misslyckad sparning får inte rapporteras som lyckad.

### BISTracker.Infrastructure

- SampleBisCatalog : IBisCatalog med parameterlös konstruktor. Exakt 17
  rekommendationer, en per slot, med ID demo-head, demo-neck, demo-shoulder,
  demo-back, demo-chest, demo-wrist, demo-hands, demo-waist, demo-legs, demo-feet,
  demo-finger1, demo-finger2, demo-trinket1, demo-trinket2, demo-mainhand,
  demo-offhand och demo-ranged. Namn är tydligt fiktiva engelska platsnamn,
  exempelvis "Example: head", inga påhittade WoW-fakta. IsSample = true.
- JsonProgressRepository(string filePath) : IProgressRepository. Saknad fil ger
  tomt framsteg. Skriv via temporär fil och ersätt för att skydda föregående data.
  Ogiltig JSON ska rapporteras som läsfel och får inte tyst skrivas över.
- BISTracker.Checks är ett paketfritt konsolprojekt som refererar till lagren och
  kör betydelsefulla kontroller med exit code 1 vid fel, 0 vid framgång.
  Kontrollera ägande/utrustningsregler, byte i samma slot, okända IDs, återläsning
  från JSON, saknad fil och korrupt JSON. Använd isolerad temporär katalog och
  använd inte spelarens riktiga framstegsfil.

## Leveransverifiering och fortsatt arbete

Den levererade produkten har **49 godkända beteendekontroller**, inklusive
tre DistributionScenarios med riktig diskpersistens och absoluta testprofiler.
[1.0.0-rapporten](data/portable-release-verification.json) verifierar exakt ZIP,
kärn-DLL-hashar, normal UI-sparning/omstart och byte av programmapp.
Publiceringen är untrimmed och paketerar .NET samt Windows App SDK/WinUI;
ren mottagardator har ännu inte provats. Dessa resultat ersätter utkastets
begränsade demoverifiering. Fortsatt arbete börjar med Chefens beslut och
överlämningen, inte med implementation av den historiska signaturlistan.
