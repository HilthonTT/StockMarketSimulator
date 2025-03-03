import { debounce } from "./utils/utils.js";
import { clearTokens, ensureAuthenticated } from "./utils/auth.js";
import { fetchBudget } from "./services/budget-service.js";
import { searchStocks, fetchPrice } from "./services/stocks-service.js";
import { fetchTransactions } from "./services/transaction-service.js";
import { TransactionWidget } from "./models/transaction-widget.js";
import { config } from "./utils/config.js";
import {
  buyTransaction,
  sellTransaction,
} from "./services/transaction-service.js";
import {
  DARK_MODE,
  checkThemePreference,
  DARK_MODE_STORAGE_KEY,
} from "./utils/theme.js";

const FIVE_SECONDS_IN_MS = 5000;

document.addEventListener("DOMContentLoaded", async () => {
  let chart;
  let initialBalance = 0;
  let previousBalance = initialBalance;

  const maxTickers = 3; // Maximum allowed tickers
  const tickers = {}; // Store price histories for each ticker
  const colors = [
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

  let selectedTicker = "";

  const currentBalance = document.getElementById("current-balance");
  const changeBalance = document.getElementById("balance-change");
  const logoutButton = document.getElementById("logout-button");

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${config.baseApiUrl}/stocks-feed`)
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  const datePicker1 = document.getElementById("datePicker1");
  const datePicker2 = document.getElementById("datePicker2");
  const inputDate1 = document.getElementById("inputDate1");
  const inputDate2 = document.getElementById("inputDate2");
  const selectedDate1 = document.getElementById("selectedDate1");
  const selectedDate2 = document.getElementById("selectedDate2");

  let isBuying = true;
  const modal = document.getElementById("modal");
  const closeModal = document.getElementById("close-modal");
  const buyButton = document.getElementById("buy-button");
  const sellButton = document.getElementById("sell-button");

  const searchResults = document.getElementById("search-results");
  const searchInput = document.getElementById("search-input");

  let dateRange1 = new Date("2024-12-22");
  let dateRange2 = new Date();

  selectedDate1.textContent = dateRange1.toDateString();
  selectedDate2.textContent = dateRange2.toDateString();

  const darkModeToggle = document.getElementById("dark-mode-toggle");
  darkModeToggle.addEventListener("click", () => {
    document.documentElement.classList.toggle(DARK_MODE);

    localStorage.setItem(
      DARK_MODE_STORAGE_KEY,
      document.documentElement.classList.contains(DARK_MODE)
    );
  });

  const transactionWidgets = [];

  // Initialize SignalR connection
  await startSignalRConnection();

  connection.on("ReceiveStockPriceUpdate", (stockUpdate) => {
    handleStockPriceUpdate(stockUpdate);
  });

  window.addEventListener("beforeunload", () => connection.stop());

  logoutButton.addEventListener("click", () => {
    clearTokens();
    window.location.reload();
  });

  datePicker1.addEventListener("click", () => inputDate1.showPicker());
  datePicker2.addEventListener("click", () => inputDate2.showPicker());

  inputDate1.addEventListener(
    "change",
    async (e) => await handleDateChange(e, selectedDate1, 1)
  );
  inputDate2.addEventListener(
    "change",
    async (e) => await handleDateChange(e, selectedDate2, 2)
  );

  buyButton.addEventListener("click", () => toggleBuySell(true));
  sellButton.addEventListener("click", () => toggleBuySell(false));

  searchInput.addEventListener("input", debounce(handleSearchInput, 500));
  searchInput.addEventListener("focus", () =>
    searchResults.classList.remove("hidden")
  );
  searchInput.addEventListener("blur", () =>
    setTimeout(() => hideSearchResults(), 100)
  );

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

  connection.onclose(async () => {
    console.warn("SignalR connection closed. Attempting to reconnect...");
    await startSignalRConnection();
  });

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

    updateBalance(totalGainOrLoss);
    updateChart(stockUpdate.price, stockUpdate.ticker);
  }

  function calculateTransactionChange(transactionWidget, newPrice) {
    const oldPrice = transactionWidget.limitPrice;
    const quantity = transactionWidget.quantity;
    return (newPrice - oldPrice) * quantity;
  }

  function updateBalance(totalGainOrLoss) {
    const newBalance = previousBalance + totalGainOrLoss;

    currentBalance.textContent = `$${newBalance.toFixed(2)}`;

    const balanceChange = newBalance - initialBalance;

    const percentageChange = (balanceChange / initialBalance) * 100;

    changeBalance.textContent = `${
      balanceChange >= 0 ? "+" : ""
    }$${balanceChange.toFixed(2)} (${percentageChange.toFixed(2)}%)`;

    changeBalance.className =
      balanceChange >= 0 ? "text-emerald-500" : "text-rose-500";

    previousBalance = newBalance;
  }

  async function handleDateChange(event, selectedDateElement, dateIndex) {
    const dateRange = new Date(event.target.value);

    selectedDateElement.textContent = dateRange.toDateString();

    if (dateIndex === 1) {
      dateRange1 = dateRange;
    } else {
      dateRange2 = dateRange;
    }

    await filterTransactions();
  }

  async function filterTransactions() {
    const transactions = await fetchTransactions();
    initialBalance = calculateInitialBalance(transactions);
    const filteredTransactions = filterByDate(transactions);

    const tbody = document.getElementById("trading-history-body");
    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";
    const tickers = new Set();

    filteredTransactions.forEach((transaction) => {
      const widget = createTransactionWidget(transaction);
      transactionWidgets.push(widget);
      tbody.appendChild(widget.element);

      tickers.add(transaction.ticker);
    });

    const firstTransaction = transactions[0];

    if (firstTransaction) {
      selectedTicker = firstTransaction.ticker;
    }

    updateBalanceUI();
    updateTickerButtons(Array.from(tickers));
  }

  function updateTickerButtons(tickers) {
    const tickerContainer = document.getElementById("ticker-buttons");

    if (!tickerContainer) {
      return;
    }

    tickerContainer.innerHTML = "";

    tickers.forEach((ticker) => {
      const button = document.createElement("button");

      button.innerText = ticker;
      button.className = `px-4 py-2 m-1 rounded-md transition ${
        selectedTicker === ticker
          ? "bg-gray-700 text-white font-bold"
          : "bg-gray-500 text-white hover:bg-gray-600"
      }`;

      button.addEventListener("click", () => {
        selectedTicker = ticker;
        updateTickerButtons(tickers);
        updateChart();
      });

      tickerContainer.appendChild(button);
    });
  }

  function calculateInitialBalance(transactions) {
    return transactions.reduce((balance, transaction) => {
      const amount = transaction.limitPrice * transaction.quantity;
      return transaction.type === 1 ? balance + amount : balance - amount;
    }, 0);
  }

  function filterByDate(transactions) {
    return transactions.filter((transaction) => {
      const createdDate = new Date(transaction.createdOnUtc);
      return createdDate >= dateRange1 && createdDate <= dateRange2;
    });
  }

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
      connection
    );
  }

  function updateBalanceUI() {
    currentBalance.textContent = `$${initialBalance.toFixed(2)}`;
    changeBalance.textContent = "+$0.00 (0.00%)";
    changeBalance.className = "text-emerald-500";
  }

  async function handleSearchInput(event) {
    const query = event.target.value.trim();
    if (query.length > 0) {
      const stockData = await fetchStockData(query);
      displayResults(stockData);
    } else {
      searchResults.classList.add("hidden");
    }
  }

  function hideSearchResults() {
    if (!searchResults.contains(document.activeElement)) {
      searchResults.classList.add("hidden");
    }
  }

  async function fetchStockData(query) {
    const matchResults = await searchStocks(query);
    return matchResults.filter((stock) =>
      stock.symbol.toLowerCase().includes(query.toLowerCase())
    );
  }

  function displayResults(results) {
    searchResults.innerHTML =
      results.length === 0
        ? "<div class='p-2 text-gray-400'>No results found.</div>"
        : results
            .map(
              (result) =>
                `<div class="p-2 text-gray-300 hover:bg-gray-700 cursor-pointer" data-ticker="${result.symbol}">${result.symbol}</div>`
            )
            .join("");

    searchResults.classList.remove("hidden");

    const resultItems = searchResults.querySelectorAll("div[data-ticker]");
    resultItems.forEach((item) => {
      item.addEventListener("click", async () => {
        const ticker = item.getAttribute("data-ticker");

        const stockPrice = await fetchPrice(ticker);

        const price = stockPrice.price;

        openModal(ticker, price);
      });
    });

    closeModal.addEventListener("click", () => modal.classList.add("hidden"));
  }

  function openModal(ticker, price) {
    const modalTitle = document.getElementById("modal-title");
    const stockName = modal.querySelector("p.font-bold.text-lg");
    const priceInput = document.querySelector(
      "#transaction-form input[readonly]"
    );

    if (modalTitle) {
      modalTitle.textContent = `Place Order for ${ticker}`;
    }

    if (stockName) {
      stockName.textContent = ticker;
    }

    if (priceInput) {
      priceInput.value = price; // Set the price in the input field
    }

    modal.classList.remove("hidden");

    const transactionForm = document.getElementById("transaction-form");
    if (transactionForm) {
      transactionForm.onsubmit = async (e) => await handleSubmit(e, ticker);
    } else {
      console.error("Transaction form not found!");
    }
  }

  async function handleSubmit(event, ticker) {
    event.preventDefault();

    const quantityInput = document.querySelector(
      "#transaction-form input[type='number']:not([readonly])"
    );
    if (!quantityInput) {
      console.error("Quantity input element not found!");
      return;
    }

    const quantity = parseInt(quantityInput.value);
    if (!quantity) {
      alert("Please enter a valid quantity.");
      return;
    }

    if (isBuying) {
      await buyTransaction(ticker, quantity);
    } else {
      await sellTransaction(ticker, quantity);
    }

    modal.classList.add("hidden");
    await filterTransactions();
  }

  function toggleBuySell(isBuy) {
    const form = document.getElementById("transaction-form");
    const buyRadioButton = form.querySelector("input[value='buy']");
    const sellRadioButton = form.querySelector("input[value='sell']");

    if (isBuy) {
      isBuying = true;
      buyRadioButton.checked = true;
      sellRadioButton.checked = false;

      buyButton.classList.add("bg-emerald-500");
      buyButton.classList.remove("bg-[#2c2d39]");

      sellButton.classList.remove("bg-rose-500");
      sellButton.classList.add("bg-[#2c2d39]");
    } else {
      isBuying = false;
      buyRadioButton.checked = false;
      sellRadioButton.checked = true;

      sellButton.classList.add("bg-rose-500");
      sellButton.classList.remove("bg-[#2c2d39]");
      buyButton.classList.remove("bg-emerald-500");
      buyButton.classList.add("bg-[#2c2d39]");
    }
  }

  async function loadBudget() {
    const budget = await fetchBudget();

    const budgetElement = document.getElementById("buying-power");
    const transactionFormBudgetElement =
      document.getElementById("buying-power-form");
    if (budgetElement) {
      budgetElement.textContent = `$${budget.buyingPower.toFixed(2)}`;
    }

    if (transactionFormBudgetElement) {
      transactionFormBudgetElement.textContent = `$${budget.buyingPower.toFixed(
        2
      )}`;
    }
  }

  function createChart() {
    const chartElement = document.querySelector(".chart");

    if (!chartElement) {
      return;
    }

    const labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"];
    const data = [100, 200, 150, 300, 250, 400, 350];

    chart = new Chart(chartElement, {
      type: "line",
      data: {
        labels: labels, // X-axis labels
        datasets: [
          {
            label: "Revenue",
            data: data, // Y-axis data points
            borderColor: "rgb(75, 192, 192)", // Line color
            tension: 0.1, // Curved line
            pointRadius: 3, // Point radius for data points
            backgroundColor: "rgba(75, 192, 192, 0.2)", // Background under the line
            fill: true, // Fill the area under the line
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true },
          tooltip: { enabled: true },
        },
        scales: {
          x: {
            display: false,
            grid: { display: false },
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

  function updateChart(price, ticker) {
    if (!chart) {
      console.error("Chart is not initialized");
      return;
    }

    // Add or update ticker price history
    if (!tickers[ticker]) {
      if (Object.keys(tickers).length >= maxTickers) {
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
        const color = colors[index % colors.length]; // Cycle colors if needed
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

  checkThemePreference();
  createChart();
  await ensureAuthenticated();

  await Promise.all([loadBudget(), filterTransactions()]);
});
