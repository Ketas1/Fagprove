# ADR-0026: Tre operasjoner for å fjerne data - sletting, arkivering og anonymisering

- **Status:** Akseptert
- **Dato:** 2026-09-08

## Kontekst

Systemet manglet enhver måte å fjerne en låntaker, en foresatt eller en ansatt
på. Det er et problem av to grunner samtidig, og de trekker i hver sin retning.

**Den praktiske.** Et barn registrert ved en feil, en foresatt som aldri kom
tilbake, en ansatt som har sluttet - alt dette ble liggende for alltid.

**Den juridiske.** GDPR artikkel 5 (1) e sier at personopplysninger ikke skal
lagres lenger enn nødvendig, og artikkel 17 gir rett til sletting. Systemet
behandler opplysninger om **barn**, for en kommunalt eid virksomhet. Det er
omtrent så strengt regulert som personopplysninger blir.

Samtidig kan ikke alt bare slettes. Utlånshistorikken er hele grunnlaget for
rapporteringen til kommunen som finansierer ordningen. Sletter man en låntaker
med utlån, forsvinner tallene kommunen betaler for å få se - og
`04-databasedesign.md` forbyr det allerede i skjemaet, med `Restrict` på
`Loan.BorrowerId`.

Den nærliggende løsningen - «kan ikke slettes, så arkiver i stedet» - løser
bare den praktiske halvdelen. Et arkiv der navnet på et barn blir liggende i
det uendelige, er nøyaktig det artikkel 5 (1) e forbyr. Arkivering må ha en
slutt.

## Beslutning

Systemet har **tre** operasjoner, ikke to:

| Operasjon | Når | Reversibel |
| --- | --- | --- |
| **Slett** | Ingenting refererer til raden | Nei |
| **Arkiver** | Driftsmessig: vokst ut av ordningen, flyttet, ikke aktuell nå | **Ja** |
| **Anonymiser** | Lagringstiden er ute, eller det kommer et krav etter artikkel 17 | Nei |

Anonymisering fjerner navn og kontaktopplysninger, men beholder raden, slik at
utlånene fortsatt kan telles. Fødselsdatoen beholdes, fordi
aldersgrupperapporten regner ut alderen ved lånetidspunktet fra den - en
fødselsdato uten navn identifiserer ingen alene.

Ansatte slettes hardt. En ansatt kan ikke slette sin egen profil, siden
`RequireLinkedStaffMiddleware` da ville låst vedkommende ute umiddelbart.
Utlån slettes aldri.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Bare hard sletting | Enkelt, og artikkel 17 oppfylles bokstavelig | Sletter utlånsrader rapportene bygger på. Skjemaet forbyr det allerede med `Restrict` | Ødelegger det systemet finnes for å dokumentere |
| Bare arkivering, ingen anonymisering | Enkelt å bygge, ingenting går tapt | Navnet på et barn blir liggende for alltid. Bryter artikkel 5 (1) e, og gjør artikkel 17 umulig å oppfylle | Løser brukerbehovet og bryter loven samtidig |
| Bare anonymisering, ingen arkivering | Alltid lovlig | Ingen vei tilbake. En låntaker som bare skal skjules en sesong, mister navnet sitt permanent | For grovt for et driftsbehov som er midlertidig |
| Kaskadesletting av alt knyttet til låntakeren | Ett kall rydder alt | Sletter utlån, og dermed rapportgrunnlaget, uten at noen ser det skje | Samme problem som første rad, men vanskeligere å oppdage |

## Konsekvenser

**Positivt**

- Artikkel 17 kan faktisk oppfylles, også for et barn med lånehistorikk:
  opplysningene forsvinner, tallene blir stående.
- Kommunen mister ikke statistikk når et barn ber om å bli slettet.
- Arkivering dekker det driftsmessige behovet uten å være en forkledd
  evigvarende lagring, fordi anonymisering er sluttstasjonen.
- Avslagene er forklarende: `409 BorrowerHasHistory` og
  `409 GuardianHasBorrowers` sier hva som må gjøres i stedet.

**Negativt eller risiko**

- **Ingenting håndhever lagringstiden automatisk.** Reglene i
  `09-lover-og-regler.md` er en rutine ansatte må følge, ikke en jobb som
  kjører. Glemmer noen det, blir opplysningene liggende - se gap-listen i
  `14-utviklerhandbok.md`.
- Anonymisering beholder fødselsdato og utlånsmønster. Kombinert med små
  forhold kan det i teorien pekes tilbake på en person. Vurdert som akseptabelt
  fordi alternativet er å miste aldersgrupperapporten, men det er en reell
  restrisiko som bør stå i en DPIA.
- Tre operasjoner er mer å forstå enn én slettknapp, både for den ansatte og
  for neste utvikler.
- Ingenting logger hvem som anonymiserte hva. Systemet har ingen
  revisjonslogg, og for en irreversibel operasjon på et barns opplysninger er
  det et hull som bør tettes før produksjon.
