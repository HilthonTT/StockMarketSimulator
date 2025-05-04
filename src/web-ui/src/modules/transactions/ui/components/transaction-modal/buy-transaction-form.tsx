"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { BuyTransactionSchema } from "@/modules/transactions/schemas";
import { useTransactionModal } from "@/modules/transactions/hooks/use-transaction-modal";
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
import { useTRPC } from "@/trpc/client";

interface BuyTransactionFormProps {
  stockPrice: StockPriceResponse;
}

export const BuyTransactionForm = ({ stockPrice }: BuyTransactionFormProps) => {
  const { onClose } = useTransactionModal();

  const form = useForm<z.infer<typeof BuyTransactionSchema>>({
    mode: "all",
    resolver: zodResolver(BuyTransactionSchema),
    defaultValues: {
      ticker: stockPrice.ticker,
      quantity: 1,
    },
  });

  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const buy = useMutation(
    trpc.transactions.buy.mutationOptions({
      onSuccess: async () => {
        toast.success("Stock bought!");

        await queryClient.invalidateQueries(
          trpc.transactions.getMany.queryFilter()
        );

        await queryClient.invalidateQueries(trpc.budgets.getOne.queryFilter());

        onClose();
      },
      onError: (error) => {
        toast.error(error.message);
      },
    })
  );

  const onSubmit = (values: z.infer<typeof BuyTransactionSchema>) => {
    if (buy.isPending) {
      return;
    }

    buy.mutate(values);
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
                <Input {...field} type="number" disabled={buy.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button
          type="submit"
          variant="elevated"
          className="bg-blue-500 text-white hover:bg-blue-500"
          disabled={buy.isPending}
        >
          Buy Now
        </Button>
      </form>
    </Form>
  );
};
