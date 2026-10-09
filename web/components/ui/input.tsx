import * as React from "react";
import { cn } from "@/lib/utils";

export function Input({ className, ...props }: React.ComponentProps<"input">) {
  return (
    <input
      className={cn(
        "flex h-10 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none placeholder:text-muted focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent",
        className,
      )}
      {...props}
    />
  );
}
