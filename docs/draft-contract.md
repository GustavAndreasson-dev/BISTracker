# Första utkastet — arbetsfördelning och kontrakt

Datum: 2026-10-07. Chefen har beställt plan, separata Codex-trådar med
instruktioner och ett första apputkast. Tracking omfattar både erhållna och
utrustade items.

## Utkastets omfattning

Fortsätt i befintlig WinUI-app. Visa utrustningsplatser, sökning, sammanfattning
och kontroller för erhållen/utrustad. Spara framsteg lokalt i JSON för en
demokaraktär. Visa tydligt att katalogen är exempeldata, inte en verifierad
BiS-lista. Fas 1 och endast dungeons/quests har därefter fastställts. Riktig data granskas separat.

Arbetsregel för utkastet: utrustad innebär erhållen, en rekommendation per
utrustningsplats kan vara utrustad, och avmarkering av erhållen tar bort dess
utrustningsmarkering. Avmarkering av utrustad behåller erhållen.

## Filansvar

- Domäntråd: BISTracker.Domain/ och BISTracker.Application/.
- Infrastrukturtråd: BISTracker.Infrastructure/ och BISTracker.Checks/.
- Datatråd: endast docs/data-research.md.
- Huvudtråd: BISTracker.Presentation/, BISTracker.slnx och gemensam dokumentation.

Arbeta i den delade lokala projektmappen. Ändra inte andras filer, solution,
gemensamma dokument eller Git-historik. Rapportera filförändringar, kontroller
och begränsningar i trådens slutmeddelande. Huvudtråden integrerar och granskar.

## Gemensamma C#-kontrakt

Alla projekt utom Presentation riktar sig mot net8.0 med nullable och implicit usings.
Placera filer i mappar enligt project-structure.md. Publika namespace i kontraktet behålls.
Använd följande publika typer och signaturer så att parallella bidrag passar ihop.

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
