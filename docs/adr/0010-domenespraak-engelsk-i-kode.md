# ADR-0010: Engelsk i kode, norsk i grensesnitt og dokumentasjon

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Oppdragsgiver og domenet er norsk. Begrepene i domenet er utlån, utstyr,
foresatt, forfalt, upålitelig og utestengt. Rammeverkene, .NET og Next.js, er
engelske, og dokumentasjonen skal leses på norsk.

Uten en bestemmelse ender slike prosjekter typisk med blandingsformer som
`GetUtlaanAsync` eller `LaanRepository`, der halve navnet er norsk og halve
engelsk, og der æ, ø og å må omskrives ulikt fra sted til sted.

## Beslutning

- **Kode, identifikatorer, API-ruter, databasetabeller og kommentarer:** engelsk.
- **All tekst brukeren ser i grensesnittet:** norsk.
- **All dokumentasjon i `docs/`:** norsk.
- **AI-instrukser (`CLAUDE.md`, skills):** engelsk.

Ordlisten som binder de to sammen ligger i `03-domenemodell.md` og speiles i
`CLAUDE.md`.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Engelsk kode, norsk grensesnitt** | Domenenavn står side om side med rammeverkets egne begreper uten å skurre. Ingen problemer med æ, ø og å i klassenavn, ruter eller kolonner. Leselig for en utvikler uten norskkunnskaper. | Krever en oversettelse mellom oppgaveteksten og koden, og en ordliste som må vedlikeholdes. | Valgt |
| Norsk overalt | Null oversettelsestap. Koden bruker nøyaktig oppdragsgivers begreper. | `Utlaan` ved siden av `IServiceCollection` og `DbContext` blir inkonsekvent. Æ, ø og å må omskrives i identifikatorer, ruter og kolonnenavn, og praksis for det blir fort ulik. | Blandingen mellom norske domenenavn og engelske rammeverksnavn er verre enn en ordliste |
| Engelsk overalt, også dokumentasjon | Mest konsekvent. | Dokumentasjonen leses på norsk. Å skrive den på et andrespråk svekker den unødvendig. | Dokumentasjonen er en del av leveransen og skal være best mulig |

## Konsekvenser

**Positivt**

- Ingen blandingsformer i koden.
- Ingen æ, ø eller å i klassenavn, ruter, tabellnavn eller filnavn.
- Dokumentasjonen skrives på det språket den leses på.
- Ordlisten er i seg selv dokumentasjon av domenet, og gjør begrepene eksplisitte.

**Negativt eller risiko**

- Ordlisten må holdes oppdatert to steder, i `03-domenemodell.md` og i
  `CLAUDE.md`. Regelen om at et nytt begrep legges inn begge steder er tatt inn i
  begge filene.
- Enkelte begreper har ingen presis engelsk motpart. "Upålitelig" er oversatt til
  `IsUnreliable` og "utestengt" til `Banned`; nyansene forklares i
  `03-domenemodell.md` i stedet for å presses inn i navnet.
