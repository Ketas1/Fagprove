import { loadEnvConfig } from "@next/env";
import path from "path";
import { Auth0Client } from "@auth0/nextjs-auth0/server";

// Proxy (middleware) runs in its own execution context and does not inherit
// the process.env populated by next.config.ts's loadEnvConfig call, so this
// module - imported by proxy.ts - loads it again itself. See
// docs/11-utviklingsmiljo.md.
loadEnvConfig(path.join(process.cwd(), ".."));

// AUTH0_AUDIENCE is not one of the environment variables the SDK reads on
// its own (unlike AUTH0_DOMAIN/AUTH0_CLIENT_ID/AUTH0_SECRET) - it must be
// passed explicitly, or Auth0 issues an opaque token for the default
// audience instead of a JWT access token the API can validate.
export const auth0 = new Auth0Client({
  authorizationParameters: {
    audience: process.env.AUTH0_AUDIENCE,
  },
});
