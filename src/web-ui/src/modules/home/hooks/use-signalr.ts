import { useEffect, useState } from "react";
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";
import { SERVER_URL } from "@/constants";

export const SIGNALR_JOIN_GROUP = "JoinGroup";
export const SIGNALR_LEAVE_GROUP = "LeaveGroup";
export const SIGNALR_STOCK_UPDATE_EVENT = "ReceiveStockPriceUpdate";

/**
 * Opens a connection to the stocks feed hub for the lifetime of the calling component.
 * The connection is exposed only once it has started, and it is stopped on unmount or
 * when the JWT changes (previously it leaked on every navigation).
 */
export const useSignalR = () => {
  const trpc = useTRPC();
  const { data: jwt } = useSuspenseQuery(trpc.auth.getJwt.queryOptions());

  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    if (!jwt) {
      return;
    }

    let disposed = false;

    const hubConnection = new HubConnectionBuilder()
      .withUrl(`${SERVER_URL}/stocks-feed`, {
        accessTokenFactory: () => jwt,
        transport: HttpTransportType.LongPolling,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    hubConnection
      .start()
      .then(() => {
        if (!disposed) {
          setConnection(hubConnection);
        }
      })
      .catch((err) => {
        if (!disposed) {
          console.error("SignalR connection error:", err);
        }
      });

    return () => {
      disposed = true;
      setConnection(null);
      void hubConnection.stop();
    };
  }, [jwt]);

  return {
    connection,
    isLoading: connection === null,
  };
};
