# Frontend

Next.js (App Router) with TypeScript, Tailwind CSS and Bun.

## Structure

```
frontend/src/
├── app/               Pages (App Router)
├── components/        Reusable UI components
├── lib/               API client and helpers
└── types/             Shared types for API responses
```

## Conventions

- **All visible text is Norwegian.** Code, component and prop names are English.
- Server components by default; client components only where interactivity needs
  them.
- Response types live in `types/` and are not redeclared per page.
- Once authentication exists, the logged-in area is protected in middleware -
  not by hiding buttons.

## Running

```bash
bun install
bun dev
```

Runs on http://localhost:3000. The front page shows the status of the API and
database, so it is immediately visible whether all three layers are connected.
It expects the backend on http://localhost:5080; override with
`NEXT_PUBLIC_API_BASE_URL`.

## Tests

Jest with Testing Library, wired through `next/jest`.

```bash
bun run test
bun run test:watch
bun run lint
bun run build
```
