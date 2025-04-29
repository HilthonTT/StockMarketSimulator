import { useEffect, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

import { trpc } from "@/trpc/client";
import { SERVER_URL } from "@/constants";

export const useSignalR = () => {
  const [jwt] = trpc.auth.getJwt.useSuspenseQuery();

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!jwt) {
      return;
    }

    console.log(jwt);

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

  return { connection, isLoading };
};
