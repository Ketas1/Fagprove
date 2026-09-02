---
name: documentation
description: Write and maintain the Norwegian documentation in docs/. Use after any change, at the end of a working session, and to audit coverage. Covers per-change sync, legal and GDPR, package licensing and terms of service, explaining every layer, recording all decisions, and keeping the project handoff-ready for another IT department.
---

# Documentation

`docs/` is written in **Norwegian**, and it is part of the deliverable - not a
summary bolted on at the end. Documentation is produced *through* development.

## What the documentation has to achieve

Four things, and every judgement call comes back to one of them:

1. **Justify the choices.** Every technical and non-technical decision, with the
   alternatives and the reasoning. Recorded when made, not reconstructed later.
2. **Meet public-sector expectations.** The customer is a municipally owned
   business handling children's personal data. GDPR, data handling, relevant
   Norwegian regulation, package licensing and terms of service are expected
   content, not optional extras.
3. **Explain every layer.** Frontend, backend, database, authentication, infra
   and CI each have to be explained and connected. No layer left as a black box.
4. **Be handoff-ready.** Another IT department must be able to take this over,
   continue development, and reach a deployable state without much difficulty.

Remember what this is: a **proposal for a solution**, not a production system. It
does not need to be instantly deployable. It does need the path from here to
deployable to be visible and short. Document the gap - do not hide it, and do not
pretend it is closed.

The documentation must stand alone for someone who will never open the code.

---

## Mode 1 - After a change (routine)

Run this in the same piece of work as the change itself.

| Change | Document |
| --- | --- |
| Technology, library or pattern chosen | New ADR in `docs/adr/` - use the `adr` skill |
| Layering, structure, request flow | `02-arkitektur.md` |
| Entity, state machine, business rule, new term | `03-domenemodell.md` |
| Schema, table, relationship, index, migration | `04-databasedesign.md` |
| Endpoint, error contract | `05-api.md` |
| Auth flow, roles, protection | `06-autentisering.md` |
| Test strategy, coverage | `07-testing.md` |
| Security measure implemented | `08-sikkerhet.md` |
| Data stored, retention, privacy decision | `09-lover-og-regler.md` |
| UI page finished | `10-bruk-av-systemet.md` |
| Setup, tooling, file structure, CI | `11-utviklingsmiljo.md` |
| New package added, licence, external service terms | `12-lisenser-og-vilkar.md` |

Steps:

1. Look at what actually changed - `git diff`, or the work just completed.
2. Edit the existing section rather than appending a new one. Documentation that
   only grows by accretion becomes self-contradictory.
3. A new domain term goes in **two** places: the glossary in
   `03-domenemodell.md` and the one in `CLAUDE.md`. They must not drift.
4. A new package goes in `12-lisenser-og-vilkar.md` with its licence, at the
   point it is added - not in a sweep at the end.
5. Replace the `> Fyll ut ...` placeholder rather than leaving it beside the real
   content.
6. Update the status column in `docs/README.md` when a document moves from
   outline to written.

---

## Mode 2 - Coverage audit

Run periodically, and before finishing the project.

**Decisions**

- [ ] Does every technical choice have an ADR?
- [ ] Does every ADR referenced from a document exist and appear in the register?
- [ ] Are decisions taken in conversation written down anywhere, or only in chat?

**Layers**

- [ ] Is each layer - frontend, backend, database, auth, infra, CI - explained?
- [ ] Is it clear how they connect, not just what each one is?
- [ ] Could a reader who has never seen the code follow the path of a request?

**Legal and privacy**

- [ ] Is the legal basis for each kind of personal data stated?
- [ ] Is every stored field justified, and is anything stored that is not needed?
- [ ] Are retention and deletion covered?
- [ ] Are the data-subject rights covered?
- [ ] Is the automated blocking and unreliable-flagging addressed honestly?

**Licensing and terms**

- [ ] Is every dependency listed with its licence?
- [ ] Is any licence incompatible with a municipally owned customer?
- [ ] Are the terms of external services (Auth0, any email provider) covered,
      including where data is processed?

**Handoff**

- [ ] Can someone else set up and run this from the documentation alone?
- [ ] Is what remains before deployment listed explicitly?
- [ ] Are known limitations and shortcuts written down?

---

## Standards

- **Why, not what.** The code shows what happens. Documentation earns its place
  with the reasoning and the alternatives.
- **Norwegian**, correct and plain. Short sentences beat impressive ones.
- **Written for someone who has not seen the code.** A stated goal is that
  another developer can understand, run and extend the system.
- **Mermaid for diagrams**, so they are versioned and diffable rather than
  screenshots that go stale.
- **Be honest about gaps.** A documented limitation reads far better than a claim
  that collapses under one question. If something was cut for time, say so and
  say why. This is a proposal - unfinished parts are expected, hidden ones are
  not.
- **No invented facts.** Do not document a measure as implemented before it is.
  Mark it as planned, with the day it belongs to.

## Consistency check

- Every ADR referenced exists and is in the register in `docs/adr/README.md`
- The glossary in `CLAUDE.md` matches `03-domenemodell.md`
- Every environment variable used in code appears in `.env.example`
- The state machines in `CLAUDE.md` and `03-domenemodell.md` match the enums in
  the code
- Every internal link resolves
