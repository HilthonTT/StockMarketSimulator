import { redirect } from "next/navigation";

import { RegisterView } from "@/modules/auth/ui/views/register-view";
import { fetchUser } from "@/modules/auth/server/caller";

const Page = async () => {
  const user = await fetchUser();
  if (user) {
    return redirect("/");
  }

  return <RegisterView />;
};

export default Page;
