import { Separator } from "@/components/ui/separator";
import { formatCurrency } from "@/lib/utils";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const CustomToolTip = ({ active, payload }: any) => {
  if (!active) {
    return null;
  }

  const price = payload[0].payload.price as number;

  return (
    <div className="rounded-sm bg-background shadow-sm border overflow-hidden">
      <Separator />
      <div className="p-2 px-3 space-y-1">
        <div className="flex items-center justify-between gap-x-4">
          <div className="flex items-center gap-x-2">
            <div className="size-1.5 bg-blue-500 rounded-full" />
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
