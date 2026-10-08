# Slotmål, alternativ och kontroll före delning

Beställt av Chefen 2026-10-08, REQ-010. Tidigare presentation räknade
rekommendationsrader som mål: exempelvis Rogue Combats 39 alternativ syntes
som 39 mål trots att karaktären har högst 17 utrustningsplatser. Den tidigare
bedömningen att appen var redo att dela var för tidig.

Nuläge inför [appbeta2](beta2-handoff.md): sloträttningen ingår i den
verifierade portabla appversionen 1.0.0 från kodcommit `e25c134`.
Releasekontrollen godkänner samtliga 27 Forever-kataloger och den separata
integrationskontrollen 54 fraktionsset. Detta dokument beskriver regler och
acceptanskriterier som nästa agent ska bevara vid beta2-ändringar; det är
inte en beställning av nya funktioner eller en ny godkänd beta2-release.

## Korrigerade regler

Chefen har därefter godkänt crafting för Forever. Det aktiva urvalet är
Dungeon/Quest/Crafting; Classic-katalogen behåller Dungeon/Quest. Tillverkade
alternativ behöver verifierad nivå-/skilltillgänglighet och konkreta villkor.

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
  Ägda mål väljer en möjlig kombination inom en enda fraktion; unionen av
  Alliance- och Horde-belöningar kan inte ge falskt fullständigt framsteg.
- Ägda och synliga mål använder samma handplan, även innan ett tvåhandsvapen
  utrustats. En faktiskt utrustad enhandsuppsättning behåller sitt offhandmål.
  Alternativ som strider mot guidens uttryckliga handkrav visas inte aktivt.
- Ändrad katalogmetadata bygger om iteminformation och fullständighetskontroll,
  även när rekommendationernas ID:n är oförändrade. Oförändrat kataloginnehåll
  behåller vyns alternativgrupper och bildstatus.

## Ägande och bevarade framsteg

Ägandet är fortfarande gemensamt per karaktär och item-ID/suffix, med separat
utrustning per spec/katalogset. Det booleska ägandet registrerar ett känt
exemplar och bevisar inte två kopior. Vid utrustning på en annan möjlig plats
flyttas samma registrerade variant; ägandet bevaras. Två okvantifierade kopior
kan inte tyst återställas som en verifierad utrustningskombination.

Befintliga filer ändras inte vid läsfel. Ett äldre tillstånd som markerar
samma variant i två slots avvisas och originalet bevaras; antal exemplar och
en eventuell sådan migration kräver ett separat beslutat arbetsflöde.
Sloträttningen krävde ingen databas- eller framstegsformatändring; v1 använder fortfarande JSON-schema 1.

## Releasegranskning

`CatalogReleaseAudit` kontrollerar de 27 produktkatalogerna och skriver ett
maskinläsbart resultat med målgrupper, handkrav, undantag, fraktionsspecifika
luckor och okända questfraktioner:

```powershell
dotnet run --project BISTracker.Checks/BISTracker.Checks.csproj -c Release -- --catalog-release-audit artifacts/beta2-slot-audit.json
```

Använd en ny rapportfil vid omkörning så att v1-granskningen bevaras.
Exitkod 1 och `releaseReady: false` betyder att katalogerna blockerar delning.
Godkända beteendekontroller betyder att kontrollen fungerar; det betyder inte
att katalogerna är fullständiga. Full slotgranskning, alla tester, renderad
UI-kontroll och distributionstest måste vara godkända före delning.

`EquipmentPlanScenarios` verifierar gruppering, saknade slots, ägda alternativ,
kapacitet för ringar/trinkets, unique, handkombinationer och fraktionsisolering.
Release-build för modelländringen passerar utan varningar/fel och projektets
49 beteendekontroller är godkända, inklusive de tre distributionsscenarierna.
Den slutliga
[releasegranskningen](data/forever-slot-release-audit.json) passerar för alla
27 kataloger, med noll luckor och noll okända questfraktioner. Den separata
[integrationskontrollen](data/forever-slot-witness-integration.json) verifierar
54 faktiska fraktionsset från de tre källgranskningarna mot produktkatalogerna.
Questkedjor, valbara belöningar, recept, bindning och yrkeskrav granskas där;
integrationskontrollen ersätter inte de källbevisen eller spelklientkontroll.
Den slutliga [UI-kontrollen](previews/slots-ui-checks.txt) passerar med verkliga
Mage-/Rogue-/Hunter-kataloger och isolerat framsteg. Det separata
[distributionstestet](distribution.md) är också godkänt för v1: normala
appens faktiska checkbox, diskpersistens, två omstarter och samma ZIP i en ny
programmapp på Windows 11 Pro 10.0.26200. Ordinarie spelarprofil var oförändrad.
[Paketeringsrapporten](data/portable-release-verification.json) gäller exakt
version 1.0.0; ren mottagardator och migration mellan olika versioner är oprövade.

Källarbetet genomfördes av tre specialisttrådar med nio kataloger var och
huvudtrådens gemensamma integration. För nästa agent är de sparade rapporterna
underlag; fortsatt samordning och nya krav följer [överlämningen](beta2-handoff.md).

## Acceptans vid nästa katalog- eller UI-ändring

Bevara ett mål per relevant slot, faktisk variantkapacitet, unique på bas-ID,
handplan, fraktionsseparation och granskade crafting-/questvillkor. Katalogens
metadataändringar ska slå igenom även om rekommendations-ID:n behålls. Vid
ändrade identiteter eller sparformat behöver migreringsbeteendet beslutas och
verifieras; befintliga filer får inte skrivas över för att få testet att passera.

Kör betydelsefulla domän-/lagringsscenarier, den fulla kataloggranskningen,
setintegrationen och relevant UI-kontroll för den faktiska ändringen. Inför
nästa delbara paket behövs ett nytt distributionstest. Fraktionsväljare,
antalshantering och andra öppna produktförslag är inte redan implementerade
krav för appbeta2; Chefen beslutar scope i [beta2-handoff.md](beta2-handoff.md).
