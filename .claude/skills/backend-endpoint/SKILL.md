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
via `AppDbContext`, calls the entity, and saves. Orchestration only - no business
rules here.

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

Return RFC 7807 `ProblemDetails` with a machine-readable `reason` the frontend
can branch on.

| Situation | Status | reason |
| --- | --- | --- |
| Borrower has an open overdue loan | `409` | `BorrowerHasOverdueLoan` |
| Borrower is banned | `409` | `BorrowerIsBanned` |
| Equipment is not available | `409` | `EquipmentNotAvailable` |
| Borrower has no guardian | `409` | `BorrowerHasNoGuardian` |
| Outside 3-18 years | `409` | `BorrowerOutsideAgeRange` |

A blocked loan is a `409` with a reason, never a bare `400`. The frontend has to
tell the member of staff *why* it was blocked - that explanation is the whole
point of the mechanism.

### 6. Integration test

Test the endpoint with `WebApplicationFactory`, as in
`SportForAlle.Tests/Controllers/`. Cover the success path, the blocked path with
the right status and reason, and the authorisation path (`401` without a token,
`403` with the wrong role).

Testcontainers is not set up yet. When a test needs real database behaviour
rather than routing, add it and record the choice.

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
