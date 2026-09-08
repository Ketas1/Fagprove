# Authentication og authorization

> **Status:** authentication er bygget og virker end-to-end. Siden
> 2026-09-04 kreves i tillegg at brukeren er koblet til en `Staff`-profil,
> se "Kobling mellom Staff og Auth0" under - det er fortsatt ikke det samme
> som rollebasert autorisasjon, som er bevisst ikke bygget ennå, se
> "Roller".

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

## Kobling mellom Staff og Auth0

Bygget 2026-09-04, se [ADR-0019](./adr/0019-staff-auth0-mapping.md) for
resonnementet og alternativene som ble vurdert.

Et Auth0 access token bærer bare `sub` (subjektidentifikatoren) - ikke navn
eller e-post, med mindre egendefinerte claims er lagt til, noe som ikke er
gjort her. Systemet kan derfor ikke automatisk opprette en ferdig utfylt
`Staff`-profil ved første innlogging; koblingen gjøres i stedet via to nye
endepunkter, begge nådd av enhver innlogget bruker, koblet eller ikke:

1. `POST /api/staff` - registrer en `Staff`-profil (kun navn).
2. `POST /api/staff/{id}/link-me` - kobler den innloggede brukerens eget
   `sub`-claim til den profilen. Ingen forespørselskropp - backend-et leser
   `sub` fra token.

`GET /api/staff` er også nådd uten kobling, slik en ansatt kan sjekke om
profilen sin allerede finnes før de oppretter en ny.

I praksis gjøres dette gjennom **`/dashboard/staff`** i frontend - en enkel
side (`components/StaffLinkPanel.tsx`) med et skjema for å opprette en
profil og en "Koble til min konto"-knapp per ukoblet profil i listen. Dette
er den tiltenkte måten å koble seg til på; å kalle de to endepunktene
direkte (for eksempel via Scalar) er bare et alternativ for feilsøking, se
`11-utviklingsmiljo.md`.

**Fra og med denne endringen avvises et kall fra en autentisert, men ukoblet
bruker med `403 Forbidden` og `reason: StaffNotLinked`** - se "Beskyttelse av
backend" under. De tre endepunktene over er unntatt nettopp for å løse
dette: uten et sted å koble seg til, ville ingen noensinne kommet forbi
sperren.

Én Auth0-konto kan bare kobles til én `Staff`-profil, og én `Staff`-profil
kan bare kobles til én Auth0-konto - håndhevet både i tjenestelaget
(`StaffRules`, med en lesbar `reason`) og av en unik indeks i databasen som
backstop, se `04-databasedesign.md`.

## Beskyttelse av backend

- **Alle endepunkter krever autentisering som standard**, håndhevet med en
  fallback-policy (`RequireAuthenticatedUser`) satt i `Program.cs` - ikke med
  `[Authorize]` lagt til på hver kontroller for seg. Et unntak (for eksempel
  et fremtidig offentlig endepunkt, eller Scalar/OpenAPI i utviklingsmiljøet,
  se `11-utviklingsmiljo.md`) må markeres eksplisitt med `[AllowAnonymous]`.
- Kall uten gyldig token gir `401`. Verifisert i
  `HealthEndpointTests.Health_endpoint_without_a_token_returns_401`.
- **Et lag til, siden 2026-09-04:** en autentisert forespørsel må i tillegg
  komme fra en bruker koblet til en `Staff`-profil - se "Kobling mellom
  Staff og Auth0" under. Håndheves av
  `Middleware/RequireLinkedStaffMiddleware.cs`, ikke av fallback-policyen -
  et endepunkt som skal være unntatt (for eksempel helsesjekken, eller
  Staff-endepunktene selv) markeres eksplisitt med `[AllowUnlinkedStaff]`,
  se `docs/adr/0019-staff-auth0-mapping.md`.
- Rollebasert autorisasjon (`403` ved feil rolle) er ikke bygget - se
  "Roller" over. **Ikke forveksle med det forrige punktet:** «koblet til en
  Staff-profil» er ikke det samme som «har riktig rolle». Alle koblede
  brukere har i dag lik tilgang.

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

## Utløpt token og fornyelse

Dette er den fellen som faktisk slo til under utvikling 2026-09-08, og den er
verdt å kjenne før man feilsøker en «uforklarlig» 500 på en beskyttet side.

**Øktinformasjonskapselen og access-tokenet utløper uavhengig av hverandre.**
`auth0.getSession()` dekrypterer bare informasjonskapselen; den sier ingenting
om hvorvidt access-tokenet inni fortsatt er gyldig. Vakten i
`app/dashboard/layout.tsx` sjekker nettopp `getSession()`, så den slipper
brukeren gjennom lenge etter at tokenet er dødt. Feilen dukket derfor først opp
nede i `fetchBackend`, midt i renderingen:

```
Error [AccessTokenError]: The access token has expired and a refresh token
was not provided. The user needs to re-authenticate.
  code: 'missing_refresh_token'
```

Resultatet var en 500 i utviklingsmodus og en «Noe gikk galt»-side i
produksjon - altså «noe er ødelagt», når det riktige svaret er «logg inn
igjen». Den ansatte hadde ingen vei videre uten å gå manuelt til
`/auth/logout`, som er den eneste handlingen som tømmer informasjonskapselen og
dermed utløser den ene tilstanden vakten faktisk håndterer (`!session`).

**Håndteringen nå:** `fetchBackend` fanger `AccessTokenError` og sender
brukeren til `/auth/login` i stedet for å kaste. Bare den feiltypen behandles
slik - en manglende miljøvariabel eller en feilkonfigurert tenant er en ekte
feil og får fortsatt bli synlig, framfor å bli forkledd som en
innloggingsforespørsel. Proxy-ruten (`app/api/[...path]/route.ts`) gjorde
allerede det tilsvarende, men svarer `401` i stedet, siden den kalles fra
nettleseren og ikke kan omdirigere en `fetch`.

**Hvorfor det ikke fantes noe refresh token.** SDK-ens `DEFAULT_SCOPES`
inneholder allerede `offline_access` (bekreftet i
`node_modules/@auth0/nextjs-auth0/dist/utils/constants.js`), så det er *ikke*
et scope som mangler i koden. Auth0 nekter å utstede et refresh token når
**«Allow Offline Access» er avslått på API-et** (Dashboard → Applications →
APIs → `https://api.sportforalle.no` → Settings). Da fjernes `offline_access`
stille fra de innvilgede scopene.

> **Gjenstår før drift:** slå på «Allow Offline Access» på API-et. Uten det må
> den ansatte logge inn på nytt hver gang access-tokenet utløper (Auth0 sin
> standard er 24 timer). Med det fornyes tokenet i bakgrunnen. Merk at et
> refresh token lagres ved innlogging, så eksisterende økter må logge ut og inn
> igjen før de får ett.

**Kjent begrensning:** hvis Auth0 skulle levere et ubrukelig token rett etter
en vellykket innlogging, vil omdirigeringen over gå i løkke. Det krever en
grunnleggende feilkonfigurert tenant - et ferskt token er gyldig per
definisjon - men det er dette man skal se etter hvis innloggingssiden begynner
å gjenta seg selv.

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
- **En innlogget bruker med utløpt token sendes til innlogging i stedet for å
  få en 500:** dekket i `frontend/src/lib/backend.test.ts`, som også sjekker at
  andre feil (for eksempel en manglende miljøvariabel) fortsatt kastes videre
  i stedet for å bli forkledd som innlogging. Se «Utløpt token og fornyelse»
  over.
- **Et kall med `User`-token mot et `Staff`-endepunkt gir `403`:** ikke
  relevant ennå, siden roller ikke er bygget.
- **Et kall fra en autentisert, men ukoblet bruker gir `403 StaffNotLinked`,
  mens Staff-endepunktene og helsesjekken forblir nådd:** dekket i
  `RequireLinkedStaffMiddlewareTests.cs`.
- **Kobling, og de to konfliktene den kan gi** (en profil som allerede er
  koblet, en konto allerede koblet til en annen profil): dekket i
  `StaffEndpointTests.cs` og enhetstestet direkte i `StaffRulesTests.cs`.
- **`CreatedByStaffId` populeres med en reell, koblet ansatt** ved en
  vanlig oppretting: dekket i
  `GuardiansEndpointTests.Create_populates_CreatedByStaffId_with_a_real_staff_member`.
