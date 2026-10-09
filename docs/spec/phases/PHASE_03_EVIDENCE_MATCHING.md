# Phase 03 — Supplier Evidence Matching

## Goal

Let a user attach fictional supplier documents and receive cautious candidate matches for human review, with deterministic expiry and duplicate checks.

## In scope

- Upload one or more supplier evidence PDFs under file limits and isolated analysis storage.
- Extract page-aware supplier text using the existing intake service.
- Ask the model for candidate requirement-to-evidence links and brief rationale, with document/page and exact supporting quote references.
- Validate evidence references against real uploaded evidence pages and text; mark unsupported matches unverified.
- Add deterministic checks for explicit dates/expiry, duplicates via file hash, and absent evidence.
- Show statuses such as “possible match,” “not found,” “expired date detected,” “unclear,” and “review required.”
- Give the user a way to mark “not applicable” with a note; do not infer applicability automatically.

## Out of scope

Legal eligibility, tax/regulatory verification, official registry lookup, portal connection, bid drafting, final audit.

## Acceptance criteria

- Every candidate match links to an actual supplier file and page; quotes are verified or marked unverified.
- Missing evidence wording says the uploaded pack did not contain a match, not that the company lacks the document.
- Date checks identify the observed date and basis; ambiguous or locale-sensitive dates require human review.
- Duplicate detection does not merge or delete user files without approval.
- “Expired” is shown only when the source clearly supports the date comparison and a deterministic current/reference date is supplied.
- Unrelated, empty, unreadable, or misleadingly similar documents remain uncertain/missing rather than falsely confirmed.
- Evidence data remains isolated to its analysis and deletable.

## Phase-local checks

- Unit tests for exact file/page references, quote checks, duplicate hashes, date parsing, ambiguous dates, missing/uncertain language, and analysis isolation.
- Integration tests using mocked model responses for false matches and ungrounded references.
- Evaluate the frozen supplier fixture and report match precision/recall, false missing flags, and missed missing evidence with counts.
- Retest upload → requirement list → evidence candidate critical path.

Do not launch the full project-wide hallucination/red-team suite or full user-error hunt before Phase 05.

## Gate artifacts

- Working candidate evidence mapping with uncertainty states.
- Fixture evaluation results and phase report.
