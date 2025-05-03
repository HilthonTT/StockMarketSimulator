import { redirect } from "next/navigation";

import { RegisterView } from "@/modules/auth/ui/views/register-view";

import { caller } from "@/trpc/server";

const Page = async () => {
  const user = await caller.auth.current();
  if (user) {
    return redirect("/");
  }

  return <RegisterView />;
};

export default Page;
