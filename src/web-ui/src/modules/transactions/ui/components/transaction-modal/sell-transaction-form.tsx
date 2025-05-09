"use client";

import Link from "next/link";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { z } from "zod";

import { SellTransactionSchema } from "@/modules/transactions/schemas";
import { useTransactionFilters } from "@/modules/transactions/hooks/use-transaction-filters";
import { useTransactionModal } from "@/modules/transactions/hooks/use-transaction-modal";
import type { StockPriceResponse } from "@/modules/stocks/types";
import type { ShortenUrlResponse } from "@/modules/shorten/types";

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
import { PAGE_SIZE } from "@/constants";

interface SellTransactionFormProps {
  stockPrice: StockPriceResponse;
  shortenUrl: ShortenUrlResponse;
}

export const SellTransactionForm = ({
  stockPrice,
  shortenUrl,
}: SellTransactionFormProps) => {
  const [filters] = useTransactionFilters();

  const { onClose } = useTransactionModal();

  const form = useForm<z.infer<typeof SellTransactionSchema>>({
    mode: "all",
    resolver: zodResolver(SellTransactionSchema),
    defaultValues: {
      ticker: stockPrice.ticker,
      quantity: 1,
    },
  });

  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const sell = useMutation(
    trpc.transactions.sell.mutationOptions({
      onSuccess: async () => {
        toast.success("Stock sold!");

        await Promise.all([
          queryClient.invalidateQueries(
            trpc.transactions.getMany.queryFilter({
              pageSize: PAGE_SIZE,
              ...filters,
            })
          ),

          queryClient.invalidateQueries(
            trpc.users.getPurchasedStockTickers.queryFilter()
          ),

          queryClient.invalidateQueries(
            trpc.transactions.getCount.queryOptions()
          ),

          queryClient.invalidateQueries(trpc.budgets.getOne.queryFilter()),
        ]);

        onClose();
      },
      onError: (error) => {
        toast.error(error.message);
      },
    })
  );

  const onSubmit = (values: z.infer<typeof SellTransactionSchema>) => {
    sell.mutate(values);
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
                <Input {...field} type="number" disabled={sell.isPending} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button variant="link" type="button" asChild>
          <Link href={shortenUrl.shortCode} target="_blank">
            More Info
          </Link>
        </Button>

        <Button
          type="submit"
          variant="elevated"
          className="bg-blue-500 text-white hover:bg-blue-500"
          disabled={sell.isPending}
        >
          Sell Now
        </Button>
      </form>
    </Form>
  );
};
