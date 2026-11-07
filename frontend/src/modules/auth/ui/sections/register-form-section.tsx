"use client";

import Link from "next/link";
import { useState } from "react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { AlertCircleIcon, LoaderIcon, MailCheckIcon } from "lucide-react";

import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useTRPC } from "@/trpc/client";

import { RegisterSchema } from "../../schemas";

export const RegisterFormSection = () => {
  const trpc = useTRPC();
  const [registeredEmail, setRegisteredEmail] = useState<string | null>(null);

  const form = useForm<z.infer<typeof RegisterSchema>>({
    resolver: zodResolver(RegisterSchema),
    defaultValues: { username: "", email: "", password: "" },
  });

  const register = useMutation(
    trpc.auth.register.mutationOptions({
      onSuccess: (_, variables) => setRegisteredEmail(variables.email),
    })
  );

  if (registeredEmail) {
    return (
      <div className="flex flex-col items-center gap-4 text-center">
        <div className="rounded-full bg-muted p-3">
          <MailCheckIcon className="size-6" aria-hidden="true" />
        </div>
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold">Check your inbox</h1>
          <p className="text-sm text-muted-foreground">
            We sent a verification link to <span className="font-medium text-foreground">{registeredEmail}</span>.
            Verify your email, then log in to start trading.
          </p>
        </div>
        <Button asChild size="lg" className="w-full">
          <Link href="/login">Go to log in</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-bold">Create your account</h1>
        <p className="text-sm text-muted-foreground">Free, and no real money involved.</p>
      </div>

      {register.error && (
        <Alert variant="destructive">
          <AlertCircleIcon />
          <AlertDescription>{register.error.message}</AlertDescription>
        </Alert>
      )}

      <Form {...form}>
        <form onSubmit={form.handleSubmit((values) => register.mutate(values))} className="flex flex-col gap-4" noValidate>
          <FormField
            control={form.control}
            name="username"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Username</FormLabel>
                <FormControl>
                  <Input {...field} autoComplete="username" disabled={register.isPending} />
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
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input {...field} type="email" autoComplete="email" placeholder="you@example.com" disabled={register.isPending} />
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
                <FormLabel>Password</FormLabel>
                <FormControl>
                  <Input {...field} type="password" autoComplete="new-password" disabled={register.isPending} />
                </FormControl>
                <FormDescription>
                  At least 6 characters with upper and lower case letters, a number and a symbol.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button type="submit" size="lg" className="mt-2 w-full" disabled={register.isPending}>
            {register.isPending && <LoaderIcon className="animate-spin" />}
            Create account
          </Button>
        </form>
      </Form>

      <p className="text-center text-sm text-muted-foreground">
        Already have an account?{" "}
        <Link href="/login" className="font-medium text-foreground underline underline-offset-4">
          Log in
        </Link>
      </p>
    </div>
  );
};
