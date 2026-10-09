# Tender Evidence Checker

A local evidence-checklist assistant for one tender PDF and the supplier documents that might support it. It extracts candidate requirements with a page and a source quote, suggests possible matches, and lets a person confirm, edit, reject, or mark a row uncertain before downloading a CSV.

The default interface is Azerbaijani. English is a toggle in the header. This is not a legal decision, a bid writer, or a connection to any official procurement portal.

## Run locally

Requirements: .NET SDK 10, Node.js, and pnpm.

```bash
cd web && pnpm install && cd ..
./scripts/dev.sh
```

- App: http://127.0.0.1:43123
- API: http://127.0.0.1:43124
- Health: http://127.0.0.1:43124/api/health

`scripts/dev.sh` starts the API, a worker process that polls the job table, and the Next.js dev server. Without the worker, an analysis stays queued.

On the home page, **Use the fictional demo pack** loads the invented Azerbaijani tender. On the analysis page, **Add the fictional supplier documents** attaches the synthetic certificates and contracts. The static sample card on the home page is not the result of an analysis.

## Checks

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
dotnet test api/TenderEvidenceChecker.slnx
cd web && pnpm typecheck && pnpm lint
```

## Configuration

Copy `.env.example` if you want to override defaults. Names only; do not commit secrets. With no `ANTHROPIC_API_KEY`, extraction uses the demo extractor and the screen says so. `OCR_ENABLED` defaults to false. A page with no readable text stays visible and is not treated as parsed.

Uploads and the SQLite database live under `data/`, outside the web root.

## Demo corpus

`tests/fixtures/tender-demo/` holds the fictional tender, synthetic supplier PDFs, and versioned ground truth. The same PDFs are served from `web/public/demo/` so the buttons can load them. Every organisation and identifier in that pack is invented.
