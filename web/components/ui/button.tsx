import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import * as React from "react";
import { cn } from "@/lib/utils";

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 rounded-xl text-sm font-medium whitespace-nowrap transition-all duration-200 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-mint disabled:pointer-events-none disabled:opacity-50",
  {
    variants: {
      variant: {
        default:
          "bg-gradient-to-b from-accent to-accent-strong text-accent-foreground shadow-[0_8px_24px_-10px_rgba(52,211,153,0.9)] hover:brightness-110 active:translate-y-px",
        outline:
          "glass text-foreground hover:border-mint/50 hover:bg-white/10 active:translate-y-px",
        ghost: "text-foreground hover:bg-white/10",
        danger:
          "bg-danger-strong text-white hover:brightness-110 active:translate-y-px focus-visible:outline-danger",
      },
      size: {
        default: "min-h-10 px-4 py-2",
        sm: "min-h-9 px-3 py-1.5 text-xs",
        lg: "min-h-12 px-6 text-base",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  },
);

export function Button({
  className,
  variant,
  size,
  asChild = false,
  ...props
}: React.ComponentProps<"button"> &
  VariantProps<typeof buttonVariants> & { asChild?: boolean }) {
  const Comp = asChild ? Slot : "button";
  return <Comp className={cn(buttonVariants({ variant, size }), className)} {...props} />;
}

export { buttonVariants };
