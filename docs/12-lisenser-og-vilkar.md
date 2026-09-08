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
| .NET 10 / ASP.NET Core | 10.0 | MIT | Bekreftet 2026-09-08 - runtime og rammeverk er MIT, se [dotnet/runtime](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT) |
| Microsoft.EntityFrameworkCore | 10.0.4 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen (`<license type="expression">MIT</license>`) |
| Microsoft.EntityFrameworkCore.Design | 10.0.4 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | PostgreSQL License | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen (`PostgreSQL`) - permissiv, BSD-lignende |
| Microsoft.AspNetCore.OpenApi | 10.0.11 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| xUnit | 2.9.3 | Apache 2.0 | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen (`Apache-2.0`). Gjelder også `xunit.runner.visualstudio` 3.1.4 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.11 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| coverlet.collector | 6.0.4 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| DotNetEnv | 3.2.0 | MIT | Bekreftet 2026-09-08 - `.nuspec` peker på en lisensfil, og filen er MIT ("The MIT License (MIT), Copyright (c) 2016 Toni Solarin-Sodara") |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.4 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
| Scalar.AspNetCore | 2.11.3 | MIT | Bekreftet 2026-09-08 mot `.nuspec` i NuGet-cachen |
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

## Full gjennomgang av avhengighetstreet

Tabellene over dekker pakkene prosjektet velger selv. Den virkelige risikoen
ligger i de transitive avhengighetene - pakker ingen har valgt bevisst, men som
følger med. Frontend-treet ble skannet i sin helhet 2026-09-08 med
`license-checker-rseidelsohn`, som leser faktisk lisensmetadata fra hver pakke i
`node_modules`:

```bash
cd frontend
bunx license-checker-rseidelsohn --summary
```

Resultat, 904 pakker:

| Lisens | Antall |
| --- | --- |
| MIT | 766 |
| ISC | 53 |
| Apache-2.0 | 28 |
| BSD-3-Clause | 20 |
| BSD-2-Clause | 14 |
| BlueOak-1.0.0 | 9 |
| MPL-2.0 | 3 |
| Øvrige, én hver | MIT-0, Python-2.0, CC-BY-4.0, CC0-1.0, 0BSD, MIT\*, UNLICENSED, `Apache-2.0 AND LGPL-3.0-or-later`, `(MPL-2.0 OR Apache-2.0)`, `(MIT AND Zlib)`, `(MIT OR CC0-1.0)`, `MIT AND ISC` |

**Hovedfunnet: ingen GPL eller AGPL noe sted i treet.** Det er regelen som
betyr noe i tabellen øverst i dokumentet, og den holder.

### Funn som krever en merknad

| Funn | Vurdering |
| --- | --- |
| `UNLICENSED` | Dette er **prosjektets egen** `frontend/package.json`, ikke en tredjepartspakke. `name: "frontend"`, ingen `license`-nøkkel, så verktøyet rapporterer den som ulisensiert. Ingen juridisk risiko, men se avsnittet om egen kildekode under - ved en overlevering bør leveransen ha en uttalt lisens. |
| `@img/sharp-win32-x64` - `Apache-2.0 AND LGPL-3.0-or-later` | Den eneste copyleft-komponenten i treet. Er Windows-binæret til `sharp` (bildebehandling, LGPL-delen er libvips), som Next.js installerer for `next/image`. **`next/image` brukes ikke noe sted i `src/`** - LGPL-komponenten ligger i `node_modules`, men kjøres aldri. LGPL tillater dessuten bruk som dynamisk lenket avhengighet uten at egen kode smittes. Merk at dette er `win32`-binæret, altså et artefakt fra utviklingsmaskinen; et Linux-bygg i Docker henter et annet. |
| `caniuse-lite` - `CC-BY-4.0` | Datapakke med nettleserstøtte, brukt av browserslist under bygging. CC-BY krever navngivelse. Dataene distribueres ikke videre i bundelen, men navngivelse hører hjemme her. |
| `rgbcolor@1.0.1` - `MIT*` | Stjernen betyr at verktøyet **gjettet** lisensen fra en README, fordi pakken verken har `license`-felt eller lisensfil. Kommer inn via `jspdf` → `canvg` → `rgbcolor`, altså i rapporteksporten ([ADR-0023](./adr/0023-rapporteksport.md)) - en reell produksjonssti, ikke bare et utviklingsverktøy. Lav risiko, men **ubekreftet**, og det eneste punktet i treet som ikke lar seg verifisere fra pakken selv. |
| `argparse` - `Python-2.0` | Python Software Foundation License 2.0. Permissiv og GPL-kompatibel. Transitiv, via ESLint-kjeden. Ingen konsekvens. |
| MPL-2.0 (`axe-core`, `lightningcss` ×2) | Svak copyleft på filnivå. `axe-core` er tilgjengelighetstesting (utvikling), `lightningcss` er CSS-verktøy som følger med Tailwind 4. Begge brukes uendret som avhengigheter, og MPL stiller da ingen krav til egen kode. `dompurify` er `(MPL-2.0 OR Apache-2.0)` - Apache-2.0 kan velges. |

### Backend

`dotnet list package --include-transitive` lister pakker, men ikke lisenser.
De direkte pakkene er derfor verifisert mot `.nuspec`-metadataen i den lokale
NuGet-cachen, som er den samme metadataen NuGet.org viser. Resultatet står i
"Bekreftet"-kolonnen over: **alt er MIT, med unntak av Npgsql (PostgreSQL
License) og xUnit (Apache-2.0)** - alle tre permissive.

**Transitive NuGet-pakker er ikke maskinelt gjennomgått.** De kommer i praksis
fra `dotnet/runtime` og `dotnet/efcore`, som begge er MIT, men det er en
antakelse dette dokumentet ikke har verifisert. Et verktøy som `nuget-license`
vil kunne lukke det hullet.

## Kontroll av lisenser

Kjøres ved sluttgjennomgangen, og resultatet føres inn i tabellene over.


```bash
# Backend - alle pakker, inkludert transitive avhengigheter
cd backend
dotnet list package --include-transitive

# Frontend - navn og versjoner
cd frontend
bun pm ls

# Frontend - faktiske lisenser for hele treet
bunx license-checker-rseidelsohn --summary
bunx license-checker-rseidelsohn --csv    # per pakke, for å finne igjen et enkelt funn
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

Per 2026-09-08 er spørsmålet **ikke avklart**, og det synes i verktøyet:
`frontend/package.json` har ingen `license`-nøkkel, så skanningen rapporterer
prosjektet selv som `UNLICENSED`. Det er ikke en feil i seg selv - en privat,
ikke-publisert pakke skal gjerne være det - men for en leveranse som skal
overtas av en annen IT-avdeling bør det stå eksplisitt hva de har lov til å
gjøre med koden.

To ting bør på plass før overlevering:

1. En avklaring med oppdragsgiver om opphavsrett og lisens for løsningen.
2. Når den er tatt: sett `"license"` i `frontend/package.json`, legg en
   `LICENSE`-fil i repoets rot, og noter valget her.
