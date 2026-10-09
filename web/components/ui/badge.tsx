import type * as React from "react";
import { cn } from "@/lib/utils";

export function Badge({
  className,
  ...props
}: React.ComponentProps<"span">) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 rounded-full border border-border bg-background px-2 py-0.5 text-xs font-medium text-foreground",
        className,
      )}
      {...props}
    />
  );
}
