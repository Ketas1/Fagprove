# ADR-0020: Server-komponenter leser direkte fra backend, klientkomponenter skriver gjennom proxyen

- **Status:** Akseptert
- **Dato:** 2026-09-06

## Kontekst

[ADR-0015](./0015-proxied-backend-for-frontend.md) etablerte at nettleseren
aldri skal kjenne backend-ets adresse eller se et token - alt går gjennom
samlefunksjonen `src/app/api/[...path]/route.ts`. Det eneste eksemplet som
fantes da sidene i dashbordet skulle bygges (`StatusWidget`) var en
klientkomponent som henter data i en `useEffect` mot nettopp den proxyen.

Med App Router er sider som standard **server-komponenter**
(`CLAUDE.md`: "server components by default"), og et dashbord med tabeller
for utlån, utstyr, låntakere og ansatte har mye mer lesing enn skriving.
Spørsmålet er om alle disse sidene skal følge `StatusWidget`s mønster
(klientkomponent + proxy), eller om en server-komponent kan hente data på en
annen måte - før flere sider ble bygget etter det ene eksisterende mønsteret
uten at det var vurdert.

## Beslutning

**Sidens første, lesende datahenting skjer i en server-komponent som kaller
backend-et direkte** (`src/lib/backend.ts`, `fetchBackend`) - henter et
access token med `auth0.getAccessToken()` og kaller backend-et med det,
uten å gå via `/api/*`-proxyen. **Alt som skriver (skjemaene i modalene)
skjer i en klientkomponent som fortsatt går gjennom `/api/*`-proxyen**,
nøyaktig slik ADR-0015 beskriver.

Begrunnelsen er at ADR-0015s løfte - at *nettleseren* aldri skal se et token
- ikke sier noe om hvordan Next.js-serveren selv henter data til en
server-komponent. En server-komponent eksponerer aldri noe til nettleseren i
utgangspunktet, så det er ingenting å vinne på at Next.js-serveren kaller sin
egen proxy-rute over HTTP for å hente noe den kunne hentet direkte.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Alle sider som klientkomponenter mot proxyen, slik `StatusWidget` gjør | Ett mønster å forholde seg til, minst mulig ny kode | Hver liste-side blir en klientkomponent med en loading-tilstand for noe som kunne vært klart før siden i det hele tatt sendes til nettleseren, i strid med CLAUDE.md sin "server components by default" | Mer klientkode og en unødvendig loading-tilstand for rene lesninger |
| **Server-komponent leser direkte, klient skriver via proxyen** | Følger CLAUDE.md sin standard. Ingen ekstra nettverkshopp for lesing (server-server i stedet for nettleser-server-server). Skriving beholder nøyaktig den garantien ADR-0015 alt har bevist riktig. | To datahentingsmønstre å kjenne til i stedet for ett - dokumentert her nettopp for å unngå at det blir en overraskelse. | Valgt |

## Konsekvenser

**Positivt**

- Tabellsidene (Utlån, Utstyr, Barn og foresatte, Ansatte) rendres ferdig
  utfylt på serveren - ingen synlig "laster..."-tilstand for data som allerede
  er hentet før siden vises.
- `src/lib/backend.ts` er det ene stedet et nytt server-komponent-kall
  legges til, på samme måte som `resolveBackendUrl` er det ene stedet en
  proxy-videresending bygges.

**Negativt eller risiko**

- To datahentingsmønstre i kodebasen: `fetchBackend` (server, lesing) og
  `fetch('/api/...')` (klient, skriving/mutasjon). Et nytt bidragsyter som
  ikke kjenner denne ADR-en kan velge feil mønster for en ny side - sjekk
  denne filen og `docs/13-frontend-designsystem.md` før en ny side bygges.
- `fetchBackend` har ingen egen feilhåndtering utover å kaste `BackendError`
  - en side som henter data må selv håndtere en `404` (se
    `app/dashboard/loans/[id]/page.tsx` for mønsteret med `notFound()`).
