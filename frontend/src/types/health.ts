/// Shape returned by GET /api/health on the backend.
export type Health = {
  status: string;
  database: 'up' | 'down';
};
