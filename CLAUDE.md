# Sport For Alle - equipment lending system

Web system for **Sport For Alle AS**, a municipally owned shop that lends sports
equipment to children aged 3-18 through a scheme funded by the municipality.

The system replaces a paper process and solves three problems:

1. Equipment is returned late or not at all, with no way to detect it.
2. No central overview of equipment, due dates, or who borrowed what.
3. No statistics or reporting for the municipality that funds the scheme.

## What this deliverable actually is

Getting this wrong distorts every other decision, so be clear about it:

**This is a proposal for a solution, not a production system.** It is not meant
to be instantly deployable, and it will not be run for real users yet. Do not add
production concerns - scaling, monitoring, high availability, load balancing -
that nobody asked for.

**But it must be handoff-ready.** A stated requirement is that the project can be
handed to another IT department who can continue development and get it to a
deployable state without much difficulty. That means the reasoning, the
architecture, the data handling and the remaining gaps are all written down. The
gap between "proposal" and "deployable" should be visible work, not a mystery.

**The customer is a municipally owned business.** That makes it public sector, so
legal and regulatory questions are not decoration - GDPR and data handling,
package licensing, terms of service, and relevant Norwegian regulation all belong
in the documentation, and are expected to be there.

## Non-negotiables

Read these before writing any code.

- **Language split.** Code, identifiers, API routes and comments are **English**.
  All user-facing UI strings and everything in `docs/` are **Norwegian**. Use the
  glossary below - never mix, never produce hybrids like `GetUtlaanAsync`.
- **Document as you go.** Documentation is part of the deliverable, not an
  afterthought. A feature is not done until `docs/` reflects it. Any technical
  choice gets an ADR in `docs/adr/`.
- **Personal data about children.** Every design decision touching names, birth
  dates, photos or notes must be defensible under GDPR. See
  `docs/09-lover-og-regler.md` and the guardrails at the bottom of this file.

## Stack

| Layer    | Technology                                                        |
| -------- | ----------------------------------------------------------------- |
| Frontend | Next.js (App Router), TypeScript, Tailwind, Bun                   |
| Backend  | .NET 10, ASP.NET Core Web API, Entity Framework Core (Code First) |
| Database | PostgreSQL                                                        |
| Auth     | Auth0 (OIDC / JWT bearer), roles User / Staff / Admin             |
| Testing  | xUnit (backend), Jest + Testing Library (frontend)                |
| Infra    | Docker Compose, GitHub Actions                                    |

Rationale for each choice lives in `docs/adr/`. Do not swap any of these without
writing a new ADR that supersedes the existing one.

## Repository layout

```
backend/
  SportForAlle.sln
  SportForAlle.Api/     One project, layered by folder
  SportForAlle.Tests/   xUnit
frontend/               Next.js app (src/app, components, lib, types)
docs/                   Norwegian documentation + ADRs
.claude/                Skills and shared settings for Claude Code
.github/                CI workflows and PR template
```

## Domain

### Glossary (Norwegian plan to English code)

| Norwegian            | Code                        |
| -------------------- | --------------------------- |
| Utlån                | `Loan`                      |
| Utstyr               | `Equipment`                 |
| Barn (låntaker)      | `Child` / `Borrower`        |
| Foresatt             | `Guardian`                  |
| Ansatt               | `Staff`                     |
| Lånestatus           | `LoanStatus`                |
| Utstyrstatus         | `EquipmentStatus`           |
| Forventet returdato  | `DueDate`                   |
| Returtidspunkt       | `ReturnedAt`                |
| Forfalt              | `Overdue`                   |
| Levert               | `Returned`                  |
| Ledig                | `Available`                 |
| Utlånt               | `OnLoan`                    |
| Ut av drift          | `OutOfService`              |
| Tapt / avskrevet     | `Lost` / `WrittenOff`       |
| Upålitelig           | `IsUnreliable`              |
| Utestengt            | `Banned`                    |
| Gebyr                | `Ban.FeePaidAt` (felt, ikke egen type) |
| Kontaktforsøk        | `ContactAttempt`            |
| Eskalering           | `Borrower.Ban()` (en overgang, ikke egen type) |
| Notat                | `Note`                      |
| Tilstand             | `EquipmentCondition`        |
| Låntakerstatus       | `BorrowerStatus`            |
| Serienummer          | `SerialNumber`              |
| Rapport              | `Report`                    |
| Kommune              | `Municipality`              |

When you add a domain term, add it here *and* in `docs/03-domenemodell.md`.

### State machines

These come from the staff workflow, documented in `docs/03-domenemodell.md`.
Treat the transitions as invariants - a state change outside these arrows is a
bug.

**LoanStatus**

```
Active   --(due date passes, automatic)--> Overdue
Active   --(equipment handed back)------->  Returned
Overdue  --(equipment handed back late)-->  Returned
Overdue  --(loss or damage confirmed)---->  Lost
```

**EquipmentStatus**

```
Available --(loan registered)-----------> OnLoan
OnLoan    --(returned, good condition)--> Available
OnLoan    --(returned, damaged)---------> OutOfService
OnLoan    --(loss confirmed)------------> WrittenOff
```

**Borrower status**

```
Active --(returned after due date)--> Active, IsUnreliable = true,
                                      LateReturnCount incremented
Active --(escalation)--------------> Banned  (reason recorded as a Note)
Banned --(fee paid in the shop)----> Active  (payment recorded)
```

`IsUnreliable` is a flag, not a status: an unreliable borrower may still borrow.
A **banned** borrower may not. Keep those two concepts separate.

### Business rules

1. A child cannot borrow until a guardian is registered and linked to them.
2. **Registering a loan is blocked** when the borrower has an open overdue loan,
   or is banned. This is the core incentive mechanism - enforce it server-side in
   the domain layer, not only in the UI.
3. Equipment must be `Available` to be lent out.
4. A photo is taken **before** the loan and **at return**. These are the evidence
   basis for damage claims and bans.
5. Every contact attempt with a guardian records date, method (email/phone) and
   outcome. Staff must be able to see whether a guardian was already contacted.
6. Overdue detection is **automatic**. Staff never have to check manually.
7. Lifting a ban requires recording that the fee was paid in the shop.

### Reporting

The municipality funds the scheme and needs evidence it works. Reports cover:
loans over a period, total loans broken down by age group, most-borrowed
equipment, and counts of late and unreturned loans.

Age groups are fixed: **3-6**, **7-12**, **13-18**. Age is calculated at
`Loan.StartedAt`, not at report time, so historical figures do not shift as
children get older.

## Conventions

### Backend

- **One project, layered by folder** - `SportForAlle.Api` with `Controllers/`,
  `Services/`, `Models/`, `Dtos/`, `Data/`, `Mapping/`, `Validation/`,
  `Middleware/`, `Configuration/`, `Helpers/`. Not Clean Architecture; see
  `docs/adr/0012-lagdelt-monolitt.md`.
- **Controllers call services. Services are the only layer that touches
  `AppDbContext`.** There is no repository layer - EF Core already is one. A
  controller reaching for the DbContext is a bug.
- **Entities own their state.** Setters are private and every transition goes
  through a method on the entity that validates first, so
  `loan.Status = LoanStatus.Returned` does not compile outside the entity. This
  is what replaces the compiler-enforced boundary Clean Architecture would have
  given, so it is not optional.
- **Business rules live on the entity or in the service**, never in a controller
  or a React component. A rule that only exists in the UI does not exist.
- **Primary keys are always `Guid`, never `int`.** Generated by the entity
  itself in its constructor (`Guid.NewGuid()`), not by the database - so an
  entity has a real id from the moment it exists in memory, before anything is
  saved. Configure `ValueGeneratedNever()` on `Id` so EF Core does not try to
  generate its own. Foreign keys follow the same type as the primary key they
  reference.
- Nullable reference types are on. Do not silence warnings with the null-forgiving
  operator - fix the model instead.
- Async everywhere for I/O, suffix `Async`, take a `CancellationToken`.
- Entities never cross the API boundary. Map to DTOs.
- REST shape: plural nouns, verbs for state transitions.

  ```
  GET    /api/loans?status=overdue
  POST   /api/loans
  POST   /api/loans/{id}/return
  POST   /api/borrowers/{id}/ban
  DELETE /api/borrowers/{id}/ban
  ```

- Errors return RFC 7807 `ProblemDetails`. A blocked loan is a `409 Conflict`
  with a machine-readable reason, not a bare `400`.
- EF Core is Code First. Schema changes happen through a migration - never by
  editing the database directly.
- **Logging: one deliberate line beats a firehose.** When something needs to be
  visible, add a specific `logger.LogInformation(...)`/`console.error(...)` at
  the point that matters, with a message that says what happened. Do not reach
  for raising a whole framework log category (`Microsoft.AspNetCore` to
  `Information`, for example) - that turns on every internal diagnostic in that
  category at once (hosting, routing, MVC action invocation, result execution),
  producing a wall of output for what should be one line. This applies to both
  layers, not just backend.

### Migrations - do not skip these

**A migration is not done until it has been applied.** Creating one only writes
files; the database is unchanged until `database update` runs. Never end a task
having added a migration without applying it - the next person to pull will have
a model and a database that disagree.

```bash
cd backend
dotnet ef migrations add <Name> --project SportForAlle.Api --startup-project SportForAlle.Api --output-dir Data/Migrations
dotnet ef database update --project SportForAlle.Api --startup-project SportForAlle.Api
```

**Before running either, confirm the connection string points at the Docker
database.** `ConnectionStrings__DefaultConnection` in the root **`.env`**
(loaded by `DotNetEnv` in `Program.cs` - see `docs/11-utviklingsmiljo.md`; it
is not in `appsettings.Development.json`) must use **`Host=localhost;Port=5433`**,
and the container must be up:

```bash
docker compose up -d db
```

Port 5433 is deliberate. A natively installed PostgreSQL usually holds 5432, and
this machine has one running. Pointing at 5432 silently reaches that server
instead - the symptom is `password authentication failed for user
"sportforalle"` even though the Docker setup is correct. If you see that error,
check the port before anything else.

Verify the result rather than assuming it: read the generated migration before
applying it (see the `ef-migration` skill), and confirm the schema afterwards.

```bash
docker exec -e PGPASSWORD=change-me-locally sportforalle-db \
  psql -U sportforalle -d sportforalle -c "\dt"
```

Then add a row to the migration table in `docs/04-databasedesign.md`.

### Frontend

- App Router, server components by default; client components only when needed.
- All visible text in Norwegian. Keep strings out of deeply nested components so
  they stay easy to review.
- Every page under the authenticated area must be unusable when logged out.
  Protect at the route/middleware level, not by hiding buttons.
- Tailwind utilities in the markup; extract a component rather than inventing a
  custom CSS layer.
- Types for API responses are defined once and shared, not re-declared per page.

### Git

- Branch per unit of work, conventional-commit style messages
  (`feat:`, `fix:`, `docs:`, `chore:`, `test:`).
- CI must be green before merging to `main`.

## Commands

Docker Compose is the normal way to run everything. These become available as
each layer is scaffolded.

Database in Docker, backend and frontend run locally.

```bash
# Database (published on host port 5433 - a local PostgreSQL usually holds 5432)
docker compose up -d db

# Backend
cd backend/SportForAlle.Api && dotnet run     # http://localhost:5080
cd backend && dotnet test
cd backend && dotnet format

# EF Core migrations (see the ef-migration skill)
cd backend
dotnet ef migrations add <Name> --project SportForAlle.Api --startup-project SportForAlle.Api --output-dir Data/Migrations
dotnet ef database update --project SportForAlle.Api --startup-project SportForAlle.Api

# Frontend
cd frontend
bun install
bun dev          # http://localhost:3000
bun run test     # Jest
bun run lint
bun run build
```

## Testing

Tests are written alongside the code from the start, not retrofitted at the end.

- **Unit tests (xUnit)** cover domain rules. The blocked-loan rule, the state
  transitions and the late-return counter are the highest-value tests in the
  project - they are what the system exists to do.
- **Integration tests** cover endpoints against a real PostgreSQL container.
- **Frontend (Jest + Testing Library)** covers component behaviour and the small
  amount of logic in `lib/`. Run with `bun run test`.
- **End-to-end** is not set up yet, and no tool has been chosen. Do not claim
  coverage that does not exist.
- Never assert against the system clock directly. Inject a clock abstraction so
  overdue logic is testable.

## Documentation rules

`docs/` is part of the deliverable, written in **Norwegian**. Use the
`documentation` skill; it carries the detail.

- Update the relevant `docs/` file in the same change as the code. Do not batch
  documentation to the end of the project.
- A technical choice gets an ADR (`docs/adr/`). Use the `adr` skill.
- A new package gets a row in `docs/12-lisenser-og-vilkar.md` with its licence,
  when it is added.
- Every layer must be explained and connected - frontend, backend, database,
  auth, infra, CI. No layer left as a black box.
- Keep `docs/README.md` (the index) accurate when adding a document.
- Write for a reader who has not seen the code: explain *why*, not just *what*.
- Diagrams as Mermaid in Markdown so they stay diffable in git.
- Be honest about gaps. A documented limitation is worth more than a claim that
  falls over under one question. Never document something as implemented before
  it is - mark it planned, with the day it belongs to.

## Skills

| Skill | Use it for |
| --- | --- |
| `do-work` | Any task that changes the repository. Understand, plan, **wait for approval**, implement, tests must pass, document, stage without committing. |
| `grill-me` | Adversarially pressure-test a plan or choice before building it. Offered at the `do-work` plan gate, and available on demand. |
| `documentation` | Writing and auditing `docs/` - per-change sync, legal and GDPR, licensing, layer coverage, handoff-readiness. |
| `adr` | Recording a technical decision as an ADR. |
| `ef-migration` | Creating and applying an EF Core migration safely. |
| `backend-endpoint` | Adding an API endpoint through every layer, with tests. |
| `frontend-page` | Adding a protected Next.js page wired to the API. |
| `worklog` | Logging the day's work for the final report. |

## Security and GDPR guardrails

This system stores personal data about **children** for a municipally owned
customer, which needs particular care under GDPR.

- **Data minimisation.** Collect only what the workflow in the plan requires.
  A birth date is needed for age-group reporting and eligibility - a full
  national identity number (fødselsnummer) is **not**, and must never be stored.
- Equipment photos are evidence. Photograph the equipment, never the child.
- Guardian contact details are used for overdue follow-up only.
- Never log personal data. No names, emails or notes in application logs.
- Authorisation is checked **server-side on every endpoint**. A `Staff`-only
  endpoint must reject a `User` token even if the UI never shows the button.
- Secrets come from environment variables. Never commit a real secret; add any
  new variable to `.env.example` with a comment.
- Reports to the municipality are aggregated. They must not expose individuals.

## Working agreement

**The developer sets the pace. You do not run ahead of it.**

This project is built stage by stage, and many functional details are still
undecided. Building something that has not been agreed is worse than building
nothing, because it has to be understood and then unpicked.

### Ask, do not guess

- **When you are unsure, ask.** This overrides the usual instinct to pick a
  sensible default and carry on. A wrong guess here costs more than a question.
- Ask *before* building, not after. A question raised once the code exists is a
  question raised too late.
- If a request could reasonably be read two ways, say so and ask which. Do not
  pick the more likely reading silently.
- Do not invent scope. If something seems missing or worth adding, raise it as a
  suggestion and wait - do not build it because it seems obviously needed.
- Distinguish clearly between what has already been specified, what has been
  decided in conversation, and what you are inferring. Never present an inference
  as a decision.

### Stay inside the current stage

- Build what was asked for in this stage, and stop there.
- Do not scaffold, stub or pre-build the next stage "while you are in there".
- If finishing the current task requires something from a later stage, say so and
  ask, rather than quietly building it.

### Git

- **Never commit without explicit approval.** Stage the changes with `git add`
  and stop. Report what is staged and wait.
- This applies on a feature branch exactly as it does on `main`. A branch is not
  permission.
- Never push, merge, or open a pull request unless asked directly.
- Creating a branch is fine. Committing to it is not.

### Dev servers

- If the user's own `dotnet run` or `bun dev` is holding a file lock that
  blocks a build, or a dev server needs a clean restart to test something,
  stop it and start it again yourself - don't just ask and wait. This is a
  standing exception to asking before killing a process, scoped to this
  project's own frontend/backend dev servers specifically.
- **When something looks visually or behaviorally wrong and the source code
  doesn't explain it, restart clean before debugging further.** Turbopack and
  `dotnet run` have both produced stale state this project that looked like
  real bugs - a route handler change not taking effect, styling that appeared
  unrendered - and were fixed by a full stop/restart (`rm -rf .next` for the
  frontend), not a code change. Rule out staleness first with a clean restart
  and a direct check (curl the actual output, read the generated CSS) before
  spending time chasing a hypothesis the code doesn't support.

### Implementation

- Plan before implementing, especially for a new layer or feature area. Say what
  you intend to change and why, and get agreement, before making a wide change.
- Prefer finishing one vertical slice end-to-end over half-building three.
- Report honestly. If tests fail, say so with the output. If something was
  skipped or is incomplete, say that plainly rather than rounding up to done.
