# ADR-0019: Staff↔Auth0-kobling og «reject unless linked»

- **Status:** Akseptert
- **Dato:** 2026-09-04

## Kontekst

`Staff` har siden kjerneentitetene ble laget (`8e67627`, 2026-09-03) hatt et
nullbart `Auth0UserId`-felt og en `LinkAuth0User(...)`-metode klar til bruk,
men ingenting kalte den - `CreatedByStaffId`/`UpdatedByStaffId` var `null` i
alle fem tjenestene bygget i CRUD-omgangen 2026-09-04
(`docs/adr/0017-global-exception-handler.md`). Å faktisk koble en innlogget
Auth0-bruker til en `Staff`-rad, og bruke den koblingen i stedet for `null`,
er denne endringen.

To spørsmål ble avklart i samtale før noe ble bygget:

1. Hva skjer med et kall fra en gyldig, autentisert, men ukoblet bruker?
2. Hvordan oppstår selve den første koblingen, gitt at det ikke fantes noe
   Staff-endepunkt fra før, og at et Auth0 access token normalt ikke bærer
   `name`/`email` - bare `sub` - med mindre egendefinerte claims er lagt til?

## Beslutning

**Et kall fra en autentisert, men ukoblet bruker avvises med `403`.** Ikke
den forsiktigere varianten (la kallet gå gjennom, la `CreatedByStaffId`
forbli `null`) - en bevisst innstramming utover dagens
`RequireAuthenticatedUser`-policy, se `06-autentisering.md`.

**En liten selvbetjenings-API for Staff**, ikke en manuell
databasesetning:

| Metode | Rute | Beskrivelse |
| --- | --- | --- |
| `GET` | `/api/staff` | Liste - slik en ukoblet bruker kan se om profilen sin allerede finnes |
| `POST` | `/api/staff` | Registrer en ny Staff-profil (kun navn) |
| `POST` | `/api/staff/{id}/link-me` | Kobler den innloggede brukerens egen `sub` til den oppgitte profilen |

Alle tre er markert `[AllowUnlinkedStaff]` - de finnes nettopp for å løse
oppstartsproblemet punkt 1 skaper: uten et unntak kunne ingen noensinne blitt
koblet i utgangspunktet.

**Håndhevelsen** er en ny `Middleware/RequireLinkedStaffMiddleware.cs`,
koblet inn etter `UseAuthorization()`: for enhver autentisert forespørsel som
ikke er markert `[AllowUnlinkedStaff]` eller `[AllowAnonymous]`, slår den opp
en `Staff`-rad på `sub`-claimet. Finnes ingen, kastes en ny
`Validation.ForbiddenException` → `403` med `reason: StaffNotLinked`,
gjennom den samme oversettelsen som `docs/adr/0017-global-exception-handler.md`
allerede etablerte - ikke en egen, annerledes formet feilrespons.

**`Helpers/CurrentUserContext.cs`**, registrert scoped, bærer resultatet
videre: `Auth0Subject` (satt for enhver autentisert forespørsel) og `StaffId`
(satt når koblingen finnes). Tjenestene kaller
`currentUser.RequireStaffId()` i stedet for å motta `null` - den kaster bare
hvis usikkerheten faktisk er brutt (en feil i sammenkoblingen, ikke noe en
bruker kan utløse), så `!`-operatoren trengs ikke, se CLAUDE.md.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Avvis med `403` + liten selvbetjenings-API** | Ingen manuelt steg utenfor applikasjonen. `link-me` leser `sub` fra brukerens eget token - ingen må dekode en JWT for hånd. | To nye endepunkter og ett unntaksattributt å vedlikeholde. | Valgt |
| La kallet gå gjennom, `CreatedByStaffId` forblir `null` | Ingen ny innstramming - identisk med dagens oppførsel. | Løser ikke det denne endringen faktisk skal løse: at revisjonssporet skal vise *hvem*. | Avvist i samtale |
| Manuell databasesetning, ikke noe Staff-endepunkt | Færrest mulige nye filer. | Krever at noen dekoder sitt eget JWT for å finne `sub`, og går utenom applikasjonens egen validering ved opprettelse. Foreslått og vurdert i samtale, men forkastet - selvbetjenings-API-et er lite nok til at det ikke forsvarer forenklingen. | Avvist i samtale |
| Automatisk provisjonering ved første innlogging | Null manuelle steg, noensinne. | Access-tokenet bærer normalt ikke et ekte navn å bruke, kun `sub` - en auto-opprettet profil ville fått et plassholdernavn. Uklart hvem som "har lov" til å bli ansatt når Auth0 selv allerede er låst til 1-4 manuelt opprettede kontoer. | Avvist i samtale |
| Deklarativ `IAuthorizationRequirement`/policy i stedet for middleware | Mer idiomatisk ASP.NET Core-mønster. | Et policy-avslag går som standard ikke gjennom `ProblemDetailsExceptionHandler` - ville krevd en egen, annerledes formet feilrespons bare for dette ene tilfellet. | Uenig med det som allerede er etablert i ADR-0017 |

## Konsekvenser

**Positivt**

- Revisjonssporet (`CreatedByStaffId`/`UpdatedByStaffId`) er nå reelt for de
  fem kjerne-CRUD-tjenestene, ikke bare et forberedt, tomt felt.
- `link-me` krever ingen manuell JWT-dekoding - `sub` leses av backend-et fra
  brukerens eget token.
- Samme feilformat (`ProblemDetails` + `reason`) som alt annet i API-et.

**Negativt eller risiko**

- `[AllowUnlinkedStaff]` er en ny, egen unntaksmekanisme ved siden av
  `[AllowAnonymous]` - et nytt endepunkt som *skulle* vært unntatt, men ikke
  er markert, blokkerer seg selv med `403` i stedet for å fungere. Verdt å
  huske ved fremtidige endepunkter, se `backend-endpoint`-skillen.
- `RequireLinkedStaffMiddleware` gjør ett ekstra databaseoppslag per
  autentisert, ikke-unntatt forespørsel. Ubetydelig i denne størrelsen, men
  et sted å se hen til om ytelse noensinne blir et tema.
- Rollebasert autorisasjon (`User`/`Staff`/`Admin`) er fortsatt ikke bygget -
  denne endringen legger til «autentisert og koblet», ikke «har riktig
  rolle». De to må ikke forveksles, se `06-autentisering.md`.

## Erfaring fra implementeringen

Alle 141 eksisterende integrasjonstester autentiserer via
`AuthenticatedWebApplicationFactory`/`TestAuthHandler` med en fast
testidentitet. For at de skulle fortsette å bestå uendret med den nye
sperren på, fikk `TestAuthHandler` (som allerede konstrueres via DI per
forespørsel) i oppgave å selv opprette og koble en `Staff`-rad for sitt faste
testsubjekt før hver autentisering.

Dette avdekket en reell feil: når to parallelle testklasser forsøkte å
opprette samme rad samtidig, feilet den ene som forventet på den unike
indeksen (fanget med `try/catch`), men den mislykkede `Staff`-entiteten ble
liggende sporet som «Added» i den samme `DbContext`-instansen - som er
*akkurat den samme instansen* resten av forespørselen (samme scope) bruker
videre. Neste, helt urelaterte lagring i samme forespørsel (for eksempel å
opprette en `Guardian`) forsøkte da å sette inn den forkastede
`Staff`-raden på nytt, og feilet på samme konflikt. Rettet ved å eksplisitt
`Detach`-e entiteten i `catch`-blokken. Se `TestSupport/TestAuthHandler.cs`.
