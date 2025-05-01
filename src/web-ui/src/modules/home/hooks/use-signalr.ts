import { useEffect, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";
import { SERVER_URL } from "@/constants";

export const SIGNALR_JOIN_GROUP = "JoinGroup";
export const SIGNALR_LEAVE_GROUP = "LeaveGroup";
export const SIGNALR_STOCK_UPDATE_EVENT = "ReceiveStockPriceUpdate";

export const useSignalR = () => {
  const trpc = useTRPC();

  const { data: jwt } = useSuspenseQuery(trpc.auth.getJwt.queryOptions());

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!jwt) {
      return;
    }

    const newConnection = new HubConnectionBuilder()
      .withUrl(`${SERVER_URL}/stocks-feed`, {
        accessTokenFactory: () => jwt,
      })
      .configureLogging(LogLevel.Information)
      .build();

    newConnection
      .start()
      .then(() => {
        setConnection(newConnection);
      })
      .catch(console.error)
      .finally(() => {
        setIsLoading(false);
      });

    return () => {
      newConnection.stop();
    };
  }, [jwt]);

  return {
    connection,
    isLoading: isLoading || connection?.state === HubConnectionState.Connecting,
  };
};
