"use client";

import { useRef, useState } from "react";
import { SearchIcon, XIcon } from "lucide-react";

import { useTransactionFilters } from "@/modules/transactions/hooks/use-transaction-filters";

import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export const SearchTransactions = () => {
  const inputRef = useRef<HTMLInputElement>(null);

  const [filters, setFilters] = useTransactionFilters();

  const [value, setValue] = useState(filters.searchTerm);

  const handleClear = () => {
    setValue("");

    setFilters({
      ...filters,
      searchTerm: "",
    });

    inputRef.current?.blur();
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    setFilters({
      ...filters,
      searchTerm: value,
    });

    inputRef.current?.blur();
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setValue(e.target.value);
  };

  return (
    <form onSubmit={handleSubmit} className="relative max-w-[720px] w-full">
      <Input
        ref={inputRef}
        value={value}
        onChange={handleChange}
        className="md:text-base px-14 w-full border-none focus-visible:shadow-[0_1px_1px_0_rgba(65,69,73,.3),0_1px_3px_1px_rgba(65,69,73,.15)]  rounded-full h-[48px] focus-visible:ring-0 focus:bg-white4"
        placeholder="Search for transactions..."
        aria-label="Search for transactions"
      />

      <Button
        type="submit"
        variant="ghost"
        size="icon"
        className="absolute left-3 top-1/2 -translate-y-1/2 [&_svg]:size-5 rounded-full"
        aria-label="Search"
      >
        <SearchIcon />
      </Button>

      <Button
        onClick={handleClear}
        type="button"
        variant="ghost"
        size="icon"
        className={cn(
          "absolute right-3 top-1/2 -translate-y-1/2 [&_svg]:size-5 rounded-full transition opacity-0 cursor-default",
          value && "opacity-100 cursor-pointer"
        )}
        aria-label="Clear search"
      >
        <XIcon />
      </Button>
    </form>
  );
};
