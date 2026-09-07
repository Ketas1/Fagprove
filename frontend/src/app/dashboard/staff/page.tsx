import { StaffLinkPanel } from '@/components/StaffLinkPanel';
import { NotBuiltYetBadge } from '@/components/not-built-yet';
import { StatusBadge } from '@/components/status-badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { fetchBackend } from '@/lib/backend';
import type { Staff } from '@/types/staff';

export const dynamic = 'force-dynamic';

export default async function StaffPage() {
  const staff = await fetchBackend<Staff[]>(['staff']);

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardHeader className="flex-row items-center justify-between">
          <CardTitle>Ansatte</CardTitle>
          <div className="flex items-center gap-2">
            <NotBuiltYetBadge reason="Rolle, e-post og sist innlogget kommer fra Auth0 og rollebasert autorisasjon, som ikke er bygget ennå (se ADR-0019). Staff-modellen har foreløpig bare navn og om kontoen er koblet." />
            <NotBuiltYetBadge reason="En admin-invitasjon (e-post og rolle til en annen ansatt) finnes ikke som eget endepunkt ennå - i dag oppretter og kobler hver ansatt sin egen profil, se panelet under." />
          </div>
        </CardHeader>
        <CardContent>
          {staff.length === 0 ? (
            <p className="py-6 text-center text-sm text-muted-foreground">Ingen ansattprofiler ennå.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Navn</TableHead>
                  <TableHead>Konto</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {staff.map((member) => (
                  <TableRow key={member.id}>
                    <TableCell className="font-medium">{member.name}</TableCell>
                    <TableCell>
                      {member.auth0UserId ? (
                        <StatusBadge tone="success">Koblet</StatusBadge>
                      ) : (
                        <StatusBadge tone="neutral">Ikke koblet</StatusBadge>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Din ansattprofil</CardTitle>
        </CardHeader>
        <CardContent>
          <StaffLinkPanel />
        </CardContent>
      </Card>
    </div>
  );
}
