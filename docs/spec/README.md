# Tender Evidence Checker — Claude Build Pack

**Status:** Hackathon product and implementation specification  
**Prepared:** 2026-10-09  
**Working product label:** Tender Evidence Checker (no brand/domain clearance implied)

## Product in one sentence

A small supplier uploads one public tender pack and its own supporting documents; the system builds an editable checklist of tender requirements, points to the exact source page and text, and flags where supporting evidence appears missing or uncertain.

## Read these files first

1. [Product brief](PRODUCT_BRIEF.md)
2. [NeuroBridge judging plan](JUDGING_PLAN.md)
3. [MVP features](MVP_FEATURES.md)
4. [User flow and interface](USER_FLOW_AND_UX.md)
5. [Technology and dependencies](TECH_STACK_AND_DEPENDENCIES.md)
6. [AI extraction pipeline](AI_PIPELINE.md)
7. [Data safety](DATA_SAFETY.md)
8. [API contract](API_CONTRACT.md)
9. [Test strategy](TEST_STRATEGY.md)
10. [Phase roadmap](ROADMAP_AND_PHASE_GATES.md)
11. [Dataset and ground-truth plan](DATASET_AND_GROUND_TRUTH.md)
12. [Brand and pitch direction](BRAND_AND_PITCH.md)
13. [Research sources and assumptions](SOURCES_AND_ASSUMPTIONS.md)
14. [Claude Code instructions](CLAUDE.md)
15. [Claude start prompt](CLAUDE_START_PROMPT.md)
16. [Implementation log template](IMPLEMENTATION_LOG_TEMPLATE.md)
17. [Phase report template](PHASE_REPORT_TEMPLATE.md)
18. [Final QA report template](FINAL_QA_REPORT_TEMPLATE.md)
19. [Project subagents](.claude/agents/)
20. [Individual phase specifications](phases/)

## Build sequence

Implement Phase 00 through Phase 05 in order. At the end of every phase, run that phase's checks, write its report, and stop for the user's review. Do not begin another phase until the user says to continue.

Phase 05 adds no product features. It is reserved for the complete application test: end-to-end regression, AI hallucination and prompt-injection review, bug search, and user-facing error scenarios.

## Boundaries

- The MVP is an evidence checklist assistant, not a legal decision-maker or an automatic bid writer.
- Use uploaded public tender documents and synthetic supplier documents in the hackathon demo.
- Do not scrape or submit to the official procurement portal. No portal API availability is assumed.
- Do not state that a supplier is legally eligible or that a bid will win.
- No test result, accuracy percentage, or cost claim may be invented.

## Research basis

The Azerbaijan competition authority describes the unified public procurement portal as publishing tender announcements, results, and contract information. Existing international products already offer tender requirement extraction and compliance workflows, so the plausible differentiator is narrow: source-grounded review of Azerbaijan-language tender packs and supporting documents, with human approval. This differentiation still needs user validation.

- [Official Azerbaijan procurement information](https://www.competition.gov.az/en/page/haqqimizda/fealiyyet-istiqametleri/dovlet-satinalmalari)
- [Example of existing AI tender compliance software: BidPilot](https://www.bidpilothq.com/tools)
- [Example of existing AI tender response workflow: Synthrics](https://www.synthrics.com/)

See [sources and assumptions](SOURCES_AND_ASSUMPTIONS.md) for technology references, limitations, and claims that still need validation.
