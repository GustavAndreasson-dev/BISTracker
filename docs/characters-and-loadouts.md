# Karaktärer och tre speclistor

Statusdatum: 2026-10-08. Funktionerna nedan är implementerade. Nya BiS-kataloger
är en separat återstående leverans; stöd för klass/spec betyder inte att dess
items redan har granskats.

## Beslut och beteende

REQ-005–007 omfattar Forever, alla klasser/specs och tre listor per karaktär.
Användaren bekräftade gemensamt ägande och separat utrustning per spec.

- En karaktär har ett stabilt lokalt GUID, namn, spelversion och klass.
- Classic och Forever stöds som separata versioner, med nio klasser och 27 specs.
  Druid har Balance/Feral/Restoration; Feral delas inte i ytterligare listor.
  Rogue använder Combat. Varje klass har exakt tre specidentiteter.
- Namn, version och klass väljs vid skapande. Dessa ändras inte genom specbyte.
  Namnet behöver inte vara unikt; realm/ruleset krävs inte för ett lokalt ID.
- Ägande identifierar item-ID plus suffix och delas inom en karaktär.
  Rekommendations-ID och slot kan skilja sig mellan listorna för samma item.
  Karaktärens spelversion avgränsar itemidentiteten; samma ID i en annan
  karaktär eller version överför aldrig ägande.
- Varje spec har en egen slot→rekommendation-uppsättning. Utrustad innebär
  ägd. Avmarkera utrustad påverkar bara vald spec; avmarkera ägd tar bort
  itemvariantens utrustningsmarkeringar i samtliga tre specs.
- Sökning/filter återställs vid byte. Aktiv karaktär och senast vald spec per
  karaktär sparas. Misslyckad sparning lämnar både framsteg och val oförändrade.

## Källor och tillgängliga listor

Spelversion, klass och spec väljer katalog via `ICharacterCatalog`.
`CharacterCatalog` returnerar den befintliga granskade 17-item-katalogen för
Classic Holy Priest. Övriga val har explicit otillgänglig katalog, inga
rekommendationer och texten “No reviewed BiS list available yet.”

Blizzards [klassöversikt](https://news.blizzard.com/en-us/article/24304075/create-the-hero-you-want-to-be-in-world-of-warcraft-forever)
anger nio klasser. [Icy Veins ursprungliga Forever-guide och klassnavigation](https://www.icy-veins.com/wow-forever/fire-mage-ranged-dps-pve-guide/)
listar de 27 specnamnen, inklusive Feral och Combat. Kontrollerat 2026-10-08.
Guidens gearsektion gäller nivå 30 i betan och är inte underlag för kompletta
pre-raid-listor på högsta nivå. Se [utredningen](forever-feasibility.md).

Inga nya items, rankingar eller spelregler har lagts till. Dungeons/quests är
fortsatt arbetsavgränsning för katalogarbetet. Fas, nivå/build, källor och
urvalsmetod måste granskas innan varje ny katalog aktiveras.

## Ansvar i koden

- Domain/Game: `CharacterDefinition`, `GameVersion`, `CharacterClass` och
  stabila spec-ID:n. Domain/Tracking: `CharacterLoadouts` skyddar gemensamt
  ägande, tre giltiga listor, slotmatchning och utrustad→ägd.
- Application/Characters: `CharacterTrackerService` orkestrerar migration,
  skapande, val och tracking. Alla karaktärer valideras före mutation/sparning.
  Snapshot returneras först efter lyckad lagring. `ICharacterTrackerService`
  är UI-kontraktet; `IWorkspaceRepository` är lagringsgränsen.
- Infrastructure/Catalog: version/klass/spec avgör vilken granskad katalog som
  får användas. Infrastructure/Persistence: `JsonWorkspaceRepository` hanterar
  schema, filåtkomst och atomisk ersättning.
- Presentation: befintlig tracking-ViewModel, väljare i skalet och
  `Features/Characters/Views/CharacterDialog` för skapande. UI duplicerar inte
  regler för ägande eller utrustning.

Den äldre `CharacterProgress`/`TrackerService`-vägen används fortfarande för
legacy-validering och befintliga kontroller; normal app använder nya aggregatet.

## Lagring och migration

Normal app använder `%LOCALAPPDATA%\BISTracker\characters-v1.json` med
`schemaVersion: 1`. Schemat sparar aktiva karaktärens GUID och varje karaktärs
namn, version, klass, valda spec, ägda itemnycklar och tre utrustningslistor.

Om filen saknas skapas “My Priest” för Classic, med Holy vald. Befintlig
`holy-priest-classic-phase1-progress.json` läses, valideras mot den riktiga
katalogen och kopieras till Holy-listan. Item-ID/suffix-nycklar härleds från
katalogen. Discipline/Shadow startar med tom utrustning. Den gamla filen skrivs
aldrig av den nya tjänsten. `draft-progress.json` läses inte som itemägande.

Migration sker en gång: en befintlig ny fil används även om legacy-filen senare
ändras. Ett fel i legacy-data hindrar migration och lämnar båda filer bevarade.
Korrupt workspace, okänd schemaversion eller inkonsekvent domäntillstånd
rapporteras; ingen tyst återställning görs. Sparning använder temporär fil i
samma katalog, flush och File.Replace/File.Move. Tempfiler städas vid fel.

Tjänstens operationer serialiseras i en appinstans. Samtidig redigering från
flera appinstanser är inte samordnad. Efter migration ska den nya appen användas;
ändringar med en äldre appversion överförs inte automatiskt igen.

## Verifiering och begränsningar

26 konsolscenarier passerar, varav nio nya i `CharacterScenarios`. De verifierar
version/klass/spec, fysisk itemidentitet, suffix, tre olika listor efter omstart,
karaktärsisolering, migration, sparfel, avbrott, korrupt data, låst målfil och
domänvalidering. Fiktiva kataloger används endast i kontrollerna för att kunna
testa gemensamma items utan att publicera obekräftad BiS-data.

WinUI kontrolleras med helt isolerad minneslagring: riktiga listan, faktiska
karaktärs-/specvalhändelser, återställda val efter sparfel, tomma kataloger,
Forever Mage, återläsning och nykaraktärsdialog. Se [verifieringen](verification.md).

Fulla kataloger för nya specs återstår. Regler för antal identiska ringar/trinkets,
unika items, tvåhandsvapen och dual wield behöver granskas med respektive
katalog innan sådana rekommendationer införs. Ingen spelintegration,
molnsynkronisering, radering eller redigering av karaktärsidentitet ingår.
