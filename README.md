# BISTracker

En app för att visa och tracka best in slot (BiS). Första versionen gäller
Holy Priest i vanliga WoW Classic (Vanilla), med endast pre-raid BiS.
Fler expansioner är en framtida riktning.

## Dokumentation

- [Arbetsregler](AGENTS.md)
- [Krav och koppling till kod](docs/requirements.md)
- [Arkitektur och domänspråk](docs/architecture.md)
- [Mappstruktur i projekten](docs/project-structure.md)
- [Plan och öppna frågor](docs/plan.md)
- [Beslut](docs/decisions.md)
- [Arbetsfördelning och kontrakt för utkastet](docs/draft-contract.md)
- [BiS-källresearch](docs/data-research.md)
- [Verifiering och körning](docs/verification.md)

Dokumenten skiljer mellan beslut, förslag och verifierad implementation.

## Nuläge

Första utkastet har en WinUI-vy med sökning, filter och tracking av erhållna
och utrustade items. Domain, Application och Infrastructure är separata projekt
med dokumenterade mappar. Framsteg sparas lokalt i JSON för en demoprofil.

Katalogen använder 17 fiktiva platshållare, inte verifierade WoW-items.
Fas, tillåtna itemkällor och rekommendationsgrund behöver bestämmas inför riktig data.
Se verifieringsdokumentet för kontroller, startkommando och begränsningar.

## Förhandsbild av apputkastet

![WinUI-utkast med isolerat demotillstånd](docs/previews/draft-overview.png)

Bildens markeringar kommer från isolerad verifieringsdata och påverkar inte dina sparade framsteg.
