# Riktig fas 1-katalog

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
anskaffningstyp och referenslänkar. Domain refererar inte till JSON eller HTTP.
Application använder fortsatt IBisCatalog; det befintliga trackingflödet
kan därför användas med den riktiga katalogen.

## Identitet och framsteg

- Vanliga mål har ID `classic-p1-item-{itemId}`.
- De tre slumpbonusmålen har ID `classic-p1-item-{itemId}-of-healing` och
  fullständigt namn med **of Healing**. Ett annat suffix uppfyller inte målet.
- Tracking är manuell; automatiskt suffix-ID eller spelinventarie analyseras inte.
- ProgressFilePaths ger en separat fil, `holy-priest-classic-phase1-progress.json`,
  i appens lokala datakatalog. Gamla `draft-progress.json` bevaras och
  överförs inte till verkligt itemägande.
- Katalogen har ett huvudval per slot. Bonecreeper Stylus anges som
  dungeonalternativ i Stormragers villkor, utan en ytterligare trackingrad.

## Verifiering av katalog och lagring

17 konsolkontroller passerar, inklusive tre nya scenarier: riktig katalog med
alla slots och källreferenser, suffixidentitet med avvisade bas-/demo-ID:n,
samt återläsning av riktiga framsteg utan ändring av demofilen.

## Koppling och presentation

App.xaml.cs använder ClassicPhaseOneBisCatalog som standard. Den isolerade
UI-kontrollen använder samma riktiga katalog med minneslagring. SampleBisCatalog
finns kvar för regressionstester och används inte av normal app.

TrackerItemViewModel ger vyn namn, anskaffning, fullständiga villkor och
item-/guidelänkar. Ikoner hämtas av WinUI när rader visas. ImageFailed lämnar
en läsbar platsmarkering, utan att blockera katalog eller tracking. Sökning
omfattar även anskaffning, till exempel ett dungeon- eller bossnamn.

UI-kontrollen verifierar riktig katalog, tracking, filter, sökning, ikonhämtning,
en avsiktligt saknad bild, korrekta ikoner efter radåteranvändning, suffixvillkor
och Stormragers fraktionsvillkor. Renderade bilder finns under docs/previews.
Källknapparnas NavigateUri är kontrollerade; externa webbläsare öppnas inte
av den automatiska kontrollen.

## Avgränsning

17 huvudval trackas. Alternativet Bonecreeper Stylus är information i wand-raden,
inte ett separat trackingmål. UBRS ingår med tiomannavillkor. Alliance-vägen
till Stormrager är en Raid-quest utomhus och detta visas uttryckligen.
Ingen automatisk inventoryimport, full questkedjekontroll eller ny statranking
har införts. Unika items förekommer bara en gång i den fasta katalogen;
regler för framtida omflyttbara ring-/trinketalternativ är ännu inte införda.
