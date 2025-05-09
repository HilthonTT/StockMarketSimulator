import { ChartSection } from "../sections/chart-section";
import { DataSection } from "../sections/data-section";
import { TransactionSection } from "../sections/transactions-section";
import { SearchStockSection } from "../sections/search-stock-section";

export const HomeView = () => {
  return (
    <>
      <SearchStockSection />
      <div className="max-w-[2400px] mx-auto mb-10 px-4 pt-2.5 flex flex-col gap-y-6 pb-24 h-full">
        <ChartSection />
        <DataSection />
        <TransactionSection />
      </div>
    </>
  );
};
