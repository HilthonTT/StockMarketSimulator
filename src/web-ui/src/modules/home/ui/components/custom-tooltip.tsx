import type { TooltipProps } from "recharts";

import { Separator } from "@/components/ui/separator";
import { formatCurrency } from "@/lib/utils";

type PayloadItem = {
  payload?: {
    price?: number;
  };
};

export const CustomToolTip = ({
  active,
  payload,
}: TooltipProps<number, string>) => {
  if (!active || !payload || payload.length === 0 || !payload[0].payload) {
    return null;
  }

  const price = (payload[0] as PayloadItem).payload?.price || 0;

  return (
    <div
      role="tooltip"
      aria-label={`Tooltip showing price: ${formatCurrency(price)}`}
      className="rounded-sm bg-background shadow-sm border overflow-hidden"
    >
      <Separator />
      <div className="p-2 px-3 space-y-1">
        <div className="flex items-center justify-between gap-x-4">
          <div className="flex items-center gap-x-2">
            <div
              className="size-1.5 bg-blue-500 rounded-full"
              aria-hidden="true"
            />
            <p className="text-sm text-muted-foreground">Price</p>
          </div>
          <p className="text-sm text-right font-medium">
            {formatCurrency(price)}
          </p>
        </div>
      </div>
    </div>
  );
};
