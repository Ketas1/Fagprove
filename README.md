# Sport For Alle

Nettbasert system for utlån av sportsutstyr til unge.

Systemet dekker den daglige driften av en utlånsordning:

- **Registrering** av utstyr, barn og foresatte, og av utlån og returer
- **Oversikt** over utstyr med status, registrerte brukere, og alle aktive og
  forfalte lån
- **Oppfølging** av forfalte lån, med automatisk oppdagelse av forfall,
  kontaktlogg mot foresatte og blokkering av nye utlån
- **Rapporter** over antall utlån i en periode og fordelt på aldersgruppe, hver
  med levert i tide, levert for sent, ikke levert og fortsatt aktive - med
  eksport til Excel og PDF

## Teknologi

| Lag | Teknologi |
| --- | --- |
| Frontend | Next.js, TypeScript, Tailwind, Bun |
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core |
| Database | PostgreSQL |
| Autentisering | Auth0 |
| Testing | xUnit, Jest og Testing Library |
| Drift | Docker Compose, GitHub Actions |

Begrunnelsen for hvert valg ligger som ADR-er i [`docs/adr/`](./docs/adr/).

## Kom i gang

Databasen kjøres i Docker, backend og frontend lokalt.

```bash
git clone https://github.com/Ketas1/Fagprove.git
cd Fagprove
cp .env.example .env                         # fyll inn Auth0-verdiene

docker compose up -d db                      # database på localhost:5433

dotnet tool install --global dotnet-ef       # engangsjobb
cd backend && dotnet ef database update \
  --project SportForAlle.Api --startup-project SportForAlle.Api

cd SportForAlle.Api && dotnet run             # API på localhost:5080
cd frontend && bun install && bun dev         # frontend på localhost:3000
```

**Første gang?** Følg [`docs/15-installasjon.md`](./docs/15-installasjon.md) i
stedet - den tar hvert steg i rekkefølge, forklarer hva du skal se underveis, og
dekker innlogging og koblingen av ansattprofil. Utviklingsmiljøet i detalj:
[`docs/11-utviklingsmiljo.md`](./docs/11-utviklingsmiljo.md).

## Struktur

```
backend/     .NET-løsning: API-prosjekt lagdelt i mapper, og testprosjekt
frontend/    Next.js-applikasjon
docs/        Dokumentasjon og ADR-er
.claude/     AI-instrukser og skills for Claude Code
.github/     CI-pipeline
CLAUDE.md    Instruksfil for Claude Code
```

## Dokumentasjon

All dokumentasjon ligger i [`docs/`](./docs/), med
[oversikt og dokumentasjonsstrategi i `docs/README.md`](./docs/README.md).

De mest sentrale:

- [Løsningsbeskrivelse](./docs/01-losningsbeskrivelse.md) - problemene og hvordan de løses
- [Arkitektur](./docs/02-arkitektur.md) - lagdeling og dataflyt
- [Domenemodell](./docs/03-domenemodell.md) - entiteter, tilstander og forretningsregler
- [Lover og regler](./docs/09-lover-og-regler.md) - personvern for data om barn
- [Utviklingsmiljø](./docs/11-utviklingsmiljo.md) - oppsett, kjøring og AI-arbeidsflyt
