"use client";

import Link from "next/link";
import { ArrowUpRightIcon } from "lucide-react";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useTradeDialog } from "../../hooks/use-trade-dialog";
import { TradeTicket } from "./trade-ticket";

/** The app-wide order dialog, opened with `useTradeDialog().open(ticker, side)`. */
export const TradeDialog = () => {
  const { isOpen, ticker, side, close } = useTradeDialog();

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && close()}>
      <DialogContent className="sm:max-w-md max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl">Trade {ticker}</DialogTitle>
          <DialogDescription className="flex items-center justify-between gap-2">
            <span>Place a market order with your virtual cash.</span>
            <Link
              href={`/stocks/${ticker}`}
              onClick={close}
              className="inline-flex items-center gap-0.5 text-foreground underline-offset-4 hover:underline shrink-0"
            >
              Details
              <ArrowUpRightIcon className="size-3.5" />
            </Link>
          </DialogDescription>
        </DialogHeader>

        {/* Keyed so each opening starts with a fresh ticket. */}
        {isOpen && ticker && (
          <TradeTicket key={`${ticker}-${side}`} ticker={ticker} defaultSide={side} onDone={close} />
        )}
      </DialogContent>
    </Dialog>
  );
};
