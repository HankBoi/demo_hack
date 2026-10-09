import * as React from "react";
import { cn } from "@/lib/utils";

export function Input({ className, ...props }: React.ComponentProps<"input">) {
  return (
    <input
      className={cn(
        "flex min-h-10 w-full rounded-xl border border-white/15 bg-black/25 px-3 py-2 text-sm text-foreground outline-none placeholder:text-muted focus-visible:border-mint focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-mint",
        className,
      )}
      {...props}
    />
  );
}
