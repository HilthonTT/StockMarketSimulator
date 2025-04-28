export type TransactionResponse = {
  id: string;
  userId: string;
  ticker: string;
  limitPrice: number;
  type: TransactionType;
  quantity: number;
  createdOnUtc: Date;
  modifiedOnUtc: Date | null | undefined;
};

export enum TransactionType {
  Sell = 0,
  Buy = 1,
}
