import type { Metadata } from "next";

import { ProfileView } from "@/modules/users/ui/views/profile-view";

export const metadata: Metadata = { title: "Profile" };

// The user, cash and positions are prefetched by the (app) layout.
const Page = () => <ProfileView />;

export default Page;
