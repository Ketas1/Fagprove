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

## Status 2026-09-07: bakgrunnsjobben er bygget

Begge halvdeler av beslutningen er nå implementert:

- Lesing: `Loan.IsOverdueNow(IClock)` - uendret siden opprinnelig beslutning.
- Skriving: `Loan.RefreshOverdueStatus(IClock)`, kalt av
  `Services/BackgroundJobs/OverdueLoanBackgroundService.cs` - en
  `BackgroundService` registrert som `IHostedService` i `Program.cs`, ikke en
  ekstern jobbplanlegger (Hangfire/Quartz). Prosjektet trengte ikke den
  avhengigheten fra før, og en `BackgroundService` med `PeriodicTimer` er det
  ASP.NET Core allerede tilbyr for nettopp dette.
- Intervallet er konfigurerbart (`OverdueCheck:IntervalSeconds` i
  `appsettings.json`, standard 60 sekunder). Verdien er valgt for at en
  overgang skal være synlig raskt ved uttesting og demonstrasjon av systemet -
  den er ikke justert for produksjonslast, og forblir en åpen vurdering hvis
  systemet noen gang driftes for reelle brukere.
- Selve `RefreshOverdueLoansAsync`-metoden er skilt ut på `LoanService` slik at
  den kan testes direkte med en flyttet klokke, uten å vente på at
  bakgrunnsjobben faktisk kjører - se `Controllers/OverdueLoanRefreshTests.cs`
  og `05-api.md`.
