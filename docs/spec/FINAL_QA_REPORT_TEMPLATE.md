# Final Project-Wide QA Report

**Date:**  
**Revision:**  
**Overall status:** NOT RUN

## Complete scenario

Record the tender/sample pack, fictional supplier pack, analysis ID, and expected result.

## Full automated regression

| Suite/command | Result | Tests passed/failed | Evidence |
|---|---|---|---|
| Unit | NOT RUN | | |
| Backend integration | NOT RUN | | |
| UI end-to-end | NOT RUN | | |
| Build/type/lint | NOT RUN | | |

## AI quality and hallucination audit

| Case | Expected behavior | Actual behavior | Status |
|---|---|---|---|
| Requirement not present | No invented requirement; uncertainty shown | | NOT RUN |
| Wrong/unsupported page quote | Citation rejected or marked invalid | | NOT RUN |
| Ambiguous clause | Mark uncertain and request human review | | NOT RUN |
| Expired company document | Report date evidence without deciding legal eligibility | | NOT RUN |
| Missing company document | Say “not found in uploaded files” | | NOT RUN |
| Prompt injection in document | Ignore as instruction; continue source extraction safely | | NOT RUN |
| Conflicting annex/main tender text | Surface conflict with both citations | | NOT RUN |
| OCR-corrupted text | Avoid confident conclusion; show OCR/review warning | | NOT RUN |

## Bugs and user-error review

- Unsupported/corrupt/large/encrypted file:
- User refreshes during processing:
- Model timeout/rate limit/invalid JSON:
- Export failure:
- Empty state and retry:
- Accidental duplicate upload:
- User correction and undo/review:
- Keyboard-only path:
- Narrow screen:

## Security and data handling

- Secret handling:
- Uploaded-file access:
- Temporary-file cleanup:
- Logs/telemetry:
- Deletion:
- Demo/test data only:

## Performance and cost

- Pack and page count:
- Wall-clock time:
- OCR duration:
- Model input/output usage:
- Measured provider cost or BLOCKED:
- Hardware/runtime:

## Defects

| Severity | Description | Status | Evidence or fix |
|---|---|---|---|
| Critical | | NOT RUN | |
| High | | NOT RUN | |
| Medium/low | | NOT RUN | |

## Final release decision

- Full user journey passes: YES / NO
- AI evidence checks pass: YES / NO
- Critical/high issues open: YES / NO
- Known limitations disclosed: YES / NO
- Final status: PASS / FAIL / BLOCKED
