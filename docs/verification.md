# Verifiering och körning av första utkastet

Datum: 2026-10-07.

## Genomförda kontroller

- Gränssnitt, exempelmetadata, status och felmeddelanden är nu på engelska.
  Den isolerade UI-kontrollen och PNG-renderingen har körts på den översatta vyn.

- Hela solutionen byggd i Release för x64 utan varningar eller fel. WinUI-utkastet och isolerad verifieringsvariant är också byggda utan varningar eller fel.
- Alla 17 konsolkontroller godkända: riktig katalog, suffixidentitet, separata profiler,
  domäninvariants, separata trackingmarkeringar,
  byte i samma slot, okända ID:n, skrivfel, JSON-återläsning, saknad fil, korrupt
  data, avbruten sparning och låst målfil.
- Appen har startats i en isolerad verifieringsvariant med minneslagring.
  Inga sparade spelarframsteg läses eller skrivs av den kontrollen.
- UI-kontrollen använder samma 17 riktiga items som appen och verifierar initialt
  tillstånd, sammanfattningar, erhållen/utrustad-regler, filter, sökning på item/slot/
  anskaffning, tomt resultat, källänkar, synliga ikonbilder och trasig bild med fallback.
  Radåteranvändning, suffixvillkor och fraktionsbundna questvillkor kontrolleras också.
- Den faktiska WinUI-vyn har renderats till [PNG](previews/draft-overview.png)
  och granskats visuellt. Checkboxarnas bredd och listans utrymme korrigerades
  efter första granskningen. Resultat finns i [UI-rapporten](previews/ui-checks.txt).
- Projektberoenden, filplacering och Markdown-spårning har granskats.

Renderingen använder appens eget visuella träd via
[RenderTargetBitmap](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.rendertargetbitmap.renderasync).
Bilden visar isolerat tillstånd: 6 erhållna och 3 utrustade, inte sparade spelarframsteg.
Ytterligare granskade renderingar: [suffixmål](previews/phase1-suffix.png),
[questvillkor](previews/phase1-quest.png) och [saknad bild](previews/phase1-icon-fallback.png).

## Bygg och starta

Från projektroten på Windows med .NET SDK och WinUI-byggberoenden:

```powershell
dotnet build BISTracker.slnx -c Release -p:Platform=x64 -p:WindowsPackageType=None
dotnet run --project BISTracker.Checks/BISTracker.Checks.csproj -c Release
```

Öppna BISTracker.slnx i Visual Studio, välj BISTracker.Presentation och x64
för normal utveckling. Den opaketerade Release-builden kan även startas direkt:

```powershell
& ./BISTracker.Presentation/bin/x64/Release/net8.0-windows10.0.19041.0/win-x64/BISTracker.Presentation.exe
```

Normal app sparar tillstånd i
`%LOCALAPPDATA%/BISTracker/holy-priest-classic-phase1-progress.json`.
Första start för den riktiga katalogen visar tomt framsteg. Gamla
`draft-progress.json` bevaras och läses inte in som verkligt itemägande.
Appen visar information om övergången om en demofil finns före första riktiga sparningen.
Felaktig framstegsfil rapporteras som fel och skrivs inte tyst över.

## Återskapa den isolerade UI-kontrollen

```powershell
dotnet build BISTracker.Presentation/BISTracker.Presentation.csproj -t:Rebuild -p:Platform=x64 -p:RuntimeIdentifier=win-x64 -p:WindowsPackageType=None -p:EnableDraftPreview=true -p:OutputPath=bin/verification/
& ./BISTracker.Presentation/bin/verification/BISTracker.Presentation.exe --draft-preview "$PWD/docs/previews"
```

Verifieringskod kompileras endast med EnableDraftPreview=true. Appen stänger
verifieringsfönstret efter kontrollen. Normal build inkluderar inte den koden.
Separat utdatakatalog undviker att en annan tråds MSIX-build ersätter
verifieringsprogrammets filer före start.

## Praktiska begränsningar

- 17 riktiga, guidebaserade huvudval. Bonecreeper Stylus beskrivs som alternativ
  men är inte ett eget trackingmål; full questkedjerevision återstår.
- En lokal Holy Priest-profil; ingen karaktärsväljare, konto, molnsynk eller spelintegration.
- Utökade spelregler för unika items, tvåhandskombinationer och
  omflyttbara ring-/trinketalternativ återstår. Den fasta katalogen innehåller
  varje unikt item en gång och inga tvåhandsalternativ.
- Ikonbilder kräver nätverk vid visning. Katalog och tracking fungerar utan nätverk;
  en saknad ikon får en platsmarkering. Externa webbläsarklick är inte automatiskt körda.
- Rendering och ViewModel-beteenden är kontrollerade. Full manuell mus- och
  tangentbordsgranskning, olika DPI/fönsterstorlekar och MSIX-distribution återstår.
- Fler expansioner är en framtida riktning, ännu inte implementerade.
