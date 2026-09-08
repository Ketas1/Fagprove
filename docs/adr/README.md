# Architecture Decision Records

En ADR er et kort dokument som beskriver ett teknisk valg: hva som ble bestemt,
hvilke alternativer som ble vurdert, og hvorfor det ene ble valgt.

## Hvorfor ADR-er

Tekniske valg skal være dokumentert. Problemet med å skrive den begrunnelsen til
slutt er at man da rekonstruerer den, og i praksis skriver ned en pyntet versjon
av hva man endte opp med. En ADR skrives i det øyeblikket valget tas, mens
alternativene fortsatt er reelle og begrunnelsen er ærlig.

De gjør også at en ny utvikler kan lese seg opp på *hvorfor* systemet ser ut som
det gjør, uten å måtte spørre.

## Regler

- Én beslutning per dokument.
- Nummereres fortløpende: `0001-`, `0002-`, og så videre.
- En ADR endres ikke etter at den er akseptert. Ombestemmer man seg, skrives en
  ny ADR som **erstatter** den gamle, og den gamle får status `Erstattet av
  ADR-XXXX`. Da beholdes historikken over hva man trodde og hvorfor man endret
  mening, som ofte er mer interessant enn beslutningen selv.
- Kort. En ADR som er lang nok til å måtte leses to ganger er for lang.

## Opprette en ny ADR

I Claude Code: `/adr <kort beskrivelse av valget>`. Skillen finner neste
nummer, bruker malen og oppdaterer registeret under.

Manuelt: kopier [`0000-mal.md`](./0000-mal.md).

## Register

| Nr | Tittel | Status |
| --- | --- | --- |
| [0001](./0001-monorepo.md) | Monorepo for frontend og backend | Akseptert |
| [0002](./0002-dotnet-backend.md) | .NET 10 som backend | Akseptert |
| [0003](./0003-frontend-nextjs.md) | Next.js, Bun og Tailwind i frontend | Akseptert |
| [0004](./0004-postgresql.md) | PostgreSQL som database | Akseptert |
| [0005](./0005-ef-core-code-first.md) | Entity Framework Core med Code First | Akseptert |
| [0006](./0006-auth0.md) | Auth0 til authentication | Akseptert |
| [0007](./0007-docker.md) | Docker Compose som kjøremiljø | Akseptert |
| [0008](./0008-github-actions.md) | GitHub Actions til CI | Akseptert |
| [0009](./0009-xunit.md) | xUnit til testing av backend | Akseptert |
| [0010](./0010-domenespraak-engelsk-i-kode.md) | Engelsk i kode, norsk i grensesnitt og dokumentasjon | Akseptert |
| [0011](./0011-automatisk-forfall.md) | Forfall både beregnet og lagret | Akseptert |
| [0012](./0012-lagdelt-monolitt.md) | Lagdelt monolitt uten repositories | Akseptert |
| [0013](./0013-guid-primaernokler.md) | Guid som primærnøkkel | Akseptert |
| [0014](./0014-revisjonsfelter-pa-alle-entiteter.md) | Revisjonsfelter på alle entiteter | Akseptert |
| [0015](./0015-proxied-backend-for-frontend.md) | Proxied backend-for-frontend i stedet for direkte kall fra nettleseren | Akseptert |
| [0016](./0016-shadcn-ui.md) | shadcn/ui som eneste komponentbibliotek i frontend | Akseptert |
| [0017](./0017-global-exception-handler.md) | Global exception-handler for RFC 7807-feilsvar | Akseptert |
| [0018](./0018-scalar-api-testing.md) | Scalar som API-testverktøy i utviklingsmiljø | Akseptert |
| [0019](./0019-staff-auth0-mapping.md) | Staff↔Auth0-kobling og «reject unless linked» | Akseptert |
| [0020](./0020-server-lesing-klient-skriving.md) | Server-komponenter leser direkte fra backend, klientkomponenter skriver gjennom proxyen | Akseptert |
| [0021](./0021-hierarkiske-utstyrskategorier.md) | Hierarkiske utstyrskategorier, uhåndhevet dybde, ett-siders trevisning | Akseptert |
| [0022](./0022-emailjs-server-side.md) | Oppfølgings-e-post sendes fra backend, ikke nettleseren | Akseptert |
| [0023](./0023-rapporteksport.md) | Rapporteksport lages i nettleseren, og inneholder bare aggregerte tall | Akseptert |
| [0024](./0024-redigerbar-fodselsdato.md) | Fødselsdato på låntaker kan rettes etter registrering | Akseptert |
| [0025](./0025-retting-av-apne-utlan.md) | Bare åpne utlån kan rettes, og feil utstyr frigis uten å telle som retur | Akseptert |
| [0026](./0026-sletting-arkivering-anonymisering.md) | Tre operasjoner for å fjerne data - sletting, arkivering og anonymisering | Akseptert |
