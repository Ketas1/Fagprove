# Domenemodell

Dette dokumentet beskriver begrepene systemet er bygget på, tilstandene de kan ha
og reglene som gjelder mellom dem. Det fysiske databaseskjemaet er beskrevet i
[`04-databasedesign.md`](./04-databasedesign.md).

Modellen er utledet fra arbeidsflyten for ansatte, som er beskrevet lenger ned i
dokumentet.

## Entiteter

```mermaid
erDiagram
    GUARDIAN ||--o{ BORROWER : "er ansvarlig for"
    BORROWER ||--o{ LOAN : "har"
    EQUIPMENT ||--o{ LOAN : "lånes ut i"
    LOAN ||--o{ LOAN_PHOTO : "dokumenteres av"
    LOAN ||--o{ CONTACT_ATTEMPT : "følges opp med"
    BORROWER ||--o{ NOTE : "har"
    BORROWER ||--o{ BAN : "kan være omfattet av"
    STAFF ||--o{ LOAN : "registrerer"
    EQUIPMENT_CATEGORY ||--o{ EQUIPMENT : "grupperer"
```

| Entitet | Norsk | Beskrivelse |
| --- | --- | --- |
| `Guardian` | Foresatt | Voksen som er ansvarlig for ett eller flere barn. Har kontaktinformasjon og kontaktes ved forfall. |
| `Borrower` | Barn / låntaker | Barnet som låner utstyret. Har fødselsdato og er knyttet til en foresatt. Har ikke egen innlogging. |
| `Equipment` | Utstyr | En fysisk gjenstand med internt serienummer, kategori, tilstand og status. |
| `EquipmentCategory` | Kategori | Gruppering av utstyr, brukt i rapportering (for eksempel ski, sykkel, skøyter). |
| `Loan` | Utlån | Kobling mellom et barn og et utstyr i en periode, med forventet returdato. |
| `LoanPhoto` | Bilde | Bilde av utstyret, tatt før utlån og ved retur. Dokumentasjonsgrunnlag. **Ikke bygget ennå** - krever et valg av lagringsløsning (blob/bøtte) som ikke er tatt. Se [`04-databasedesign.md`](./04-databasedesign.md). |
| `ContactAttempt` | Kontaktforsøk | Ett forsøk på å nå foresatt om et forfalt lån, med dato, metode og resultat. |
| `Note` | Notat | Fritekstnotat som ansatt skriver om en låntaker. |
| `Ban` | Utestengelse | En periode der låntakeren ikke får låne, med årsak og eventuell gebyrbetaling. |
| `Staff` | Ansatt | Ansatt i butikken. Knyttet til en Auth0-bruker. |

### Sentrale felter

**`Borrower`**

| Felt | Type | Merknad |
| --- | --- | --- |
| `DateOfBirth` | dato | Brukes til aldersgruppe i rapporter og til å sjekke 3-18 år. Fødselsnummer lagres aldri. |
| `LateReturnCount` | heltall | Antall ganger levert etter frist. Økes ved hver forsene retur. |
| `IsUnreliable` | boolsk | Markering av at låntakeren har levert for sent tidligere. |
| `GuardianId` | fremmednøkkel | Påkrevd. Et barn uten foresatt kan ikke låne. |

**`Loan`**

| Felt | Type | Merknad |
| --- | --- | --- |
| `StartedAt` | tidspunkt | Når utlånet ble registrert. |
| `DueDate` | tidspunkt | Forventet returdato. |
| `ReturnedAt` | tidspunkt, valgfri | Tom så lenge lånet er aktivt. |
| `DaysLate` | heltall | Antall dager for sent, beregnet ved retur. |
| `Status` | enum | Se tilstandsmaskinen under. |

## Tilstandsmaskiner

De tre tilstandsmaskinene er systemets viktigste invarianter. En tilstandsendring
som ikke følger en pil under er en feil.

### Lånestatus (`LoanStatus`)

```mermaid
stateDiagram-v2
    [*] --> Active : utlån registrert
    Active --> Overdue : forventet returdato passert (automatisk)
    Active --> Returned : utstyr levert i tide
    Overdue --> Returned : utstyr levert etter frist
    Overdue --> Lost : tap eller skade bekreftet
    Returned --> [*]
    Lost --> [*]
```

| Tilstand | Norsk | Betydning |
| --- | --- | --- |
| `Active` | Aktivt | Utlånet løper, fristen er ikke passert. |
| `Overdue` | Forfalt | Fristen er passert og utstyret er ikke levert. Blokkerer nye utlån. |
| `Returned` | Levert | Utstyret er levert tilbake. |
| `Lost` | Tapt | Utstyret er bekreftet tapt eller ødelagt, og saken er flagget for erstatning. |

### Utstyrstatus (`EquipmentStatus`)

```mermaid
stateDiagram-v2
    [*] --> Available : utstyr registrert
    Available --> OnLoan : utlån registrert
    OnLoan --> Available : levert i god stand
    OnLoan --> OutOfService : levert skadet
    OnLoan --> WrittenOff : tap bekreftet
    OutOfService --> Available : reparert
    WrittenOff --> [*]
```

| Tilstand | Norsk | Betydning |
| --- | --- | --- |
| `Available` | Ledig | Kan lånes ut. |
| `OnLoan` | Utlånt | Er ute på et aktivt lån. |
| `OutOfService` | Ut av drift | Skadet, må repareres før det kan lånes ut igjen. |
| `WrittenOff` | Tapt / avskrevet | Tapt eller kassert. Kan ikke lånes ut. |

### Låntakerstatus

```mermaid
stateDiagram-v2
    [*] --> Active
    Active --> Active : levert etter frist<br/>(IsUnreliable = true, LateReturnCount + 1)
    Active --> Banned : eskalering
    Banned --> Active : gebyr betalt i butikken
```

Merk skillet: **upålitelig** er en markering, ikke en tilstand. En upålitelig
låntaker får fortsatt låne, men ansatte ser advarselen. En **utestengt** låntaker
får ikke låne i det hele tatt. De to må ikke slås sammen.

## Arbeidsflyt for ansatte

Diagrammet under viser hele arbeidsflyten ansatte går gjennom i systemet.

```mermaid
flowchart TD
    RB["Registrer bruker<br/>barn og foresatt"]
    RU["Registrer utstyr"]
    RL["Registrer utlån"]
    OK["Utstyr levert<br/>status: Returned"]
    FF["Forfalt lån<br/>(automatisk)"]
    KF["Kontakt foresatt"]
    RS["Retur, levert etter frist"]
    MR["Manglende respons"]
    TS["Tap eller skade bekreftet"]
    ES["Eskalering / utestengelse"]
    FU["Fjerne utestengelse"]

    RB --> RL
    RU --> RL
    RL --> OK
    RL --> FF
    FF --> KF
    KF --> RS
    KF --> MR
    KF --> TS
    MR --> ES
    TS --> ES
    ES --> FU
```

Hva hvert steg registrerer:

| Steg | Registreres |
| --- | --- |
| **Registrer bruker** | Kontaktinformasjon til foresatt, barnets navn og fødselsdato, kobling mellom barn og foresatt. |
| **Registrer utstyr** | Navn, kategori, tilstand, status, internt serienummer. |
| **Registrer utlån** | Velg barn og utstyr som er ledig. Sjekk for åpent forfalt lån eller markering. Starttidspunkt og forventet returdato. Bilde av utstyret før utlån. Utstyrstatus settes til `OnLoan`. |
| **Forfalt lån** | Automatisk sjekk om forventet returdato er passert. Lånestatus settes til `Overdue` og lånet markeres i dashbordet. |
| **Kontakt foresatt** | E-post eller telefon om at utstyret må leveres, eller at det må sies fra hvis noe er ødelagt. Kontaktforsøk logges med dato, metode og resultat. |
| **Retur etter frist** | Bilde av utstyret ved retur, returtidspunkt, antall dager for sent. `LateReturnCount` økes og låntakeren markeres som upålitelig. Utstyrstatus settes ut fra tilstand. |
| **Manglende respons** | Nytt kontaktforsøk registreres, antall mislykkede forsøk telles, saken sendes til eskalering. |
| **Tap eller skade bekreftet** | Lånestatus `Lost`, utstyrstatus `WrittenOff`, saken flagges for erstatning. |
| **Eskalering** | Låntaker settes til `Banned`, årsak registreres som notat, nye utlån blokkeres. |
| **Fjerne utestengelse** | Gebyr registrert betalt i butikken, utestengelsen oppheves. |

## Forretningsregler

Reglene håndheves i domenelaget, ikke i grensesnittet.

1. **Foresatt er påkrevd.** Et barn kan ikke låne før en foresatt er registrert og
   koblet til barnet.
2. **Blokkering av nye utlån.** Et utlån avvises dersom låntakeren har minst ett
   lån med status `Overdue`, eller er utestengt. Dette er kjernemekanismen i
   løsningen på problem 1.
3. **Utstyret må være ledig.** Kun utstyr med status `Available` kan lånes ut.
4. **Alder.** Låntakeren må være mellom 3 og 18 år på utlånstidspunktet.
5. **Bilde ved utlån og retur.** Begge deler er dokumentasjonsgrunnlag for
   erstatningskrav og utestengelser.
6. **Kontaktforsøk logges alltid** med dato, metode og resultat, slik at ansatte
   ser om foresatt allerede er kontaktet.
7. **Forfall oppdages automatisk.** Ansatte skal aldri måtte sjekke manuelt.
8. **Opphevelse av utestengelse krever registrert gebyrbetaling.**

Regel 2 og regel 7 er de to som løser hovedproblemet systemet er laget for, og
er de viktigste å enhetsteste.

## Rapportering

| Rapport | Grunnlag |
| --- | --- |
| Antall utlån i en periode | `Loan.StartedAt` innenfor perioden |
| Utlån fordelt på aldersgruppe | `Borrower.DateOfBirth` mot `Loan.StartedAt` |
| Mest utlånte utstyr | Antall `Loan` per `Equipment` og per `EquipmentCategory` |
| Forsene leveringer | `Loan.DaysLate > 0` |
| Uleverte lån | `Loan.Status` er `Overdue` eller `Lost` |

Aldersgruppene er **3-6 år**, **7-12 år** og **13-18 år**. Inndelingen er avklart
med oppdragsgiver og skal brukes uendret i rapportene.

Alder regnes på utlånstidspunktet (`Loan.StartedAt`), ikke på rapporttidspunktet.
Et barn som fyller 13 år etter at lånet ble registrert telles altså i gruppen det
tilhørte da lånet skjedde, slik at historiske tall ikke endrer seg over tid.

Alle rapporter er aggregerte og viser ikke enkeltpersoner, se
[`09-lover-og-regler.md`](./09-lover-og-regler.md).

## Ordliste norsk til engelsk

Koden bruker engelske navn, grensesnittet norske. Denne tabellen er fasiten, og
er speilet i `CLAUDE.md`.

| Norsk | Engelsk i kode |
| --- | --- |
| Utlån | `Loan` |
| Utstyr | `Equipment` |
| Kategori | `EquipmentCategory` |
| Barn / låntaker | `Borrower` |
| Foresatt | `Guardian` |
| Ansatt | `Staff` |
| Lånestatus | `LoanStatus` |
| Utstyrstatus | `EquipmentStatus` |
| Forventet returdato | `DueDate` |
| Returtidspunkt | `ReturnedAt` |
| Aktivt | `Active` |
| Forfalt | `Overdue` |
| Levert | `Returned` |
| Ledig | `Available` |
| Utlånt | `OnLoan` |
| Ut av drift | `OutOfService` |
| Tapt / avskrevet | `Lost` / `WrittenOff` |
| Upålitelig | `IsUnreliable` |
| Antall dager for sent | `DaysLate` |
| Utestengt | `Banned` |
| Utestengelse | `Ban` |
| Gebyr | `Ban.FeePaidAt` (felt, ikke egen type) |
| Kontaktforsøk | `ContactAttempt` |
| Notat | `Note` |
| Tilstand | `EquipmentCondition` |
| Låntakerstatus | `BorrowerStatus` |
| Serienummer | `SerialNumber` |
| Rapport | `Report` |
| Kommune | `Municipality` |
