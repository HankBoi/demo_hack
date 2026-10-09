import * as React from "react";
import { cn } from "@/lib/utils";

export function Card({ className, ...props }: React.ComponentProps<"section">) {
  return (
    <section
      className={cn("glass rounded-2xl text-foreground", className)}
      {...props}
    />
  );
}
