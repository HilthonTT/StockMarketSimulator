"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { zodResolver } from "@hookform/resolvers/zod";
import { RefreshCcwDotIcon } from "lucide-react";
import { useEffect } from "react";

import { UserResponse } from "@/modules/auth/types";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useTRPC } from "@/trpc/client";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Hint } from "@/components/hint";

import { UpdateUsernameSchema } from "../../schemas";

interface ProfileFormProps {
  user: UserResponse;
}

export const ProfileForm = ({ user }: ProfileFormProps) => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const form = useForm<z.infer<typeof UpdateUsernameSchema>>({
    mode: "all",
    resolver: zodResolver(UpdateUsernameSchema),
    defaultValues: {
      newUsername: user.username,
    },
  });

  const { isDirty } = form.formState;

  const updateUsername = useMutation(
    trpc.users.updateUsername.mutationOptions({
      onSuccess: async () => {
        toast.success("Profile information up to date!");

        await queryClient.invalidateQueries(
          trpc.users.getCurrent.queryFilter()
        );

        form.reset();
      },
      onError: () => {
        toast.error("Something went wrong");
      },
    })
  );

  const onSubmit = (values: z.infer<typeof UpdateUsernameSchema>) => {
    updateUsername.mutate(values);
  };

  useEffect(() => {
    form.reset({ newUsername: user.username });
  }, [user.username, form]);

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="gap-y-2 flex flex-col mt-4"
      >
        <div className="flex items-end gap-2 w-full">
          <FormField
            name="newUsername"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormLabel>Your username</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    aria-label="New username"
                    disabled={updateUsername.isPending}
                    required
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Hint label="Reset to original username">
            <Button
              type="button"
              className="mt-6 shrink-0"
              variant="elevated"
              aria-label="Reset to original username"
              onClick={() => form.setValue("newUsername", user.username)}
            >
              <RefreshCcwDotIcon className="size-4 shrink-0" />
              <span className="sr-only">Reset to original username</span>
            </Button>
          </Hint>
        </div>

        <div className="mt-4 w-full">
          <Button
            type="submit"
            variant="elevated"
            className="w-full"
            disabled={updateUsername.isPending || !isDirty}
            aria-label="Save profile changes"
          >
            Save Changes
          </Button>
        </div>
      </form>
    </Form>
  );
};
