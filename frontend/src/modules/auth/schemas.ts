import { z } from "zod";

export const PasswordSchema = z
  .string()
  .min(6, { message: "Use at least 6 characters" })
  .refine((val) => /[a-z]/.test(val), { message: "Add a lowercase letter" })
  .refine((val) => /[A-Z]/.test(val), { message: "Add an uppercase letter" })
  .refine((val) => /\d/.test(val), { message: "Add a number" })
  .refine((val) => /[^a-zA-Z0-9]/.test(val), { message: "Add a special character (e.g. !, #, ?)" });

export const LoginSchema = z.object({
  email: z.string().trim().email({ message: "Enter a valid email address" }),
  password: z.string().min(1, { message: "Enter your password" }),
});

export const RegisterSchema = z.object({
  username: z.string().trim().min(1, { message: "Choose a username" }).max(50),
  email: z.string().trim().email({ message: "Enter a valid email address" }),
  password: PasswordSchema,
});
