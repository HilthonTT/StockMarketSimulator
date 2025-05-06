"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { zodResolver } from "@hookform/resolvers/zod";

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

import { UpdatePasswordSchema } from "../../schemas";

export const ChangePasswordForm = () => {
  const trpc = useTRPC();

  const form = useForm<z.infer<typeof UpdatePasswordSchema>>({
    resolver: zodResolver(UpdatePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
    },
  });

  const { isDirty } = form.formState;

  const updatePassword = useMutation(
    trpc.users.updatePassword.mutationOptions({
      onSuccess: async () => {
        toast.success("Profile information up to date!");

        form.reset();
      },
      onError: () => {
        toast.error("Something went wrong");
      },
    })
  );

  const onSubmit = (values: z.infer<typeof UpdatePasswordSchema>) => {
    updatePassword.mutate(values);
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="gap-y-8 flex flex-col mt-4"
      >
        <div className="flex items-end gap-2 w-full">
          <FormField
            name="currentPassword"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormLabel>Your current password</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    aria-label="Current password"
                    disabled={updatePassword.isPending}
                    type="password"
                    required
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="flex items-end gap-2 w-full">
          <FormField
            name="newPassword"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormLabel>Your new password</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    aria-label="New password"
                    disabled={updatePassword.isPending}
                    type="password"
                    required
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="mt-4 w-full">
          <Button
            type="submit"
            variant="elevated"
            className="w-full"
            disabled={updatePassword.isPending || !isDirty}
            aria-label="Change password"
          >
            Change password
          </Button>
        </div>
      </form>
    </Form>
  );
};
