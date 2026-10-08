# Beslutslogg

Beslut ska ange datum, bakgrund och konsekvens. Förslag dokumenteras i plan eller
arkitektur tills de är beslutade. Chefens senare instruktioner gäller framför
äldre dokumentation; uppdatera loggen när ett beslut ersätts.

Status 2026-10-08: version 1.0.0 är lokalt verifierad och paketerad.
[Överlämningen inför appens beta 2](beta2-handoff.md) skiljer denna leverans
från nästa ännu ospecificerade beta. Äldre beslut nedan är historik när
senare beslut uttryckligen utökar deras scope.

## DEC-001 — Första produktomfånget

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: första versionen visar och trackar pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla).
- Konsekvens vid första beslutet: fler expansioner var framtida stöd. Forever tillkom senare genom DEC-010/011. Classic-fas och itemkällor preciserades i DEC-007. Guidebaserat kandidatunderlag finns i phase1-items.md.

## DEC-002 — Arkitektur och arbetsregler

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: använd DDD-struktur och SOLID-principer, samt Markdown-filer som håller reda på regler och kan jämföras med koden.
- Konsekvens: implementationen ska följas upp mot dokumenterade krav och arkitektur. Utkastets införda domängränser och projektindelning beskrivs i architecture.md och project-structure.md.

## DEC-003 — Samordning

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: huvudtråden har huvudansvaret och tar emot instruktioner om hur projektet ska byggas. Andra trådar kan bidra under samordning.
- Konsekvens: huvudtråden håller den gemensamma planen och granskar hur bidrag passar ihop med krav och arkitektur.

## DEC-004 — Tracking och första utkast

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: tracking ska hantera både erhållna och utrustade items. Huvudtråden ska lägga upp planen, skapa andra Codex-trådar med instruktioner och skapa ett första utkast.
- Konsekvens: arbetet delas efter filansvar enligt draft-contract.md. Utkastet byggs i befintlig WinUI-app med tydligt markerad exempeldata och lokal lagring som genomförandeval.

## DEC-005 — Mappstruktur

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: varje projekt ska ha en god mappstruktur som dokumenteras i Markdown.
- Konsekvens: project-structure.md anger områden och ansvar inom lagren. Nya filer ska placeras enligt kartan och den faktiska strukturen granskas tillsammans med koden.

## DEC-006 — Regelbundna commits

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: gör commits regelbundet efter avklarade delar, i lagom stora grupper som hänger ihop.
- Konsekvens: huvudtråden granskar, verifierar och commitar färdiga delar löpande. Det första utkastets befintliga ändringar delas upp efter domän/användningsfall, lagring/kontroller, UI, källresearch och gemensam projektdokumentation.

## DEC-007 — Fas och tillåtna anskaffningssätt

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: fas 1 gäller. BiS-listan ska endast omfatta dungeons och quests.
- Konsekvens för Classic: crafting och köpta/world-drop BoE-items exkluderas. Detta förblir Classic-policy. Forever får även crafting enligt DEC-012. Moderna Era-itemposter bevisar inte själva historisk fastillgänglighet.

## DEC-008 — Engelskt gränssnitt

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: arbetet med att göra appen på engelska ska startas.
- Konsekvens: användarsynliga texter, tillgänglighetsnamn och status-/feltexter ska vara på engelska. Metadata ska hämtas med engelska itemnamn. Svenska projektdokument behöver inte översättas.

## DEC-009 — Aktivera den riktiga listan

- Datum: 2026-10-07.
- Status: beställt av Chefen och implementerat.
- Beslut: koppla in den granskade riktiga listan i appen.
- Genomförande: 17 huvudval i en inbyggd katalog bakom IBisCatalog, med bilder, anskaffning, villkor och källänkar. Of Healing bevaras i namn och tracking-ID. Bonecreeper Stylus anges som alternativ i wand-raden.
- Genomförandeval: använd en separat fas 1-framstegsfil och bevara demofilen. UBRS ingår som dungeon med tiomannavillkor; Stormragers Raid-quest för Alliance beskrivs uttryckligen. Se catalog-integration.md för verifiering och avgränsning.

## DEC-010 — Forever, alla klasser och tre speclistor

- Datum: 2026-10-08.
- Status: beställt av Chefen; struktur och listfunktion implementerade. Nivå 30-kataloger tillkom enligt DEC-011; nivå 60-data återstår.
- Beslut: utred Blizzards Forever som nästa spelversion, utöka till alla klasser/specs om genomförbart och stöd tre separata listor per karaktär.
- Förtydligande från Chefen: ägande är gemensamt för karaktären, utrustning separat per spec.
- Genomförande: nio klasser med tre specs i Classic/Forever, lokalt karaktärs-ID, gemensam itemvariantidentitet, versionsstyrd JSON och bevarande av äldre framsteg. En otillgänglig katalog visas explicit; inga Vanilla-rekommendationer används som verifierad Forever-BiS. Se characters-and-loadouts.md.
- Commitregel vid denna leverans: inga commits gjordes före explicit begäran. Efterföljande beställning av regelbundna commits beskrivs i DEC-011.

## DEC-011 — Nivå 30 nu, nivå 60-import senare

- Datum: 2026-10-08.
- Status: beställt av Chefen; alla 27 nivå 30-kataloger och nivå 60-import implementerade och verifierade. Verklig nivå 60-data inväntas.
- Beslut: skaffa Forever-kataloger för alla specs på nivå 30 och förbered import av nivå 60 när data finns.
- Arbetsform: utnyttja fyra arbetstrådar och gör regelbundna commits efter verifierade sammanhängande delar. Huvudtråden samordnar integration och commits; tre datatrådar äger varsin grupp om nio specs.
- Källpolicy vid det historiska beslutet: dungeons/quests tills förtydligandet för Forever-beta besvarades. Chefens svar finns nu i DEC-012: crafting tillåts där. Research bevarar andra guidekandidater, men dessa är inte därmed aktiva produktval.
- Senare korrigering: källpolicy och kravet på fullständiga slotmål ändrades enligt DEC-012.
- Genomförande: källbelagda guidealternativ för beta nivå 30/patch 1.60.1, separat nivå 60-kontext, validerad atomisk packimport och bevarade utrustningsuppsättningar. Luckor fylls inte med egna rankingar. Denna historiska del verifierades med 39 beteendekontroller; senare slot-/distributionskontroller ger dagens 49 enligt DEC-012/013. [Leverans](forever-level30-catalogs.md).

## DEC-012 — Korrekt slotmodell och crafting i Forever

- Datum: 2026-10-08.
- Status: implementerat och verifierat; 27 kataloger och 54 fraktionsset godkända. Slotleveransens 46 beteendekontroller utökades till dagens 49 vid DEC-013. UI och lokal portabel distribution är nu godkända; ren mottagardator återstår.
- Beslut: varje relevant slot ska vara ett mål med grupperade alternativ. Antalet rekommendationer får inte visas som antalet slots. Kontrollera alla specs före delning.
- Förtydligande från Chefen: crafting tillåts i Forever-katalogerna för att fylla källbelagda luckor. Classic fas 1 behåller dungeons/quests. World drops, vendor, PvP och reputation ingår inte i det nya tillståndet.
- Konsekvens: komplett set kontrolleras med fysisk itemkapacitet, unique, handuppsättning, fraktion och källgranskade quest-/professionsvillkor. En rad i varje slot är inte i sig bevis. Tidigare delningsbedömning var för tidig; [releasegranskningen](slot-catalog-correctness.md) måste godkännas.

## DEC-013 — Paketera och verifiera första testleveransen

- Datum: 2026-10-08.
- Status: Chefens beställning genomförd lokalt; version 1.0.0 kan delas för vänners testning med dokumenterade begränsningar.
- Beställning: kör paketering och distributionsprov efter kataloggranskningen.
- Genomförandeval: portabel osignerad Windows x64-ZIP med både .NET och Windows App SDK. `PublishTrimmed=false` krävs för befintlig JSON-serialisering; normal leverans saknar DraftPreview. Paketet byggdes från committad kod `e25c134` med ren arbetskatalog; metadata dokumenterades i `043d859`.
- Verifiering: Release-build utan varningar/fel, 49 beteendekontroller, 27 kataloger, ZIP-manifest och 275 PE-binärer. Verklig publicerad WinUI-checkbox sparade/återläste på disk efter omstart och byte av programmapp med samma ZIP. Fem runtime-filer laddades från paketet; ordinarie spelarprofil var oförändrad. [Rapport](data/portable-release-verification.json).
- Avgränsning: Windows 11 Pro 10.0.26200 provad. Ren dator, äldre Windows, signering, MSIX och datamigration mellan framtida versioner är inte verifierade. [Leveransformat och kommandon](distribution.md).

## DEC-014 — Dokumentöverlämning inför appens beta 2

- Datum: 2026-10-08.
- Status: genomfört och dokumentgranskat enligt REQ-012; alla 23 befintliga Markdown-filer uppdaterade och en samlad överlämning tillagd.
- Beställning: uppdatera alla projektspecifika Markdown-filer så en annan agent kan fortsätta från nuvarande läge inför beta 2.
- Genomförande: README/AGENTS och [beta2-handoff.md](beta2-handoff.md) ger startordning, verifierad bas, kodansvar, kommandon och kvarstående arbete. Äldre draft-, research- och överföringsdokument märks som historiska där de inte längre styr arbetet.
- Konsekvens: inga nya beta 2-funktioner, programversionsnummer, källtyper eller migreringsregler beslutas genom denna dokumentation. Nästa release måste verifiera sina egna ändringar och sitt exakta paket.

## DEC-015 — Claude som arbetsmiljö och underagenter

- Datum: 2026-10-08.
- Status: beslutat av Chefen.
- Beslut: `AGENTS.md` döps om till [CLAUDE.md](../CLAUDE.md). Nya arbetsbranches använder prefixet `claude/`. Huvudagenten får dela upp arbetet på flera underagenter efter eget omdöme.
- Genomförande: de tidigare Codex-trådarna ersätts av underagenter i `.claude/agents/` med filansvar enligt [planen](plan.md#underagenter): domän, infrastruktur/kontroller, katalogdata per klassgrupp, presentation och en oberoende granskare.
- Konsekvens: arbetsregler, scope och datapolicy är oförändrade. Huvudagenten integrerar, verifierar och committar; underagenter committar inte. Äldre `codex/`-branches och chatt-ID:n är historik.
