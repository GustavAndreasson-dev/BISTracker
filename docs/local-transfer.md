# Lokal överföring från Boromir

Historisk överföringsrapport: 2026-10-08. Projektets kod och data kan utvecklas
och köras från `C:\Users\gusta\source\repos\BISTracker` utan tillgång till Boromir.
Överföringen är avslutad och ska inte göras om inför [appbeta2](beta2-handoff.md).
Denna fil bevarar ursprung, integritetskontroll och dåvarande verifiering.
Aktuellt funktionsläge och nästa agents arbete finns i överlämningen.

Nuvarande v1-bas är portabel version 1.0.0 från kodcommit `e25c134`, med
leveransdokumentation i `043d859`, 49 godkända kontroller och 27 granskade
Forever-kataloger. [Verifiering](verification.md) och [distribution](distribution.md)
redovisar dessa senare resultat; de ska inte förväxlas med överföringens 17 kontroller.

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
Själva överföringsmomentet skapade ingen commit eller push. Senare lokal
implementation och v1-paketering finns i repots efterföljande historik;
överföringens HEAD `8bf43b7` är inte aktuellt utvecklings-HEAD.

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

## Historisk lokal verifiering vid överföringen

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

## Historisk chattflytt och Git-åtkomst

Ingen chatthistorik flyttades. Direktflytten stoppades av appens stöd för
paginerade chattar; exporten hämtades genom ett uttryckligen godkänt uppdrag
till originalchatten. Projektets dokumentation och lokala fortsättningschatt
innehåller arbetsläget. Inga andra trådar behövs för att fortsätta koda här.

GitHub-åtkomst löstes senare samma dag genom att aktivera installerad Git
Credential Manager: global credential.helper är manager. Ett befintligt konto
för GustavAndreasson-dev kunde användas utan ny inloggning. git fetch origin
och git push --dry-run origin HEAD:main passerade med interaktiv autentisering
avstängd för kontrollerna. Ingen riktig push gjordes i just den åtkomstkontrollen. Serverns main var då
ce7ef2b och lokal main låg sex commits före dåvarande origin/main. Detta
beskriver kontrolltidpunkten, inte fjärrrepots eller lokala main:s aktuella läge. Autentiseringshemligheter
överfördes inte från Boromir och skrevs inte till projektdokumentation.
Vid överföringen kördes vissa kommandon utanför sandboxen efter felet
`setup refresh had errors`. Det är ett historiskt miljöfynd, inte ett krav
att nästa agent ska upprepa samma upplägg eller inloggningskonfiguration.
Ingen ny fetch, push, autentiseringsändring eller fjärrlägeskontroll har gjorts
som del av denna dokumentuppdatering.

## Fortsättning på denna dator

De då beställda REQ-005–007 har senare implementerats: Forever nivå30 för
alla klasser/specs och tre listor med gemensamt ägande och separat utrustning.
Nivå60-import och egna kataloguppsättningar finns; verkliga nivå60-listor
saknas. Dessa funktioner och den lokalt verifierade v1-ZIP:en är dagens
utgångspunkt, inte nya överföringsuppgifter.

Nästa agent börjar i det lokala repot med [beta2-handoff.md](beta2-handoff.md),
README, arbetsregler och faktiskt Git-status. Boromir, temp-exporten, gamla
chattar och en ny Git-inloggningsinstallation behövs inte för att läsa eller
bygga projektet. Bevara de importerade objekten och arkivreferenserna som
historik. Nya beta2-funktioner och nästa versionsnummer är ännu inte beslutade.
Ingen databasändring ingår i denna överlämning.
