# Verifiering och körning av första utkastet

Datum: 2026-10-07.

## Genomförda kontroller

- Hela solutionen byggd i Release för x64 utan varningar eller fel. WinUI-utkastet och isolerad verifieringsvariant är också byggda utan varningar eller fel.
- Alla 14 konsolkontroller godkända: domäninvariants, separata trackingmarkeringar,
  byte i samma slot, okända ID:n, skrivfel, JSON-återläsning, saknad fil, korrupt
  data, avbruten sparning och låst målfil.
- Appen har startats i en isolerad verifieringsvariant med minneslagring.
  Inga riktiga användarframsteg läses eller skrivs av den kontrollen.
- UI-kontrollen verifierar initialt tillstånd, uppdaterade sammanfattningar,
  erhållen/utrustad-regler via ViewModel, filter, sökning och tomt resultat.
- Den faktiska WinUI-vyn har renderats till [PNG](previews/draft-overview.png)
  och granskats visuellt. Checkboxarnas bredd och listans utrymme korrigerades
  efter första granskningen. Resultat finns i [UI-rapporten](previews/ui-checks.txt).
- Projektberoenden, filplacering och Markdown-spårning har granskats.

Renderingen använder appens eget visuella träd via
[RenderTargetBitmap](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.rendertargetbitmap.renderasync).
Bilden visar verifieringsdata: 6 erhållna och 3 utrustade, inte sparade användarframsteg.

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

Normal app sparar demoprofilens tillstånd i
`%LOCALAPPDATA%/BISTracker/draft-progress.json`. Första start visar tomt framsteg.
Felaktig framstegsfil rapporteras som fel och skrivs inte tyst över.

## Återskapa den isolerade UI-kontrollen

```powershell
dotnet build BISTracker.Presentation/BISTracker.Presentation.csproj -p:Platform=x64 -p:RuntimeIdentifier=win-x64 -p:WindowsPackageType=None -p:EnableDraftPreview=true
& ./BISTracker.Presentation/bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64/BISTracker.Presentation.exe --draft-preview "$PWD/docs/previews"
```

Verifieringskod kompileras endast med EnableDraftPreview=true. Appen stänger
verifieringsfönstret efter kontrollen. Normal build inkluderar inte den koden.

## Praktiska begränsningar

- 17 fiktiva platshållare, inte en verifierad pre-raid BiS-lista.
- En lokal demoprofil; ingen karaktärsväljare, konto, molnsynk eller spelintegration.
- Utökade spelregler som unika items, tvåhandskombinationer och verkliga
  ring-/trinketidentiteter behöver modelleras tillsammans med riktig data.
- Rendering och ViewModel-beteenden är kontrollerade. Full manuell mus- och
  tangentbordsgranskning, olika DPI/fönsterstorlekar och MSIX-distribution återstår.
- Fler expansioner är en framtida riktning, ännu inte implementerade.
