# Plan och öppna frågor

Statusdatum: 2026-10-07. Plan för det beställda utkastet och fortsatt utveckling.

## Nuläge

- Bekräftat scope: Vanilla Classic, Holy Priest och pre-raid BiS.
- DDD, SOLID och Markdown-dokumentation är beslutade.
- Tracking omfattar både erhållna och utrustade items.
- Utkastet byggs i befintlig WinUI/C#/.NET 8-app med Domain, Application och Infrastructure.
- UI har utrustningslista, sökning, filter och sammanfattningar. Lokal JSON-lagring används för en demoprofil.
- Katalogen innehåller tydligt fiktiva exempel; riktig BiS-lista är inte fastställd.
- Mappstruktur och kodspårning dokumenteras. Slutkontroller redovisas i verification.md.

## Genomförandeplan

| Steg | Leverans | Klart när | Status |
| --- | --- | --- | --- |
| 1. Gemensam grund | Krav, DDD-kontrakt, mappstruktur och tråduppdrag. | Ansvarsområden och gränssnitt är dokumenterade. | Klar. |
| 2. Första utkast | WinUI-vy, domänregler och lokal tracking med exempeldata. | Bygg och relevanta kontroller passerar; faktisk UI-rendering granskas. | Klar: Release-build, 14 beteendekontroller och isolerad UI-kontroll godkända. |
| 3. BiS-underlag | Källjämförelse och förslag på datametod. | Rapport med spårbara källor och kvarstående val finns. | Klar som research i data-research.md. |
| 4. Verifierad katalog | Riktiga items och anskaffning för vald kontext. | Fas, tillåtna källor och rekommendationsgrund är beslutade; katalogen är verifierad. | Återstår; behöver produktbeslut. |
| 5. Förfinad produkt | Anpassat UI, beslutad karaktärshantering och komplett kravverifiering. | Överenskomna acceptanskriterier är uppfyllda med riktig data. | Planerad efter återkoppling på utkastet. |

## Öppna beslut

| Fråga | Varför den behövs |
| --- | --- |
| Vilken fas och itemtillgänglighet gäller inom Vanilla Classic? | Bestämmer vilka rekommendationer som är relevanta. |
| Vilka källor till items ingår i pre-raid, exempelvis crafting, quests och PvP? | Gör listans avgränsning tydlig. |
| Vilken källa och metod ska styra BiS-listan? Ska alternativ visas? | Gör rekommendationerna spårbara och jämförbara. |
| Ska första versionen hantera en eller flera karaktärer? | Bestämmer vilken kontext framsteg hör till. |
| Behövs annan plattform eller synkronisering senare? | Utkastet fortsätter med WinUI och lokal lagring; framtida behov är öppna. |

## Skapade Codex-trådar

Huvudtråden håller ihop krav, arkitekturbeslut och integration. Varje delegerad
uppgift ska ha tydligt scope, filansvar, beroenden och acceptanskriterier.
Resultat granskas mot AGENTS.md och relevanta krav innan uppgiften markeras klar.

| Tråd | ID | Filansvar | Uppdrag |
| --- | --- | --- | --- |
| BISTracker – Domän och användningsfall | 01a116a0-396a-71e3-a185-222598448506 | Domain/ och Application/ | Aggregat, invariants och användningsfall enligt draft-contract.md. |
| BISTracker – Lagring och verifiering | 01a116a0-52c0-72f2-a7b9-f2a82712e13b | Infrastructure/ och Checks/ | Exempelkatalog, säker JSON-lagring och beteendekontroller. |
| BISTracker – BiS-källor och speldata | 01a116a0-7a8a-7a71-9161-34a0e1e3cca1 | docs/data-research.md | Källresearch och förslag till verifierad katalog. |

Huvudtråden äger Presentation/, solution, gemensamma docs och integration.
Trådarna har fått uppdrag och den senare instruktionen om mappstruktur.

## Löpande leverans

En avklarad och verifierad del ska följas av en sammanhängande commit med ett
tydligt syfte. Huvudtråden samordnar detta i den delade projektmappen enligt
AGENTS.md. Koden och den dokumentation som berör ändringen uppdateras tillsammans.
