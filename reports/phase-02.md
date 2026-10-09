# Phase 02 — Completion and Test Report

**Date:** 2026-10-09  
**Phase:** 02 Requirement extraction  
**Commit/revision:** 2a7ed28  
**Status:** PASS

## What changed

Requirement extraction goes through `IModelProvider`. With no API key the demo extractor copies quotes from page text. Application code then checks that the file belongs to the analysis, the page is in range, and the quote is a verbatim substring of the normalized page. Invalid citations are not shown as verified. An instruction-like paragraph is kept as uncertain content, not as a mandatory requirement.

## Feature and acceptance status

| ID | Acceptance condition | Status | Evidence |
|---|---|---|---|
| P02-01 | Expected requirements found on the fixture | PASS | `extraction_hits 7/7` |
| P02-02 | Quotes supported by the page | PASS | `quote_verified 8/8` on 8 predicted rows |
| P02-03 | Injection is not a mandatory requirement | PASS | Unit test plus the fixture assertion. The row on page 8 is class `uncertain`. |
| P02-04 | UI names the provider | PASS | Banner: “Demo çıxarışı (canlı model yoxdur).” Provider id `demo-extractor`. |

## Tests run

| Command/check | Result | Notes |
|---|---|---|
| `dotnet test api/TenderEvidenceChecker.slnx --logger "console;verbosity=detailed"` | PASS | Printed `extraction_hits 7/7`, `predicted 8`, `quote_verified 8/8` |

The eighth predicted row is the injection paragraph. It is outside the 7 expected requirements and is not classified mandatory.

## AI/model information

- Provider/model ID: `demo-extractor` / `demo-rules-2026-10-09`
- Prompt/schema version: live prompt `2026-10-09.1` was not executed
- Test sample: `tests/fixtures/tender-demo/` version `2026-10-09.1`
- Measured metrics: 7/7 expected requirement hits, 8 predicted rows, 8/8 quotes verified, on this pack only
- Blocked live checks: no `ANTHROPIC_API_KEY`, so Claude was not called and no token cost was measured

## Issues and deviations

Labels in `ground_truth.json` were written with the fixture. A second person has not reviewed them. Do not read 7/7 as a general accuracy rate.

## Phase gate

- Required checks passed: YES
- Open release-blocking issue: NO
- Next phase authorized by user: YES
- Final status: PASS
