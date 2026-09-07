import { redirect } from 'next/navigation';
import { AlertTriangle } from 'lucide-react';
import { auth0 } from '@/lib/auth0';
import { Button } from '@/components/ui/button';

export const dynamic = 'force-dynamic';

export default async function Home({
  searchParams,
}: {
  searchParams: Promise<{ authError?: string }>;
}) {
  const session = await auth0.getSession();
  const { authError } = await searchParams;

  if (session) {
    redirect('/dashboard');
  }

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-5 p-8 text-center">
      <div className="flex size-[60px] items-center justify-center rounded-2xl bg-primary">
        <svg
          viewBox="0 0 24 24"
          fill="none"
          stroke="white"
          strokeWidth="1.75"
          strokeLinecap="round"
          strokeLinejoin="round"
          width="28"
          height="28"
          aria-hidden="true"
        >
          <circle cx="12" cy="12" r="8.5" />
          <path d="M12 3.5c2.3 2.4 3.5 5.3 3.5 8.5s-1.2 6.1-3.5 8.5M12 3.5c-2.3 2.4-3.5 5.3-3.5 8.5s1.2 6.1 3.5 8.5M3.8 9h16.4M3.8 15h16.4" />
        </svg>
      </div>
      <div className="max-w-sm">
        <h1 className="text-[19px] font-semibold tracking-tight">Sport For Alle</h1>
        <p className="mt-2.5 text-[13.5px] leading-relaxed text-muted-foreground">
          Internt system for utlån av sportsutstyr til barn og unge. Logg inn med din ansattkonto for å se
          oversikten, registrere utlån og følge opp forfalte lån.
        </p>
      </div>

      {authError && (
        <div className="flex max-w-sm gap-2.5 rounded-lg bg-status-danger-bg p-3 text-left text-status-danger-fg">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <div className="text-[12.5px] leading-relaxed">
            <p className="font-medium">Innlogging feilet</p>
            <p>{authError}</p>
          </div>
        </div>
      )}

      <Button nativeButton={false} render={<a href="/auth/login">Logg inn</a>} />
      <p className="text-[11.5px] text-muted-foreground">
        Sport For Alle AS · utlånsordning i samarbeid med kommunen
      </p>
    </main>
  );
}
