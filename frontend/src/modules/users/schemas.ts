import { z } from "zod";

import { PasswordSchema } from "@/modules/auth/schemas";

export const UpdateUsernameSchema = z.object({
  newUsername: z
    .string()
    .trim()
    .min(1, { message: "Username is required" })
    .max(50, { message: "Username must be 50 characters or fewer" }),
});

/** Sent to the API. */
export const UpdatePasswordSchema = z.object({
  currentPassword: z.string().min(1, { message: "Enter your current password" }),
  newPassword: PasswordSchema,
});

/** The form adds a confirmation field that never leaves the browser. */
export const ChangePasswordFormSchema = UpdatePasswordSchema.extend({
  confirmPassword: z.string(),
}).refine((values) => values.newPassword === values.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});
