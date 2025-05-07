import { z } from "zod";

export const ContactSchema = z.object({
  message: z.string().min(20, {
    message: "Message must contain at least 20 character(s)",
  }),
});
