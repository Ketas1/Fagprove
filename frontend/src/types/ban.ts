/// Shape returned by GET/POST /api/borrowers/{id}/ban, POST .../ban/fee-paid on the backend.
export type Ban = {
  id: string;
  borrowerId: string;
  reason: string;
  createdAt: string;
  feePaidAt: string | null;
  liftedAt: string | null;
  isActive: boolean;
};
