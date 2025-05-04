"use client";

import { useState } from "react";
import { VisuallyHidden } from "@radix-ui/react-visually-hidden";
import { useQuery } from "@tanstack/react-query";
import { LoaderIcon } from "lucide-react";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useTRPC } from "@/trpc/client";

import { BuyTransactionForm } from "./buy-transaction-form";
import { SellTransactionForm } from "./sell-transaction-form";

import { useTransactionModal } from "../../../hooks/use-transaction-modal";

export const TransactionModal = () => {
  const trpc = useTRPC();

  const { isOpen, onClose, initialValues } = useTransactionModal();

  const { data: stockPrice, isLoading } = useQuery(
    trpc.stocks.getOne.queryOptions(
      { ticker: initialValues.ticker },
      { enabled: !!initialValues.ticker }
    )
  );

  const [isBuying, setIsBuying] = useState(true);

  if (isLoading) {
    return (
      <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              <VisuallyHidden>Loading transaction modal</VisuallyHidden>
            </DialogTitle>
          </DialogHeader>

          <div className="flex items-center justify-center">
            <LoaderIcon className="animate-spin" />
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  if (!stockPrice) {
    return null;
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-3xl text-center">
            {stockPrice.ticker.toUpperCase()}
          </DialogTitle>
          <DialogDescription className="text-center font-semibold">
            {isBuying
              ? "Enter the quantity of shares you want to buy for this stock."
              : "Enter the quantity of shares you want to sell from your portfolio."}
          </DialogDescription>
        </DialogHeader>

        <div className="flex items-center justify-between gap-2">
          <Button
            type="button"
            variant="elevated"
            className={cn(
              "flex-1",
              isBuying && "bg-blue-500 text-white hover:bg-blue-500"
            )}
            onClick={() => setIsBuying(true)}
          >
            Buy
          </Button>
          <Button
            type="button"
            variant="elevated"
            className={cn(
              "flex-1",
              !isBuying && "bg-blue-500 text-white hover:bg-blue-500"
            )}
            onClick={() => setIsBuying(false)}
          >
            Sell
          </Button>
        </div>

        {isBuying ? (
          <BuyTransactionForm stockPrice={stockPrice} />
        ) : (
          <SellTransactionForm stockPrice={stockPrice} />
        )}
      </DialogContent>
    </Dialog>
  );
};
