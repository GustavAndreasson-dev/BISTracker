# Arbetsregler för BISTracker

## Startpunkt inför beta 2

- Läs [README](README.md) och [överlämningen inför beta 2](docs/beta2-handoff.md)
  innan fortsatt arbete. Krav och beslut har företräde framför historiska
  utkastuppdrag och researchförslag.
- Nuläge 2026-10-08: version 1.0.0 är paketerad som portabel Windows x64-ZIP
  och lokalt verifierad. 49 beteendekontroller, 27 Forever-kataloger och
  54 fraktionsset är godkända. Ren mottagardator och verklig Forever nivå
  60-data återstår. Nästa appbeta har ännu inga beslutade nya funktioner.
- Arbeta lokalt i detta repo. Boromir och gamla chatt-ID:n är historik,
  inte beroenden som måste återstartas eller kontaktas.

## Ansvar och scope

- Projektägaren ska benämnas Chefen eller Bossen, inte "Användaren". Använd "spelaren" när dokumentationen beskriver appens slutanvändare.

- Chefen beslutar om produktens riktning och krav. Huvudtråden samordnar planering, arkitektur, uppgifter och granskning.
- Classic-urvalet gäller Holy Priest i vanliga WoW Classic (Vanilla), fas 1, endast pre-raid BiS från dungeons och quests. Crafting och köpta/world-drop BoE-items ingår inte i Classic-listan.
- Forever är beslutat scope: nivå 30-beta/patch 1.60.1 för nio klasser och 27 specs. Dungeon/Quest/Crafting är godkänt där. Nivå 60-import är implementerad men verklig data saknas; blanda inte spelversionernas källpolicy.
- Appens användarsynliga texter ska vara på engelska. Samtalet och projektdokumentationen kan vara på svenska.
- Fler expansioner ska kunna stödjas senare. Implementera deras funktioner först när de ingår i ett beslutat scope.
- Läs README.md och relevanta dokument i docs/ före arbete. Skilj beslutade krav från förslag och öppna frågor.
- Skapa eller kontakta andra användarägda trådar endast på Chefens uppdrag. Vid delegering ska uppgift, berörda filer, beroenden och acceptanskriterier vara tydliga.
- Chefens arbetsform för pågående projektfortsättning är fyra arbetstrådar:
  huvudagent och tre medhjälpare med tydliga filansvar. Huvudagenten ansvarar
  för integration och kontroll; en delrapport ersätter inte egen slutgranskning.

## Lokal körning och säkerhet

- Arbetsmiljön är Windows och PowerShell. Kör relevant `dotnet build` efter
  .NET-ändringar när det är möjligt och redovisa kontroller som inte kunde köras.
- Bevara lokalt arbete, spelarens data och genererade resultat. Kontrollera
  mål och befintligt innehåll före ersättning; inga destruktiva Git-kommandon
  utan Chefens uttryckliga tillstånd.
- Databasändringar kräver Chefens godkännande före implementation. Nuvarande
  lagring är lokal JSON, inte en databas. Schema- och katalogmigrationer
  måste ändå granskas mot sparade framsteg och relaterad C#-kod.
- För isolerade prov används en separat absolut `BISTRACKER_DATA_DIRECTORY`
  bara i testprocessen. Normal app behåller `%LOCALAPPDATA%\BISTracker`.
- UI ska vara kortfattat. Lägg inte till instruktioner eller förklaringar för
  självklar interaktion utan Chefens beställning.

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
- Bevara Chefens befintliga ändringar och undvik orelaterade refaktoreringar.

## Git och commits

- Gör commits regelbundet när en sammanhängande del är avklarad och relevant verifiering är godkänd. Samla inte flera färdiga delar till en stor slutcommit.
- Chefen har uttryckligen beställt regelbundna commits för denna fortsättning.
  Huvudagenten samordnar dem; publicering kräver committad kod och ren
  arbetskatalog. Ett nytt paket måste provas, inte bara få ett nytt filnamn.
- Varje commit ska ha ett tydligt syfte och ett beskrivande meddelande. Håll ihop kod, relevant dokumentation och kontroller för samma ändring.
- Nya arbetsbranches använder prefixet `codex/` om Chefen inte begär ett annat namn.
- Granska det som är staged före commit och inkludera endast avsedda filer. Byggutdata, IDE-filer och spelarens framstegsdata ska inte versionshanteras.
- Huvudtråden samordnar commits i den delade projektmappen. Andra trådar följer sitt uppdrag och rapporterar färdiga delar för integration och commit.
