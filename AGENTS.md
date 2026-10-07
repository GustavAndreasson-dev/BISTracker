# Arbetsregler för BISTracker

## Ansvar och scope

- Användaren beslutar om produktens riktning och krav. Huvudtråden samordnar planering, arkitektur, uppgifter och granskning.
- Första versionen gäller Holy Priest i vanliga WoW Classic (Vanilla), endast pre-raid BiS.
- Fler expansioner ska kunna stödjas senare. Implementera deras funktioner först när de ingår i ett beslutat scope.
- Läs README.md och relevanta dokument i docs/ före arbete. Skilj beslutade krav från förslag och öppna frågor.
- Skapa eller kontakta andra användarägda trådar endast på användarens uppdrag. Vid delegering ska uppgift, berörda filer, beroenden och acceptanskriterier vara tydliga.

## DDD och SOLID

- Ha en tydlig mappstruktur inom varje projekt enligt docs/project-structure.md. Gruppera kod efter domänområde, användningsfall och tekniskt ansvar. Använd inte projektroten som samlingsplats för alla klasser.
- Placera nya filer i rätt ansvarsområde och uppdatera strukturkartan när ett område tillkommer. Skapa inte tomma mappar eller generiska Helpers/Utils-mappar utan tydligt ansvar.

- Domänmodellen ska uttrycka utrustning, BiS-rekommendationer och spelarens framsteg med ett gemensamt språk.
- Domänregler ska ligga i domänlagret och kunna testas utan UI, filsystem, databas eller externa tjänster.
- Applikationslagret orkestrerar användningsfall. Infrastruktur implementerar lagring och externa integrationer. Presentation visar information och hanterar användarinteraktion.
- Beroenden ska gå mot domän och användningsfall. Domänen ska inte referera till övriga lager.
- Följ SOLID: avgränsade ansvar, utbytbara implementationer, små ändamålsenliga kontrakt och beroenden mot relevanta abstraktioner.
- Inför abstraktioner där de skyddar en verklig gräns eller variation. Undvik generiska ramverk och framtida funktioner utan konkret behov.
- Gränser och aggregat ska motiveras av domänens regler; mappar och projektnamn räcker inte som DDD.

## Data, verifiering och dokumentation

- BiS-data måste ha källa och en uttrycklig spelversion samt tillgänglighetskontext. Hitta inte på items, rankingar eller spelregler.
- Bekräfta fas, tillåtna itemkällor och urvalsmetod innan en skarp BiS-lista fastställs.
- Ge beslutade produktkrav stabila ID:n och koppla dem till faktisk kod och relevant verifiering i docs/requirements.md.
- Uppdatera berörda dokument tillsammans med förändringar i krav, arkitektur eller implementation.
- Testa betydelsefulla domänregler, användningsfall och lagringsbeteenden. Kör relevanta kontroller för ändringen och redovisa begränsningar.
- Markera en uppgift klar först när implementationen och dess acceptanskriterier har verifierats. Dokumenterat eller planerat betyder inte implementerat.
- Bevara användarens befintliga ändringar och undvik orelaterade refaktoreringar.

## Git och commits

- Gör commits regelbundet när en sammanhängande del är avklarad och relevant verifiering är godkänd. Samla inte flera färdiga delar till en stor slutcommit.
- Varje commit ska ha ett tydligt syfte och ett beskrivande meddelande. Håll ihop kod, relevant dokumentation och kontroller för samma ändring.
- Granska det som är staged före commit och inkludera endast avsedda filer. Byggutdata, IDE-filer och användarens framstegsdata ska inte versionshanteras.
- Huvudtråden samordnar commits i den delade projektmappen. Andra trådar följer sitt uppdrag och rapporterar färdiga delar för integration och commit.
