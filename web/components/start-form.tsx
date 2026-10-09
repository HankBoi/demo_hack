"use client";

import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ApiError, createAnalysis, filesFromDemo, DEMO_TENDER, getHealth } from "@/lib/api";
import { useI18n } from "@/lib/i18n";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export function StartForm() {
  const { t } = useI18n();
  const router = useRouter();
  const [label, setLabel] = useState("Şəhər kitabxanası mebeli");
  const [file, setFile] = useState<File | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [health, setHealth] = useState<"loading" | "ok" | "down">("loading");

  useEffect(() => {
    let stop = false;
    getHealth()
      .then(() => {
        if (!stop) setHealth("ok");
      })
      .catch(() => {
        if (!stop) setHealth("down");
      });
    return () => {
      stop = true;
    };
  }, []);

  async function submit(nextFile: File, nextLabel: string) {
    setBusy(true);
    setError(null);
    try {
      const created = await createAnalysis(nextLabel.trim(), nextFile);
      router.push(`/analyses/${created.analysis_id}`);
    } catch (caught) {
      setError(messageFor(caught, t));
      setBusy(false);
    }
  }

  return (
    <Card className="p-5">
      <h2 className="text-xl font-semibold">{t("startTitle")}</h2>
      <p className="mt-2 text-sm text-muted" role="status">
        {health === "ok" ? t("apiOk") : health === "down" ? t("apiDown") : t("loading")}
      </p>
      <form
        className="mt-4 grid gap-4"
        onSubmit={(event) => {
          event.preventDefault();
          if (!file || label.trim().length < 2) {
            setError(t("errors.unsupported_file_type"));
            return;
          }
          void submit(file, label);
        }}
      >
        <div className="grid gap-1.5">
          <Label htmlFor="tender-name">{t("tenderName")}</Label>
          <Input
            id="tender-name"
            name="label"
            value={label}
            minLength={2}
            maxLength={200}
            required
            onChange={(event) => setLabel(event.target.value)}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="tender-file">{t("tenderFile")}</Label>
          <Input
            id="tender-file"
            name="file"
            type="file"
            accept="application/pdf,.pdf"
            onChange={(event) => setFile(event.target.files?.[0] ?? null)}
          />
        </div>
        {error ? (
          <Alert className="border-danger/40 text-danger" role="alert">
            {error}
          </Alert>
        ) : null}
        <div className="flex flex-wrap gap-2">
          <Button type="submit" disabled={busy}>
            {busy ? t("working") : t("start")}
          </Button>
          <Button
            type="button"
            variant="outline"
            disabled={busy}
            onClick={() => {
              void (async () => {
                setBusy(true);
                setError(null);
                try {
                  const [demo] = await filesFromDemo([DEMO_TENDER]);
                  await submit(demo, label.trim().length >= 2 ? label : "Uydurma tender");
                } catch (caught) {
                  setError(messageFor(caught, t));
                  setBusy(false);
                }
              })();
            }}
          >
            {t("useDemo")}
          </Button>
        </div>
        <p className="text-xs leading-5 text-muted">{t("demoHint")}</p>
      </form>
    </Card>
  );
}

function messageFor(caught: unknown, t: (path: string) => string) {
  if (caught instanceof ApiError) {
    const key = `errors.${caught.code}`;
    const localized = t(key);
    return localized === key ? caught.message : localized;
  }
  return t("errors.generic");
}
