// Empty means "same host as this page". Next forwards /api to the API process.
// A browser on another machine must not call 127.0.0.1:43124 directly.
export const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "";

export class ApiError extends Error {
  code: string;
  retryable: boolean;
  status: number;

  constructor(code: string, message: string, retryable: boolean, status: number) {
    super(message);
    this.code = code;
    this.retryable = retryable;
    this.status = status;
  }
}

export type StoredFile = {
  file_id: string;
  original_name: string;
  role: string;
  size_bytes: number;
  page_count: number;
  sha256: string;
  status: string;
  duplicate_of_file_id?: string | null;
  unreadable_pages?: number[];
};

export type Analysis = {
  analysis_id: string;
  label: string;
  status: string;
  progress_stage: string;
  provider_id?: string | null;
  model_id?: string | null;
  prompt_version?: string | null;
  stale: boolean;
  counts: {
    files: number;
    pages: number;
    usable_pages: number;
    unreadable_pages: number;
    requirements: number;
  };
  error?: { code: string; user_message: string; retryable: boolean } | null;
  warnings: string[];
  files: StoredFile[];
};

export type EvidenceMatch = {
  evidence_file_id?: string | null;
  evidence_file_name?: string | null;
  page_number?: number | null;
  quote?: string | null;
  quote_verified: boolean;
  explanation?: string | null;
  date_observation?: string | null;
  status: string;
  needs_human_review: boolean;
  review_reason?: string | null;
};

export type Requirement = {
  requirement_id: string;
  statement: string;
  edited_statement?: string | null;
  requirement_class: string;
  mandatory_label: string;
  source_file_id?: string | null;
  source_file_name?: string | null;
  page_number?: number | null;
  quote?: string | null;
  quote_verified: boolean;
  needs_human_review: boolean;
  review_reason?: string | null;
  review_status: string;
  reviewer_decision?: string | null;
  human_note?: string | null;
  reviewed_at?: string | null;
  evidence: EvidenceMatch[];
};

export type PagePreview = {
  file_id: string;
  file_name: string;
  role: string;
  page_number: number;
  usable: boolean;
  extraction_method: string;
  ocr_status: string;
  preview: string;
};

async function readError(response: Response): Promise<ApiError> {
  try {
    const body = (await response.json()) as {
      code?: string;
      user_message?: string;
      retryable?: boolean;
    };
    return new ApiError(
      body.code ?? "generic",
      body.user_message ?? "Request failed",
      Boolean(body.retryable),
      response.status,
    );
  } catch {
    return new ApiError("generic", "Request failed", false, response.status);
  }
}

async function send<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: init?.body instanceof FormData ? init.headers : { ...init?.headers },
  });
  if (!response.ok) {
    throw await readError(response);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  const text = await response.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

export function getHealth() {
  return send<{ status: string }>("/api/health");
}

export function createAnalysis(label: string, file: File) {
  const body = new FormData();
  body.set("label", label);
  body.set("file", file);
  return send<{ analysis_id: string; status: string }>("/api/analyses", {
    method: "POST",
    body,
  });
}

export function getAnalysis(id: string) {
  return send<Analysis>(`/api/analyses/${id}`);
}

export function getRequirements(id: string) {
  return send<{ requirements: Requirement[] }>(`/api/analyses/${id}/requirements`);
}

export function getPages(id: string) {
  return send<{ pages: PagePreview[] }>(`/api/analyses/${id}/pages`);
}

export function addEvidence(id: string, files: File[]) {
  const body = new FormData();
  for (const file of files) {
    body.append("files", file);
  }
  return send<{ files: StoredFile[]; processing_status: string }>(
    `/api/analyses/${id}/evidence-files`,
    { method: "POST", body },
  );
}

export function matchEvidence(id: string) {
  return send<{ job_id: string; status: string }>(`/api/analyses/${id}/match-evidence`, {
    method: "POST",
  });
}

export function retryAnalysis(id: string) {
  return send<{ analysis_id: string; status: string }>(`/api/analyses/${id}/retry`, {
    method: "POST",
  });
}

export function cancelAnalysis(id: string) {
  return send<{ analysis_id: string; status: string }>(`/api/analyses/${id}/cancel`, {
    method: "POST",
  });
}

export function patchRequirement(
  id: string,
  decision: string,
  statement?: string,
  note?: string,
) {
  return send<Requirement>(`/api/requirements/${id}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      decision,
      statement: statement ?? null,
      note: note ?? null,
    }),
  });
}

export async function downloadChecklist(id: string) {
  const response = await fetch(`${API_BASE}/api/analyses/${id}/export.csv`);
  if (!response.ok) {
    throw await readError(response);
  }
  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = "tender-checklist.csv";
  link.click();
  URL.revokeObjectURL(url);
}

export function deleteAnalysis(id: string) {
  return send<{ status: string }>(`/api/analyses/${id}`, { method: "DELETE" });
}

export const DEMO_TENDER = "/demo/uydurma-tender.pdf";

export const DEMO_EVIDENCE = [
  "vergi-shehadetname.pdf",
  "tecrube-muqavile.pdf",
  "kataloq.pdf",
  "elaqesiz.pdf",
  "keyfiyyet-shehadetname.pdf",
  "qeyri-muayyen-tarix.pdf",
  "skan-bos.pdf",
  "vergi-shehadetname-surati.pdf",
];

export async function filesFromDemo(paths: string[]) {
  const files: File[] = [];
  for (const path of paths) {
    const response = await fetch(path);
    if (!response.ok) {
      throw new ApiError("generic", path, false, response.status);
    }
    const blob = await response.blob();
    const name = path.split("/").pop() ?? "document.pdf";
    files.push(new File([blob], name, { type: "application/pdf" }));
  }
  return files;
}
