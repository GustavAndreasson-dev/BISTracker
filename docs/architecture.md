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
och kopplar katalog, repository och TrackerService till TrackerViewModel.

## Domängränser i utkastet

- Utrustningskatalog: itemidentitet, utrustningsplats och var ett item kan erhållas.
- BiS-rekommendationer: vilka items som rekommenderas för en given klass, specialisering och spelkontext, samt rekommendationens källa.
- Framsteg: spelarens registrerade framsteg mot en vald rekommendation.

Detta är logiska gränser inom samma app. CharacterProgress är aggregatet som
skyddar ägande/utrustning. Katalogen är referensdata. Djupare rekommendationsregler
utvecklas när riktiga källor och urvalsmetod är beslutade.

### Invariants och användningsfall

- Utrustad innebär erhållen; avmarkering av erhållen tar bort aktiv utrustning för itemet.
- Avmarkering av utrustad behåller erhållen.
- Ett item per EquipmentSlot kan vara utrustat. Ring- och trinketplatser är separata slotvärden i utkastet.
- Okända katalog-ID:n och inkonsekvent återställt tillstånd avvisas.
- TrackerService serialiserar läs/ändra/spara och returnerar snapshot först efter lyckad sparning.
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
