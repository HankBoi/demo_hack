# Phase 05 — Final Whole-Project QA

## Goal

Perform the only complete system-wide audit after Phases 00–04 are implemented and reviewed. Find and fix AI hallucinations, software bugs, misleading outcomes, and errors users may encounter. Add no new product features unless a defect fix requires a small change.

## Entry conditions

- Phases 00–04 have reports and user review; all required critical-path functions are implemented.
- Frozen fixture and ground truth are versioned.
- Team has a runnable, documented environment and can identify the exact model/OCR versions.
- If any entry condition is unmet, record it as blocked and test only what is runnable; do not claim a complete audit.

## Final QA workstreams

### A. Whole-product behavior and regression

- Fresh setup and documented startup.
- Complete upload → extraction → requirement checklist → evidence matching → human review/edit → save/reload → export → delete journey.
- All feature-level and integration tests, plus regression checks for prior phase acceptance criteria.
- Test empty, partial, long, corrupted, encrypted, scanned, multilingual, duplicate, and mixed-quality packs within declared limits.

### B. AI hallucination and grounding audit

- Evaluate every requirement and evidence mapping in the frozen ground-truth pack.
- Inspect every displayed source page and quote on the original page.
- Search for invented requirements, unsupported mandatory/conditional labels, made-up dates, wrong pages, paraphrases presented as quotes, nonexistent evidence, false evidence matches, and unjustified “missing” or “expired” conclusions.
- Test ambiguous wording and conflicting clauses; require explicit uncertainty and human review.
- Test malicious or instruction-like text embedded in tender/supplier documents; it must be treated as document content and not change system behavior.
- Test empty/unsupported model responses, malformed structured output, truncated results, timeout, rate limits, provider errors, and partial page failures.
- Report each issue with input, observed output, expected safe behavior, severity, reproducibility, and fix status.

### C. Bug and user error hunt

- Walk the interface as a first-time supplier, including keyboard-only use and narrow viewport.
- Exercise wrong file, oversize file, too many pages, cancellation, duplicate uploads, session reload, lost connection, double-click, retry, deletion, export before completion, and recoverable server/model failures.
- Verify messages say what happened and a clear next action; do not expose stack traces, secrets, or internal paths.
- Review whether a user could confuse AI suggestion with a confirmed fact or legal verdict.
- Confirm destructive actions are clear and do not silently delete another analysis.

### D. Security, privacy, accessibility, and feasibility

- Verify server-only secrets, upload constraints, filename handling, per-analysis isolation, deletion behavior, log hygiene, safe CSV escaping, and no portal automation.
- Basic accessibility review: labels, focus order, keyboard controls, contrast, status announcements, and error association.
- Record actual end-to-end latency, model tokens/cost if available, OCR use, local machine requirements, and dependency versions.
- Confirm no confidential/personal data appears in fixtures, logs, screenshots, video, or export.

### E. Demo readiness

- Rehearse from a clean start using the exact submitted build.
- Capture a <=2 minute recording showing a real successful flow and one meaningful uncertainty/failure case.
- Verify public/demo link and source/export access; include setup fallback if link fails.
- Ensure pitch claims match measured test evidence and disclose model/data/components.

## Severity and disposition

- **Critical:** data exposure/cross-analysis access, fabricated evidence presented as confirmed, destructive loss, or misleading legal verdict. Fix before demo or clearly mark the project blocked from demo.
- **High:** core journey fails, source citation routinely wrong, or export loses review/provenance. Fix and rerun affected regression checks.
- **Medium:** recoverable flow defect or confusing state that can mislead some users. Fix if feasible; otherwise disclose and show safe workaround.
- **Low:** cosmetic issue without material effect. Record and decide based on remaining hackathon time.

Never close a finding as fixed without retesting it. Distinguish fixed, accepted limitation, and unresolved blocker.

## Required final outputs

- Completed `FINAL_QA_REPORT_TEMPLATE.md` with scope, environment, model/data versions, all commands, outcomes, metrics and denominators, severity-ranked findings, fixes/retest evidence, unresolved limitations, and final go/no-go demo assessment.
- Final feature/test matrix with PASS/FAIL/BLOCKED/NOT RUN.
- Reproducible setup instructions and measured model/cost notes where available.
- A <=2-minute demo script/recording and accurate submission disclosures.

## Exit gate

The final QA phase ends only when the entire report is complete and the user-facing result is accurately described. Do not call the product production-ready based on this hackathon audit. Do not invent metrics or claim the AI is hallucination-free; report the tested sample and remaining uncertainty.
