# Beslutslogg

Beslut ska ange datum, bakgrund och konsekvens. Förslag dokumenteras i plan eller
arkitektur tills de är beslutade. Chefens senare instruktioner gäller framför
äldre dokumentation; uppdatera loggen när ett beslut ersätts.

## DEC-001 — Första produktomfånget

- Datum: 2026-10-07.
- Status: beslutat av Chefen.
- Beslut: första versionen visar och trackar pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla).
- Konsekvens: fler expansioner är framtida stöd. Fas och itemkällor preciserades senare i DEC-007. Guidebaserat kandidatunderlag finns i phase1-items.md.

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
- Konsekvens: crafting och köpta/world-drop BoE-items ska exkluderas. Guideval från dessa källor behöver ersättas med verifierade alternativ. Moderna Era-itemposter bevisar inte själva historisk fastillgänglighet.

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
- Tillåtna itemkällor: tidigare avgränsning dungeons/quests gäller tills Chefen besvarat förtydligandet för beta-listorna. Datainsamlingen bevarar även andra guidekandidater för ett eventuellt ändrat beslut.
- Genomförande: källbelagda guidealternativ för beta nivå 30/patch 1.60.1, separat nivå 60-kontext, validerad atomisk packimport och bevarade utrustningsuppsättningar. Luckor fylls inte med egna rankingar. Release-build, 39 beteendekontroller och isolerad WinUI-kontroll godkända; [leverans](forever-level30-catalogs.md).
