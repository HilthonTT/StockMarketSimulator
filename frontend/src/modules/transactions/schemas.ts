import { z } from "zod";

const tickerSchema = z
  .string({
    error: (issue) =>
      issue.input === undefined ? "Ticker is required" : "Ticker must be a string",
  })
  .min(1, { error: "Ticker cannot be empty" })
  .max(10, { error: "Ticker must be 10 characters or fewer" });

// `z.coerce.number<number>()` keeps the *input* type as number so react-hook-form's resolver
// types line up (Zod 4 types coerced inputs as `unknown` by default).
const quantitySchema = z.coerce
  .number<number>({
    error: (issue) =>
      issue.input === undefined ? "Quantity is required" : "Quantity must be a number",
  })
  .int({ error: "Quantity must be a whole number" })
  .min(1, { error: "Quantity must be at least 1" });

export const BuyTransactionSchema = z.object({
  ticker: tickerSchema,
  quantity: quantitySchema,
});

export const SellTransactionSchema = z.object({
  ticker: tickerSchema,
  quantity: quantitySchema,
});
