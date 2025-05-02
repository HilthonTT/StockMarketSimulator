"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Poppins } from "next/font/google";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
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

import { RegisterSchema } from "../../schemas";
import { cn } from "@/lib/utils";

const poppins = Poppins({
  subsets: ["latin"],
  weight: ["700"],
});

export const RegisterFormSection = () => {
  const trpc = useTRPC();
  const router = useRouter();

  const form = useForm<z.infer<typeof RegisterSchema>>({
    mode: "all",
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      username: "",
      email: "",
      password: "",
    },
  });

  const register = useMutation(
    trpc.auth.register.mutationOptions({
      onSuccess: () => {
        toast.success(
          "Please check your emails for the email verification link!"
        );

        router.push("/login");
      },
      onError: (error) => {
        toast.error(error.message);
      },
    })
  );

  const onSubmit = (values: z.infer<typeof RegisterSchema>) => {
    register.mutate(values);
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="size-full gap-y-4 flex flex-col"
      >
        <h1 className={cn("text-4xl font-bold mb-4", poppins.className)}>
          Attempt to make a bank of your virtual currencies!
        </h1>

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
