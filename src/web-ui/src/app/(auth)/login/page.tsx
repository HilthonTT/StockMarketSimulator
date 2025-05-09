import { redirect } from "next/navigation";

import { LoginView } from "@/modules/auth/ui/views/login-view";

import { caller } from "@/trpc/server";

const Page = async () => {
  const user = await caller.auth.current();
  if (user) {
    return redirect("/");
  }

  return <LoginView />;
};

export default Page;
