# Løsningsbeskrivelse

## Om oppdragsgiver

Sport For Alle AS er en butikk som låner ut sportsutstyr til barn mellom 3 og 18
år. Ordningen finansieres av kommunen, og målet er at barn skal kunne drive med
idrett uavhengig av familiens økonomi.

I dag drives utlånet manuelt på papir. Det er dette systemet skal erstatte.

## Mål med løsningen

Hovedmålet er å lage et forslag til en nettbasert løsning som bidrar til
digitalisering og effektivisering av den daglige driften, og som løser tre
konkrete problemer hos oppdragsgiver.

I tillegg skal løsningen være:

- Dokumentert gjennom hele utviklingen: tekniske valg, arkitekturdesign,
  databasedesign, backend/API, testing, sikkerhet, relevante lover og regler, og
  bruk av systemet.
- Brukervennlig, intuitiv, sikker, strukturert og moderne.
- Enkel for en annen utvikler å forstå, drifte og videreutvikle.

## Aktører

| Aktør | Rolle i systemet | Beskrivelse |
| --- | --- | --- |
| **Ansatt** | `Staff` | Jobber i butikken. Registrerer utstyr, brukere og utlån, følger opp forfalte lån, og henter ut rapporter og statistikk til kommunen. |
| **Foresatt** | `User` | Ansvarlig for barnet. Må registreres før barnet kan låne utstyr, og er den som kontaktes hvis utstyr ikke leveres tilbake. |
| **Barn (3-18 år)** | - | Den som låner utstyret. Registreres av ansatt og knyttes til en foresatt. Har ikke egen innlogging. |
| **Administrator** | `Admin` | Utvidede rettigheter, blant annet brukeradministrasjon. |

Systemet brukes primært av ansatte. Foresatt og barn er registrerte parter i
utlånet, ikke daglige brukere av grensesnittet.

## Problem 1: Utstyr blir ikke levert tilbake, eller ikke i tide

### Hvorfor problemet oppstår

- Dagens system er manuelt og på papir. Det finnes ingen måte å oppdage at et lån
  har passert leveringsfristen på - ansatte må sjekke det for hånd.
- Det er ingen konsekvens for kunder som leverer for sent. De kan låne nytt
  utstyr selv om de ikke har levert tilbake det gamle.
- Det finnes ingen god logg over hvem som har lånt hva, når det ble lånt, eller
  om låneren har god eller dårlig historikk med å levere tilbake.
- Oppfølging blir ikke logget. Det er derfor vanskelig å vite om en foresatt
  allerede har blitt kontaktet om ulevert utstyr.

**Konsekvens:** utstyr som ikke leveres tilbake må erstattes, og det koster penger.

### Hvordan systemet løser det

- **Automatisk oppdagelse av forfall.** Systemet setter selv lånestatus til
  `Overdue` når forventet returdato passeres, og markerer lånet i dashbordet.
  Ansatte trenger ikke huske å sjekke.
- **Blokkering av nye utlån.** En bruker med et åpent forfalt lån får ikke låne
  mer. Dette er insentivet som får utstyret tilbake, og er den viktigste enkelte
  forretningsregelen i systemet.
- **Historikk per bruker.** Systemet teller hvor mange ganger en bruker har
  levert for sent og hvor mange dager for sent, og markerer brukeren som
  upålitelig. Ansatte ser historikken før de registrerer et nytt utlån.
- **Utestengelse.** Ved gjentatte brudd eller ødelagt utstyr kan brukeren
  utestenges midlertidig eller permanent. Utestengelsen fjernes når et gebyr er
  betalt i butikken.
- **Dokumentert grunnlag.** Det tas bilde av utstyret både før utlån og ved
  retur, slik at en utestengelse eller et erstatningskrav har et dokumentert
  grunnlag.
- **Oppfølging via e-post.** Ansatte kan sende e-post til foresatt fra systemet
  når et lån er forfalt, med beskjed om at utstyret må leveres, at det må sies
  fra hvis noe er ødelagt, og at brukeren kan bli utestengt. Hvert kontaktforsøk
  logges med dato, metode og resultat.

## Problem 2: Ingen samlet oversikt over utstyr, frister og brukere

### Hvorfor problemet oppstår

All informasjon om utstyr, lån og kunder ligger i lister og på papir i stedet for
i ett system. Det blir fort uoversiktlig, og desto verre jo mer utstyr butikken
har og jo mer populær ordningen blir. Oppdatering gjøres for hånd, noe som gjør
det lett å gjøre feil, og det finnes ingen automatikk.

**Konsekvens:** ansatte bruker mye tid på å lete etter informasjon i stedet for å
hjelpe kunder.

### Hvordan systemet løser det

En relasjonsdatabase der alle data lagres strukturert, med et grensesnitt som gir
ansatte oversikt over:

- alt utstyr og status på hvert enkelt (ledig, utlånt, ut av drift, tapt)
- alle registrerte brukere, med kobling mellom barn og foresatt
- alle aktive og forfalte utlån, med forventet returdato
- kontaktinformasjon til foresatt direkte fra det forfalte lånet
- notater og historikk per bruker

Fordi dataene ligger strukturert, kan systemet svare på spørsmål som "hvilke lån
er forfalt akkurat nå" umiddelbart, i stedet for at noen må lete gjennom papirer.

## Problem 3: Ingen statistikk eller rapportering

### Hvorfor problemet oppstår

Å få ut statistikk om utlån og engasjement krever i dag mye manuelt arbeid. Uten
en database kan man ikke gjøre spørringer som enkelt gir "antall utlån siste
måned" eller hvilket utstyr som er mest populært.

**Konsekvens:** kommunen som finansierer ordningen har ikke godt nok grunnlag for
å vurdere om den fungerer, eller om den er verdt å drifte videre.

### Hvordan systemet løser det

En rapportside med **to** rapporter. Begge viser de samme fem tallene, og de
fire underradene summerer seg alltid til totalen:

| Rad | Hva den teller |
| --- | --- |
| Utlån totalt | Alle utlån registrert i perioden |
| Levert i tide | Levert innen fristen |
| Levert for sent | Levert etter fristen |
| Ikke levert | Forfalt eller bekreftet tapt |
| Fortsatt aktive | Løper fortsatt, og har ikke forfalt |

1. **Utlån i perioden** - de fem tallene for perioden den ansatte velger.
2. **Utlån per aldersgruppe** - de samme fem tallene fordelt på 3-7, 8-12 og
   13-18 år.

I tillegg vises utviklingen over tid som et søylediagram: antall utlån per dag,
uke eller måned. Begge rapportene kan lastes ned som Excel eller PDF, slik at
kommunen får tallene i et format de kan arkivere og regne videre på.

**Mest utlånte utstyr var opprinnelig med i denne listen, men er tatt ut.**
Oppdragsgiver trenger å vite hvor mange utlån som er gjort, ikke hva som ble
lånt ut. Endringen ble avklart 2026-09-07.

Rapportene er aggregerte. De viser tall og trender, ikke enkeltpersoner - se
[`09-lover-og-regler.md`](./09-lover-og-regler.md). Eksportfilene inneholder av
samme grunn bare disse tallene: ingen navn, ingen id-er, ingen enkeltlån.

Rapportsiden er bygget 2026-09-07 (`/dashboard/reports`), koblet til de ekte
endepunktene. Se [`05-api.md`](./05-api.md) og
[ADR-0023](./adr/0023-rapporteksport.md) for hvordan eksporten fungerer.

## Avgrensninger

Følgende er bevisst utenfor omfanget av denne løsningen:

- Foresatte har ikke selvbetjening. All registrering gjøres av ansatt.
- Betaling av gebyr skjer i butikken og registreres manuelt i systemet. Det er
  ingen betalingsintegrasjon.
- Systemet settes ikke i produksjon. Det kjøres lokalt med Docker.
- Reservasjon av utstyr på forhånd er ikke en del av løsningen.

## Videre lesning

- Arkitekturen som realiserer dette: [`02-arkitektur.md`](./02-arkitektur.md)
- Domenemodellen og de presise reglene: [`03-domenemodell.md`](./03-domenemodell.md)
- Begrunnelsen for hvert teknologivalg: [`adr/`](./adr/)
