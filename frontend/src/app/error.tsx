'use client';

import { AlertTriangle, RotateCw } from 'lucide-react';
import { Button } from '@/components/ui/button';

/**
 * React error boundary for unexpected runtime errors anywhere in the app -
 * distinct from the auth callback fix in lib/auth0.ts, which handles a
 * different failure surface (the SDK's own route handler, which never
 * throws into the React tree, so this boundary can't catch it). `retry` is
 * this Next.js version's prop name (stable as of 16.3.0, was `reset`
 * before) - confirmed against node_modules/next/dist/docs, not assumed.
 */
export default function Error({ retry }: { error: Error & { digest?: string }; retry: () => void }) {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-5 p-8 text-center">
      <div className="flex size-[60px] items-center justify-center rounded-2xl bg-destructive/10">
        <AlertTriangle className="size-7 text-destructive" />
      </div>
      <div className="max-w-sm">
        <h1 className="text-[19px] font-semibold tracking-tight">Noe gikk galt</h1>
        <p className="mt-2.5 text-[13.5px] leading-relaxed text-muted-foreground">
          En uventet feil oppstod. Prøv igjen - hvis problemet vedvarer, kontakt IT-support.
        </p>
      </div>
      <Button onClick={() => retry()}>
        <RotateCw /> Prøv igjen
      </Button>
    </main>
  );
}
