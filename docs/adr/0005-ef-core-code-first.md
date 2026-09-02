# ADR-0005: Entity Framework Core med Code First

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Databaseskjemaet vil endre seg flere ganger i løpet av utviklingen, etter hvert
som domenemodellen blir tydeligere. Endringene må kunne gjøres uten at
eksisterende data og oppsett faller fra hverandre, og de må være sporbare for en
utvikler som overtar prosjektet senere.

## Beslutning

Datamodellen defineres som C#-klasser i `SportForAlle.Domain`, og databasen
genereres fra dem med EF Core-migrasjoner (Code First). Databasen endres aldri
manuelt.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **EF Core Code First** | Modellen er kode, og versjoneres sammen med applikasjonen. Migrasjoner gir en sporbar historikk over skjemaendringer. Kompilatoren fanger opp uoverensstemmelser mellom modell og bruk. | Genererte spørringer er ikke alltid optimale. Migrasjoner må gjennomgås, ikke bare genereres. | Valgt |
| Database First | Full kontroll over skjemaet. | Skjemaet lever utenfor versjonskontroll, og modellen må regenereres etter hver endring. Vanskeligere å se hva som endret seg og hvorfor. | Sporbarhet i skjemaendringer er et krav i prosjektet |
| Dapper med håndskrevet SQL | Rask, og full kontroll over hver spørring. | Alt skjemaarbeid og all mapping gjøres manuelt. Ingen migrasjoner. Betydelig mer arbeid. | Tidsbruken kan ikke forsvares for denne datamengden |

## Konsekvenser

**Positivt**

- En skjemaendring er en commit: modellendring og migrasjon følges ad.
- Nye utviklere setter opp databasen med én kommando (`dotnet ef database update`).
- Domenemodellen er ett sted, og både forretningsregler og skjema stammer fra den.

**Negativt eller risiko**

- Genererte migrasjoner må leses gjennom før de kjøres. En omdøpt egenskap kan
  ellers bli tolket som "slett kolonne og lag ny", med tap av data.
- Tunge rapportspørringer kan bli ineffektive gjennom LINQ. Dersom det skjer,
  skrives den enkelte spørringen som rå SQL - uten at hele tilnærmingen endres.
- Migrasjonsfiler er generert kode og skal ikke redigeres i etterkant, bare
  erstattes av en ny migrasjon.
