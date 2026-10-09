# User Flow and UX Requirements

## Main flow

1. **Start analysis:** User names the tender and selects the tender document pack.
2. **Validate files:** Show accepted file type, size, page count, password/encryption errors, and OCR-needed pages.
3. **Extract requirements:** Display a progress state; do not label the analysis complete until rows and references are validated.
4. **Review tender checklist:** Each row shows requirement text, type, source page, exact quote, and review state.
5. **Add company evidence:** User uploads candidate certificates, policies, experience records, and other supporting files.
6. **Review suggestions:** Each possible match shows the requirement, suggested file/page, exact text, and “confirm / edit / reject / uncertain”.
7. **Resolve gaps:** Missing, expired, unreadable, and conflicting evidence are separate statuses.
8. **Export:** Download an editable CSV or spreadsheet-friendly file with source references and review status.

## Required statuses

- Queued
- Processing
- Needs review
- Completed
- Completed with unresolved items
- Failed
- Cancelled

For requirement rows:

- Not reviewed
- Evidence suggested
- Confirmed by user
- Missing from uploaded files
- Expired or date conflict
- Unclear / needs specialist review
- Not applicable (user-selected, with note)

Never equate “not found” with “the supplier does not have it”.

## Evidence card

Show at minimum:

- requirement statement;
- requirement type;
- source file name;
- page number;
- exact supporting quote or a clear “no quote found” state;
- possible company evidence file/page;
- AI suggestion label;
- human review status and correction action.

## Error states

- Unsupported file or extension/content mismatch.
- Password-protected/corrupt PDF.
- File or page limit exceeded.
- No readable text found.
- OCR unavailable or failed.
- Model timeout/rate limit.
- Invalid model response.
- Source citation does not resolve.
- Company evidence not found.
- Export unavailable.
- Browser refresh/retry while analysis is processing.

Every error should explain what happened, whether the upload is retained, and the next action. Do not replace an error with an empty checklist.

## Accessibility and language

- Source interface is English for implementation simplicity; core demo labels and extracted tender content must support Azerbaijani Unicode.
- User-visible text belongs in a localization resource, not scattered hard-coded strings.
- Keyboard users can navigate and review all rows.
- Status is conveyed with text/icon as well as color.
- Page citations and quotes must be copyable and readable at narrow screen sizes.
