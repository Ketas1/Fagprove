---
name: ef-migration
description: Create and apply an Entity Framework Core migration after changing the domain model. Use whenever an entity, property, relationship or enum changes in SportForAlle.Domain, or when the database schema needs to change for any reason. Covers reviewing the generated migration before it runs, which is the step that prevents data loss.
---

# Entity Framework Core migration

The database is **Code First**: the model is the source of truth and the schema
is generated from it. Never change the database directly - if the schema is
wrong, the model is wrong.

## Before starting

Make sure the database is running:

```bash
docker compose up -d db
```

## Steps

1. **Change the model** in `SportForAlle.Domain`, and the configuration in
   `SportForAlle.Infrastructure` if the relationship, precision or constraint
   needs to be explicit.

2. **Generate the migration.** Name it after what it does, in PascalCase -
   `AddBanEntity`, `AddLateReturnCountToBorrower`.

   ```bash
   cd backend
   dotnet ef migrations add <Name> \
     --project src/SportForAlle.Infrastructure \
     --startup-project src/SportForAlle.Api
   ```

3. **Read the generated migration before applying it.** This is the step that
   matters. Look for:

   - **A rename that became a drop plus an add.** EF cannot tell a renamed
     property from a deleted one and a new one. `DropColumn` followed by
     `AddColumn` destroys the data in that column. Replace it with
     `RenameColumn` by hand.
   - **A new non-nullable column without a default**, on a table that already
     has rows. It will fail, or silently fill with a default that means nothing.
   - **Unintended drops** of tables, columns or indexes.
   - **A delete behaviour that is wrong for the domain.** Deleting a borrower must
     not cascade away the loan history the reports are built on - see
     `docs/09-lover-og-regler.md`.

4. **Apply it.**

   ```bash
   dotnet ef database update \
     --project src/SportForAlle.Infrastructure \
     --startup-project src/SportForAlle.Api
   ```

5. **Document it.** Add a row to the migration table in
   `docs/04-databasedesign.md`, and update the ER diagram and the table
   descriptions there if the change is structural.

6. **Commit the model change and the migration together.** They are one change.
   A migration without its model change, or the reverse, leaves the repository in
   a state that does not build.

## If the migration is wrong

As long as it has **not been applied**, remove it and start over:

```bash
dotnet ef migrations remove \
  --project src/SportForAlle.Infrastructure \
  --startup-project src/SportForAlle.Api
```

Once it has been applied, do not edit it. Write a new migration that corrects
the schema. Editing an applied migration makes the migration history disagree
with the database.

## Conventions for this project

- Timestamps are `timestamptz` and stored in UTC. Conversion to Norwegian time
  happens in the frontend.
- Enums are stored as **text**, not as integers, so the database stays readable
  and adding a value cannot shift the meaning of existing rows.
- `Borrower.GuardianId` is required. A child cannot exist in the system without
  a guardian.
- Index what is actually queried. The overdue-loans overview and the
  open-overdue-loan check on every new loan are the two hot paths.
