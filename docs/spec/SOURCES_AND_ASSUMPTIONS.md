# Sources and Assumptions

**Research checked:** 2026-10-09. Software versions, model availability, pricing, legal requirements, and API capabilities may change. Recheck them during the hackathon before implementation or demo claims.

## Product and market framing

- Azerbaijan's competition authority describes the unified public procurement portal as publishing procurement announcements, results, and contracts: [State procurement activity](https://www.competition.gov.az/en/page/haqqimizda/fealiyyet-istiqametleri/dovlet-satinalmalari).
- Existing products already cover AI-assisted tender requirements and response workflows, including [BidPilot](https://www.bidpilothq.com/tools) and [Synthrics](https://www.synthrics.com/). Therefore “AI for tenders” alone is not a defensible originality claim. The proposed wedge is source-grounded review of Azerbaijani tender packs and supplier evidence, which still needs interviews and validation.
- Use the official [revised procurement law](https://admin.etender.gov.az/email/7-Law_revised) as a source for background only. The MVP must not encode legal interpretations as definitive eligibility decisions; obtain qualified review before any such feature is considered.

## Technology references

- Anthropic documents PDF input including page-aware visual interpretation and limitations: [PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support).
- Anthropic structured output can constrain response shape; application validation and source verification are still required: [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs).
- Available Claude models and identifiers change: [Models overview](https://platform.claude.com/docs/en/models/overview). Keep the model ID configurable and verify it immediately before building. `claude-sonnet-5-5` is only a research-time candidate, not a hardcoded guarantee.
- PaddleOCR's documentation lists supported languages per model/version; verify Azerbaijani support for the exact selected pipeline: [PaddleOCR OCR usage](https://github.com/PaddlePaddle/PaddleOCR/blob/main/docs/version3.x/pipeline_usage/OCR.md).
- Framework references: [Next.js App Router](https://nextjs.org/docs/app), [FastAPI tutorial](https://fastapi.tiangolo.com/tutorial/), [pypdf](https://pypdf.readthedocs.io/en/stable/), and [pytest](https://docs.pytest.org/en/stable/getting-started.html).
- Claude Code reads project instructions from `CLAUDE.md` and supports project-scoped custom subagents under `.claude/agents/`: [Memory and CLAUDE.md](https://code.claude.com/docs/en/memory), [Subagents](https://code.claude.com/docs/en/sub-agents).

## Explicit assumptions to validate

1. Small local suppliers experience material time loss assembling and cross-checking tender evidence. Validate with short interviews; do not claim a measured market-wide saving.
2. The first demo can use one document pack and synthetic company evidence rather than an official portal integration.
3. The selected model can process the chosen sample size and languages within the team's time and budget. Measure actual token use/cost and latency.
4. Text extraction plus optional OCR is adequate for the chosen examples. The app must expose unreadable pages instead of hiding uncertainty.
5. “Tender Evidence Checker” is a working descriptive label only. Check trademark/domain availability separately before presenting it as a cleared global brand.

## Not established by research

- No official portal API or permission to automate portal interactions has been established here.
- No product-market fit, willingness to pay, legal accuracy, operational accuracy, or production readiness is established.
- No accuracy, time saving, or cost figure is established until the team runs and reports its own reproducible evaluation.
