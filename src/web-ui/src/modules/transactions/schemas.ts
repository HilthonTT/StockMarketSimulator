import { z } from "zod";

export const BuyTransactionSchema = z.object({
  ticker: z
    .string({
      required_error: "Ticker is required",
      invalid_type_error: "Ticker must be a string",
    })
    .min(1, { message: "Ticker cannot be empty" })
    .max(10, { message: "Ticker must be 10 characters or fewer" }),

  quantity: z.coerce
    .number({
      invalid_type_error: "Quantity must be a number",
      required_error: "Quantity is required",
    })
    .min(1, { message: "Quantity must be at least 1" }),
});

export const SellTransactionSchema = z.object({
  ticker: z
    .string({
      required_error: "Ticker is required",
      invalid_type_error: "Ticker must be a string",
    })
    .min(1, { message: "Ticker cannot be empty" })
    .max(10, { message: "Ticker must be 10 characters or fewer" }),

  quantity: z.coerce
    .number({
      invalid_type_error: "Quantity must be a number",
      required_error: "Quantity is required",
    })
    .min(1, { message: "Quantity must be at least 1" }),
});
