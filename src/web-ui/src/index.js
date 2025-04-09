import {
  Chart,
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

import { config } from "./utils/config.js";
import {
  DARK_MODE,
  checkThemePreference,
  DARK_MODE_STORAGE_KEY,
} from "./utils/theme.js";

Chart.register(
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend
);

const FIVE_SECONDS_IN_MS = 5000;
const MAX_TICKERS = 3;
const COLORS = [
  {
    borderColor: "rgb(75, 192, 192)",
    backgroundColor: "rgba(75, 192, 192, 0.2)",
  }, // Teal
  {
    borderColor: "rgb(255, 99, 132)",
    backgroundColor: "rgba(255, 99, 132, 0.2)",
  }, // Red
  {
    borderColor: "rgb(54, 162, 235)",
    backgroundColor: "rgba(54, 162, 235, 0.2)",
  }, // Blue
];

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

  /**
   * @type {Chart}
   * The chart instance created using Chart.js
   */
  let chart;

  function createChart() {
    const chartElement = document.querySelector(".chart");

    if (!chartElement) {
      return;
    }

    chart = new Chart(chartElement, {
      type: "line",
      data: {
        labels: [],
        datasets: [],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
          },
          tooltip: {
            enabled: true,
          },
        },
        scales: {
          x: {
            display: false,
            grid: {
              display: false,
            },
            ticks: {
              color: "rgb(156, 163, 175)", // Gray-400 in Tailwind
              font: { size: 12 },
            },
          },
          y: {
            display: true,
            position: "right",
            grid: { display: true, color: "rgba(156, 163, 175, 0.2)" },
            ticks: {
              color: "rgb(156, 163, 175)", // Gray-400 in Tailwind
              font: { size: 10 },
              callback: function (value, index, values) {
                if (index === 0 || index === values.length - 1) {
                  return "$" + value.toFixed(2);
                }

                return "";
              },
            },
          },
        },
        // False to avoid funky animation updates in real time.
        animation: false,
      },
    });
  }

  const tickers = {};
  let selectedTicker = "";

  function updateChart(price, ticker) {
    if (!chart) {
      console.error("Chart not initialized");
      return;
    }

    // Add or update ticker price history
    if (!tickers[ticker]) {
      if (Object.keys(tickers).length >= MAX_TICKERS) {
        // Remove the oldest ticker when limit is reached
        delete tickers[Object.keys(tickers)[0]];
      }
      tickers[ticker] = [];
    }

    tickers[ticker].push(price);
    if (tickers[ticker].length > 30) {
      tickers[ticker].shift(); // Keep last 30 prices
    }

    // Update chart data only for selectedTicker
    chart.data.labels = Array.from({ length: 30 }, (_, i) => i + 1); // Keep x-axis consistent

    chart.data.datasets = Object.entries(tickers)
      .filter(([name]) => name === selectedTicker) // Keep only selected ticker
      .map(([name, prices], index) => {
        const color = COLORS[index % COLORS.length]; // Cycle colors if needed
        return {
          label: name,
          data: prices,
          borderColor: color.borderColor,
          backgroundColor: color.backgroundColor,
          tension: 0.1,
          pointRadius: 3,
          fill: true,
        };
      });

    chart.update();
  }

  createChart();

  const darkModeToggle = document.getElementById("dark-mode-toggle");
  darkModeToggle.addEventListener("click", () => {
    document.documentElement.classList.toggle(DARK_MODE);

    localStorage.setItem(
      DARK_MODE_STORAGE_KEY,
      document.documentElement.classList.contains(DARK_MODE)
    );
  });

  checkThemePreference();
});
