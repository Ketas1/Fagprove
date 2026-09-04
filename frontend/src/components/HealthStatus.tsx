import type { Health } from '@/types/health';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

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
      <Card>
        <CardHeader>
          <CardTitle>Status</CardTitle>
        </CardHeader>
        <CardContent>
          <Badge variant="destructive">Får ikke kontakt med API-et.</Badge>
        </CardContent>
      </Card>
    );
  }

  const databaseUp = health.database === 'up';

  return (
    <Card>
      <CardHeader>
        <CardTitle>Status</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        <div className="flex items-center justify-between">
          <span className="text-muted-foreground">API</span>
          <Badge>Kjører</Badge>
        </div>

        <div className="flex items-center justify-between">
          <span className="text-muted-foreground">Database</span>
          <Badge variant={databaseUp ? 'default' : 'destructive'}>
            {databaseUp ? 'Tilkoblet' : 'Ikke tilkoblet'}
          </Badge>
        </div>
      </CardContent>
    </Card>
  );
}
