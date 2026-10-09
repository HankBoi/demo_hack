"use client";

import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { activateDemoPlan } from "@/lib/api";
import { useErrorText, useQuota } from "@/lib/hooks";
import { useI18n } from "@/lib/i18n";
import { cn } from "@/lib/utils";
import { BadgeCheck, CalendarClock, CreditCard, Loader2, ShieldOff } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

export function PlanScreen() {
  const { t, formatDate } = useI18n();
  const errorText = useErrorText();
  const { quota, failed, reload } = useQuota();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [justActivated, setJustActivated] = useState(false);

  async function activate() {
    setBusy(true);
    setError(null);
    try {
      await activateDemoPlan();
      setJustActivated(true);
      await reload();
    } catch (caught) {
      setError(errorText(caught));
    } finally {
      setBusy(false);
    }
  }

  const demoMode = quota?.payments_mode === "demo";
  const percent = quota ? Math.min(100, Math.round((quota.free_used / Math.max(1, quota.free_limit)) * 100)) : 0;

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{t("plan.title")}</h1>
        <p className="mt-2 max-w-2xl leading-7 text-muted">{t("plan.subtitle")}</p>
      </div>

      {failed ? (
        <Alert tone="danger" role="alert">
          {t("errors.api_unreachable")}
        </Alert>
      ) : null}
      {!quota && !failed ? <Alert>{t("loading")}</Alert> : null}
      {error ? (
        <Alert tone="danger" role="alert">
          {error}
        </Alert>
      ) : null}

      {quota ? (
        <div className="grid items-start gap-6 lg:grid-cols-2">
          <Card className="grid gap-4 p-6" aria-labelledby="free-heading">
            <h2 id="free-heading" className="text-lg font-semibold">
              {t("plan.freeTitle")}
            </h2>
            <div
              role="progressbar"
              aria-label={t("plan.freeTitle")}
              aria-valuemin={0}
              aria-valuemax={quota.free_limit}
              aria-valuenow={Math.min(quota.free_used, quota.free_limit)}
              className="h-3 overflow-hidden rounded-full bg-black/35"
            >
              <div className="h-full rounded-full bg-gradient-to-r from-accent-strong to-mint transition-[width] duration-700" style={{ width: `${percent}%` }} />
            </div>
            <p className="text-sm" role="status">
              {t("plan.freeUsed", { used: Math.min(quota.free_used, quota.free_limit), total: quota.free_limit })}
            </p>
            <p className="text-sm leading-6 text-muted">{t("plan.freeRule")}</p>
            {quota.free_remaining > 0 || quota.subscription_active ? (
              <Link href="/new" className={cn(buttonVariants({ variant: "outline" }), "w-fit")}>
                {t("hero.cta")}
              </Link>
            ) : (
              <Alert tone="warn" role="status">
                {t("plan.freeUsedUp")}
              </Alert>
            )}
          </Card>

          <Card className="grid gap-4 p-6" aria-labelledby="demo-heading">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <h2 id="demo-heading" className="text-lg font-semibold">
                {t("plan.demoTitle")}
              </h2>
              {quota.subscription_active ? (
                <Badge tone="mint">
                  <BadgeCheck aria-hidden="true" className="size-3.5" />
                  {t("plan.statusActive")}
                </Badge>
              ) : quota.subscription_status === "expired" ? (
                <Badge tone="medium">
                  <CalendarClock aria-hidden="true" className="size-3.5" />
                  {t("plan.statusExpired")}
                </Badge>
              ) : (
                <Badge tone="uncertain">{t("plan.statusNone")}</Badge>
              )}
            </div>

            <div>
              <p className="text-4xl font-semibold">
                {quota.demo_price_azn} <span className="text-lg text-muted">AZN / {t("plan.month")}</span>
              </p>
              <p className="mt-1 text-sm font-semibold text-amber-200">{t("plan.demoOnly")}</p>
            </div>

            <ul className="grid gap-2 text-sm leading-6 text-muted">
              <li className="flex gap-2">
                <CalendarClock aria-hidden="true" className="mt-1 size-4 shrink-0 text-mint" />
                {t("plan.days")}
              </li>
              <li className="flex gap-2">
                <ShieldOff aria-hidden="true" className="mt-1 size-4 shrink-0 text-mint" />
                {t("plan.noCard")}
              </li>
              <li className="flex gap-2">
                <CreditCard aria-hidden="true" className="mt-1 size-4 shrink-0 text-mint" />
                {t("plan.noCharge")}
              </li>
            </ul>

            {quota.subscription_active ? (
              <Alert tone="success" role="status">
                {justActivated ? `${t("plan.activated")} ` : ""}
                {t("plan.activeUntil", { date: formatDate(quota.subscription_expires_at) })}
              </Alert>
            ) : quota.subscription_status === "expired" ? (
              <Alert tone="warn" role="status">
                {t("plan.expiredOn", { date: formatDate(quota.subscription_expires_at) })}
              </Alert>
            ) : null}

            {demoMode ? (
              <Button type="button" size="lg" disabled={busy} onClick={() => void activate()}>
                {busy ? <Loader2 aria-hidden="true" className="size-5 animate-spin" /> : null}
                {quota.subscription_active ? t("plan.extend") : t("plan.confirm")}
              </Button>
            ) : (
              <Alert tone="warn" role="status">
                {t("errors.payments_disabled")}
              </Alert>
            )}
            <p className="text-xs leading-5 text-muted">{t("plan.honest")}</p>
          </Card>
        </div>
      ) : null}

      <Card className="p-6 text-sm leading-6 text-muted">
        <h2 className="font-semibold text-foreground">{t("plan.accessTitle")}</h2>
        <p className="mt-2">{t("plan.accessBody")}</p>
      </Card>
    </div>
  );
}
