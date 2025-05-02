"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { useMutation } from "@tanstack/react-query";

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
import { useTRPC } from "@/trpc/client";

import { LoginSchema } from "../../schemas";
import { Poppins } from "next/font/google";
import { cn } from "@/lib/utils";

const poppins = Poppins({
  subsets: ["latin"],
  weight: ["700"],
});

export const LoginFormSection = () => {
  const trpc = useTRPC();
  const router = useRouter();

  const form = useForm<z.infer<typeof LoginSchema>>({
    mode: "all",
    resolver: zodResolver(LoginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const login = useMutation(
    trpc.auth.login.mutationOptions({
      onSuccess: () => {
        toast.success("You've logged in!");

        router.push("/");
      },
      onError: (error) => {
        toast.error(error.message);
      },
    })
  );

  const onSubmit = (values: z.infer<typeof LoginSchema>) => {
    login.mutate(values);
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="size-full gap-y-4 flex flex-col"
      >
        <h1 className={cn("text-4xl font-bold mb-4", poppins.className)}>
          Welcome back to Stock Market Simulator
        </h1>

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
