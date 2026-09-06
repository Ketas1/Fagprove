/// Shape returned by GET/POST/PUT /api/guardians on the backend.
export type Guardian = {
  id: string;
  name: string;
  email: string;
  phone: string;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};
