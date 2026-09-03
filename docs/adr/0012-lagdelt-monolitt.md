# ADR-0012: Lagdelt monolitt uten repositories

- **Status:** Akseptert
- **Dato:** 2026-09-02

## Kontekst

Backend måtte struktureres før scaffoldingen. Systemet har rundt ti entiteter,
én konsument (frontend), én utvikler og en kort utviklingsperiode. Samtidig er
det et krav at prosjektet skal kunne overtas av en annen IT-avdeling.

Utgangspunktet i [`02-arkitektur.md`](../02-arkitektur.md) var Clean
Architecture med fire prosjekter: `Api`, `Application`, `Domain` og
`Infrastructure`.

## Beslutning

Backend er **ett prosjekt** med lagdeling i mapper:

```
SportForAlle.Api/
  Controllers/  Services/  Models/  Dtos/  Data/
  Mapping/  Validation/  Middleware/  Configuration/  Helpers/
```

Tre valg henger sammen og dokumenteres derfor som én beslutning:

1. **Ett prosjekt i stedet for fire.** Lagene skilles med mapper og namespaces.
2. **Ingen repositories.** Services bruker `AppDbContext` direkte.
3. **Entiteter eier sin egen tilstand.** Setterne er private, og hver
   tilstandsendring går gjennom en metode på entiteten som validerer først.

Testprosjektet `SportForAlle.Tests` er skilt ut, fordi xUnit krever et eget
prosjekt.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| **Lagdelt monolitt** | Færre filer og ingen prosjektreferanser å vedlikeholde. Kjent og gjenkjennelig struktur for de fleste .NET-utviklere. | Lagskillet håndheves av konvensjon, ikke av kompilatoren. | Valgt |
| Clean Architecture, fire prosjekter | Kompilatoren garanterer at domenelaget ikke kan referere EF Core. Byttbar infrastruktur. | Fire assemblies, prosjektreferanser og DI-oppsett på tvers for ti entiteter og én konsument. Uavhengigheten av rammeverk innløses i praksis nesten aldri. | Overkill for denne størrelsen |
| Tykke controllers, uten servicelag | Færrest mulig filer. | Forretningsregler i controllere kan ikke gjenbrukes fra andre inngangspunkter, og kan ikke testes uten HTTP. Regelen om blokkerte utlån må gjelde uansett hvor et utlån registreres. | Avvist |

### Hvorfor repositories ble droppet

Repositories ble vurdert og valgt bort, ikke glemt.

I EF Core er `DbSet<T>` allerede et repository og `DbContext` allerede en unit of
work. Et lag på toppen blir i praksis gjennomstikk, og det koster noe reelt:
`IQueryable` kan ikke lenger komponeres fritt, så rapportspørringene ender enten
med N+1-uttrekk eller med én egen repository-metode per spørring.

Hovedargumentet for repositories er at de kan mockes i enhetstester. Det
argumentet er allerede innløst på en annen måte: integrasjonstestene kjører mot
en ekte PostgreSQL i en container, se [ADR-0009](./0009-xunit.md). Da er det lite
igjen som forsvarer laget.

Det ville også vært inkonsekvent å fjerne Clean Architecture som seremoni og
samtidig beholde et lag som er vanskeligere å forsvare i denne størrelsen.

### Hvordan invariantene beskyttes uten Clean Architecture

Dette er den reelle kostnaden ved å droppe fire prosjekter, og den må dekkes.

Med fire prosjekter kan `Domain` fysisk ikke referere EF Core, og kompilatoren
garanterer at reglene ikke smøres ut i datatilgangen. Mapper gir bare en
konvensjon, og konvensjoner taper mot tidspress.

Løsningen er at entitetene eier sin egen tilstand:

```csharp
public LoanStatus Status { get; private set; }

public void RegisterReturn(DateTime returnedAt, IClock clock)
{
    if (Status is not (LoanStatus.Active or LoanStatus.Overdue))
    {
        throw new DomainException(...);
    }

    Status = LoanStatus.Returned;
    ReturnedAt = returnedAt;
}
```

`loan.Status = LoanStatus.Returned` kompilerer ikke utenfor entiteten.
Tilstandsmaskinene i [`03-domenemodell.md`](../03-domenemodell.md) håndheves
dermed av typen, ikke av disiplin, og kan enhetstestes uten database, HTTP eller
Auth0.

## Konsekvenser

**Positivt**

- Vesentlig færre filer og ingen wiring på tvers av assemblies.
- Rapportspørringer kan komponeres fritt med LINQ mot `DbContext`.
- Forretningsreglene er fortsatt testbare uten infrastruktur, fordi de ligger på
  entitetene.
- Strukturen er gjenkjennelig for en .NET-utvikler som overtar prosjektet, noe
  som er et uttalt krav.

**Negativt eller risiko**

- Ingenting hindrer teknisk at en controller kaller `DbContext` direkte.
  Konvensjonen er at controllere kaller services, og at services er det eneste
  laget som snakker med `DbContext`. Dette må håndheves i gjennomgang av kode.
- Dersom systemet en gang skal ha flere konsumenter eller byttes til en annen
  datakilde, må lagene skilles ut i egne prosjekter. Fordi avhengighetene
  allerede peker samme vei, er det en refaktorering og ikke en omskriving.
- `AppDbContext` er tilgjengelig fra hele prosjektet. Det er prisen for å slippe
  repository-laget, og den er akseptert bevisst.

## Erstatter

Denne beslutningen erstatter lagdelingen som var beskrevet i
[`02-arkitektur.md`](../02-arkitektur.md) før scaffoldingen. Ingen tidligere ADR
låste den strukturen, så ingen ADR settes til erstattet.
