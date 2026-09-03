import type { Health } from '@/types/health';

type Props = {
  health: Health | null;
};

/**
 * Shows whether the backend and database are reachable. Presentational only,
 * so it can be tested without fetching anything.
 */
export function HealthStatus({ health }: Props) {
  if (health === null) {
    return (
      <p className="text-red-600 dark:text-red-400">
        Får ikke kontakt med API-et.
      </p>
    );
  }

  const databaseUp = health.database === 'up';

  return (
    <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-2">
      <dt className="text-slate-500 dark:text-slate-400">API</dt>
      <dd className="font-medium text-emerald-600 dark:text-emerald-400">Kjører</dd>

      <dt className="text-slate-500 dark:text-slate-400">Database</dt>
      <dd
        className={
          databaseUp
            ? 'font-medium text-emerald-600 dark:text-emerald-400'
            : 'font-medium text-red-600 dark:text-red-400'
        }
      >
        {databaseUp ? 'Tilkoblet' : 'Ikke tilkoblet'}
      </dd>
    </dl>
  );
}
