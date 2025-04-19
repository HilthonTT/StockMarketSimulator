import { fetchStockPrice } from "../services/stock-service.js";

/**
 * Represents a transaction displayed in a widget, with real-time updates via SignalR.
 */
export class TransactionWidget {
  /**
   * @param {string} id - The unique ID of the transaction.
   * @param {string} userId - The ID of the user who created the transaction.
   * @param {string} ticker - The stock ticker symbol.
   * @param {number} limitPrice - The limit price for the transaction.
   * @param {number} type - The transaction type (e.g., 0 for buy, 1 for sell).
   * @param {number} quantity - The quantity of the stock involved.
   * @param {number} totalAmount - The total amount for the transaction.
   * @param {string|Date} createdOnUtc - The creation timestamp in UTC.
   * @param {string|Date|null} [modifiedOnUtc] - The modification timestamp in UTC, if available.
   * @param {import("@microsoft/signalr").HubConnection} connection - The SignalR hub connection.
   */
  constructor(
    id,
    userId,
    ticker,
    limitPrice,
    type,
    quantity,
    totalAmount,
    createdOnUtc,
    modifiedOnUtc,
    connection
  ) {
    /** @type {string} */
    this.id = id;

    /** @type {string} */
    this.userId = userId;

    /** @type {string} */
    this.ticker = ticker;

    /** @type {number} */
    this.limitPrice = limitPrice;

    /** @type {number} */
    this.type = type;

    /** @type {number} */
    this.quantity = quantity;

    /** @type {number} */
    this.totalAmount = totalAmount;

    /** @type {Date} */
    this.createdOnUtc = new Date(createdOnUtc);

    /** @type {Date|null} */
    this.modifiedOnUtc = modifiedOnUtc ? new Date(modifiedOnUtc) : null;

    /** @type {import("@microsoft/signalr").HubConnection} */
    this.connection = connection;

    /** @type {HTMLElement} */
    this.element = this.createWidget();

    /** @type {HTMLElement} */
    this.priceElement = this.element.querySelector(".stock-price");

    /** @type {number|null} */
    this.lastPrice = null;

    /** @type {number[]} */
    this.priceHistory = [];

    /** @type {number} */
    this.minPrice = Infinity;

    /** @type {number} */
    this.maxPrice = -Infinity;

    this.fetchPrice();
  }

  /**
   * Creates the HTML element representing this transaction widget.
   * @returns {HTMLTableRowElement} The widget element.
   */
  createWidget() {
    const widget = document.createElement("tr");

    widget.className =
      "even:bg-gray-100 dark:even:bg-[#292B36] text-black dark:text-white hover:bg-gray-200 dark:hover:bg-[#2C2E39] transition";

    widget.innerHTML = `
        <td class="py-3 px-4">
        ${this.createdOnUtc.toLocaleString("en-US", {
          month: "short",
          day: "2-digit",
          year: "numeric",
          hour: "2-digit",
          minute: "2-digit",
        })}
        </td>
        <td class="py-3 px-4">${this.ticker}</td>
        <td class="py-3 px-4 font-semibold">
        <button class="px-3 py-1 rounded-md transition ${
          this.type === 1
            ? "bg-emerald-500 text-white hover:bg-emerald-600"
            : "bg-red-500 text-white hover:bg-red-600"
        }">
            ${this.type === 0 ? "Sell" : "Buy"}
        </button>
        </td>
        <td class="py-3 px-4">$${this.limitPrice.toFixed(2)}</td>
        <td class="py-3 px-4">${this.quantity}</td>
        <td class="py-3 px-4">${this.limitPrice.toFixed(2)}</td>
        <td class="py-3 px-4 stock-price">Loading...</td>
        `;

    return widget;
  }

  /**
   * Fetches the current stock price and subscribes to live updates via SignalR.
   * Displays and updates the price in the widget.
   * @returns {Promise<void>}
   */
  async fetchPrice() {
    try {
      const stockPrice = await fetchStockPrice(this.ticker);

      if (!stockPrice) {
        this.priceElement.textContent = "Error fetching price";
        return;
      }

      this.updatePrice(stockPrice.price);

      await this.connection.invoke("JoinGroup", this.ticker);
      console.log(`Joined SignalR group for ${this.ticker}`);
    } catch (error) {
      console.error("Error fetching stock price:", error);
      this.priceElement.textContent = "Error fetching price";
    }
  }

  /**
   * Updates the widget with the new stock price and computes change from last price.
   * @param {number} newPrice - The latest stock price to display.
   */
  updatePrice(newPrice) {
    const formattedPrice = `$${newPrice.toFixed(2)}`;

    this.priceElement.textContent = formattedPrice;

    if (this.lastPrice !== null) {
      const change = newPrice - this.lastPrice;
      const changePercent = (change / this.lastPrice) * 100;
      const changeText = `${change >= 0 ? "+" : ""}${change.toFixed(
        2
      )} (${changePercent.toFixed(2)}%)`;

      this.changeElement.textContent = changeText;
      this.changeElement.className = `stock-change text-xl font-semibold ${
        change >= 0 ? "text-green-600" : "text-red-600"
      }`;

      this.lastPrice = newPrice;
    }
  }

  /**
   * Removes the widget and unsubscribes from SignalR group.
   * Calls `onRemove()` callback if defined.
   * @returns {Promise<void>}
   */
  async remove() {
    try {
      await this.connection.invoke("LeaveStockGroup", this.ticker);
      console.log(`Left SignalR group for ${this.ticker}`);

      this.element.remove();

      if (typeof this.onRemove === "function") {
        this.onRemove(this.ticker);
      }
    } catch (error) {
      console.error(`Error leaving SignalR group for ${this.ticker}:`, error);
    }
  }
}
