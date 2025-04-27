import { FaPiggyBank } from "react-icons/fa";
import { FaArrowTrendDown, FaMedal } from "react-icons/fa6";

import { DataCard } from "../components/data-card";

export const DataSection = () => {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-2 mb-8">
      <DataCard
        title="Budget"
        value={5000}
        percentageChange={12}
        icon={FaPiggyBank}
        variant="default"
        dateRange={""}
      />
      <DataCard
        title="Transactions"
        value={32}
        percentageChange={12}
        icon={FaArrowTrendDown}
        variant="default"
        dateRange={""}
      />
      <DataCard
        title="Top Perfomer"
        value="TSLA"
        percentageChange={12}
        icon={FaMedal}
        variant="default"
        dateRange={""}
      />
    </div>
  );
};
