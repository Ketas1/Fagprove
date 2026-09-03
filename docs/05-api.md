# Backend-API

> **Status:** ikke ferdigstilt. Fylles ut sammen med implementeringen av
> endepunktene. Strukturen og konvensjonene under er bestemt på forhånd, slik at
> endepunktene blir konsistente fra første implementasjon.

## Konvensjoner

- Base-URL: `/api`
- Substantiv i flertall for ressurser, verb for tilstandsoverganger:
  `POST /api/loans/{id}/return`
- Alle svar er JSON. Feltnavn i `camelCase`.
- Tidspunkter i ISO 8601 med UTC-tidssone.
- Entiteter eksponeres aldri direkte. Alt går via DTO-er.
- Alle endepunkter krever autentisering med mindre noe annet er dokumentert.

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
| `403` | Autentisert, men mangler rollen |
| `404` | Ressursen finnes ikke |
| `409` | Regelbrudd - blokkert utlån, utstyr ikke ledig, låntaker utestengt |
| `422` | Forespørselen er syntaktisk riktig, men semantisk umulig |

Et blokkert utlån er `409` med maskinlesbar `reason`, ikke en generisk `400`.
Frontend bruker `reason` til å vise riktig forklaring til den ansatte.

## Endepunkter

> Fyll ut etter hvert som de implementeres. Foreslått inndeling under.
> **Rolle-kolonnen i tabellene er planlagt, ikke håndhevet ennå** - se
> `06-autentisering.md`. Alle endepunkter krever i dag kun at brukeren er
> innlogget; det finnes ingen rollesjekk.

### Drift

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/health` | Enhver innlogget bruker | Rapporterer om API-et kjører og om databasen er tilgjengelig. Krever autentisering, som alle andre endepunkter. |

### Utstyr

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/equipment` | Staff | Liste, med filter på status og kategori |
| `GET` | `/api/equipment/{id}` | Staff | Detaljer |
| `POST` | `/api/equipment` | Staff | Registrer nytt utstyr |
| `PUT` | `/api/equipment/{id}` | Staff | Oppdater |
| `DELETE` | `/api/equipment/{id}` | Admin | Fjern |

### Brukere

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/guardians` | Staff | Liste over foresatte |
| `POST` | `/api/guardians` | Staff | Registrer foresatt |
| `GET` | `/api/borrowers` | Staff | Liste over barn |
| `POST` | `/api/borrowers` | Staff | Registrer barn, koblet til foresatt |
| `GET` | `/api/borrowers/{id}` | Staff | Detaljer med historikk og notater |
| `POST` | `/api/borrowers/{id}/notes` | Staff | Skriv notat |
| `POST` | `/api/borrowers/{id}/ban` | Staff | Utesteng |
| `DELETE` | `/api/borrowers/{id}/ban` | Staff | Opphev utestengelse, med gebyr registrert |

### Utlån

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/loans` | Staff | Liste, med filter `?status=active\|overdue\|returned` |
| `POST` | `/api/loans` | Staff | Registrer utlån. `409` ved regelbrudd |
| `GET` | `/api/loans/{id}` | Staff | Detaljer |
| `POST` | `/api/loans/{id}/return` | Staff | Registrer retur, med tilstand og bilde |
| `POST` | `/api/loans/{id}/contact-attempts` | Staff | Logg kontaktforsøk |
| `POST` | `/api/loans/{id}/mark-lost` | Staff | Bekreft tap eller skade |

### Rapporter

| Metode | Rute | Rolle | Beskrivelse |
| --- | --- | --- | --- |
| `GET` | `/api/reports/loans?from=&to=` | Staff | Antall utlån i perioden |
| `GET` | `/api/reports/age-groups?from=&to=` | Staff | Fordeling på aldersgruppe |
| `GET` | `/api/reports/popular-equipment` | Staff | Mest utlånte utstyr |
| `GET` | `/api/reports/overdue-summary` | Staff | Forsene og uleverte leveringer |

Rapportendepunktene returnerer kun aggregerte tall.

## API-dokumentasjon

> Dokumenter hvordan OpenAPI-spesifikasjonen genereres og hvor den er
> tilgjengelig under kjøring.

## Testing av API-et

> Beskriv integrasjonstestene og hvordan de kjøres mot en ekte database.
> Se [`07-testing.md`](./07-testing.md).
