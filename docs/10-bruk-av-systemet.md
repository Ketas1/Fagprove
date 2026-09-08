# Bruk av systemet

Brukerveiledning for ansatte i butikken. Den forutsetter ingen teknisk
bakgrunn - les den fra toppen første gang, og slå opp i den etterpå.

Systemet erstatter papirskjemaene. Alt du før skrev ned for hånd - hvem som har
lånt hva, når det skal leveres, hvem du har ringt - registreres her, og
systemet passer på fristene for deg.

**Innhold**

| Oppgave | Gå til |
| --- | --- |
| Logge inn | [Logge inn](#logge-inn) |
| Se hva som haster i dag | [Oversikten](#oversikten) |
| Registrere et nytt barn | [Registrere barn og foresatt](#registrere-barn-og-foresatt) |
| Registrere nytt utstyr | [Registrere utstyr](#registrere-utstyr) |
| Låne ut noe | [Registrere et utlån](#registrere-et-utlån) |
| Se hvem som har hva | [Se aktive utlån](#se-aktive-utlån) |
| Purre på et lån | [Følge opp forfalte utlån](#følge-opp-forfalte-utlån) |
| Ta imot utstyr tilbake | [Registrere en retur](#registrere-en-retur) |
| Utestenge noen | [Utestenge og oppheve utestengelse](#utestenge-og-oppheve-utestengelse) |
| Hente ut tall til kommunen | [Rapporter](#rapporter) |

> **Status:** teksten er komplett per 2026-09-08. Skjermbildene mangler
> fortsatt - se [Skjermbilder som skal legges inn](#skjermbilder-som-skal-legges-inn)
> nederst. Det som ennå ikke virker, står i
> [Det som ikke er bygget ennå](#det-som-ikke-er-bygget-ennå), slik at du ikke
> leter etter knapper som ikke finnes.

---

## Logge inn

**[Skjermbilde 1]**

1. Åpne systemet i nettleseren.
2. Klikk **Logg inn**.
3. Logg inn med arbeidskontoen din.

Du blir sendt til en innloggingsside som driftes av Auth0, ikke av systemet
selv. Passordet ditt lagres altså ikke i utlånssystemet.

Første gang må profilen din som ansatt kobles til kontoen din. Får du ikke
tilgang til noe etter innlogging, er det som regel dette som mangler - se
**Ansatte** i menyen, eller spør en kollega som allerede er inne.

Får du meldingen **«Innlogging feilet»**, prøv på nytt. Fortsetter det, er det
et teknisk problem, ikke noe du har gjort feil.

Hele systemet er låst bak innlogging. Ingen kan se opplysninger om barn uten å
være logget inn.

---

## Oversikten

Det første du ser etter innlogging. Den er laget for å svare på ett spørsmål:
**hva må jeg gjøre noe med i dag?**

**[Skjermbilde 2]**

Øverst står fire tall:

| Tall | Betyr |
| --- | --- |
| **Aktive utlån** | Utstyr som er ute hos noen nå |
| **Forfalte utlån** | Utstyr som skulle vært levert. Krever oppfølging |
| **Ledig utstyr** | Hvor mye som kan lånes ut, av totalen |
| **Utestengte låntakere** | Barn som ikke får låne før gebyret er betalt |

Under står **Utlån som krever oppfølging** - alle forfalte lån, med hvor lenge
de har vært forfalt. Klikk på navnet for å åpne lånet og følge det opp.

Til venstre ligger menyen: Oversikt, Utlån, Utstyr, Barn og foresatte,
Rapporter, Ansatte og Logg.

---

## Registrere barn og foresatt

**Et barn kan ikke låne før en foresatt er registrert og knyttet til barnet.**
Det er ikke en teknisk begrensning - det er fordi den foresatte er den vi
kontakter hvis utstyret ikke kommer tilbake. Derfor gjøres begge deler i samme
skjema.

**[Skjermbilde 3]**

1. Gå til **Barn og foresatte** i menyen.
2. Klikk **Registrer barn**.
3. Fyll ut under **Barn**:
   - **Navn**
   - **Fødselsdato** - velges i en kalender. Brukes til å sjekke at barnet er
     mellom 3 og 18 år, og til aldersgruppene i rapportene til kommunen.
4. Under **Foresatt**, velg ett av to:
   - **Ny foresatt** - fyll inn **Navn**, **Telefon** og **E-post**.
   - **Eksisterende foresatt** - søk opp en foresatt som allerede er
     registrert. Bruk denne når du registrerer søsken.
5. Klikk **Registrer barn**.

### Om «Identitet bekreftet»

Når du oppretter en ny foresatt, kan du krysse av for **Identitet bekreftet
(ID vist i butikken)**. Det er frivillig.

Avkrysningen erstatter det å skrive ned fødselsnummer. **Systemet lagrer aldri
fødselsnummer**, og du skal aldri skrive det inn i et notat heller. Har du sett
legitimasjon i butikken, krysser du av - det er dokumentasjonen.

### Vanlige feil her

| Melding | Hva den betyr |
| --- | --- |
| Alder utenfor 3-18 år | Ordningen gjelder barn mellom 3 og 18. Kontroller fødselsdatoen |
| Skjemaet lar seg ikke sende | Feltene med rød stjerne mangler innhold |

---

## Registrere utstyr

**[Skjermbilde 4]**

1. Gå til **Utstyr**.
2. Klikk **Nytt utstyr**.
3. Fyll ut:
   - **Navn** - vær konkret, for eksempel «Snowboard, str. 155». Det er dette
     du senere leter etter i utlånsskjemaet.
   - **Serienummer** - butikkens eget merkenummer, for eksempel «SF-0411».
     Ikke produsentens.
   - **Kategori** - søk den opp. Finnes den ikke, skriv navnet, og velg
     **Opprett «…»** nederst i listen for å lage den med én gang.
   - **Tilstand** - hvilken stand utstyret er i når det kommer inn i systemet.
4. Klikk for å lagre.

Nytt utstyr blir automatisk **Ledig** og kan lånes ut med en gang.

### Kategorier

Til venstre på utstyrssiden ligger kategoritreet. Kategorier kan ligge inni
hverandre - «Ski» kan ha «Slalåm» og «Langrenn» under seg. Klikk på en kategori
for å se bare utstyret i den; klikk **Alt utstyr** for å se alt igjen.

---

## Registrere et utlån

**[Skjermbilde 5]**

1. Gå til **Utlån** og klikk **Nytt utlån**.
2. **Låntaker** - søk opp barnet.
3. **Utstyr** - søk opp gjenstanden. Listen viser **bare ledig utstyr**,
   gruppert etter kategori, så du kan ikke låne ut noe som allerede er ute.
4. **Forventet returdato** - velg dato i kalenderen.
5. Klikk **Registrer utlån**.

Utstyret settes til **Utlånt** med en gang, og lånet får status **Aktiv**.

### Hvis utlånet blir blokkert

Da får du en rød boks som sier **«Utlån blokkert»**, med årsaken.

Det skjer i to tilfeller:

| Årsak | Hva du gjør |
| --- | --- |
| Barnet har et **forfalt lån** som ikke er levert | Utstyret må leveres først. Registrer returen, så kan barnet låne igjen |
| Barnet er **utestengt** | Utestengelsen må oppheves, og det krever at gebyret betales i butikken |

**Dette er ikke en feil, og det kan ikke overstyres i systemet.** Sperren er
hele grunnen til at ordningen fungerer: den gjør det tydelig at utstyr må
komme tilbake før man får låne noe nytt. Forklar det til den foresatte -
teksten i boksen sier hvorfor lånet ble stoppet.

---

## Se aktive utlån

Gå til **Utlån**. Der finner du alle lån, med **Låntaker**, **Utstyr**,
**Startdato**, **Forventet retur** og **Status**.

**[Skjermbilde 6]**

Du kan velge mellom to visninger:

- **Tabell** - vanlig liste. Best når du leter etter noe bestemt.
- **Kanban** - lånene sortert i kolonner etter status. Best for å se helheten.

Filtrer på status (Aktiv, Forfalt, Levert, Tapt) eller på tidsrom (Denne uken,
Denne måneden, Alle). Søkefeltet finner både navn og serienummer - et kopiert
nummer kan limes inn med eller uten `#` foran.

Statusene betyr:

| Status | Betyr |
| --- | --- |
| **Aktiv** | Ute hos låntaker, fristen har ikke gått ut |
| **Forfalt** | Fristen har gått ut, utstyret er ikke levert |
| **Levert** | Kommet tilbake |
| **Tapt** | Bekreftet tapt eller ødelagt |

Klikk **Se detaljer** på en rad for å åpne lånet.

---

## Følge opp forfalte utlån

**Du trenger ikke lete etter forfalte lån.** Systemet setter status til
**Forfalt** av seg selv når fristen går ut, og de dukker opp på oversikten.

**[Skjermbilde 7]**

Åpne lånet fra oversikten eller fra utlånslisten. På lånesiden ligger
**Kommunikasjon med foresatt**, som viser alle kontaktforsøk som er gjort -
**så du ser om en kollega allerede har ringt**.

### Sende oppfølgings-e-post

Når lånet er forfalt, vises knappen **Send oppfølgings-e-post**. Den sender en
påminnelse til den foresatte og logger kontaktforsøket automatisk. Du trenger
ikke registrere det i tillegg.

### Registrere et kontaktforsøk manuelt

Har du ringt, eller snakket med noen i butikken, registrerer du det selv:

1. Velg **metode** - e-post eller telefon.
2. Skriv **resultatet** - hva som faktisk skjedde. «Ikke svar, la igjen
   beskjed» er nyttig. «Prøvde» er det ikke.
3. Lagre.

Dato og klokkeslett settes automatisk.

Skriv saklig og faktabasert. Dette er opplysninger om en familie, og de kan
kreve innsyn i det som står registrert om dem.

---

## Registrere en retur

**[Skjermbilde 8]**

1. Finn lånet, i utlånslisten eller på oversikten.
2. Klikk **Registrer retur**.
3. Velg **Tilstand** på utstyret slik det kom tilbake: **Ny**, **God**,
   **Slitt** eller **Skadet**.
4. Bekreft.

Tilstanden avgjør hva som skjer med utstyret:

| Tilstand du velger | Utstyret blir |
| --- | --- |
| Ny, God eller Slitt | **Ledig** - kan lånes ut igjen med en gang |
| **Skadet** | **Ut av drift** - lånes ikke ut før det er reparert |

Bare **Skadet** tar utstyret ut av drift. «Slitt» registrerer slitasjen, men
gjenstanden kan fortsatt lånes ut - velg den når utstyret er brukt, men
fullt brukbart.

### Hvis utstyret leveres for sent

Da skjer dette automatisk, uten at du gjør noe:

- Lånet får status **Levert**, og antall dager forsinket regnes ut.
- Telleren over forsinkede leveringer for barnet økes.
- Barnet markeres som **upålitelig**.

**Merk:** «upålitelig» er en markering, ikke en sperre. Et barn som er markert
upålitelig **kan fortsatt låne**. Markeringen er der for at du skal ha
bakgrunnen tilgjengelig hvis mønsteret gjentar seg. Det er bare
**utestengelse** som faktisk stopper nye lån.

---

## Notater og markeringen «upålitelig»

På siden til et barn kan du legge inn **notater**. Bruk dem til det en kollega
trenger å vite - avtaler som er gjort, forklaringer, forhold rundt et lån.

Retningslinjen står i selve skjemaet: skriv **saklig og faktabasert**. Ikke
skriv vurderinger av familien, helseopplysninger eller noe du ikke ville sagt
til dem direkte. Den foresatte har rett til innsyn i det som er registrert.

Markeringen **upålitelig** settes automatisk ved for sen levering, og teller
hvor mange ganger det har skjedd. Den fjernes ikke automatisk.

---

## Utestenge og oppheve utestengelse

Utestengelse er det sterkeste virkemiddelet. En utestengt låntaker får ikke
låne i det hele tatt.

**[Skjermbilde 9]**

### Utestenge

1. Åpne barnets side under **Barn og foresatte**.
2. Klikk **Utesteng låntaker**.
3. Skriv **årsaken**. Den blir stående, og er det du - eller en kollega - kan
   vise til senere.
4. Bekreft.

### Oppheve

Utestengelsen oppheves i to trinn, og rekkefølgen er fast:

1. **Registrer gebyr betalt** når gebyret er betalt i butikken.
2. **Opphev utestengelse** blir da mulig å klikke på.

Knappen for å oppheve er **deaktivert helt til betalingen er registrert**. Det
er med vilje: det skal ikke være mulig å oppheve en utestengelse uten at
betalingen er dokumentert.

---

## Bekrefte tap eller skade

Kommer utstyret aldri tilbake, eller er det ødelagt for godt:

1. Åpne lånet.
2. Klikk **Bekreft tap eller skade**.
3. Bekreft i dialogen som kommer opp.

**Dette kan ikke angres.** Lånet settes til **Tapt** og utstyret til
**Avskrevet** - det forsvinner ut av beholdningen. Derfor kommer det et eget
bekreftelsessteg. Er du usikker, vent og følg opp i stedet.

---

## Rapporter

Her henter du ut tallene kommunen ber om. **Alle tall er samletall** - ingen
navn, ingen enkeltbarn.

**[Skjermbilde 10]**

1. Gå til **Rapporter**.
2. Velg **periode**: Siste 30 dager, Siste 6 måneder, Siste 12 måneder, Hele
   historikken, eller **Egendefinert** med egne fra- og til-datoer.
3. Velg **Vis per**: Dag, Uke eller Måned. Dette styrer bare søylediagrammet.

Du får to rapporter, begge med de samme fem tallene:

| Tall | Betyr |
| --- | --- |
| **Utlån totalt** | Alle utlån registrert i perioden |
| **Levert i tide** | Levert innen fristen |
| **Levert for sent** | Levert etter fristen |
| **Ikke levert** | Forfalt eller bekreftet tapt |
| **Fortsatt aktive** | Fortsatt ute, fristen har ikke gått ut |

De fire nederste **summerer seg alltid til «Utlån totalt»**. Går det ikke opp,
er det en feil - meld fra.

Nederst vises de samme fem tallene fordelt på aldersgruppene **3-7**, **8-12**
og **13-18 år**. Alderen regnes ut fra da lånet ble registrert, ikke i dag, så
gamle tall endrer seg ikke når barna har bursdag.

### Laste ned

Klikk **Eksporter**:

- **Last ned Excel** - tre ark (Sammendrag, Aldersgrupper, Utvikling) med
  ekte tall og datoer, så du kan regne videre eller lage egne diagrammer.
- **Last ned PDF** - ferdig oppsett til å legge ved en rapport.

Begge filene inneholder **bare tallene** - aldri navn, aldri enkeltlån. De kan
sendes til kommunen som de er.

> Får du meldingen «Perioden gir for mange søyler med denne oppdelingen», har
> du valgt en lang periode med **Dag**. Velg **Uke** eller **Måned**.

---

## Det som ikke er bygget ennå

Ærlig liste, så du ikke leter forgjeves.

| Det du ser | Situasjonen |
| --- | --- |
| **Bilde av utstyr** i utlåns- og returskjemaet | Boksen vises, men er merket «Ikke koblet til lagring ennå». **Den lagrer ingenting.** Til den virker, må butikken selv avtale hvordan utstyr dokumenteres før utlån og ved retur |
| **Siste hendelser** på oversikten | Ikke bygget. Feltet står tomt |
| **Logg** i menyen | Siden finnes, men er tom av samme grunn |
| **Kontaktstatus** i oversiktstabellen | Viser «Ikke bygget ennå». Dette er utdatert - kontaktforsøk **virker**, du finner dem på den enkelte lånesiden |
| **Invitere en ansatt** | Finnes ikke. Hver ansatt oppretter og kobler sin egen profil |
| **Roller** (Ansatt/Administrator) | Ikke skilt fra hverandre ennå. Alle innloggede ansatte har samme tilgang |

Fotografering før utlån og ved retur er en **forutsetning i planen** - det er
bevisgrunnlaget hvis noe blir ødelagt eller ikke kommer tilbake. At det ikke
virker ennå, er den viktigste begrensningen i denne versjonen.

---

## Vanlige spørsmål

**Hvorfor får jeg ikke registrert et utlån?**
Barnet har enten et forfalt lån som ikke er levert, eller er utestengt. Boksen
«Utlån blokkert» sier hvilken av delene. Se
[Hvis utlånet blir blokkert](#hvis-utlånet-blir-blokkert).

**Barnet står ikke i listen når jeg skal låne ut.**
Er barnet registrert, og knyttet til en foresatt? Uten foresatt kan det ikke
låne. Se [Registrere barn og foresatt](#registrere-barn-og-foresatt).

**Utstyret står ikke i listen.**
Listen viser bare **ledig** utstyr. Er det allerede utlånt, ut av drift eller
avskrevet, vises det ikke. Sjekk statusen under **Utstyr**.

**Hva gjør jeg hvis utstyret kommer tilbake ødelagt?**
Registrer returen som vanlig og velg tilstanden som beskriver skaden. Utstyret
settes ut av drift i stedet for ledig. Er det ødelagt for godt, bruk
**Bekreft tap eller skade**.

**Hva gjør jeg hvis foresatt ikke svarer?**
Registrer hvert forsøk med metode og resultat. Da ser du - og kollegaene dine -
hva som allerede er prøvd, og du har dokumentasjonen hvis saken må eskaleres
til utestengelse.

**Jeg har registrert noe feil.**
Barn, foresatte og utstyr kan redigeres. Et registrert **utlån** kan ikke
slettes, fordi historikken er grunnlaget for rapporteringen til kommunen.
Skriv et notat om hva som skjedde.

**Må jeg sjekke forfalte lån manuelt?**
Nei. Systemet setter status selv, og forfalte lån vises på oversikten.

---

## Skjermbilder som skal legges inn

Teksten over er ferdig. Bildene mangler, og må tas fra det kjørende systemet
med **oppdiktede testdata** - aldri med ekte opplysninger om et barn.

Legg dem i `docs/bilder/` og sett dem inn der `**[Skjermbilde N]**` står.

| Nr | Hva som skal fotograferes |
| --- | --- |
| 1 | Innloggingssiden med «Logg inn»-knappen |
| 2 | Oversikten med de fire tallene og tabellen over forfalte lån |
| 3 | «Registrer barn»-skjemaet, med «Ny foresatt» valgt |
| 4 | «Nytt utstyr»-skjemaet, gjerne med kategorivelgeren åpen |
| 5 | «Nytt utlån»-skjemaet **og** et eget bilde av «Utlån blokkert»-boksen |
| 6 | Utlånssiden i tabellvisning, med statusfilteret synlig |
| 7 | En lånedetaljside med «Kommunikasjon med foresatt» |
| 8 | «Registrer retur»-skjemaet med tilstandsvalget |
| 9 | Barnesiden med utestengelsesfeltet |
| 10 | Rapportsiden med diagram og «Eksporter»-menyen åpen |

Bilde 5 er det viktigste: den blokkerte utlånsmeldingen er den situasjonen en
ny ansatt oftest lurer på.
