# Testing

> **Status:** strategien er bestemt. Konkrete tester og resultater fylles ut
> etter hvert som de skrives.

Tester skrives sammen med koden fra starten av utviklingen, ikke som en egen fase
til slutt. Grunnen er praktisk: å feilsøke et ferdig system uten tester tar mer
tid enn å skrive testene underveis, og pipelinen skal gi rask tilbakemelding ved
feil.

## Testnivåer

| Nivå | Verktøy | Dekker | Kjøres |
| --- | --- | --- | --- |
| Enhetstester | xUnit | Forretningsregler og tilstandsoverganger i domenelaget | Hver commit |
| Integrasjonstester | xUnit + Testcontainers | API-endepunkter mot ekte PostgreSQL | Hver commit |
| End to end | Playwright | Hele arbeidsflyten gjennom grensesnittet | Hver commit, og manuelt |
| Statisk analyse | `dotnet build`, ESLint, `dotnet format` | Kompileringsfeil, lintfeil, formatering | Hver commit |

## Hva som prioriteres

Med begrenset tid testes det som faktisk kan gå galt, og det systemet er laget
for å løse. Prioritert rekkefølge:

1. **Blokkering av nye utlån.** En låntaker med åpent forfalt lån eller
   utestengelse skal ikke få låne. Dette er kjernemekanismen i løsningen, og skal
   testes både som enhetstest i domenelaget og som integrasjonstest mot
   endepunktet.
2. **Automatisk forfall.** Et lån blir forfalt når forventet returdato passeres,
   uten at noen gjør noe.
3. **Retur etter frist.** `DaysLate` beregnes riktig, `LateReturnCount` økes, og
   låntakeren markeres som upålitelig.
4. **Tilstandsoverganger.** Ugyldige overganger skal avvises, ikke føre til en
   inkonsistent tilstand. For eksempel: utstyr som allerede er `OnLoan` kan ikke
   lånes ut på nytt.
5. **Autorisasjon.** Kall uten token gir `401`, feil rolle gir `403`.
6. **Rapportspørringer.** Aldersgruppefordeling og opptelling gir riktige tall.

## Testbar tid

Domenelaget bruker en abstraksjon for klokken i stedet for systemtiden direkte.
Uten det er forfall og forsinkelse i praksis ikke mulig å teste, fordi testen må
vente på at tiden går. Med en injisert klokke kan en test flytte tiden fram og
verifisere at lånet blir forfalt.

Dette er et av de viktigste designvalgene for testbarhet i prosjektet.

## Testdata

> Beskriv hvordan testdata bygges opp, og hvordan hver test starter fra en kjent
> tilstand.

Testene bruker ikke ekte personopplysninger. Navn og kontaktinformasjon i
testdata er oppdiktet.

## Kjøre testene

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && bun run lint && bun run build

# End to end
cd frontend && bun run test:e2e
```

## Manuell testing

> Dokumenter hvilke deler som testes manuelt og etter hvilken sjekkliste.
> Hovedarbeidsflyten skal gjennomgås manuelt i grensesnittet:
>
> 1. Registrer foresatt og barn, koble dem sammen
> 2. Registrer utstyr
> 3. Registrer utlån, med bilde
> 4. La lånet forfalle
> 5. Kontakt foresatt og logg forsøket
> 6. Registrer retur etter frist
> 7. Kontroller at låntakeren er markert som upålitelig
> 8. Forsøk nytt utlån mens et forfalt lån er åpent, og kontroller at det blokkeres
> 9. Utesteng, opphev utestengelsen mot registrert gebyr
> 10. Kontroller rapportene

## Testresultater

> Fyll ut mot slutten: hva som er dekket, hva som ikke er dekket, og hvilke feil
> som ble funnet og rettet underveis. En ærlig beskrivelse av det som ikke er
> testet hører også hjemme her.
