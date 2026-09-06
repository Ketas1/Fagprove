import { redirect } from 'next/navigation';
import { auth0 } from '@/lib/auth0';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { DashboardHeaderTitle } from '@/components/dashboard-header-title';
import { DashboardNav } from '@/components/dashboard-nav';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/sidebar';

export default async function DashboardLayout({ children }: { children: React.ReactNode }) {
  const session = await auth0.getSession();

  if (!session) {
    redirect('/auth/login');
  }

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <div className="flex items-center gap-2 px-2 py-1">
            <div className="size-[22px] shrink-0 rounded-[7px] bg-primary" />
            <span className="text-[13.5px] font-semibold tracking-tight">Sport For Alle</span>
          </div>
        </SidebarHeader>
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Meny</SidebarGroupLabel>
            <SidebarGroupContent>
              <DashboardNav />
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
      </Sidebar>

      <SidebarInset>
        <header className="flex h-13 items-center justify-between border-b px-6">
          <div className="flex items-center gap-3">
            <SidebarTrigger />
            <DashboardHeaderTitle />
          </div>
          <div className="flex items-center gap-2.5">
            <span className="text-sm text-muted-foreground">{session.user.email}</span>
            {/* Role-based auth isn't built yet (ADR-0019) - every signed-in
                user shows the same "Ansatt" label until it is. */}
            <Badge variant="secondary">Ansatt</Badge>
            <Button
              variant="outline"
              size="sm"
              nativeButton={false}
              render={<a href="/auth/logout">Logg ut</a>}
            />
          </div>
        </header>

        <main className="flex flex-1 flex-col gap-6 p-6">{children}</main>
      </SidebarInset>
    </SidebarProvider>
  );
}
