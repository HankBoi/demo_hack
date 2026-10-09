# Phase 04 — Human Review and CSV Export

## Goal

Complete a usable MVP journey in which a person can inspect, edit, confirm, and export AI-generated evidence suggestions.

## In scope

- Requirements/evidence review screen with source tender page/quote and evidence page/quote access.
- Human editable requirement summary and evidence status, with audit fields for original AI suggestion, user edit, reviewer decision, and timestamps.
- Explicit states for pending, confirmed, changed, not applicable, missing in uploaded pack, and uncertain.
- CSV export of the reviewed checklist with provenance, review status, and clear “AI-assisted draft / human-reviewed” label.
- Delete analysis and uploaded files; prevent export for incomplete/unreadable analysis unless the export marks limitations clearly.
- End-to-end happy path and targeted user error states for this completed flow.
- Prepare a concise demo script using only the fixture pack.

## Out of scope

Native electronic bid forms, portal submission, PDF report generation, organization accounts, multiple tenders at once, Phase 05 broad audit.

## Acceptance criteria

- A user can upload tender, wait for extraction, add supplier evidence, inspect, edit, confirm, and export one checklist without developer intervention.
- AI suggestions remain visibly separate from human decisions after save/reload.
- The CSV opens with correct UTF-8 Azerbaijani characters and stable column headers; CSV cell contents are escaped against spreadsheet formula injection.
- Source provenance is retained in exported rows, and no unsupported claim of compliance appears.
- Errors during save/export/deletion are recoverable and do not silently lose reviewed changes.
- The demo uses synthetic company data and a permitted public or fictional tender sample.

## Phase-local checks

- Unit/integration tests for review state transitions, persistence, CSV encoding/escaping, incomplete-analysis export, and deletion.
- Run the full happy-path demo flow and targeted Phase 04 error paths.
- Retest the previously built upload → extraction → matching critical path.
- Capture manual elapsed time on the chosen pack for both current manual process and prototype; record method and do not overgeneralize.

Do not yet run broad red-team, hallucination, app-wide bug hunt, security review, or accessibility sweep; these are Phase 05 deliverables.

## Gate artifacts

- Working review/edit/export flow.
- Demo script and phase report with end-to-end evidence and manual-baseline method.
