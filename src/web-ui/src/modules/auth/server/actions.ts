"use server";

import { z } from "zod";

import { LoginSchema } from "../schemas";
import { signIn, signOut } from "../auth";
import { DEFAULT_LOGIN_REDIRECT } from "../routes";

export const login = async (values: z.infer<typeof LoginSchema>) => {
  const validatedFields = await LoginSchema.safeParseAsync(values);

  if (!validatedFields.success) {
    throw new Error("Invalid fields");
  }

  const { email, password } = validatedFields.data;

  await signIn("credentials", {
    email,
    password,
    redirectTo: DEFAULT_LOGIN_REDIRECT,
  });
};

export const logout = async () => {
  await signOut();
};
