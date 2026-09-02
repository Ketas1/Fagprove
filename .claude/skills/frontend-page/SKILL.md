---
name: frontend-page
description: Add a page to the Next.js frontend, wired to the backend API and protected behind authentication. Use when building the dashboard, equipment list, borrower list, loan registration, overdue follow-up or reports pages. Covers route protection, Norwegian UI text and handling the blocked-loan error properly.
---

# Add a frontend page

## Rules that are not negotiable

1. **All visible text is Norwegian.** Code, props, component and file names are
   English. See `docs/adr/0010-domenespraak-engelsk-i-kode.md`.
2. **Protection happens in middleware**, not by hiding buttons. Someone who types
   the URL directly while logged out must be redirected to login, never shown the
   page shell.
3. **Server components by default.** Add a client component only where there is
   real interactivity - a form, a filter, a modal.
4. **The access token stays on the server.** Never put it in `localStorage` or
   pass it into client code.

## Steps

1. **Place the route.** Anything requiring login goes under `(dashboard)`.
   Login and logout live under `(auth)`.

2. **Fetch on the server** where possible, through the shared API client in
   `lib/`. Do not re-declare response types per page - they live in `types/`.

3. **Build the UI** with Tailwind utilities. Extract a component into
   `components/` when markup repeats, rather than adding a custom CSS layer.

4. **Handle every state.** A page that only handles the happy path will be
   noticed:

   - loading
   - empty (no equipment yet, no overdue loans - say so in Norwegian, do not
     render an empty table)
   - error (the API is unreachable)
   - the blocked action, described below

5. **Handle the blocked loan properly.** When `POST /api/loans` returns `409`,
   read `reason` from the `ProblemDetails` body and show the matching Norwegian
   explanation:

   | reason | Melding til ansatt |
   | --- | --- |
   | `BorrowerHasOverdueLoan` | Låntakeren har et forfalt lån som ikke er levert. |
   | `BorrowerIsBanned` | Låntakeren er utestengt. Gebyr må betales i butikken. |
   | `EquipmentNotAvailable` | Utstyret er ikke ledig. |
   | `BorrowerHasNoGuardian` | Barnet må knyttes til en foresatt før utlån. |
   | `BorrowerOutsideAgeRange` | Låntakeren er utenfor aldersgrensen 3-18 år. |

   Never show a raw status code or an English error string to a member of staff.
   Explaining why the loan was blocked is the mechanism that gets the equipment
   back - see `docs/01-losningsbeskrivelse.md`.

6. **Present the unreliable marking with its evidence.** Show the number of late
   returns alongside it, never a bare label. Required by
   `docs/09-lover-og-regler.md`.

7. **Document it** in `docs/10-bruk-av-systemet.md` once the page is working,
   with a screenshot.

## Checklist

- [ ] Route is protected in middleware, verified while logged out
- [ ] All visible text is Norwegian
- [ ] Loading, empty and error states handled
- [ ] `409` reasons mapped to Norwegian explanations
- [ ] Response types come from `types/`, not redeclared
- [ ] No token or personal data in client-side code
- [ ] `bun run lint` and `bun run build` pass
