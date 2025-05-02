"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { SellTransactionSchema } from "@/modules/transactions/schemas";
import { StockPriceResponse } from "@/modules/stocks/types";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { formatCurrency } from "@/lib/utils";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

interface SellTransactionFormProps {
  stockPrice: StockPriceResponse;
}

export const SellTransactionForm = ({
  stockPrice,
}: SellTransactionFormProps) => {
  const form = useForm<z.infer<typeof SellTransactionSchema>>({
    mode: "all",
    resolver: zodResolver(SellTransactionSchema),
    defaultValues: {
      ticker: stockPrice.ticker,
      quantity: 1,
    },
  });

  const onSubmit = (values: z.infer<typeof SellTransactionSchema>) => {
    console.log({ values });
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="gap-y-4 flex flex-col"
      >
        <div>
          <Label className="mb-1">Price Per Unit</Label>
          <div className="p-2 w-full rounded-lg border">
            <p className="font-bold">{formatCurrency(stockPrice.price)}</p>
          </div>
        </div>

        <FormField
          name="quantity"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Quantity of stock</FormLabel>
              <FormControl>
                <Input {...field} type="number" disabled={false} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button
          type="submit"
          variant="elevated"
          className="bg-blue-500 text-white hover:bg-blue-500"
        >
          Sell Now
        </Button>
      </form>
    </Form>
  );
};
