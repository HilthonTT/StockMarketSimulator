"use client";

import { useSuspenseQuery } from "@tanstack/react-query";

import { AccountSummarySection } from "@/modules/dashboard/ui/sections/account-summary-section";

import { useTRPC } from "@/trpc/client";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { QueryBoundary } from "@/components/query-boundary";

import { ProfileHeader } from "../components/profile-header";
import { ProfileForm } from "../components/profile-form";
import { ChangePasswordForm } from "../components/change-password-form";

export const ProfileView = () => (
  <QueryBoundary
    errorTitle="Couldn't load your profile"
    fallback={
      <div className="flex flex-col gap-6">
        <Skeleton className="h-64 w-full rounded-xl" />
        <Skeleton className="h-72 w-full rounded-xl" />
      </div>
    }
  >
    <Profile />
  </QueryBoundary>
);

const Profile = () => {
  const trpc = useTRPC();
  const { data: user } = useSuspenseQuery(trpc.users.getCurrent.queryOptions());

  return (
    <>
      <ProfileHeader user={user} />

      <AccountSummarySection />

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Account</CardTitle>
            <CardDescription>Your display name across the simulator.</CardDescription>
          </CardHeader>
          <CardContent>
            {/* Keyed so the form resets when the saved username changes. */}
            <ProfileForm key={user.username} user={user} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Security</CardTitle>
            <CardDescription>Change the password you use to log in.</CardDescription>
          </CardHeader>
          <CardContent>
            <ChangePasswordForm />
          </CardContent>
        </Card>
      </div>
    </>
  );
};
