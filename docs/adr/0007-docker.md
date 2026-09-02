# ADR-0007: Docker Compose som kjøremiljø

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Systemet består av tre deler som må kjøre samtidig for at noe skal kunne testes
end to end. Utviklingsmiljøet må være likt hver gang, og en databaseinstans må
være tilgjengelig for migrasjoner og spørringer under utvikling.

## Beslutning

Hele systemet kjøres med Docker Compose: `frontend`, `api` og `db` som tre
tjenester, startet med `docker compose up`.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Docker Compose** | Standardiserer miljøet. Alle tre lag startes med én kommando. Databasen krever ingen lokal installasjon. Nær et realistisk driftsoppsett. | Krever Docker Desktop. Litt tregere iterasjon enn å kjøre rett fra IDE. | Valgt |
| Lokal installasjon av alt | Raskest mulig iterasjon. | PostgreSQL må installeres og vedlikeholdes lokalt. Miljøet blir personlig og vanskelig å gjenskape. | Går imot målet om at systemet skal være enkelt for en annen utvikler å sette opp |
| Kun database i Docker | Rask iterasjon på applikasjonskoden, enkel database. | Systemet kan da aldri kjøres som en helhet på den måten det er ment å kjøre. | Et krav er at alle tre lag skal kunne kjøres samtidig |

Det siste alternativet er likevel den vanlige arbeidsmåten *under* utvikling:
`docker compose up -d db` starter bare databasen, og applikasjonen kjøres fra
IDE-en mot den. Begge deler støttes, og det er dokumentert i
`11-utviklingsmiljo.md`.

## Konsekvenser

**Positivt**

- `docker compose up --build` er alt som trengs for å kjøre systemet.
- Databasen er tilgjengelig uten lokal installasjon, med data i et navngitt
  volum slik at den overlever omstart.
- Integrasjonstestene kan bruke samme databaseversjon som utviklingsmiljøet.
- Reduserer forskjellene mellom utviklings- og et eventuelt produksjonsmiljø.

**Negativt eller risiko**

- Docker Desktop må kjøre. Uten det virker verken database eller stack.
- Hot reload gjennom containere er tregere enn å kjøre direkte. Derfor er
  arbeidsmåten med kun databasen i Docker dokumentert som et alternativ.
- Dockerfilene må vedlikeholdes sammen med prosjektene.
