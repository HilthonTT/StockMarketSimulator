import { useEffect, useRef, useState } from "react";
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

export const useSignalR = () => {
  const trpc = useTRPC();
  const { data: jwt } = useSuspenseQuery(trpc.auth.getJwt.queryOptions());

  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);

  useEffect(() => {
    if (!jwt || connectionRef.current) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${SERVER_URL}/stocks-feed`, {
        accessTokenFactory: () => jwt,
        transport: HttpTransportType.LongPolling,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;
    setIsConnecting(true);

    const startConnection = async () => {
      try {
        await connection.start();
        setIsConnected(true);
      } catch (err) {
        console.error("SignalR connection error:", err);
        connectionRef.current = null;
      } finally {
        setIsConnecting(false);
      }
    };

    startConnection();
  }, [jwt]);

  return {
    connection: isConnected ? connectionRef.current : null,
    isLoading: isConnecting || !isConnected,
  };
};
