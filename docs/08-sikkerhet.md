# Sikkerhet

> **Status:** påbegynt. Prinsippene og tiltakene er bestemt. Bekreftelse på at
> hvert tiltak faktisk er implementert fylles ut etter hvert.

Systemet inneholder personopplysninger om barn. Sikkerhet er derfor et krav i
seg selv, og ikke bare en kvalitetsegenskap. Personvernsiden er dokumentert i
[`09-lover-og-regler.md`](./09-lover-og-regler.md); dette dokumentet handler om
de tekniske tiltakene.

## Prinsipper

1. **Sikkerhet i backend, ikke i grensesnittet.** Frontend kan skjule en knapp,
   men det er backend som må avvise kallet. Alt annet er en illusjon av
   sikkerhet.
2. **Sikker som standard.** Endepunkter krever autentisering med mindre noe annet
   er eksplisitt bestemt, ikke omvendt.
3. **Minste privilegium.** En rolle får bare den tilgangen den trenger.
4. **Ingen hemmeligheter i koden.** All konfigurasjon som er hemmelig kommer fra
   miljøvariabler.

## Tiltak

| Område | Tiltak | Status |
| --- | --- | --- |
| Authentication | Auth0 håndterer identitet og passord. Systemet lagrer aldri passord selv. | Planlagt |
| Authorization | Rollebasert tilgang kontrollert på hvert endepunkt | Planlagt |
| Transport | HTTPS i produksjon | Planlagt |
| Tokens | Access tokens i kryptert server-side økt, aldri i `localStorage` | Planlagt |
| SQL-injeksjon | EF Core parametriserer alle spørringer. Ingen strengbygde SQL-spørringer. | Planlagt |
| Inndatavalidering | Validering på API-nivå før forespørselen når domenelaget | Planlagt |
| CORS | Kun frontend-origin tillates, satt via `Cors__AllowedOrigins` | Planlagt |
| Hemmeligheter | Miljøvariabler, `.env` i `.gitignore`, `.env.example` uten ekte verdier | Ferdig |
| Logging | Ingen personopplysninger i logger | Planlagt |
| Filopplasting | Bilder valideres på type og størrelse, lagres utenfor gjettbare URL-er | Planlagt |
| Avhengigheter | Kontroll av pakker og lisenser | Planlagt |

## Kjente risikoer og vurderinger

> Fyll ut underveis. Kandidater å vurdere:

| Risiko | Vurdering | Tiltak |
| --- | --- | --- |
| Ansatt med `Staff`-rolle ser alle låntakere | Nødvendig for daglig drift | Rollen gis kun til ansatte. Handlinger kan spores til ansatt. |
| Bilder kan inneholde utilsiktet informasjon | Bilder tas av utstyr, ikke av personer | Retningslinje dokumentert, og synliggjort i grensesnittet |
| Fritekstnotater kan inneholde sensitiv informasjon | Reell risiko | Retningslinjer for notatskriving, se `09-lover-og-regler.md` |
| Utestengelse er en inngripende handling | Krever dokumentasjon | Årsak registreres, bilder som grunnlag, menneskelig beslutning |

## Sporbarhet

> Dokumenter hva som logges av hvem som gjorde hva. Handlinger som utestengelse,
> opphevelse av utestengelse og registrering av tap skal kunne spores til en
> ansatt og et tidspunkt, uten at personopplysninger om låntakeren havner i
> applikasjonsloggen.

## Avhengigheter

Lisenser og vilkår er dokumentert i
[`12-lisenser-og-vilkar.md`](./12-lisenser-og-vilkar.md). Her handler det om
sikkerhetssiden av de samme pakkene.

> Fylles ut ved gjennomgangen av avhengigheter:
> - Kjør `dotnet list package --vulnerable --include-transitive` og
>   `bun audit`, og dokumenter funnene.
> - Se etter pakker som ikke lenger vedlikeholdes, og pakker som ble lagt til
>   for å prøve noe og aldri fjernet.
> - Hver avhengighet er en utvidelse av angrepsflaten. Færre pakker er et
>   sikkerhetstiltak i seg selv.

## Sikkerhetsgjennomgang

> Fylles ut ved sluttgjennomgangen: en kontroll av at tiltakene i tabellen over
> faktisk er på plass, med hvordan hvert punkt ble verifisert.
