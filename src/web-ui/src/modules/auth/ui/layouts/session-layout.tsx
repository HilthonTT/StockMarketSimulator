"use client";

import { signOut, useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { JWT_INVALID_ERROR } from "../../constants";

interface SessionLayoutProps {
  children: React.ReactNode;
}

export const SessionLayout = ({ children }: SessionLayoutProps) => {
  const { data: sessionData } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (sessionData?.error === JWT_INVALID_ERROR) {
      signOut();
    }
  }, [sessionData?.error, router]);

  return <>{children}</>;
};
