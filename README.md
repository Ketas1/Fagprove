# Sport For Alle

Nettbasert system for utlån av sportsutstyr til unge.

Systemet dekker den daglige driften av en utlånsordning:

- **Registrering** av utstyr, barn og foresatte, og av utlån og returer
- **Oversikt** over utstyr med status, registrerte brukere, og alle aktive og
  forfalte lån
- **Oppfølging** av forfalte lån, med automatisk oppdagelse av forfall,
  kontaktlogg mot foresatte og blokkering av nye utlån
- **Rapporter** over antall utlån, fordeling på aldersgruppe, populært utstyr og
  forsene leveringer

## Teknologi

| Lag | Teknologi |
| --- | --- |
| Frontend | Next.js, TypeScript, Tailwind, Bun |
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core |
| Database | PostgreSQL |
| Autentisering | Auth0 |
| Testing | xUnit, Playwright |
| Drift | Docker Compose, GitHub Actions |

Begrunnelsen for hvert valg ligger som ADR-er i [`docs/adr/`](./docs/adr/).

## Kom i gang

```bash
git clone https://github.com/Ketas1/Fagprove.git
cd Fagprove
cp .env.example .env      # fyll inn verdiene
docker compose up -d db   # starter databasen
```

Når backend og frontend er scaffoldet, starter `docker compose up --build` hele
systemet. Full veiledning i
[`docs/11-utviklingsmiljo.md`](./docs/11-utviklingsmiljo.md).

## Struktur

```
backend/     .NET-løsning: API, domene, datatilgang, tester
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
