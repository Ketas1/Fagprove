# Databasedesign

> **Status:** ikke ferdigstilt. Fylles ut sammen med implementeringen av
> skjemaet. Strukturen under viser hva dokumentet skal dekke, og inneholder et
> utgangspunkt utledet fra [`03-domenemodell.md`](./03-domenemodell.md).

## Valg av database

PostgreSQL, se [ADR-0004](./adr/0004-postgresql.md).

## Tilnærming: Code First

Skjemaet defineres som C#-klasser i `SportForAlle.Api/Models/`, med
tabellkonfigurasjon i `SportForAlle.Api/Data/Configurations/`, og genereres til
tabeller med EF Core-migrasjoner. Databasen endres aldri manuelt - alle endringer
går gjennom en migrasjon som ligger i versjonskontroll sammen med koden.

Migrasjonene ligger i `SportForAlle.Api/Data/Migrations/`.

Se [ADR-0005](./adr/0005-ef-core-code-first.md).

## Tabeller

> Fyll ut med endelig skjema. Utgangspunktet under er utledet fra domenemodellen.

| Tabell | Innhold |
| --- | --- |
| `Guardians` | Foresatte, med kontaktinformasjon |
| `Borrowers` | Barn, med fødselsdato, kobling til foresatt, `LateReturnCount`, `IsUnreliable` |
| `EquipmentCategories` | Kategorier for gruppering og rapportering |
| `Equipment` | Utstyr, med serienummer, kategori, tilstand og status |
| `Loans` | Utlån, med start, forfallsdato, retur, status og `DaysLate` |
| `LoanPhotos` | Bilder før utlån og ved retur |
| `ContactAttempts` | Kontaktforsøk mot foresatt, med dato, metode og resultat |
| `Notes` | Notater om låntakere |
| `Bans` | Utestengelser, med årsak, start, slutt og gebyrbetaling |
| `Staff` | Ansatte, koblet til Auth0-bruker |

## ER-diagram

> Sett inn Mermaid-diagram over det ferdige skjemaet, med kolonner og nøkler.

## Nøkler og relasjoner

> Dokumenter for hver relasjon: kardinalitet, om fremmednøkkelen er påkrevd, og
> hva som skjer ved sletting (`Restrict`, `Cascade`, `SetNull`) - og hvorfor.
>
> Særlig å avklare:
> - `Borrower.GuardianId` er påkrevd. Et barn kan ikke eksistere uten foresatt.
> - Sletting av en låntaker med historikk må ikke slette utlånshistorikken som
>   rapportene bygger på. Vurder anonymisering i stedet for sletting, se
>   [`09-lover-og-regler.md`](./09-lover-og-regler.md).

## Indekser

> Dokumenter indekser og hvorfor de trengs. Kandidater:
> - `Loans (Status, DueDate)` - oversikten over forfalte lån er systemets mest
>   brukte spørring.
> - `Loans (BorrowerId, Status)` - sjekken for åpent forfalt lån gjøres ved hvert
>   nye utlån.
> - `Equipment (SerialNumber)` unik.

## Datatyper og konvensjoner

> Dokumenter valgene, blant annet:
> - Primærnøkler: `int` eller `Guid`, og hvorfor.
> - Tidspunkter lagres som `timestamptz` (UTC). Konvertering til norsk tid skjer
>   i grensesnittet.
> - Enums lagres som tekst, ikke tall, slik at databasen er lesbar og en ny verdi
>   ikke forskyver betydningen av eksisterende rader.
> - Navnekonvensjon for tabeller og kolonner.

## Migrasjoner

> Dokumenter migrasjonsrutinen og list opp migrasjonene etter hvert som de lages.

```bash
dotnet ef migrations add <Navn> \
  --project SportForAlle.Api \
  --startup-project SportForAlle.Api \
  --output-dir Data/Migrations

dotnet ef database update \
  --project SportForAlle.Api \
  --startup-project SportForAlle.Api
```

| Migrasjon | Dato | Endring |
| --- | --- | --- |
| `InitialCreate` | 2026-09-02 | Oppretter `EquipmentCategories` med `Id` (identity) og `Name` (`varchar(100)`, unik). Første migrasjon, laget for å verifisere at EF Core, migrasjoner og Docker-databasen henger sammen. |

## Testdata

> Beskriv hvordan databasen fylles med realistiske testdata for utvikling og
> demonstrasjon, og hvordan det holdes atskilt fra ekte data.
