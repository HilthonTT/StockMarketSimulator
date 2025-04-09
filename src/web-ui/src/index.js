import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

import { config } from "./utils/config";

const FIVE_SECONDS_IN_MS = 5000;

document.addEventListener("DOMContentLoaded", () => {
  const connection = new HubConnectionBuilder()
    .withUrl(`${config.baseApiUrl}/stocks-feed`)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveStockPriceUpdate", (stockUpdate) => {
    console.log(stockUpdate);
  });

  connection.onclose(async () => {
    console.warn("SignalR connection closed. Attempting to reconnect...");
    await startSignalRConnection();
  });

  window.addEventListener("beforeunload", () => connection.stop());

  async function startSignalRConnection() {
    try {
      await connection.start();
      console.log("SignalR connected.");
    } catch (error) {
      console.error(
        "SignalR connection failed. Retrying in 5 seconds...",
        error
      );
      setTimeout(startSignalRConnection, FIVE_SECONDS_IN_MS);
    }
  }

  startSignalRConnection();
});
