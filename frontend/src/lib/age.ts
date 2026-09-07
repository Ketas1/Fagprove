/// The three fixed age groups reports are broken down by, see CLAUDE.md and
/// docs/03-domenemodell.md. `null` means outside the scheme's 3-18 range.
export type AgeGroup = '3-6' | '7-12' | '13-18';

export function calculateAge(dateOfBirth: string, at: Date): number {
  const birth = new Date(dateOfBirth);
  let age = at.getFullYear() - birth.getFullYear();
  const hasHadBirthdayThisYear =
    at.getMonth() > birth.getMonth() ||
    (at.getMonth() === birth.getMonth() && at.getDate() >= birth.getDate());

  if (!hasHadBirthdayThisYear) {
    age -= 1;
  }

  return age;
}

export function ageGroup(age: number): AgeGroup | null {
  if (age >= 3 && age <= 6) return '3-6';
  if (age >= 7 && age <= 12) return '7-12';
  if (age >= 13 && age <= 18) return '13-18';
  return null;
}
