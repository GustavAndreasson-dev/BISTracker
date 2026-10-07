# Beslutslogg

Beslut ska ange datum, bakgrund och konsekvens. Förslag dokumenteras i plan eller
arkitektur tills de är beslutade. Användarens senare instruktioner gäller framför
äldre dokumentation; uppdatera loggen när ett beslut ersätts.

## DEC-001 — Första produktomfånget

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: första versionen visar och trackar pre-raid BiS för Holy Priest i vanliga WoW Classic (Vanilla).
- Konsekvens: fler expansioner är framtida stöd. Fas, itemkällor och rankingmetod återstår att precisera.

## DEC-002 — Arkitektur och arbetsregler

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: använd DDD-struktur och SOLID-principer, samt Markdown-filer som håller reda på regler och kan jämföras med koden.
- Konsekvens: implementationen ska följas upp mot dokumenterade krav och arkitektur. Konkreta domängränser och projektindelningen är ännu förslag.

## DEC-003 — Samordning

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: huvudtråden har huvudansvaret och tar emot instruktioner om hur projektet ska byggas. Andra trådar kan bidra under samordning.
- Konsekvens: huvudtråden håller den gemensamma planen och granskar hur bidrag passar ihop med krav och arkitektur.

## DEC-004 — Tracking och första utkast

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: tracking ska hantera både erhållna och utrustade items. Huvudtråden ska lägga upp planen, skapa andra Codex-trådar med instruktioner och skapa ett första utkast.
- Konsekvens: arbetet delas efter filansvar enligt draft-contract.md. Utkastet byggs i befintlig WinUI-app med tydligt markerad exempeldata och lokal lagring som genomförandeval.

## DEC-005 — Mappstruktur

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: varje projekt ska ha en god mappstruktur som dokumenteras i Markdown.
- Konsekvens: project-structure.md anger områden och ansvar inom lagren. Nya filer ska placeras enligt kartan och den faktiska strukturen granskas tillsammans med koden.

## DEC-006 — Regelbundna commits

- Datum: 2026-10-07.
- Status: beslutat av användaren.
- Beslut: gör commits regelbundet efter avklarade delar, i lagom stora grupper som hänger ihop.
- Konsekvens: huvudtråden granskar, verifierar och commitar färdiga delar löpande. Det första utkastets befintliga ändringar delas upp efter domän/användningsfall, lagring/kontroller, UI, källresearch och gemensam projektdokumentation.
