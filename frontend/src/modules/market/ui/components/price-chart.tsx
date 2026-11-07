"use client";

import { useId } from "react";
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
  type TooltipContentProps,
} from "recharts";

import type { PricePoint } from "../../store";

import { cn, formatCurrency } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface PriceChartProps {
  history: PricePoint[] | undefined;
  /** Reference line, e.g. the user's average cost. */
  baseline?: { value: number; label: string };
  height?: number;
  className?: string;
}

const ChartTooltip = ({ active, payload }: Partial<TooltipContentProps<number, string>>) => {
  const point = payload?.[0]?.payload as PricePoint | undefined;

  if (!active || !point) {
    return null;
  }

  return (
    <div className="rounded-md border bg-popover px-3 py-2 text-sm shadow-md">
      <p className="font-semibold tabular-nums">{formatCurrency(point.price)}</p>
      <p className="text-xs text-muted-foreground">{format(point.time, "HH:mm:ss")}</p>
    </div>
  );
};

/** Live area chart of the ticks received this session. */
export const PriceChart = ({ history, baseline, height = 320, className }: PriceChartProps) => {
  const gradientId = useId();

  if (!history) {
    return <Skeleton className={cn("w-full rounded-lg", className)} style={{ height }} />;
  }

  if (history.length < 2) {
    return (
      <div
        className={cn(
          "flex flex-col items-center justify-center gap-2 rounded-lg border border-dashed text-sm text-muted-foreground",
          className
        )}
        style={{ height }}
      >
        <ActivityIcon className="size-5 animate-pulse" aria-hidden="true" />
        Waiting for live price updates…
      </div>
    );
  }

  const first = history[0].price;
  const last = history[history.length - 1].price;
  const color = last >= first ? "var(--gain)" : "var(--loss)";

  return (
    <div className={className} style={{ height }} role="img" aria-label="Live price chart">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={history} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
          <defs>
            <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor={color} stopOpacity={0.3} />
              <stop offset="100%" stopColor={color} stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid vertical={false} strokeDasharray="3 3" className="stroke-border" />
          <XAxis
            dataKey="time"
            type="number"
            scale="time"
            domain={["dataMin", "dataMax"]}
            tickFormatter={(value: number) => format(value, "HH:mm:ss")}
            tick={{ fontSize: 12 }}
            tickMargin={8}
            minTickGap={48}
            stroke="var(--muted-foreground)"
          />
          <YAxis
            orientation="right"
            domain={[
              (min: number) => Math.min(min, baseline?.value ?? min) * 0.999,
              (max: number) => Math.max(max, baseline?.value ?? max) * 1.001,
            ]}
            tickFormatter={(value: number) => formatCurrency(value)}
            tick={{ fontSize: 12 }}
            width={80}
            stroke="var(--muted-foreground)"
          />
          <Tooltip content={<ChartTooltip />} />
          {baseline && (
            <ReferenceLine
              y={baseline.value}
              stroke="var(--muted-foreground)"
              strokeDasharray="4 4"
              label={{
                value: baseline.label,
                position: "insideTopLeft",
                fontSize: 12,
                fill: "var(--muted-foreground)",
              }}
            />
          )}
          <Area
            type="monotone"
            dataKey="price"
            stroke={color}
            strokeWidth={2}
            fill={`url(#${gradientId})`}
            isAnimationActive={false}
            dot={false}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};
