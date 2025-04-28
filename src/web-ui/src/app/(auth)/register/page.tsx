import { redirect } from "next/navigation";

import { RegisterView } from "@/modules/auth/ui/views/register-view";

import { trpc } from "@/trpc/server";

const Page = async () => {
  const user = await trpc.auth.isAuthenticated();
  if (user) {
    return redirect("/");
  }

  return <RegisterView />;
};

export default Page;
