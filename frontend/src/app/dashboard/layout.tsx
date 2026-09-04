import { redirect } from 'next/navigation';
import { auth0 } from '@/lib/auth0';
import { Button } from '@/components/ui/button';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
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
          <span className="px-2 text-sm font-semibold">Sport For Alle</span>
        </SidebarHeader>
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupContent>
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton isActive render={<a href="/dashboard">Status</a>} />
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton render={<a href="/dashboard/staff">Ansatte</a>} />
                </SidebarMenuItem>
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
      </Sidebar>

      <SidebarInset>
        <header className="flex h-12 items-center justify-between border-b px-4">
          <SidebarTrigger />
          <div className="flex items-center gap-3">
            <span className="text-muted-foreground text-sm">{session.user.email}</span>
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
