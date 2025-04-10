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
import { getCurrentUser, clearTokens } from "./utils/auth.js";
import {
  DARK_MODE,
  checkThemePreference,
  DARK_MODE_STORAGE_KEY,
} from "./utils/theme.js";
import { searchStocks } from "./services/stock-service.js";
import { debounce } from "./utils/debounce.js";

Chart.register(
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend
);

/**
 * @constant {number}
 */
const FIVE_SECONDS_IN_MS = 5000;

/**
 * @constant {number}
 */
const MAX_TICKERS = 3;

/**
 * @typedef {Object} ChartColor
 * @property {string} borderColor
 * @property {string} backgroundColor
 */

/**
 * @type {ChartColor[]}
 */
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

/**
 * @typedef {Object} StockPriceUpdate
 * @property {string} ticker
 * @property {number} price
 * @property {string} timestamp
 */

/**
 * @typedef {Object} StockSearchResponse
 * @property {string} ticker
 * @property {string} name
 * @property {string} type
 * @property {string} region
 * @property {string} marketOpen
 * @property {string} timezone
 * @property {string} currency
 */

document.addEventListener("DOMContentLoaded", async () => {
  const connection = new HubConnectionBuilder()
    .withUrl(`${config.baseApiUrl}/stocks-feed`)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  /**
   * Receives stock price updates from SignalR.
   * @param {StockPriceUpdate} stockUpdate
   */
  connection.on("ReceiveStockPriceUpdate", (stockUpdate) => {
    console.log(stockUpdate);
  });

  connection.onclose(async () => {
    console.warn("SignalR connection closed. Attempting to reconnect...");
    await startSignalRConnection();
  });

  window.addEventListener("beforeunload", () => connection.stop());

  /**
   * Starts the SignalR connection with automatic retries.
   * @returns {Promise<void>}
   */
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

  /**
   * Initializes the Chart.js chart.
   */
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

  /**
   * A map of ticker symbols to their price histories.
   * @type {Object.<string, number[]>}
   */
  const tickers = {};

  /** @type {string} */
  let selectedTicker = "";

  /**
   * Updates the chart with new price data.
   *
   * @param {number} price - The latest stock price.
   * @param {string} ticker - The ticker symbol for the stock.
   */
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

  const user = await getCurrentUser();
  console.log(user);

  const logoutButton = document.getElementById("logout-button");
  logoutButton.addEventListener("click", () => {
    clearTokens();
    window.location.href = "/sign-in/index.html";
  });

  const searchResults = document.getElementById("search-results");
  const searchInput = document.getElementById("search-input");

  searchInput.addEventListener("input", debounce(handleSearchInput, 500));
  searchInput.addEventListener("focus", () =>
    searchResults.classList.remove("hidden")
  );
  searchInput.addEventListener("blur", () =>
    setTimeout(() => hideSearchResults(), 100)
  );

  /**
   * Handles user input in the search field.
   * @param {InputEvent} event
   */
  async function handleSearchInput(event) {
    const input = /** @type {HTMLInputElement} */ (event.target);
    const query = input.value.trim();
    if (query.length > 0) {
      const data = await searchStocks(query, 1);

      displayResults(data.items);
    } else {
      searchResults.classList.add("hidden");
    }
  }

  /**
   * @param {StockSearchResponse[]} items
   */
  function displayResults(items) {
    searchResults.innerHTML = "";

    if (items.length === 0) {
      searchResults.innerHTML =
        "<div class='p-2 text-gray-400'>No results found.</div>";
    } else {
      searchResults.innerHTML =
        items.length === 0
          ? "<div class='p-2 text-gray-400'>No results found.</div>"
          : items
              .map(
                (item) =>
                  `<div class="p-2 text-gray-300 hover:bg-gray-700 cursor-pointer" data-ticker="${item.ticker}">${item.ticker}</div>`
              )
              .join("");
    }

    // Ensure the search results are visible
    searchResults.classList.remove("hidden");

    // Event delegation to handle clicks on dynamically generated result items
    const resultItems = searchResults.querySelectorAll("div[data-ticker]");

    resultItems.forEach((item) => {
      if (!item) {
        return;
      }

      // Ensure the clicked element is a valid result item
      if (item.hasAttribute("data-ticker")) {
        const ticker = item.getAttribute("data-ticker");

        // TODO: Open modal and fetch price for the clicked ticker
        item.addEventListener("click", () => {
          console.log(`Clicked: ${ticker}`);
        });
      }
    });
  }

  function hideSearchResults() {
    if (searchResults.contains(document.activeElement)) {
      searchResults.classList.add("hidden");
    }
  }
});
