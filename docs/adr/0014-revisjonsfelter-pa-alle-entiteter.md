# ADR-0014: Revisjonsfelter på alle entiteter

- **Status:** Akseptert
- **Dato:** 2026-09-03

## Kontekst

Butikken har tre ansatte. Når noe endres - et utstyr rekategoriseres, en
låntaker utestenges, et notat skrives - bør det være mulig å se hvem som
gjorde det og når, ikke bare hva som skjedde. ER-diagrammet i
[`03-domenemodell.md`](../03-domenemodell.md) tegner bare én slik kobling
eksplisitt (`STAFF ||--o{ LOAN : "registrerer"`), men det samme behovet
gjelder i praksis alle entiteter en ansatt oppretter eller endrer.

Autentisering er ikke bygget ennå - det er neste steg etter domenemodellen.
Spørsmålet er derfor ikke bare *om* revisjonsfelter skal finnes, men om
skjemaet skal bygges klart for dem nå, eller om det utsettes til Auth0 er på
plass.

## Beslutning

Alle entiteter arver en felles `AuditableEntity`-baseklasse med `CreatedAt`,
`CreatedByStaffId`, `UpdatedAt` og `UpdatedByStaffId`. `*StaffId`-feltene er
nullbare fremmednøkler til `Staff`, og forblir tomme helt til
autentiseringssteget kobler et Auth0-token til en ansatt-id og sender den inn
i hvert kall. Kolonnene finnes altså i skjemaet allerede nå, slik at det
steget ikke krever en egen migrasjon bare for å legge dem til.

Det finnes bevisst **ingen** `DeletedAt`/`DeletedByStaffId`. Sletting av
personopplysninger (låntakere, foresatte) krever en anonymiseringsrutine som
ikke er avklart med oppdragsgiver ennå, se
[`09-lover-og-regler.md`](../09-lover-og-regler.md). Et sletteflagg bygget
foran den avklaringen ville låst en løsning før spørsmålet er besvart.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Vent med revisjonsfelter til autentisering er på plass | Ingen kolonner som står ubrukte i mellomtiden. | Krever en ny migrasjon på alle tabeller samtidig som autentiseringssteget bygges, i stedet for å være en ferdig del av skjemaet. | Splitter ett skjemavalg i to migrasjoner uten grunn |
| Revisjonsfelter bare på de entitetene som er tydelig "ansatt-handlinger" (`Ban`, `ContactAttempt`, `Note`) | Mindre skjema. | `Guardian`, `Borrower` og `Equipment` registreres også av ansatte, og ville manglet samme sporbarhet uten noen god grunn til forskjellen. | Vilkårlig skille som ikke følger av domenet |
| **Revisjonsfelter på alle entiteter, via en felles base-klasse** | Konsekvent sporbarhet uten unntak å huske på. Skjemaet er klart for autentiseringssteget uten en ekstra migrasjon. | Kolonnene er ubrukte (alltid `NULL` for `*StaffId`) fram til autentisering er bygget. | Valgt |
| Et generisk `DeletedAt`-sletteflagg på alle entiteter nå | Konsekvent med de andre revisjonsfeltene. | Ville late som om sletting av personopplysninger er løst med et flagg, når reell sletting av `Borrower`/`Guardian` krever anonymisering, ikke skjuling av raden. | Bygger foran en personvernavklaring som ikke er tatt |

## Konsekvenser

**Positivt**

- Når autentisering er på plass, er det å fylle disse feltene en endring i
  tjenestelaget, ikke en ny migrasjon.
- Sporbarhet er lik for alle entiteter - ingen må huske hvilke tabeller som
  "har" revisjonsfelter og hvilke som ikke har det.
- `Loan.CreatedByStaffId` dekker `STAFF ||--o{ LOAN : "registrerer"` fra
  ER-diagrammet direkte, uten et eget felt ved siden av.
- Fremmednøklene til `Staff` er `SetNull` ved sletting, ikke `Restrict` - å
  fjerne en tidligere ansatt låser ikke historikken deres fast.

**Negativt eller risiko**

- `CreatedByStaffId` og `UpdatedByStaffId` er `NULL` på alt som opprettes før
  autentisering er bygget - revisjonssporet er ufullstendig for data fra
  denne perioden.
- Ingen sletting av personopplysninger er løst av dette - kun forberedt for.
  Anonymiseringsrutinen i `09-lover-og-regler.md` er fortsatt et åpent punkt
  som må avklares med oppdragsgiver før den bygges.
