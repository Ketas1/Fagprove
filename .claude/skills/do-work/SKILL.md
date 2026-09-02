---
name: do-work
description: The default way of taking on any development task in this project. Runs understand, then plan, then a hard stop for approval, then implement, then tests must pass, then document, then stage without committing. Use for any feature, layer, fix or piece of setup - anything that changes the repository. The approval gate and the test gate are not optional.
---

# do-work

The developer sets the pace. This skill exists to stop work running ahead of
agreement, and to stop tasks being called finished when they are not.

Six phases, in order. **Do not skip ahead, and do not merge phases.**

---

## Phase 1 - Understand

Before proposing anything, work out what is actually being asked.

- Restate the task in your own words, including what you believe is *out* of
  scope. A restatement that only covers the easy reading is not a restatement.
- Read what already exists: `CLAUDE.md`, the relevant document in `docs/`, the
  ADRs, and the code touching this area. Do not plan against assumptions the repo
  can answer.
- **List every genuine uncertainty and ask.** Not rhetorically - actually stop
  and ask.

Ask when:

- the request could reasonably be read more than one way
- a functional detail has not been decided yet (many still have not)
- the work touches personal data, and the handling is not already documented
- a choice would be hard to reverse later
- it is not clear whether something belongs to this stage or a later one

Do **not** silently pick a default and continue. In this project a wrong guess
costs more than a question. If a question is small, ask it anyway.

Only when the uncertainties are resolved, move on.

---

## Phase 2 - Plan and propose

Propose a solution. Do not write implementation code yet.

The proposal covers:

| | |
| --- | --- |
| **Goal** | What this achieves, in one or two sentences |
| **Approach** | How, concretely - which layers, which files, which shape |
| **Alternatives** | What else was considered and why it lost. At least one real alternative. |
| **Impact** | What changes: files, schema, API surface, documentation |
| **Testing** | What will be tested and how it will be proven to work |
| **Out of scope** | What this deliberately does not do |
| **Open questions** | Anything still unresolved |

Keep it readable. A plan nobody finishes reading is not a plan.

Then offer the grill:

> Vil du at jeg skal kjøre `/grill-me` på denne planen først?

Offer it - never force it. If the answer is yes, run the `grill-me` skill against
the plan, then come back with a revised proposal.

---

## Phase 3 - Approval gate (hard stop)

**Stop. Wait for explicit approval.**

- Silence is not approval. A question about the plan is not approval. "Sounds
  reasonable" is not approval to build the whole thing - if in doubt, confirm.
- Do not start implementing "the obvious part" while waiting.
- If the plan is changed during discussion, restate what you are now going to
  build and get agreement on that.

---

## Phase 4 - Implement

Build what was approved. Nothing else.

- Stay inside the approved scope. If something needed turns up mid-way, stop and
  raise it rather than expanding the change.
- Follow the conventions in `CLAUDE.md`: layering, English code and Norwegian UI,
  business rules in the domain layer, server-side authorisation, no personal data
  in logs.
- If a state transition is involved, check it against the state machines in
  `docs/03-domenemodell.md`. A transition that is not an arrow there is a bug.
- If it turns out the approved approach does not work, stop and say so. Do not
  quietly substitute a different design.

---

## Phase 5 - Tests must pass (hard gate)

A job is not finished until this passes. No exceptions, no "I will add tests
after".

### Application code - frontend or backend

Unit tests are **required**, and they must **actually run and pass**.

- Backend: xUnit. Cover the rule, including the failure case - the path that
  should be *rejected* matters more than the happy path here.
- Frontend: cover the logic worth covering. Do not write assertions that only
  restate the implementation.
- Run them. Paste the real result.
- **If tests fail, the job is not done.** Report the failure with its output.
  Never describe failing work as complete.

Highest-value cases in this project, per `docs/07-testing.md`:
blocked loan for an overdue borrower, blocked loan for a banned borrower,
automatic overdue detection, late-return counter and unreliable flag, invalid
state transitions being rejected, `401` and `403` on protected endpoints.

### Configuration and infrastructure - Docker, CI, config, tooling

Unit tests usually do not apply. Verification still does. **Prove it works and
say exactly what you proved:**

- Docker or compose changes: start it and show the result
- CI workflow: validate the file, and say whether the run itself was checked
- Config or environment variables: show the application reading them
- A file format: parse or validate it

Then state plainly what was verified and what was not. "Should work" is not
verification, and neither is "the file looks correct".

### Documentation-only changes

No tests. Check that internal links resolve and that the content matches what
the code actually does.

---

## Phase 6 - Document, then stage

1. **Update the documentation** in the same piece of work, using the
   `documentation` skill. A feature is not done until `docs/` reflects it.
2. **Write an ADR** if a technical choice was made, using the `adr` skill.
3. **Stage the changes:**

   ```bash
   git add -A
   ```

4. **Stop there. Do not commit.** Not on `main`, not on a feature branch. A
   branch is not permission. Never push, merge or open a pull request unless
   asked directly.

5. **Report:**
   - what was built
   - test results, as they actually came out
   - what was verified, and how
   - what is staged
   - anything left incomplete, and why
   - anything found along the way that was deliberately not touched

Report honestly. If part of it does not work, say so in the first line rather
than the last.
