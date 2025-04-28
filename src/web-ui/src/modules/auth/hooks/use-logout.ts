import { toast } from "sonner";
import { useRouter } from "next/navigation";

import { trpc } from "@/trpc/client";

export const useLogout = () => {
  const router = useRouter();

  const { isLoading: isAuthLoading, data: user } =
    trpc.auth.isAuthenticated.useQuery();

  const logout = trpc.auth.logout.useMutation({
    onError: (error) => {
      toast.error(error.message);
    },
    onSuccess: () => {
      toast.success("Logged out!");
      router.push("/login");
    },
  });

  const handleLogout = () => {
    // Avoid triggering logout if authentication query is still loading
    if (isAuthLoading || !user) {
      return;
    }

    logout.mutate();
  };

  return {
    isLoading: isAuthLoading || logout.isPending,
    handleLogout,
    isLoggedOut: user === null || user === undefined,
  };
};
