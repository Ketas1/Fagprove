# ADR-0004: PostgreSQL som database

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Et av de tre hovedproblemene hos oppdragsgiver er at det ikke finnes noen samlet
oversikt over utstyr, frister og brukere, og at det derfor ikke kan hentes ut
statistikk. Løsningen på begge er strukturerte data som kan spørres mot.

Dataene er tydelig relasjonelle: en foresatt har barn, et barn har utlån, et
utlån gjelder ett utstyr og kan ha flere kontaktforsøk og bilder.

## Beslutning

PostgreSQL brukes som database, kjørt i Docker lokalt.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **PostgreSQL** | Moden relasjonsdatabase. Sterk på aggregeringer og datoer, som rapportene bygger på. Gratis og åpen. Godt offisielt Docker-image. God EF Core-støtte via Npgsql. | Litt mer oppsett enn en filbasert database. | Valgt |
| SQL Server | Tettest integrasjon med .NET. | Tyngre image, og lisensforhold å ta hensyn til. Mindre naturlig sammen med Docker på tvers av plattformer. | PostgreSQL gir det samme uten lisensspørsmål |
| SQLite | Null oppsett, en enkelt fil. | Svakere på samtidighet og på datotyper. Skiller seg fra en realistisk produksjonsdatabase, noe som svekker verdien av å teste mot den. | Ønsket at utvikling og test skjer mot samme databasetype som en reell drift ville brukt |
| MongoDB | Fleksibelt skjema. | Dataene er relasjonelle, og rapportene er aggregeringer på tvers av tabeller. Et dokumentlager ville gjort begge deler vanskeligere. | Feil verktøy for denne datamodellen |

## Konsekvenser

**Positivt**

- Relasjonene mellom barn, foresatt, utstyr og utlån håndheves av databasen med
  fremmednøkler.
- Rapportene kan skrives som vanlige aggregeringsspørringer.
- `timestamptz` gir korrekt håndtering av tidssoner, som er viktig når forfall
  beregnes på dato.
- Testene kan kjøre mot en ekte PostgreSQL i en container, slik at de tester det
  samme som utviklingsmiljøet bruker.

**Negativt eller risiko**

- Krever at Docker kjører for at utvikling skal fungere. Akseptert, siden Docker
  uansett brukes til hele stacken, se ADR-0007.
- Datoer og tidssoner må håndteres bevisst: lagres som UTC, vises i norsk tid.
