# Verifiering och körning

Ursprunglig verifiering på Boromir: 2026-10-07.
Lokal överföring och verifiering: 2026-10-08, se [rapport](local-transfer.md).
Aktuell leverans: 2026-10-08, Forever-beta nivå 30 och importstöd inför nivå 60.

## Genomförda kontroller

- Gränssnitt, exempelmetadata, status och felmeddelanden är nu på engelska.
  Den isolerade UI-kontrollen och PNG-renderingen har körts på den översatta vyn.

- Hela solutionen byggd i Release för x64 utan varningar eller fel. WinUI-utkastet och isolerad verifieringsvariant är också byggda utan varningar eller fel.
- Alla **39 konsolkontroller** godkända: 17 äldre katalog-/domän-/tracking-/
  lagringsscenarier, nio `CharacterScenarios`, sex `CatalogContextScenarios`
  och sju `ForeverCatalogScenarios`.
- Karaktärsscenarierna verifierar 54 versions-/klass-/specval, tre olika listor
  efter omstart, delat ägande med separata rekommendations-ID:n och suffix,
  karaktärs-/versionsisolering, migration, aktivt val, sparfel, avbrott och låst
  workspace. Se [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs).
- Kontextscenarierna verifierar sparade nivå 30-/60-uppsättningar för alla tre
  specs, återläsning, äldre filer utan katalogval, avvisade versionsval,
  tvåhandsvapen/offhand, unique över suffixvarianter och sparfel vid nivåbyte
  eller borttaget ägande. Se [CatalogContextScenarios](../BISTracker.Checks/Scenarios/CatalogContextScenarios.cs).
- Katalog-/importscenarierna verifierar alla 27 riktiga nivå 30-kataloger,
  metadata och defaultpolicyn dungeons/quests, suffixnamn, kompletterande
  rekommendationskällor, nivå 60-import och uppdatering av samma levande
  katalogprovider. Ogiltig JSON, schema, kontext, nivåkrav, vapenhand och
  duplicerade ID:n avvisas; blandade giltiga/ogiltiga batcher, avbrott och
  tom import bevarar tidigare filer. Se [ForeverCatalogScenarios](../BISTracker.Checks/Scenarios/ForeverCatalogScenarios.cs).
- De 27 Forever-katalogerna innehåller **1 020 placeringsrader** med
  dungeons/quests i defaultvyn; granskningsdata innehåller **1 394 rader**
  före källfiltrering. Alternativa placeringar och flera guideval ingår i
  antalet; det är inte lika många verkliga items eller en rangordnad optimal
  utrustningsuppsättning. [Samlad leverans](forever-level30-catalogs.md),
  [hybrider](forever-level30-hybrids.md), [casters](forever-level30-casters.md)
  och [övriga klasser](forever-level30-physical.md) redovisar rekommendations-
  och metadataunderlag, quest-/fraktionsvillkor, ikonkontroller och uteslutna källfel.
- Appen har startats i en isolerad verifieringsvariant med minneslagring.
  Inga sparade spelarframsteg läses eller skrivs av den kontrollen.
- UI-kontrollen använder samma 17 riktiga items som appen och verifierar initialt
  tillstånd, sammanfattningar, erhållen/utrustad-regler, filter, sökning på item/slot/
  anskaffning, tomt resultat, källänkar, synliga ikonbilder och trasig bild med fallback.
  Radåteranvändning, suffixvillkor och fraktionsbundna questvillkor kontrolleras också.
- Den aktuella UI-kontrollen verifierar en riktig Forever Mage-katalog på
  nivå 30, det uttryckliga tomma nivå 60-valet, återställd utrustning vid byte
  tillbaka och att faktisk väljare återställs efter sparfel. Importuppdatering
  i samma aktiva kontext verifieras med ett **TEST ONLY**-nivå 60-pack i en
  temporär katalog och minneslagrade framsteg. Detta testpaket är inte produktdata.
  [Aktuell rapport](previews/forever-level30-ui-checks.txt),
  [nivå 30-vy](previews/forever-level30-overview.png) och
  [nivå 60 utan data](previews/forever-level60-pending.png).
- Projektberoenden, filplacering och Markdown-spårning har granskats.

Renderingen använder appens eget visuella träd via
[RenderTargetBitmap](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.rendertargetbitmap.renderasync).
Samtliga verifieringsbilder visar isolerade markeringar, inte sparade spelarframsteg.
Ytterligare granskade renderingar av Classic-listan: [suffixmål](previews/phase1-suffix.png),
[questvillkor](previews/phase1-quest.png) och [saknad bild](previews/phase1-icon-fallback.png).

## Historiska verifieringar

Boromirs utkast från 2026-10-07 renderades till [PNG](previews/draft-overview.png)
och granskades visuellt; checkboxarnas bredd och listans utrymme korrigerades.
[Ursprunglig UI-rapport](previews/ui-checks.txt). Dess bild visar sex erhållna
och tre utrustade items i isolerat verifieringstillstånd.

Karaktärsleveransen tidigare 2026-10-08 verifierade 26 konsolscenarier och
klass-/specval innan Forever-katalogerna infördes. Dess tomma Forever-vy
beskriver dåvarande dataläge. [Historisk rapport](previews/characters-ui-checks.txt),
[översikt](previews/characters-overview.png), [dåvarande Forever-vy](previews/characters-forever.png)
och [nykaraktärsdialog](previews/characters-create.png).

## Bygg och starta

Från projektroten på Windows med .NET SDK och WinUI-byggberoenden:

```powershell
dotnet build BISTracker.slnx -c Release -p:Platform=x64 -p:WindowsPackageType=None
dotnet run --project BISTracker.Checks/BISTracker.Checks.csproj -c Release
```

Öppna BISTracker.slnx i Visual Studio, välj BISTracker.Presentation och x64
för normal utveckling. Den opaketerade Release-builden kan även startas direkt:

```powershell
& ./BISTracker.Presentation/bin/x64/Release/net8.0-windows10.0.19041.0/BISTracker.Presentation.exe
```

Normal app sparar tillstånd i `%LOCALAPPDATA%/BISTracker/characters-v1.json`.
Första start importerar giltiga äldre Holy Priest-framsteg en gång, eller
skapar “My Priest” med tomt framsteg om den äldre filen saknas. Originalet
`holy-priest-classic-phase1-progress.json` och `draft-progress.json` bevaras.
Demoägande importeras inte. Felaktig framstegsfil rapporteras och skrivs inte
tyst över. Se [migrationsreglerna](characters-and-loadouts.md).

Normal app importerar granskade katalogpack till
`%LOCALAPPDATA%/BISTracker/Catalogs`. Import kopierar en komplett validerad
batch till en ny underkatalog, bevarar källfiler och tillåter inte att ett
befintligt katalog-ID ersätts. Nivå 60-data måste ha egen källgranskad kontext;
de inbyggda nivå 30-listorna etiketteras inte om automatiskt.

## Återskapa den isolerade UI-kontrollen

```powershell
dotnet build BISTracker.Presentation/BISTracker.Presentation.csproj -t:Rebuild -p:Platform=x64 -p:RuntimeIdentifier=win-x64 -p:WindowsPackageType=None -p:EnableDraftPreview=true -p:OutputPath=bin/verification/
$previewDirectory = Join-Path $env:TEMP ('BISTracker-preview-' + [guid]::NewGuid().ToString('N'))
& ./BISTracker.Presentation/bin/verification/BISTracker.Presentation.exe --draft-preview $previewDirectory
```

Verifieringskod kompileras endast med EnableDraftPreview=true. Appen stänger
verifieringsfönstret efter kontrollen. Normal build inkluderar inte den koden.
Separat utdatakatalog undviker att en annan tråds MSIX-build ersätter
verifieringsprogrammets filer före start.

## Praktiska begränsningar

- Classic Holy Priest har 17 riktiga, guidebaserade huvudval. Bonecreeper Stylus beskrivs som alternativ
  men är inte ett eget trackingmål; full questkedjerevision återstår.
- Lokala karaktärer i Classic/Forever, med nio klasser och tre specs vardera.
  Forever har granskade nivå 30-betaalternativ för samtliga 27 specs; övriga
  Classic-specs saknar fortfarande granskade kataloger.
  Inget konto, molnsynk eller spelintegration ingår. Flera appinstanser som
  skriver samma framsteg eller importerar till samma katalog samtidigt är
  inte samordnade.
- Unique-/tvåhands-/offhand-regler och alternativa ring-/trinketplaceringar
  är implementerade och verifierade. Antalet ägda exemplar modelleras inte;
  ett shared-owned-värde bevisar inte att spelaren har två kopior av ett item.
- Betaguiderna har källluckor och föränderliga rekommendationer. Listorna är
  guidebaserade alternativ enligt vald källpolicy, inte ett beräknat optimalt
  set. Fulla questkedjor och klientkontroll för varje anskaffningsväg återstår.
- Ikonbilder kräver nätverk vid visning. Katalog och tracking fungerar utan nätverk;
  en saknad ikon får en platsmarkering. Externa webbläsarklick är inte automatiskt körda.
- Rendering och ViewModel-beteenden är kontrollerade. Full manuell mus- och
  tangentbordsgranskning, olika DPI/fönsterstorlekar och MSIX-distribution återstår.
- Ingen verklig Forever-nivå 60-katalog finns i leveransen. Nivå 60-valet visar
  uttrycklig datastatus; importen verifieras med fiktiva kontrollpack. Betans
  nivå 30-kataloger används inte som slutlig pre-raid-data för nivå 60.
