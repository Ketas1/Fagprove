/// RFC 7807 error shape returned by the backend on a rejected request, see
/// docs/05-api.md. `reason` is the machine-readable code (for example
/// `BorrowerHasOverdueLoan`, `BorrowerBanned`, `EquipmentNotAvailable`) set
/// on a DomainConflictException/ForbiddenException; `detail` is already a
/// user-facing Norwegian sentence, safe to show directly.
export type ProblemDetails = {
  status?: number;
  title?: string;
  detail?: string;
  type?: string;
  reason?: string;
};
