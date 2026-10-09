# NeuroBridge Judging Plan

This plan maps the concept to the scoring card visible in the user's event screenshot. Scores below are planning estimates, not judge predictions.

## Rubric fit

| Criterion | Weight | Proposed proof | Current planning estimate |
|---|---:|---|---:|
| Value for the user | 25 | A supplier receives an editable checklist that points to the exact tender page and surfaces a missing/uncertain evidence item. | 4/5 |
| Prototype and use of AI | 30 | A working upload → extraction → evidence match → human review flow; AI handles semantic extraction/matching, deterministic checks validate pages and dates. | 4/5 |
| Quality testing | 20 | Manually labeled requirements, citation checks, failure documents, and timed manual-versus-assisted comparison. | 5/5 |
| Feasibility | 15 | One pack, bounded pages/file size, upload-only input, no portal integration, human approval. | 4/5 |
| Originality | 10 | Local Azerbaijani tender-document workflow and evidence-linked review, if interviews confirm it differs from existing tools. | 3/5 |

**Rough weighted fit: 82/100.** The biggest uncertainty is originality and verified local user pain.

## First-round pitch card

Keep the published card concrete enough for an AI judge to understand without a live explanation:

- **User:** Small suppliers responding to Azerbaijan public tenders.
- **Problem:** Building and checking the tender evidence checklist across a tender pack and company documents is manual and easy to miss.
- **Solution:** Upload the tender pack and supporting documents; receive an editable checklist with source-page citations and human-reviewed evidence matches.
- **AI contribution:** Extracts tender requirements and finds possible semantic matches; software verifies citations and applies deterministic checks.
- **Outcome:** A reviewable list of found, missing, expired, and uncertain evidence before a person submits a bid.
- **Limit:** Decision support only; the supplier remains responsible for legal and submission decisions.

## Two-minute demo video

1. Introduce the supplier and task in one sentence.
2. Upload the tender pack and fictional supplier documents.
3. Show one extracted requirement with page number and source quote.
4. Show a likely matching document and one missing/expired item.
5. Have the user confirm or correct the suggested match.
6. Export the checklist and show the manual-versus-assisted test result, only if actually measured.

## Submission checklist from the visible rules

- Pitch deck with user, problem, solution, and evidence.
- Working demo link and exact setup steps if judges need them.
- Source repository or project export that judges can inspect.
- Video of the core scenario, no longer than two minutes.
- Model, data, libraries, public templates, and existing components disclosed.
- Quality test results, failures, and comparison with the current manual approach.

The screenshot says research, tool research, and open-data research may happen before the event, but the project itself must be built after the hackathon begins. It also says the first-day 20:00 submission is the frozen competition version, and the deck cannot change afterward. Prepare the problem brief, public source documents, answer key, and test cases beforehand; do not pre-build the project.
