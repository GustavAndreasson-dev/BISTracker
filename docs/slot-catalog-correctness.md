# Slotmål, alternativ och kontroll före delning

Beställt av Chefen 2026-10-08, REQ-010. Tidigare presentation räknade
rekommendationsrader som mål: exempelvis Rogue Combats 39 alternativ syntes
som 39 mål trots att karaktären har högst 17 utrustningsplatser. Den tidigare
bedömningen att appen var redo att dela var för tidig.

## Korrigerade regler

- `EquipmentPlan` grupperar rekommendationer till ett mål per förväntad slot.
  Flera alternativ i samma slot ökar inte antalet mål eller den slotens framsteg.
  En saknad rekommendation blir ett synligt ofyllt mål, inte en dold rad.
- Gruppning innebär alternativ för samma plats. Likvärdighet, eget BiS-val
  eller rangordning påstås inte utan rekommendationskälla som stöder det.
- Ett komplett set kräver en giltig kombination av olika fysiska itemvarianter,
  inte bara en rad på varje position. Samma enda trinket på två platser lämnar
  en andra-trinket-lucka. Unique gäller basitem-ID även över suffixvarianter.
- Tvåhandsmål behöver inte OffHand. Guidens uttryckliga `weaponSetup` styr
  om ett tvåhandsalternativ får uppfylla målet eller om MainHand + OffHand
  behövs. Sådana krav behöver en separat HTTPS-källa i katalogen.
- Slotundantag behöver konkret motivering och källa; ett hål i en guide
  räcker inte. `slotExemptions` får inte motsäga ett item i samma slot.
- Ett fullständigt set ska vara möjligt separat för Alliance och Horde.
  `availableFactions` anger verifierad questtillgänglighet; en okänd
  questfraktion blockerar releasegranskningen. Fraktionsvillkor visas i
  itemvillkoren; karaktärens fraktion väljs ännu inte i appen.

## Ägande och bevarade framsteg

Ägandet är fortfarande gemensamt per karaktär och item-ID/suffix, med separat
utrustning per spec/katalogset. Det booleska ägandet registrerar ett känt
exemplar och bevisar inte två kopior. Vid utrustning på en annan möjlig plats
flyttas samma registrerade variant; ägandet bevaras. Två okvantifierade kopior
kan inte tyst återställas som en verifierad utrustningskombination.

Befintliga filer ändras inte vid läsfel. Ett äldre tillstånd som markerar
samma variant i två slots avvisas och originalet bevaras; antal exemplar och
en eventuell sådan migration kräver ett separat beslutat arbetsflöde.
Inget databas- eller framstegsfilformat har ändrats.

## Releasegranskning

`CatalogReleaseAudit` kontrollerar de 27 produktkatalogerna och skriver ett
maskinläsbart resultat med målgrupper, handkrav, undantag, fraktionsspecifika
luckor och okända questfraktioner:

```powershell
dotnet run --project BISTracker.Checks/BISTracker.Checks.csproj -c Release -- --catalog-release-audit docs/data/forever-slot-release-audit.json
```

Exitkod 1 och `releaseReady: false` betyder att katalogerna blockerar delning.
Passing beteendekontroller betyder att kontrollen fungerar; det betyder inte
att katalogerna är fullständiga. Full slotgranskning, alla tester, renderad
UI-kontroll och distributionstest måste vara godkända före delning.

`EquipmentPlanScenarios` verifierar gruppering, saknade slots, ägda alternativ,
kapacitet för ringar/trinkets, unique, handkombinationer och fraktionsisolering.
Release-build för modelländringen passerar utan varningar/fel och projektets
46 beteendekontroller är godkända. Den första körda releasegranskningen blockerar
katalogerna, vilket är förväntat medan komplettering och fraktionsrevision pågår.
Specialisttrådarna granskar varsin grupp om nio kataloger; huvudtråden äger
integration, granskning av hela resultatet och beslut om faktiskt verifierat nuläge.
