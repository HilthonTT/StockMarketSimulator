import { useTransition } from "react";

import { logout } from "../server/actions";

export const useLogout = () => {
  const [isLoggingOut, startTransition] = useTransition();

  const handleLogout = () => {
    startTransition(async () => {
      await logout();
    });
  };

  return { handleLogout, isLoggingOut };
};
