# ADR-0001: Monorepo for frontend og backend

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Løsningen består av tre deler: en Next.js-frontend, et .NET-API og en
PostgreSQL-database. Koden må organiseres slik at den er enkel å utvikle,
versjonere og kjøre, av én utvikler i en kort utviklingsperiode.

## Beslutning

Frontend og backend ligger i samme repositorium, i mappene `frontend/` og
`backend/`, med felles dokumentasjon, CI og Docker-oppsett i roten.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Monorepo** | Endringer på tvers av lagene i én commit. Én CI-pipeline. Én versjon av systemet til enhver tid. | Repoet blir større og blander to teknologistacker. | Valgt |
| Separate repositorier | Tydeligere skille mellom lagene. Kan deployes uavhengig. | En endring i API-et og siden som bruker det må koordineres over to repoer og to pull requests. Vanskeligere å vite hvilke versjoner som hører sammen. | Overhead uten gevinst når én utvikler jobber på begge lag samtidig |
| Alt i ett prosjekt (Blazor eller MVC med Razor) | Kun én teknologi å forholde seg til. | Går bort fra Next.js, som er valgt av andre grunner. Svakere skille mellom API og grensesnitt. | Ønsket et tydelig API-lag, se ADR-0002 og ADR-0003 |

## Konsekvenser

**Positivt**

- Et endepunkt og siden som bruker det kan endres og testes i samme commit.
- CI bygger og tester begge lag mot samme versjon av koden.
- Docker Compose kan starte hele systemet fra roten av repoet.
- Dokumentasjonen ligger sammen med all koden den beskriver.

**Negativt eller risiko**

- CI må håndtere to økosystemer (.NET og Bun) i samme pipeline. Løst med
  separate jobber som kjører parallelt.
- Ved en eventuell senere deploy må det være tydelig hvilken del som deployes
  hvor. Ikke aktuelt innenfor omfanget av dette forslaget.
