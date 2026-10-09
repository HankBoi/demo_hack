"use client";

import { Button } from "@/components/ui/button";
import { useI18n, type Locale } from "@/lib/i18n";
import Link from "next/link";

export function SiteHeader() {
  const { locale, setLocale, t } = useI18n();

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-4">
        <Link href="/" className="group min-w-0">
          <p className="text-xs font-medium tracking-[0.14em] text-accent uppercase">
            {t("localDemo")}
          </p>
          <p className="truncate text-lg font-semibold text-foreground">{t("productName")}</p>
        </Link>
        <div className="flex items-center gap-2" role="group" aria-label={t("language")}>
          <span className="text-xs text-muted">{t("language")}</span>
          {(["az", "en"] as Locale[]).map((code) => (
            <Button
              key={code}
              type="button"
              size="sm"
              variant={locale === code ? "default" : "outline"}
              aria-pressed={locale === code}
              onClick={() => setLocale(code)}
            >
              {code.toUpperCase()}
            </Button>
          ))}
        </div>
      </div>
    </header>
  );
}
