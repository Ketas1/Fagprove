# Backend

.NET 10 Web API. Not scaffolded yet.

## Planned structure

```
backend/
├── SportForAlle.sln
├── Dockerfile
├── src/
│   ├── SportForAlle.Api/             HTTP endpoints, DTOs, authorisation
│   ├── SportForAlle.Application/     Use cases, orchestration
│   ├── SportForAlle.Domain/          Entities, state machines, business rules
│   └── SportForAlle.Infrastructure/  EF Core, email, file storage
└── tests/
    ├── SportForAlle.Domain.Tests/          xUnit unit tests
    └── SportForAlle.Api.IntegrationTests/  Endpoint tests via Testcontainers
```

Dependencies point inward: `Api` to `Application` to `Domain`. The domain
references nothing outward, which is what makes the business rules testable
without a database, HTTP or Auth0.

See [`docs/02-arkitektur.md`](../docs/02-arkitektur.md) for the reasoning, and
`CLAUDE.md` for the conventions that apply here.
