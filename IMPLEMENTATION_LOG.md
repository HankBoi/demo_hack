# Implementation Log

## Project record

- Repository / commit: local demo on `main`. Application commit `2a7ed28`. Reports and the duplicate-file fix are in the following commit.
- Team and roles: one implementation pass. No second reviewer checked the ground-truth labels.
- Dataset version: `2026-10-09.1` in `tests/fixtures/tender-demo/`.
- Model ID / provider: `demo-extractor` / `demo-rules-2026-10-09` when `ANTHROPIC_API_KEY` is empty. The live path uses `CLAUDE_MODEL_ID` (default `claude-sonnet-4-5` in configuration) and prompt version `2026-10-09.1`. That live path was not called in this session.
- OCR engine and version: none. `OCR_ENABLED` is false. Tesseract with an Azerbaijani model was not installed or verified.

## Phase history

| Phase | Date | Commit | Status | Report | User approved next phase? |
|---|---|---|---|---|---|
| 00 — Foundation | 2026-10-09 | 2a7ed28 | PASS | reports/phase-00.md | Implemented in the same session |
| 01 — Intake | 2026-10-09 | 2a7ed28 | PASS | reports/phase-01.md | Implemented in the same session |
| 02 — Requirements | 2026-10-09 | 2a7ed28 | PASS | reports/phase-02.md | Implemented in the same session |
| 03 — Evidence matching | 2026-10-09 | 2a7ed28 | PASS | reports/phase-03.md | Implemented in the same session |
| 04 — Human review/export | 2026-10-09 | 2a7ed28 | PASS | reports/phase-04.md | Implemented in the same session |
| 05 — Final QA | 2026-10-09 | this commit | PASS with disclosed limits | reports/phase-05.md and reports/FINAL_QA_REPORT.md | N/A |

## Disclosures

- Models and model identifiers: measurements below are from the demo extractor on the fictional pack only. They are not a general accuracy rate and not a live Claude result.
- OCR and other AI components: scanned pages stay visible with “no readable text”. They are not marked parsed.
- Data sources: the tender and supplier PDFs were generated in this repo. No official portal was scraped. VÖEN `0000000001` is fictional.
- Libraries: Next.js 16.4, ASP.NET Core on .NET SDK 10.0.401, EF Core SQLite, PdfPig 0.1.16, QuestPDF only in the test project to draw the fixture PDFs, Noto Sans and Noto Serif for Azerbaijani letters.
- External APIs: none were called. Provider cost was not measured.
- Stack departure: the uploaded pack described FastAPI and Python. This build uses C# / ASP.NET Core, as requested, with the same product behavior.
- Unresolved limitations: no authentication, no live-model run, no verified OCR, ground-truth labels were not independently reviewed, and the product is a local demo. It is not production-ready and not hallucination-free.

## Deviations

- PdfPig replaces pypdf. Page numbers stay 1-based.
- The worker is a second `dotnet run -- --worker` process on a random port. It shares the SQLite file. It does not bind port 43124.
- A duplicate hash is stored on the later file only. Files are not merged or deleted.
- Demo-extractor explanations for evidence rows are English sentences, including the required “does not mean the supplier lacks the document” line. Chrome around them is localized.
