"use client";

import { FilePicker } from "@/components/file-picker";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
  deleteDocument,
  documentUrl,
  listDocuments,
  uploadDocuments,
  type LibraryDocument,
  type RejectedUpload,
} from "@/lib/api";
import { formatBytes, useConfig, useErrorText } from "@/lib/hooks";
import { useI18n } from "@/lib/i18n";
import { ExternalLink, FileText, Loader2, Trash2 } from "lucide-react";
import { useCallback, useEffect, useState } from "react";

export function DocumentsScreen() {
  const { t, has, formatDate } = useI18n();
  const errorText = useErrorText();
  const { config } = useConfig();
  const [documents, setDocuments] = useState<LibraryDocument[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [rejected, setRejected] = useState<RejectedUpload[]>([]);
  const [busy, setBusy] = useState(false);
  const [confirming, setConfirming] = useState<string | null>(null);

  const [tick, setTick] = useState(0);
  const load = useCallback(async () => setTick((value) => value + 1), []);

  useEffect(() => {
    let stop = false;
    listDocuments()
      .then((body) => {
        if (stop) return;
        setDocuments(body.documents);
        setError(null);
      })
      .catch((caught) => {
        if (!stop) setError(errorText(caught));
      });
    return () => {
      stop = true;
    };
  }, [tick, errorText]);

  async function upload(files: File[]) {
    setBusy(true);
    setError(null);
    setNotice(null);
    setRejected([]);
    try {
      const result = await uploadDocuments(files);
      setRejected(result.rejected);
      if (result.saved.length > 0) {
        setNotice(t("documents.saved", { n: result.saved.length }));
      }
      await load();
    } catch (caught) {
      setError(errorText(caught));
    } finally {
      setBusy(false);
    }
  }

  async function remove(id: string) {
    setBusy(true);
    setError(null);
    setNotice(null);
    try {
      await deleteDocument(id);
      setConfirming(null);
      setNotice(t("documents.deleted"));
      await load();
    } catch (caught) {
      setError(errorText(caught));
    } finally {
      setBusy(false);
    }
  }

  function rejectionText(item: RejectedUpload) {
    const key = `errors.${item.code}`;
    return has(key) ? t(key) : t("errors.generic");
  }

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{t("documents.title")}</h1>
        <p className="mt-2 max-w-2xl leading-7 text-muted">{t("documents.subtitle")}</p>
      </div>

      <Alert>{t("documents.privacy")}</Alert>

      <Card className="grid gap-3 p-5 sm:p-6" aria-labelledby="upload-heading">
        <h2 id="upload-heading" className="text-lg font-semibold">
          {t("documents.upload")}
        </h2>
        <FilePicker
          label={t("documents.upload")}
          buttonLabel={t("addFiles")}
          multiple
          disabled={busy}
          hint={t("documents.hint", {
            mb: config?.limits.max_upload_mb ?? 15,
            max: config?.limits.max_library_documents ?? 30,
          })}
          onFiles={(files) => void upload(files)}
        />
        {busy ? (
          <p className="flex items-center gap-2 text-sm text-muted" role="status">
            <Loader2 aria-hidden="true" className="size-4 animate-spin" />
            {t("working")}
          </p>
        ) : null}
      </Card>

      {error ? (
        <Alert tone="danger" role="alert">
          {error}
        </Alert>
      ) : null}
      {notice ? <Alert tone="success">{notice}</Alert> : null}
      {rejected.length > 0 ? (
        <Alert tone="warn" role="alert">
          <p className="font-medium">{t("documents.rejected")}</p>
          <ul className="mt-1 grid gap-1">
            {rejected.map((item, index) => (
              <li key={`${item.original_name}-${index}`}>
                <span className="font-medium break-all">{item.original_name}</span>: {rejectionText(item)}
              </li>
            ))}
          </ul>
        </Alert>
      ) : null}

      {documents === null && !error ? <Alert>{t("loading")}</Alert> : null}
      {documents && documents.length === 0 ? (
        <Card className="grid justify-items-center gap-2 p-10 text-center">
          <FileText aria-hidden="true" className="size-10 text-mint" />
          <h2 className="text-lg font-semibold">{t("documents.emptyTitle")}</h2>
          <p className="max-w-md text-sm leading-6 text-muted">{t("documents.emptyBody")}</p>
        </Card>
      ) : null}

      <ul className="grid gap-3">
        {documents?.map((document) => (
          <li key={document.document_id}>
            <Card className="flex flex-wrap items-center gap-3 p-4">
              <FileText aria-hidden="true" className="size-6 shrink-0 text-mint" />
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">{document.original_name}</p>
                <p className="text-xs text-muted">
                  {document.page_count} {t("pagesShort")} · {formatBytes(document.size_bytes)} · {formatDate(document.created_at)}
                </p>
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <a
                  href={documentUrl(document.document_id)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="glass inline-flex min-h-9 items-center gap-1.5 rounded-xl px-3 text-xs font-medium hover:bg-white/10"
                  aria-label={t("documents.viewNamed", { name: document.original_name })}
                >
                  <ExternalLink aria-hidden="true" className="size-4" />
                  {t("documents.view")}
                </a>
                {confirming === document.document_id ? (
                  <>
                    <span className="text-sm">{t("documents.confirmDelete")}</span>
                    <Button type="button" size="sm" variant="danger" disabled={busy} onClick={() => void remove(document.document_id)}>
                      {t("delete")}
                    </Button>
                    <Button type="button" size="sm" variant="outline" onClick={() => setConfirming(null)}>
                      {t("keep")}
                    </Button>
                  </>
                ) : (
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    aria-label={t("documents.deleteNamed", { name: document.original_name })}
                    onClick={() => setConfirming(document.document_id)}
                  >
                    <Trash2 aria-hidden="true" className="size-4" />
                    {t("delete")}
                  </Button>
                )}
              </div>
            </Card>
          </li>
        ))}
      </ul>
    </div>
  );
}
