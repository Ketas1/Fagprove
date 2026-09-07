import type { StatusTone } from '@/components/status-badge';
import type { LoanStatus } from '@/types/loan';
import type { EquipmentCondition, EquipmentStatus } from '@/types/equipment';
import type { BorrowerStatus } from '@/types/borrower';

/// Norwegian labels and badge colors for domain enums, kept in one place so
/// every page shows the same word and color for the same state - see the
/// status-color table in docs/13-frontend-designsystem.md.
export function loanStatusInfo(status: LoanStatus): { label: string; tone: StatusTone } {
  switch (status) {
    case 'Active':
      return { label: 'Aktiv', tone: 'info' };
    case 'Overdue':
      return { label: 'Forfalt', tone: 'danger' };
    case 'Returned':
      return { label: 'Levert', tone: 'success' };
    case 'Lost':
      return { label: 'Tapt', tone: 'solid' };
  }
}

export function equipmentStatusInfo(status: EquipmentStatus): { label: string; tone: StatusTone } {
  switch (status) {
    case 'Available':
      return { label: 'Ledig', tone: 'success' };
    case 'OnLoan':
      return { label: 'Utlånt', tone: 'info' };
    case 'OutOfService':
      return { label: 'Ute av drift', tone: 'warning' };
    case 'WrittenOff':
      return { label: 'Avskrevet', tone: 'neutral' };
  }
}

export function borrowerStatusInfo(status: BorrowerStatus): { label: string; tone: StatusTone } {
  switch (status) {
    case 'Active':
      return { label: 'Aktiv', tone: 'success' };
    case 'Banned':
      return { label: 'Utestengt', tone: 'danger' };
  }
}

export function equipmentConditionLabel(condition: EquipmentCondition): string {
  switch (condition) {
    case 'New':
      return 'Ny';
    case 'Good':
      return 'God';
    case 'Worn':
      return 'Slitt';
    case 'Damaged':
      return 'Skadet';
  }
}
