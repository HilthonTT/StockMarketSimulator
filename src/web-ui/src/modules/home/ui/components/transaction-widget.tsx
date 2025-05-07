"use client";

import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";

import {
  TransactionResponse,
  TransactionType,
} from "@/modules/transactions/types";
import { StockPriceResponse } from "@/modules/stocks/types";

import { useTRPC } from "@/trpc/client";
import { TableCell, TableRow } from "@/components/ui/table";
import { cn, formatCurrency } from "@/lib/utils";

import {
  SIGNALR_JOIN_GROUP,
  SIGNALR_LEAVE_GROUP,
  SIGNALR_STOCK_UPDATE_EVENT,
} from "../../hooks/use-signalr";

interface TransactionWidgetProps {
  connection: HubConnection | null;
  transaction: TransactionResponse;
}

export const TransactionWidget = ({
  connection,
  transaction,
}: TransactionWidgetProps) => {
  const trpc = useTRPC();

  const [pricePerUnit, setPricePerUnit] = useState(transaction.amount);

  const [change, setChange] = useState(0);

  const totalPrice = useMemo(
    () => pricePerUnit * transaction.quantity,
    [pricePerUnit, transaction.quantity]
  );

  const {} = useQuery(
    trpc.stocks.getOne.queryOptions({ ticker: transaction.ticker })
  );

  useEffect(() => {
    if (!connection) {
      return;
    }

    connection
      .invoke(SIGNALR_JOIN_GROUP, transaction.ticker)
      .catch(console.error);

    const handler = (stockUpdate: StockPriceResponse) => {
      if (stockUpdate.ticker !== transaction.ticker) {
        return;
      }

      setPricePerUnit(stockUpdate.price);

      const diff =
        transaction.type === TransactionType.Expense
          ? stockUpdate.price - transaction.amount
          : transaction.amount - stockUpdate.price;

      setChange(diff);
    };

    connection.on(SIGNALR_STOCK_UPDATE_EVENT, handler);

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.off(SIGNALR_STOCK_UPDATE_EVENT, handler);
        connection
          .invoke(SIGNALR_LEAVE_GROUP, transaction.ticker)
          .catch(console.error);
      }
    };
  }, [
    transaction.type,
    connection,
    transaction.ticker,
    transaction.quantity,
    transaction.amount,
  ]);

  if (!connection) {
    return null;
  }

  return (
    <TableRow>
      <TableCell aria-label="Ticker" className="text-xl font-bold">
        {transaction.ticker}
      </TableCell>
      <TableCell className="text-xl font-bold">
        {formatCurrency(transaction.amount)}{" "}
        {transaction.type === TransactionType.Income && (
          <span className="text-rose-500">(Sold)</span>
        )}
      </TableCell>
      <TableCell className="lg:text-lg text-base">
        {formatCurrency(pricePerUnit)} (
        <span
          className={cn(
            "lg:text-lg text-base",
            change >= 0 ? "text-emerald-500" : "text-rose-500"
          )}
        >
          {change >= 0 ? "+" : "-"}
          {Math.abs(change).toFixed(2)}%
        </span>
        )
      </TableCell>
      <TableCell className="lg:text-lg text-base">
        {transaction.quantity}{" "}
      </TableCell>
      <TableCell className="text-right lg:text-lg text-base">
        {formatCurrency(totalPrice)}
      </TableCell>
    </TableRow>
  );
};
