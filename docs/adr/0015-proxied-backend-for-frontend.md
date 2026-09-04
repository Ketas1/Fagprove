# ADR-0015: Proxied backend-for-frontend i stedet for direkte kall fra nettleseren

- **Status:** Akseptert
- **Dato:** 2026-09-03

## Kontekst

Frontend trenger en måte å kalle backend-API-et på når en innlogget bruker
gjør noe. `06-autentisering.md` har allerede lovet at "access tokens ligger i
en kryptert, server-side økt [og] eksponeres aldri i `localStorage` eller i
klientkoden" - spørsmålet er hvordan API-kall i praksis kan holde det løftet.

To reelle måter å gjøre det på ble vurdert: nettleseren kaller API-et direkte
med et token den selv har fått utlevert, eller nettleseren kaller kun sin
egen Next.js-server, som via ren server-til-server-kommunikasjon henter
tokenet og videresender kallet.

## Beslutning

Frontend har én samlefunksjon, `src/app/api/[...path]/route.ts`, som tar imot
alle kall til `/api/*` fra nettleseren og videresender dem til det ekte
backend-API-et. Nettleseren kjenner aldri backend-ets adresse og ser aldri et
token - den henvender seg utelukkende til sin egen opprinnelse.

To krav til denne funksjonen ble avklart før den ble bygget, ikke underveis:

1. **Utgående headere bygges fra bunnen av, aldri videresendt fra det
   innkommende kallet.** `Cookie` inneholder appens egen krypterte økt og skal
   aldri nå backend-et.
2. **Backend-URL-en bygges med `URL`-konstruktøren mot én betrodd
   basiskonstant** (`resolveBackendUrl` i `src/lib/api.ts`), aldri ved å sette
   sammen den innkommende stien som tekst.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Nettleseren kaller backend-et direkte, med et token hentet fra en egen "gi meg et token"-rute | Enklere mentalt: ingen videresendingslag. Vanlig SPA-mønster. | Tokenet må uansett eksponeres til klientkoden et sted for å kunne legges på kallet, i strid med det som allerede er lovet i `06-autentisering.md`. Krever CORS, som må konfigureres riktig og holdes riktig etter hvert som miljøer endrer seg. | Motsier et løfte som allerede er dokumentert, i stedet for å oppfylle det |
| **Én samlefunksjon som videresender alt** | Tokenet forlater aldri serveren. Ingen CORS i det hele tatt, fordi backend-et bare noensinne snakker med Next.js-serveren. | Må håndtere vilkårlige metoder, stier og kroppsformater riktig - se de to kravene over. | Valgt |
| Én egen rute per endepunkt (for eksempel `/api/health-proxy`) | Eksplisitt og lett å se hva som videresendes hvor. | Antall endepunkter i systemet vokser med domenemodellen; hver ny backend-rute ville krevd en ny frontend-fil som gjør nøyaktig det samme. | Videresendingslogikken er identisk uansett endepunkt - forskjellen er bare stien |

## Konsekvenser

**Positivt**

- Oppfyller løftet i `06-autentisering.md` bokstavelig: tokenet finnes aldri i
  noe nettleseren kan lese.
- Ingen CORS-konfigurasjon å holde riktig - `Cors:AllowedOrigins` er fjernet
  fra backend-et.
- Nye backend-endepunkter krever ikke ny videresendingskode i frontend.

**Negativt eller risiko**

- Videresendingsfunksjonen er ett sted med reelt sikkerhetsansvar
  (header-allow-listen og URL-oppløsningen over). En feil der påvirker alle
  kall, ikke bare ett endepunkt.
- Store filopplastinger (utstyrsbilder, se `03-domenemodell.md`) går gjennom
  ett ekstra ledd. Ikke testet med reelle filer ennå - vurderes når
  bildeopplasting bygges.
- Én ekstra nettverkshopp (nettleser → Next.js → API) sammenlignet med et
  direkte kall. Ubetydelig i utvikling og på samme region i drift.
