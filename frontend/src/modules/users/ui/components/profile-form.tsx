"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { zodResolver } from "@hookform/resolvers/zod";
import { LoaderIcon } from "lucide-react";

import type { UserResponse } from "@/modules/auth/types";

import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { useTRPC } from "@/trpc/client";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import { UpdateUsernameSchema } from "../../schemas";

interface ProfileFormProps {
  user: UserResponse;
}

export const ProfileForm = ({ user }: ProfileFormProps) => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const form = useForm<z.infer<typeof UpdateUsernameSchema>>({
    resolver: zodResolver(UpdateUsernameSchema),
    defaultValues: { newUsername: user.username },
  });

  const { isDirty } = form.formState;

  const updateUsername = useMutation(
    trpc.users.updateUsername.mutationOptions({
      onSuccess: async () => {
        toast.success("Username updated");
        await queryClient.invalidateQueries(trpc.users.getCurrent.queryFilter());
      },
      onError: (error) => {
        toast.error("Couldn't update your username", { description: error.message });
      },
    })
  );

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((values) => updateUsername.mutate(values))} className="flex flex-col gap-4">
        <FormField
          control={form.control}
          name="newUsername"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Username</FormLabel>
              <FormControl>
                <Input {...field} autoComplete="username" disabled={updateUsername.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-2">
          <Button
            type="button"
            variant="outline"
            disabled={!isDirty || updateUsername.isPending}
            onClick={() => form.reset()}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={!isDirty || updateUsername.isPending}>
            {updateUsername.isPending && <LoaderIcon className="animate-spin" />}
            Save changes
          </Button>
        </div>
      </form>
    </Form>
  );
};
