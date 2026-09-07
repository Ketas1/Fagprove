import { loadEnvConfig } from "@next/env";
import path from "path";
import { NextResponse } from "next/server";
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
  // Confirmed by reading node_modules/@auth0/nextjs-auth0/dist/server/auth-client.js
  // directly (this project's AGENTS.md warns library behaviour can differ
  // from training data - it did here): the SDK's own default `onCallback`
  // returns a bare `new NextResponse(error.message, { status: 500 })` on a
  // failed login (denied consent, a misconfigured tenant, a token exchange
  // failure) - unstyled plain text, not a page. This override keeps the
  // success path identical to the default (redirect to `returnTo`, or `/`)
  // and only changes what happens on `error`: redirect to the landing page
  // with the message in a query param, so `app/page.tsx` can show it
  // through the app's own styling instead of a raw response.
  async onCallback(error, ctx) {
    const appBaseUrl = ctx.appBaseUrl;

    if (!appBaseUrl) {
      return new NextResponse(error?.message ?? "Ukjent innloggingsfeil.", { status: 500 });
    }

    if (error) {
      const url = new URL("/", appBaseUrl);
      url.searchParams.set("authError", error.message);
      return NextResponse.redirect(url);
    }

    return NextResponse.redirect(new URL(ctx.returnTo || "/", appBaseUrl));
  },
});
