import { loadEnvConfig } from "@next/env";
import path from "path";
import type { NextConfig } from "next";

// The .env file lives at the repository root, not here, so the frontend and
// backend can share one file - see docs/11-utviklingsmiljo.md. Must run
// before the config object below: Next.js reads process.env both for
// server-side code and for inlining NEXT_PUBLIC_* values at build time.
loadEnvConfig(path.join(__dirname, ".."));

const nextConfig: NextConfig = {
  /* config options here */
};

export default nextConfig;
