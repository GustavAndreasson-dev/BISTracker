# Lokal överföring från Boromir

Datum: 2026-10-08. Projektets kod och data kan nu utvecklas och köras från
C:\Users\gusta\source\repos\BISTracker utan tillgång till Boromir.

## Överförda original

Källa: C:\Dev\BISTracker på Boromir. Ren arbetsmapp på main, HEAD
8bf43b77ffe098057e1c0d4e5310d3eed3340cd9. Inga ocommittade eller otrackade
projektfiler och ingen framstegsdata fanns i den använda
%LOCALAPPDATA%\BISTracker-mappen på Boromir.

Lokal main fördes från ce7ef2b till samma HEAD med fast-forward. Dessa sex
originalcommits är nu lokala:

| Commit | Del |
| --- | --- |
| ba4cb5f | Engelskt gränssnitt och statusmeddelanden. |
| 2ee5976 | Fas 1-kandidater och verifierat metadataimportprov. |
| 209e9d3 | Scope, dokumentation och tilltal. |
| d0354e1 | Riktig fas 1-katalog och separat framstegsprofil. |
| 4e60e86 | Katalogkoppling, ikoner, anskaffning och källänkar i UI. |
| 8bf43b7 | Dokumentation och tidigare verifiering av integrationen. |

Katalog, metadata, importverktyg, källresearch, dokumentation och alla
versionshanterade förhandsbilder följde med. Lokala forever-feasibility.md
bevarades vid överföringen, innan dess status och krav-ID:n uppdaterades.
Denna session har inte skapat någon commit eller push.

## Integritetskontroll och historik

Exporten BISTracker-all-refs.bundle innehåller hela tillgängliga Git-historiken
och samtliga refs. Storlek: 755902 bytes. SHA-256 på båda datorerna:

```text
023a23caf4edc0427879b9efe6f49a5f92b11cb70c74838e42664f629f15069b
```

Alla 84 överföringsdelar mottogs. Checksumma, git bundle verify och
git fsck --connectivity-only --no-dangling passerade lokalt. Historiken är
importerad till det lokala repot. Kopior av källans refs finns under
refs/archive/boromir/ och dess main under refs/remotes/boromir/main.
Detta är lokala referenser; inga anslutningar till Boromir krävs för att läsa dem.
Även källans Codex-checkpoints är bevarade under arkivreferenserna.

Exporten och säkerhetskopian av föregående lokala historik finns i
C:\Users\gusta\AppData\Local\Temp\BISTracker-transfer-0f4b2dd53cf54e77b39d90943d43089b.
Fortsatt arbete beror på repots importerade objekt, inte på att tempmappen finns kvar.

## Lokal verifiering

Miljö: Windows x64, .NET SDK 10.0.400 och .NET 8-runtime 8.0.30.
Projektets angivna WinUI/NuGet-paket återställdes lokalt.

- Release-build av BISTracker.slnx: godkänd, 0 varningar och 0 fel.
- Samtliga 17 beteendekontroller: godkända.
- Isolerad UI-build: godkänd, 0 varningar och 0 fel.
- UI-körning med minneslagring: exit code 0. Riktig katalog, tracking,
  sammanfattningar, filter, sökning, källänkar, ikonhämtning, trasig ikon med
  fallback, suffixvillkor och fraktionsvillkor passerade.
- Lokalt renderade översikts-, suffix- och questbilder granskades visuellt.

Första UI-körningen upptäckte ett tidsberoende fel i verifieringskoden:
en gammal IsIconLoaded-markering kunde vara true innan den nya filterraden
fanns i det visuella trädet. DraftPreview väntar nu på aktuell bild, rätt URI
och avkodade bildmått. Rättningen kompileras bara i verifieringsvarianten;
normal produktfunktion och framstegsformat har inte ändrats.

Rapporten finns i [local-ui-checks.txt](previews/local-ui-checks.txt).
De nya lokala renderingarna ligger i överföringens tempmapp, under
local-ui-preview-fixed. Boromirs versionshanterade bilder har inte skrivits över.

Körning och ordinarie byggkommandon finns i [verification.md](verification.md).
Den isolerade lokala varianten byggdes med:

```powershell
dotnet build BISTracker.Presentation/BISTracker.Presentation.csproj -p:Platform=x64 -p:RuntimeIdentifier=win-x64 -p:WindowsPackageType=None -p:EnableDraftPreview=true -p:OutputPath=bin/verification-local/
```

## Kvarstående miljöfrågor och nästa arbete

Ingen chatthistorik flyttades. Direktflytten stoppades av appens stöd för
paginerade chattar; exporten hämtades genom ett uttryckligen godkänt uppdrag
till originalchatten. Projektets dokumentation och lokala fortsättningschatt
innehåller arbetsläget. Inga andra trådar behövs för att fortsätta koda här.

GitHub-åtkomst löstes senare samma dag genom att aktivera installerad Git
Credential Manager: global credential.helper är manager. Ett befintligt konto
för GustavAndreasson-dev kunde användas utan ny inloggning. git fetch origin
och git push --dry-run origin HEAD:main passerade med interaktiv autentisering
avstängd för kontrollerna. Ingen riktig push gjordes. Serverns main var då
ce7ef2b; lokal main är sex commits före origin/main. Autentiseringshemligheter
överfördes inte från Boromir och skrevs inte till projektdokumentation.
Kommandon har körts utanför sandboxen eftersom dess uppstart fortfarande ger
setup refresh had errors.

Nästa funktionsarbete är REQ-005–007 enligt [Forever-utredningen](forever-feasibility.md).
Forever, alla klasser/specs och tre listor per karaktär är beställda men ännu
inte implementerade. Ingen databasändring har gjorts eller föreslagits.
