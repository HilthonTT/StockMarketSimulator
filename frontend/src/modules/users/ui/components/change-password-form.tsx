"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { zodResolver } from "@hookform/resolvers/zod";
import { LoaderIcon } from "lucide-react";

import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { useTRPC } from "@/trpc/client";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import { ChangePasswordFormSchema } from "../../schemas";

export const ChangePasswordForm = () => {
  const trpc = useTRPC();

  const form = useForm<z.infer<typeof ChangePasswordFormSchema>>({
    resolver: zodResolver(ChangePasswordFormSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
  });

  const updatePassword = useMutation(
    trpc.users.updatePassword.mutationOptions({
      onSuccess: () => {
        toast.success("Password changed");
        form.reset();
      },
      onError: (error) => {
        toast.error("Couldn't change your password", { description: error.message });
      },
    })
  );

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(({ currentPassword, newPassword }) =>
          updatePassword.mutate({ currentPassword, newPassword })
        )}
        className="flex flex-col gap-4"
      >
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Current password</FormLabel>
              <FormControl>
                <Input {...field} type="password" autoComplete="current-password" disabled={updatePassword.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>New password</FormLabel>
              <FormControl>
                <Input {...field} type="password" autoComplete="new-password" disabled={updatePassword.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Confirm new password</FormLabel>
              <FormControl>
                <Input {...field} type="password" autoComplete="new-password" disabled={updatePassword.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end">
          <Button type="submit" disabled={updatePassword.isPending || !form.formState.isDirty}>
            {updatePassword.isPending && <LoaderIcon className="animate-spin" />}
            Change password
          </Button>
        </div>
      </form>
    </Form>
  );
};
