# Utviklingsmiljø og prosjektoppsett

Dette dokumentet beskriver hvordan prosjektet, teknologistacken og filstrukturen
er satt opp, og hvordan systemet kjøres lokalt.

## Teknologistack

| Lag | Teknologi | Begrunnelse |
| --- | --- | --- |
| Frontend | Next.js (App Router), TypeScript | Strukturert rammeverk for moderne webapplikasjoner. [ADR-0003](./adr/0003-frontend-nextjs.md) |
| Styling | Tailwind CSS | Rask UI-utvikling, som er viktig med en kort utviklingsperiode. |
| Pakkehåndtering frontend | Bun | Rask installasjon og kjøring. |
| Backend | .NET 10, ASP.NET Core Web API | Mest erfaring, godt egnet til API-er og databehandling, stort økosystem. [ADR-0002](./adr/0002-dotnet-backend.md) |
| ORM | Entity Framework Core (Code First) | Datamodellen defineres som kode og versjoneres med applikasjonen. [ADR-0005](./adr/0005-ef-core-code-first.md) |
| Database | PostgreSQL | Relasjonsdatabase. [ADR-0004](./adr/0004-postgresql.md) |
| Autentisering | Auth0 | Ferdig identitetsløsning, unngår egen passordhåndtering. [ADR-0006](./adr/0006-auth0.md) |
| Testing backend | xUnit | Godt integrert med .NET. [ADR-0009](./adr/0009-xunit.md) |
| Kjøremiljø | Docker Compose | Likt miljø hver gang, alle tre lag samtidig. [ADR-0007](./adr/0007-docker.md) |
| CI | GitHub Actions | Bygg og test på hver commit. [ADR-0008](./adr/0008-github-actions.md) |
| Versjonskontroll | Git og GitHub | Historikk og oversikt over endringer. |
| Utviklingsverktøy | VS Code, Claude Code, GitHub Copilot | Se avsnittet om AI-arbeidsflyt. |

## Filstruktur

```
Fagprove/
├── CLAUDE.md                  Instruksfil for Claude Code
├── README.md                  Kort oversikt og oppstart
├── docker-compose.yml         Frontend + backend + database
├── .env.example               Alle miljøvariabler, dokumentert
├── .editorconfig              Felles formateringsregler
├── .gitattributes             Linjeskift og binærfiler
│
├── backend/                   .NET-løsning
│   ├── src/
│   │   ├── SportForAlle.Api/             Endepunkter, DTO-er, autorisasjon
│   │   ├── SportForAlle.Application/     Bruksmønstre
│   │   ├── SportForAlle.Domain/          Entiteter og forretningsregler
│   │   └── SportForAlle.Infrastructure/  EF Core, e-post, fillagring
│   └── tests/
│       ├── SportForAlle.Domain.Tests/          Enhetstester
│       └── SportForAlle.Api.IntegrationTests/  Integrasjonstester
│
├── frontend/                  Next.js-applikasjon
│   └── src/
│       ├── app/               Sider (App Router)
│       ├── components/        Gjenbrukbare komponenter
│       ├── lib/               API-klient og hjelpefunksjoner
│       └── types/             Delte typer for API-svar
│
├── docs/                      Dokumentasjon og ADR-er
├── .claude/                   Skills og delte innstillinger
└── .github/workflows/         CI-pipeline
```

Begrunnelsen for lagdelingen i backend står i
[`02-arkitektur.md`](./02-arkitektur.md).

## Forutsetninger

| Verktøy | Versjon | Merknad |
| --- | --- | --- |
| Docker Desktop | nyeste | Kreves for å kjøre databasen og hele stacken |
| .NET SDK | 10.x | Kun nødvendig for å kjøre backend utenfor Docker |
| Bun | 1.x | Kun nødvendig for å kjøre frontend utenfor Docker |
| Git | nyeste | |

## Kom i gang

```bash
git clone https://github.com/Ketas1/Fagprove.git
cd Fagprove
cp .env.example .env
```

Fyll inn verdiene i `.env`. Auth0-verdiene hentes fra Auth0-dashbordet, se
[`06-autentisering.md`](./06-autentisering.md). Generer `AUTH0_SECRET` med:

```bash
openssl rand -hex 32
```

Start hele systemet:

```bash
docker compose up --build
```

| Tjeneste | Adresse |
| --- | --- |
| Frontend | http://localhost:3000 |
| API | http://localhost:5080 |
| API-dokumentasjon | http://localhost:5080/scalar |
| Database | localhost:5432 |

## Kjøre lagene hver for seg

Under utvikling er det ofte raskere å kjøre bare databasen i Docker og resten fra
IDE-en:

```bash
# Kun database
docker compose up -d db

# Backend
cd backend
dotnet run --project src/SportForAlle.Api

# Frontend
cd frontend
bun install
bun dev
```

Husk at `ConnectionStrings__DefaultConnection` da må peke på `localhost` i stedet
for `db`.

## Database og migrasjoner

Databaseskjemaet defineres som C#-klasser og genereres med EF Core-migrasjoner.
Databasen endres aldri direkte.

```bash
cd backend

# Ny migrasjon etter endring i modellen
dotnet ef migrations add <Navn> \
  --project src/SportForAlle.Infrastructure \
  --startup-project src/SportForAlle.Api

# Kjør migrasjoner mot databasen
dotnet ef database update \
  --project src/SportForAlle.Infrastructure \
  --startup-project src/SportForAlle.Api
```

Detaljer i [`04-databasedesign.md`](./04-databasedesign.md). Arbeidsflyten er
også tilgjengelig som en skill i Claude Code: `/ef-migration`.

## Testing

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && bun run lint && bun run build
```

Teststrategien er beskrevet i [`07-testing.md`](./07-testing.md).

## CI-pipeline

`.github/workflows/ci.yml` kjører på hver push og pull request mot `main`:

1. **Backend** - `dotnet restore`, `dotnet build` med advarsler som feil, og
   `dotnet test`.
2. **Frontend** - `bun install`, `bun run lint` og `bun run build`.
3. **Formatering** - `dotnet format --verify-no-changes` mot `.editorconfig`.

Jobbene kjører parallelt, og hopper over seg selv så lenge den aktuelle mappen er
tom. Slik er pipelinen på plass fra start uten å feile før prosjektene finnes.

## AI-arbeidsflyt

AI er en aktiv del av arbeidsflyten gjennom hele prosjektet, ikke bare et
oppslagsverktøy. Oppsettet består av tre deler:

| Fil / mappe | Rolle |
| --- | --- |
| `CLAUDE.md` | Prosjektets instruksfil. Lastes automatisk i hver økt, og inneholder domenekunnskap, tilstandsmaskiner, forretningsregler, konvensjoner og personvernkrav. |
| `.claude/skills/` | Navngitte arbeidsflyter for oppgaver som gjentas: opprette ADR, oppdatere dokumentasjon, legge til endepunkt, kjøre migrasjon, lage frontend-side. |
| `.claude/settings.json` | Delte innstillinger, blant annet hvilke kommandoer som kan kjøres uten å spørre. |

Arbeidsmåten per del av systemet er: planlegg sammen med Claude, la Claude lage
første utkast, gå gjennom resultatet, og skriv tester underveis. GitHub Copilot
brukes til kodefullføring i editoren.

Grunnen til at instruksene ligger i repoet og ikke i en chat, er at de da
versjoneres sammen med koden og gjelder likt i hver økt. Det er også dokumentasjon
i seg selv: `CLAUDE.md` beskriver systemet presist nok til at en ny utvikler kan
lese den som en teknisk innføring.

## Kodekonvensjoner

- `.editorconfig` gjelder for både C# og TypeScript, og håndheves i CI.
- Kode, API-ruter og kommentarer på engelsk. Grensesnittekst og dokumentasjon på
  norsk. Ordliste i [`03-domenemodell.md`](./03-domenemodell.md), begrunnelse i
  [ADR-0010](./adr/0010-domenespraak-engelsk-i-kode.md).
- Commit-meldinger følger conventional commits: `feat:`, `fix:`, `docs:`,
  `test:`, `chore:`.
