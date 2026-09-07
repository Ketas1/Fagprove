# Backend-API

> **Status:** kjerneflyten - kategorier, foresatte, barn, utstyr og utlån - er
> bygget, og det samme er hele oppfølgingsarbeidsflyten: notater, utestengelse,
> kontaktforsøk, bekreftet tap/skade og rapportene. Se "Ikke bygget i denne
> omgangen" nederst for det som gjenstår - i hovedsak bilde ved utlån/retur og
> rollebasert autorisasjon.

## Konvensjoner

- Base-URL: `/api`
- Substantiv i flertall for ressurser, verb for tilstandsoverganger:
  `POST /api/loans/{id}/return`
- Alle svar er JSON. Feltnavn i `camelCase`.
- Tidspunkter i ISO 8601 med UTC-tidssone.
- Entiteter eksponeres aldri direkte. Alt går via DTO-er.
- Alle endepunkter krever autentisering med mindre noe annet er dokumentert,
  og de fleste krever i tillegg en `Staff`-profil koblet til kontoen - se
  `06-autentisering.md`. De tre unntakene (helsesjekken og de to
  Staff-bootstrap-endepunktene) er markert "Enhver innlogget bruker" i
  tabellene under, i stedet for "Staff".

## Feilhåndtering

Feil returneres som RFC 7807 `ProblemDetails`, slik at frontend kan skille
mellom feiltyper maskinelt.

```json
{
  "type": "https://sportforalle.no/errors/loan-blocked",
  "title": "Utlån blokkert",
  "status": 409,
  "detail": "Låntakeren har et åpent forfalt lån.",
  "reason": "BorrowerHasOverdueLoan"
}
```

| Kode | Brukes når |
| --- | --- |
| `400` | Ugyldig forespørsel, feil i modellvalidering |
| `401` | Mangler eller ugyldig token |
| `403` | Autentisert, men ikke koblet til en `Staff`-profil (`reason: StaffNotLinked`, se `06-autentisering.md`) eller - når det bygges - mangler rollen |
| `404` | Ressursen finnes ikke |
| `409` | Regelbrudd - blokkert utlån, utstyr ikke ledig, låntaker utestengt |
| `422` | Forespørselen er syntaktisk riktig, men semantisk umulig |

Et blokkert utlån er `409` med maskinlesbar `reason`, ikke en generisk `400`.
Frontend bruker `reason` til å vise riktig forklaring til den ansatte.

Oversettelsen fra unntak til `ProblemDetails` skjer på ett sted:
`Middleware/ProblemDetailsExceptionHandler.cs`, se
[ADR-0017](./adr/0017-global-exception-handler.md). Services kaster
`Validation.NotFoundException` (→ `404`) eller `Validation.DomainConflictException`
(→ `409` som standard, eller en annen status angitt eksplisitt - `422` for
`BorrowerOutsideAgeRange`, se under) i stedet for å bygge et feilsvar selv.

### Kjente `reason`-verdier

| `reason` | Status | Utløses av |
| --- | --- | --- |
| `BorrowerOutsideAgeRange` | `422` | `POST /api/borrowers` - fødselsdato gir en alder utenfor 3-18 år |
| `BorrowerBanned` | `409` | `POST /api/loans` - låntakeren er utestengt |
| `BorrowerHasOverdueLoan` | `409` | `POST /api/loans` - låntakeren har et åpent forfalt lån |
| `EquipmentNotAvailable` | `409` | `POST /api/loans` - utstyret er ikke `Available` |
| `LoanAlreadyClosed` | `409` | `POST /api/loans/{id}/return`, `POST /api/loans/{id}/mark-lost` - lånet er allerede `Returned` eller `Lost` |
| `DuplicateCategoryName` | `409` | `POST /api/equipment-categories` - navnet er allerede i bruk |
| `DuplicateSerialNumber` | `409` | `POST /api/equipment` - serienummeret er allerede i bruk |
| `EquipmentHasLoanHistory` | `409` | `DELETE /api/equipment/{id}` - utstyret har vært del av et utlån |
| `StaffNotLinked` | `403` | Ethvert endepunkt uten `[AllowUnlinkedStaff]` - brukeren er autentisert, men ikke koblet til en `Staff`-profil, se `06-autentisering.md` |
| `StaffAlreadyLinked` | `409` | `POST /api/staff/{id}/link-me` - profilen er allerede koblet til en Auth0-konto |
| `Auth0AccountAlreadyLinked` | `409` | `POST /api/staff/{id}/link-me` - denne Auth0-kontoen er allerede koblet til en annen profil |
| `BorrowerAlreadyBanned` | `409` | `POST /api/borrowers/{id}/ban` - låntakeren er allerede utestengt |
| `BorrowerNotBanned` | `409` | `POST /api/borrowers/{id}/ban/fee-paid`, `DELETE /api/borrowers/{id}/ban` - låntakeren er ikke utestengt |
| `BanFeeNotPaid` | `409` | `DELETE /api/borrowers/{id}/ban` - gebyret er ikke registrert betalt ennå, se forretningsregel 8 |
| `LoanNotOverdue` | `409` | `POST /api/loans/{id}/send-followup-email` - lånet er ikke forfalt |
| `EmailSendFailed` | `502` | `POST /api/loans/{id}/send-followup-email` - EmailJS avviste kallet eller kunne ikke nås, se [ADR-0022](./adr/0022-emailjs-server-side.md) |

## Endepunkter

**Rolle-kolonnen i tabellene under er planlagt, ikke håndhevet ennå** - se
`06-autentisering.md`. Alle endepunkter krever i dag kun at brukeren er
innlogget; det finnes ingen rollesjekk.

### Drift

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/health` | Enhver innlogget bruker | Rapporterer om API-et kjører og om databasen er tilgjengelig. Krever autentisering, men ikke en koblet `Staff`-profil (`[AllowUnlinkedStaff]`) - en operasjonell sjekk, ikke domenedata. |

### Ansatte

Bygget 2026-09-04 sammen med koblingen mellom `Staff` og Auth0, se
[ADR-0019](./adr/0019-staff-auth0-mapping.md) og `06-autentisering.md`. Alle
fire er `[AllowUnlinkedStaff]` - de finnes nettopp for å la en ukoblet bruker
bli koblet.

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/staff` | Enhver innlogget bruker | Liste, slik en ukoblet bruker kan se om profilen sin allerede finnes |
| `POST` | `/api/staff` | Enhver innlogget bruker | Registrer en ny profil (kun navn) |
| `POST` | `/api/staff/{id}/link-me` | Enhver innlogget bruker | Kobler den innloggede brukerens eget Auth0-`sub` til profilen. Ingen forespørselskropp. `404` hvis profilen ikke finnes, `409 StaffAlreadyLinked` hvis den allerede er koblet, `409 Auth0AccountAlreadyLinked` hvis denne kontoen allerede er koblet et annet sted |
| `GET` | `/api/staff/me` | Enhver innlogget bruker | Den innloggede brukerens egen profil, funnet via Auth0-`sub` (ikke en id i ruten). `404` hvis kontoen ikke er koblet ennå - det er den vanlige tilstanden for noen som ikke har fullført oppstartsflyten. Bygget 2026-09-07 slik at frontend kan spørre "er dette meg?" uten å hente hele listen, som eksponerer alle profilers `auth0UserId` |

### Utstyrskategorier

Lagt til utenfor det opprinnelige forslaget: `Equipment.CategoryId` er
påkrevd, og uten et endepunkt for kategorier ville utstyr aldri kunne
registreres. Ingen `PUT`/`DELETE` ennå - ikke etterspurt for denne omgangen.

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/equipment-categories` | Staff | Liste |
| `POST` | `/api/equipment-categories` | Staff | Registrer kategori. `409 DuplicateCategoryName` ved duplikat navn |

### Utstyr

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/equipment?status=&categoryId=` | Staff | Liste, med valgfritt filter på status og kategori |
| `GET` | `/api/equipment/{id}` | Staff | Detaljer. `404` hvis utstyret ikke finnes |
| `POST` | `/api/equipment` | Staff | Registrer nytt utstyr. `404` hvis kategorien ikke finnes, `409 DuplicateSerialNumber` ved duplikat serienummer |
| `PUT` | `/api/equipment/{id}` | Staff | Endre navn og kategori |
| `DELETE` | `/api/equipment/{id}` | Admin | Fjern. `409 EquipmentHasLoanHistory` hvis utstyret har vært del av et utlån - historikken må bestå, se `04-databasedesign.md` |

### Foresatte og barn

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/guardians` | Staff | Liste over foresatte |
| `GET` | `/api/guardians/{id}` | Staff | Detaljer. `404` hvis foresatt ikke finnes |
| `POST` | `/api/guardians` | Staff | Registrer foresatt |
| `PUT` | `/api/guardians/{id}` | Staff | Oppdater navn, e-post og telefon |
| `GET` | `/api/borrowers` | Staff | Liste over barn |
| `GET` | `/api/borrowers/{id}` | Staff | Detaljer. `404` hvis barnet ikke finnes |
| `POST` | `/api/borrowers` | Staff | Registrer barn. Krever enten `guardianId` (kobler en eksisterende foresatt - for eksempel et søsken) eller `newGuardian` (oppretter en foresatt i samme kall), aldri begge eller ingen (`400`). `422 BorrowerOutsideAgeRange` hvis fødselsdatoen gir en alder utenfor 3-18 år |
| `PUT` | `/api/borrowers/{id}` | Staff | Endre navn. Fødselsdato og foresatt kan ikke endres etter registrering |
| `GET` | `/api/borrowers/{id}/ban` | Staff | Låntakerens aktive utestengelse. `404` både hvis låntakeren ikke finnes og hvis de ikke er utestengt - det finnes ingen "aktiv utestengelse"-ressurs i noen av tilfellene |
| `POST` | `/api/borrowers/{id}/ban` | Staff | Utesteng låntakeren, med årsak. `409 BorrowerAlreadyBanned` hvis allerede utestengt |
| `POST` | `/api/borrowers/{id}/ban/fee-paid` | Staff | Registrer at gebyret er betalt i butikken. `409 BorrowerNotBanned` hvis låntakeren ikke er utestengt |
| `DELETE` | `/api/borrowers/{id}/ban` | Staff | Opphev utestengelsen. `409 BorrowerNotBanned` hvis ikke utestengt, `409 BanFeeNotPaid` hvis gebyret ikke er registrert betalt ennå - se forretningsregel 8 |
| `POST` | `/api/borrowers/{id}/notes` | Staff | Legg til et fritekstnotat om låntakeren |
| `GET` | `/api/borrowers/{id}/notes` | Staff | Liste over notater, nyeste først |

Utestengelseshistorikk (tidligere, opphevede utestengelser) er ikke
eksponert som egen liste ennå - `Borrower.Status` viser om låntakeren er
utestengt *nå*, som er det en fremtidig låntaker-detaljside trenger for å vise
riktig knapp. Full historikk venter til den siden bygges.

### Utlån

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/loans?status=` | Staff | Liste, med valgfritt filter på status (`Active`, `Overdue`, `Returned`, `Lost`) |
| `GET` | `/api/loans/{id}` | Staff | Detaljer. `404` hvis lånet ikke finnes |
| `POST` | `/api/loans` | Staff | Registrer utlån. `404` hvis låntaker eller utstyr ikke finnes. `409 BorrowerBanned`, `409 BorrowerHasOverdueLoan` eller `409 EquipmentNotAvailable` ved regelbrudd |
| `POST` | `/api/loans/{id}/return` | Staff | Registrer retur, med utstyrets tilstand (`condition`). `409 LoanAlreadyClosed` hvis lånet allerede er avsluttet. **Krever ikke bilde ennå** - se forretningsregel 5 i `03-domenemodell.md` |
| `POST` | `/api/loans/{id}/mark-lost` | Staff | Registrer bekreftet tap eller skade. Setter lånet til `Lost` og utstyret til `WrittenOff`. `409 LoanAlreadyClosed` hvis lånet allerede er avsluttet |
| `POST` | `/api/loans/{id}/contact-attempts` | Staff | Logg et kontaktforsøk overfor foresatt, med metode og resultat - se forretningsregel 6 |
| `GET` | `/api/loans/{id}/contact-attempts` | Staff | Liste over kontaktforsøk for lånet, nyeste først, slik at ansatte ser om foresatt allerede er kontaktet |
| `POST` | `/api/loans/{id}/send-followup-email` | Staff | Send en oppfølgings-e-post til foresatt via EmailJS og logg den som et kontaktforsøk (`Method: Email`) i samme handling - se [ADR-0022](./adr/0022-emailjs-server-side.md). `409 LoanNotOverdue` hvis lånet ikke er forfalt. `502 EmailSendFailed` hvis EmailJS avviser kallet - da logges intet kontaktforsøk, siden e-posten ikke faktisk ble sendt |

Den automatiske forfallsdeteksjonen (forretningsregel 7) er beskrevet i
[ADR-0011](./adr/0011-automatisk-forfall.md): `Loan.Status` beregnes korrekt
ved hver lesing uansett, og materialiseres i tillegg til databasen av en
bakgrunnsjobb (`OverdueLoanBackgroundService`) som kjører med jevne
mellomrom (`OverdueCheck:IntervalSeconds`, standard 60 sekunder - valgt for at
en overgang skal være synlig raskt ved uttesting, ikke justert for
produksjonslast). Jobben har ikke noe eget endepunkt; den kjører i bakgrunnen
av seg selv.

### Rapporter

Alle rapportendepunktene returnerer kun aggregerte tall, aldri enkeltlån eller
-låntakere, se `09-lover-og-regler.md`.

De **to** rapportene (se `03-domenemodell.md`) deler samme `LoanFigures`-objekt
med fem tall. Alle fem teller lån med `StartedAt` i perioden, så de fire
underradene summerer seg alltid til `totalLoans`:

```jsonc
{
  "totalLoans": 412,      // registrert i perioden
  "returnedOnTime": 305,  // Status = Returned, DaysLate = 0
  "returnedLate": 37,     // Status = Returned, DaysLate > 0
  "notReturned": 6,       // Status = Overdue eller Lost
  "stillActive": 64       // Status = Active
}
```

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/reports/loans?from=&to=` | Staff | **Rapport 1 av 2.** De fem tallene for perioden |
| `GET` | `/api/reports/age-groups?from=&to=` | Staff | **Rapport 2 av 2.** De samme fem tallene per aldersgruppe (3-7, 8-12, 13-18). Alder regnes ved `Loan.StartedAt`, ikke ved rapporttidspunktet |
| `GET` | `/api/reports/timeline?from=&to=&interval=` | Staff | Antall utlån per `Day`, `Week` eller `Month` - grunnlaget for søylediagrammet. `interval` er `Month` hvis den utelates |
| `GET` | `/api/reports/popular-equipment` | Staff | Antall utlån per utstyr, hele historikken, sortert synkende. **Ikke i bruk** av noe grensesnitt, se `03-domenemodell.md` |

**`from` og `to` er valgfrie** på alle tre periodeendepunktene (`yyyy-MM-dd`).
Utelates de, dekker svaret hele historikken, og `from`/`to` i responsen er
`null`. Det er slik «Hele historikken» på rapportsiden slipper å finne på en
startdato. Oppgis begge, kan `to` ikke være før `from` - det gir `400`.

`timeline` fyller inn tomme bøtter med `count: 0` i stedet for å hoppe over
dem, slik at en stille måned vises som et hull i diagrammet framfor å
forsvinne. En uke starter på mandag. Et intervall som ville gitt mer enn **400
bøtter** (for eksempel `Day` over hele historikken) avvises med `400` framfor å
returnere et svar ingen kan tegne; rapportsiden fanger den feilen og ber den
ansatte velge en grovere oppdeling.

> **Fjernet 2026-09-07:** `GET /api/reports/overdue-summary`. De to tallene den
> ga finnes nå som `returnedLate` og `notReturned` på rapport 1, forankret i
> perioden. Den gamle varianten talte over hele historikken uten datofilter, så
> å beholde begge ville gitt to endepunkter som svarer forskjellig på samme
> spørsmål. En integrasjonstest sjekker at ruten faktisk gir `404`.

## Ikke bygget i denne omgangen

CRUD-laget bygget 2026-09-04 dekker kjerneflyten, og oppfølgingsarbeidsflyten
(notater, utestengelse, kontaktforsøk, bekreftet tap/skade, automatisk
forfall og rapportene) ble lagt til 2026-09-07. Det som gjenstår, og hvorfor:

| Del | Hvorfor ikke nå |
| --- | --- |
| Bilde ved utlån og retur | Lagringsløsning for bilder er ikke valgt, se `04-databasedesign.md` |
| Rollebasert autorisasjon | Se `06-autentisering.md` - ingen rolle-claim finnes i tokenet ennå |
| Utestengelseshistorikk som egen liste | `Borrower.Status` dekker det en fremtidig låntaker-detaljside trenger i første omgang (er låntakeren utestengt *nå*); historikken (tidligere, opphevede utestengelser) venter til den siden faktisk bygges |

## API-dokumentasjon

> Dokumenter hvordan OpenAPI-spesifikasjonen genereres og hvor den er
> tilgjengelig under kjøring.

## Testing av API-et

Hvert endepunkt bygget i denne omgangen har en integrasjonstest i
`SportForAlle.Tests/Controllers/`, som starter hele applikasjonen i minnet
(`WebApplicationFactory<Program>`) mot den ekte Docker-databasen og kaller
endepunktet med ekte HTTP-forespørsler. `AuthenticatedWebApplicationFactory`
bytter ut Auth0-skjemaet med et som alltid lykkes, se `06-autentisering.md`.
Delt testdata (kategori, foresatt, barn, utstyr) bygges via
`TestSupport/ApiTestDataBuilder.cs` i stedet for å gjentas i hver testfil.

Forretningsreglene selv - blokkert utlån, alder utenfor 3-18 år, utstyr som
ikke er ledig - er enhetstestet direkte mot `SportForAlle.Api/Services/Rules/`
uten database, se [`07-testing.md`](./07-testing.md).

To reelle feil ble funnet og rettet mens dette laget ble bygget, se
"Erfaring fra implementeringen" i
[ADR-0017](./adr/0017-global-exception-handler.md). En tredje, i
`TestAuthHandler` selv, ble funnet og rettet da Staff↔Auth0-koblingen ble
lagt til - se [ADR-0019](./adr/0019-staff-auth0-mapping.md).

Rapportendepunktene spør mot hele `Loans`-tabellen, som er delt mellom alle
tester og ikke isolert per test (se over) - og xUnit kjører forskjellige
testklasser parallelt som standard. `Controllers/ReportsEndpointTests.cs`
sammenligner derfor et rapportresultat før og etter at testen selv legger til
én kjent rad, og forventer *minst* én økning i tallet, ikke nøyaktig én - en
annen testklasse som registrerer et lån i samme sekund er forventet, ikke en
feil. Der et resultat ikke kan påvirkes av andre tester i det hele tatt (en
fersk, unik utstyrs-id, eller et datointervall langt utenfor "i dag") sjekkes
den eksakte verdien i stedet.

Den automatiske forfallsjobben (`OverdueLoanBackgroundService`) testes ikke
ved å vente på at den faktisk kjører - det ville gjort testene trege og
tidsavhengige. I stedet kaller `Controllers/OverdueLoanRefreshTests.cs` samme
metode jobben selv bruker (`LoanService.RefreshOverdueLoansAsync`) direkte,
med en klokke flyttet forbi fristen, se `07-testing.md`.
