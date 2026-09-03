# Backend

ASP.NET Core Web API on .NET 10, with Entity Framework Core against PostgreSQL.

## Structure

One project, layered by folder rather than by assembly. See
[`docs/adr/0012-lagdelt-monolitt.md`](../docs/adr/0012-lagdelt-monolitt.md) for
why, and what was rejected.

```
backend/
├── SportForAlle.sln
├── SportForAlle.Api/
│   ├── Program.cs
│   ├── Controllers/       HTTP endpoints, DTOs in and out
│   ├── Services/          Use cases; the only layer that touches AppDbContext
│   ├── Models/            Entities that own their state and rules
│   ├── Dtos/              Objects crossing the API boundary
│   ├── Data/              AppDbContext, Configurations/, Migrations/
│   ├── Mapping/  Validation/  Middleware/
│   └── Configuration/  Helpers/
└── SportForAlle.Tests/    xUnit
```

## Rules that hold this together

- Controllers call services. **Services are the only layer that uses
  `AppDbContext`.** There is no repository layer - EF Core already is one.
- **Entities own their state.** Setters are private and transitions go through
  methods that validate first, so an invalid state cannot be assigned from
  elsewhere. This is what keeps the business rules enforceable without the
  compiler-enforced boundary a multi-project layout would give.

## Running

The database runs in Docker; the API runs locally.

```bash
docker compose up -d db          # from the repository root
cd SportForAlle.Api && dotnet run
```

The API listens on http://localhost:5080 and `GET /api/health` reports whether
the database is reachable.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project SportForAlle.Api --startup-project SportForAlle.Api \
  --output-dir Data/Migrations

dotnet ef database update \
  --project SportForAlle.Api --startup-project SportForAlle.Api
```

Read the generated migration before applying it - see the `ef-migration` skill.

## Tests

```bash
dotnet test
```

## Note on package versions

EF Core is pinned to **10.0.4** rather than the latest patch, because the Npgsql
provider is built against that version. Mixing it with a newer EF Core produces
an assembly conflict on `Microsoft.EntityFrameworkCore.Relational` when the test
project builds. Let the provider drive the EF version.
