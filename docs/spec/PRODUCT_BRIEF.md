# Product Brief — Tender Evidence Checker

## The user and the problem

**Primary user:** A small Azerbaijani supplier preparing to respond to a public-sector tender, often without a dedicated bid-compliance team.

**Problem hypothesis:** The supplier must read a tender pack, turn its requirements into a checklist, find the relevant company certificates and evidence, and notice missing or expired items before submission. This may involve repeated manual reading across PDFs and company folders. This workflow is a hypothesis to validate with local suppliers; public sources confirm that tender information is available, not how much time a supplier currently loses.

## Proposed solution

The user uploads:

1. one tender pack (PDFs and annexes);
2. a small folder of company supporting documents.

The app extracts requirements into a reviewable matrix, cites each requirement to a page and source quotation, suggests possible supporting documents, and flags gaps for a human to confirm.

## One demo scenario

Use an openly available tender document and a fictional supplier pack. Show the app identify:

- a mandatory requirement with its source page and exact supporting quotation;
- a likely matching company document with its page;
- one absent or expired supporting document, clearly marked “Needs human review”.

The user edits or confirms the result and exports a checklist. Do not submit a bid.

## Value proposition

Help a supplier check a tender pack more consistently and spend less time building a manual requirements checklist. Measure the outcome with observed review time, requirement extraction quality, citation validity, and the number of missing items found against a human-prepared answer key.

Do not promise a higher win rate, legal compliance, automatic eligibility, or a fixed percentage of time saved.

## Product limits

- The tender document remains the source of truth.
- AI outputs are suggestions for human review, not legal advice.
- The app must distinguish “not found in uploaded documents” from “does not exist”.
- A missing source page, unreadable scan, ambiguous clause, or conflicting document must remain uncertain.
- Results are specific to the uploaded tender pack; do not silently add requirements from laws or web search.
- No automatic bid drafting, scoring/win prediction, tender submission, or procurement portal scraping in the MVP.

## Why this may fit the hackathon

- **Specific user and outcome:** supplier gets a cited, editable tender evidence checklist.
- **AI has a real role:** interpreting tender wording and finding semantically relevant evidence across documents.
- **Quality is testable:** compare the checklist and source references with a manually labeled answer key and current manual process.
- **Feasible scope:** upload files, process one tender pack, review, export; no procurement-system integration.
- **Originality wedge:** Azerbaijan-language and local document handling with page-level evidence; this is a hypothesis, because general tender-analysis products already exist.

## Validation before building

Before the hackathon, ask a few local suppliers, procurement consultants, or tender specialists:

- How do they turn a tender pack into a checklist today?
- Which supporting documents are most often overlooked or out of date?
- How many pages and annexes are typical?
- What software or templates already solve part of this?
- Can they provide a public or anonymized example and a manually prepared checklist?

If they report that their current system already handles the task well, or no local document pattern creates a distinct need, do not overstate the local differentiation.
