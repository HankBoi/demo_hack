import type * as React from "react";
import { cn } from "@/lib/utils";

export function Alert({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div
      role="status"
      className={cn("rounded-md border border-border bg-card px-4 py-3 text-sm", className)}
      {...props}
    />
  );
}
