export type TransactionResponse = {
  id: string;
  userId: string;
  ticker: string;
  amount: number;
  currencyCode: string;
  type: TransactionType;
  quantity: number;
  // JSON strings from the API; parse with `parseUtc`.
  createdOnUtc: string;
  modifiedOnUtc?: string | null | undefined;
};

export enum TransactionType {
  Income = 2,
  Expense = 1,
}

export type TransactionCountResponse = {
  count: number;
};
