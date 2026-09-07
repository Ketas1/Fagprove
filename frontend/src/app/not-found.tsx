import { CompassIcon } from 'lucide-react';
import { auth0 } from '@/lib/auth0';
import { Button } from '@/components/ui/button';

/**
 * Root `app/not-found.tsx`, per node_modules/next/dist/docs - this also
 * catches any unmatched URL for the whole app, not just an explicit
 * `notFound()` call from a page. Matches the landing page's visual style
 * (app/page.tsx).
 */
export default async function NotFound() {
  const session = await auth0.getSession();
  const homeHref = session ? '/dashboard' : '/';

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-5 p-8 text-center">
      <div className="flex size-[60px] items-center justify-center rounded-2xl bg-primary">
        <CompassIcon className="size-7 text-primary-foreground" aria-hidden="true" />
      </div>
      <div className="max-w-sm">
        <h1 className="text-[19px] font-semibold tracking-tight">Fant ikke siden</h1>
        <p className="mt-2.5 text-[13.5px] leading-relaxed text-muted-foreground">
          Siden du leter etter finnes ikke, eller er flyttet.
        </p>
      </div>
      <Button nativeButton={false} render={<a href={homeHref} />}>
        {session ? 'Til oversikten' : 'Til forsiden'}
      </Button>
    </main>
  );
}
