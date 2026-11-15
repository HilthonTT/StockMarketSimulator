"use client";

import Link from "next/link";
import { format } from "date-fns";
import { LoaderIcon, XIcon } from "lucide-react";

import { cn, formatCurrency, formatNumber, parseUtc } from "@/lib/utils";
import { assetHref, displaySymbol } from "@/lib/assets";
import { AssetIcon } from "@/components/asset-icon";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

import type { OrderResponse, OrderStatus } from "../../types";
import { fillCondition, useCancelOrder } from "../../hooks/use-orders";

const STATUS_STYLES: Record<OrderStatus, string> = {
  pending: "bg-primary/15 text-primary border-primary/30",
  filled: "bg-gain/15 text-gain border-gain/30",
  cancelled: "bg-muted text-muted-foreground",
  rejected: "bg-loss/15 text-loss border-loss/30",
};

export const OrderStatusBadge = ({ status }: { status: OrderStatus }) => (
  <Badge variant="outline" className={cn("capitalize", STATUS_STYLES[status])}>
    {status === "pending" && <span className="size-1.5 animate-pulse rounded-full bg-current" aria-hidden="true" />}
    {status}
  </Badge>
);

interface OrderListProps {
  orders: OrderResponse[];
  /** Hide the symbol column (e.g. on a stock's own page). */
  hideTicker?: boolean;
}

/** Orders table with a cancel action for pending ones. */
export const OrderList = ({ orders, hideTicker = false }: OrderListProps) => {
  const cancel = useCancelOrder();

  return (
    <Table>
      <TableHeader>
        <TableRow className="bg-muted/50 hover:bg-muted/50">
          <TableHead className="pl-4">Placed</TableHead>
          {!hideTicker && <TableHead>Asset</TableHead>}
          <TableHead>Order</TableHead>
          <TableHead className="text-right">Qty</TableHead>
          <TableHead className="text-right">Trigger</TableHead>
          <TableHead className="hidden text-right sm:table-cell">Filled at</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="pr-4 text-right">
            <span className="sr-only">Actions</span>
          </TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {orders.map((order) => {
          const createdOn = parseUtc(order.createdOnUtc);
          const isCancelling = cancel.isPending && cancel.variables?.orderId === order.id;

          return (
            <TableRow key={order.id}>
              <TableCell className="pl-4">
                <div className="flex flex-col">
                  <span>{format(createdOn, "MMM d")}</span>
                  <span className="num text-xs text-muted-foreground">{format(createdOn, "HH:mm:ss")}</span>
                </div>
              </TableCell>
              {!hideTicker && (
                <TableCell>
                  <Link href={assetHref(order.ticker)} className="flex items-center gap-2 font-semibold hover:underline">
                    <AssetIcon ticker={order.ticker} size="sm" />
                    {displaySymbol(order.ticker)}
                  </Link>
                </TableCell>
              )}
              <TableCell>
                <div className="flex flex-col">
                  <span className={cn("font-medium capitalize", order.side === "buy" ? "text-gain" : "text-loss")}>
                    {order.type} {order.side}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    fills {fillCondition(order.side, order.type)} trigger
                  </span>
                </div>
              </TableCell>
              <TableCell className="num text-right">{formatNumber(order.quantity)}</TableCell>
              <TableCell className="num text-right">{formatCurrency(order.triggerPrice)}</TableCell>
              <TableCell className="num hidden text-right sm:table-cell">
                {order.filledPrice !== null ? formatCurrency(order.filledPrice) : "—"}
              </TableCell>
              <TableCell>
                <div className="flex flex-col items-start gap-0.5">
                  <OrderStatusBadge status={order.status} />
                  {order.rejectionReason && (
                    <span className="max-w-[14rem] text-xs text-muted-foreground">{order.rejectionReason}</span>
                  )}
                </div>
              </TableCell>
              <TableCell className="pr-4 text-right">
                {order.status === "pending" && (
                  <Button
                    size="sm"
                    variant="ghost"
                    className="h-8 text-muted-foreground hover:text-loss"
                    disabled={isCancelling}
                    onClick={() => cancel.mutate({ orderId: order.id })}
                    aria-label={`Cancel ${order.type} ${order.side} order for ${order.ticker}`}
                  >
                    {isCancelling ? <LoaderIcon className="animate-spin" /> : <XIcon />}
                    Cancel
                  </Button>
                )}
              </TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
};
