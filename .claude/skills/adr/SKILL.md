---
name: adr
description: Record a technical decision as an Architecture Decision Record in docs/adr/. Use whenever a technology, library, pattern, schema shape or design approach is chosen - and especially when an earlier decision is being reversed. Decisions must be captured when they are made, not reconstructed later.
---

# Write an Architecture Decision Record

ADRs are written in **Norwegian**, like everything else in `docs/`.

## Steps

1. **Find the next number.** List `docs/adr/` and take the highest number plus
   one, zero-padded to four digits. Never reuse a number.

2. **Check whether this supersedes an existing ADR.** If the decision reverses or
   replaces an earlier one, the old ADR is **not edited or deleted**. Set its
   status to `Erstattet av ADR-XXXX` and explain in the new ADR what changed and
   why. The history of what was believed and why it changed is the most valuable
   part of the record.

3. **Write the file** as `docs/adr/XXXX-kort-tittel.md`, using
   `docs/adr/0000-mal.md` as the template. The filename is lowercase, words
   separated by hyphens, no æ/ø/å.

4. **Update the register** table in `docs/adr/README.md`.

5. **Link it** from the document that is affected - typically
   `11-utviklingsmiljo.md` for a tooling choice, `02-arkitektur.md` for a
   structural one, or `04-databasedesign.md` for a schema one.

## What makes an ADR worth having

The value is in the alternatives and the trade-offs, not the conclusion.

- **State the real alternatives.** At least two, with genuine advantages. An
  alternatives table where every other option is obviously bad means the thinking
  has not been done, and a reader will notice.
- **Be honest about the downsides.** Every decision costs something. An ADR with
  an empty "Negativt" section is not finished. If a choice really has no
  drawback, it was not a decision worth recording.
- **Explain the context first.** A reader should understand why the question came
  up before reading the answer.
- **Keep it short.** One page. If it needs to be read twice, it is too long.
- **Present tense for the decision:** "Systemet bruker X", not "Vi har bestemt at
  vi skal bruke X".

## Anchor it in this project

Where relevant, tie the decision back to what the project is actually about: the
three problems the system solves, the limited development timeframe, that the
system holds personal data about children for a municipally owned customer, or
that another IT department must be able to take the project over. A decision
justified by the project's own constraints reads very differently from one
justified by general preference.
