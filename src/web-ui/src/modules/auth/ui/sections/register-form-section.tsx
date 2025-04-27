"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { trpc } from "@/trpc/client";

import { RegisterSchema } from "../../schemas";

export const RegisterFormSection = () => {
  const router = useRouter();

  const form = useForm<z.infer<typeof RegisterSchema>>({
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      username: "",
      email: "",
      password: "",
    },
  });

  const register = trpc.auth.register.useMutation({
    onSuccess: () => {
      toast.success(
        "Please check your emails for the email verification link!"
      );

      router.push("/login");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });

  const onSubmit = (values: z.infer<typeof RegisterSchema>) => {
    register.mutate(values);
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="size-full gap-y-4 flex flex-col"
      >
        <FormField
          control={form.control}
          name="username"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-xl lg:text-2xl">Username</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  placeholder="Lando"
                  disabled={register.isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-xl lg:text-2xl">
                Email Address
              </FormLabel>
              <FormControl>
                <Input
                  {...field}
                  placeholder="lando@lando.com"
                  disabled={register.isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-xl lg:text-2xl">Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  type="password"
                  placeholder="••••••••"
                  disabled={register.isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button
          type="submit"
          variant="elevated"
          size="lg"
          disabled={register.isPending}
        >
          Register
        </Button>
      </form>
    </Form>
  );
};
