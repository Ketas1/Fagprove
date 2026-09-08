# ADR-0025: Bare åpne utlån kan rettes, og feil utstyr frigis uten å telle som retur

- **Status:** Akseptert
- **Dato:** 2026-09-08

## Kontekst

Et utlån registreres i butikken mens barnet står ved disken. Velger den ansatte
feil barn i nedtrekkslisten, feil ski, eller taster feil returdato, fantes det
ingen måte å rette det på. Utlånet måtte enten leveres og registreres på nytt -
noe som gir en falsk retur i historikken - eller rettes direkte i databasen.

Retting av et utlån er ikke en vanlig oppdatering, fordi utlånet er knyttet til
to tilstandsmaskiner og en forretningsregel samtidig:

- **Utstyret.** Bytter man utstyr på lånet, må det forrige settes tilbake til
  `Available` og det nye til `OnLoan`, ellers lyver utstyrslisten.
- **Låntakeren.** Forretningsregel 2 sier at en utestengt låntaker, eller en
  med et åpent forfalt lån, ikke kan låne. Flytter man lånet til en annen
  låntaker, må regelen kjøres på nytt mot den nye.
- **Avsluttede utlån.** `DaysLate` regnes ut i det øyeblikket utlånet leveres,
  og en for sen levering har allerede økt `Borrower.LateReturnCount` og satt
  `IsUnreliable`. Begge er skrevet én gang og kan ikke regnes ut på nytt fra en
  redigeringsdialog.

Rapportene til kommunen bygger på nettopp disse tallene.

## Beslutning

`PUT /api/loans/{id}` retter **bare** utlån som er `Active` eller `Overdue`. Et
`Returned` eller `Lost` utlån avvises med `409 LoanAlreadyClosed`. Notater kan
fortsatt legges til på et avsluttet utlån.

Bytte av utstyr frigis gjennom `Equipment.ReleaseFromCorrectedLoan`, en egen
overgang fra `OnLoan` til `Available` som **ikke** rører `Condition` og ikke
teller som retur.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Tillate retting av alle utlån, også avsluttede | Alt kan rettes, ingen blindsone | `DaysLate` må regnes ut på nytt, og `LateReturnCount`/`IsUnreliable` må rulles tilbake på den gamle låntakeren og legges til på den nye. Ingen av delene lar seg gjøre riktig uten en historikk over hva som faktisk skjedde | Ville stille rapportgrunnlaget i ubalanse uten at noen merker det |
| Bruke `Equipment.Return()` for å frigi feil utstyr | Gjenbruker en overgang som allerede finnes | `Return()` setter `Condition` og betyr «levert tilbake». Utstyret var aldri ute - en retting ville se ut som en retur i dataene | Feil utstyr ble aldri lånt ut, og skal ikke etterlate spor av en retur |
| Slette utlånet og registrere det på nytt | Ingen ny kode i det hele tatt | Sletter historikk rapportene bygger på, og gir feil `StartedAt`. Sletting av utlån er dessuten ikke tillatt, se `04-databasedesign.md` | Ødelegger nettopp det systemet finnes for å dokumentere |
| Bare tillate retting av datoer, ikke låntaker og utstyr | Enklest, ingen tilstandsmaskiner berøres | Den vanligste feilen i butikken er å velge feil rad i en liste, ikke å taste feil dato | Løser den minst sannsynlige feilen |

## Konsekvenser

**Positivt**

- Den ansatte kan rette en feilregistrering med én gang, mens barnet fortsatt
  står der.
- Utstyrslisten og utlånet kan ikke komme i utakt: begge elementene flyttes i
  samme operasjon.
- Forretningsregel 2 gjelder også ved retting, så en retting kan ikke brukes til
  å omgå en utestengelse.
- En retting kan aldri forfalske en retur, siden `ReleaseFromCorrectedLoan` er
  en annen overgang enn `Return()`.

**Negativt eller risiko**

- **En feil oppdaget etter at utlånet er levert, kan ikke rettes i
  grensesnittet.** Det er en bevisst blindsone, og den må rettes i databasen.
- `ReleaseFromCorrectedLoan` er en overgang som ikke står i «Utstyrstatus»-
  diagrammet i `03-domenemodell.md`. Diagrammet beskriver den normale
  livssyklusen; denne pilen er en rettepil ved siden av. Det er en ekstra ting
  en ny utvikler må forstå.
- Ingenting logger at en retting har skjedd, eller hva verdiene var før.
  Systemet har ingen revisjonslogg - se gap-listen i `14-utviklerhandbok.md`.
- Endres `StartedAt`, flytter utlånet seg mellom rapportperioder. Det er samme
  klasse konsekvens som i [ADR-0024](./0024-redigerbar-fodselsdato.md), og
  gjelder også her.
