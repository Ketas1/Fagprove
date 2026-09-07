# ADR-0021: Hierarkiske utstyrskategorier, uhåndhevet dybde, ett-siders trevisning

- **Status:** Akseptert
- **Dato:** 2026-09-06

## Kontekst

Utstyrskategorier var flate (`Id`, `Name`, unikt globalt) og utstyrssiden i
frontend viste én flat tabell med et nedtrekksfelt for kategori. Med flere
titalls kategorier ble det vanskelig å finne fram, og det var ikke noen egen
side for å se og administrere kategoriene selv.

Ønsket var underkategorier - "sport-tilbehør" kan ha "baller" og "hjelmer"
under seg - men hvor dypt, hvordan navngiving skal fungere på tvers av grener,
og hvor tungt selve grensesnittet skal bygges var ikke avklart før valget
under ble tatt i samtale.

## Beslutning

**`EquipmentCategory` får en nullbar, selvrefererende `ParentCategoryId`.**
En kategori kan ha både underkategorier og eget utstyr samtidig - det finnes
ikke noe "kun blader har utstyr"-krav. Navn er unike blant søsken (samme
forelder), ikke globalt. Dybden er **ikke** håndhevet noe sted i kode - det er
en vane ut fra faktisk bruk (butikken vil trolig aldri gå dypere enn 2-3
nivåer), ikke en dataintegritetsregel, så det finnes ingen kode som teller
dybde. En kategoris forelder settes kun ved opprettelse; det finnes ingen
"flytt kategori"-operasjon, så en sirkel er strukturelt umulig uten at noe må
sjekke for det.

**Frontend er én side, ikke en rute per mappe.** Utstyrssiden viser et
sammenleggbart kategoritre og innholdet i valgt kategori (dens underkategorier
og eget utstyr) side om side, med lokal komponenttilstand for hvilken kategori
som er valgt - ingen brødsmulesti-komponent, ingen egen rute per nivå.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Ubegrenset dybde **med** håndhevet grense i kode | Hindrer at katalogen vokser seg stygg | Krever at hver opprettelse teller dybde opp foreldrekjeden (eller en lagret dybde-/sti-kolonne som må holdes riktig) - reell kode og testoverflate for et problem som egentlig er sosialt, ikke et dataintegritetsproblem | Avvist i samtale - "det er mer hvordan folk faktisk bruker det" |
| Kun blader kan ha utstyr (streng mappe-modell) | Enklere visning: en kategori viser enten undermapper eller filer, aldri begge | Hva skjer med utstyr som allerede ligger direkte i en kategori når noen senere legger til en underkategori der? Krever enten å blokkere underkategorien eller å flytte utstyret - en regel ingen egentlig trengte | Avvist i samtale til fordel for en ekte mappe (som kan ha både filer og undermapper samtidig) |
| Globalt unike navn (dagens oppførsel beholdt) | Minimal endring av eksisterende unik indeks | Blokkerer at to forskjellige grener begge har en underkategori kalt "Diverse" - en helt naturlig ting å ønske i et tre | Avvist i samtale til fordel for unikt-per-forelder |
| Full mappenavigasjon: egen Next.js-rute per nivå (`[[...categoryPath]]`), brødsmulesti-komponent, rutebar URL per mappe | Delelig lenke til en spesifikk mappe, tilbake-knapp fungerer på nettleser-nivå | Vesentlig mer å bygge (catch-all-rute, brødsmulesti, sidenavigasjon per klikk) for et katalognivå som trolig aldri blir dypt eller stort | Avvist i samtale - "jeg vil ikke ha noe tungt, jeg vil ha lettvekt og enkelt" |
| **Selvrefererende FK, uhåndhevet dybde, ett-siders tre + innhold** | Enkleste datamodell som løser det faktiske problemet. Ingen ny rute, ingen brødsmulesti-komponent - gjenbruker den eksisterende utstyrstabellen. Rask å bygge og lett å forstå for neste utvikler | To utvikler-synlige detaljer å kjenne til: Postgres sin NULL-semantikk krever to filtrerte indekser i stedet for én sammensatt (se `04-databasedesign.md`), og "ingen dybdegrense" er en bevisst utelatelse, ikke en forglemmelse | Valgt |

## Konsekvenser

**Positivt**

- Migrasjonen (`AddEquipmentCategoryHierarchy`) er ikke-destruktiv: en nullbar
  kolonne pluss to nye indekser. Eksisterende kategorier ble toppnivå-kategorier
  uten at noe måtte flyttes.
- Frontend-arbeidet er en utvidelse av den eksisterende utstyrstabellen og
  -siden, ikke en ny sidetype - mindre å bygge, mindre å vedlikeholde.
- `EquipmentCategoryRules.EnsureCanBeDeleted` følger nøyaktig samme mønster som
  `EquipmentRules.EnsureCanBeDeleted` (sjekk før sletting, `409` med en
  maskinlesbar `reason`), ikke en ny, annerledes feilform.

**Negativt eller risiko**

- Ingen håndhevet dybdegrense betyr at ingenting i koden hindrer en ansatt i å
  bygge et unødvendig dypt tre. Akseptert bevisst - løses ved å ikke bygge et
  grensesnitt som inviterer til det, ikke ved validering.
- To filtrerte unike indekser i stedet for én er en subtil detalj en fremtidig
  utvikler kan gå glipp av hvis de "forenkler" til én sammensatt indeks -
  dokumentert i `04-databasedesign.md` nettopp for å forhindre det.
- Uten en rutebar URL per kategori kan ikke en ansatt dele en lenke direkte til
  én bestemt kategori - hele poenget med å velge bort det, men verdt å nevne
  som en reell begrensning hvis behovet dukker opp senere.
