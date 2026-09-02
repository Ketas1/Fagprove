# ADR-0011: Forfall både beregnet og lagret

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Et av de tre hovedproblemene systemet skal løse er at ansatte i dag må sjekke
manuelt om et lån har passert leveringsfristen. Systemet skal oppdage forfall
selv.

Spørsmålet er hvordan `LoanStatus.Overdue` skal oppstå: skal statusen beregnes
når data leses, eller skrives til databasen av en jobb som kjører jevnlig?

## Beslutning

Begge deler.

- Statusen **beregnes ved lesing**: et lån er forfalt dersom `DueDate` er passert
  og `ReturnedAt` er tom. Det er alltid korrekt, uavhengig av om en jobb har
  kjørt.
- En **bakgrunnsjobb skriver statusen** til databasen med jevne mellomrom, slik
  at `Status` er et reelt felt som kan brukes i spørringer, filtre og rapporter.

Beregningen er fasit. Jobben er en materialisering av den, ikke en egen sannhet.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt alene |
| --- | --- | --- | --- |
| Kun beregnet ved lesing | Alltid korrekt. Ingen bakgrunnsjobb å drifte. | Kan ikke filtrere eller aggregere effektivt på status i databasen. Rapportspørringer må gjenta datologikken. | Rapportering og indeksering blir tungvint |
| Kun bakgrunnsjobb | Status er et vanlig felt, enkelt å spørre mot og indeksere. | Oversikten er feil i tidsrommet mellom at fristen går ut og at jobben kjører. En ansatt kan se et lån som aktivt selv om det er forfalt. | Systemet ville vist feil informasjon i perioder |
| **Begge** | Alltid korrekt visning, og et lagret felt å spørre mot. | Samme regel finnes to steder og må holdes i sync. | Valgt |

## Konsekvenser

**Positivt**

- Ansatte ser aldri et forfalt lån presentert som aktivt, uansett når jobben sist
  kjørte.
- Rapporter og filtre kan bruke `Status` direkte, med indeks på
  `(Status, DueDate)`.
- Regelen om blokkering av nye utlån kan bygge på den beregnede statusen, og er
  dermed ikke avhengig av at en jobb har kjørt. Det er viktig: den regelen er
  systemets kjernemekanisme.

**Negativt eller risiko**

- Logikken for "hva er forfalt" må finnes ett sted i domenelaget og brukes både
  av beregningen og av jobben. Den skal ikke skrives to ganger.
- Bakgrunnsjobben er en ekstra komponent som må kjøre og som må testes.
- Klokken må være injisert, ikke lest fra systemet direkte, ellers kan verken
  beregningen eller jobben testes. Se `07-testing.md`.
