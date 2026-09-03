import { loadEnvConfig } from "@next/env";
import path from "path";
import { Auth0Client } from "@auth0/nextjs-auth0/server";

// Proxy (middleware) runs in its own execution context and does not inherit
// the process.env populated by next.config.ts's loadEnvConfig call, so this
// module - imported by proxy.ts - loads it again itself. See
// docs/11-utviklingsmiljo.md.
loadEnvConfig(path.join(process.cwd(), ".."));

export const auth0 = new Auth0Client();
