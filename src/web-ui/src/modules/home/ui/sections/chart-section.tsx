"use client";

import { HubConnectionState } from "@microsoft/signalr";
import { Suspense, useCallback, useEffect, useRef, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { useSuspenseQuery } from "@tanstack/react-query";
import { SlidersHorizontalIcon } from "lucide-react";

import { StockPriceResponse } from "@/modules/stocks/types";

import { Card, CardContent, CardTitle } from "@/components/ui/card";
import { useTRPC } from "@/trpc/client";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

import {
  SIGNALR_JOIN_GROUP,
  SIGNALR_LEAVE_GROUP,
  SIGNALR_STOCK_UPDATE_EVENT,
  useSignalR,
} from "../../hooks/use-signalr";
import { CustomToolTip } from "../components/custom-tooltip";
import { TickerSelector } from "../components/ticker-selector";

export const ChartSection = () => {
  return (
    <Suspense fallback={<ChartSectionLoading />}>
      <ErrorBoundary fallback={<p>Error</p>}>
        <ChartSectionSuspense />
      </ErrorBoundary>
    </Suspense>
  );
};

const ChartSectionLoading = () => {
  return (
    <Card>
      <CardTitle className="w-full relative" aria-label="Chart header">
        <div className="flex justify-center items-center relative">
          <h1 className="text-center lg:text-5xl text-4xl">Loading...</h1>
          <div className="absolute right-8">
            <Button size="icon" variant="outline" disabled>
              <SlidersHorizontalIcon className="size-5" aria-hidden="true" />
            </Button>
          </div>
        </div>
      </CardTitle>

      <CardContent className="px-2 sm:p-6 sm:pt-0">
        <Skeleton className="h-[350px] rounded-xl" />
      </CardContent>
    </Card>
  );
};

interface PricePoint {
  index: number;
  price: number;
}

const MAX_POINTS = 30;

const ChartSectionSuspense = () => {
  const trpc = useTRPC();

  const { data: purchasedStockTickers } = useSuspenseQuery(
    trpc.users.getPurchasedStockTickers.queryOptions()
  );

  const { connection, isLoading } = useSignalR();

  const [pending, setPending] = useState(false);

  const defaultTicker = purchasedStockTickers.tickers[0]
    ? purchasedStockTickers.tickers[0]
    : "";

  const [selectedTicker, setSelectedTicker] = useState<string>(defaultTicker);
  const [data, setData] = useState<PricePoint[]>([]);

  const resetChart = useCallback(() => {
    setData([]);
  }, []);

  const latestTickerRef = useRef(selectedTicker);

  useEffect(() => {
    latestTickerRef.current = selectedTicker;
  }, [selectedTicker]);

  const handleStockUpdate = useCallback((stockUpdate: StockPriceResponse) => {
    if (stockUpdate.ticker !== latestTickerRef.current) {
      return;
    }

    setData((prev) => {
      const nextIndex = prev.length > 0 ? prev[prev.length - 1].index + 1 : 1;
      const updated = [...prev, { index: nextIndex, price: stockUpdate.price }];
      if (updated.length > MAX_POINTS) {
        updated.shift();
      }
      return updated;
    });
  }, []);

  const onChangeTicker = useCallback(
    async (ticker: string) => {
      if (!connection || ticker === selectedTicker || pending) {
        return;
      }

      setPending(true);

      try {
        await connection.invoke(SIGNALR_LEAVE_GROUP, selectedTicker);
        await connection.invoke(SIGNALR_JOIN_GROUP, ticker);

        resetChart();
        setSelectedTicker(ticker);
      } catch (error) {
        console.error("Error switching ticker groups:", error);
      } finally {
        setPending(false);
      }
    },
    [connection, selectedTicker, resetChart, pending]
  );

  useEffect(() => {
    if (!connection || !selectedTicker) {
      return;
    }

    connection.invoke(SIGNALR_JOIN_GROUP, selectedTicker).catch(console.error);
    connection.on(SIGNALR_STOCK_UPDATE_EVENT, handleStockUpdate);

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.off(SIGNALR_STOCK_UPDATE_EVENT, handleStockUpdate);
        connection
          .invoke(SIGNALR_LEAVE_GROUP, selectedTicker)
          .catch(console.error);
      }
    };
  }, [connection, selectedTicker, handleStockUpdate]);

  if (isLoading) {
    return <ChartSectionLoading />;
  }

  return (
    <Card className="dark:bg-black" aria-label="Stock chart card">
      <CardTitle className="w-full relative" aria-label="Chart header">
        <div className="flex justify-center items-center relative">
          <h1
            className="text-center lg:text-5xl text-4xl"
            aria-label={`Selected ticker: ${selectedTicker || "N/A"}`}
          >
            {selectedTicker || "N/A"}
          </h1>
          <div className="absolute right-8">
            <TickerSelector
              disabled={pending || isLoading}
              onChange={onChangeTicker}
              tickers={purchasedStockTickers.tickers}
              aria-label="Select ticker symbol"
            />
          </div>
        </div>
      </CardTitle>
      <CardContent className="px-2 sm:p-6 sm:pt-0" aria-label="Chart content">
        <ResponsiveContainer width="100%" height={350}>
          <LineChart data={data} aria-label="Price over time line chart">
            <defs>
              <linearGradient id="colorLine" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#38bdf8" stopOpacity={0.8} />
                <stop offset="100%" stopColor="#38bdf8" stopOpacity={0.2} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis
              dataKey="index"
              tickFormatter={(value) => `#${value}`}
              tick={{ fontSize: 12 }}
              tickMargin={8}
              aria-label="X axis: Index number"
            />
            <YAxis
              domain={["dataMin - 0.5", "dataMax + 0.5"]}
              tickFormatter={(value) => `$${value.toFixed(2)}`}
              tick={{ fontSize: 12 }}
              tickMargin={8}
              aria-label="Y axis: Price in USD"
            />
            <Tooltip content={<CustomToolTip />} />
            <Line
              type="monotone"
              dataKey="price"
              stroke="#3d82f6"
              strokeWidth={3}
              dot={false}
              aria-label="Line representing price"
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
};
