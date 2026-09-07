# Lover og regler

Systemet behandler personopplysninger om **barn mellom 3 og 18 år** og deres
foresatte. Det gjør personvern til et sentralt designhensyn, ikke noe som legges
på til slutt.

## Regelverk som gjelder

| Regelverk | Relevans |
| --- | --- |
| **Personvernforordningen (GDPR)** | Hovedregelverket. Gjelder all behandling av personopplysninger. |
| **Personopplysningsloven (2018)** | Gjennomfører GDPR i norsk rett. |
| **Bokføringsloven** | Kan gjelde registrering av innbetalt gebyr, dersom det regnes som en transaksjon i regnskapet. Avklares med oppdragsgiver. |

Barns personopplysninger har et særlig vern. Fortalepunkt 38 i GDPR slår fast at
barn fortjener særskilt beskyttelse fordi de er mindre bevisste på risikoen ved
behandling av egne opplysninger.

## Roller

| Rolle | Hvem |
| --- | --- |
| Behandlingsansvarlig | Sport For Alle AS - bestemmer formål og midler for behandlingen. |
| Databehandler | Auth0 (identitetshåndtering) og EmailJS (oppfølgings-e-post til foresatt). Krever databehandleravtale med begge. |
| Registrerte | Barn (låntakere), foresatte og ansatte. |

Auth0 skal settes opp med **EU-region** slik at persondata om innloggede brukere
lagres innenfor EU/EØS. Se [ADR-0006](./adr/0006-auth0.md).

**EmailJS**, lagt til 2026-09-07 (se [ADR-0022](./adr/0022-emailjs-server-side.md)),
publiserer en egen databehandleravtale
(<https://www.emailjs.com/legal/data-protection-agreement/>), men hvor
tjeneren/dataen faktisk befinner seg er **ikke bekreftet** - avklares med
EmailJS og signeres før systemet driftes for reelle brukere, samme
behandling som andre åpne punkter i dette dokumentet. Personopplysningene
som faktisk sendes ut av systemet i én e-post er begrenset til det
malen bruker: foresattes navn og e-post, barnets fornavn, utstyrsnavn og
forfallsdato - ikke mer enn det som allerede er begrunnet i tabellen under
"Dataminimering".

## Behandlingsgrunnlag

Behandlingen bygger på GDPR artikkel 6:

| Behandling | Grunnlag |
| --- | --- |
| Registrering av barn, foresatt og utlån | Art. 6 (1) b - nødvendig for å oppfylle avtalen om utlån med foresatt. |
| Oppfølging av forfalte lån, kontaktforsøk | Art. 6 (1) b og f - oppfyllelse av avtalen, og berettiget interesse i å få utstyret tilbake. |
| Bilder av utstyr som dokumentasjon | Art. 6 (1) f - berettiget interesse i å dokumentere tilstand ved erstatningskrav. |
| Rapportering til kommunen | Art. 6 (1) e - oppgave i allmennhetens interesse. Rapportene er aggregerte og inneholder ikke personopplysninger. |

Foresatt skal informeres om behandlingen ved registrering, jf. artikkel 13.

## Dataminimering: hva som lagres og hvorfor

Artikkel 5 (1) c krever at opplysningene er **adekvate, relevante og begrenset**
til det som er nødvendig. Hvert felt i systemet skal kunne begrunnes.

| Opplysning | Formål | Vurdering |
| --- | --- | --- |
| Barnets navn | Identifisere hvem som har lånt utstyret | Nødvendig |
| Barnets fødselsdato | Kontrollere aldersgrensen 3-18 år, og aldersgruppe i rapporter | Nødvendig. Kun dato, ikke fødselsnummer. |
| Foresatts navn | Vite hvem som er ansvarlig | Nødvendig |
| Foresatts e-post og telefon | Kontakt ved forfalt lån | Nødvendig |
| Utlånshistorikk | Håndheve blokkering, og vurdere pålitelighet | Nødvendig |
| `LateReturnCount`, `IsUnreliable` | Grunnlag for oppfølging og eventuell utestengelse | Nødvendig, men se avsnittet om profilering |
| Notater om låntaker | Dokumentere oppfølging og årsak til utestengelse | Nødvendig, men se avsnittet om notater |
| Bilder av utstyr | Dokumentere tilstand før og etter utlån | Nødvendig |

### Dette lagres bevisst ikke

- **Fødselsnummer.** Systemet har ikke behov for å identifisere personer på
  tvers av offentlige registre. Fødselsdato er tilstrekkelig for både
  aldersgrense og rapportering. Dette er den viktigste enkeltavgrensningen i
  personvernet for systemet.

  Bygget 2026-09-07: `Guardian.IdentityVerifiedAt` er mekanismen som erstatter
  behovet for å lagre fødselsnummer som identitetsbevis. Ansatt kan se
  foresattes legitimasjon fysisk i butikken ved registrering og krysse av for
  det - systemet lagrer da kun *at* og *når* dette skjedde
  (`IdentityVerifiedAt`), aldri ID-dokumentet, nummeret eller noe bilde av det.
  Feltet er valgfritt og blokkerer ikke registrering - det er et hjelpemiddel
  for ansatte, ikke en ny forretningsregel.
- **Bilder av barn.** Det tas bilde av *utstyret*, aldri av låntakeren.
- **Helseopplysninger** eller andre særlige kategorier etter artikkel 9.
- **Personopplysninger i applikasjonslogger.** Logger inneholder identifikatorer
  (ID-er), ikke navn, e-postadresser eller notattekst.

## Notater om låntakere

Notatfeltet er den største personvernrisikoen i systemet, fordi det er fritekst
og fordi det handler om barn.

Retningslinjer som skal gjelde, og som synliggjøres i grensesnittet:

- Notater skal være **saklige og faktabaserte**: hva som skjedde, når, og hva som
  ble avtalt. Ikke vurderinger av barnet eller familien.
- Notater er personopplysninger. Den registrerte har rett til innsyn i dem, og
  de kan bli lest av personen det gjelder.
- Notater knyttet til utestengelse skal begrunne vedtaket, ikke karakterisere
  personen.

## Profilering og automatiserte avgjørelser

Systemet blokkerer automatisk nye utlån for låntakere med åpent forfalt lån eller
utestengelse, og markerer låntakere som upålitelige basert på historikk. Dette
grenser mot artikkel 22 om automatiserte individuelle avgjørelser.

Vurdering og tiltak:

- Blokkeringen er en direkte konsekvens av et faktisk forhold (et lån er ikke
  levert), ikke en score eller en prediksjon.
- **Ansatte kan overstyre.** En utestengelse settes og oppheves av et menneske,
  ikke av systemet. Systemet foreslår og synliggjør - det avgjør ikke alene.
- Årsaken skal alltid være synlig for den ansatte, slik at avgjørelsen kan
  forklares til foresatt.
- Markeringen "upålitelig" skal presenteres sammen med tallgrunnlaget (antall
  forsene leveringer), ikke som en frittstående merkelapp.

## Lagringsbegrensning og sletting

Artikkel 5 (1) e krever at opplysninger ikke lagres lenger enn nødvendig.
Følgende rutine foreslås og avklares med oppdragsgiver:

| Data | Foreslått lagringstid |
| --- | --- |
| Aktive låneforhold | Så lenge forholdet er aktivt |
| Avsluttede utlån | 2 år, som grunnlag for historikk og rapportering |
| Bilder av utstyr | 6 måneder etter at lånet er avsluttet uten tvist |
| Notater og kontaktforsøk | Slettes sammen med utlånet de hører til |
| Låntakere uten aktivitet | Slettes eller anonymiseres etter 3 år |

Rapporteringsgrunnlag beholdes i **anonymisert** form, slik at kommunen fortsatt
kan følge utviklingen uten at personopplysninger bevares.

## De registrertes rettigheter

Foresatte kan på vegne av barnet kreve:

| Rettighet | Artikkel | Hvordan systemet støtter det |
| --- | --- | --- |
| Innsyn | 15 | Ansatt kan hente ut alle registrerte opplysninger om en låntaker |
| Retting | 16 | Ansatt kan rette feil i navn, fødselsdato og kontaktinformasjon |
| Sletting | 17 | Sletting av låntaker med tilhørende data, når det ikke finnes aktive lån |
| Dataportabilitet | 20 | Eksport av opplysningene om en låntaker |
| Protest | 21 | Håndteres manuelt av oppdragsgiver |

## Informasjonssikkerhet

Artikkel 32 krever egnede tekniske og organisatoriske tiltak. Konkret i dette
systemet:

- All tilgang krever innlogging. Ingen data er tilgjengelig uautentisert.
- Autorisasjon kontrolleres **på hvert endepunkt i backend**, ikke bare i
  grensesnittet.
- Rollene `User`, `Staff` og `Admin` gir tilgang etter behov.
- Hemmeligheter ligger i miljøvariabler, ikke i koden.
- Trafikk går over HTTPS i produksjon.
- Bilder lagres slik at de ikke er tilgjengelige via en gjettbar URL.

Se [`08-sikkerhet.md`](./08-sikkerhet.md) for tiltakene i detalj.

## Personvernkonsekvensvurdering (DPIA)

Behandlingen gjelder barn, og innebærer en form for systematisk vurdering av
adferd. Det taler for at en forenklet DPIA etter artikkel 35 bør gjennomføres
før systemet settes i produksjon. Det ligger utenfor omfanget av dette forslaget,
men er dokumentert her som en anbefaling til oppdragsgiver.

## Fremtidig vurdering: samtykke til erstatningsansvar

Vurdert 2026-09-07 og bevisst **ikke bygget** i dette forslaget: at foresatt,
ved registrering av utlån, krysser av for å ha akseptert et vilkår om at de er
økonomisk ansvarlige for skade på utstyret, og skal dekke reparasjon eller
erstatning ved behov.

Dette er **ikke en erstatning for bilder av utstyret** (forretningsregel 5 i
`03-domenemodell.md`), og skal ikke bygges som en. Et avkrysningsfelt gir
butikken en avtalemessig rett til å kreve betaling - det sier ingenting om
*når* skaden oppsto. Det er fortsatt `EquipmentCondition` registrert av ansatt
ved utlån og retur, styrket av bilder, som skal avgjøre om skaden skjedde i
låneperioden. Uten det bevisgrunnlaget avgjøres enhver uenighet til butikkens
fordel per definisjon, noe som er vanskelig å forsvare overfor familier i en
kommunalt finansiert ordning for barn.

Grunnen til at feltet likevel ikke bygges nå: en avkrysning som gir inntrykk
av en bindende avtale forutsetter en faktisk vilkårstekst, gjennomgått av
kommunens jurist - ikke tekst skrevet av utvikleren av et forslag som per
CLAUDE.md eksplisitt ikke skal fremstå som produksjonsklart. Å bygge
funksjonen uten den gjennomgangen ville gitt systemet en falsk fremstilling av
juridisk bindende kraft.

**Anbefaling til oppdragsgiver:** vurder en slik samtykkemekanisme som et
tillegg til - ikke en erstatning for - bildedokumentasjon, når reelle vilkår
er utarbeidet og godkjent juridisk.

## Oppsummering av designvalg begrunnet i personvern

1. Fødselsdato i stedet for fødselsnummer.
2. Bilder av utstyr, aldri av barn.
3. Ingen personopplysninger i logger.
4. Aggregerte rapporter til kommunen.
5. Utestengelse settes av et menneske, ikke automatisk av systemet.
6. Auth0 i EU-region.
7. Definert lagringstid med sletting og anonymisering.
8. Oppfølgings-e-post sendes fra backend, aldri fra nettleseren - EmailJS sin
   hemmelige nøkkel eksponeres da aldri i klientkode.
