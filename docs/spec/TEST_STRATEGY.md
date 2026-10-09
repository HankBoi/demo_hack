# Test Strategy and Quality Evidence

## Phase-local testing rule

At the end of each phase, test only that phase's acceptance criteria and the previously completed critical path. Record exact commands, actual results, failures, and artifacts. Stop for user review before the next phase.

The full product-wide test, broad adversarial AI review, hallucination audit, regression run, and user-error hunt are reserved for Phase 05. Do not label earlier phase checks as the final audit.

## Reproducible test data

Create a small test pack from public tender documents plus fictional supplier documents. Store a manually labeled answer key with:

- requirement text;
- class/type;
- source file and page;
- exact supporting quote;
- expected evidence type;
- expected company evidence file/page or expected missing/uncertain state.

Do not store personal or confidential data in fixtures.

## Metrics to report

- Requirement extraction precision and recall against the labeled set.
- Page reference validity.
- Exact quote support rate.
- Evidence-match precision and recall.
- False missing flags and missed missing-evidence cases.
- OCR failure/uncertain-page rate.
- Human review corrections per analysis.
- Median elapsed time using the current manual process and the prototype on the same pack.
- Model/API cost for the actual measured sample, if available.

Report sample size and method. Do not invent results or extrapolate one sample to all tenders.

## Checks by phase

- **Phase 00:** App starts; main shell routes work; no fake success states.
- **Phase 01:** Upload validation, PDF page extraction, OCR fallback state, cleanup, and file errors.
- **Phase 02:** Structured requirement output, page/quote validation, duplicates, missing fields, and model failure.
- **Phase 03:** Evidence matching, date checks, human review states, and “not found” wording.
- **Phase 04:** Editing, persistence, CSV export, and phase-specific end-to-end flow.
- **Phase 05 only:** Full end-to-end regression, AI hallucination and prompt-injection review, bug search, security/access review, UX error-path review, accessibility pass, performance/cost smoke, and final demo rehearsal.

## Status vocabulary

- PASS: ran and evidence meets the acceptance condition.
- FAIL: ran and did not meet it.
- BLOCKED: could not run due to a named missing service, secret, data permission, or environment.
- NOT RUN: not attempted.
- NOT IN PHASE: explicitly outside the active phase.

BLOCKED, NOT RUN, and NOT IN PHASE are never PASS.
