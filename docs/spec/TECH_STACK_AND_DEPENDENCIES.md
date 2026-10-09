# Technology Stack and Dependencies

## Recommended hackathon stack

| Layer | Choice | Purpose |
|---|---|---|
| Web UI | Next.js App Router, React, TypeScript | Upload flow, checklist, evidence review, and export screens. |
| UI styling | Tailwind CSS with shadcn/ui or an equivalent accessible component set | Fast consistent UI; keep accessible labels, focus, and error states. |
| Backend API | Python FastAPI | PDF/OCR processing and model orchestration live in the Python ecosystem. |
| API schemas | Pydantic | Validate requests, job state, extraction output, and exports. |
| PDF text | pypdf | Extract selectable text page by page while retaining page identifiers. |
| OCR | PaddleOCR, optional for scanned pages | Local OCR fallback. Its documentation lists Azerbaijani among supported language codes, but support differs by OCR model version; verify the selected model/version. |
| AI runtime | Anthropic Claude API, called only from the backend | Extract requirements and suggest evidence matches from page-scoped text. |
| AI output shape | Claude Structured Outputs + Pydantic re-validation | Constrain JSON shape, then independently validate evidence and citations in application code. |
| Persistence | SQLite through SQLAlchemy for local hackathon prototype | Store analyses, requirements, review decisions, and job state without adding a hosted database dependency. |
| File storage | Local private uploads directory for the demo; strict cleanup | Keep uploaded files out of public web roots and source control. |
| Test tools | pytest and HTTPX for backend; Playwright for UI end-to-end checks | Cover API behavior and user-visible browser flow. |
| Runtime tooling | uv for Python; pnpm for JavaScript | Reproducible installs and lockfiles. |

## Dependency groups

### Frontend

- next
- react and react-dom
- typescript
- tailwindcss
- zod for client-side form validation where useful
- accessible UI primitives from shadcn/ui or equivalent
- a maintained CSV export utility only if native CSV serialization is insufficient

### Backend

- fastapi
- uvicorn
- pydantic
- python-multipart for uploads
- pypdf
- paddleocr and a compatible paddlepaddle build only when OCR is enabled
- anthropic
- sqlalchemy
- pytest
- httpx

Avoid adding a vector database, Redis, a workflow engine, or a second cloud AI provider to the hackathon MVP. Add them only when measured requirements justify the extra setup.

## Model choice

Use one model provider first, behind a small interface so tests can inject a deterministic fake provider.

- **Suggested initial provider:** Anthropic Claude API.
- **Suggested initial model ID:** claude-sonnet-5-5, subject to a live documentation check when implementation starts.
- Read the model ID from an environment variable. Do not scatter it through code.
- Record provider/model ID and prompt/schema version in the analysis metadata, never the API key.
- Check current pricing and limits before the event; estimate cost from the actual sample pack and measured tokens.
- A mock model is useful for repeatable automated tests, but it is not proof that live AI extraction works.

Optional local inference with Ollama is a later experiment if customer data residency requires it. It is not assumed in the MVP because local compute still has hardware, setup, speed, and maintenance costs.

## Runtime topology

For the hackathon, run:

1. Next.js web app;
2. FastAPI backend;
3. one local Python worker process polling a small persisted analysis-job table;
4. SQLite database and private local file directory.

Do not add a distributed job queue for a single-demo workflow. Ensure the UI can recover job status after refresh. If the app is later deployed for multiple users or larger documents, move jobs to a durable queue and files to private object storage, then use PostgreSQL and authentication.

## Environment configuration

Document a .env.example containing names only, not real values:

- ANTHROPIC_API_KEY
- CLAUDE_MODEL_ID
- DATABASE_URL
- PRIVATE_UPLOAD_DIR
- MAX_UPLOAD_MB
- MAX_TENDER_PAGES
- OCR_ENABLED
- RETENTION_HOURS
- NEXT_PUBLIC_API_BASE_URL

Never commit a real .env file or expose the model key through a NEXT_PUBLIC variable.

## Compatibility and licensing

- Use current stable compatible dependency versions and commit both JavaScript and Python lockfiles.
- Verify licenses for every package and model used in the submission.
- Verify the selected PaddleOCR language/model pairing and test it with actual Azerbaijani text.
- Do not claim scan or handwriting support until it is tested. Handwritten tender forms are outside MVP scope unless the team has reliable examples and tests.

## Official references

- [Next.js App Router](https://nextjs.org/docs/app)
- [FastAPI tutorial](https://fastapi.tiangolo.com/tutorial/)
- [pypdf documentation](https://pypdf.readthedocs.io/en/stable/)
- [PaddleOCR OCR pipeline and language/model support](https://github.com/PaddlePaddle/PaddleOCR/blob/main/docs/version3.x/pipeline_usage/OCR.md)
- [Claude PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support)
- [Claude structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Claude model overview](https://platform.claude.com/docs/en/models/overview)
- [pytest getting started](https://docs.pytest.org/en/stable/getting-started.html)
- [Claude Code project instructions](https://code.claude.com/docs/en/memory)
- [Claude Code custom subagents](https://code.claude.com/docs/en/sub-agents)
