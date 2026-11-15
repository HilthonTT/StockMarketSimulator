import { z } from "zod";

export const PlaceOrderSchema = z.object({
  ticker: z.string().trim().min(1).max(12),
  side: z.enum(["buy", "sell"]),
  type: z.enum(["limit", "stop"]),
  quantity: z.number().int().min(1, { error: "Quantity must be at least 1" }),
  triggerPrice: z.number().positive({ error: "The trigger price must be above $0" }),
});

export type PlaceOrderInput = z.infer<typeof PlaceOrderSchema>;
