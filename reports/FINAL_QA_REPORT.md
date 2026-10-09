# Final Project-Wide QA Report

**Date:** 2026-10-09  
**Revision:** `main`, after `2a7ed28`  
**Overall status:** PASS with disclosed limits

## Complete scenario

Fictional pack version `2026-10-09.1`: `tests/fixtures/tender-demo/tender/uydurma-tender.pdf` (9 pages) and eight synthetic supplier PDFs. Reference date 2026-10-09. Provider `demo-extractor` / `demo-rules-2026-10-09`. One browser analysis was created from the in-app demo button, reviewed, exported, and deleted. A later analysis was left open only to confirm the page list after a reload.

## Full automated regression

| Suite/command | Result | Tests passed/failed | Evidence |
|---|---|---|---|
| Backend | PASS | 16 passed / 0 failed on the full `dotnet test` run | Includes intake, limits, citation, injection, and the fixture journey |
| Fixture metrics | PASS | `extraction_hits 7/7`, `predicted 8`, `quote_verified 8/8` | Detailed logger output |
| UI end-to-end | PASS | Browser journey, not a scripted UI suite | Recording `tender-checklist-review-and-export.mp4` |
| Build/type/lint | PASS | `tsc --noEmit` and `eslint . --max-warnings 0` | Next.js 16.4.0, Node v22.14.0, .NET SDK 10.0.401 |

## AI quality and hallucination audit

| Case | Expected behavior | Actual behavior | Status |
|---|---|---|---|
| Requirement not present | No invented requirement; uncertainty shown | 7 expected rows hit. The extra row is the injection paragraph, class uncertain. | PASS on this pack |
| Wrong/unsupported page quote | Citation rejected or marked invalid | 8/8 predicted quotes verified against page text | PASS on this pack |
| Ambiguous clause | Mark uncertain and request human review | Warranty contradiction on page 9 is class uncertain | PASS |
| Expired company document | Report the date, do not decide eligibility | `expired_date_detected` for 2020-05-01. Text does not say eligible. | PASS |
| Missing company document | “Not found in the uploaded files” | Joint-activity row uses that sentence and adds that this does not mean the supplier lacks the document | PASS |
| Prompt injection in document | Treat as content | Page 8 quote kept. Class is uncertain, not mandatory. | PASS |
| Conflicting warranty sentences | Surface the conflict | One uncertain row quotes the contradictory warranty paragraph | PASS |
| OCR-corrupted / blank scan | Do not treat the page as parsed | `skan-bos.pdf` page 1 stays visible as no readable text. OCR was not run. | PASS for the off state |

These counts are numerator/denominator for this sample. They are not a general accuracy claim. The ground-truth labels were written with the fixture and were not checked by a second reviewer.

## Bugs and user-error review

- Unsupported file: a `.txt` upload returned 400 `unsupported_file_type` with no stack trace.
- Empty-text PDF: the job fails and does not become a successful checklist.
- Encrypted PDF: rejected as encrypted or unreadable. The message does not include a temp path.
- Refresh during processing: the analysis page polls `GET /api/analyses/{id}` and restores status.
- Model timeout, rate limit, and invalid JSON: covered by the provider boundary. Not exercised against a live model.
- Export: CSV download worked after a checklist existed. Cells that start like a formula are prefixed in the fixture test.
- Duplicate upload: the later byte copy is flagged and kept.
- Correction: confirm and edit persist. The AI statement is not overwritten.
- Keyboard: actions are real buttons with visible text. Status uses an icon plus text.
- Narrow screen: at 390px the cards stack and the review buttons remain on the page. A later attempt to drag the window narrower did not change the layout; the device-mode capture is the one that shows 390px.

## Security and data handling

- Secret handling: `.env.example` has names only. The demo runs with no API key.
- Uploaded-file access: files live under `data/uploads`, outside the web root, and are gitignored.
- Deletion: delete removes that analysis. The fixture test checks a second analysis remains.
- Logs: the API exception payload is the error object, not a stack trace. Model payloads are not logged.
- Demo data: organisations and the VÖEN in the fixture are invented.

## Performance and cost

- Pack: 9 tender pages plus 8 supplier files.
- Wall-clock: about 1.2 seconds from tender upload to `needs_review` in the API smoke. The recorded browser path through export and delete was about 1 minute 14 seconds.
- OCR duration: not run.
- Model usage and provider cost: BLOCKED. Claude was not called.
- Hardware/runtime: this cloud VM, .NET SDK 10.0.401, Node v22.14.0.

## Defects

| Severity | Description | Status | Evidence or fix |
|---|---|---|---|
| Critical | None observed in this demo scope | PASS | |
| High | None left open in the local journey | PASS | |
| Medium/low | Both copies of a file could point at each other | Fixed | Later file now stores `duplicate_of_file_id` |
| Medium/low | Page list showed the internal token `not_needed` | Fixed | Fresh load shows “Oxunan mətn” only |
| Low | Demo-extractor evidence sentences are English inside the Azerbaijani UI | Open, disclosed | The required caution sentence is asserted in English by the test |

## Final release decision

- Full user journey passes: YES, on the fictional pack with the demo extractor
- AI evidence checks pass: YES, for the counted cases on this pack
- Critical/high issues open: NO
- Known limitations disclosed: YES
- Final status: PASS with disclosed limits

This is a local evidence checklist. It does not decide compliance, eligibility, or a tender result. It is not production-ready and it is not hallucination-free.
