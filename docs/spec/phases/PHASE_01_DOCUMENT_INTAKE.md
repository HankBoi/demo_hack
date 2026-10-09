# Phase 01 — Document Intake and Page-Aware Text

## Goal

Let a user upload one tender PDF, validate it, extract page-separated text, and see honest processing status, including unreadable/OCR-limited pages.

## In scope

- Tender PDF upload with strict file count, size, page count, MIME/signature, and parsing validation.
- Server-side opaque filenames and temporary/local storage according to `DATA_SAFETY.md`.
- Page-separated text extraction with pypdf; preserve 1-based original page numbers.
- Persist the analysis job and file metadata using the selected local SQLite approach, including queued/running/complete/failed states and safe error messages.
- Add optional OCR only if the exact engine supports the chosen language and the team can install/run it in time. Otherwise display a truthful “scanned page not readable in this demo” state.
- Show extracted page count and a small text preview; never display a successful parse if no usable text was recovered.
- Add deletion/cleanup for uploaded files and associated records.

## Out of scope

AI requirement extraction, supplier evidence matching, legal conclusions, CSV export, whole-project QA.

## Acceptance criteria

- Valid text PDF is accepted and each extracted text segment maps to the correct original page number.
- Invalid file type, oversized file, corrupt/encrypted PDF, too many pages, empty-text scan, and parser exception produce safe, actionable feedback.
- Temporary files and database rows are associated with a single analysis ID and can be deleted.
- Job status remains truthful across success and failure; no indefinite spinner without timeout/retry guidance.
- OCR disabled/unavailable is clearly identified, and unreadable pages are not silently omitted.
- No user filename/path or raw document content is exposed in public errors or logs.

## Phase-local checks

- Unit/integration tests for valid PDF, page count/page numbering, invalid signature/type, size/page limit, corrupt/encrypted PDF, no-text scan, parser failure, cleanup, and job status.
- Exercise the upload flow manually with the synthetic fixture.
- Retest Phase 00 shell/startup critical path.

Do not run model hallucination tests or project-wide red-team/security/UX audits; reserve them for Phase 05.

## Gate artifacts

- Working upload and page-aware extraction.
- Test fixture metadata and phase report with command output, actual limitations, and OCR configuration.
