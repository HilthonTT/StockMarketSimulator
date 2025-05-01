"use client";

import { HubConnection } from "@microsoft/signalr";
import { useEffect, useState } from "react";

import { TransactionResponse } from "@/modules/transactions/types";
import { StockPriceResponse } from "@/modules/stocks/types";

import { TableCell, TableRow } from "@/components/ui/table";
import { trpc } from "@/trpc/client";
import { cn } from "@/lib/utils";

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
  const [pricePerUnit, setPricePerUnit] = useState(transaction.limitPrice);
  const [totalPrice, setTotalPrice] = useState(
    pricePerUnit * transaction.quantity
  );
  const [change, setChange] = useState(0);

  trpc.stocks.getOne.useQuery({ ticker: transaction.ticker });

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
      setTotalPrice(stockUpdate.price * transaction.quantity);
      setChange(stockUpdate.price - transaction.limitPrice);
    };

    connection.on(SIGNALR_STOCK_UPDATE_EVENT, handler);

    return () => {
      connection.off(SIGNALR_STOCK_UPDATE_EVENT, handler);
      connection
        .invoke(SIGNALR_LEAVE_GROUP, transaction.ticker)
        .catch(console.error);
    };
  }, [
    connection,
    transaction.ticker,
    transaction.quantity,
    transaction.limitPrice,
  ]);

  return (
    <TableRow>
      <TableCell className="text-xl font-bold">{transaction.ticker}</TableCell>
      <TableCell className="lg:text-lg text-base">
        ${pricePerUnit} (
        <span
          className={cn(
            "lg:text-lg text-base",
            change > 0 ? "text-emerald-500" : "text-rose-500"
          )}
        >
          {change >= 0 ? "+" : "-"}
          {change.toFixed(2)}%
        </span>
        )
      </TableCell>
      <TableCell className="lg:text-lg text-base">
        {transaction.quantity}{" "}
      </TableCell>
      <TableCell className="text-right lg:text-lg text-base">
        ${totalPrice}
      </TableCell>
    </TableRow>
  );
};
