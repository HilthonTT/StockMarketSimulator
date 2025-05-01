import { z } from "zod";

export const LoginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1, {
    message: "Password must contain at least 1 character(s)",
  }),
});

export const RegisterSchema = z.object({
  username: z.string(),
  email: z.string().email(),
  password: z.string().min(1, {
    message: "Password must contain at least 1 character(s)",
  }),
});
