import { redirect } from "next/navigation";

import { LoginView } from "@/modules/auth/ui/views/login-view";

import { trpc } from "@/trpc/server";

const Page = async () => {
  const user = await trpc.auth.isAuthenticated();
  if (user) {
    return redirect("/");
  }

  return <LoginView />;
};

export default Page;
