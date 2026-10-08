# Verifiering och körning

Ursprunglig verifiering på Boromir: 2026-10-07.
Lokal överföring och verifiering: 2026-10-08, se [rapport](local-transfer.md).
Verifierad v1-bas: portabel appversion **1.0.0**, byggd 2026-10-08 från
kodcommit `e25c1345c84dd4c975c208adb35c83ee68b1b88c`. Leveransens dokumentation
registrerades i `043d859`. Dessa är daterade referenser, inte ett påstående
om framtida HEAD. Denna fil redovisar genomförda kontroller och hur nästa
agent kan återskapa dem inför [appbeta2](beta2-handoff.md).
Appbeta2 är nästa testleverans av appen, skild från Forever-katalogernas
spelbeta. Nya funktioner och nästa programversionsnummer är inte beslutade.

## Genomförda kontroller

- Gränssnitt, exempelmetadata, status och felmeddelanden är nu på engelska.
  Den isolerade UI-kontrollen och PNG-renderingen har körts på den översatta vyn.

- Hela solutionen byggd i Release för x64 utan varningar eller fel. WinUI-utkastet och isolerad verifieringsvariant är också byggda utan varningar eller fel.
- Alla **49 konsolkontroller** godkända: 17 äldre katalog-/domän-/tracking-/
  lagringsscenarier, nio `CharacterScenarios`, sex `CatalogContextScenarios`
  och sju `ForeverCatalogScenarios`, sju `EquipmentPlanScenarios` samt tre
  [DistributionScenarios](../BISTracker.Checks/Scenarios/DistributionScenarios.cs).
- Portabel Windows x64-publicering inkluderar båda runtimes, utan trimning
  eller DraftPreview-kod. Den extraherade normala appens checkbox, diskfil,
  omstart och byte av programmapp har testats på Windows 11 Pro 10.0.26200.
  Ordinarie spelarprofil är oförändrad. Det separata publicerade checkhostet
  använder samma Domain/Application/Infrastructure-DLL-hashar och verifierar
  Classic samt alla tre Rogue-specs över nivå 30/60 med verklig JSON-lagring.
  Nivå 60-fixtures är fortfarande enbart testdata. Den sparade
  [v1-rapporten](data/portable-release-verification.json) anger PASS, 517 filer,
  90 273 350 byte, 275 inventerade PE-binärer och noll saknade lokala
  VC-beroenden. Samma ZIP har startats i en ny programmapp; testet bevisar
  inte migration mellan olika programversioner. Se [distribution](distribution.md).
- Karaktärsscenarierna verifierar 54 versions-/klass-/specval, tre olika listor
  efter omstart, delat ägande med separata rekommendations-ID:n och suffix,
  karaktärs-/versionsisolering, migration, aktivt val, sparfel, avbrott och låst
  workspace. Se [CharacterScenarios](../BISTracker.Checks/Scenarios/CharacterScenarios.cs).
- Kontextscenarierna verifierar sparade nivå 30-/60-uppsättningar för alla tre
  specs, återläsning, äldre filer utan katalogval, avvisade versionsval,
  tvåhandsvapen/offhand, unique över suffixvarianter och sparfel vid nivåbyte
  eller borttaget ägande. Se [CatalogContextScenarios](../BISTracker.Checks/Scenarios/CatalogContextScenarios.cs).
- Katalog-/importscenarierna verifierar alla 27 riktiga nivå 30-kataloger,
  metadata och Forever-policyn dungeons/quests/crafting, suffixnamn, kompletterande
  rekommendationskällor, nivå 60-import och uppdatering av samma levande
  katalogprovider. Ogiltig JSON, schema, kontext, nivåkrav, vapenhand och
  duplicerade ID:n avvisas; blandade giltiga/ogiltiga batcher, avbrott och
  tom import bevarar tidigare filer. Se [ForeverCatalogScenarios](../BISTracker.Checks/Scenarios/ForeverCatalogScenarios.cs).
- De 27 Forever-katalogerna innehåller **1 635 alternativa placeringsrader**
  efter D/Q/Crafting- och handfiltret; rådata innehåller **1 655 rader**.
  UI räknar slotmål, aldrig alternativrader. [Releasekontrollen](data/forever-slot-release-audit.json)
  passerar med noll ofullständiga kataloger och noll okända questfraktioner.
  [Integrationskontrollen](data/forever-slot-witness-integration.json) verifierar
  54 konkreta fraktionsset mot faktiskt aktiva items, slots, unique, fysisk
  variantkapacitet, handkrav och granskade professionskrav. Källgranskningen
  kontrollerar publicerade questkedjor, valbara belöningar, recept och bindning.
  Det bevisar genomförbarhet, inte optimal ranking. [Samlad leverans](forever-level30-catalogs.md),
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
  Den slutliga slotkontrollen verifierar också riktig Rogue Combat med
  17 mål, två ägda alternativ som fyller bara en slot, sökning i grupper,
  konsekventa 16-/17-mål för Hunter före/efter utrustning samt uppdaterad
  metadata med samma rekommendations-ID:n. [Aktuell rapport](previews/slots-ui-checks.txt),
  [Mage-vy](previews/slots-mage-overview.png), [Rogue-vy](previews/slots-rogue-overview.png),
  [grupperade alternativ](previews/slots-rogue-alternatives.png) och
  [nivå 60 utan data](previews/forever-level60-pending.png).
- Projektberoenden, filplacering och Markdown-spårning granskades under v1-arbetet.
  Den dåvarande strukturella kontrollen omfattade 19 Python- och åtta
  PowerShell-researchverktyg, 78 JSON-filer och 21 Markdown-filer.
  Antalen beskriver det kontrolltillfället; de är inte dagens filinventering.
  Vanliga katalogscenariet kräver också komplett täckning för båda fraktioner;
  framtida slotluckor kan inte passera enbart den ordinarie kontrollkörningen.

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
$verificationDirectory = Join-Path 'artifacts' ('beta2-verification-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $verificationDirectory | Out-Null
dotnet run --project BISTracker.Checks/BISTracker.Checks.csproj -c Release -- --catalog-release-audit (Join-Path $verificationDirectory 'catalog-release-audit.json')
python tools/data/verify-forever-slot-witnesses.py (Join-Path $verificationDirectory 'slot-witness-integration.json')
```

Använd nya rapportsökvägar vid beta2-arbete så att de daterade v1-bevisen
bevaras. För en ny delbar ZIP ska även publicerings- och distributionstestet
från [distribution.md](distribution.md) köras. De 49 kontrollerna gäller v1;
nya scenarier kräver också att testskriptets förväntade kontrollantal uppdateras.

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

Det separata distributionsprovet startar den normala publicerade appen med
en absolut `BISTRACKER_DATA_DIRECTORY` endast i provets process. UI Automation
använder processens eget fönster. Skriptet jämför ordinarie spelarprofilens
hashar även i `finally` och skapar en ny PENDING/PASS/FAIL-rapport per körning.
Detta diskprov ska hållas isär från DraftPreview med minneslagring.

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
  är implementerade och verifierade. Ägandet registrerar ett exemplar per
  variant; samma variant flyttas vid platsbyte och fyller aldrig två mål.
  Äldre dubbelutrustade varianter avvisas med originalfilen bevarad.
- Betaguiderna har föränderliga rekommendationer. Osäkra items har uteslutits
  och tidigare slotluckor är kompletterade. Listorna är
  guidebaserade alternativ enligt vald källpolicy, inte ett beräknat optimalt
  set. Publicerade questkedjor har källgranskats; en full spelklientgenomgång
  av varje anskaffningsväg har inte genomförts.
- Ikonbilder kräver nätverk vid visning. Katalog och tracking fungerar utan nätverk;
  en saknad ikon får en platsmarkering. Externa webbläsarklick är inte automatiskt körda.
- Rendering och ViewModel-beteenden är kontrollerade. Full manuell mus- och
  tangentbordsgranskning, olika DPI/fönsterstorlekar och MSIX-distribution återstår.
- Ingen verklig Forever-nivå 60-katalog finns i leveransen. Nivå 60-valet visar
  uttrycklig datastatus; importen verifieras med fiktiva kontrollpack. Betans
  nivå 30-kataloger används inte som slutlig pre-raid-data för nivå 60.

För appbeta2 kvarstår prov på ren mottagardator, olika DPI-/fönsterstorlekar
och eventuell framtida versions-/datamigration. WindowsSandbox.exe saknades
på den verifierade datorn; ingen Sandbox-körning gjordes. MSIX, certifikat
och signering är oprövade. Denna dokumentrevision ändrar inga tidigare
byggresultat och verifierar inte en ny beta2-release. Se [överlämningen](beta2-handoff.md).

## Dokumentöverlämning inför beta 2

Dokumentationskontroll 2026-10-08, REQ-012: samtliga 23 befintliga
projektspecifika Markdown-filer är uppdaterade. Med beta2-handoff.md
granskades 24 dokument och 270 lokala länkar, inklusive interna rubrikankare,
utan fel. Varje områdesdokument leder till överlämningen. Versionsnummer,
källcommit, testantal, ZIP-storlek och SHA-256 jämfördes med den sparade
1.0.0-rapporten och den lokala ZIP-filen. Diff-/filkontrollen visar endast
Markdown-ändringar; ingen kod, katalogdata, lagringsfil eller paket ändrades.

De ovan beskrivna 49 beteendekontrollerna och distributionsresultaten är
fortfarande v1-bevis. Inga nya builds, beteendetester, webbresearch, fetch
eller push kördes för dokumentuppdraget. Läs- och metadatautvärdering av
MSBuild bekräftade den dokumenterade UI-preview-utdatasökvägen; det var
inte en ny kompilering eller UI-körning.
