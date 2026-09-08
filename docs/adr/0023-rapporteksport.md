# ADR-0023: Rapporteksport lages i nettleseren, og inneholder bare aggregerte tall

- **Status:** Akseptert
- **Dato:** 2026-09-07

## Kontekst

Kommunen finansierer utlånsordningen og trenger dokumentasjon på at den
virker. Tallene må kunne tas ut av systemet og legges ved en rapport,
arkiveres eller regnes videre på i et regneark - ikke bare leses på en skjerm.

Det gir to spørsmål som må avgjøres sammen, fordi svaret på det ene begrenser
det andre:

1. **Hva skal filen inneholde?** Et naturlig førsteforslag er «rådata»: én rad
   per utlån. Men [`09-lover-og-regler.md`](../09-lover-og-regler.md) bygger
   hele grunnlaget for rapportering til kommunen på GDPR art. 6 (1) e, og det
   grunnlaget forutsetter at rapportene er **aggregerte og ikke inneholder
   personopplysninger**. En fil med én rad per utlån - med barnets navn, eller
   med id-er som kan slås opp igjen - ville være en utlevering av
   personopplysninger om barn, og et annet rettslig spørsmål enn det som
   allerede er utredet.
2. **Hvor skal filen lages?** Enten på serveren, som nye endepunkter som
   returnerer `text/csv` og `application/pdf`, eller i nettleseren fra tallene
   siden allerede har hentet.

Rapportsiden viser to rapporter (se
[`03-domenemodell.md`](../03-domenemodell.md)), begge med de samme fem tallene,
pluss en oppdeling over tid til søylediagrammet. Det er alt eksporten skal
inneholde.

## Beslutning

Eksporten inneholder **bare aggregerte tall** - ingen navn, ingen id-er, ingen
enkeltlån - og begge filformatene bygges **i nettleseren** fra tallene
rapportsiden allerede har hentet fra API-et. Formatene er **Excel (`.xlsx`)**
og **PDF**. Excel lages med `write-excel-file`, PDF med `jspdf` og
`jspdf-autotable`. Alle tre lastes først når noen faktisk ber om en fil.

Begge formatene rendres fra én felles, **typet** kilde (`buildReportTables` i
`frontend/src/lib/report-export.ts`), slik at et tall ikke kan stå i den ene
filen og avvike i den andre.

### Endret 2026-09-07: CSV erstattet av Excel

Første versjon av denne beslutningen brukte **CSV**, skrevet for hånd uten
noen ny avhengighet. Det ble forkastet samme dag, etter at filen faktisk ble
åpnet i Excel. En CSV er bare tekst, og bærer verken celletyper eller
formatering. Excel må derfor gjette, og gjetter feil på to måter som betyr noe
her:

- **Datokonvertering.** Excel leste aldersgruppen `3-7` som 3. juli og `8-12`
  som 8. desember, mens `13-18` ble stående som tekst fordi det ikke finnes
  noen måned 18. Kolonnen ble altså både feil og *inkonsistent*. Det samme
  skjedde med månedsetiketten `2026-03`. Dette er den samme oppførselen som
  til slutt tvang genetikere til å døpe om menneskegener, fordi `SEPT1` stadig
  ble 1. september.
- **Skilletegn.** Excel velger skilletegn ut fra Windows sin regioninnstilling.
  Semikolon er riktig for norsk Excel, men på en engelskspråklig maskin deles
  det på komma, og hele filen havner i kolonne A.

Ingen av delene kan løses *i* et CSV-format. Eneste utvei er `="3-7"`, et
Excel-spesifikt påfunn som ødelegger filen for alle andre verktøy. En `.xlsx`
sier derimot i selve filen at cellen er tekst, at 412 er et tall, og at 0,74 er
en prosent - og kan ha tre atskilte ark. CSV ble derfor fjernet helt, ikke
beholdt ved siden av: å la den ligge ville bare gitt en fil som stille
forvansker tallene for den som åpner den.

**Fanget under samme endring:** en `Date` bygget på lokal midnatt ble
serialisert til Excel som 28. februar kl. 23:00 i stedet for 1. mars, slik at
en månedsbøtte viste feil måned. Perioden bæres derfor som en `yyyy-MM-dd`-
streng gjennom hele modellen og gjøres om til en dato først i selve
Excel-cellen, som UTC-midnatt. Feilen ble funnet ved å pakke ut den genererte
`.xlsx`-filen og lese celleverdiene direkte.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Rådata-CSV med én rad per utlån | Kommunen kan pivotere selv | Utlevering av personopplysninger om barn; bryter grunnlaget i `09-lover-og-regler.md` | Krever et nytt rettslig grunnlag som ingen har bedt om. Oppdragsgiver bekreftet 2026-09-07 at bare antall er relevant |
| Pseudonymisert CSV (én rad per utlån, uten identifiserende felter) | Ekte rådata, fortsatt forsvarlig | Fortsatt en radliste som kan sammenstilles med annen informasjon; mer å begrunne | Vurdert som reelt alternativ, men oppdragsgiver trenger bare tallene. Kan tas opp igjen hvis behovet oppstår |
| Serverside-endepunkter som returnerer ferdig fil | Én kilde til filen; kan kalles av andre systemer | To nye endepunkter, og en PDF-pakke i backend. `QuestPDF` sin gratis lisens har en omsetningsgrense, som er en dårlig arv å gi videre til en kommunal virksomhet | Filen bygges av tall nettleseren allerede har. Serverrunden gir ingenting her |
| `window.print()` og «lagre som PDF» | Ingen nye pakker, ingen lisensrader | Filnavn og topptekst styres av nettleserens dialog, og det er ikke en nedlastingsknapp | Var anbefalingen så lenge PDF-en skulle inneholde diagrammer. Da innholdet ble strammet inn til rene talltabeller, ble biblioteksveien både billigere og bedre |
| `jspdf` + `html2canvas` | Gjenbruker sidens utseende direkte | Rasteriserer alt til bilde - uskarp tekst, ingen markerbar tekst i PDF-en | Unødvendig: innholdet er talltabeller, som `jspdf-autotable` setter som ekte tekst |
| CSV med `sep=;` og én flat tabell | Ingen ny avhengighet | Løser skilletegnet, men ikke datokonverteringen eller celletypene | Halv løsning på et problem som forsvinner helt med `.xlsx` |
| `xlsx` (SheetJS) | Mest kjente biblioteket | npm-pakken står på 0.18.5 fra 2022; SheetJS distribuerer nå utenfor npm | Dårlig arv å gi videre til en annen IT-avdeling |
| `exceljs` | Moden, MIT | Tyngre, og nettleserbygget er klumpete | `write-excel-file` er mindre og laget for nettleseren |

## Konsekvenser

**Positivt**

- Eksportfilen kan ikke lekke personopplysninger, fordi datagrunnlaget den
  bygges av ikke inneholder noen. Det er en egenskap ved arkitekturen, ikke en
  regel noen må huske å følge.
- Ingen nye endepunkter, og ingen PDF-pakke i backend.
- PDF-en inneholder ekte, markerbar og søkbar tekst.
- Excel-fila bærer typene selv: 412 er et tall, 0,74 er en prosent, og «3-7»
  er tekst. Ingen som åpner den trenger å vite noe om importveivisere eller
  regioninnstillinger.
- Alle tre bibliotekene lastes dynamisk, så vekten treffer bare den som faktisk
  ber om en fil - ikke alle som åpner rapportsiden.
- Fire nye avhengigheter (`recharts`, `jspdf`, `jspdf-autotable`,
  `write-excel-file`), alle MIT, alle bekreftet mot lisensfilen i
  `node_modules` - se [`12-lisenser-og-vilkar.md`](../12-lisenser-og-vilkar.md).

**Negativt eller risiko**

- **Kommunen kan ikke regne videre på annet enn det vi har valgt å telle.**
  Ønskes en ny oppdeling, må den bygges i API-et først. Det er en bevisst
  innsnevring, ikke en glemsel.
- Eksporten finnes bare som en knapp i grensesnittet. Et annet system som vil
  hente tallene automatisk må kalle rapportendepunktene selv og lage sin egen
  fil.
- Selve filgenereringen er **ikke enhetstestet ende til ende**. Innholdet er
  det - `buildReportTables` og `buildReportSheets` er testet, inkludert at
  hver aldersgruppe-etikett faktisk får celletypen `String`, og at ingen uuid
  kan opptre i filen - men at `jspdf` produserer en gyldig PDF er ikke
  verifisert automatisk, bare manuelt. Excel-filen *er* verifisert ved å
  generere den og lese celleverdiene ut av zip-arkivet. Se
  [`07-testing.md`](../07-testing.md).
- **`.xlsx` er et binærformat.** Det kan ikke inspiseres i en teksteditor eller
  diffes i git slik en CSV kan, og krever Excel, LibreOffice eller et
  bibliotek for å leses. Det er prisen for at filen bærer typer i det hele
  tatt.
- Én avhengighet mer enn den opprinnelige planen, som eksplisitt var «ingen
  ny pakke». Den er MIT og lastes dynamisk, men det er likevel en pakke til
  som må vedlikeholdes.
