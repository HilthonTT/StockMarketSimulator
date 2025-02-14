import { debounce } from "./utils/utils.js";
import { clearTokens, ensureAuthenticated } from "./utils/auth.js";
import { fetchBudget } from "./services/budget-service.js";
import { searchStocks } from "./services/stocks-service.js";
import { fetchTransactions } from "./services/transaction-service.js";
import { TransactionWidget } from "./models/transaction-widget.js";
import { config } from "./utils/config.js";

document.addEventListener("DOMContentLoaded", async () => {
  const transactionWidgets = [];

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${config.baseApiUrl}/stocks-feed`)
    .configureLogging(signalR.LogLevel.Information)
    .build();

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

  // Start the SignalR connection
  await startSignalRConnection();

  connection.on("ReceiveStockPriceUpdate", (stockUpdate) => {
    const transactions = transactionWidgets.filter(
      (transaction) => transaction.ticker === stockUpdate.ticker
    );

    transactions.forEach((transactionWidget) => {
      transactionWidget.updatePrice(stockUpdate.price);
    });
  });

  window.addEventListener("beforeunload", () => {
    connection.stop().then(() => console.log("SignalR Connection Stopped"));
  });

  const logoutButton = document.getElementById("logout-button");

  logoutButton.addEventListener("click", () => {
    clearTokens();

    window.location.reload();
  });

  // Date filters
  let dateRange1 = new Date("2024-12-22");
  let dateRange2 = new Date();

  const datePicker1 = document.getElementById("datePicker1");
  const datePicker2 = document.getElementById("datePicker2");
  const inputDate1 = document.getElementById("inputDate1");
  const inputDate2 = document.getElementById("inputDate2");
  const selectedDate1 = document.getElementById("selectedDate1");
  const selectedDate2 = document.getElementById("selectedDate2");

  selectedDate1.textContent = dateRange1.toDateString();
  selectedDate2.textContent = dateRange2.toDateString();

  datePicker1.addEventListener("click", () => inputDate1.showPicker());
  datePicker2.addEventListener("click", () => inputDate2.showPicker());

  inputDate1.addEventListener("change", (e) => {
    dateRange1 = new Date(e.target.value);
    selectedDate1.textContent = dateRange1.toDateString();
    filterTransactions();
  });

  inputDate2.addEventListener("change", (e) => {
    dateRange2 = new Date(e.target.value);
    selectedDate2.textContent = dateRange2.toDateString();
    filterTransactions();
  });

  // Transaction form modal
  let isBuying = true;

  const modal = document.getElementById("modal");
  const closeModal = document.getElementById("close-modal");
  const buyButton = document.getElementById("buy-button");
  const sellButton = document.getElementById("sell-button");

  buyButton.addEventListener("click", () => {
    buyButton.classList.add("bg-emerald-500");
    buyButton.classList.remove("bg-[#2c2d39]");

    sellButton.classList.remove("bg-rose-500");
    sellButton.classList.add("bg-[#2c2d39]");

    isBuying = true;
    console.log({ isBuying });
  });

  sellButton.addEventListener("click", () => {
    sellButton.classList.add("bg-rose-500");
    sellButton.classList.remove("bg-[#2c2d39]");
    buyButton.classList.remove("bg-emerald-500");
    buyButton.classList.add("bg-[#2c2d39]");

    isBuying = false;
    console.log({ isBuying });
  });

  // Search filter
  const searchResults = document.getElementById("search-results");
  const searchInput = document.getElementById("search-input");

  searchInput.addEventListener(
    "input",
    debounce(async (event) => {
      const query = event.target.value.trim();

      if (query.length > 0) {
        const stockData = await fetchStockData(query);

        displayResults(stockData);
      } else {
        searchResults.classList.add("hidden");
      }
    }, 500)
  );

  searchInput.addEventListener("focus", () => {
    if (searchResults.innerHTML.trim() !== "") {
      searchResults.classList.remove("hidden");
    }
  });

  searchInput.addEventListener("blur", () => {
    setTimeout(() => {
      if (!searchResults.contains(document.activeElement)) {
        searchResults.classList.add("hidden");
      }
    }, 100);
  });

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

  async function filterTransactions() {
    // 0: Buy
    // 1: Sell

    const transactions = await fetchTransactions();

    const filteredTransactions = transactions.filter((t) => {
      const createdDate = new Date(t.createdOnUtc);

      return createdDate >= dateRange1 && createdDate <= dateRange2;
    });

    const tbody = document.getElementById("trading-history-body");
    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    filteredTransactions.forEach((transaction, index) => {
      const transactionWidget = new TransactionWidget(
        transaction.id,
        transaction.userId,
        transaction.ticker,
        transaction.limitPrice,
        transaction.type,
        transaction.quantity,
        transaction.totalAmount,
        transaction.createdOnUtc,
        connection,
        index
      );

      transactionWidgets.push(transactionWidget);

      tbody.appendChild(transactionWidget.element);
    });
  }

  async function fetchStockData(query) {
    const matchResults = await searchStocks(query);

    return matchResults.filter((stock) =>
      stock.symbol.toLowerCase().includes(query.toLowerCase())
    );
  }

  function displayResults(results) {
    searchResults.innerHTML = "";

    if (results.length === 0) {
      searchResults.innerHTML =
        "<div class='p-2 text-gray-400'>No results found.</div>";
      return;
    }

    searchResults.innerHTML = results
      .map(
        (result) =>
          `<div 
            class="p-2 text-gray-300 hover:bg-gray-700 cursor-pointer" 
            data-ticker="${result.symbol}" 
            data-price="${390}">
            ${result.symbol}
          </div>`
      )
      .join("");

    searchResults.classList.remove("hidden");

    const resultItems = searchResults.querySelectorAll("div[data-ticker]");

    resultItems.forEach((item) => {
      item.addEventListener("click", () => {
        const ticker = item.getAttribute("data-ticker");
        const price = item.getAttribute("data-price");

        openModal(ticker, price);
      });
    });

    closeModal.addEventListener("click", () => {
      modal.classList.add("hidden");
    });
  }

  function openModal(ticker, price) {
    const modalTitle = document.getElementById("modal-title");

    const stockName = modal.querySelector("p.font-bold.text-lg");
    const priceInput = modal.querySelector("input[readonly]");

    if (modalTitle) {
      modalTitle.textContent = `Place Order for ${ticker}`;
    } else {
      console.error("Modal title element not found!");
    }

    if (stockName) {
      stockName.textContent = ticker;
    } else {
      console.error("Stock name element not found!");
    }

    if (priceInput) {
      priceInput.value = price;
    } else {
      console.error("Price input element not found!");
    }

    modal.classList.remove("hidden");

    const transactionForm = document.getElementById("transaction-form");
    if (transactionForm) {
      transactionForm.onsubmit = (e) => handleSubmit(e, ticker, price);
    } else {
      console.error("Transaction form not found!");
    }
  }

  function handleSubmit(event, ticker, price) {
    event.preventDefault();

    const quantityInput = document.querySelector(
      "#transaction-form input[type='number']:not([readonly])"
    );
    if (!quantityInput) {
      console.error("Quantity input element not found!");
      return;
    }

    const quantity = parseInt(quantityInput.value, 10);

    if (isNaN(quantity) || quantity <= 0) {
      console.error("Invalid quantity entered!");
      return;
    }

    if (isBuying) {
      console.log({ ticker, price, quantity });
    } else {
      console.log({ ticker, price, quantity });
    }
  }

  function createChart() {
    const chartElement = document.querySelector(".chart");

    if (!chartElement) {
      return;
    }

    const labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"];
    const data = [100, 200, 150, 300, 250, 400, 350];

    const _ = new Chart(chartElement, {
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
            display: true,
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
              callback: function (value) {
                return "$" + value.toFixed(2); // Format Y-axis values as currency
              },
            },
          },
        },
        animation: true,
      },
    });
  }

  const user = await ensureAuthenticated();

  console.log({ user });

  createChart();
  await loadBudget();
  filterTransactions();
});
