# Karaktärer och tre speclistor

Statusdatum: 2026-10-08. Karaktärer, tre speclistor per katalogset och
27 Forever-kataloger för betans nivå 30 är implementerade och verifierade.
Katalogerna innehåller källbelagda guidealternativ som grupperas till slotmål.
Verkliga nivå 60-kataloger saknas; importstödet är förberett och verifierat.
Se [nivå 30-katalogerna](forever-level30-catalogs.md) och
[katalogset och import](catalog-contexts.md).

## Beslut och beteende

REQ-005–007 omfattar Forever, alla klasser/specs och tre listor per karaktär.
Chefen bekräftade gemensamt ägande och separat utrustning per spec och
beställde därefter nivå 30-kataloger med möjlighet att importera nivå 60.

- En karaktär har ett stabilt lokalt GUID, namn, spelversion och klass.
- Classic och Forever stöds som separata versioner, med nio klasser och 27 specs.
  Druid har Balance/Feral/Restoration; Feral delas inte i ytterligare listor.
  Rogue använder Combat. Varje klass har exakt tre specidentiteter.
- Namn, version och klass väljs vid skapande. Dessa ändras inte genom specbyte.
  Namnet behöver inte vara unikt; realm/ruleset krävs inte för ett lokalt ID.
- Ägande identifierar item-ID plus suffix och delas inom en karaktär mellan
  dess specs och katalogset. Rekommendations-ID och slot kan skilja sig mellan
  listorna för samma item. Karaktärer och spelversioner delar aldrig ägande.
- Varje katalogset har tre egna slot→rekommendation-uppsättningar, en per spec.
  Utrustad innebär ägd. Vid nivåbyte arkiveras tidigare listor och återställs
  när spelaren byter tillbaka. Avmarkera utrustad påverkar bara vald spec i
  valt set; avmarkera ägd rensar itemvariantens utrustning i samtliga specs
  och arkiverade set.
- Tvåhandsvapen och offhand kan inte vara utrustade samtidigt i en spec.
  Utrusta det ena avmarkerar det andra, med ägandet och andra specs bevarade.
  Unique-equipped gäller basitem-ID oavsett suffix: en ny variant ersätter
  tidigare placering av samma unika basitem i den valda specen.
- Sökning/filter återställs vid byte. Aktiv karaktär, vald spec per karaktär
  och aktivt katalogset sparas. Misslyckad sparning lämnar framsteg och val
  oförändrade.

Ägandet registrerar ett känt exemplar per item-ID/suffix. Samma variant
flyttas mellan möjliga ring-/trinket-/handplaceringar vid utrustningsbyte;
den räknas aldrig som två fyllda mål. Ett äldre tillstånd med samma variant
utrustad två gånger avvisas och filen bevaras. Antal exemplar och en sådan
migration kräver ett separat beslutat flöde. Se [slotreglerna](slot-catalog-correctness.md).

## Källor och tillgängliga listor

Spelversion, klass, spec och katalogset väljer katalog via `ICharacterCatalog`.
`CharacterCatalog` returnerar den granskade 17-item-katalogen för Classic Holy
Priest och separata Forever-kataloger för samtliga 27 specs på nivå 30,
beta/patch 1.60.1. Övriga Classic-specs och Forever nivå 60 utan import har
uttrycklig otillgänglig katalog; Vanilla-items används inte som Forever-data.

Forever visar Dungeon-, Quest- och Crafting-rader enligt Chefens godkännande;
Classic visar fortfarande endast Dungeon/Quest. Andra anskaffningstyper kan
finnas i granskat källunderlag men filtreras bort av standardpolicyn. Suffix,
faction-villkor, krav och källlänkar bevaras. Saknade slots fylls inte med
gissade ersättare. Katalogerna är guidebaserade alternativ, inte en egen
simulering eller en garanti om ett komplett optimalt set. Källor och
individuella luckor finns i [nivå 30-rapporten](forever-level30-catalogs.md).

Nivå 60-importen validerar katalogkontext, klass/spec, källreferenser och items.
Dess verifiering använder uttryckliga testfixtures; ingen fiktiv nivå 60-lista
följer med produkten. Se [import och katalogbyte](catalog-contexts.md).

Blizzards [klassöversikt](https://news.blizzard.com/en-us/article/24304075/create-the-hero-you-want-to-be-in-world-of-warcraft-forever)
och [Icy Veins specnavigation](https://www.icy-veins.com/wow-forever/fire-mage-ranged-dps-pve-guide/)
kontrollerades vid den första genomförbarhetsgranskningen 2026-10-08.
De stöder klass/spec-identiteterna. Den efterföljande kataloggranskningen
använder ursprungliga nivå 30-guider och Forever-itemmetadata; betans
rekommendationer presenteras inte som slutliga nivå 60-rankningar.

## Ansvar i koden

- Domain/Game: `CharacterDefinition`, `GameVersion`, `CharacterClass`,
  stabila spec-ID:n och `CatalogSet`. Domain/Tracking: `CharacterLoadouts`
  skyddar gemensamt ägande, tre giltiga listor, slotmatchning, utrustad→ägd,
  unique-regler och tvåhands/offhand-konflikter.
- Application/Characters: `CharacterTrackerService` orkestrerar migration,
  skapande, karaktär/spec/set-val och tracking. Alla karaktärer och sparade
  set valideras före mutation/sparning. Snapshot returneras först efter
  lyckad lagring. `ICharacterTrackerService` är UI-kontraktet;
  `IWorkspaceRepository` är lagringsgränsen.
- Infrastructure/Catalog: `CharacterCatalog` laddar inbyggda och importerade
  paket; `ForeverCatalogReader` validerar kontext, metadata och källpolicy.
  Infrastructure/Persistence: `JsonWorkspaceRepository` hanterar schema,
  filåtkomst och atomisk ersättning.
- Presentation: tracking-ViewModel, karaktär/spec/set-val och import i skalet,
  samt `Features/Characters/Views/CharacterDialog` för skapande. UI
  duplicerar inte regler för ägande eller utrustning.

Den äldre `CharacterProgress`/`TrackerService`-vägen används fortfarande för
legacy-validering och befintliga kontroller; normal app använder nya aggregatet.

## Lagring och migration

Normal app använder `%LOCALAPPDATA%\BISTracker\characters-v1.json` med
`schemaVersion: 1`. Schemat sparar aktiva karaktärens GUID och varje karaktärs
namn, version, klass, valda spec, ägda itemnycklar, aktivt katalogset och
utrustning för aktiva och arkiverade set. Äldre schema 1-filer utan
`catalogSetId`/`archivedLoadouts` läses med versionens standardset och
bevarar identitet och framsteg.

Om filen saknas skapas “My Priest” för Classic, med Holy vald. Befintlig
`holy-priest-classic-phase1-progress.json` läses, valideras mot den riktiga
katalogen och kopieras till Holy-listan. Item-ID/suffix-nycklar härleds från
katalogen. Discipline/Shadow startar med tom utrustning. Den gamla filen skrivs
aldrig av den nya tjänsten. `draft-progress.json` läses inte som itemägande.

Migration sker en gång: en befintlig ny fil används även om legacy-filen senare
ändras. Ett fel i legacy-data hindrar migration och lämnar båda filer bevarade.
Korrupt workspace, okänd schemaversion, borttaget sparat katalogset eller
inkonsekvent domäntillstånd rapporteras; ingen tyst återställning görs.
Det gäller även en tidigare sparad utrustningsreferens till ett item som
uteslutits efter källrevision. Originalfilen bevaras; appen raderar inte
Chefens eller spelarens tidigare val för att få valideringen att passera.
Sparning använder temporär fil i samma katalog, flush och File.Replace/File.Move.
Tempfiler städas vid fel.

Tjänstens operationer serialiseras i en appinstans. Samtidig redigering från
flera appinstanser är inte samordnad. Efter migration ska den nya appen användas;
ändringar med en äldre appversion överförs inte automatiskt igen.
Ingen databas har införts.

## Verifiering och begränsningar

46 konsolkontroller passerar. De verifierar version/klass/spec, alla 27
Forever-kataloger och godkänd D/Q/Crafting-policy, fysisk itemidentitet och suffix,
tre listor per set efter omstart, karaktärsisolering, migration,
nivå 60-import, nivåbyte, gemensamt ägande över arkiverade set, unique-regler
och tvåhands/offhand-konflikter. Sparfel, avbrott, korrupt data och låsta filer
kontrolleras. Regressionerna visar att sparfel vid nivåbyte eller borttaget
ägande inte ändrar aktiva eller arkiverade listor.

Den isolerade WinUI-kontrollen använder minneslagring och testkataloger för
import. Den kontrollerar riktiga Classic-listan, Forever nivå 30, karaktär/
spec/set-val, återställda val efter sparfel och nivå 60-import utan att röra
spelarens framstegsfil. Se [verifieringen](verification.md).

Katalogerna har verifierade kompletta fraktionsset; beta-data kan ändras.
Verkliga nivå 60-items återstår. Ingen spelintegration, molnsynkronisering,
radering eller redigering av karaktärsidentitet ingår.
