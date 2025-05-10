"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";
import { Skeleton } from "@/components/ui/skeleton";

import { ProfileBanner } from "../components/profile-banner";
import { ProfileInfo } from "../components/profile-info";

export const ProfileUserSection = () => {
  return (
    <Suspense fallback={<ProfileUserLoading />}>
      <ErrorBoundary fallback={<p>Error</p>}>
        <ProfileUserSectionSuspense />
      </ErrorBoundary>
    </Suspense>
  );
};

const ProfileUserLoading = () => {
  return (
    <div className="flex flex-col gap-y-8">
      <Skeleton className="w-full h-[20vh] bg-white dark:bg-black rounded-xl" />

      <Skeleton className="w-full h-[500px] bg-white dark:bg-black rounded-xl" />
    </div>
  );
};

const ProfileUserSectionSuspense = () => {
  const trpc = useTRPC();

  const { data: user } = useSuspenseQuery(trpc.users.getCurrent.queryOptions());
  const { data: budget } = useSuspenseQuery(trpc.budgets.getOne.queryOptions());

  return (
    <div className="flex flex-col">
      <ProfileBanner user={user} />
      <ProfileInfo user={user} budget={budget} />
    </div>
  );
};
