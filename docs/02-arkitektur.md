# Arkitektur

## Overordnet

Systemet er en trelagsapplikasjon: en frontend, et backend-API og en database.
Alt ligger i ett repositorium (monorepo), og alle tre lagene kjøres samtidig
lokalt med Docker Compose.

```mermaid
flowchart LR
    B["Nettleser<br/>(ansatt)"]
    F["Frontend<br/>Next.js + Tailwind<br/>:3000"]
    A["Backend-API<br/>.NET 10<br/>:5080"]
    D[("PostgreSQL<br/>:5432")]
    Z["Auth0<br/>(ekstern)"]

    B --> F
    F -- "HTTPS + JWT" --> A
    A -- "EF Core" --> D
    F -. "innlogging (OIDC)" .-> Z
    A -. "validerer token" .-> Z
```

Frontend snakker aldri med databasen direkte. All datatilgang går gjennom API-et,
slik at forretningsreglene håndheves ett sted.

## Hvorfor monorepo

Frontend og backend ligger i samme repositorium fordi de utvikles av én person i
ett tempo og endres ofte sammen. Et endepunkt og siden som bruker det kan da
endres i samme commit, og CI kan bygge og teste hele systemet mot samme versjon.
Se [ADR-0001](./adr/0001-monorepo.md).

```
Fagprove/
├── backend/          .NET-løsning
├── frontend/         Next.js-applikasjon
├── docs/             Dokumentasjon (denne mappen)
├── .claude/          AI-instrukser og skills
├── .github/          CI-pipeline
├── docker-compose.yml
└── CLAUDE.md         Instruksfil for Claude Code
```

## Backend: lagdeling

Backend er delt i fire prosjekter. Avhengighetene peker innover: ytre lag kjenner
indre lag, aldri omvendt.

```mermaid
flowchart TD
    API["SportForAlle.Api<br/>Endepunkter, DTO-er, autorisasjon"]
    APP["SportForAlle.Application<br/>Bruksmønstre, orkestrering"]
    DOM["SportForAlle.Domain<br/>Entiteter, tilstander, forretningsregler"]
    INF["SportForAlle.Infrastructure<br/>EF Core, e-post, fillagring"]

    API --> APP
    APP --> DOM
    INF --> DOM
    API -. "registrerer implementasjoner ved oppstart" .-> INF
```

| Prosjekt | Ansvar | Kjenner til |
| --- | --- | --- |
| `Domain` | Entiteter, enums, tilstandsoverganger og forretningsregler. Ren C# uten rammeverk. | Ingenting |
| `Application` | Bruksmønstre som "registrer utlån" eller "registrer retur". Definerer grensesnittene infrastrukturen må oppfylle. | `Domain` |
| `Infrastructure` | EF Core `DbContext`, migrasjoner, e-postsending, lagring av bilder. Implementerer grensesnittene fra `Application`. | `Domain`, `Application` |
| `Api` | HTTP-endepunkter, DTO-er, modellvalidering, autorisasjon, feilhåndtering. | `Application` |

### Hvorfor denne lagdelingen

Den viktigste regelen i systemet er at et nytt utlån blokkeres når låneren har et
åpent forfalt lån eller er utestengt. Den regelen må gjelde uansett hvor utlånet
registreres fra. Ved å legge regelen i `Domain` kan den ikke omgås ved å kalle
API-et direkte, og den kan enhetstestes uten database, HTTP eller Auth0.

Lagdelingen gjør også at databasevalget er byttbart: `Domain` vet ikke at
PostgreSQL finnes.

## Frontend: struktur

Next.js med App Router. Serverkomponenter er standard; klientkomponenter brukes
bare der det trengs interaktivitet.

```
frontend/src/
├── app/
│   ├── (auth)/           Innlogging og utlogging
│   └── (dashboard)/      Beskyttet område - utstyr, brukere, utlån, rapporter
├── components/           Gjenbrukbare UI-komponenter
├── lib/                  API-klient, autentisering, hjelpefunksjoner
└── types/                Delte TypeScript-typer for API-svar
```

Alt under `(dashboard)` er beskyttet i middleware, ikke ved å skjule knapper. En
uinnlogget bruker som skriver inn URL-en direkte blir sendt til innlogging.

## Dataflyt: registrering av et utlån

Eksempelet viser hvordan lagene henger sammen, inkludert det tilfellet der
regelen slår inn.

```mermaid
sequenceDiagram
    participant A as Ansatt
    participant F as Frontend
    participant API as Api
    participant APP as Application
    participant D as Domain
    participant DB as PostgreSQL

    A->>F: Velger barn og utstyr, trykker "Registrer utlån"
    F->>API: POST /api/loans (JWT i header)
    API->>API: Validerer token og rollen Staff
    API->>APP: RegisterLoanCommand
    APP->>DB: Henter låntaker, utstyr og åpne lån
    APP->>D: Loan.Register(borrower, equipment, dueDate, clock)
    alt Låntaker har åpent forfalt lån eller er utestengt
        D-->>APP: Regelbrudd
        APP-->>API: Feil med årsak
        API-->>F: 409 Conflict + ProblemDetails
        F-->>A: Forklaring på hvorfor utlånet er blokkert
    else Utlån tillatt
        D-->>APP: Loan (Active), utstyr satt til OnLoan
        APP->>DB: Lagrer utlån og oppdatert utstyrstatus
        APP-->>API: LoanDto
        API-->>F: 201 Created
        F-->>A: Kvittering
    end
```

## Automatisk forfall

Forfall skal oppdages uten at ansatte gjør noe. Det finnes to måter:

1. **Beregnet ved lesing** - et lån er forfalt dersom `DueDate` er passert og
   `ReturnedAt` er tom. Ingen bakgrunnsjobb, alltid korrekt.
2. **Bakgrunnsjobb** - en jobb som med jevne mellomrom setter `Status = Overdue`
   på lån som har passert fristen.

Systemet bruker begge: statusen beregnes ved lesing slik at oversikten aldri
viser feil, og en bakgrunnsjobb skriver statusen til databasen slik at forfall
kan brukes i rapporter og spørringer. Det unngår at oversikten er avhengig av at
en jobb faktisk har kjørt. Se [ADR-0011](./adr/0011-automatisk-forfall.md).

## Kjøremiljø

Alle tre lagene kjøres i Docker Compose, slik at miljøet er likt hver gang.

| Tjeneste | Port | Beskrivelse |
| --- | --- | --- |
| `frontend` | 3000 | Next.js |
| `api` | 5080 | .NET-API |
| `db` | 5432 | PostgreSQL med navngitt volum for data |

Konfigurasjon settes med miljøvariabler, dokumentert i `.env.example`. Se
[`11-utviklingsmiljo.md`](./11-utviklingsmiljo.md) for oppsett og kjøring.

## Videre lesning

- Domenemodellen og reglene: [`03-domenemodell.md`](./03-domenemodell.md)
- Databaseskjema: [`04-databasedesign.md`](./04-databasedesign.md)
- Autentisering: [`06-autentisering.md`](./06-autentisering.md)
