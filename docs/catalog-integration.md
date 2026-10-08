# Riktig fas 1-katalog

Statusdatum: 2026-10-08. Starta nästa agents arbete i
[beta2-handoff.md](beta2-handoff.md). Denna fil beskriver hur den bevarade
Classic Holy Priest-katalogen ingår i dagens karaktärs- och katalogflöde.
Beta 2-funktioner är ännu inte beslutade av Chefen.

Baslinjen är portabel **1.0.0**, kod/runtime `e25c134` och dokumentations-
revision `043d859`. [Leveransrapporten](data/portable-release-verification.json)
redovisar 49 godkända beteendekontroller och produktions-UI med faktisk
checkbox, sparning, omstart och programmapputbyte på Windows 11. Ren dator,
Windows 10 och verklig nivå 60-data är inte verifierade.

## Data och ansvar

ClassicPhaseOneBisCatalog under Infrastructure/Catalog läser en inbyggd,
versionshanterad JSON-katalog från Catalog/Data/holy-priest-classic-phase1.json.
Den innehåller 17 huvudval från phase1-items.md med engelska namn, anskaffning,
villkor, ikon-URL och separata länkar till item och rekommendationsguide.
Ingen HTTP-hämtning krävs för att läsa katalogen.

Data kopierades från det verifierade metadataunderlaget och kompletterades
med de granskade anskaffningsvillkoren 2026-10-07. Forskningsfilerna under
docs/data är separat underlag och läses inte av appen. Ändrade huvudval ska
granskas mot fas 1 och endast dungeons/quests innan produktfilen uppdateras.

Domain/Equipment/ItemDetails beskriver Classic-ID, nödvändigt suffix,
anskaffningstyp och referenslänkar; den gemensamma modellen har också
vapenhand, Unique och fraktionsvillkor för Forever. Domain refererar inte
till JSON eller HTTP. `ClassicPhaseOneBisCatalog` implementerar fortfarande
`IBisCatalog`, men normal apps Application-flöde använder nu
`ICharacterCatalog`/`CharacterCatalog` för val av version, klass, spec och
katalogset. Bara Classic Holy Priest har granskad Classic-data; övriga
Classic-specs visar uttrycklig datastatus.

Forever har egna beta nivå 30-kataloger för alla 27 specs med dungeons,
quests och crafting, samt 54 verifierade fraktionsset. Classic-filtreringen
är fortsatt endast dungeons/quests. Classic-listan återanvänds inte som
Forever-data; se [forever-level30-catalogs.md](forever-level30-catalogs.md).

## Identitet och framsteg

- Vanliga mål har ID `classic-p1-item-{itemId}`.
- De tre slumpbonusmålen har ID `classic-p1-item-{itemId}-of-healing` och
  fullständigt namn med **of Healing**. Ett annat suffix uppfyller inte målet.
- Tracking är manuell; automatiskt suffix-ID eller spelinventarie analyseras inte.
- Normal app använder nu `characters-v1.json` i `%LOCALAPPDATA%/BISTracker`,
  med gemensamt item/suffix-ägande per karaktär och separat utrustning per
  spec/katalogset. Den äldre `holy-priest-classic-phase1-progress.json`
  importeras en gång när ett nytt workspace skapas och bevaras därefter.
  Gamla `draft-progress.json` bevaras och demoägande överförs inte.
  `BISTRACKER_DATA_DIRECTORY` kan välja en absolut isolerad datakatalog
  för verifiering; den normala sökvägen ska inte ändras för spelarens profil.
- Katalogen har ett huvudval per slot. Bonecreeper Stylus anges som
  dungeonalternativ i Stormragers villkor, utan en ytterligare trackingrad.

## Verifiering av katalog och lagring

Den ursprungliga katalogintegrationen passerade 17 konsolkontroller, bland
annat riktig katalog med alla slots/källreferenser, suffixidentitet med
avvisade bas-/demo-ID:n och återläsning utan ändring av demofilen. Detta är
historik, inte dagens antal. Aktuell leverans har 49 godkända kontroller
som även omfattar karaktärer, delat ägande, katalogset, nivå 60-import med
testfixturer, slotkapacitet, utrustningsregler och isolerad datakatalog.
Se [verification.md](verification.md).

## Koppling och presentation

App.xaml.cs komponerar `CharacterTrackerService`, `CharacterCatalog`,
`JsonWorkspaceRepository`, legacy-läsaren och `CatalogPackImporter`.
Första start skapar/importerar “My Priest” i Classic Holy när inget
workspace finns. Den isolerade previewkontrollen använder riktiga kataloger
med minneslagring; `DRAFT_PREVIEW` ingår bara med explicit byggflagga.
`SampleBisCatalog` används för regressioner och väljs inte av normal app.
Klassen finns fortfarande i Infrastructure-assemblyn; det innebär inte
att demodata eller previewprofil aktiveras i produktionsflödet.

TrackerItemViewModel ger vyn namn, anskaffning, fullständiga villkor och
item-/guidelänkar. Ikoner hämtas av WinUI när rader visas. ImageFailed lämnar
en läsbar platsmarkering, utan att blockera katalog eller tracking. Sökning
omfattar även anskaffning, till exempel ett dungeon- eller bossnamn.

UI-kontrollen verifierar riktig katalog, tracking, filter, sökning, ikonhämtning,
en avsiktligt saknad bild, korrekta ikoner efter radåteranvändning, suffixvillkor
och Stormragers fraktionsvillkor. Renderade bilder finns under docs/previews.
Källknapparnas NavigateUri är kontrollerade; externa webbläsare öppnas inte
av den automatiska kontrollen.

`TrackerSlotViewModel` grupperar alternativ till ett mål per relevant slot.
Classic har fortsatt 17 mål med ett huvudval vardera. Forever-alternativ
räknas inte som extra mål; `EquipmentPlan` kontrollerar samtidig kapacitet,
fraktionsuppsättningar och handkrav. Ägt och utrustat tvåhandsval använder
konsekventa slotmål. Import i samma aktiva kontext uppdaterar även metadata
med oförändrade rekommendations-ID:n.

## Avgränsning

17 huvudval trackas. Alternativet Bonecreeper Stylus är information i wand-raden,
inte ett separat trackingmål. UBRS ingår med tiomannavillkor. Alliance-vägen
till Stormrager är en Raid-quest utomhus och detta visas uttryckligen.
Ingen automatisk inventoryimport, full questkedjekontroll eller ny statranking
har införts. Unika items förekommer bara en gång i den fasta Classic-
katalogen. Gemensamma regler för omflyttbara alternativ är nu införda:
ägande registrerar en kopia per item/suffix, samma variant flyttas mellan
ring-/trinketplatser, Unique gäller basitem-ID och tvåhand utesluter offhand.

## Bevara inför beta 2

Ändra inte Classic-urval, lagringsidentitet eller migrationsregler utifrån
äldre researchförslag. En ny begäran behöver spåras till Chefens beslut
och relevant källgranskning. Koppla ny katalogdata till rätt version/nivå/
spec, behåll spelarens framsteg och kör berörda katalog-, lagrings- och
UI-kontroller enligt [beta2-handoff.md](beta2-handoff.md). Importvägen för
verklig Forever nivå 60 är färdig, men sådan produktdata finns ännu inte.
