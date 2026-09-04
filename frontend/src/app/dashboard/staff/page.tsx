import { StaffLinkPanel } from '@/components/StaffLinkPanel';

export const dynamic = 'force-dynamic';

export default function StaffPage() {
  return (
    <div className="flex flex-col gap-4">
      <h1 className="text-xl font-semibold">Ansattprofiler</h1>
      <StaffLinkPanel />
    </div>
  );
}
