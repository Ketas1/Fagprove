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
| Backend-struktur | Lagdelt monolitt, ett prosjekt | Færre filer og mindre wiring enn Clean Architecture, uten å miste testbare forretningsregler. [ADR-0012](./adr/0012-lagdelt-monolitt.md) |
| ORM | Entity Framework Core (Code First) | Datamodellen defineres som kode og versjoneres med applikasjonen. [ADR-0005](./adr/0005-ef-core-code-first.md) |
| Database | PostgreSQL | Relasjonsdatabase. [ADR-0004](./adr/0004-postgresql.md) |
| Autentisering | Auth0 | Ferdig identitetsløsning, unngår egen passordhåndtering. [ADR-0006](./adr/0006-auth0.md) |
| Testing backend | xUnit | Godt integrert med .NET. [ADR-0009](./adr/0009-xunit.md) |
| Testing frontend | Jest og Testing Library | Offisielt støttet av Next.js gjennom `next/jest`, og kjent for de fleste utviklere. |
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
│   ├── SportForAlle.sln
│   ├── SportForAlle.Api/      Selve applikasjonen, lagdelt i mapper
│   │   ├── Program.cs
│   │   ├── Controllers/       HTTP-endepunkter
│   │   ├── Services/          Bruksmønstre, eneste bruker av AppDbContext
│   │   ├── Models/            Entiteter med tilstand og forretningsregler
│   │   ├── Dtos/              Objekter som krysser API-grensen
│   │   ├── Data/              AppDbContext, konfigurasjon, migrasjoner
│   │   ├── Mapping/           Entitet til DTO
│   │   ├── Validation/        Validering av forespørsler
│   │   ├── Middleware/        Tverrgående håndtering
│   │   ├── Configuration/     Sterkt typede innstillinger
│   │   └── Helpers/           Små hjelpefunksjoner
│   └── SportForAlle.Tests/    xUnit, ett prosjekt for alle testnivåer
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

## Kjøre applikasjonen

Databasen kjøres i Docker, mens backend og frontend kjøres lokalt. Det gir
raskest mulig iterasjon under utvikling, og databasen oppfører seg likt hver
gang.

Start hvert steg i sitt eget terminalvindu:

```bash
# 1. Database
docker compose up -d db

# 2. Skjemaet - engangsjobb, og etter hver nye migrasjon
dotnet tool install --global dotnet-ef
cd backend
dotnet ef database update --project SportForAlle.Api --startup-project SportForAlle.Api

# 3. Backend
cd SportForAlle.Api
dotnet run

# 4. Frontend
cd frontend
bun install
bun dev
```

> Steg 2 er lett å overse. Databasecontaineren starter tom, så uten det starter
> API-et fint, men hver side feiler. Se
> [`15-installasjon.md`](./15-installasjon.md) for hele førstegangsoppsettet,
> inkludert innlogging og kobling av ansattprofil.

| Tjeneste | Adresse |
| --- | --- |
| Frontend (innlogging) | http://localhost:3000 |
| Frontend (dashbord, krever innlogging) | http://localhost:3000/dashboard |
| API | http://localhost:5080 |
| API-dokumentasjon (rå OpenAPI-spesifikasjon) | http://localhost:5080/openapi/v1.json |
| API-testing (Scalar, kun i Development) | http://localhost:5080/scalar |
| Database | localhost:5433 |

Alle endepunkter i selve API-et (`/api/*`) krever autentisering som standard.
**Unntaket** er `/scalar` og `/openapi/v1.json`, som er anonymt tilgjengelige,
men **kun i Development** - se "API-testing med Scalar" under for hvorfor og
[`06-autentisering.md`](./06-autentisering.md) for resten av
autentiseringsoppsettet.

### API-testing med Scalar

Scalar (`Scalar.AspNetCore`) gir et brukergrensesnitt for å teste API-et
direkte, uten frontend - nyttig for å verifisere at et endepunkt fungerer mot
en ekte database, og at autentisering faktisk håndheves. Bygget på den
innebygde OpenAPI-genereringen (`Microsoft.AspNetCore.OpenApi`, ikke
Swashbuckle), ikke et eget verktøy ved siden av.

- Åpne **http://localhost:5080/scalar** mens backend kjører i `Development`.
- Trykk **Authentication** øverst til høyre, velg **Bearer**, og lim inn et
  ekte Auth0 access token for API-et - hentet fra Auth0-dashbordet, under
  applikasjonen sin API - fanen **Test**, som gir en `curl`-kommando med et
  gyldig token. Scalar husker tokenet i nettleseren til det byttes ut eller
  utløper.
- Ethvert kall mot `/api/*` uten et gyldig token gir fortsatt `401` - det er
  nettopp poenget: Scalar tester den ekte autentiseringen, ikke en forbikjøring
  av den.

`/scalar` og `/openapi/v1.json` er markert `[AllowAnonymous]` **utelukkende**
inne i `if (app.Environment.IsDevelopment())`-blokken i `Program.cs` - uten
det unntaket kunne siden aldri åpnes uten et token allerede satt, siden den
globale fallback-policyen ellers ville krevd autentisering for absolutt alle
endepunkter, inkludert selve testsiden. Selve `/api/*`-endepunktene er
urørt og fullt beskyttet som før.

Dashbordet viser status for API og database, slik at det er lett å se om alle
tre lagene henger sammen - hentet gjennom frontendens egen proxy, se
[ADR-0015](./adr/0015-proxied-backend-for-frontend.md).

### Hvorfor databasen ligger på port 5433

Containeren publiseres på **5433**, ikke 5432. En lokalt installert PostgreSQL
opptar som regel 5432, og da ville `localhost:5432` stille gått til den lokale
serveren i stedet for containeren. Feilen viser seg som
`password authentication failed`, selv om oppsettet i Docker er riktig.

Inne i compose-nettverket lytter databasen fortsatt på 5432. Porten kan endres
med `POSTGRES_PORT` i `.env`.

### Én delt `.env` for hele stacken

`.env` ligger i repo-roten, ikke inne i `backend/` eller `frontend/`, slik at
backend og frontend deler ett sett med variabler i stedet for å holde to
kopier synkronisert. Ingen av de to rammeverkene leser en fil i en
foreldremappe av seg selv, så begge laster den eksplisitt:

- **Backend** bruker pakken `DotNetEnv`. `Program.cs` leter etter en `.env`
  ved å gå oppover fra arbeidskatalogen til den finner én, og laster den før
  `WebApplication.CreateBuilder` kjører - slik at verdiene når konfigurasjonen
  gjennom den vanlige miljøvariabel-provideren ASP.NET Core allerede har.
  Finnes ingen `.env` (slik som i CI), hoppes lastingen stille over.
  **`appsettings.Development.json` har bevisst ingen `ConnectionStrings`-
  seksjon lenger** - tilkoblingsstrengen kommer utelukkende fra `.env`.
- **Frontend** bruker `@next/env`, pakken Next.js selv bruker internt til
  dette formålet. Den kalles to steder: i `next.config.ts` (for vanlig
  server-side og build-time-kode), og igjen i `src/lib/auth0.ts`. Grunnen til
  at den må lastes to steder: Proxy (`src/proxy.ts`, tidligere kalt
  Middleware) kjører i en egen eksekveringskontekst som ikke arver
  `process.env`-endringer gjort fra `next.config.ts`, selv om begge kjører på
  Node.js-runtimen. `Auth0Client` konstrueres i `lib/auth0.ts`, som importeres
  av både sider og av proxyen, så den må laste `.env` selv for å fungere i
  begge kontekster.

Miljøvariabler har forrang over `appsettings.Development.json` i ASP.NET
Cores standard konfigurasjonsrekkefølge, så en verdi i `.env` overstyrer alt
annet for backend.

**Én verdi må endres etter `Host=db` i `.env.example`:**
`ConnectionStrings__DefaultConnection` der peker på `Host=db;Port=5432` - det
Docker-interne nettverksnavnet, som bare finnes når selve API-et også kjører
som en compose-tjeneste (den kommenterte `api`-tjenesten i
`docker-compose.yml`, ikke aktiv ennå). For den dokumenterte arbeidsflyten -
database i Docker, **backend kjørt lokalt** - må den lokale `.env` i stedet
bruke `Host=localhost;Port=5433`, av samme grunn som beskrevet over.

## Database og migrasjoner

Databaseskjemaet defineres som C#-klasser og genereres med EF Core-migrasjoner.
Databasen endres aldri direkte.

```bash
cd backend

# Ny migrasjon etter endring i modellen
dotnet ef migrations add <Navn> \
  --project SportForAlle.Api \
  --startup-project SportForAlle.Api \
  --output-dir Data/Migrations

# Kjør migrasjoner mot databasen
dotnet ef database update \
  --project SportForAlle.Api \
  --startup-project SportForAlle.Api
```

Detaljer i [`04-databasedesign.md`](./04-databasedesign.md). Arbeidsflyten er
også tilgjengelig som en skill i Claude Code: `/ef-migration`.

## Testing

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && bun run test
cd frontend && bun run lint && bun run build
```

Teststrategien er beskrevet i [`07-testing.md`](./07-testing.md).

## CI-pipeline

`.github/workflows/ci.yml` kjører på hver push og pull request mot `main`:

1. **Backend** - `dotnet restore`, `dotnet build` med advarsler som feil,
   deretter `dotnet ef database update` mot en midlertidig PostgreSQL-
   tjenestecontainer (`postgres:17-alpine`, samme image som
   `docker-compose.yml`) før `dotnet test` kjøres mot den, og til slutt
   `dotnet format --verify-no-changes` mot `.editorconfig`. Containeren er tom
   og finnes kun for denne jobben - uten volum, så ingenting overlever mellom
   CI-kjøringer, i motsetning til den lokale databasen. Siden ingenting i
   appen selv kaller `Database.Migrate()` ved oppstart, installeres
   `dotnet-ef` som globalt verktøy i jobben (det er ikke satt opp som lokalt
   verktøy i repoet) og kjører migrasjonene eksplisitt. Se
   [ADR-0008](./adr/0008-github-actions.md) for hvorfor en tjenestecontainer
   brukes fremfor mocking.
2. **Frontend** - `bun install`, `bun run lint`, `bun run test` og
   `bun run build`.

Jobbene kjører parallelt. De hopper over seg selv så lenge den aktuelle mappen
mangler et prosjekt, slik at pipelinen kunne settes opp før koden fantes. Nå som
begge prosjektene er på plass, kjører begge jobbene.

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
