'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { SidebarMenu, SidebarMenuButton, SidebarMenuItem } from '@/components/ui/sidebar';
import { isNavItemActive, navItems } from '@/lib/nav-items';

export function DashboardNav() {
  const pathname = usePathname();

  return (
    <SidebarMenu>
      {navItems.map(({ href, label, icon: Icon }) => {
        const isActive = isNavItemActive(href, pathname);

        return (
          <SidebarMenuItem key={href}>
            <SidebarMenuButton isActive={isActive} render={<Link href={href} />}>
              <Icon className={isActive ? 'text-primary' : undefined} />
              {label}
            </SidebarMenuButton>
          </SidebarMenuItem>
        );
      })}
    </SidebarMenu>
  );
}
