"use client";

import { useEffect, useState } from "react";
import { endOfDay, format, startOfDay, subDays } from "date-fns";
import { CalendarIcon, SearchIcon, XIcon } from "lucide-react";
import type { DateRange } from "react-day-picker";

import { cn } from "@/lib/utils";
import { useDebounce } from "@/hooks/use-debounce";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Separator } from "@/components/ui/separator";

import { useTransactionFilters } from "../../hooks/use-transaction-filters";

const PRESETS = [
  { label: "Today", days: 0 },
  { label: "Last 7 days", days: 6 },
  { label: "Last 30 days", days: 29 },
  { label: "Last 90 days", days: 89 },
];

export const TransactionFilters = () => {
  const [filters, setFilters] = useTransactionFilters();
  const [search, setSearch] = useState(filters.search);
  const [isCalendarOpen, setIsCalendarOpen] = useState(false);

  // Debounce typing into the URL (which refetches) instead of requiring Enter.
  const debouncedSearch = useDebounce(search.trim(), 300);
  useEffect(() => {
    void setFilters((current) =>
      current.search === debouncedSearch ? current : { ...current, search: debouncedSearch }
    );
  }, [debouncedSearch, setFilters]);

  const setRange = (from: Date | undefined, to: Date | undefined) => {
    void setFilters({
      from: from ? startOfDay(from) : null,
      to: to ? endOfDay(to) : from ? endOfDay(from) : null,
    });
  };

  const hasRange = !!filters.from || !!filters.to;
  const rangeLabel = hasRange
    ? `${filters.from ? format(filters.from, "MMM d, yyyy") : "…"} – ${filters.to ? format(filters.to, "MMM d, yyyy") : "…"}`
    : "All time";

  const selected: DateRange | undefined = hasRange
    ? { from: filters.from ?? undefined, to: filters.to ?? undefined }
    : undefined;

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="relative w-full sm:max-w-xs">
        <SearchIcon className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Filter by ticker"
          className="h-10 pl-9 pr-9"
          aria-label="Filter by ticker"
        />
        {search && (
          <Button
            variant="ghost"
            size="icon"
            className="absolute right-1 top-1/2 size-8 -translate-y-1/2 border-transparent"
            onClick={() => setSearch("")}
            aria-label="Clear ticker filter"
          >
            <XIcon />
          </Button>
        )}
      </div>

      <div className="flex items-center gap-1">
        <Popover open={isCalendarOpen} onOpenChange={setIsCalendarOpen}>
          <PopoverTrigger asChild>
            <Button variant="outline" className={cn("h-10 justify-start font-normal", !hasRange && "text-muted-foreground")}>
              <CalendarIcon />
              {rangeLabel}
            </Button>
          </PopoverTrigger>
          <PopoverContent className="flex w-auto flex-col p-0 sm:flex-row" align="start">
            <div className="flex flex-row flex-wrap gap-1 p-3 sm:w-36 sm:flex-col">
              {PRESETS.map((preset) => (
                <Button
                  key={preset.label}
                  variant="ghost"
                  size="sm"
                  className="h-8 justify-start border-transparent"
                  onClick={() => {
                    setRange(subDays(new Date(), preset.days), new Date());
                    setIsCalendarOpen(false);
                  }}
                >
                  {preset.label}
                </Button>
              ))}
              <Button
                variant="ghost"
                size="sm"
                className="h-8 justify-start border-transparent"
                onClick={() => {
                  setRange(undefined, undefined);
                  setIsCalendarOpen(false);
                }}
              >
                All time
              </Button>
            </div>
            <Separator orientation="vertical" className="hidden sm:block" />
            <Calendar
              mode="range"
              numberOfMonths={1}
              defaultMonth={filters.from ?? new Date()}
              selected={selected}
              onSelect={(range) => setRange(range?.from, range?.to)}
              disabled={{ after: new Date() }}
            />
          </PopoverContent>
        </Popover>
        {hasRange && (
          <Button
            variant="ghost"
            size="icon"
            className="size-10 border-transparent"
            onClick={() => setRange(undefined, undefined)}
            aria-label="Clear date range"
          >
            <XIcon />
          </Button>
        )}
      </div>
    </div>
  );
};
