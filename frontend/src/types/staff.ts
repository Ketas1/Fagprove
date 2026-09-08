/// Shape returned by GET/POST/PUT /api/staff and POST /api/staff/{id}/link-me on the backend.
///
/// `jobTitle` is the position in the shop ("Butikkleder") - it is not an
/// authorisation role, and nothing reads it to decide what a user may do.
/// See docs/adr/0019-staff-auth0-mapping.md. All three optional fields are
/// null when nothing has been recorded.
export type Staff = {
  id: string;
  name: string;
  jobTitle: string | null;
  email: string | null;
  phone: string | null;
  auth0UserId: string | null;
};

/// Body for POST /api/staff.
export type CreateStaffRequest = {
  name: string;
  jobTitle?: string;
  email?: string;
  phone?: string;
};

/// Body for PUT /api/staff/{id}. A blank optional field clears what was stored.
export type UpdateStaffRequest = {
  name: string;
  jobTitle?: string;
  email?: string;
  phone?: string;
};
