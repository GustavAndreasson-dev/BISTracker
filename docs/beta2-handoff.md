# Överlämning inför appens beta 2

Statusdatum: 2026-10-08. Detta är startpunkten för nästa agent. Chefen har
beställt att hela projektdokumentationen ska vara aktuell inför nästa beta.
Beta 2-scope är beslutat i [DEC-016](decisions.md#dec-016--scope-för-appens-beta-2):
Classic fas 1 för alla 27 specs (REQ-013) samt namnbyte och borttagning av
karaktärer (REQ-014, REQ-015). Implementera inte andra öppna förslag som om
de vore beslutade krav.

**Appens beta 2 är nästa testleverans av BISTracker.** Den ska inte blandas
ihop med spelversionen WoW Forever eller katalogernas `releaseStage: Beta`.
Den färdiga första testleveransen har programversion **1.0.0**; nästa
programversionsnummer och beta 2-innehåll är inte beslutade.

## Börja här

1. Läs [README](../README.md) och [CLAUDE](../CLAUDE.md), sedan detta dokument.
2. Kontrollera `git status --short` och `git log -5 --oneline`. Arbetet ligger
   på `main` vid denna överlämning; nya arbetsbranches använder `claude/`.
   Bevara
   befintligt arbete. Arbetsplatsen är `C:\Users\gusta\source\repos\BISTracker`
   på denna dator; Boromir och gamla användarägda chattar behövs inte.
3. Läs [krav](requirements.md), [beslut](decisions.md) och [plan](plan.md).
   Välj därefter områdesdokument från kartan nedan och läs berörd kod.
4. Utgå från verifierade rapporter. Den senaste dokumentationscommiten kan
   ligga efter paketets källcommit utan att den paketerade koden har ändrats.
5. När Chefen ger återkoppling eller nya beta 2-krav: koppla varje konkret
   ändring till ett stabilt krav-ID, berörda filer och acceptanskriterier.
   Slutför och verifiera en sammanhängande del före nästa del och commit.

## Vad som faktiskt är färdigt

| Område | Verifierad bas för nästa beta |
| --- | --- |
| Classic | Holy Priest, Vanilla fas 1, pre-raid, 17 huvudval från endast dungeons/quests. Andra Classic-specs har ingen granskad katalog. |
| Forever | Nio klasser och 27 specs med egna nivå 30-beta/patch 1.60.1-kataloger. Dungeons, quests och crafting är godkända. Inga Vanilla-listor används som ersättning. |
| Alternativ och slots | 1 655 råa placeringsrader, 1 635 aktiva efter källpolicy/handfilter. UI visar relevanta slotmål; Rogue har 17, vissa tvåhandsplaner 16. Alternativ i samma slot ger ett mål. |
| Fullständighet | 27 kataloger och 54 möjliga Alliance/Horde-set granskade med faktisk variantkapacitet, unique, händer, fraktion och dokumenterade anskaffningsvillkor. |
| Karaktärer | Namn/version/klass, tre specs per karaktär. Ägande gemensamt, utrustning separat per spec och katalogset; aktivt val återställs efter omstart. |
| Nivå 60 | Atomisk import och separata utrustningsuppsättningar finns. Verklig Forever nivå 60-data saknas. Den synliga väntande kontexten är `forever-launch-level60`, inte `forever-beta-level60`. |
| UI | Engelska texter, karaktär/spec/nivåval, sökning/filter, grupperade alternativ, item-/guidelänkar och ikoner med fallback. |
| Lagring | Lokal JSON, schema 1, kontrollerad engångsmigration av äldre verkliga Classic-framsteg. Demodata importeras inte som verkligt ägande. |
| Distribution | Portabel Windows x64-ZIP 1.0.0 med .NET och Windows App SDK. Release utan trimning eller DraftPreview. Normal publicerad app har sparat och återläst via faktisk UI-checkbox. |

Guidealternativ är inte en egen beräknad optimal ranking. Fullständiga slots
bevisar en möjlig utrustningskombination enligt granskningen, inte att varje
questkedja och crafting-väg har spelats igenom i klienten.

## Paket, commits och bevis

| Referens | Värde eller roll |
| --- | --- |
| Paketerad källcommit | `e25c1345c84dd4c975c208adb35c83ee68b1b88c` |
| Föregående release-dokumentation | `043d859`, metadata för version 1.0.0 |
| Slot- och källintegration | `173d766`; föregående klassgruppscommits `680c8e6`, `abfe1d0`, `925233c` |
| ZIP | `artifacts/distribution-b6f8906d7a114fed8462afb3ee2bc582/BISTracker-1.0.0-win-x64.zip` |
| Storlek och filer | 90 273 350 byte; 517 filer |
| SHA-256 | `81c6bb0bfc145ee313737d362c930af9c9fe4960f6146ad7613b1e886314c51f` |
| Distributionsbevis | [portable-release-verification.json](data/portable-release-verification.json) |
| Slotgranskning | [forever-slot-release-audit.json](data/forever-slot-release-audit.json) |
| Konkreta fraktionsset | [forever-slot-witness-integration.json](data/forever-slot-witness-integration.json) |
| UI-bevis | [slots-ui-checks.txt](previews/slots-ui-checks.txt) och bilder i [README](../README.md) |

ZIP, checkhost och provrapporter ligger i ignorerade `artifacts/`; de finns
lokalt men följer inte med vid Git-kloning. Återskapa med publiceringsskriptet
om de saknas. Katalogerna som appen läser är inbyggda resurser i
`BISTracker.Infrastructure/Catalog/Data`, inte researchfilerna i `docs/data`.

Verifierad bas: **49 beteendekontroller**, 27 kompletta kataloger,
54 källgranskade fraktionsset, 275 PE-binärer utan saknade lokala
VC-runtimeimporter och Release-build med 0 varningar/fel. Publicerat checkhost
har samma Domain/Application/Infrastructure-DLL-hashar som appen. Windows
11 Pro `10.0.26200` är provad. Fem observerade runtime-filer laddades från
den extraherade programmappen; ordinarie sparfiler var oförändrade.

## Arbete som återstår eller kräver ett nytt beslut

| Punkt | Status inför beta 2 | Nästa konkreta åtgärd |
| --- | --- | --- |
| Återkoppling från vänner | Ingen återkoppling från vänners körning av ZIP 1.0.0 är registrerad i repot vid denna överlämning. | Registrera rapportens appversion, klass/spec/nivå, steg, förväntat/faktiskt resultat och om data påverkas. Prioritera reproducerbara fel med Chefen. |
| Ren mottagardator | Inte provad. `WindowsSandbox.exe` saknas lokalt. | Prova extraherat paket på en tillgänglig ren Windows x64-miljö och dokumentera faktiska beroenden/resultat. Lokal runtime-laddning och PE-inventering ersätter inte detta. |
| UI och tillgänglighet | Automatiska/UI-preview-prov finns; full manuell mus/tangentbords-, DPI- och fönsterstorleksgranskning saknas. | Reproducera och åtgärda faktiska fel i prioriterat beta 2-scope. |
| Ny katalogrevision | Beta-guider kan förändras; tidigare inkompatibelt sparat ägande/utrustning avvisas med originalfilen bevarad. | Besluta strategi för kataloguppdateringar och migration innan ändrade itemidentiteter eller ersättningsimport levereras. |
| Forever nivå 60 | Importstödet är klart, verklig källgranskad data saknas. | Bekräfta patch/fas/källor när data finns; skapa kompletta verifierade pack för rätt Launch60-kontext. Använd inte nivå 30 eller testfixtures som nivå 60-data. |
| Klassdata och anskaffning | Slots/källunderlag granskade; full klientrevision av alla quest-/professionsvägar återstår. | Följ respektive klassgrupps källrapport; behåll crafting-villkor och fraktion. |
| Identiska exemplar | Ägande är booleskt för ett exemplar per itemvariant. | Antalshantering är ett öppet produktbeslut, inte ett färdigt krav. |
| Andra källor, Classic-specs, synk, plattformar | Inte beställda som beta 2-funktioner. | Invänta konkreta krav; utöka inte scope genom äldre researchförslag. |
| Signering/MSIX/äldre Windows/ARM64 | Inte distributionsverifierade; nuvarande ZIP är osignerat. | Hantera först om vald leverans eller teståterkoppling kräver det. |

Byte av programmapp har testats med samma version av ZIP-paketet.
Det är inte ett bevis för datamigration mellan framtida programversioner.

## Kodens ansvarsgränser och regler som måste bevaras

- `BISTracker.Domain/Equipment/EquipmentPlan.cs`: slotgrupper, matching, fraktion,
  unique och en fysisk kopia per item-ID/suffix. `OwnedCoverage` väljer
  en möjlig fraktion och respekterar utrustad handplan; alternativa ringar
  och trinkets får inte ge falskt komplett set.
- `BISTracker.Domain/Tracking/CharacterLoadouts.cs`: tre specs, delat ägande,
  separata katalogutrustningar och invariants. Reglerna ska kunna provas utan UI.
- `BISTracker.Application/Characters/CharacterTrackerService.cs`: val, tracking,
  migration och sparning med återställning vid fel. Ändra inte sparad state
  eller UI-val som om en misslyckad operation lyckats.
- `BISTracker.Infrastructure/Catalog`: strikt schema-/kontextvalidering, inbyggda
  resurser, atomisk packimport och levande kataloguppdatering. Duplicerade
  pack-ID:n får inte ersätta inbyggda eller tidigare importerade kataloger.
- `BISTracker.Infrastructure/Persistence`: atomisk JSON-sparning, bevarande av
  korrupta/future-schema-filer och äldre original. Lagringsformatet är schema 1.
- `BISTracker.Presentation/Features/Tracking/ViewModels`: räknare/filter från slotmål,
  grupperade alternativ och metadatauppdatering även vid samma rad-ID:n.
  Lägg inte nya domänregler i vyn eller förklarande UI-text för självklar interaktion.

Sparfiler ligger normalt i `%LOCALAPPDATA%\BISTracker`, inte i programmappen:
`characters-v1.json`, äldre `holy-priest-classic-phase1-progress.json`,
`draft-progress.json` och importer under `Catalogs/`. Radera eller skriv
inte över dessa för att få ett test att gå igenom. En absolut
`BISTRACKER_DATA_DIRECTORY` kan anges enbart för testprocessen; normal
start behåller den befintliga Windows-profilen.

## Körning och verifiering vid nästa kodändring

Kör PowerShell från repot. .NET SDK, Windows-buildverktyg och Python behövs
för utvecklingskontrollerna, medan mottagarens ZIP inkluderar runtimes.

```powershell
dotnet build BISTracker.slnx -c Release -p:Platform=x64 -p:WindowsPackageType=None -p:EnableDraftPreview=false
dotnet run --project BISTracker.Checks -c Release
dotnet run --project BISTracker.Checks -c Release -- --catalog-release-audit artifacts/beta2-slot-audit.json
python tools/data/verify-forever-slot-witnesses.py artifacts/beta2-witness-audit.json
```

Använd en ny rapportfil om ett tidigare resultat eller genererad data ska
bevaras. Antalet 49 gäller nuvarande bas. Läggs riktiga scenarier till ska
också `Test-PortableRelease.ps1` uppdateras: där är förväntat antal **49**
och tre distributionsscenarier ett uttryckligt releasevillkor.

Inför en ny ZIP: granska staged filer och committa verifierad kod samt
relevant dokumentation. Publiceringsskriptet kräver en ren arbetskatalog.
Ange nästa beslutade programversionsnummer med `-Version`; formatet är
`major.minor.patch`, utan betasuffix. Båda runtimes ska följa med och
`PublishTrimmed=false`, `EnableDraftPreview=false`, `WindowsPackageType=None`
ska behållas. Testdata och checkhost får inte följa med i ZIP-filen.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/distribution/Publish-PortableRelease.ps1 -Version "X.Y.Z"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/distribution/Test-PortableRelease.ps1 -ZipPath "<utskriven ZIP-sökväg>" -CheckHostDirectory "<utskriven checkhost-mapp>"
```

ExecutionPolicy ändras bara för dessa processer. Varje publicering använder
en ny GUID-mapp; varje prov får en egen PENDING/PASS/FAIL-rapport.
Den inbyggda UI Automation-kontrollen använder provets process-ID och
temporära data. En tidigare computer-use-hjälpprocess kunde inte starta i
denna session; det lyckade distributionsprovet använder därför sitt eget
testverktyg. Detta kräver inte gamla Codex-chattar eller den hjälpprocessen.

## Dokumentkarta

| Dokument | Användning vid fortsatt arbete |
| --- | --- |
| [README](../README.md), [CLAUDE](../CLAUDE.md) | Start, arbetsregler, godkänt scope och språk. |
| [Plan](plan.md), [krav](requirements.md), [beslut](decisions.md) | Vad som är klart, öppet respektive beställt. |
| [Arkitektur](architecture.md), [mappstruktur](project-structure.md) | Ansvarsgränser och faktisk filplacering. |
| [Karaktärer](characters-and-loadouts.md), [katalogkontexter](catalog-contexts.md) | Ägande, tre specs, lagring, nivåbyte och importkontrakt. |
| [Verifiering](verification.md), [distribution](distribution.md) | Kommandon, faktiska prov och kvarvarande begränsningar. |
| [Slotkorrekthet](slot-catalog-correctness.md), [Forever-kataloger](forever-level30-catalogs.md) | Acceptanskriterier för katalogändringar och release. |
| [Casters](forever-level30-casters.md), [hybrids](forever-level30-hybrids.md), [physical](forever-level30-physical.md) | Klassspecifika källor, slotset och crafting-/questvillkor. |
| [Forever-utredning](forever-feasibility.md) | Bakgrund och skillnaden mellan spelversioner; ny data kräver ny verifiering. |
| [Classic-källresearch](data-research.md), [fas 1-items](phase1-items.md) | Historisk research och underlag för dagens Classic-urval. |
| [Metadata/importresearch](data-import.md), [Classic-integration](catalog-integration.md) | Historiskt införande och bevarade katalog-/suffixvillkor. |
| [Utkastkontrakt](draft-contract.md), [lokal överföring](local-transfer.md) | Historiska uppdrag och Boromir-flytten; inte aktiva nya uppgifter eller beroenden. |

## Arbetsform för nästa agent

Arbetet drivs av Claude: en huvudagent som samordnar och integrerar, och
underagenter i `.claude/agents/` med avgränsade filansvar (domän,
infrastruktur/kontroller, katalogdata per klassgrupp, presentation och en
oberoende granskare). Se [underagenterna](plan.md#underagenter) och
[arbetsreglerna](../CLAUDE.md). Chefen har beställt regelbundna commits;
huvudagenten granskar och committar. Gamla Codex-trådar och chatt-ID:n är
historik. Nya användarägda sessioner skapas endast på Chefens uppdrag.

Detta dokumentationsuppdrag ändrar inga appfunktioner, kataloger eller
spelardata. Dokumentationskontroller ska skiljas från de tidigare
bygg-/beteende-/distributionsresultaten; en md-uppdatering gör inte en ny
beta testad. Markera beta 2 klar först efter verifiering av dess faktiska
ändringar och det exakta nya paketet.
