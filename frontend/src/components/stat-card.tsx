import type { LucideIcon } from "lucide-react";

import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface StatCardProps {
  label: string;
  value: React.ReactNode;
  icon?: LucideIcon;
  footer?: React.ReactNode;
  className?: string;
}

export const StatCard = ({ label, value, icon: Icon, footer, className }: StatCardProps) => {
  return (
    <Card className={cn("gap-0 py-0", className)}>
      <CardContent className="flex flex-col gap-1 p-5">
        <div className="flex items-center justify-between gap-2">
          <span className="text-sm font-medium text-muted-foreground">{label}</span>
          {Icon && <Icon className="size-4 text-muted-foreground" aria-hidden="true" />}
        </div>
        <div className="text-2xl font-bold tracking-tight tabular-nums truncate">{value}</div>
        {footer && <div className="text-sm text-muted-foreground">{footer}</div>}
      </CardContent>
    </Card>
  );
};

export const StatCardSkeleton = () => (
  <Card className="gap-0 py-0">
    <CardContent className="flex flex-col gap-2 p-5">
      <Skeleton className="h-4 w-24" />
      <Skeleton className="h-7 w-32" />
      <Skeleton className="h-4 w-20" />
    </CardContent>
  </Card>
);
