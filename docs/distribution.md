# Portabel Windows-distribution

Status 2026-10-08: publiceringskedjan och ett extraherat portabelt Windows
x64-paket är lokalt verifierade. Slutpaketets checksumma och källcommit
redovisas efter publicering från committad kod. Ren mottagardator är inte provad.

## Leveransformat

Leveransen är ett ZIP-arkiv med en komplett programkatalog för
Windows x64. Både .NET-runtime och Windows App SDK-runtime följer med.
Avsikten är att mottagaren ska kunna starta appen utan Visual Studio,
.NET SDK eller separata installationer av dessa två runtimes.

Microsoft beskriver två separata inställningar: `SelfContained=true` för
.NET och `WindowsAppSDKSelfContained=true` för Windows App SDK.
`WindowsPackageType=None` anger den opaketerade appen. I detta leveransformat
ligger Windows App SDK:s beroenden bredvid programmets EXE och måste följa
med vid kopiering. Se [Microsofts distributionsguide](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/self-contained-deploy/deploy-self-contained-apps)
och [.NET:s publiceringsöversikt](https://learn.microsoft.com/en-us/dotnet/core/deploying/).

ZIP-filen är därför en portabel **programkatalog**, inte en ensam EXE.
DLL-filer, resurser och övriga publicerade filer är delar av leveransen.
Paketet är en vanlig Release-version utan den isolerade
`EnableDraftPreview`-verifieringen. Trimning är avstängd eftersom appens
JSON-läsare använder reflektionsbaserad serialisering. .NET 8 stänger av
den som standard vid trimning; se [Microsofts kompatibilitetsbeskrivning](https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/8.0/publishtrimmed).

## Starta som mottagare

1. Hämta den aktuella Windows x64-ZIP-filen.
2. Extrahera **hela arkivet** till en egen mapp, exempelvis
   `BISTracker` under Dokument. Behåll alla filer och undermappar.
3. Öppna den extraherade programkatalogen och starta
   `BISTracker.Presentation.exe`.

Starta från den extraherade mappen. Att endast kopiera EXE-filen eller
starta den direkt inne i ZIP-arkivet ger inte appen dess kompletta beroenden.
Någon MSIX-installation ingår inte i denna leverans.

Målplattformen är Windows x64. Projektet deklarerar miniminivån
`10.0.17763.0`, men äldre Windows-versioner och andra processorarkitekturer
har ännu inte distributionsverifierats. Bekräftad Windows-version för
det färdiga paketet redovisas i testtabellen nedan.

## Sparade karaktärer och uppdatering

Normal app sparar data för den aktuella Windows-profilen i
`%LOCALAPPDATA%\BISTracker`, separat från programkatalogen:

| Sökväg | Innehåll |
| --- | --- |
| `characters-v1.json` | Karaktärer, aktivt val, gemensamt ägande och separata spec-/kataloguppsättningar |
| `Catalogs\` | Lokalt importerade, validerade katalogpack |
| `holy-priest-classic-phase1-progress.json` | Äldre Classic-framsteg som bevaras vid engångsmigration |
| `draft-progress.json` | Äldre demoframsteg; importeras inte som verkligt ägande |

Sökvägarna är kontrollerade mot appens startkod och
`ProgressFilePaths`. Lagrings- och migrationsregler finns i
[karaktärer och speclistor](characters-and-loadouts.md) och
[verifieringsdokumentet](verification.md).

För att uppdatera stänger mottagaren appen, extraherar den nya ZIP-filen
till en ny programmapp och startar dess EXE. Behåll
`%LOCALAPPDATA%\BISTracker`. Bytet av programkatalog ändrar inte appens
datasökväg för samma Windows-profil. En kopia av datamappen kan sparas
separat före uppdateringen. Byte till en ny programmapp med samma version
är verifierat med oförändrad sparfil. Datamigration mellan olika framtida
releaser är inte provad av detta test.

ZIP-filen innehåller inte mottagarens sparade framsteg. Om bara
programkatalogen flyttas till en annan dator eller Windows-profil följer
karaktärerna därför inte automatiskt med. Kör inte flera appinstanser som
skriver samma datamapp samtidigt; sådan samordning ingår inte i appen.

## Distributionskontroller

Huvudtråden fyller i resultat, miljö och rapportlänkar efter varje faktisk
kontroll. Tester från utvecklingsbuilden redovisas separat i
[verification.md](verification.md).

| Kontroll | Resultat för ZIP-leveransen |
| --- | --- |
| Release-publicering Windows x64 med båda runtimes inkluderade | Godkänd; utan trimning, preview eller buildvarningar |
| Paketets fullständiga filer, storlek och SHA-256 | Manifest och extraktion godkända; slutmetadata redovisas efter committad publicering |
| ZIP-extraktion till ny mapp och kontroll av filinnehåll | Godkänd; SHA-256 för varje fil, sökväg med blanksteg |
| Start av extraherad normal app utanför byggmappen | Godkänd; verklig WinUI-checkbox, ingen PreviewRepository |
| Isolerade UI-/katalogkontroller från publicerat innehåll | 49 beteendekontroller och 27 slotkataloger godkända; tre verkliga Rogue-specs med diskpersistens |
| Karaktärsdata sparas, återläses efter omstart och bevaras vid byte av programmapp | Godkänd; checkboxens diskdata återläst i två omstarter, ordinarie spelarprofil oförändrad |
| Bekräftad Windows-version och medföljande runtime-filer | Windows 11 Pro 10.0.26200; coreclr, hostfxr, hostpolicy, WindowsAppRuntime och WinUI laddades från paketet |
| Ren mottagardator utan utvecklingsverktyg eller tidigare runtimes | Ej genomfört |
| Windows Sandbox | Ej genomfört; WindowsSandbox.exe saknas lokalt |
| MSIX-paket, certifikat och installation | Ej genomfört; ingår inte i ZIP-provet |

En lyckad lokal start på utvecklingsdatorn bevisar inte självständigt att
alla beroenden är med på en ren dator. Ett sådant prov ska redovisas som
återstående tills det faktiskt genomförts. Samma sak gäller signering,
MSIX-installation och olika DPI-/fönsterstorlekar.

Kataloger och tracking är lokala. Itemikoner hämtas över nätverket och
visar en platsmarkering om hämtningen misslyckas; externa källänkar öppnar
webbsidor. Verkliga Forever nivå 60-kataloger ingår fortfarande inte.

## Återskapa och verifiera leveransen

Kör från repot med committad kod och ren arbetskatalog:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/distribution/Publish-PortableRelease.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/distribution/Test-PortableRelease.ps1 -ZipPath "<den utskrivna ZIP-sökvägen>" -CheckHostDirectory "<den utskrivna checkhost-mappen>"
```

ExecutionPolicy gäller endast dessa processer. Skripten ändrar inte datorns
policy, installerar inga certifikat och ändrar inga Windows-funktioner.
Publicering använder en ny GUID-mapp under ignorerade `artifacts/`.
Checkhost, rapporter och testprofiler följer inte med i ZIP-filen.
Varje prov får en ny rapportmapp med PENDING/PASS/FAIL, så gamla resultat
inte skrivs över. Slutlig PASS kräver alla 49 beteendekontroller och
27 kompletta Forever-kataloger.

UI-provet använder `BISTRACKER_DATA_DIRECTORY` enbart i den teststartade
processen. Appen accepterar då en absolut datamapp; vanlig start behåller
`%LOCALAPPDATA%\BISTracker`. UI Automation riktar sig bara till provets eget
process-ID och fönster. De fem observerade runtime-filerna måste ligga i
den extraherade programmappen. En PE-inventering kontrollerar vanliga och
fördröjda VC-runtimeimporter i 275 binärer; inga saknade lokala VC-beroenden
hittades. Detta är en statisk kontroll, inte bevis för körning på ren dator.
