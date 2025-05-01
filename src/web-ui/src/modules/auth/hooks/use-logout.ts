import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { useMutation, useQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";

export const useLogout = () => {
  const trpc = useTRPC();
  const router = useRouter();

  const { data: user, isLoading: isAuthLoading } = useQuery(
    trpc.auth.isAuthenticated.queryOptions()
  );

  const logout = useMutation(
    trpc.auth.logout.mutationOptions({
      onError: (error) => {
        toast.error(error.message);
      },
      onSuccess: () => {
        toast.success("Logged out!");
        router.push("/login");
      },
    })
  );

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
