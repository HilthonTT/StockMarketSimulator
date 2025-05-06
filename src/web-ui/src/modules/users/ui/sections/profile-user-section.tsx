"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";

import { ProfileBanner } from "../components/profile-banner";
import { ProfileInfo } from "../components/profile-info";

export const ProfileUserSection = () => {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <ErrorBoundary fallback={<p>Error</p>}>
        <ProfileUserSectionSuspense />
      </ErrorBoundary>
    </Suspense>
  );
};

const ProfileUserSectionSuspense = () => {
  const trpc = useTRPC();

  const { data: user } = useSuspenseQuery(trpc.users.getCurrent.queryOptions());
  const { data: budget } = useSuspenseQuery(trpc.budgets.getOne.queryOptions());

  return (
    <div className="flex flex-col">
      <ProfileBanner />
      <ProfileInfo user={user} budget={budget} />
    </div>
  );
};
