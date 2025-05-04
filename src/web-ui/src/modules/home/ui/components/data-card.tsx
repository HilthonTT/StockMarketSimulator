import { IconType } from "react-icons";
import { VariantProps, cva } from "class-variance-authority";

import { cn, formatCurrency } from "@/lib/utils";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CountUp } from "@/components/count-up";

const boxVariant = cva("rounded-md p-3", {
  variants: {
    variant: {
      default: "bg-blue-500/20",
      success: "bg-emerald-500/20",
      danger: "bg-rose-500/20",
      warning: "bg-yellow-500/20",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

const iconVariant = cva("size-6", {
  variants: {
    variant: {
      default: "fill-blue-500",
      success: "fill-emerald-500",
      danger: "fill-rose-500",
      warning: "fill-yellow-500",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

type BoxVariants = VariantProps<typeof boxVariant>;
type IconVariants = VariantProps<typeof iconVariant>;

interface DataCardProps extends BoxVariants, IconVariants {
  icon: IconType;
  title: string;
  description: string;
  value?: number | string;
  dollarPrefix?: boolean;
}

export const DataCard = ({
  icon: Icon,
  title,
  value = 0,
  variant = "default",
  description,
  dollarPrefix,
}: DataCardProps) => {
  return (
    <Card className="border-none drop-shadow-sm dark:bg-black">
      <CardHeader className="flex flex-row items-center justify-between gap-x-4">
        <div className="space-y-2">
          <CardTitle className="text-2xl line-clmap-1">{title}</CardTitle>
        </div>
        <div
          className={cn(
            "shrink-0",
            boxVariant({
              variant,
            })
          )}
        >
          <Icon
            className={cn(
              iconVariant({
                variant,
              })
            )}
          />
        </div>
      </CardHeader>
      <CardContent>
        <h1 className="font-bold text-2xl mb-2 line-clamp-1 break-all">
          {typeof value === "number" && (
            <CountUp
              preserveValue
              start={0}
              end={value}
              decimal="2"
              decimalPlaces={2}
              formattingFn={dollarPrefix ? formatCurrency : undefined}
            />
          )}
          {typeof value === "string" && value}
        </h1>
        <p className="text-muted-foreground text-sm line-clamp-1">
          {description}
        </p>
      </CardContent>
    </Card>
  );
};
