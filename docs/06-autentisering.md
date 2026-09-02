# Authentication og authorization

> **Status:** ikke ferdigstilt. Strukturen under viser hva dokumentet skal
> dekke, og hvilke valg som allerede er tatt.

Authentication settes opp **før** resten av systemet, fordi det er en sentral del
av sikkerheten og fordi det er enklere å bygge funksjonalitet inn i et system som
allerede er beskyttet enn å sikre det i etterkant.

## Valg av løsning

Auth0, se [ADR-0006](./adr/0006-auth0.md). Hovedgrunnen er at systemet da slipper
å håndtere passord selv - lagring, hashing, tilbakestilling og
brute force-beskyttelse er de vanligste stedene å gjøre en sikkerhetsfeil.

Auth0-tenanten settes opp i **EU-region**, se
[`09-lover-og-regler.md`](./09-lover-og-regler.md).

## Begreper

| Begrep | Betydning |
| --- | --- |
| Authentication | Hvem er brukeren |
| Authorization | Hva har brukeren lov til |

## Flyt

> Sett inn sekvensdiagram for innlogging og for et autentisert API-kall.

Planlagt flyt:

1. Bruker trykker "Logg inn" i frontend.
2. Next.js sender brukeren til Auth0 (OIDC Authorization Code Flow med PKCE).
3. Auth0 autentiserer og sender brukeren tilbake med en kode.
4. Next.js veksler koden inn i en økt, og får et access token for API-et.
5. Frontend sender tokenet som `Authorization: Bearer <token>` til backend.
6. Backend validerer signatur, `issuer`, `audience` og utløpstid mot Auth0.
7. Rollen leses fra tokenet og avgjør tilgangen til endepunktet.

## Roller

| Rolle | Tilgang |
| --- | --- |
| `User` | Foresatt. Begrenset tilgang til egne opplysninger. |
| `Staff` | Ansatt. Full tilgang til daglig drift: utstyr, brukere, utlån, oppfølging og rapporter. |
| `Admin` | Ansatt med utvidede rettigheter, blant annet brukeradministrasjon og sletting. |

Rollene tildeles i Auth0 og legges inn i tokenet som et claim.

> Dokumenter det endelige claim-navnet og hvordan rollene mappes i backend.

## Beskyttelse av backend

- Alle endepunkter krever autentisering som standard. Unntak må være eksplisitte.
- Autorisasjon kontrolleres på **hvert enkelt endepunkt**, ikke bare i
  grensesnittet. Et `Staff`-endepunkt skal avvise et `User`-token selv om knappen
  aldri vises i UI-et.
- Kall uten gyldig token gir `401`, gyldig token uten riktig rolle gir `403`.

## Beskyttelse av frontend

- Alt under det innloggede området beskyttes i middleware, ikke ved å skjule
  knapper.
- En uinnlogget bruker som skriver inn en URL direkte sendes til innlogging.
- Access tokens ligger i en kryptert, server-side økt. De eksponeres aldri i
  `localStorage` eller i klientkoden.

## Konfigurasjon

Variablene er dokumentert i `.env.example`. Backend bruker `Auth0__Domain` og
`Auth0__Audience`, frontend bruker `AUTH0_*`. `AUTH0_AUDIENCE` i frontend må være
identisk med `Auth0__Audience` i backend, ellers utsteder Auth0 et token API-et
ikke godtar.

## Testing

> Dokumenter hvordan det testes at:
> - et kall uten token gir `401`
> - et kall med `User`-token mot et `Staff`-endepunkt gir `403`
> - en uinnlogget bruker ikke får åpnet en beskyttet side i frontend
>
> Dette er blant de viktigste testene i prosjektet og skal dekkes av
> integrasjonstester, ikke bare manuell utprøving.
