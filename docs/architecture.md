# Arkitektur och domänspråk

Status: arkitekturen nedan är implementerad i första utkastet. DDD och SOLID är beslutade principer.
Mappindelningen inom lagren ska följa [strukturkartan](project-structure.md).
WinUI-presentationen använder C# och .NET 8, med separata domän-, applikations- och infrastrukturprojekt.

## Implementerade lager

| Projekt | Ansvar | Tillåtna beroenden |
| --- | --- | --- |
| BISTracker.Domain | Domänbegrepp, värdeobjekt, invariants och regler. | Inga andra BISTracker-lager. |
| BISTracker.Application | Användningsfall och kontrakt för lagring eller dataåtkomst som dessa behöver. | Domain. |
| BISTracker.Infrastructure | Implementationer av lagring och eventuella dataintegrationer. | Application och Domain. |
| BISTracker.Presentation | WinUI-vyer, presentationslogik och koppling till användningsfall. | Application; Infrastructure i appens composition root för att koppla implementationer. |

Presentation kan använda domäntyper när det är motiverat, men får inte duplicera
domänregler. Filformat och externa datamodeller översätts vid infrastrukturgränsen.
Presentation använder MVVM för listan och filter. App.xaml.cs är composition root
och kopplar CharacterCatalog, JsonWorkspaceRepository och CharacterTrackerService
till TrackerViewModel genom ICharacterTrackerService.

## Domängränser i utkastet

- Utrustningskatalog: itemidentitet, utrustningsplats och var ett item kan erhållas.
- BiS-rekommendationer: vilka items som rekommenderas för en given klass, specialisering och spelkontext, samt rekommendationens källa.
- Framsteg: spelarens registrerade framsteg mot en vald rekommendation.

Detta är logiska gränser inom samma app. CharacterLoadouts är aggregatet som
skyddar gemensamt ägande och tre separata utrustningslistor per karaktär.
CharacterProgress finns kvar för legacy-validering. Katalogen är referensdata. Djupare rekommendationsregler
utvecklas när riktiga källor och urvalsmetod är beslutade.

### Invariants och användningsfall

- Utrustad innebär erhållen; avmarkering av erhållen tar bort aktiv utrustning för itemet.
- Avmarkering av utrustad behåller erhållen.
- Ett item per EquipmentSlot kan vara utrustat. Ring- och trinketplatser är separata slotvärden i utkastet.
- Okända katalog-ID:n och inkonsekvent återställt tillstånd avvisas.
- CharacterTrackerService serialiserar läs/ändra/spara, validerar alla karaktärer och returnerar snapshot först efter lyckad sparning.
- JSON-lagringen skriver temporär fil och ersätter målet. Korrupt data rapporteras och bevaras.
- UI visar fel vid läs- eller skrivproblem och uppdaterar endast framsteg efter lyckat användningsfall.

## Gemensamt språk

| Begrepp | Arbetsdefinition |
| --- | --- |
| Item | Ett identifierbart utrustningsföremål. |
| Equipment slot | Utrustningsplats; hantering av flera platser av samma typ behöver modelleras uttryckligt. |
| BiS recommendation | En källbelagd rekommendation för en definierad spelkontext, inte en universell ranking. |
| Pre-raid | Avgränsning vars tillåtna itemkällor ska bestämmas innan listan tas fram. |
| Game context | Spelvariant eller expansion samt den fas/tillgänglighet som styr rekommendationen. |
| Progress | Spelarens erhållna och utrustade items mot en rekommendation. |

## Utbyggnad till fler expansioner

Spelkontext ska vara uttrycklig i data och rekommendationer. Framtida expansioners
regler ska kunna skiljas från Vanilla-regler. Implementera endast de variationer
som behövs nu; generalisera vid konkreta nya krav.

## Verifiering av arkitekturen

Vid kodgranskning kontrolleras projektberoenden, placeringen av domänregler och
att användningsfall kan verifieras utan WinUI. Kritiska invariants testas i
domänen; lagringskontrakt verifieras mot den valda implementationen.

## Riktig katalog och variantidentitet

ClassicPhaseOneBisCatalog läser den granskade produktkatalogen som inbyggd
resurs i Infrastructure. JSON och dess validering stannar där. ItemDetails
i Domain beskriver Classic-ID, suffix, anskaffningstyp och referenslänkar.
IBisCatalog och TrackerService används utan HTTP-beroenden.

De tre of Healing-målen har variant i både namn och tracking-ID. Domain
avvisar okända ID:n; ett basitem eller demo-ID kan inte räknas som den
nödvändiga varianten. Presentation visar fakta och hämtar externa ikoner
med fallback. Separata framstegsfiler skyddar övergången från fiktiva items.
Se [integrationen](catalog-integration.md) för filansvar och begränsningar.

## Karaktärer och speclistor

En karaktär har stabilt ID, version och klass. CharacterDefinition anger exakt
tre giltiga spec-ID:n för klassen. CharacterLoadouts skiljer gemensamt ägande
(item-ID/suffix inom karaktärens version) från utrustning (rekommendations-ID i
varje spec). Borttaget ägande rensar motsvarande utrustning i alla tre specs.
Listorna och ägandet isoleras mellan karaktärer. Katalogkontraktet väljer uttrycklig
version/klass/spec och kan ange att granskad data saknas.

Application orkestrerar migration och tillståndsbyten genom IWorkspaceRepository.
Infrastructure sköter JSON och atomisk filersättning. Gamla framsteg kopieras en
gång utan att originalet ändras. Reglerna kräver varken UI eller filsystem för
att verifieras. Detaljer och begränsningar finns i [listmodellen](characters-and-loadouts.md).
