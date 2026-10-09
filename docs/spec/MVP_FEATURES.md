# MVP Feature Catalog

## MVP user journey

Create an analysis → upload one tender pack → extract requirements → upload supporting company documents → review evidence suggestions → correct/confirm status → export checklist.

## Required features

| ID | Feature | User-visible result | Phase |
|---|---|---|---|
| F-01 | Tender upload | Upload PDF and see file/page validation before processing. | 01 |
| F-02 | Text extraction | Digital text is extracted page by page; scanned pages use OCR when enabled. | 01 |
| F-03 | Requirement extraction | Tender requirements become structured checklist rows. | 02 |
| F-04 | Page evidence | Each row links to a source page and supporting quote; invalid references are rejected. | 02 |
| F-05 | Requirement type | Distinguish mandatory, scored/evaluated, informational, and uncertain items. | 02 |
| F-06 | Company-document upload | Attach supporting documents to an analysis. | 03 |
| F-07 | Evidence suggestions | Suggest a document/page that may support a requirement, with evidence and uncertainty. | 03 |
| F-08 | Deterministic checks | Check clear dates, expiry, missing fields, and duplicates where structured data supports it. | 03 |
| F-09 | Human review | User can confirm, edit, reject, or mark a row uncertain. | 04 |
| F-10 | Export | Download a simple checklist with status and source references. | 04 |
| F-11 | Analysis states | Show queued, processing, needs review, completed, and failed states with useful errors. | 01–04 |
| F-12 | Demo dataset | Provide one public tender pack plus fictional company documents and a labeled answer key. | 00–02 |

## Explicitly out of scope

- Scraping or logging into etender.gov.az.
- Assuming a public API or integrating with a procurement portal.
- Auto-submitting bids or signing documents.
- Writing bid responses or inventing company credentials.
- Making legal eligibility decisions, providing legal advice, or predicting tender outcomes.
- Multi-tenant enterprise product, SSO, billing, and full retention administration.
- General-purpose contract analysis outside a tender-response checklist.

## Acceptance rule

A feature is not done because a screen exists. Its input, server-side validation, state changes, persistence where required, error path, and phase test must work. Any demo-only behavior must be visibly labeled.
