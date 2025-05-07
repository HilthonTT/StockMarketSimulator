import { redirect } from "next/navigation";

import { AboutView } from "@/modules/about/ui/views/about-view";

import { caller, HydrateClient } from "@/trpc/server";

const Page = async () => {
  const user = await caller.auth.current();
  if (!user) {
    return redirect("/login");
  }

  return (
    <HydrateClient>
      <AboutView />
    </HydrateClient>
  );
};

export default Page;
