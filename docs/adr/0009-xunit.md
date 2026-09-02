# ADR-0009: xUnit til testing av backend

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Forretningsreglene i systemet - særlig blokkering av nye utlån og de tre
tilstandsmaskinene - er kjernen i det systemet skal løse. De må være dekket av
tester, og testene må kunne kjøre i CI på hver commit.

## Beslutning

xUnit brukes til enhetstester og integrasjonstester i backend, med
`Microsoft.AspNetCore.Mvc.Testing` for tester mot API-et og Testcontainers for en
ekte PostgreSQL under integrasjonstestene.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **xUnit** | De facto standard i .NET. Godt integrert med `dotnet test` og GitHub Actions. Ny instans per test, som gir god isolasjon. | Færre innebygde påstandsmetoder enn enkelte alternativer. | Valgt |
| NUnit | Rikt påstandsbibliotek, fleksible attributter. | Delt tilstand mellom tester som standard, som lettere gir tester som påvirker hverandre. | Isolasjonen i xUnit er å foretrekke |
| MSTest | Følger med Visual Studio. | Minst utbredt av de tre i moderne .NET-prosjekter. | Ingen fordel over xUnit |

### In-memory-database mot ekte database

For integrasjonstestene ble EF Cores in-memory-provider vurdert. Den er rask,
men oppfører seg ikke som PostgreSQL: den håndhever ikke fremmednøkler og
behandler datoer annerledes. En test som passerer der kan feile i praksis.
Testcontainers starter en ekte PostgreSQL i en container, og velges derfor selv
om det gjør testene tregere.

## Konsekvenser

**Positivt**

- Domenereglene kan testes uten database, HTTP eller Auth0, fordi domenelaget
  ikke er avhengig av noe av det - se ADR-0002.
- `dotnet test` kjører alt, både lokalt og i CI, uten ekstra oppsett.
- Integrasjonstestene tester mot samme databasetype som systemet faktisk bruker.

**Negativt eller risiko**

- Testcontainers krever at Docker kjører, også i CI. Det gjør testene tregere.
  Akseptert, fordi alternativet er tester med lavere verdi.
- Tid må håndteres gjennom en klokkeabstraksjon for at forfall skal kunne testes.
  Det er et designkrav på domenelaget, ikke bare et testvalg, og er dokumentert i
  `07-testing.md`.
