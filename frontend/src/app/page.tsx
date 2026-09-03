import { redirect } from 'next/navigation';
import { auth0 } from '@/lib/auth0';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export const dynamic = 'force-dynamic';

export default async function Home() {
  const session = await auth0.getSession();

  if (session) {
    redirect('/dashboard');
  }

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-8">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Sport For Alle</CardTitle>
        </CardHeader>
        <CardContent>
          <Button
            className="w-full"
            nativeButton={false}
            render={<a href="/auth/login">Logg inn</a>}
          />
        </CardContent>
      </Card>
    </main>
  );
}
