"use client";

import { Badge } from "@/components/ui/badge";
import {
  Ban,
  Check,
  CircleAlert,
  CircleX,
  Clock,
  Eye,
  LoaderCircle,
  type LucideIcon,
} from "lucide-react";

const icons: Record<string, LucideIcon> = {
  queued: Clock,
  processing: LoaderCircle,
  needs_review: Eye,
  completed: Check,
  completed_with_unresolved_items: CircleAlert,
  failed: CircleX,
  cancelled: Ban,
};

const tones: Record<string, "neutral" | "mint" | "high" | "medium" | "uncertain"> = {
  queued: "neutral",
  processing: "mint",
  needs_review: "medium",
  completed: "mint",
  completed_with_unresolved_items: "medium",
  failed: "high",
  cancelled: "uncertain",
};

export function StatusMark({ status, label }: { status: string; label: string }) {
  const Icon = icons[status] ?? Eye;
  return (
    <Badge tone={tones[status] ?? "neutral"}>
      <Icon aria-hidden="true" className={status === "processing" ? "size-3.5 animate-spin" : "size-3.5"} />
      <span>{label}</span>
    </Badge>
  );
}
