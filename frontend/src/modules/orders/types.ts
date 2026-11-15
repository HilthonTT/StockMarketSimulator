export type OrderSide = "buy" | "sell";
export type OrderType = "limit" | "stop";
export type OrderStatus = "pending" | "filled" | "cancelled" | "rejected";

export type OrderResponse = {
  id: string;
  ticker: string;
  side: OrderSide;
  type: OrderType;
  quantity: number;
  triggerPrice: number;
  status: OrderStatus;
  filledPrice: number | null;
  rejectionReason: string | null;
  createdOnUtc: string;
  filledOnUtc: string | null;
};
