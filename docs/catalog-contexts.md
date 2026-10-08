# Katalognivåer och säkert byte

Beställt av Chefen 2026-10-08: Forever nivå 30 nu och nivå 60-import när data finns.
Detta dokument beskriver den implementerade modellgränsen; katalogimport och
produktdata verifieras separat innan de markeras klara.

`CatalogSet` skiljer Classic fas 1/nivå 60 från Forever beta/nivå 30 och kommande
Forever nivå 60. `ICharacterCatalog` kan erbjuda nya källgranskade set. Val av
ett set från fel spelversion avvisas. Spelversion, nivå och beta/launch anges
uttryckligt; beta-listor presenteras inte som slutliga pre-raid-rankningar.

Karaktären behåller gemensamt itemägande inom sin spelversion, men tre egna
utrustningslistor per katalogset. Vid nivåbyte arkiveras tidigare listor och
återställs när spelaren byter tillbaka. Det aktiva setet sparas. Borttaget
ägande rensar itemvariantens utrustning i alla specs och arkiverade nivåer.

JSON-schema 1 utökas med frivilliga `catalogSetId` och `archivedLoadouts`.
Äldre filer utan dem använder versionens ursprungliga set och bevarar
karaktärs-ID, ägande och utrustning. Okända eller borttagna sparade set
rapporteras som fel i stället för att framsteg raderas. Inget databasschema ändras.

Domänen hanterar källbelagda tvåhands/offhand-konflikter och unika items.
Utrusta tvåhandsvapen avmarkerar vald specs offhand; utrusta offhand avmarkerar
dess tvåhandsvapen. Ägandet bevaras. En unik itemvariant flyttas mellan möjliga
ring-/trinketplaceringar och kan inte återställas som dubbelutrustad.
Katalogen måste bara erbjuda vapenplaceringar som verifierats för klass/spec;
inga generella nya dual-wield-talanger antas.

Release-build godkänd utan varningar/fel; samtliga 29 konsolscenarier passerar.
`CatalogContextScenarios` verifierar tre olika nivå30-listor, separat nivå60-lista,
återläsning, delat ägande, arkiverade markeringar, äldre JSON-format, fel version,
tvåhandskonflikter och unique-regler. Huvudtråden samordnar fortsatt integration.
