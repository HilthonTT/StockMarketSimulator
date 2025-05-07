import { redirect } from "next/navigation";

import { ContactView } from "@/modules/contact/ui/views/contact-view";

import { caller, HydrateClient } from "@/trpc/server";

const Page = async () => {
  const user = await caller.auth.current();
  if (!user) {
    return redirect("/login");
  }

  return (
    <HydrateClient>
      <ContactView />
    </HydrateClient>
  );
};

export default Page;
