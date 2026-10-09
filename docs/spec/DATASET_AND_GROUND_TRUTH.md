# Dataset and Ground Truth Plan

## Purpose

The demo and evaluation need a small, inspectable answer key. A handful of attractive examples can demonstrate the idea, but cannot support broad accuracy claims. Keep the corpus small enough that a teammate can manually verify every label.

## Fixture pack

Create one versioned fixture pack containing:

1. One public procurement/tender document that the team is allowed to redistribute for a demo, or a clearly labeled fictional tender made for the hackathon.
2. A fictional supplier evidence pack with intentionally varied examples: a plausible matching certificate, an expired certificate, a document that is similar but not equivalent, a duplicate, a missing item, a low-quality scan, and an unrelated file.
3. At least one Azerbaijani-language example. If English or Russian samples are added, label the language and do not combine their results into a single unqualified metric.
4. A versioned, manually reviewed answer key. Keep all names, tax identifiers, contact details, signatures, QR codes, and bank details fictional or redacted.

Do not use confidential bids, personal data, or an uploaded user's real documents as a persistent test fixture. Record the source/license and retrieval date for every public sample. If redistribution rights are unclear, use a fictional document instead.

## Ground-truth fields

For each expected requirement, record:

| Field | Meaning |
|---|---|
| `requirement_id` | Stable fixture identifier. |
| `requirement_text` | Manually transcribed concise requirement. |
| `requirement_type` | Eligibility, qualification, technical, delivery, financial, document, or other. |
| `mandatory_or_conditional` | Explicit, conditional, or unclear in the source. |
| `source_file` | Fixture name. |
| `source_page` | Human-verified 1-based page number. |
| `source_quote` | Exact source text supporting the requirement. |
| `expected_evidence` | Evidence kind that could support review; not a legal conclusion. |
| `expected_match_file/page` | Fixture evidence location, or empty when absent. |
| `expected_status` | Match candidate, missing, expired, ambiguous, unreadable, or unrelated. |
| `review_note` | Why the label is difficult or what must not be inferred. |

Also label page-level text extraction/OCR quality and any expected parsing failures. Have a second teammate independently check page and quote labels before using the answer key for metrics.

## Evaluation method

- Freeze and identify the dataset version before evaluation; record the model ID, prompt version, OCR engine/version, and date.
- Compare model results with the answer key using requirement-level precision/recall and exact source-page/quote support rate.
- Report counts as well as percentages. For a tiny test set, show the raw numerator/denominator and examples of false positives/negatives.
- Review every reported quote against the original rendered page. A valid page number alone does not make an unsupported quote correct.
- Separate extraction quality from evidence matching quality. Do not score “legal compliance.”
- If a prompt or model changes, rerun the same frozen pack and record a new result. Do not edit labels to make a run look better without documenting the correction and reason.
- Include one baseline using the team's current manual process on the same tender pack, with the method and elapsed time recorded. This is a small demonstration comparison, not a market-wide benchmark.

## Suggested folder layout in the implementation repository

```text
tests/fixtures/tender-demo/
  README.md                 # sample source/license, version, redaction statement
  tender/                   # public-permitted or fictional tender PDF
  supplier/                 # synthetic supplier PDFs
  ground_truth.json         # labels; no personal data
  expected_failures.json    # unreadable pages and intentionally ambiguous cases
```

Keep this Markdown pack separate from the actual fixture PDFs. Add only the fixture artifacts the team has permission to include in the implementation repository.
