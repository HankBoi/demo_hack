"use client";

import { SiteHeader } from "@/components/site-header";
import { StatusMark } from "@/components/status-mark";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  ApiError,
  addEvidence,
  cancelAnalysis,
  deleteAnalysis,
  downloadChecklist,
  filesFromDemo,
  getAnalysis,
  getPages,
  getRequirements,
  matchEvidence,
  patchRequirement,
  retryAnalysis,
  DEMO_EVIDENCE,
  type Analysis,
  type PagePreview,
  type Requirement,
} from "@/lib/api";
import { useI18n } from "@/lib/i18n";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";

const active = new Set(["queued", "processing"]);

export function AnalysisWorkspace({ id }: { id: string }) {
  const { t } = useI18n();
  const router = useRouter();
  const [analysis, setAnalysis] = useState<Analysis | null>(null);
  const [requirements, setRequirements] = useState<Requirement[]>([]);
  const [pages, setPages] = useState<PagePreview[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [tick, setTick] = useState(0);

  const refresh = useCallback(() => setTick((value) => value + 1), []);

  useEffect(() => {
    let stop = false;
    let timer = 0;

    async function load() {
      try {
        const next = await getAnalysis(id);
        if (stop) return;
        setAnalysis(next);
        setError(null);
        const [requirementBody, pageBody] = await Promise.all([getRequirements(id), getPages(id)]);
        if (stop) return;
        setRequirements(requirementBody.requirements);
        setPages(pageBody.pages);
        if (active.has(next.status)) {
          timer = window.setTimeout(load, 1500);
        }
      } catch (caught) {
        if (!stop) setError(messageFor(caught, t));
      }
    }

    void load();
    return () => {
      stop = true;
      window.clearTimeout(timer);
    };
  }, [id, tick, t]);

  async function run(action: () => Promise<void>) {
    setBusy(true);
    setError(null);
    setNotice(null);
    try {
      await action();
      refresh();
    } catch (caught) {
      setError(messageFor(caught, t));
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <SiteHeader />
      <main className="mx-auto grid w-full max-w-6xl gap-6 px-4 py-6">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="min-w-0">
            <Link href="/" className="text-sm text-accent underline-offset-4 hover:underline">
              {t("back")}
            </Link>
            <h1 className="mt-1 truncate text-2xl font-semibold">
              {analysis?.label ?? t("loading")}
            </h1>
          </div>
          {analysis ? (
            <div className="flex flex-wrap items-center gap-2" role="status" aria-live="polite">
              <StatusMark status={analysis.status} label={labelFor(t, "status", analysis.status)} />
              <span className="text-sm text-muted">
                {t("progressLabel")}: {labelFor(t, "progress", analysis.progress_stage)}
              </span>
            </div>
          ) : null}
        </div>

        {error ? (
          <Alert className="border-danger/40" role="alert">
            {error}
          </Alert>
        ) : null}
        {notice ? <Alert role="status">{notice}</Alert> : null}
        {!analysis && !error ? <Alert role="status">{t("loading")}</Alert> : null}
        {analysis ? (
          <>
            <WarningList analysis={analysis} />
            <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_20rem]">
              <section className="grid gap-4" aria-labelledby="requirements-heading">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <h2 id="requirements-heading" className="text-lg font-semibold">
                    {t("requirements")}
                  </h2>
                  <div className="flex flex-wrap gap-2">
                    {active.has(analysis.status) ? (
                      <Button type="button" variant="outline" disabled={busy} onClick={() => void run(() => cancelAnalysis(id).then(() => undefined))}>
                        {t("cancel")}
                      </Button>
                    ) : null}
                    {analysis.status === "failed" ? (
                      <Button type="button" disabled={busy} onClick={() => void run(() => retryAnalysis(id).then(() => undefined))}>
                        {t("retry")}
                      </Button>
                    ) : null}
                    <Button
                      type="button"
                      variant="outline"
                      disabled={busy || requirements.length === 0 || active.has(analysis.status)}
                      onClick={() =>
                        void run(async () => {
                          await downloadChecklist(id);
                        })
                      }
                    >
                      {t("export")}
                    </Button>
                  </div>
                </div>
                {analysis.error ? (
                  <Alert className="border-danger/40" role="alert">
                    {messageFor(new ApiError(analysis.error.code, analysis.error.user_message, analysis.error.retryable, 0), t)}
                  </Alert>
                ) : null}
                {requirements.length === 0 && !active.has(analysis.status) && analysis.status !== "failed" ? (
                  <Alert>{t("noRequirements")}</Alert>
                ) : null}
                {requirements.map((requirement) => (
                  <RequirementCard
                    key={requirement.requirement_id}
                    requirement={requirement}
                    disabled={busy}
                    onSave={async (decision, statement, note) => {
                      const saved = await patchRequirement(requirement.requirement_id, decision, statement, note);
                      setRequirements((current) =>
                        current.map((item) => (item.requirement_id === saved.requirement_id ? saved : item)),
                      );
                      setNotice(t("saved"));
                      refresh();
                    }}
                  />
                ))}
              </section>
              <aside className="grid gap-4">
                <EvidencePanel
                  analysis={analysis}
                  busy={busy}
                  onUpload={(files) =>
                    run(async () => {
                      await addEvidence(id, files);
                    })
                  }
                  onDemo={() =>
                    run(async () => {
                      const files = await filesFromDemo(DEMO_EVIDENCE.map((name) => `/demo/${name}`));
                      await addEvidence(id, files);
                    })
                  }
                  onMatch={() =>
                    run(async () => {
                      await matchEvidence(id);
                    })
                  }
                />
                <FileList analysis={analysis} />
                <PageList pages={pages} />
                <Card className="p-4">
                  {confirmingDelete ? (
                    <div className="grid gap-3">
                      <p className="text-sm">{t("confirmDelete")}</p>
                      <div className="flex flex-wrap gap-2">
                        <Button
                          type="button"
                          variant="danger"
                          disabled={busy}
                          onClick={() =>
                            void run(async () => {
                              await deleteAnalysis(id);
                              router.push("/");
                            })
                          }
                        >
                          {t("delete")}
                        </Button>
                        <Button type="button" variant="outline" onClick={() => setConfirmingDelete(false)}>
                          {t("keep")}
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <Button type="button" variant="danger" onClick={() => setConfirmingDelete(true)}>
                      {t("delete")}
                    </Button>
                  )}
                </Card>
              </aside>
            </div>
          </>
        ) : null}
      </main>
    </>
  );
}

function WarningList({ analysis }: { analysis: Analysis }) {
  const { t } = useI18n();
  const warnings = analysis.warnings ?? [];
  const demo = warnings.includes("demo_extractor") || analysis.provider_id === "demo-extractor";
  const live = analysis.provider_id && analysis.provider_id !== "demo-extractor";
  const unreadable = warnings.some((item) => item.startsWith("unreadable:"));
  const duplicate = warnings.some((item) => item.startsWith("duplicate:"));
  if (!demo && !live && !unreadable && !duplicate && !analysis.stale && !active.has(analysis.status)) {
    return null;
  }
  return (
    <div className="grid gap-2">
      {active.has(analysis.status) ? <Alert>{t("waitingWorker")}</Alert> : null}
      {analysis.stale ? <Alert>{t("stale")}</Alert> : null}
      {demo ? <Alert>{t("demoExtractor")}</Alert> : null}
      {live ? (
        <Alert>
          {t("liveModel")}
          {analysis.model_id ? `: ${analysis.model_id}` : ""}
        </Alert>
      ) : null}
      {unreadable ? <Alert>{t("unreadable")}</Alert> : null}
      {duplicate ? <Alert>{t("duplicate")}</Alert> : null}
    </div>
  );
}

function RequirementCard({
  requirement,
  disabled,
  onSave,
}: {
  requirement: Requirement;
  disabled: boolean;
  onSave: (decision: string, statement?: string, note?: string) => Promise<void>;
}) {
  const { t } = useI18n();
  const [editing, setEditing] = useState(false);
  const [statement, setStatement] = useState(requirement.edited_statement || requirement.statement);
  const [note, setNote] = useState(requirement.human_note ?? "");
  const [localError, setLocalError] = useState<string | null>(null);
  const shown = requirement.edited_statement || requirement.statement;
  const evidence = requirement.evidence?.[0];

  async function save(decision: string) {
    if (decision === "not_applicable" && note.trim().length === 0) {
      setLocalError(t("noteRequired"));
      return;
    }
    if (decision === "edit" && statement.trim().length === 0) {
      setLocalError(t("statementRequired"));
      return;
    }
    setLocalError(null);
    await onSave(decision, decision === "edit" ? statement.trim() : undefined, note.trim() || undefined);
    setEditing(false);
  }

  return (
    <Card className="p-4" aria-labelledby={`req-${requirement.requirement_id}`}>
      <div className="flex flex-wrap items-center gap-2">
        <StatusMark status={reviewIcon(requirement.review_status)} label={labelFor(t, "review", requirement.review_status)} />
        <Badge>{labelFor(t, "class", requirement.requirement_class)}</Badge>
        <Badge>{labelFor(t, "mandatoryLabel", requirement.mandatory_label)}</Badge>
      </div>
      <h3 id={`req-${requirement.requirement_id}`} className="mt-3 text-base font-medium leading-6">
        {shown}
      </h3>
      {requirement.edited_statement ? (
        <p className="mt-2 text-sm text-muted">
          <span className="font-medium text-foreground">{t("aiSuggestion")}: </span>
          {requirement.statement}
        </p>
      ) : (
        <p className="mt-2 text-xs text-muted">{t("aiSuggestion")}</p>
      )}
      <figure className="mt-4 rounded-md border border-border bg-background p-3">
        <figcaption className="mb-2 flex flex-wrap justify-between gap-2 text-xs text-muted">
          <span>{requirement.source_file_name ?? t("sourceQuote")}</span>
          <span>
            {t("page")} {requirement.page_number ?? "—"}
          </span>
        </figcaption>
        {requirement.quote_verified && requirement.quote ? (
          <blockquote className="font-serif text-base leading-7 select-text">
            <mark className="bg-highlight">{requirement.quote}</mark>
          </blockquote>
        ) : (
          <p className="text-sm text-muted">{requirement.quote ? t("quoteUnverified") : t("noQuote")}</p>
        )}
        <p className="mt-2 text-xs text-muted">
          {requirement.quote_verified ? t("quoteVerified") : t("quoteUnverified")}
        </p>
      </figure>
      {evidence ? (
        <div className="mt-4 border-l-2 border-accent pl-3 text-sm">
          <p className="font-medium">{labelFor(t, "evidenceStatus", evidence.status)}</p>
          {evidence.evidence_file_name ? (
            <p className="mt-1 text-muted">
              {evidence.evidence_file_name}
              {evidence.page_number ? `, ${t("page")} ${evidence.page_number}` : ""}
            </p>
          ) : null}
          {evidence.quote_verified && evidence.quote ? (
            <p className="mt-2 font-serif leading-6 select-text">{evidence.quote}</p>
          ) : null}
          {evidence.explanation ? <p className="mt-2 leading-6 text-muted">{evidence.explanation}</p> : null}
          {evidence.status === "not_found" ? <p className="mt-1 text-muted">{t("notFoundDetail")}</p> : null}
          {evidence.date_observation ? <p className="mt-2">{evidence.date_observation}</p> : null}
        </div>
      ) : null}
      <div className="mt-4 grid gap-2">
        <Label htmlFor={`note-${requirement.requirement_id}`}>{t("note")}</Label>
        <Textarea
          id={`note-${requirement.requirement_id}`}
          value={note}
          onChange={(event) => setNote(event.target.value)}
        />
        {editing ? (
          <div className="grid gap-2">
            <Label htmlFor={`edit-${requirement.requirement_id}`}>{t("edit")}</Label>
            <Textarea
              id={`edit-${requirement.requirement_id}`}
              value={statement}
              onChange={(event) => setStatement(event.target.value)}
            />
            <Button type="button" disabled={disabled} onClick={() => void save("edit")}>
              {t("save")}
            </Button>
          </div>
        ) : null}
        {localError ? (
          <p className="text-sm text-danger" role="alert">
            {localError}
          </p>
        ) : null}
        <div className="flex flex-wrap gap-2" role="group" aria-label={t("humanDecision")}>
          <Button type="button" size="sm" disabled={disabled} onClick={() => void save("confirm")}>
            {t("confirm")}
          </Button>
          <Button type="button" size="sm" variant="outline" disabled={disabled} onClick={() => setEditing(true)}>
            {t("edit")}
          </Button>
          <Button type="button" size="sm" variant="outline" disabled={disabled} onClick={() => void save("reject")}>
            {t("reject")}
          </Button>
          <Button type="button" size="sm" variant="outline" disabled={disabled} onClick={() => void save("uncertain")}>
            {t("uncertain")}
          </Button>
          <Button type="button" size="sm" variant="outline" disabled={disabled} onClick={() => void save("missing")}>
            {t("missing")}
          </Button>
          <Button type="button" size="sm" variant="outline" disabled={disabled} onClick={() => void save("not_applicable")}>
            {t("notApplicable")}
          </Button>
        </div>
      </div>
    </Card>
  );
}

function EvidencePanel({
  analysis,
  busy,
  onUpload,
  onDemo,
  onMatch,
}: {
  analysis: Analysis;
  busy: boolean;
  onUpload: (files: File[]) => void;
  onDemo: () => void;
  onMatch: () => void;
}) {
  const { t } = useI18n();
  const blocked = active.has(analysis.status) || analysis.status === "failed" || analysis.status === "cancelled";
  return (
    <Card className="p-4">
      <h2 className="font-semibold">{t("evidenceTitle")}</h2>
      <p className="mt-1 text-sm leading-6 text-muted">{t("evidenceHint")}</p>
      <form
        className="mt-3 grid gap-3"
        onSubmit={(event) => {
          event.preventDefault();
          const input = event.currentTarget.elements.namedItem("files");
          const list = input instanceof HTMLInputElement ? Array.from(input.files ?? []) : [];
          if (list.length === 0) return;
          onUpload(list);
          event.currentTarget.reset();
        }}
      >
        <Label htmlFor="evidence-files">{t("addFiles")}</Label>
        <input
          id="evidence-files"
          name="files"
          type="file"
          accept="application/pdf,.pdf"
          multiple
          className="text-sm"
        />
        <Button type="submit" variant="outline" disabled={busy || blocked}>
          {t("addFiles")}
        </Button>
      </form>
      <div className="mt-3 flex flex-col gap-2">
        <Button type="button" variant="outline" disabled={busy || blocked} onClick={onDemo}>
          {t("addDemoEvidence")}
        </Button>
        <Button type="button" disabled={busy || blocked || analysis.counts.requirements === 0} onClick={onMatch}>
          {t("match")}
        </Button>
      </div>
    </Card>
  );
}

function FileList({ analysis }: { analysis: Analysis }) {
  const { t } = useI18n();
  return (
    <Card className="p-4">
      <h2 className="font-semibold">{t("filesHeading")}</h2>
      <ul className="mt-3 grid gap-3 text-sm">
        {analysis.files.map((file) => (
          <li key={file.file_id} className="border-t border-border pt-3 first:border-0 first:pt-0">
            <p className="font-medium break-all">{file.original_name}</p>
            <p className="text-muted">
              {file.role === "tender" ? t("tenderRole") : t("evidenceRole")} · {file.page_count} {t("pages").toLowerCase()}
            </p>
            {file.duplicate_of_file_id ? <p>{t("duplicate")}</p> : null}
            {file.unreadable_pages && file.unreadable_pages.length > 0 ? (
              <p>
                {t("notReadable")}: {file.unreadable_pages.join(", ")}
              </p>
            ) : null}
          </li>
        ))}
      </ul>
    </Card>
  );
}

function PageList({ pages }: { pages: PagePreview[] }) {
  const { t } = useI18n();
  if (pages.length === 0) return null;
  return (
    <Card className="p-4">
      <h2 className="font-semibold">{t("pages")}</h2>
      <ul className="mt-3 grid max-h-[32rem] gap-3 overflow-auto text-sm">
        {pages.map((page) => (
          <li key={`${page.file_id}-${page.page_number}`} className="border-t border-border pt-3 first:border-0 first:pt-0">
            <p className="font-medium">
              {page.file_name} · {t("page")} {page.page_number}
            </p>
            <p className="text-muted">
              {page.usable ? t("readable") : t("notReadable")} · {page.ocr_status}
            </p>
            {page.usable && page.preview ? (
              <p className="mt-1 font-serif leading-6 select-text">{page.preview}</p>
            ) : (
              <p className="mt-1 text-muted">{t("unreadable")}</p>
            )}
          </li>
        ))}
      </ul>
    </Card>
  );
}

function labelFor(t: (path: string) => string, group: string, value: string) {
  const key = `${group}.${value}`;
  const text = t(key);
  return text === key ? value : text;
}

function messageFor(caught: unknown, t: (path: string) => string) {
  if (caught instanceof ApiError) {
    const key = `errors.${caught.code}`;
    const localized = t(key);
    return localized === key ? caught.message : localized;
  }
  return t("errors.generic");
}

function reviewIcon(status: string) {
  if (status === "confirmed" || status === "changed") return "completed";
  if (status === "missing" || status === "expired" || status === "unclear") return "completed_with_unresolved_items";
  if (status === "not_applicable") return "cancelled";
  return "needs_review";
}
