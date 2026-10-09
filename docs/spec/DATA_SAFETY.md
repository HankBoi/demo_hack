# Data Safety, Privacy, and Product Boundaries

## Hackathon data rules

- Use publicly available tender files and fictional supplier documents in the demo.
- Do not upload confidential live bid packs, personal data, tax identifiers, or real company certificates without explicit permission.
- Tell the user which files are sent to the configured AI provider.
- Do not use uploaded documents to train or fine-tune a model.
- Keep local uploads outside the public directory and delete them according to the configured short retention window.

## API provider boundary

If using Anthropic API, tender text/OCR text and any included pages are transmitted to that provider for processing. Do not describe that as local-only processing. Before a real customer pilot, review the provider's current data retention and contractual terms and obtain user consent appropriate to the documents.

## Access and secrets

- Keep API keys server-side and read them from environment configuration.
- Never put an API key in a client bundle, error page, screenshot, or report.
- Apply upload limits and validate actual file content, not only the filename extension.
- Use opaque IDs for files and analysis jobs; do not trust a client-supplied storage path.
- Do not expose uploaded files through a public static path.
- Keep raw page text and model payloads out of routine logs.
- Add authentication and per-tenant authorization before a multi-user pilot. The local hackathon build is not a production multi-tenant service.

## Legal/product boundaries

- The app is an administrative checklist aid, not legal advice.
- It does not decide whether a bidder is legally eligible or compliant.
- It does not decide whether a requirement is lawful or current.
- It does not submit bids, sign documents, send messages to procuring entities, or alter the official portal.
- The tender pack itself is the scope and source of truth for an analysis.
- A human reviewer owns final interpretation and submission.

## File lifecycle and safety

- Limit file size, page count, and number of files per analysis.
- Reject encrypted or malformed PDFs safely.
- Use isolated temporary files and clean them up on success, failure, cancellation, and expiry.
- Record whether OCR or AI processing failed on a page.
- Provide deletion for an analysis and its associated files.
- Do not include document contents in analytics telemetry.

## Future pilot controls

Before accepting live supplier documents, add user sign-in, storage encryption, access audit, deletion guarantees, configurable retention, provider/data-processing review, backup policy, and a documented incident contact. Do not claim a security certification that the project has not earned.
