# Backend-API

> **Status:** kjerneflyten - kategorier, foresatte, barn, utstyr og utlån - er
> bygget og dokumentert under. Notater, utestengelse, kontaktforsøk, bekreftet
> tap/skade og rapporter er ikke bygget ennå, se "Ikke bygget i denne omgangen"
> nederst.

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
| `LoanAlreadyClosed` | `409` | `POST /api/loans/{id}/return` - lånet er allerede `Returned` eller `Lost` |
| `DuplicateCategoryName` | `409` | `POST /api/equipment-categories` - navnet er allerede i bruk |
| `DuplicateSerialNumber` | `409` | `POST /api/equipment` - serienummeret er allerede i bruk |
| `EquipmentHasLoanHistory` | `409` | `DELETE /api/equipment/{id}` - utstyret har vært del av et utlån |
| `StaffNotLinked` | `403` | Ethvert endepunkt uten `[AllowUnlinkedStaff]` - brukeren er autentisert, men ikke koblet til en `Staff`-profil, se `06-autentisering.md` |
| `StaffAlreadyLinked` | `409` | `POST /api/staff/{id}/link-me` - profilen er allerede koblet til en Auth0-konto |
| `Auth0AccountAlreadyLinked` | `409` | `POST /api/staff/{id}/link-me` - denne Auth0-kontoen er allerede koblet til en annen profil |

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
tre er `[AllowUnlinkedStaff]` - de finnes nettopp for å la en ukoblet bruker
bli koblet.

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/staff` | Enhver innlogget bruker | Liste, slik en ukoblet bruker kan se om profilen sin allerede finnes |
| `POST` | `/api/staff` | Enhver innlogget bruker | Registrer en ny profil (kun navn) |
| `POST` | `/api/staff/{id}/link-me` | Enhver innlogget bruker | Kobler den innloggede brukerens eget Auth0-`sub` til profilen. Ingen forespørselskropp. `404` hvis profilen ikke finnes, `409 StaffAlreadyLinked` hvis den allerede er koblet, `409 Auth0AccountAlreadyLinked` hvis denne kontoen allerede er koblet et annet sted |

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

`POST /api/borrowers/{id}/notes`, `POST /api/borrowers/{id}/ban` og
`DELETE /api/borrowers/{id}/ban` er ikke bygget ennå, se "Ikke bygget i denne
omgangen" nederst.

### Utlån

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/loans?status=` | Staff | Liste, med valgfritt filter på status (`Active`, `Overdue`, `Returned`, `Lost`) |
| `GET` | `/api/loans/{id}` | Staff | Detaljer. `404` hvis lånet ikke finnes |
| `POST` | `/api/loans` | Staff | Registrer utlån. `404` hvis låntaker eller utstyr ikke finnes. `409 BorrowerBanned`, `409 BorrowerHasOverdueLoan` eller `409 EquipmentNotAvailable` ved regelbrudd |
| `POST` | `/api/loans/{id}/return` | Staff | Registrer retur, med utstyrets tilstand (`condition`). `409 LoanAlreadyClosed` hvis lånet allerede er avsluttet. **Krever ikke bilde ennå** - se forretningsregel 5 i `03-domenemodell.md` |

`POST /api/loans/{id}/contact-attempts` og `POST /api/loans/{id}/mark-lost` er
ikke bygget ennå.

### Rapporter

Ikke bygget ennå.

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/reports/loans?from=&to=` | Staff | Antall utlån i perioden |
| `GET` | `/api/reports/age-groups?from=&to=` | Staff | Fordeling på aldersgruppe |
| `GET` | `/api/reports/popular-equipment` | Staff | Mest utlånte utstyr |
| `GET` | `/api/reports/overdue-summary` | Staff | Forsene og uleverte leveringer |

Rapportendepunktene skal returnere kun aggregerte tall, se
`09-lover-og-regler.md`.

## Ikke bygget i denne omgangen

CRUD-laget bygget 2026-09-04 dekker kjerneflyten - kategori, utstyr, foresatt,
barn og utlån (registrering og retur). Bevisst utelatt, og hvorfor:

| Del | Hvorfor ikke nå |
| --- | --- |
| Notater (`Note`) | Ikke etterspurt for kjerneflyten; bygges sammen med oppfølgingsarbeidsflyten |
| Utestengelse (`Ban`) | Samme - hører til oppfølging av forfalte lån, ikke registreringsflyten |
| Kontaktforsøk (`ContactAttempt`) | Samme |
| Bekreftet tap/skade (`mark-lost`) | Samme |
| Bilde ved utlån og retur | Lagringsløsning for bilder er ikke valgt, se `04-databasedesign.md` |
| Rapporter | Egen senere leveranse |
| Rollebasert autorisasjon | Se `06-autentisering.md` - ingen rolle-claim finnes i tokenet ennå |

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
