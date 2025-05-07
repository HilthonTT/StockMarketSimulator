import { redirect } from "next/navigation";

import { ProfileView } from "@/modules/users/ui/views/profile-view";

import { caller, getQueryClient, HydrateClient, trpc } from "@/trpc/server";

const Page = async () => {
  const user = await caller.auth.current();
  if (!user) {
    return redirect("/login");
  }

  const queryClient = getQueryClient();

  void queryClient.prefetchQuery(trpc.users.getCurrent.queryOptions());
  void queryClient.prefetchQuery(trpc.budgets.getOne.queryOptions());

  return (
    <HydrateClient>
      <ProfileView />
    </HydrateClient>
  );
};

export default Page;
