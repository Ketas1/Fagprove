# ADR-0002: .NET 10 som backend

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Backend skal eksponere et API for registrering av utstyr, brukere og utlån,
håndheve forretningsreglene og gjøre spørringer mot databasen for rapportering.
Utviklingsperioden er kort, og løsningen skal være enkel for en annen utvikler å
drifte og videreutvikle.

## Beslutning

Backend bygges som et ASP.NET Core Web API på .NET 10, delt i lagene `Api`,
`Application`, `Domain` og `Infrastructure`.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **.NET 10** | Mest erfaring fra før. Godt egnet til API-er og databehandling. Svært godt dokumentert, stort økosystem. Sterk typing og god verktøystøtte. | Tyngre å komme i gang med enn et minimalt rammeverk. | Valgt |
| Node.js med Express eller NestJS | Samme språk som frontend. Rask oppstart. | Mindre erfaring. Svakere typegarantier med mindre TypeScript settes opp grundig. Mister EF Core, som er et selvstendig argument. | Erfaringen med .NET veier tyngre enn gevinsten ved ett språk |
| Python med FastAPI | Rask å skrive. God API-dokumentasjon ut av boksen. | Minst erfaring. Dynamisk typing gir svakere garantier i en domenemodell med mange tilstander. | Tilstandsmaskinene i domenet drar nytte av en kompilator |

Valget av .NET er også det som gjør EF Core Code First og xUnit naturlige, se
ADR-0005 og ADR-0009.

## Konsekvenser

**Positivt**

- Kjent rammeverk, som gir mer tid til domenet og mindre tid til å lære verktøy.
- Kompilatoren fanger opp feil i tilstandsoverganger og nullhåndtering.
- EF Core gir migrasjoner og en modell som versjoneres sammen med koden.
- Lagdelingen gjør forretningsreglene testbare uten database eller HTTP.

**Negativt eller risiko**

- To språk i prosjektet, C# og TypeScript, med hver sine konvensjoner.
  Håndteres med felles regler i `.editorconfig` og en tydelig ordliste.
- .NET 10 er en ny versjon. Skulle noe i økosystemet ikke være klart for den, er
  fallback å gå ned til .NET 9, som er installert på maskinen fra før.
