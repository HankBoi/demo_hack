# Claude Code Start Prompt

Paste this after placing the complete `tender-evidence-checker` Markdown pack at the root of the implementation repository. Start only Phase 00. In later sessions, use the short continuation prompt below with the requested phase number.

---

You are implementing the Tender Evidence Checker described in this repository's Markdown pack. First read `CLAUDE.md` and `README.md`, then inspect the repository. Read all product-wide requirements and read `phases/PHASE_00_FOUNDATION.md` only as the active implementation scope.

Build Phase 00 completely. Follow the technology choices and product boundaries in the pack. Do not implement Phase 01 or later features. Run only Phase 00 checks, report their actual outputs, fill in the phase report, and stop for my review. Do not run the final project-wide QA, red-team, hallucination, broad bug, or user-error audit; that work is reserved for Phase 05.

If this repository already contains code, preserve it and explain any necessary adaptation. If a dependency, secret, or external service is missing, build what can be completed without it and record the blocker accurately. Do not invent test results or ask me to make routine technical choices.

---

## Continuation prompt for each next phase

After reviewing the previous phase report, replace `NN` with exactly the next phase number and send:

> Continue with Phase NN only. Read `CLAUDE.md`, the phase roadmap, the completed report from the prior phase, and `phases/PHASE_NN_<NAME>.md`. Inspect the current implementation, complete this phase's acceptance criteria, and run only its defined phase-local checks plus required critical-path checks. Update the phase report with exact commands and actual results. Do not implement the following phase and do not run the final project-wide QA before Phase 05. Stop at the phase gate for my review.

For Phase 05, use the same prompt but explicitly confirm that this is the final phase and follow `phases/PHASE_05_FINAL_QA.md` in full.
