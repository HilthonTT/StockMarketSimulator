"use client";

import { SlidersHorizontalIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface TickerSelectorProps {
  onChange: (ticker: string) => void;
  disabled?: boolean;
  tickers: string[];
}

export const TickerSelector = ({
  onChange,
  tickers,
  disabled,
}: TickerSelectorProps) => {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          size="icon"
          variant="outline"
          disabled={disabled}
          aria-label="Select ticker"
        >
          <SlidersHorizontalIcon className="size-5" aria-hidden="true" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent side="left" aria-label="Ticker options">
        {tickers.map((ticker) => (
          <DropdownMenuItem
            disabled={disabled}
            key={ticker}
            onClick={() => onChange(ticker)}
            className="cursor-pointer transition"
            aria-label={`Select ${ticker}`}
          >
            {ticker}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
