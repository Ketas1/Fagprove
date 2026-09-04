# ADR-0017: Global exception-handler for RFC 7807-feilsvar

- **Status:** Akseptert
- **Dato:** 2026-09-04

## Kontekst

`05-api.md` har siden API-konvensjonene ble skrevet lovet RFC 7807
`ProblemDetails` med en maskinlesbar `reason` for regelbrudd som et blokkert
utlån - lenge før noe reelt endepunkt fantes. Med de første ekte endepunktene
(utstyrskategorier, foresatte, barn, utstyr, utlån) måtte noe faktisk
produsere den formen konsekvent, uten at hver av de rundt femten
endepunktmetodene i denne endringen bygger sitt eget feilsvar.

Entitetene kaster allerede ren `ArgumentException` fra sin egen validering
(se ADR-0012). Services trenger i tillegg en måte å signalisere et
forretningsregelbrudd på - blokkert utlån, utstyr ikke ledig, duplikat
serienummer - som bærer både en HTTP-status og en `reason` frontend kan lese
maskinelt.

## Beslutning

Én `IExceptionHandler` (`Middleware/ProblemDetailsExceptionHandler`),
registrert via `AddExceptionHandler<T>()`/`AddProblemDetails()` og koblet inn
tidlig i pipelinen, fanger unntak kastet av services og oversetter dem til
`ProblemDetails`. To nye unntakstyper i `Validation/` bærer det handleren
trenger:

- `NotFoundException` → `404`.
- `DomainConflictException` → `409` som standard, men overstyrbar per
  instans (`422` for `BorrowerOutsideAgeRange`, se `05-api.md`), og bærer en
  `Reason`-streng som legges på som `reason`-feltet i responsen.

Alt annet handleren kjenner igjen - `ArgumentException` (forsvar mot
validering kastet av en entitet) og `DbUpdateException` (en reell
databasebegrensning ble truffet til tross for en sjekk i applikasjonslaget,
for eksempel et kappløp mot en unik indeks) - får en generisk, norsk
`ProblemDetails`-tekst som aldri gjengir den underliggende (engelske)
unntaksmeldingen til klienten. Alt annet faller gjennom til ASP.NET Core sin
standardhåndtering.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Én global `IExceptionHandler` + to nye unntakstyper** | Services signaliserer en regel ved å kaste, ikke ved å bygge et `ProblemDetails`-objekt selv. Ett sted eier oversettelsen til HTTP-status og `reason`. | Et unntak som ikke er en av de kjente typene faller gjennom til standard 500-håndtering - riktig oppførsel, men krever at utvikleren kjenner skillet. | Valgt |
| Try/catch og `ProblemDetails` i hver controller-metode | Eksplisitt, ingen skjult oversettelse. | Kode og feilformat gjentas i hver av de ~15 endepunktene i denne endringen alene, og vokser med hvert nytt endepunkt. | For mye gjentagelse for en kort utviklingsperiode |
| Et `Result<T>`-mønster i stedet for unntak | Ingen skjult kontrollflyt; feil er en del av signaturen. | Krever at hver service-metode og hver kallende controller håndterer et resultatobjekt eksplisitt - en videre-gående stilendring enn det som allerede er lagt i ADR-0012, der entitetene allerede kaster `ArgumentException`. | Uenig med et mønster som allerede er etablert |

## Konsekvenser

**Positivt**

- Alle fem ressursene i denne endringen (kategori, foresatt, låntaker,
  utstyr, utlån) bruker nøyaktig samme feilformat, uten at det er skrevet
  fem ganger.
- `reason` er tilgjengelig konsekvent på tvers av alle blokkerte handlinger,
  som er akkurat det `05-api.md` og `backend-endpoint`-skillen allerede
  lovet frontend.
- Underliggende (engelske) valideringsmeldinger fra entitetene lekker aldri
  til klienten, se CLAUDE.md-regelen om norsk i grensesnittet.

**Negativt eller risiko**

- En feil `catch`-gren her feiler stille for alle endepunkter samtidig, ikke
  bare ett. Under selve implementeringen skjedde nettopp dette - se
  "Erfaring fra implementeringen" under.
- Krever at utviklere vet at et unntak må være en av de gjenkjente typene
  for å bli oversatt riktig. Et nytt unntak trenger enten en ny gren her
  eller havner som `500`.

## Erfaring fra implementeringen

Handleren avdekket, mens den ble bygget, to reelle feil den ellers ville
skjult:

1. `CreatedAtAction(nameof(GetByIdAsync), ...)` feilet med
   `InvalidOperationException: No route matches the supplied values`, fordi
   ASP.NET Core som standard fjerner "Async"-endelsen fra handlingsnavn ved
   ruting - den registrerte ruten heter `GetById`, ikke `GetByIdAsync`.
   Rettet ved å sette `SuppressAsyncSuffixInActionNames = false` i
   `Program.cs`, slik at handlingsnavnet er identisk med metodenavnet i
   koden, konsistent med `Async`-konvensjonen i CLAUDE.md.
2. En spørring som filtrerte på et felt i en allerede projisert post
   (`.Where(row => row.Equipment.Id == id)` etter
   `select new EquipmentWithCategoryName(...)`) kunne ikke oversettes av
   EF Core (`InvalidOperationException`). Rettet ved å legge filteret inn i
   selve LINQ-spørringen, før projeksjonen, i stedet for å kjede et
   `.Where` på resultatet - se `Services/BorrowerService.cs`,
   `EquipmentService.cs` og `LoanService.cs`.

Den første av disse ble oppdaget *fordi* handleren opprinnelig hadde en
tredje gren, `InvalidOperationException → 409`, ment som et forsvar mot at en
entitet kastet ved en ugyldig tilstandsovergang. Den grenen gjorde
rutingfeilen usynlig som en falsk `409 Regelbrudd` i stedet for en synlig
`500`. Grenen ble fjernet igjen da dette ble oppdaget - et unntak av en type
handleren ikke uttrykkelig kjenner igjen, skal være synlig som en feil, ikke
maskeres som et forretningsregelbrudd.

**Oppdaget etterpå, manuelt gjennom Scalar (se ADR-0018):** samme
`InvalidOperationException`-mønster som punkt 2, men for `.OrderBy`/
`.OrderByDescending` kjedet på resultatet av en allerede projisert spørring
(`GET /api/borrowers`, som feilet i praksis - ikke bare i teorien). Rettet på
samme måte: sortering lagt inn i selve spørringen, som `orderby` før
`select`, i stedet for kjedet på resultatet.

Grunnen til at dette ikke ble fanget opp av testene som allerede fantes: ingen
av integrasjonstestene for `Borrower`, `Equipment` eller `Loan` kalte det
autentiserte `GET`-listeendepunktet i det hele tatt - bare `401`-sjekken uten
token, som aldri når spørringen. `SportForAlle.Tests/Controllers/*EndpointTests.cs`
har nå en `GetAll_returns_...`-test per ressurs som faktisk henter en liste
med data i, nettopp for å dekke dette. Se også oppdateringen i
`backend-endpoint`-skillen: en integrasjonstest skal dekke listeendepunktet
med reelle data, ikke bare opprettelse og enkeltoppslag.
