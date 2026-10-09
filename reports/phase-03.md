# Phase 03 — Completion and Test Report

**Date:** 2026-10-09  
**Phase:** 03 Evidence matching  
**Commit/revision:** 2a7ed28, with the duplicate-pointer fix in the report commit  
**Status:** PASS

## What changed

Supplier PDFs attach to one analysis. The demo matcher suggests a possible overlap, says when nothing in the upload shares enough wording, and leaves uncertain requirements unmatched. An explicit ISO date next to an expiry phrase, compared with reference date 2026-10-09, can show an expiry observation. An ambiguous numeric date does not. Byte-identical files are flagged and kept.

## Feature and acceptance status

| ID | Acceptance condition | Status | Evidence |
|---|---|---|---|
| P03-01 | Experience contract preferred over the catalogue that says it is not the contract | PASS | Fixture test expects `tecrube-muqavile.pdf`. |
| P03-02 | Quality certificate shows the date, not eligibility | PASS | Status `expired_date_detected`, observation contains `2020-05-01`, and does not contain “eligible”. |
| P03-03 | Missing joint-activity contract uses the cautious sentence | PASS | Status `not_found`. Explanation contains “does not mean the supplier lacks the document.” |
| P03-04 | Ambiguous 03/04/2025 is not an expiry | PASS | Fixture test rejects `expired_date_detected` on `qeyri-muayyen-tarix.pdf`. |
| P03-05 | Duplicate and blank scan | PASS | Later copy gets `duplicate_of_file_id`. `skan-bos.pdf` keeps unreadable page 1. |

## Tests run

| Command/check | Result | Notes |
|---|---|---|
| `dotnet test api/TenderEvidenceChecker.slnx` | PASS | Includes `Fictional_pack_is_grounded_and_cautious` |
| API smoke of the eight supplier PDFs | PASS | Match returned 202. Wall clock from tender upload through match was about 2.2 seconds on this machine. |

## AI/model information

- Provider/model ID: `demo-extractor` / `demo-rules-2026-10-09`
- Measured on this pack only. A byte-identical copy can be the suggested file when it is the first equal overlap. The suggestion is still the same document text.
- Blocked live checks: Claude was not called

## Issues and deviations

The first pairing loop could mark both copies as duplicates of each other. The later file now points at the earlier one. Files are still not merged or deleted.

## Phase gate

- Required checks passed: YES
- Open release-blocking issue: NO
- Next phase authorized by user: YES
- Final status: PASS
