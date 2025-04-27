import { columns } from "../components/columns";
import { DataTable } from "../components/data-table";

const data = [
  {
    id: "728ed52f",
    amount: 100,
    status: "pending",
    email: "m@example.com",
  },
];

export const TransactionSection = () => {
  return <DataTable columns={columns} data={data} />;
};
