import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { auth } from "@/modules/auth/auth";
import { AppShell } from "@/modules/shell/ui/layouts/app-shell";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

interface LayoutProps {
  children: React.ReactNode;
}

const Layout = async ({ children }: LayoutProps) => {
  const session = await auth();

  if (!session?.user || session.error) {
    redirect("/login");
  }

  const cookieStore = await cookies();
  const defaultSidebarOpen = cookieStore.get("sidebar_state")?.value !== "false";

  // Data every signed-in page shows in the shell (cash, user menu, ticker tape, feed).
  const queryClient = getQueryClient();
  // Awaited (in parallel): these are read with useQuery, so the server render must already have
  // them, or the server HTML (loading state) and the hydrated client (data) differ.
  await Promise.all([
    queryClient.prefetchQuery(trpc.auth.getJwt.queryOptions()),
    queryClient.prefetchQuery(trpc.budgets.getOne.queryOptions()),
    queryClient.prefetchQuery(trpc.users.getCurrent.queryOptions()),
    queryClient.prefetchQuery(trpc.portfolio.getPositions.queryOptions()),
  ]);

  return (
    <HydrateClient>
      <AppShell defaultSidebarOpen={defaultSidebarOpen}>{children}</AppShell>
    </HydrateClient>
  );
};

export default Layout;
