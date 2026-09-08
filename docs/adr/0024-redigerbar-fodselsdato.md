# ADR-0024: Fødselsdato på låntaker kan rettes etter registrering

- **Status:** Akseptert
- **Dato:** 2026-09-08

## Kontekst

Fødselsdatoen til et barn ble opprinnelig satt ved registrering og kunne ikke
endres etterpå. `Borrower` hadde ingen metode for å endre den, og
`UpdateBorrowerRequest` tok bare imot navn. Redigeringsdialogen i grensesnittet
forklarte begrensningen med en henvisning til `03-domenemodell.md` - men den
regelen står ikke der. Begrensningen var altså et valg i koden, ikke en
forretningsregel noen hadde blitt enige om.

Samtidig er fødselsdato et felt ansatte taster inn manuelt i butikken, og det er
det eneste feltet som avgjør både om barnet er innenfor aldersgrensen 3-18 og
hvilken aldersgruppe utlånet havner i i rapporten til kommunen. En skrivefeil
her var ikke mulig å rette uten direkte tilgang til databasen.

Det som gjorde spørsmålet reelt, er at rettingen ikke er uskyldig:
`ReportService.GetAgeGroupsAsync` slår opp `Borrower.DateOfBirth` på nytt hver
gang rapporten kjøres, og regner ut alderen ved `Loan.StartedAt`. Alderen fryses
altså ikke ned på lånet. Endrer man fødselsdatoen, flytter barnets tidligere
utlån seg mellom aldersgruppene i rapporter for perioder som allerede er
avsluttet.

## Beslutning

Fødselsdato kan rettes gjennom `PUT /api/borrowers/{id}`. Rapporttallene regnes
ut på nytt fra den rettede datoen, også for historiske perioder. Foresatt-
koblingen (`GuardianId`) forblir uforanderlig - å flytte et barn til en annen
foresatt er en annen operasjon enn å rette en skrivefeil, og har ikke noe
endepunkt.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Beholde datoen uforanderlig | Historiske rapporttall kan aldri endre seg i ettertid | En skrivefeil er umulig å rette uten databasetilgang. Bryter med artikkel 16 i GDPR (retting), som `09-lover-og-regler.md` allerede lover at systemet støtter | Retten til retting er ikke valgfri, og en feil dato gir feil tall uansett |
| Fryse alderen på lånet ved registrering | Historiske rapporter blir helt stabile, uavhengig av senere retting | Krever et nytt felt på `Loan` og en migrasjon. En rettet fødselsdato ville da *ikke* rette de gamle tallene - feilen ville blitt stående i rapportene for alltid | Løser stabilitet, men bevarer feilen. Det er feil prioritering når hele poenget er å rette noe som er galt |
| La datoen rettes, men bare når barnet ikke har utlån | Ingen rapport kan endre seg | Nettopp de barna som har lånt mest, er de det haster mest å rette | Gjør funksjonen ubrukelig akkurat der den trengs |

## Konsekvenser

**Positivt**

- Artikkel 16 (retting) er faktisk støttet i grensesnittet, ikke bare påstått i
  dokumentasjonen.
- En feiltastet dato gir feil aldersgruppe i rapporten til kommunen. Å rette
  datoen retter også tallene, som er det kommunen faktisk vil ha.
- Aldersgrensen 3-18 kontrolleres mot en dato som stemmer.

**Negativt eller risiko**

- **En rapport som allerede er sendt til kommunen, kan ikke reproduseres.**
  Kjører man samme periode på nytt etter en retting, kommer det andre tall. Det
  finnes ingen versjonering av rapporter i systemet, og ingenting logger at en
  retting fant sted.
- Ingenting skiller en reell retting fra en feilretting som selv er feil. Den
  ansatte som taster inn en ny dato, kan gjøre vondt verre uten at systemet
  merker det.
- Grensesnittet advarer om konsekvensen bare når datoen faktisk endres i
  dialogen. Kaller man API-et direkte, finnes ingen advarsel.

En senere IT-avdeling som trenger sporbare, reproduserbare rapporter bør
vurdere det andre alternativet over - å fryse alderen på `Loan` ved
registrering - kombinert med en logg over rettinger. Det er bevisst ikke gjort
her, fordi det krever både en migrasjon og en revisjonslogg som ikke er bygget.
