import { redirect } from "next/navigation";

import { LoginView } from "@/modules/auth/ui/views/login-view";
import { fetchUser } from "@/modules/auth/server/caller";

const Page = async () => {
  const user = await fetchUser();
  if (user) {
    return redirect("/");
  }

  return <LoginView />;
};

export default Page;
