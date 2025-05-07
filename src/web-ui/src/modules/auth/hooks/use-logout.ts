import { toast } from "sonner";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";

import { logout } from "../server/actions";

export const useLogout = () => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const { mutate: handleLogout, isPending: isLoggingOut } = useMutation({
    mutationFn: logout,
    onSuccess: async () => {
      toast.success("Logged out!");

      await queryClient.invalidateQueries(trpc.auth.current.queryFilter());
      await queryClient.invalidateQueries(trpc.users.getCurrent.queryFilter());
      await queryClient.invalidateQueries(trpc.auth.getJwt.queryFilter());
    },
  });

  return { handleLogout, isLoggingOut };
};
