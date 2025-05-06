import { z } from "zod";

export const UpdateUsernameSchema = z.object({
  newUsername: z.string().min(1, {
    message: "Username must contain at least 1 character(s)",
  }),
});

export const UpdatePasswordSchema = z.object({
  currentPassword: z.string().min(1, {
    message: "Current password must contain at least 1 character(s)",
  }),
  newPassword: z.string().min(6, {
    message: "Current password must contain at least 6 character(s)",
  }),
});
