"use client";

import { useEffect, useRef } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { useInvalidateAccount } from "@/modules/transactions/hooks/use-invalidate-account";

import { useTRPC } from "@/trpc/client";
import { formatCurrency, formatNumber } from "@/lib/utils";

import type { OrderResponse } from "../types";

/** How often open orders are re-checked while any are pending. */
const OPEN_ORDERS_POLL_MS = 5_000;

export const describeOrder = (order: Pick<OrderResponse, "side" | "type" | "quantity" | "ticker" | "triggerPrice">) =>
  `${order.type === "limit" ? "Limit" : "Stop"} ${order.side} ${formatNumber(order.quantity)} ${order.ticker} @ ${formatCurrency(order.triggerPrice)}`;

/** When an order with these settings fills, in plain words. */
export const fillCondition = (side: OrderResponse["side"], type: OrderResponse["type"]) => {
  if (type === "limit") {
    return side === "buy" ? "at or below" : "at or above";
  }

  return side === "buy" ? "at or above" : "at or below";
};

/** The user's open orders; polls while any are pending so fills show up quickly. */
export function useOpenOrders() {
  const trpc = useTRPC();

  return useQuery(
    trpc.orders.getMany.queryOptions(
      { status: "open" },
      {
        refetchInterval: (query) => ((query.state.data?.length ?? 0) > 0 ? OPEN_ORDERS_POLL_MS : false),
        refetchIntervalInBackground: false,
      }
    )
  );
}

/**
 * Mounted once in the app shell: notices open orders that stopped being pending and tells
 * the user whether they filled or were rejected, then refreshes cash/positions.
 */
export function useOrderFillNotifications() {
  const trpc = useTRPC();
  const queryClient = useQueryClient();
  const invalidateAccount = useInvalidateAccount();
  const { data: openOrders } = useOpenOrders();
  const previousIds = useRef<Set<string> | null>(null);

  useEffect(() => {
    if (!openOrders) {
      return;
    }

    const currentIds = new Set(openOrders.map((order) => order.id));
    const before = previousIds.current;
    previousIds.current = currentIds;

    if (!before) {
      return;
    }

    const settled = [...before].filter((id) => !currentIds.has(id));
    if (settled.length === 0) {
      return;
    }

    void (async () => {
      const allOrders = await queryClient.fetchQuery(
        trpc.orders.getMany.queryOptions({ status: "all" }, { staleTime: 0 })
      );

      let anyFilled = false;

      for (const order of allOrders.filter((candidate) => settled.includes(candidate.id))) {
        if (order.status === "filled") {
          anyFilled = true;
          toast.success(`Order filled: ${order.side === "buy" ? "bought" : "sold"} ${formatNumber(order.quantity)} ${order.ticker}`, {
            description: order.filledPrice ? `Filled at ${formatCurrency(order.filledPrice)} per share.` : undefined,
          });
        } else if (order.status === "rejected") {
          toast.error(`Order rejected: ${describeOrder(order)}`, {
            description: order.rejectionReason ?? undefined,
          });
        }
      }

      if (anyFilled) {
        await invalidateAccount();
      }
    })();
  }, [openOrders, queryClient, trpc, invalidateAccount]);
}

export function useCancelOrder() {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  return useMutation(
    trpc.orders.cancel.mutationOptions({
      onSuccess: async () => {
        toast.success("Order cancelled");
        await queryClient.invalidateQueries(trpc.orders.getMany.pathFilter());
      },
      onError: (error) => {
        toast.error("Couldn't cancel the order", { description: error.message });
      },
    })
  );
}
