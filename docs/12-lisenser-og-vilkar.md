# Lisenser og vilkår

> **Status:** påbegynt. Oversikten under er ført opp fra teknologivalgene i
> prosjektet, og lisensene skal **verifiseres mot faktisk installerte pakker**
> ved sluttgjennomgangen. Nye pakker føres inn her når de legges til, ikke i en
> opprydding til slutt.

Oppdragsgiver er en kommunalt eid virksomhet. Det gjør lisensforhold og vilkår
for eksterne tjenester til noe som må være dokumentert og kontrollert, ikke noe
som forutsettes å gå bra.

## Hvorfor dette dokumentet finnes

Tre grunner:

1. **Offentlig eierskap.** En kommunalt eid virksomhet kan ikke ta i bruk
   programvare med vilkår som er uforenlige med offentlig drift, og må kunne vise
   hvilke lisenser løsningen bygger på.
2. **Videreutvikling.** Systemet skal kunne overtas av en annen IT-avdeling. De
   må vite hva de arver, uten å måtte kartlegge det selv.
3. **Copyleft-risiko.** Enkelte lisenser stiller krav til at avledet kode gjøres
   tilgjengelig. Det er håndterbart, men må være et bevisst valg og ikke en
   overraskelse.

## Lisenstyper og hva de betyr her

| Type | Eksempler | Konsekvens for prosjektet |
| --- | --- | --- |
| Permissive | MIT, Apache 2.0, BSD, PostgreSQL License | Fri bruk, endring og distribusjon. Krever normalt at lisenstekst og opphavsrett følger med. Uproblematisk. |
| Svak copyleft | LGPL, MPL 2.0 | Endringer i selve biblioteket må deles. Bruk som avhengighet er som regel greit. Vurderes hvis det dukker opp. |
| Sterk copyleft | GPL, AGPL | Kan kreve at hele applikasjonen gjøres tilgjengelig under samme lisens. **Skal unngås** i dette prosjektet. |
| Kommersielle vilkår | Auth0, Docker Desktop | Ikke en lisens, men bruksvilkår og eventuelt abonnement. Må leses, ikke antas. |

## Hovedavhengigheter

> Kolonnen "Bekreftet" fylles ut med faktisk output fra kommandoene nederst i
> dokumentet.

### Backend

| Pakke / komponent | Versjon | Forventet lisens | Bekreftet |
| --- | --- | --- | --- |
| .NET 10 / ASP.NET Core | 10.0 | MIT | |
| Microsoft.EntityFrameworkCore | 10.0.4 | MIT | |
| Microsoft.EntityFrameworkCore.Design | 10.0.4 | MIT | |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | PostgreSQL License | |
| Microsoft.AspNetCore.OpenApi | 10.0.11 | MIT | |
| xUnit | 2.9.3 | Apache 2.0 | |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.11 | MIT | |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT | |
| coverlet.collector | 6.0.4 | MIT | |
| DotNetEnv | 3.2.0 | MIT | |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.4 | MIT | |
| Scalar.AspNetCore | 2.11.3 | MIT | |
| Testcontainers | ikke installert | MIT | |

> EF Core er låst til **10.0.4**, ikke nyeste patch. Npgsql-provideren 10.0.3 er
> bygget mot 10.0.4, og en nyere EF-versjon i tillegg gir konflikt på
> `Microsoft.EntityFrameworkCore.Relational` når testprosjektet bygges.
> Versjonene følger provideren, ikke omvendt.

### Frontend

| Pakke / komponent | Versjon | Forventet lisens | Bekreftet |
| --- | --- | --- | --- |
| Next.js | 16.3.4 | MIT | |
| React / React DOM | 19.2.8 | MIT | |
| Tailwind CSS | 4 | MIT | |
| TypeScript | 5 | Apache 2.0 | |
| ESLint / eslint-config-next | 9 / 16.3.4 | MIT | |
| Jest | 30.5.1 | MIT | |
| jest-environment-jsdom | 30.5.1 | MIT | |
| @testing-library/react | 16.3.3 | MIT | |
| @testing-library/jest-dom | 7.0.1 | MIT | |
| @auth0/nextjs-auth0 | 4.28.0 | MIT | |
| shadcn (CLI, dev-avhengighet) | 4.20.1 | MIT | |
| @base-ui/react | 1.7.0 | MIT | |
| class-variance-authority | 0.7.1 | Apache 2.0 | |
| clsx | 2.1.1 | MIT | |
| tailwind-merge | 3.6.0 | MIT | |
| lucide-react | 1.40.0 | ISC | |
| tw-animate-css | 1.4.0 | MIT | |
| cmdk | 1.1.1 | MIT | |
| react-day-picker | 10.0.1 | MIT | Bekreftet 2026-09-07 mot `node_modules/react-day-picker/LICENSE` og `package.json` |
| recharts | 3.10.1 | MIT | Bekreftet 2026-09-07 mot `node_modules/recharts/LICENSE` og `package.json` |
| jspdf | 4.2.1 | MIT | Bekreftet 2026-09-07 mot `node_modules/jspdf/LICENSE` og `package.json` |
| jspdf-autotable | 5.0.8 | MIT | Bekreftet 2026-09-07 mot `node_modules/jspdf-autotable/LICENSE.txt` og `package.json` |
| write-excel-file | 4.1.1 | MIT | Bekreftet 2026-09-07 mot `node_modules/write-excel-file/LICENSE` og `package.json`. Importeres som `write-excel-file/browser` - pakken har ingen rot-eksport |
| Bun | 1.3.6 | MIT | |

> shadcn er ikke ett bibliotek som installeres og importeres, men en CLI som
> kopierer komponentkildekode inn i `src/components/ui/`. Radene over er de
> faktiske kjøretidsavhengighetene komponentene bruker (Base UI som
> tilgjengelig, ustylet fundament), ikke shadcn-prosjektet selv. Se
> [ADR-0016](./adr/0016-shadcn-ui.md).

### Database og drift

| Komponent | Forventet lisens | Bekreftet |
| --- | --- | --- |
| PostgreSQL | PostgreSQL License (BSD-lignende) | |
| Docker Engine | Apache 2.0 | |

### Designmal

> Frontend skal bygges på en ferdig designmal. Maler er et vanlig sted å arve
> uventede vilkår.
> **Før en mal tas i bruk:** før opp navn, kilde, lisens og eventuelle krav om
> attribusjon eller forbud mot kommersiell bruk her.

| Mal | Kilde | Lisens | Krav |
| --- | --- | --- | --- |
| | | | |

## Vilkår for eksterne tjenester

### Auth0

Auth0 er ikke en pakke, men en tjeneste som behandler personopplysninger på
vegne av oppdragsgiver. Følgende må være avklart:

- [ ] Hvilken plan brukes, og hva koster den ved reell drift
- [ ] Databehandleravtale (DPA) er inngått
- [ ] Tenant er satt opp i **EU-region**, slik at data lagres i EU/EØS
- [ ] Hva som faktisk lagres hos Auth0, og hva som blir liggende i egen database
- [ ] Hva som skjer med brukerdata dersom tjenesten avvikles - eksport og
      innlåsing

Se [`09-lover-og-regler.md`](./09-lover-og-regler.md) og
[ADR-0006](./adr/0006-auth0.md).

### Docker Desktop

Docker Engine er åpen kildekode under Apache 2.0. **Docker Desktop** har egne
kommersielle vilkår, og krever betalt abonnement for virksomheter over en viss
størrelse. Dette er verdt å merke seg for en kommunalt eid virksomhet, selv om
det ikke berører lokal utvikling.

- [ ] Kontroller gjeldende terskel og vilkår før produksjonsbruk
- [ ] Merk at Docker Engine alene, uten Desktop, ikke er omfattet

### E-postleverandør

> Fylles ut når leverandør for utsending av e-post til foresatte er valgt.
> Utsending av e-post til foresatte innebærer behandling av personopplysninger,
> og krever databehandleravtale på samme måte som Auth0.

## Kontroll av lisenser

Kjøres ved sluttgjennomgangen, og resultatet føres inn i tabellene over.

```bash
# Backend - alle pakker, inkludert transitive avhengigheter
cd backend
dotnet list package --include-transitive

# Frontend
cd frontend
bun pm ls
```

Gå gjennom listene og se etter:

- GPL eller AGPL i noen ledd, inkludert transitive avhengigheter
- pakker uten oppgitt lisens
- pakker som ikke lenger vedlikeholdes
- pakker som ble lagt til for å prøve noe, og som ikke er i bruk lenger

## Egen kildekode

> Avklar og dokumenter: hvilken lisens leveres selve løsningen under, og hvem
> eier opphavsretten til koden. Dette er et spørsmål for oppdragsgiver, ikke et
> teknisk valg, men det hører hjemme i dokumentasjonen ved en overlevering.
