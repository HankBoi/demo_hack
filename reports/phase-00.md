# Phase 00 — Completion and Test Report

**Date:** 2026-10-09  
**Phase:** 00 Foundation  
**Commit/revision:** 2a7ed28  
**Status:** PASS

## What changed

The repo has a Next.js app on port 43123 and an ASP.NET Core API on port 43124. The home page is Azerbaijani by default, with an English toggle. It explains the local-demo boundary, starts a check, and shows a static sample card that says it did not come from an analysis. `GET /api/health` returns ok without secrets.

## Feature and acceptance status

| ID | Acceptance condition | Status | Evidence |
|---|---|---|---|
| P00-01 | Documented setup starts both apps | PASS | `README.md`, `scripts/dev.sh`. Health and home both returned 200. |
| P00-02 | Sample card is static | PASS | Home page text “Bu kart analizdən gəlmir”. |
| P00-03 | Secrets not required | PASS | Health check with an empty API key uses the demo extractor. |
| P00-04 | Upload left disabled until Phase 01 | NOT IN PHASE | Later phases were built in the same session, so the start form submits a real PDF. |

## Tests run

| Command/check | Result | Notes |
|---|---|---|
| `curl -sf http://127.0.0.1:43124/api/health` | PASS | `{"status":"ok","service":"tender-evidence-checker"}` |
| `curl -sf -o /dev/null -w "%{http_code}" http://127.0.0.1:43123/` | PASS | 200, Azerbaijani tagline in the HTML |
| `cd web && pnpm exec tsc --noEmit && pnpm exec eslint . --max-warnings 0` | PASS | Next.js 16.4, Node v22.14.0 |

## Manual review

- Main path: desktop home page shows the boundary, the form, and the sample quote.
- Screen size: two columns on a wide window. The English toggle was used in the browser.
- Known rough edges: the browser’s own file dialog is hidden behind a localized button.

## Issues and deviations

Backend is ASP.NET Core rather than FastAPI. See `IMPLEMENTATION_LOG.md`.

## Phase gate

- Required checks passed: YES
- Open release-blocking issue: NO
- Next phase authorized by user: YES, by the request to finish every phase
- Final status: PASS
