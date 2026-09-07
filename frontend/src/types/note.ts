/// Shape returned by GET/POST /api/borrowers/{id}/notes on the backend.
export type Note = {
  id: string;
  borrowerId: string;
  text: string;
  createdAt: string;
  createdByStaffId: string | null;
};
