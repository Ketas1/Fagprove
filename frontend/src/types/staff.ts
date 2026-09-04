/// Shape returned by GET/POST /api/staff and POST /api/staff/{id}/link-me on the backend.
export type Staff = {
  id: string;
  name: string;
  auth0UserId: string | null;
};
