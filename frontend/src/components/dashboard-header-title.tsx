'use client';

import { usePathname } from 'next/navigation';
import { isNavItemActive, navItems } from '@/lib/nav-items';

export function DashboardHeaderTitle() {
  const pathname = usePathname();
  const current = navItems.find((item) => isNavItemActive(item.href, pathname));

  return <span className="text-[13.5px] font-semibold">{current?.label ?? 'Sport For Alle'}</span>;
}
