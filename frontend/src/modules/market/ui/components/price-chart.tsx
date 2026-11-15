"use client";

import { useId, useState } from "react";
import { format } from "date-fns";
import { ActivityIcon } from "lucide-react";
import {
  Area,
  AreaChart,
  CartesianGrid,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { HISTORY_RANGES, type HistoryRange } from "@/modules/stocks/types";

import { cn, formatCurrency, formatPercent } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";

import type { PricePoint } from "../../store";
import { usePriceHistory } from "../../hooks/use-price-history";

type ReferenceLevel = { value: number; label: string };

interface PriceChartProps {
  history: PricePoint[] | undefined;
  /** Dashed horizontal levels, e.g. the previous close or the user's average cost. */
  references?: ReferenceLevel[];
  /** Colour the line against this value (defaults to the first point). */
  baselineValue?: number;
  range?: HistoryRange;
  height?: number;
  className?: string;
  emptyLabel?: string;
}

const AXIS_FORMAT: Record<HistoryRange, string> = {
  "1D": "HH:mm",
  "1W": "EEE HH:mm",
  "1M": "MMM d",
  "3M": "MMM d",
};

const TOOLTIP_FORMAT: Record<HistoryRange, string> = {
  "1D": "HH:mm:ss",
  "1W": "EEE MMM d, HH:mm",
  "1M": "MMM d, HH:mm",
  "3M": "MMM d, yyyy",
};

type ChartTooltipProps = {
  active?: boolean;
  payload?: ReadonlyArray<{ payload?: unknown }>;
  range: HistoryRange;
  baseline: number;
};

const ChartTooltip = ({
  active,
  payload,
  range,
  baseline,
}: ChartTooltipProps) => {
  const point = payload?.[0]?.payload as PricePoint | undefined;

  if (!active || !point) {
    return null;
  }

  const change = baseline ? (point.price - baseline) / baseline : 0;

  return (
    <div className="rounded-md border bg-popover px-3 py-2 text-sm shadow-lg">
      <p className="num font-semibold">{formatCurrency(point.price)}</p>
      <p className={cn("num text-xs", change >= 0 ? "text-gain" : "text-loss")}>
        {formatPercent(change)}
      </p>
      <p className="text-xs text-muted-foreground">
        {format(point.time, TOOLTIP_FORMAT[range])}
      </p>
    </div>
  );
};

/** Area chart of a price series, coloured green/red against its baseline. */
export const PriceChart = ({
  history,
  references = [],
  baselineValue,
  range = "1D",
  height = 320,
  className,
  emptyLabel = "Waiting for price data…",
}: PriceChartProps) => {
  const gradientId = useId();

  if (!history) {
    return (
      <Skeleton
        className={cn("w-full rounded-lg", className)}
        style={{ height }}
      />
    );
  }

  if (history.length < 2) {
    return (
      <div
        className={cn(
          "flex flex-col items-center justify-center gap-2 rounded-lg border border-dashed text-sm text-muted-foreground",
          className,
        )}
        style={{ height }}
      >
        <ActivityIcon className="size-5 animate-pulse" aria-hidden="true" />
        {emptyLabel}
      </div>
    );
  }

  const baseline = baselineValue ?? history[0].price;
  const last = history[history.length - 1].price;
  const color = last >= baseline ? "var(--gain)" : "var(--loss)";
  const levels = references.map((reference) => reference.value);

  return (
    <div
      className={className}
      style={{ height }}
      role="img"
      aria-label={`Price chart, ${range}: from ${formatCurrency(history[0].price)} to ${formatCurrency(last)}`}
    >
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart
          data={history}
          margin={{ top: 8, right: 4, bottom: 0, left: 0 }}
        >
          <defs>
            <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor={color} stopOpacity={0.28} />
              <stop offset="100%" stopColor={color} stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid
            vertical={false}
            strokeDasharray="3 3"
            stroke="var(--border)"
          />
          <XAxis
            dataKey="time"
            type="number"
            scale="time"
            domain={["dataMin", "dataMax"]}
            tickFormatter={(value: number) => format(value, AXIS_FORMAT[range])}
            tick={{ fontSize: 11 }}
            tickMargin={8}
            minTickGap={56}
            stroke="var(--muted-foreground)"
            tickLine={false}
            axisLine={false}
          />
          <YAxis
            orientation="right"
            domain={[
              (min: number) => Math.min(min, ...levels) * 0.998,
              (max: number) => Math.max(max, ...levels) * 1.002,
            ]}
            tickFormatter={(value: number) => formatCurrency(value)}
            tick={{ fontSize: 11 }}
            width={76}
            stroke="var(--muted-foreground)"
            tickLine={false}
            axisLine={false}
          />
          <Tooltip
            content={({ active, payload }) => (
              <ChartTooltip
                active={active}
                payload={payload}
                range={range}
                baseline={baseline}
              />
            )}
            cursor={{
              stroke: "var(--muted-foreground)",
              strokeDasharray: "3 3",
            }}
          />
          {references.map((reference) => (
            <ReferenceLine
              key={reference.label}
              y={reference.value}
              stroke="var(--muted-foreground)"
              strokeDasharray="4 4"
              label={{
                value: `${reference.label} ${formatCurrency(reference.value)}`,
                position: "insideTopLeft",
                fontSize: 11,
                fill: "var(--muted-foreground)",
              }}
            />
          ))}
          <Area
            type="monotone"
            dataKey="price"
            stroke={color}
            strokeWidth={2}
            fill={`url(#${gradientId})`}
            isAnimationActive={false}
            dot={false}
            activeDot={{ r: 3, strokeWidth: 0, fill: color }}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};

interface RangedPriceChartProps {
  ticker: string;
  previousClose?: number;
  /** Extra levels, e.g. the user's average cost. */
  references?: ReferenceLevel[];
  height?: number;
  defaultRange?: HistoryRange;
  className?: string;
}

/** Price chart with 1D / 1W / 1M / 3M tabs. The intraday view keeps appending live ticks. */
export const RangedPriceChart = ({
  ticker,
  previousClose,
  references = [],
  height = 340,
  defaultRange = "1D",
  className,
}: RangedPriceChartProps) => {
  const [range, setRange] = useState<HistoryRange>(defaultRange);
  const { points, isError } = usePriceHistory(ticker, range);

  const isIntraday = range === "1D";
  const levels =
    isIntraday && previousClose
      ? [{ value: previousClose, label: "Prev close" }, ...references]
      : references;

  return (
    <div className={cn("flex flex-col gap-3", className)}>
      <ToggleGroup
        type="single"
        size="sm"
        variant="outline"
        value={range}
        onValueChange={(value) => value && setRange(value as HistoryRange)}
        aria-label="Chart range"
        className="self-start"
      >
        {HISTORY_RANGES.map((option) => (
          <ToggleGroupItem
            key={option}
            value={option}
            className="num px-3 text-xs"
          >
            {option}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
      <PriceChart
        history={isError ? [] : points}
        range={range}
        references={levels}
        baselineValue={isIntraday ? previousClose : undefined}
        height={height}
        emptyLabel={
          isError
            ? "Couldn't load the price history."
            : "Waiting for price data…"
        }
      />
    </div>
  );
};
