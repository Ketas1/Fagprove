import { StaffLinkPanel } from '@/components/StaffLinkPanel';
import { StaffTable } from '@/components/staff/staff-table';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { fetchBackend } from '@/lib/backend';
import type { Staff } from '@/types/staff';

export const dynamic = 'force-dynamic';

export default async function StaffPage() {
  const staff = await fetchBackend<Staff[]>(['staff']);

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardHeader>
          <CardTitle>Ansatte</CardTitle>
        </CardHeader>
        <CardContent>
          <StaffTable staff={staff} />
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
