import { z } from "zod";

export const LoginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1, {
    message: "Password must contain at least 1 character(s)",
  }),
});

export const RegisterSchema = z.object({
  username: z.string().nonempty({ message: "Username is required" }),

  email: z
    .string()
    .nonempty({ message: "Email is required" })
    .email({ message: "Invalid email format" }),

  password: z
    .string()
    .nonempty({ message: "Password is required" })
    .min(6, { message: "Password must contain at least 6 character(s)" })
    .refine((val) => /[a-z]/.test(val), {
      message: "Password must contain at least one lowercase letter",
    })
    .refine((val) => /[A-Z]/.test(val), {
      message: "Password must contain at least one uppercase letter",
    })
    .refine((val) => /\d/.test(val), {
      message: "Password must contain at least one digit",
    })
    .refine((val) => /[^a-zA-Z0-9]/.test(val), {
      message: "Password must contain at least one special character",
    }),
});
