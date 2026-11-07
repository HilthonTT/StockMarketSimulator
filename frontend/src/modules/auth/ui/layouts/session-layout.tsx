"use client";

import { signOut, useSession } from "next-auth/react";
import { useEffect } from "react";

import { JWT_INVALID_ERROR } from "../../constants";

interface SessionLayoutProps {
  children: React.ReactNode;
}

export const SessionLayout = ({ children }: SessionLayoutProps) => {
  const { data: sessionData } = useSession();

  useEffect(() => {
    if (sessionData?.error === JWT_INVALID_ERROR) {
      void signOut({ redirectTo: "/login" });
    }
  }, [sessionData?.error]);

  return <>{children}</>;
};
