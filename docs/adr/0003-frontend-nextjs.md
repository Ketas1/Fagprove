# ADR-0003: Next.js, Bun og Tailwind i frontend

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Grensesnittet brukes av ansatte i butikken til registrering av utstyr, brukere og
utlån, oppfølging av forfalte lån og uthenting av rapporter. Det skal være
brukervennlig, intuitivt og moderne, og bygges på kort tid.

## Beslutning

Frontend bygges med Next.js (App Router) og TypeScript, styles med Tailwind CSS,
og bruker Bun til pakkehåndtering og kjøring.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Next.js** | Gir struktur ut av boksen: routing, layouts, server- og klientkomponenter. God Auth0-integrasjon. Stort økosystem av ferdige komponenter og maler. | Mer rammeverk enn en ren SPA trenger. | Valgt |
| React med Vite | Enklere og lettere. Full kontroll. | Routing, beskyttelse av sider og prosjektstruktur må settes opp selv. Det koster tid som ikke finnes. | Strukturen Next.js gir er verdt mer enn kontrollen som ofres |
| Blazor | Samme språk som backend, én stack. | Mindre erfaring med det. Færre ferdige designmaler. Går glipp av økosystemet rundt React. | Designmaler og komponenter er avgjørende når UI skal bygges på kort tid |

### Tailwind

Tailwind velges fordi UI-et kan bygges direkte i markupen uten å bytte mellom
filer, og fordi de fleste ferdige designmalene som er aktuelle er bygget med
Tailwind. Alternativet, egne CSS-moduler, gir renere markup men går vesentlig
saktere.

### Bun

Bun brukes til å installere pakker og kjøre utviklingsserveren, fordi det er
merkbart raskere enn npm. Det er et verktøyvalg, ikke et rammeverksvalg: skulle
noe ikke fungere, kan `npm` brukes i stedet uten at koden endres.

## Konsekvenser

**Positivt**

- Sideoppsett, routing og beskyttelse av det innloggede området følger et kjent
  mønster i stedet for å måtte designes fra bunnen.
- Serverkomponenter gjør at access tokens kan holdes på serversiden og aldri
  eksponeres i nettleseren, se ADR-0006.
- En ferdig designmal kan tas i bruk direkte.

**Negativt eller risiko**

- Skillet mellom server- og klientkomponenter er en vanlig kilde til feil.
  Håndteres med regelen om at serverkomponenter er standard.
- Bun er mindre utbredt enn npm i CI-sammenheng. GitHub Actions har en offisiell
  `oven-sh/setup-bun`, så risikoen er liten.
