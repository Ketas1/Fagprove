# ADR-0013: Guid som primærnøkkel

- **Status:** Akseptert
- **Dato:** 2026-09-03

## Kontekst

Domenemodellen fikk sine første ekte entiteter (`Borrower`, `Loan`, `Equipment`
og de øvrige) i samme endring som denne ADR-en hører til, og valget av
primærnøkkeltype måtte gjøres for alle på én gang - se
[`04-databasedesign.md`](../04-databasedesign.md).

To forhold gjorde spørsmålet konkret, ikke bare teoretisk:

- Systemet lagrer personopplysninger om **barn**. Ressurser eksponeres i
  URL-er som `/api/borrowers/{id}`. Med fortløpende heltall kan hvem som helst
  med tilgang telle seg gjennom alle barn i systemet ved å øke tallet - det
  samme prinsippet som allerede gjelder for bilder i
  [`08-sikkerhet.md`](../08-sikkerhet.md) ("bilder lagres slik at de ikke er
  tilgjengelige via en gjettbar URL") bør gjelde enhetlig for alle ressurser
  som gjelder barn og foresatte.
- Flere entiteter oppretter relaterte poster før noe er lagret - et
  `Borrower` som blir utestengt oppretter samtidig en `Ban` som peker på
  låntakerens egen id (se `Borrower.Ban` i `Models/Borrower.cs`). Med en
  databasegenerert heltallsnøkkel er `Id` `0` helt fram til `SaveChanges`
  kjører, og den relasjonen kan ikke settes riktig i minnet før det skjer.

## Beslutning

Alle entiteter bruker `Guid` som primærnøkkel, generert av entiteten selv i
konstruktøren (`Guid.NewGuid()`), ikke av databasen. `Id`-egenskapen
konfigureres med `ValueGeneratedNever()` i EF Core, og fremmednøkler har samme
type som nøkkelen de peker på.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| `int`/`bigint` med autogenerert identity | Mindre lagringsplass og indekser. Lett å lese og skrive for hånd ved feilsøking. Databasen garanterer unikhet. | Fortløpende og gjettbare - upassende når ressursene gjelder barn. `Id` er `0` fram til lagring, som gjør det vanskelig å bygge relasjoner mellom nye entiteter før `SaveChanges`. | Gjettbarheten er et reelt personvernproblem for denne dataen, ikke bare en teoretisk innvending |
| **`Guid`, klientgenerert** | Ikke gjettbar. Entiteten har en ekte id fra det øyeblikket den opprettes i minnet, uavhengig av databasen. | Større lagringsplass (16 byte mot 4). Ikke sortbar eller lett å skrive for hånd. Unikhet er applikasjonens ansvar, ikke databasens - kollisjonssannsynligheten er likevel forsvinnende liten. | Valgt |
| `int` internt, egen offentlig `Guid` i tillegg | Beholder databasens fordeler internt. | To identifikatorer per entitet å holde styr på og mappe mellom, for en fordel som uansett ikke trengs i et system i denne størrelsen. | Unødvendig kompleksitet for problemet |

## Konsekvenser

**Positivt**

- Ingen ressurs-id i systemet er gjettbar - en forlengelse av prinsippet som
  allerede gjelder bilder, til alle ressurser, se
  [`09-lover-og-regler.md`](../09-lover-og-regler.md).
- En entitet har en gyldig, endelig `Id` fra konstruksjonen, som gjør det
  enkelt for en aggregatrot å bygge relaterte poster (som `Borrower` og
  `Ban`) før noe lagres.
- `CreatedAt` fra `AuditableEntity` (se ADR-0014) dekker behovet for å se
  opprettelsesrekkefølge, som ellers ville vært den vanligste grunnen til å
  savne et sorterbart heltall.

**Negativt eller risiko**

- Litt større lagringsplass og indeksstørrelse enn `int`. Ubetydelig for
  datamengden dette systemet håndterer.
- `Guid`-verdier er upraktiske å lese, skrive eller sammenligne for hånd ved
  manuell feilsøking i databasen, sammenlignet med et lite heltall.
- Unikhet garanteres av `Guid.NewGuid()`, ikke av databasen. Sannsynligheten
  for kollisjon er neglisjerbar i praksis, men det er formelt et ansvar som
  flyttes fra databasen til applikasjonen.
