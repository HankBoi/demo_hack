# Phase 02 — AI Tender Requirement Extraction

## Goal

Extract candidate requirements from the page text and show each candidate with a verifiable original page and supporting quote.

## In scope

- Add the server-side Claude API provider using a configurable model ID, bounded input size, timeout, and clear unavailable/rate-limit errors.
- Use a structured response schema and validate again in the backend (Pydantic or equivalent).
- Split large documents into bounded page groups while preserving file/page identity; avoid losing page labels when combining results.
- Extract requirement text, type, mandatory/conditional/unclear label, exact quote, page number, and uncertainty/review note.
- Deterministically verify that cited pages exist and the exact quote is present in that page's extracted text. Reject or mark unverified output rather than fabricating a citation.
- De-duplicate near-identical candidates without deleting source provenance.
- Show progress, partial failure, retry behavior, and “review required.”
- Add prompt instructions to treat document text as untrusted data and ignore instructions found inside documents.

## Out of scope

Matching requirements to supplier documents, legal interpretation, declaring compliance, autonomous bid generation, final red-team audit.

## Acceptance criteria

- Every displayed requirement has a valid source file/page and quote verified against that page, or is explicitly marked unverified and not presented as supported fact.
- Schema-invalid output, unknown labels, invalid page numbers, unsupported quotes, empty output, timeout, model rejection, and rate limit become safe visible states.
- API key remains server-side; prompts/responses do not leak into logs by default.
- Model identity is configurable and visible in project disclosures.
- A frozen labeled sample can be evaluated and raw counts for extraction, page validity, and quote support are reported honestly.
- UI language distinguishes extracted text from AI interpretation and requires human review.

## Phase-local checks

- Unit tests for schema validation, page bounds, quote matching, grouping/page provenance, and deduplication.
- Integration tests with deterministic mocked model responses, including malformed, conflicting, and failed responses.
- Run the small labeled extraction fixture and report precision/recall, page validity, and exact quote support with denominators.
- Retest upload-to-extracted-text critical path.

Do not run the final broad hallucination/prompt-injection suite or whole-app regression; those are reserved for Phase 05. Direct citation-validation tests for this feature are required now.

## Gate artifacts

- Working source-grounded requirement list.
- Frozen prompt/schema/model record and dataset version.
- Phase report with measured sample results, failures, and limitations.
