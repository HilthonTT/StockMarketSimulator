import flatpickr from "flatpickr";
import { z } from "zod";
import {
  Chart,
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  LineController,
} from "chart.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Notyf } from "notyf";

import { config } from "./utils/config.js";
import { getCurrentUser, clearTokens, getJwt } from "./utils/auth.js";
import {
  DARK_MODE,
  checkThemePreference,
  DARK_MODE_STORAGE_KEY,
} from "./utils/theme.js";
import { searchStocks, fetchStockPrice } from "./services/stock-service.js";
import { debounce } from "./utils/debounce.js";
import {
  buyTransaction,
  getTransactions,
  sellTransaction,
} from "./services/transaction-service.js";
import { fetchBudget } from "./services/budget-service.js";
import { TransactionWidget } from "./models/transaction-widget.js";

Chart.register(
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  LineController
);

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

/**
 * @typedef {Object} TransactionResponse
 * @property {string} id
 * @property {string} userId
 * @property {string} ticker
 * @property {number} limitPrice
 * @property {number} type
 * @property {number} quantity
 */

/**
 * @constant {number}
 */
const FIVE_SECONDS_IN_MS = 5000;

/**
 * @constant {number}
 */
const PAGE_SIZE = 20;

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

const transactionSchema = z.object({
  userId: z.string().uuid(),
  ticker: z.string().max(10),
  quantity: z.number().min(1),
});

document.addEventListener("DOMContentLoaded", async () => {
  const notyf = new Notyf();

  const connection = new HubConnectionBuilder()
    .withUrl(`${config.baseApiUrl}/stocks-feed`, {
      accessTokenFactory: () => {
        return getJwt();
      },
    })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  /**
   * Receives stock price updates from SignalR.
   * @param {StockPriceUpdate} stockUpdate
   */
  connection.on("ReceiveStockPriceUpdate", (stockUpdate) => {
    handleStockPriceUpdate(stockUpdate);
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

    const themeColors = {
      primary: "rgb(59, 130, 246)", // Tailwind blue-500
      secondary: "rgb(37, 99, 235)", // Tailwind blue-600
      gray: "rgb(156, 163, 175)", // Tailwind gray-400
      white: "rgb(255, 255, 255)", // Tailwind white
      black: "rgb(0, 0, 0)", // Tailwind black
    };

    chart = new Chart(chartElement, {
      type: "line",
      data: {
        labels: [], // Set your labels dynamically
        datasets: [], // Set your datasets dynamically
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            labels: {
              color: themeColors.gray, // Customize legend text color to match theme
              font: { size: 12 }, // Adjust font size for the legend
            },
          },
          tooltip: {
            enabled: true,
            backgroundColor: themeColors.primary, // Tooltip background color
            titleColor: themeColors.white, // Tooltip title color
            bodyColor: themeColors.white, // Tooltip body color
            borderColor: themeColors.secondary, // Tooltip border color
            borderWidth: 1, // Tooltip border width
          },
        },
        scales: {
          x: {
            display: false,
            grid: {
              display: false,
            },
            ticks: {
              color: themeColors.gray, // Color for X axis ticks
              font: { size: 12 },
            },
          },
          y: {
            display: true,
            position: "right",
            grid: {
              display: true,
              color: "rgba(156, 163, 175, 0.2)", // Grid color for Y axis
            },
            ticks: {
              color: themeColors.gray, // Color for Y axis ticks
              font: { size: 10 },
              callback: function (value, index, values) {
                // Format Y-axis tick values with dollar sign
                if (index === 0 || index === values.length - 1) {
                  return "$" + value.toFixed(2);
                }
                return "";
              },
            },
          },
        },
        animation: false, // Disable animation to avoid performance issues
        elements: {
          line: {
            borderColor: themeColors.primary, // Set line border color to match theme
            borderWidth: 2, // Line border width
            tension: 0.4, // Line tension for smooth curves
          },
          point: {
            backgroundColor: themeColors.secondary, // Point color on the line
            radius: 5, // Point size
          },
        },
        layout: {
          padding: {
            top: 20,
            right: 20,
            bottom: 20,
            left: 20,
          },
        },
      },
    });
  }

  /**
   * A map of ticker symbols to their price histories.
   * @type {Object.<string, number[]>}
   */
  const tickers = {};

  /** @type {string} */
  let firstTickerInTransactions = "";

  /** @type {string} */
  let selectedTickerInSearchResults = "";

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
      .filter(([name]) => name === firstTickerInTransactions) // Keep only selected ticker
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
  if (!user) {
    window.location.href = "/sign-in/index.html";
    return;
  }

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
        "<div class='p-2 text-black'>No results found.</div>";
    } else {
      searchResults.innerHTML =
        items.length === 0
          ? "<div class='p-2 text-black'>No results found.</div>"
          : items
              .map(
                (item) =>
                  `<div class="w-full text-left p-4 hover:bg-black transition-colors cursor-pointer hover:text-white flex items-center text-base font-medium" data-ticker="${item.ticker}">${item.ticker}</div>`
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

        item.addEventListener("click", async () => {
          const stockPrice = await fetchStockPrice(ticker);

          selectedTickerInSearchResults = ticker;

          openModal(ticker, stockPrice?.price || 0);
        });
      }
    });
  }

  function hideSearchResults() {
    if (searchResults.contains(document.activeElement)) {
      searchResults.classList.add("hidden");
    }
  }

  const tickerMoreInfoButton = document.getElementById(
    "ticker-more-info-button"
  );
  tickerMoreInfoButton.addEventListener("click", async () => {
    if (!selectedTickerInSearchResults) {
      return;
    }

    const urlToOpen = `${config.baseApiUrl}/api/v1/shorten/ticker/${selectedTickerInSearchResults}`;
    window.open(urlToOpen, "_blank");
  });

  let isBuying = true;

  const modal = document.getElementById("modal");
  const closeModal = document.getElementById("close-modal");
  closeModal.addEventListener("click", () => {
    modal.classList.add("hidden");
  });

  const buyButton = document.getElementById("buy-button");
  const sellButton = document.getElementById("sell-button");

  buyButton.addEventListener("click", () => toggleBuySell(true));
  sellButton.addEventListener("click", () => toggleBuySell(false));

  /**
   * Toggles between buyin and selling stocks in the transaction form
   *
   * @param {boolean} isBuy - Is buying
   * @returns {void}
   */
  function toggleBuySell(isBuy) {
    const form = document.getElementById("transaction-form");
    const buyRadioButton = form.querySelector("input[value='buy']");
    const sellRadioButton = form.querySelector("input[value='sell']");

    const submitButton = document.getElementById("transaction-form-submit");

    if (isBuy) {
      isBuying = true;
      buyRadioButton.checked = true;
      sellRadioButton.checked = false;

      buyButton.classList.add("bg-emerald-500");
      buyButton.classList.remove("bg-[#2c2d39]");

      sellButton.classList.remove("bg-rose-500");
      sellButton.classList.add("bg-[#2c2d39]");

      submitButton.textContent = "Buy";
      submitButton.classList.add("bg-emerald-500");
      submitButton.classList.remove("bg-rose-500");
    } else {
      isBuying = false;
      buyRadioButton.checked = false;
      sellRadioButton.checked = true;

      sellButton.classList.add("bg-rose-500");
      sellButton.classList.remove("bg-[#2c2d39]");
      buyButton.classList.remove("bg-emerald-500");
      buyButton.classList.add("bg-[#2c2d39]");

      submitButton.textContent = "Sell";
      submitButton.classList.add("bg-rose-500");
      submitButton.classList.remove("bg-emerald-500");
    }
  }

  /**
   * Opens the buy/sell transaction modal
   *
   * @param {string} ticker - The ticker SYMBOL
   * @param {number} price - The stock's price
   * @returns {void}
   */
  function openModal(ticker, price) {
    const modalTitle = document.getElementById("modal-title");
    const stockName = modal.querySelector("p.font-bold.text-lg");
    const priceInput = document.querySelector(
      "#transaction-form input[readonly]"
    );

    if (modalTitle) {
      modalTitle.textContent = `Place order for ${ticker}`;
    }

    if (stockName) {
      stockName.textContent = ticker;
    }

    if (priceInput) {
      priceInput.value = price;
    }

    modal.classList.remove("hidden");
    modal.classList.add("flex");

    const transactionForm = document.getElementById("transaction-form");
    if (transactionForm) {
      transactionForm.onsubmit = (e) => handleSubmit(e, ticker);
    } else {
      console.error("Transaction form not found!");
    }
  }

  /**
   * Handles the transaction form submit
   * @param {Event} event
   * @param {string} ticker
   *
   */
  async function handleSubmit(event, ticker) {
    event.preventDefault();

    const quantityInput = document.getElementById("transaction-form-quantity");

    if (!quantityInput) {
      console.error("Quantity input element not found!");
      return;
    }

    const quantityError = quantityInput.nextElementSibling;

    // Clear previous errors
    [quantityError].forEach((el) => el.classList.add("hidden"));

    const quantity = parseInt(quantityInput.value);
    if (!quantity) {
      notyf.error("Please enter a valid quantity.");
      return;
    }

    const formData = {
      userId: user.id,
      ticker: ticker.trim(),
      quantity,
    };

    const result = await transactionSchema.safeParseAsync(formData);
    if (!result.success) {
      const formattedErrors = result.error.format();

      if (formattedErrors.quantity?._errors) {
        quantityError.textContent = formattedErrors.quantity._errors[0];
        quantityError.classList.remove("hidden");
      }

      return;
    }

    if (isBuying) {
      const { success, error } = await buyTransaction(formData);

      if (success) {
        loadBudget();
        loadTransactions();

        notyf.success(success);
      } else if (error) {
        notyf.error(error);
      }
    } else {
      const { success, error } = await sellTransaction(formData);

      if (success) {
        loadBudget();
        loadTransactions();

        notyf.success(success);
      } else if (error) {
        notyf.error(error);
      }
    }

    modal.classList.add("hidden");
  }

  async function loadBudget() {
    const budget = await fetchBudget();
    if (!budget) {
      return;
    }

    function updateElementText(id, text) {
      const element = document.getElementById(id);
      if (element) {
        element.textContent = text;
      }
    }

    const formattedBuyingPower = `$${budget.buyingPower}`;

    updateElementText("current-balance", formattedBuyingPower);
    updateElementText("buying-power", formattedBuyingPower);
    updateElementText(
      "buying-power-form",
      `Buying Power: ${formattedBuyingPower}`
    );
  }

  loadBudget();

  const transactionWidgets = [];

  /**
   *
   * @param {TransactionResponse} transaction
   * @returns
   */
  function createTransactionWidget(transaction) {
    return new TransactionWidget(
      transaction.id,
      transaction.userId,
      transaction.ticker,
      transaction.limitPrice,
      transaction.type,
      transaction.quantity,
      transaction.totalAmount,
      transaction.createdOnUtc,
      transaction.modifiedOnUtc,
      connection
    );
  }

  let currentPage = 1;
  let hasPreviousPage = false;
  let hasNextPage = false;

  /**
   * @type {HTMLButtonElement|null}
   */
  const previousButton = document.getElementById(
    "transactions-previous-button"
  );
  previousButton.addEventListener("click", async () => {
    if (hasPreviousPage) {
      await loadTransactions(currentPage - 1);
    }
  });

  /**
   * @type {HTMLButtonElement|null}
   */
  const nextButton = document.getElementById("transactions-next-button");
  nextButton.addEventListener("click", async () => {
    if (hasNextPage) {
      await loadTransactions(currentPage + 1);
    }
  });

  /**
   *
   * @param {*} page
   * @param {Object} [filters] - Optional filters for transactions
   * @param {string} [filters.searchTerm] - A search string to filter results
   * @param {Date} [filters.startDate] - Minimum transaction date
   * @param {Date} [filters.endDate] - Maximum transaction date
   * @returns {Promise<void>}
   */
  async function loadTransactions(page = currentPage, filters = {}) {
    const tbody = document.getElementById("trading-history-body");
    if (!tbody) {
      return;
    }

    const pagedTransactions = await getTransactions(page, PAGE_SIZE, filters);
    if (!pagedTransactions) {
      return;
    }

    hasPreviousPage = pagedTransactions.hasPreviousPage;
    hasNextPage = pagedTransactions.hasNextPage;

    tbody.innerHTML = "";
    const tickers = new Set();

    pagedTransactions.items.forEach((transaction) => {
      const widget = createTransactionWidget(transaction);
      transactionWidgets.push(widget);
      tbody.appendChild(widget.element);

      tickers.add(transaction.ticker);
    });

    const firstTransaction = pagedTransactions.items[0];

    if (firstTransaction) {
      firstTickerInTransactions = firstTransaction.ticker;
    }

    if (previousButton) {
      if (!pagedTransactions.hasPreviousPage) {
        previousButton.disabled = true;
        previousButton.classList.add("opacity-50", "cursor-not-allowed");
      } else {
        previousButton.disabled = false;
        previousButton.classList.remove("opacity-50", "cursor-not-allowed");
      }
    }

    if (nextButton) {
      if (!pagedTransactions.hasNextPage) {
        nextButton.disabled = true;
        nextButton.classList.add("opacity-50", "cursor-not-allowed");
      } else {
        nextButton.disabled = false;
        nextButton.classList.remove("opacity-50", "cursor-not-allowed");
      }
    }

    const paginationInfo = document.getElementById("pagination-info");
    if (paginationInfo) {
      const totalPages = Math.ceil(
        pagedTransactions.totalCount / pagedTransactions.pageSize
      );
      paginationInfo.innerHTML = `${pagedTransactions.page} of ${totalPages}`;
    }
  }

  function calculateTransactionChange(transactionWidget, newPrice) {
    const oldPrice = transactionWidget.limitPrice;
    const quantity = transactionWidget.quantity;
    return (newPrice - oldPrice) * quantity;
  }

  async function handleStockPriceUpdate(stockUpdate) {
    const transactions = transactionWidgets.filter(
      (widget) => widget.ticker === stockUpdate.ticker
    );

    let totalGainOrLoss = 0;

    transactions.forEach((widget) => {
      const change = calculateTransactionChange(widget, stockUpdate.price);
      totalGainOrLoss += change;
      widget.updatePrice(stockUpdate.price);
    });

    // updateBalance(totalGainOrLoss);
    updateChart(stockUpdate.price, stockUpdate.ticker);
  }

  let query;
  let startDate;
  let endDate;

  flatpickr("#datePicker1", {
    mode: "range",
    onChange: async (selectedDates, dateStr, instance) => {
      const [start, end] = selectedDates;

      if (start) {
        startDate = start;

        document.getElementById("selectedDate1").textContent =
          start.toDateString();
      }

      if (end) {
        endDate = end;

        document.getElementById("selectedDate2").textContent =
          end.toDateString();
      }

      await loadTransactions(currentPage, {
        searchTerm: query,
        startDate,
        endDate,
      });
    },
  });

  flatpickr("#datePicker2", {
    mode: "range",
    onChange: (selectedDates, dateStr, instance) => {
      const [start, end] = selectedDates;

      if (start) {
        startDate = start;

        document.getElementById("selectedDate1").textContent =
          start.toDateString();
      }

      if (end) {
        endDate = end;

        document.getElementById("selectedDate2").textContent =
          end.toDateString();
      }
    },
  });

  const transactionSearchInput = document.getElementById(
    "transaction-search-input"
  );

  transactionSearchInput.addEventListener(
    "input",
    debounce(handleTransactionSearchInput, 500)
  );

  /**
   * Handles user input in the search field.
   * @param {InputEvent} event
   */
  async function handleTransactionSearchInput(event) {
    const input = /** @type {HTMLInputElement} */ (event.target);
    query = input.value.trim();

    await loadTransactions(currentPage, {
      searchTerm: query,
      startDate,
      endDate,
    });
  }

  loadTransactions();
});
