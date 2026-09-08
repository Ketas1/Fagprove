import { ChartColumn, Home, Package, ShieldCheck, Tag, Users } from 'lucide-react';

/// The main sections, in the order the design's
/// sidebar shows them. Shared between the sidebar nav and the header title
/// so both read the current section from the same list.
export const navItems = [
  { href: '/dashboard', label: 'Oversikt', icon: Home },
  { href: '/dashboard/loans', label: 'Utlån', icon: Package },
  { href: '/dashboard/equipment', label: 'Utstyr', icon: Tag },
  { href: '/dashboard/borrowers', label: 'Barn og foresatte', icon: Users },
  // Not one of the design's 15 artboards - see docs/13-frontend-designsystem.md.
  { href: '/dashboard/reports', label: 'Rapporter', icon: ChartColumn },
  { href: '/dashboard/staff', label: 'Ansatte', icon: ShieldCheck },
] as const;

export function isNavItemActive(href: string, pathname: string): boolean {
  return href === '/dashboard' ? pathname === href : pathname.startsWith(href);
}
