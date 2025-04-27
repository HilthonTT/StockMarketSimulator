"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

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

import { LoginSchema } from "../../schemas";

export const LoginFormSection = () => {
  const router = useRouter();

  const form = useForm<z.infer<typeof LoginSchema>>({
    resolver: zodResolver(LoginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const login = trpc.auth.login.useMutation({
    onSuccess: () => {
      toast.success("You've logged in!");

      router.push("/");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });

  const onSubmit = (values: z.infer<typeof LoginSchema>) => {
    login.mutate(values);
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="size-full gap-y-4 flex flex-col"
      >
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
                  placeholder="email@email.com"
                  disabled={login.isPending}
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
                  disabled={login.isPending}
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
          disabled={login.isPending}
        >
          Login
        </Button>
      </form>
    </Form>
  );
};
