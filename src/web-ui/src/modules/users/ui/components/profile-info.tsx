"use client";

import CountUp from "react-countup";
import { format } from "date-fns";

import type { UserResponse } from "@/modules/auth/types";
import type { BudgetResponse } from "@/modules/budgets/types";

import { UserAvatar } from "@/components/user-avatar";
import { Separator } from "@/components/ui/separator";
import { formatCurrency } from "@/lib/utils";

import { ProfileForm } from "./profile-form";
import { ChangePasswordForm } from "./change-password-form";
import { ProfilePictureModal } from "./profile-picture-modal";

import { useProfilePictureModal } from "../../hooks/use-profile-picture-modal";

interface ProfileInfoProps {
  user: UserResponse;
  budget: BudgetResponse;
}

export const ProfileInfo = ({ user, budget }: ProfileInfoProps) => {
  const { onOpen } = useProfilePictureModal();

  return (
    <>
      <ProfilePictureModal />

      <div className="py-6">
        <div className="flex items-start gap-4">
          <UserAvatar
            size="xl"
            imageUrl="profile.png"
            className="bg-white cursor-pointer hover:opacity-80 transition-opacity duration-300"
            name={user.username}
            onClick={() => onOpen(user.id, "")}
          />
          <div className="flex-1 min-w-0">
            <div className="flex flex-col items-start">
              <h1 className="text-3xl font-bold">{user.username}</h1>
              <h3 className="text-xs font-semibold">{user.email}</h3>
            </div>
            <div className="flex flex-col items-start gap-1 text-sm text-muted-foreground mt-4">
              <p>
                Date of creation {format(user.createdOnUtc, "MMMM d, yyyy")}
              </p>
              <div className="flex items-center gap-x-2">
                <p>Budget: </p>

                <CountUp
                  preserveValue
                  start={0}
                  end={budget.amount}
                  decimal="2"
                  decimalPlaces={2}
                  formattingFn={formatCurrency}
                  className="font-extrabold"
                />
              </div>
            </div>
          </div>
        </div>

        <Separator className="my-4" />

        <div className="flex-1">
          <h1 className="text-4xl font-bold">Your Settings</h1>

          <ProfileForm user={user} />
        </div>

        <Separator className="my-4" />

        <div className="flex-1">
          <h1 className="text-4xl font-bold">Your Credentials</h1>

          <ChangePasswordForm />
        </div>
      </div>
    </>
  );
};
