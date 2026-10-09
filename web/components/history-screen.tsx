"use client";

import { StatusMark } from "@/components/status-mark";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { deleteAnalysis, listAnalyses, type AnalysisListItem } from "@/lib/api";
import { useErrorText } from "@/lib/hooks";
import { useI18n } from "@/lib/i18n";
import { cn } from "@/lib/utils";
import { FlaskConical, FolderOpen, PlusCircle, Trash2 } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";

export function HistoryScreen() {
  const { t, formatDate } = useI18n();
  const errorText = useErrorText();
  const [items, setItems] = useState<AnalysisListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [confirming, setConfirming] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState<string | null>(null);

  const [tick, setTick] = useState(0);

  useEffect(() => {
    let stop = false;
    listAnalyses()
      .then((body) => {
        if (stop) return;
        setItems(body.analyses);
        setError(null);
      })
      .catch((caught) => {
        if (!stop) setError(errorText(caught));
      });
    return () => {
      stop = true;
    };
  }, [tick, errorText]);

  async function remove(id: string) {
    setBusy(true);
    setNotice(null);
    try {
      await deleteAnalysis(id);
      setConfirming(null);
      setNotice(t("history.deleted"));
      setTick((value) => value + 1);
    } catch (caught) {
      setError(errorText(caught));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="grid gap-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{t("history.title")}</h1>
          <p className="mt-2 max-w-2xl leading-7 text-muted">{t("history.subtitle")}</p>
        </div>
        <Link href="/new" className={cn(buttonVariants())}>
          <PlusCircle aria-hidden="true" className="size-4" />
          {t("nav.new")}
        </Link>
      </div>

      {error ? (
        <Alert tone="danger" role="alert">
          {error}
        </Alert>
      ) : null}
      {notice ? <Alert tone="success">{notice}</Alert> : null}
      {items === null && !error ? <Alert>{t("loading")}</Alert> : null}

      {items && items.length === 0 ? (
        <Card className="grid justify-items-center gap-3 p-10 text-center">
          <FolderOpen aria-hidden="true" className="size-10 text-mint" />
          <h2 className="text-lg font-semibold">{t("history.emptyTitle")}</h2>
          <p className="max-w-md text-sm leading-6 text-muted">{t("history.emptyBody")}</p>
          <Link href="/new" className={cn(buttonVariants())}>
            {t("hero.cta")}
          </Link>
        </Card>
      ) : null}

      <ul className="grid gap-4">
        {items?.map((item) => (
          <li key={item.analysis_id}>
            <Card className="grid gap-3 p-5">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0">
                  <h2 className="truncate text-lg font-semibold">
                    <Link href={`/analyses/${item.analysis_id}`} className="hover:text-mint hover:underline">
                      {item.label}
                    </Link>
                  </h2>
                  <p className="mt-1 text-xs text-muted">
                    {t("history.created")}: {formatDate(item.created_at)}
                  </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {item.is_sample ? (
                    <Badge tone="uncertain">
                      <FlaskConical aria-hidden="true" className="size-3.5" />
                      {t("sample.badge")}
                    </Badge>
                  ) : null}
                  <StatusMark status={item.status} label={t(`status.${item.status}`)} />
                </div>
              </div>
              <dl className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-4">
                <Stat label={t("history.files")} value={item.files} />
                <Stat label={t("history.findings")} value={item.findings} />
                <Stat label={t("history.reviewed")} value={`${item.reviewed}/${item.findings}`} />
                <Stat label={t("history.unverified")} value={item.source_not_verified} />
              </dl>
              {item.error_code ? (
                <p className="text-sm text-danger" role="status">
                  {t(`errors.${item.error_code}`) === `errors.${item.error_code}` ? t("errors.generic") : t(`errors.${item.error_code}`)}
                </p>
              ) : null}
              <div className="flex flex-wrap items-center gap-2">
                <Link href={`/analyses/${item.analysis_id}`} className={cn(buttonVariants({ size: "sm" }))}>
                  {t("history.open")}
                </Link>
                {confirming === item.analysis_id ? (
                  <>
                    <span className="text-sm">{t("confirmDelete")}</span>
                    <Button type="button" size="sm" variant="danger" disabled={busy} onClick={() => void remove(item.analysis_id)}>
                      {t("delete")}
                    </Button>
                    <Button type="button" size="sm" variant="outline" onClick={() => setConfirming(null)}>
                      {t("keep")}
                    </Button>
                  </>
                ) : (
                  <Button type="button" size="sm" variant="outline" onClick={() => setConfirming(item.analysis_id)}>
                    <Trash2 aria-hidden="true" className="size-4" />
                    {t("delete")}
                  </Button>
                )}
              </div>
            </Card>
          </li>
        ))}
      </ul>
      {items && items.length > 0 ? <p className="text-xs leading-5 text-muted">{t("history.quotaNote")}</p> : null}
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-black/20 px-3 py-2">
      <dt className="text-xs text-muted">{label}</dt>
      <dd className="mt-0.5 text-base font-semibold">{value}</dd>
    </div>
  );
}
