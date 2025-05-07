export type TransactionResponse = {
  id: string;
  userId: string;
  ticker: string;
  amount: number;
  currencyCode: string;
  type: TransactionType;
  quantity: number;
  createdOnUtc: Date;
  modifiedOnUtc?: Date | null | undefined;
};

export enum TransactionType {
  Income = 2,
  Expense = 1,
}
