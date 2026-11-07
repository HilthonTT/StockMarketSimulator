"use client";

import { Suspense } from "react";
import { ErrorBoundary, type FallbackProps } from "react-error-boundary";
import { QueryErrorResetBoundary } from "@tanstack/react-query";
import { AlertTriangleIcon, RotateCcwIcon } from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

interface QueryBoundaryProps {
  children: React.ReactNode;
  fallback: React.ReactNode;
  errorTitle?: string;
  className?: string;
}

/**
 * Suspense + error boundary for a data section. Errors render an inline message with a
 * retry button that refetches the failed queries, instead of the old bare "Error" text.
 */
export const QueryBoundary = ({ children, fallback, errorTitle, className }: QueryBoundaryProps) => {
  return (
    <QueryErrorResetBoundary>
      {({ reset }) => (
        <ErrorBoundary
          onReset={reset}
          fallbackRender={(props) => (
            <ErrorFallback {...props} title={errorTitle} className={className} />
          )}
        >
          <Suspense fallback={fallback}>{children}</Suspense>
        </ErrorBoundary>
      )}
    </QueryErrorResetBoundary>
  );
};

const ErrorFallback = ({
  error,
  resetErrorBoundary,
  title = "This section couldn't load",
  className,
}: FallbackProps & { title?: string; className?: string }) => {
  const message = error instanceof Error ? error.message : undefined;

  return (
    <div
      role="alert"
      className={cn(
        "flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed p-8 text-center",
        className
      )}
    >
      <AlertTriangleIcon className="size-6 text-loss" aria-hidden="true" />
      <div>
        <p className="font-medium">{title}</p>
        {message && <p className="mt-1 text-sm text-muted-foreground">{message}</p>}
      </div>
      <Button variant="outline" size="sm" onClick={resetErrorBoundary}>
        <RotateCcwIcon />
        Try again
      </Button>
    </div>
  );
};
