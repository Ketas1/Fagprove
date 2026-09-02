# ADR-0008: GitHub Actions til CI

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Prosjektet trenger en enkel CI-pipeline som bygger og tester frontend og backend
på hver commit, slik at feil oppdages raskt. Koden ligger allerede på GitHub.

## Beslutning

GitHub Actions brukes til CI. `.github/workflows/ci.yml` kjører på push og pull
request mot `main`, med separate parallelle jobber for backend og frontend.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **GitHub Actions** | Integrert med repositoriet som allerede brukes. Ingen ekstra kontoer eller tjenester. Gratis for offentlige og små private repoer. Ferdige actions for både .NET og Bun. | Bundet til GitHub. | Valgt |
| GitLab CI | Godt CI-verktøy. | Ville krevd at koden flyttes til GitLab. | Ingen grunn til å flytte |
| Ingen CI, kun lokal testing | Ingen oppsett. | Ingenting fanger opp at noe er ødelagt før det oppdages manuelt. Går imot kravet om rask tilbakemelding. | Ikke et reelt alternativ |

## Konsekvenser

**Positivt**

- Rask tilbakemelding dersom noe brekker, uten at det må oppdages manuelt.
- Pipelinen dokumenterer i praksis hvordan prosjektet bygges og testes.
- Formateringssjekk mot `.editorconfig` holder kodestilen konsistent uten
  diskusjon.
- Jobbene kjører parallelt, slik at frontend ikke venter på backend.

**Negativt eller risiko**

- Pipelinen settes opp før prosjektene finnes. Jobbene er derfor skrevet slik at
  de hopper over seg selv så lenge `backend/` eller `frontend/` er tomme, i
  stedet for å feile. Sjekken fjernes når prosjektene er på plass.
- Integrasjonstester mot database krever en tjenestecontainer i pipelinen, som
  gjør den tregere. Akseptert, fordi testene ellers ville testet mot noe annet
  enn det systemet faktisk bruker.
