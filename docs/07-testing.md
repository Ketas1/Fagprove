# Testing

> **Status:** strategien er bestemt. Konkrete tester og resultater fylles ut
> etter hvert som de skrives.

Tester skrives sammen med koden fra starten av utviklingen, ikke som en egen fase
til slutt. Grunnen er praktisk: å feilsøke et ferdig system uten tester tar mer
tid enn å skrive testene underveis, og pipelinen skal gi rask tilbakemelding ved
feil.

## Testnivåer

| Nivå | Verktøy | Dekker | Status |
| --- | --- | --- | --- |
| Enhetstester backend | xUnit | Forretningsregler og tilstandsoverganger på entitetene | På plass |
| EF-modelltester | xUnit | At EF-modellen bygger og at konfigurasjonene er tatt i bruk | På plass |
| Endepunkttester | xUnit + `WebApplicationFactory` | At API-et starter og ruter riktig | På plass |
| Enhetstester frontend | Jest + Testing Library | Komponenter og logikk i `lib/` | På plass |
| Integrasjonstester mot database | xUnit + Testcontainers | Endepunkter mot ekte PostgreSQL | Ikke satt opp |
| End to end | Ikke valgt | Hele arbeidsflyten gjennom grensesnittet | Ikke satt opp |
| Statisk analyse | `dotnet build -warnaserror`, `dotnet format`, ESLint | Kompileringsfeil, formatering, lintfeil | På plass |

Entitetstestene er de viktigste: fordi entitetene eier sin egen tilstand, kan
forretningsreglene testes uten database, HTTP eller Auth0. Se
[ADR-0012](./adr/0012-lagdelt-monolitt.md).

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
   Den viktigste er at de fire underradene summerer seg til totalen: hvert
   utlån har nøyaktig én av de fire statusene, så et tall som ikke går opp
   betyr at en tilstand er glemt eller telt to ganger.

## Testbar tid

Domenelaget bruker en abstraksjon for klokken i stedet for systemtiden direkte.
Uten det er forfall og forsinkelse i praksis ikke mulig å teste, fordi testen må
vente på at tiden går. Med en injisert klokke kan en test flytte tiden fram og
verifisere at lånet blir forfalt.

Dette er et av de viktigste designvalgene for testbarhet i prosjektet.

## Testdata

Entitetstester bygger det de trenger direkte i testen (se `TestSupport/FakeClock.cs`
for den injiserte klokken). Endepunkttester som trenger en kjede av
forutsetninger - en foresatt før et barn, en kategori før utstyr - bruker
`SportForAlle.Tests/TestSupport/ApiTestDataBuilder.cs`, som oppretter dem via
ekte HTTP-kall mot den kjørende test-applikasjonen, i stedet for at hver test
gjentar de samme request-kroppene.

Hver test kjører mot den delte Docker-databasen (`docker compose up -d db`),
ikke en isolert database per test. Testene bruker derfor alltid tilfeldige
verdier (`Guid.NewGuid()`) der noe må være unikt, som serienummer og
kategorinavn, i stedet for faste navn som ville kollidert på tvers av
testkjøringer.

Testene bruker ikke ekte personopplysninger. Navn og kontaktinformasjon i
testdata er oppdiktet.

## Kjøre testene

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && bun run test && bun run lint && bun run build

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

Sist kjørt 2026-09-07, etter at rapportsiden og eksporten ble lagt til:

| Kommando | Resultat |
| --- | --- |
| `dotnet test` | **219 bestått, 0 feilet** |
| `dotnet format --verify-no-changes` | ingen avvik |
| `dotnet build -warnaserror` | null advarsler |
| `bun run test` (Jest) | **98 bestått, 16 suiter** |
| `bun run lint` | ingen avvik |
| `bun run build` | kompilerer, typesjekker og bygger alle rutene |

Dekket: alle forretningsreglene i `03-domenemodell.md` (blokkering av nye
utlån for utestengt/forfalt låntaker, aldersgrense ved registrering,
automatisk forfall - både lesing og den nye skrivende bakgrunnsjobben,
retur etter frist med `LateReturnCount`/`IsUnreliable`, utestengelse og
opphevelse inkludert gebyrkravet, bekreftet tap som setter både lån og utstyr
i riktig sluttstatus, kontaktforsøk) og rapportene, som integrasjonstester
mot den ekte databasen. For rapportene dekkes de fem tallene hver for seg,
at de summerer seg til totalen, aldersgrensene 7 og 8 (der inndelingen ble
rettet 2026-09-07), bøtter per dag/uke/måned inkludert tomme, at en uke
starter på mandag, at et for finmasket intervall avvises med `400`, og at
`overdue-summary` faktisk er borte.

I frontend er `report-period.ts` og `report-export.ts` enhetstestet - blant
annet at en halvutfylt egendefinert periode ikke stilltiende blir «hele
historikken», at måneder ikke renner over ved månedsskifte, at hver
aldersgruppe-etikett får celletypen `String` slik at Excel ikke kan gjøre
«3-7» om til en dato, at periodekolonnen blir UTC-midnatt og ikke faller på
dagen før, og at eksporten aldri kan inneholde en uuid.

Ikke dekket: ende-til-ende gjennom grensesnittet (ikke satt opp, se
`Testnivåer` over), selve PDF-genereringen (innholdet den bygges av er
testet, men at `jspdf` produserer en gyldig fil er bare bekreftet manuelt -
se [ADR-0023](./adr/0023-rapporteksport.md)), og selve tidsstyringen i
`OverdueLoanBackgroundService` (`PeriodicTimer`-løkken kjører ikke i noen
test - bare metoden den kaller, se `05-api.md`). Rollebasert `403` er ikke
testet, fordi rollen ikke finnes i tokenet ennå (samme begrunnelse som i
`05-api.md`, "Ikke bygget i denne omgangen").

Ingen feil funnet i denne runden ble stående - de tre som dukket opp
underveis (en `OrderBy` lagt til etter en allerede projisert type i
rapportspørringen, mangelfull `CurrentUserContext` i en manuelt opprettet
test-scope, og for stramme før/etter-sammenligninger i rapporttestene som
ikke tålte at xUnit kjører testklasser parallelt mot den delte databasen) ble
rettet før commit.
