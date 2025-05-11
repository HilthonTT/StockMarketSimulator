"use client";

import { endOfMonth, format, startOfMonth } from "date-fns";
import { CalendarIcon, XIcon } from "lucide-react";
import { DateRange } from "react-day-picker";

import { useTransactionFilters } from "@/modules/transactions/hooks/use-transaction-filters";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

export const DateRangePicker = ({
  className,
}: React.HTMLAttributes<HTMLDivElement>) => {
  const [filters, setFilters] = useTransactionFilters();

  const startOfThisMonth = startOfMonth(new Date());
  const endOfThisMonth = endOfMonth(new Date());

  const date: DateRange | undefined = {
    from: getDateOrFallback(filters.startDate, startOfThisMonth),
    to: getDateOrFallback(filters.endDate, endOfThisMonth),
  };

  const handleSetDateRange = (range: DateRange | undefined) => {
    if (!range) {
      return;
    }

    setFilters({
      startDate: range.from,
      endDate: range.to,
    });
  };

  const handleClear = (e: React.MouseEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();

    setFilters({
      startDate: new Date(0),
      endDate: new Date(0),
    });
  };

  const initialTime = new Date(0).getTime();

  const hasValue =
    filters.startDate?.getTime() !== initialTime ||
    filters.endDate?.getTime() !== initialTime;

  console.log({ hasValue });

  return (
    <div
      className={cn(
        "flex items-center justify-center gap-2 shrink-0 ",
        className
      )}
    >
      <Popover>
        <PopoverTrigger asChild>
          <Button
            id="date"
            variant="outline"
            className={cn(
              "lg:w-[270px] justify-start text-left font-normal rounded-full shrink-0 relative",
              !date && "text-muted-foreground",
              hasValue ? "w-[70px] lg:w-[290px]" : "w-auto"
            )}
          >
            <CalendarIcon className="shrink-0" />
            <p className="lg:block hidden">
              {date?.from ? (
                date.to ? (
                  <>
                    {format(date.from, "LLL dd, y")} -{" "}
                    {format(date.to, "LLL dd, y")}
                  </>
                ) : (
                  format(date.from, "LLL dd, y")
                )
              ) : (
                <span>Pick a date</span>
              )}
            </p>

            <div
              onClick={handleClear}
              className={cn(
                "absolute right-3 top-1/2 -translate-y-1/2 [&_svg]:size-5 rounded-full transition hidden cursor-default",
                hasValue && "block cursor-pointer"
              )}
              aria-label="Clear search"
            >
              <XIcon />
            </div>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            initialFocus
            mode="range"
            defaultMonth={date?.from}
            selected={date}
            onSelect={handleSetDateRange}
            numberOfMonths={2}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
};

function getDateOrFallback(date: Date | undefined, fallback: Date) {
  if (!date || date.getTime() === new Date(0).getTime()) {
    return fallback;
  }

  return date;
}
