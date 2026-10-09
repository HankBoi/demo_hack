# Roadmap and Phase Gates

The build has six phases. Claude must implement one phase, run its targeted tests, write the report, and stop. The user reviews the phase before authorizing the next one.

| Phase | Scope | Test boundary |
|---|---|---|
| 00 — Foundation | App skeleton, analysis shell, sample-state UX, setup instructions. | Shell smoke tests only. |
| 01 — Document intake | Tender PDF upload, validation, page-aware text extraction, OCR fallback state, persisted job status. | Intake and parsing tests only. |
| 02 — AI requirements | Extract structured requirements and source page/quote evidence. | Labeled extraction sample and citation unit/integration tests only. |
| 03 — Evidence matching | Upload supplier pack, suggest evidence matches, deterministic date/duplicate checks, uncertainty. | Matching and rules tests only. |
| 04 — Human review and export | Review/edit/confirm workflow and CSV export. | Review/export and MVP demo flow tests. Do not run the final red-team suite yet. |
| 05 — Final system audit | No new product features; fix findings from the complete project-wide review. | Full regression, hallucinations, bugs, user errors, safety, accessibility, and demo review. |

## Gate for Phases 00–04

A phase is complete when:

- every acceptance item in its phase file has PASS/FAIL/BLOCKED/NOT RUN;
- required phase-local checks have run;
- failures are fixed and relevant checks rerun;
- exact commands and evidence are in the phase report;
- no false success or test claim is shown;
- a reviewer can run the phase demo path;
- the report identifies remaining work.

At a phase gate, Claude must stop and wait. User approval to continue is required.

## Phase 05 exit gate

The final phase is complete only when:

- the whole user journey has been tested from upload through export;
- the labeled AI evaluation is reported with method and sample size;
- adversarial and error cases are documented;
- critical and high-severity defects are fixed or clearly reported as release blockers;
- the full test suite and manual review results are recorded;
- known limitations, model/data/dependencies, and actual measured costs are disclosed;
- a final two-minute demo script and setup instructions are usable.

Do not declare the project production-ready solely because hackathon checks pass.
