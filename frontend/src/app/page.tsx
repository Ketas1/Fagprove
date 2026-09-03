/* import { HealthStatus } from '@/components/HealthStatus';
import { apiUrl } from '@/lib/api';
import type { Health } from '@/types/health'; */
import { auth0 } from "@/lib/auth0";

// Nothing is cached: this page exists to show the current state of the stack.
export const dynamic = 'force-dynamic';

/* async function fetchHealth(): Promise<Health | null> {
  try {
    const response = await fetch(apiUrl('/api/health'), { cache: 'no-store' });

    if (!response.ok) {
      return null;
    }

    return (await response.json()) as Health;
  } catch {
    // The API not running is an expected state during development, not a crash.
    return null;
  }
} */

export default async function Home() {
/*   const health = await fetchHealth(); */  
  const session = await auth0.getSession();

  if (!session) {
    return (
      <>
        {/* Redirects to Auth0 to sign up */}
        <a href="/auth/login?screen_hint=signup">Signup</a>
        <br />
        {/* Redirects to Auth0 to log in */}
        <a href="/auth/login">Login</a>
      </>
    );
  }
  
  return (
    <>
      <p>Logged in as {session.user.email}</p>

      {/* Display user info (name, email, etc.) */}
      <h1>User Profile</h1>
      <pre>{JSON.stringify(session.user, null, 2)}</pre>

      {/* Ends the session and redirects to Auth0 to log out */}
      <a href="/auth/logout">Logout</a>
    </>
  );

  /* return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-8 p-8">
      <div className="text-center">
        <h1 className="text-2xl font-semibold">Sport For Alle</h1>
        <p className="mt-1 text-slate-500 dark:text-slate-400">
          Utlån av sportsutstyr
        </p>
      </div>

      <section className="rounded-lg border border-slate-200 p-6 dark:border-slate-700">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
          Systemstatus
        </h2>
        <HealthStatus health={health} />
      </section>
    </main>
  ); */
}
