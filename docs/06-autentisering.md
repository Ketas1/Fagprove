# Authentication og authorization

> **Status:** authentication er bygget og virker end-to-end. Authorization
> (rollebasert tilgang) er bevisst ikke bygget ennå - se "Roller" under.

Authentication settes opp **før** resten av systemet, fordi det er en sentral del
av sikkerheten og fordi det er enklere å bygge funksjonalitet inn i et system som
allerede er beskyttet enn å sikre det i etterkant.

## Valg av løsning

Auth0, se [ADR-0006](./adr/0006-auth0.md). Hovedgrunnen er at systemet da slipper
å håndtere passord selv - lagring, hashing, tilbakestilling og
brute force-beskyttelse er de vanligste stedene å gjøre en sikkerhetsfeil.

Auth0-tenanten er satt opp i **EU-region**, se
[`09-lover-og-regler.md`](./09-lover-og-regler.md).

## Begreper

| Begrep | Betydning |
| --- | --- |
| Authentication | Hvem er brukeren |
| Authorization | Hva har brukeren lov til |

## Flyt

1. Bruker trykker "Logg inn" på `/`, som sender til `/auth/login`.
2. Next.js sender brukeren til Auth0 (OIDC Authorization Code Flow med PKCE),
   med `audience` satt eksplisitt til `AUTH0_AUDIENCE` - se merknaden under
   "Konfigurasjon".
3. Auth0 autentiserer og sender brukeren tilbake til `/auth/callback` med en
   kode.
4. Next.js veksler koden inn i en økt (kryptert, server-side) og får et access
   token for API-et.
5. Brukeren havner på `/dashboard`.
6. Alt `/dashboard` trenger fra backend-et - i dag statusvisningen - går
   gjennom frontendens egen proxy (`src/app/api/[...path]/route.ts`), som
   henter tokenet fra økten og legger det på som
   `Authorization: Bearer <token>` **på serversiden**. Nettleseren ser aldri
   tokenet eller backend-ets adresse. Se [ADR-0015](./adr/0015-proxied-backend-for-frontend.md).
7. Backend validerer signatur, `issuer`, `audience` og utløpstid mot Auth0
   (`AddJwtBearer`, se "Beskyttelse av backend").

## Roller

**Ikke bygget ennå - bevisst utsatt.** Tabellen under er den planlagte
inndelingen, avtalt i samtale, men det finnes i dag ingen rolle-claim i
tokenet og ingen kode som leser en rolle. Alle innloggede brukere har lik
tilgang. De 1-4 kontoene som finnes i Auth0 nå er testbrukere opprettet
manuelt i dashbordet; hvilken av dem som er "administrator" er bare en
uformell merkelapp, ikke noe systemet vet om.

| Rolle | Tilgang |
| --- | --- |
| `User` | Foresatt. Begrenset tilgang til egne opplysninger. |
| `Staff` | Ansatt. Full tilgang til daglig drift: utstyr, brukere, utlån, oppfølging og rapporter. |
| `Admin` | Ansatt med utvidede rettigheter, blant annet brukeradministrasjon og sletting. |

Når dette bygges, tildeles rollene i Auth0 og legges inn i tokenet som et
claim via en Auth0 Action. Claim-navn og mapping i backend er ikke bestemt.

## Beskyttelse av backend

- **Alle endepunkter krever autentisering som standard**, håndhevet med en
  fallback-policy (`RequireAuthenticatedUser`) satt i `Program.cs` - ikke med
  `[Authorize]` lagt til på hver kontroller for seg. Et unntak (for eksempel
  et fremtidig offentlig endepunkt) må markeres eksplisitt med
  `[AllowAnonymous]`.
- Kall uten gyldig token gir `401`. Verifisert i
  `HealthEndpointTests.Health_endpoint_without_a_token_returns_401`.
- Rollebasert autorisasjon (`403` ved feil rolle) er ikke bygget - se
  "Roller" over.

## Beskyttelse av frontend

- `/dashboard` sjekker økten server-side (`auth0.getSession()`) og sender en
  uinnlogget bruker til `/auth/login` med en redirect, ikke ved å skjule
  innhold i klienten.
- `/` sender en allerede innlogget bruker videre til `/dashboard`.
- Access tokens ligger i en kryptert, server-side økt. De eksponeres aldri i
  `localStorage`, i klientkoden, eller i noe nettleseren kan lese - se
  [ADR-0015](./adr/0015-proxied-backend-for-frontend.md) for hvordan
  proxyen holder det løftet i praksis.

## Auth0-dashbordoppsett

Gjort manuelt i Auth0-dashbordet, dokumentert her slik at en overtagelse ikke
må rekonstruere det ved prøving og feiling:

- **Sign-ups er deaktivert** på database-connectionen. Dette er det som
  faktisk fjerner registreringsfanen fra Universal Login og sørger for at
  "1-4 faste brukere" holder seg til nøyaktig det - den eneste måten å
  opprette en konto på er at noen gjør det manuelt i dashbordet.
- **Ingen sosiale innloggingsleverandører** (Google med flere) er koblet til
  denne applikasjonen. Uten en tilkoblet social connection viser Universal
  Login kun brukernavn/passord-skjemaet.
- **Allowed Callback URLs:** `http://localhost:3000/auth/callback`.
  **Allowed Logout URLs:** `http://localhost:3000`.
- Et eget **API** er registrert (Dashboard → Applications → APIs), med
  **Identifier** identisk med `Auth0__Audience`/`AUTH0_AUDIENCE`
  (`https://api.sportforalle.no` i `.env.example`), signeringsalgoritme
  RS256. Dette er forskjellig fra Application-en (brukt til innlogging) - uten
  denne API-registreringen har Auth0 ingenting gyldig å utstede et
  API-scoped access token mot.
- 1-4 brukere er opprettet manuelt under User Management → Users.

## Konfigurasjon

Variablene er dokumentert i `.env.example`, og lastes av begge lag fra én
delt `.env` i repo-roten - se `11-utviklingsmiljo.md`. Backend bruker
`Auth0__Domain` og `Auth0__Audience`, frontend bruker `AUTH0_*`.
`AUTH0_AUDIENCE` i frontend må være identisk med `Auth0__Audience` i backend,
ellers utsteder Auth0 et token API-et ikke godtar.

**Én ting å være obs på:** `AUTH0_AUDIENCE` er ikke en av variablene
`@auth0/nextjs-auth0`-SDK-en leser automatisk (i motsetning til
`AUTH0_DOMAIN`/`AUTH0_CLIENT_ID`/`AUTH0_SECRET`). Den må sendes eksplisitt som
`authorizationParameters.audience` i `src/lib/auth0.ts` - uten det ber
frontend aldri Auth0 om et token for riktig audience, og Auth0 utsteder da et
ugyldig (ikke API-scoped) token uten at noe feiler synlig før backend-kallet
avvises.

Det finnes ingen `Cors__AllowedOrigins` lenger: nettleseren kaller aldri
backend-et direkte, bare frontendens egen proxy gjør, og det er ikke en
cross-origin-forespørsel å tillate.

## Testing

- **Kall uten token gir `401`:** dekket,
  `HealthEndpointTests.Health_endpoint_without_a_token_returns_401`.
- **Et kall med gyldig autentisering går gjennom:**
  `HealthEndpointTests.Health_endpoint_reports_a_status_once_authenticated`,
  med en `TestAuthHandler` som erstatter den ekte JWT-valideringen slik at
  testen ikke trenger et ekte Auth0-token.
- **En uinnlogget bruker sendes til innlogging, ikke direkte til beskyttet
  innhold:** verifisert manuelt (`curl` mot `/dashboard` uinnlogget gir en
  redirect til `/auth/login`). Ikke dekket av en automatisert frontend-test
  ennå - Server Component-redirects er upraktiske å teste med Jest alene, og
  end to end-testing er ikke satt opp, se `07-testing.md`.
- **Et kall med `User`-token mot et `Staff`-endepunkt gir `403`:** ikke
  relevant ennå, siden roller ikke er bygget.
