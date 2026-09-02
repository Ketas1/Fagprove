# Frontend

Next.js application. Not scaffolded yet.

## Planned structure

```
frontend/
├── Dockerfile
├── package.json
└── src/
    ├── app/
    │   ├── (auth)/        Login and logout
    │   └── (dashboard)/   Protected area - equipment, users, loans, reports
    ├── components/        Reusable UI components
    ├── lib/               API client, auth helpers
    └── types/             Shared types for API responses
```

Everything under `(dashboard)` is protected in middleware, not by hiding
buttons. All visible text is in Norwegian; code and component names are in
English.

See [`docs/02-arkitektur.md`](../docs/02-arkitektur.md) and the `frontend-page`
skill in `.claude/skills/`.
