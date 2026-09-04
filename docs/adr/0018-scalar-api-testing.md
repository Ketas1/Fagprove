# ADR-0018: Scalar som API-testverktøy i utviklingsmiljø

- **Status:** Akseptert
- **Dato:** 2026-09-04

## Kontekst

Backend-endepunktene bygget 2026-09-04 (kategori, foresatt, barn, utstyr,
utlån) trengte en måte å testes på direkte, uten frontend - både for å
verifisere at de faktisk fungerer mot en ekte database, og for å bekrefte at
autentisering håndheves riktig (`401` uten token, suksess med et gyldig).
`Microsoft.AspNetCore.OpenApi` var allerede på plass fra scaffoldingen
(`AddOpenApi()`/`MapOpenApi()`), men uten et grensesnitt å utforske
spesifikasjonen i - bare rå JSON.

To spørsmål måtte avklares før noe ble bygget: hvilket Scalar-pakke passer med
den eksisterende OpenAPI-genereringen, og hvordan skal testverktøyet selv
forholde seg til at **alle** endepunkter krever autentisering som standard
(`RequireAuthenticatedUser` som fallback-policy, se `06-autentisering.md`)?

## Beslutning

**Pakke:** `Scalar.AspNetCore` (uten avhengigheter, fungerer med hvilken som
helst OpenAPI-spesifikasjon), ikke `Scalar.AspNetCore.Microsoft` eller
`Scalar.AspNetCore.Swashbuckle`. Prosjektet bruker allerede den innebygde
`Microsoft.AspNetCore.OpenApi`-genereringen, ikke Swashbuckle - å legge til en
Swashbuckle-avhengighet bare for Scalar ville vært en ny, overflødig
spesifikasjonsgenerator ved siden av den som allerede finnes.

**Bearer-skjema i spesifikasjonen:** `AddOpenApi` konfigureres med en
`AddDocumentTransformer` som beskriver et HTTP Bearer-skjema
(`type: http, scheme: bearer, bearerFormat: JWT`) i den genererte
spesifikasjonen. Uten dette viser Scalar et generisk, tomt
"Authentication"-panel; med det får det et eget felt for å lime inn et
bearer-token.

**Autentisering av selve testsiden:** `app.MapOpenApi()` og
`app.MapScalarApiReference(...)` markeres begge `.AllowAnonymous()`, men
**utelukkende** inne i den eksisterende
`if (app.Environment.IsDevelopment())`-blokken i `Program.cs` - aldri i en
reell driftssituasjon. Uten unntaket ville den globale fallback-policyen krevd
autentisering for `/scalar` og `/openapi/v1.json` også, og det ville vært
umulig å åpne testsiden i det hele tatt uten et token allerede satt et annet
sted - en side som finnes for å teste autentisering kan ikke i seg selv kreve
den løst på forhånd. De faktiske `/api/*`-endepunktene er urørt og fullt
beskyttet, som før - det er nettopp det som skal testes.

**Ingen automatisk token-injisering.** Et alternativ som ble vurdert og
forkastet, se under - token limes inn manuelt i Scalars eget
"Authentication"-panel, hentet fra Auth0-dashbordet sin API-fane **Test**.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **`Scalar.AspNetCore` mot eksisterende `Microsoft.AspNetCore.OpenApi`, manuell token-innliming** | Ingen ny spesifikasjonsgenerator, ingen ny miljøvariabel, ingen ny middleware. Scalar husker tokenet i nettleseren til det byttes ut eller utløper. | Krever ett manuelt steg (hente og lime inn et token) etter at tokenet utløper. | Valgt |
| Swashbuckle ved siden av `Microsoft.AspNetCore.OpenApi` (opprinnelig eksempel) | Kjent oppsett fra andre prosjekter. | To spesifikasjonsgeneratorer for samme API - den ene ville aldri faktisk vært i bruk. Ny avhengighet uten reell gevinst. | Overflødig ved siden av det som allerede finnes |
| Automatisk token-injisering fra en `DEV_JWT_TOKEN`-miljøvariabel (opprinnelig eksempel) | Aldri manuelt steg, selv etter at et token utløper - bare oppdater `.env` og start backend på nytt. | Ny miljøvariabel å dokumentere, ny middleware, og løser et problem Scalar allerede løser selv ved å huske tokenet i nettleseren. | Unødvendig kompleksitet for gevinsten - kan legges til senere om manuell gjeninnliming viser seg upraktisk |
| Testsiden bak samme autentisering som resten av API-et, uten unntak | Null nye `[AllowAnonymous]`-unntak; matcher det dokumenterte prinsippet bokstavelig. | Testsiden kan ikke åpnes uten et token allerede satt - en side som finnes for å teste autentisering kan da aldri hjelpe med det aller første tokenet. | Løser ikke oppstartsproblemet |

## Konsekvenser

**Positivt**

- Endepunktene bygget 2026-09-04 kan verifiseres mot en ekte database uten
  frontend, inkludert at autentisering faktisk avviser manglende og ugyldige
  token - verifisert manuelt: `/api/health` gir `401` både uten token og med
  et ugyldig token, `200` med et gyldig.
- Ingen ny spesifikasjonsgenerator, ingen ny miljøvariabel.
- `/scalar` og `/openapi/v1.json` er nå det første stedet i prosjektet med et
  eksplisitt `[AllowAnonymous]`-unntak - nøyaktig slik `06-autentisering.md`
  allerede beskrev at et unntak skulle se ut, bare det første reelle
  eksempelet på det.

**Negativt eller risiko**

- `[AllowAnonymous]` er skrevet to steder (`MapOpenApi()` og
  `MapScalarApiReference()`), begge betinget av
  `IsDevelopment()`. En fremtidig endring som fjerner miljøsjekken ved en
  feil ville eksponert API-ets skjema (rutenavn, DTO-feltnavn) uten
  autentisering - ikke personopplysninger, men et brudd på prinsippet om at
  autentisering håndheves globalt. Verdt å holde øye med i kodegjennomgang.
- Et utløpt token gir en litt uklar feilmelding i Scalars grensesnitt (et
  `401` på selve API-kallet, ikke en tydelig "token utløpt"-melding) - et
  akseptert, mindre irritasjonsmoment mot å slippe en egen miljøvariabel.
