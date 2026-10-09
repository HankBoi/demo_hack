# Phase 00 — Foundation and Honest Demo Shell

## Goal

Create a runnable application foundation that makes the intended workflow and product boundaries clear, without pretending the AI analysis already works.

## In scope

- Set up the agreed frontend/backend structure, package managers, lockfiles, environment example, lint/type-check commands, and local run instructions.
- Create a clean responsive shell with the product name, Azerbaijani default UI copy, concise product explanation, upload entry point in a disabled/not-yet-available state, and visible “demo data / AI suggestions require review” boundary.
- Provide a clearly labeled sample report preview populated with static synthetic content, including an example tender requirement, source page/quote, evidence candidate, and “human review required” state.
- Add health/status route and user-safe empty/loading/error design tokens or components needed by later phases.
- Add contribution/setup notes and a placeholder implementation log. Record deviations from the stack doc.

## Out of scope

Real upload, parsing, OCR, model calls, persistence of user documents, authentication, portal integration, export, final broad AI tests.

## Acceptance criteria

- A teammate can follow the documented commands to start frontend and backend locally from a clean checkout.
- Main route and health route return successfully; environment secrets are not needed for this phase.
- The sample preview is visibly synthetic/static and never implies it came from a real analysis.
- No control offers a false success action; future upload entry is visibly unavailable until Phase 01.
- Responsive layout can be inspected at desktop and narrow mobile widths.
- Lint/type checks relevant to the scaffold pass, or exact blockers are recorded.
- `.env.example` has placeholders only and no secret values.

## Phase-local checks

- Fresh setup using documented commands.
- Frontend build/start smoke check and backend health-route smoke check.
- Lint/type-check if configured.
- Manually inspect the shell at desktop and narrow mobile width.

Do not run product-wide regression, AI hallucination, red-team, security, accessibility, or user-error audits. Those are Phase 05 work.

## Gate artifacts

- Running app and setup instructions.
- Phase report with actual commands and results.
- List of deviations and known limitations.
