# API Contract — Hackathon MVP

This contract keeps the UI and document-processing backend explicit. Route names may be adjusted during implementation, but behavior and validation must remain.

## Analysis resource

Analysis states: queued, processing, needs_review, completed, completed_with_unresolved_items, failed, cancelled.

An analysis stores the tender label, uploaded file metadata, timestamps, processing status, and user-edited requirements.

## Endpoints

| Method and route | Purpose | Main response |
|---|---|---|
| POST /api/analyses | Create an analysis and upload a tender pack. | analysis_id, status, accepted_files, warnings |
| GET /api/analyses/{analysis_id} | Get progress and summary. | status, progress stage, counts, safe error |
| GET /api/analyses/{analysis_id}/requirements | Read extracted rows and source references. | requirements array |
| POST /api/analyses/{analysis_id}/evidence-files | Attach supporting company documents. | file metadata and processing status |
| POST /api/analyses/{analysis_id}/match-evidence | Start evidence matching for the current checklist. | job_id, status |
| PATCH /api/requirements/{requirement_id} | Confirm/edit/reject an AI suggestion or set a human review state. | updated row and audit metadata |
| GET /api/analyses/{analysis_id}/export.csv | Download reviewed checklist. | CSV with evidence/page/status columns |
| DELETE /api/analyses/{analysis_id} | Delete analysis and its files. | deleted status |

## Validation and error responses

Use a stable error shape with code, user_message, retryable, and request_id. Do not return stack traces or model-provider secrets.

Expected error codes include:

- unsupported_file_type
- file_too_large
- page_limit_exceeded
- pdf_encrypted
- pdf_unreadable
- ocr_unavailable
- model_rate_limited
- model_timeout
- model_output_invalid
- citation_validation_failed
- analysis_not_found
- export_failed

## Processing behavior

- Upload creates a persisted analysis before expensive processing starts.
- The frontend polls GET /api/analyses/{id}; refresh must not lose the analysis.
- The worker records progress stage and a terminal status.
- Retrying a failed job must not duplicate requirement rows.
- File and page IDs in model output are validated against server-owned records.

## Demo API simplification

For a single-user local prototype, authentication can be omitted only if the interface is clearly a local demo and never stores confidential user documents. Do not expose the demo server publicly with this shortcut.
