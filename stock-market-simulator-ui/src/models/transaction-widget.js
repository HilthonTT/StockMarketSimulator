import { config } from "../utils/config.js";

export class TransactionWidget {
  constructor(
    id,
    userId,
    ticker,
    limitPrice,
    type,
    quantity,
    totalAmount,
    createdOnUtc,
    connection
  ) {
    this.id = id;
    this.userId = userId;
    this.ticker = ticker;
    this.limitPrice = limitPrice;
    this.type = type;
    this.quantity = quantity;
    this.totalAmount = totalAmount;
    this.createdOnUtc = new Date(createdOnUtc);
    this.connection = connection;

    this.element = this.createWidget();
    this.priceElement = this.element.querySelector(".stock-price");

    this.lastPrice = null;
    this.priceHistory = [];
    this.minPrice = Infinity;
    this.maxPrice = -Infinity;
    this.fetchPrice();
  }

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
        <td class="py-3 px-4">$${this.totalAmount.toFixed(2)}</td>
        <td class="py-3 px-4 stock-price">Loading...</td>
    `;

    return widget;
  }

  async fetchPrice() {
    try {
      const response = await fetch(
        `${config.baseApiUrl}/api/v1/stocks/${this.ticker}`
      );

      const data = await response.json();

      if (data.price) {
        this.updatePrice(data.price);

        await this.connection.invoke("JoinGroup", this.ticker);
        console.log(`Joined SignalR group for ${this.ticker}`);
      } else {
        this.priceElement.textContent = "Unable to fetch price";
      }
    } catch (error) {
      console.error("Error fetching stock price:", error);
      this.priceElement.textContent = "Error fetching price";
    }
  }

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
