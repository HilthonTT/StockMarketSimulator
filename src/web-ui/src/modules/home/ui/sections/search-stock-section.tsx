import { SearchIcon } from "lucide-react";

import { Input } from "@/components/ui/input";

// TODO: Implement it later
export const SearchStockSection = () => {
  return (
    <div className="px-4 lg:px-12 py-8 border-b flex flex-col gap-4 w-full ">
      <div className="flex items-center gap-2 w-full">
        <div className="relative w-full">
          <SearchIcon className="absolute top-1/2 left-3 -translate-y-1/2 size-4 text-neutral-500" />
          <Input
            className="pl-8"
            placeholder="Search stocks to buy..."
            disabled={false}
          />
        </div>
      </div>
    </div>
  );
};
