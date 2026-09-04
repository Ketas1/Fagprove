---
name: backend-endpoint
description: Add or change an API endpoint in the .NET backend, through every layer, with tests and documentation. Use when building CRUD for equipment, borrowers or loans, adding a state transition such as return or ban, or adding a report endpoint. Enforces the layering, the error contract and the rule that business logic lives on the entity or in the service.
---

# Add a backend endpoint

Work outward from the entity, not inward from the controller. The endpoint is the
last layer, not the first.

## Order of work

### 1. The entity first

Put the rule and the state change on the entity in `SportForAlle.Api/Models/`.
Entities own their state, which is what makes the rules testable without a
database, HTTP or Auth0 - see `docs/adr/0012-lagdelt-monolitt.md`.

- Is this a state transition? Check it against the state machines in
  `docs/03-domenemodell.md`. A transition that is not an arrow in those diagrams
  is a bug, not a new feature.
- **Private setters, behaviour methods.** The transition happens inside a method
  on the entity that validates first. Never expose a public setter for `Status`
  or any other field an invariant depends on.
- A `Loan` must not be constructible in an invalid state.
- Anything time-dependent takes the injected clock. Never read the system clock
  directly - overdue logic becomes untestable.

### 2. Write the entity test now

Before wiring anything up. The rules worth testing hardest:

- A borrower with an open overdue loan cannot borrow.
- A banned borrower cannot borrow.
- Equipment that is not `Available` cannot be lent out.
- A late return increments `LateReturnCount` and sets `IsUnreliable`.
- Invalid transitions are rejected rather than silently applied.

### 3. Service layer

Add the service method in `SportForAlle.Api/Services/` that loads what it needs
via `AppDbContext`, calls the rule checks, calls the entity, saves, and maps
the result to a response DTO. Orchestration only - a method should read as a
short sequence, not a place where rules or mapping logic accumulate.

- **Business-rule checks go in `Services/Rules/`**, one static class per
  entity area (`LoanRules`, `BorrowerRules`, `EquipmentRules`) - not as
  private methods inside the service class. They take entities already
  loaded by the service (no `AppDbContext` dependency), so they can be unit
  tested directly, and they throw `Validation.DomainConflictException` when a
  rule is violated. This is what makes the rule checks reusable and testable
  without pulling a database into the test.
- **Mapping to a response DTO goes in `Mapping/`**, one static mapper class
  per entity (`BorrowerMapper.ToResponse(...)`). If an entity has no
  navigation property for something the DTO needs (most don't - see
  `04-databasedesign.md`), the service joins it in itself and passes it to
  the mapper as an extra argument.
- **EF Core query gotcha:** if a query needs a filter, an order, or both,
  *and* a projection into a named type (not an anonymous type), put the
  filter and the order *inside* the same query - a `where` and an `orderby`
  before `select new SomeRecord(...)` - not chained onto the already-projected
  `IQueryable<T>` (`.Where(...)`, `.OrderBy(...)`, `.OrderByDescending(...)`
  all have this problem, not just `.Where`). EF Core cannot translate a
  predicate or a sort key over members of an already constructor-projected
  type; it throws `InvalidOperationException` with "could not be translated"
  at query execution time, not at compile time - and this only surfaces once
  the query actually runs against a real database with matching rows, not
  against an empty table or in a unit test. See
  `docs/adr/0017-global-exception-handler.md`, "Erfaring fra implementeringen",
  which records two real instances of exactly this, one of them only caught
  by manually testing through Scalar after the automated tests had already
  passed - see the next point for why.

**Services are the only layer that touches `AppDbContext`.** There is no
repository layer; EF Core already is one. If the schema changes, use the
`ef-migration` skill.

### 4. Controller

- Route follows the convention: plural nouns for resources, verbs for state
  transitions.

  ```
  GET    /api/loans?status=overdue
  POST   /api/loans
  POST   /api/loans/{id}/return
  DELETE /api/borrowers/{id}/ban
  ```

- **DTOs in and out.** Entities never cross the API boundary.
- **Authorisation on the endpoint itself.** Every endpoint requires
  authentication; a `Staff` endpoint must reject a `User` token even if the UI
  never renders the button. Hiding a control is not access control.
- Validate input before it reaches the service.

### 5. Errors

Don't build a `ProblemDetails` in the controller or service. Throw instead, and
let `Middleware/ProblemDetailsExceptionHandler.cs` translate it - see
`docs/adr/0017-global-exception-handler.md`:

- Resource not found → throw `Validation.NotFoundException("Fant ikke ...")`.
  Translated to `404`.
- A business rule was violated → throw `Validation.DomainConflictException("SomeReason", "Norwegian detail text")`
  from the rule check in `Services/Rules/`. Translated to `409` by default; pass
  a third argument (`StatusCodes.Status422UnprocessableEntity`) for a request
  that is syntactically fine but semantically impossible, like an age outside
  the allowed range.

The `reason` argument becomes the machine-readable `reason` field in the
response, which is the whole point - the frontend uses it to tell the member of
staff *why* something was blocked, not just that it was. Known reasons already
in use (see `05-api.md` for the current, complete list):

| Situation | Status | reason |
| --- | --- | --- |
| Borrower has an open overdue loan | `409` | `BorrowerHasOverdueLoan` |
| Borrower is banned | `409` | `BorrowerBanned` |
| Equipment is not available | `409` | `EquipmentNotAvailable` |
| Outside 3-18 years (checked at borrower registration, not at loan time) | `422` | `BorrowerOutsideAgeRange` |
| A loan is already returned or lost | `409` | `LoanAlreadyClosed` |
| A unique field (category name, equipment serial number) is already in use | `409` | `Duplicate<Thing>` |

Never build the detail text from the entity's own (English) exception message -
`ProblemDetails` content is user-facing, and user-facing text is Norwegian, see
CLAUDE.md. Write the Norwegian detail text explicitly at the call site.

### 6. Integration test

Test the endpoint with `WebApplicationFactory`, as in
`SportForAlle.Tests/Controllers/`. Cover the success path, the blocked path with
the right status and reason, and the authorisation path (`401` without a token,
`403` with the wrong role). Use `AuthenticatedWebApplicationFactory` for the
authenticated cases, and build any prerequisite resources (a guardian before a
borrower, a category before equipment) with
`SportForAlle.Tests/TestSupport/ApiTestDataBuilder.cs` rather than repeating
the request bodies - add a method there if the endpoint introduces a new kind
of prerequisite.

**If the endpoint is a `GET` list, an authenticated test must actually call it
with data in the table**, not just check `401` without a token. The `401`
check never reaches the query, so it proves nothing about whether the query
itself runs - and a query that only fails once real rows exist (see the EF
Core gotcha above) will pass every test that never calls the endpoint
authenticated. This exact gap - a list endpoint with only a `401` test,
nothing authenticated - is what let one of the two query bugs above reach a
real, manual test session before it was caught.

Testcontainers is not set up yet; tests run against the shared Docker database
(`docker compose up -d db`), not an isolated one per test. Use random values
(`Guid.NewGuid()`) for anything that must be unique, since fixed names would
collide across test runs.

### 7. Document

Add the endpoint to the table in `docs/05-api.md`, including its role and any
error reason it can return.

## Checklist before calling it done

- [ ] Business rule is on the entity or in the service, not in the controller
- [ ] Entity unit test covers the rule, including the failure case
- [ ] Endpoint requires authentication, and the correct role
- [ ] Errors return `ProblemDetails` with a `reason`
- [ ] Integration test covers success, block and authorisation
- [ ] Entity is not exposed - DTO in and out
- [ ] No personal data written to logs
- [ ] `docs/05-api.md` updated
