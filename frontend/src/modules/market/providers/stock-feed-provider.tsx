"use client";

import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { createContext, useCallback, useEffect, useMemo, useRef } from "react";
import { useQuery } from "@tanstack/react-query";

import type { StockPriceResponse } from "@/modules/stocks/types";

import { useTRPC } from "@/trpc/client";
import { SERVER_URL } from "@/constants";

import {
  SIGNALR_HUB_PATH,
  SIGNALR_JOIN_GROUP,
  SIGNALR_LEAVE_GROUP,
  SIGNALR_STOCK_UPDATE_EVENT,
} from "../constants";
import { normalizeTicker, useFeedStore } from "../store";

interface StockFeedContextValue {
  /** Subscribes to live ticks for a ticker; returns the unsubscribe function. */
  subscribe: (ticker: string) => () => void;
}

export const StockFeedContext = createContext<StockFeedContextValue | null>(null);

/**
 * Owns the app's single SignalR connection to the stocks feed.
 *
 * SignalR groups are per connection, not per component, so subscriptions are
 * reference-counted here: a ticker's group is joined on its first subscriber and only
 * left when its last subscriber unmounts. Groups are re-joined after a reconnect.
 */
export const StockFeedProvider = ({ children }: { children: React.ReactNode }) => {
  const trpc = useTRPC();
  const { data: jwt } = useQuery(trpc.auth.getJwt.queryOptions());

  const connectionRef = useRef<HubConnection | null>(null);
  const subscriberCounts = useRef(new Map<string, number>());

  const setStatus = useFeedStore((state) => state.setStatus);
  const push = useFeedStore((state) => state.push);

  useEffect(() => {
    if (!jwt) {
      return;
    }

    let disposed = false;

    const connection = new HubConnectionBuilder()
      .withUrl(`${SERVER_URL}${SIGNALR_HUB_PATH}`, {
        accessTokenFactory: () => jwt,
        transport: HttpTransportType.LongPolling,
      })
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    const joinAll = () => {
      for (const ticker of subscriberCounts.current.keys()) {
        connection.invoke(SIGNALR_JOIN_GROUP, ticker).catch(console.error);
      }
    };

    connection.on(SIGNALR_STOCK_UPDATE_EVENT, (update: StockPriceResponse) => {
      push(update.ticker, update.price);
    });
    connection.onreconnecting(() => setStatus("reconnecting"));
    connection.onreconnected(() => {
      setStatus("live");
      joinAll();
    });
    connection.onclose(() => {
      if (!disposed) {
        setStatus("offline");
      }
    });

    connectionRef.current = connection;
    setStatus("connecting");

    connection
      .start()
      .then(() => {
        if (disposed) {
          return;
        }

        setStatus("live");
        joinAll();
      })
      .catch((error) => {
        if (!disposed) {
          console.error("Stock feed connection failed:", error);
          setStatus("offline");
        }
      });

    return () => {
      disposed = true;
      connectionRef.current = null;
      setStatus("idle");
      void connection.stop();
    };
  }, [jwt, push, setStatus]);

  const subscribe = useCallback((rawTicker: string) => {
    const ticker = normalizeTicker(rawTicker);
    const counts = subscriberCounts.current;
    const current = counts.get(ticker) ?? 0;

    counts.set(ticker, current + 1);

    const connection = connectionRef.current;
    if (current === 0 && connection?.state === HubConnectionState.Connected) {
      connection.invoke(SIGNALR_JOIN_GROUP, ticker).catch(console.error);
    }

    let released = false;

    return () => {
      if (released) {
        return;
      }
      released = true;

      const remaining = (counts.get(ticker) ?? 1) - 1;

      if (remaining > 0) {
        counts.set(ticker, remaining);
        return;
      }

      counts.delete(ticker);

      const activeConnection = connectionRef.current;
      if (activeConnection?.state === HubConnectionState.Connected) {
        activeConnection.invoke(SIGNALR_LEAVE_GROUP, ticker).catch(console.error);
      }
    };
  }, []);

  const value = useMemo(() => ({ subscribe }), [subscribe]);

  return <StockFeedContext.Provider value={value}>{children}</StockFeedContext.Provider>;
};
