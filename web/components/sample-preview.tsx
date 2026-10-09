"use client";

import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { useI18n } from "@/lib/i18n";
import { Eye } from "lucide-react";

export function SamplePreview() {
  const { t } = useI18n();

  return (
    <Card className="p-5" aria-labelledby="sample-title">
      <p className="text-xs font-medium tracking-[0.14em] text-accent uppercase">{t("sampleKicker")}</p>
      <h2 id="sample-title" className="mt-1 text-xl font-semibold">
        {t("sampleTitle")}
      </h2>
      <p className="mt-2 text-sm leading-6 text-muted">{t("sampleBody")}</p>

      <figure className="mt-4 rounded-md border border-border bg-background p-4">
        <figcaption className="mb-3 flex items-center justify-between text-xs text-muted">
          <span>uydurma-tender.pdf</span>
          <span>
            {t("page")} 3
          </span>
        </figcaption>
        <p className="font-serif text-base leading-7 text-foreground">
          <mark className="bg-highlight px-0.5">{t("sampleQuote")}</mark>
        </p>
      </figure>

      <div className="mt-4 border-l-2 border-accent pl-4">
        <p className="text-sm font-medium">{t("sampleRequirement")}</p>
        <p className="mt-2 text-sm text-muted">{t("sampleEvidence")}</p>
        <Badge className="mt-3">
          <Eye aria-hidden="true" className="size-3.5" />
          {t("sampleStatus")}
        </Badge>
      </div>
    </Card>
  );
}
