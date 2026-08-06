# Contributing Guide

## Branch Strategy

- `main`: production-ready code only.
- `develop`: integration branch for completed features.
- Feature branches: `feature/<ticket-id>-<short-name>`.
- Bugfix branches: `fix/<ticket-id>-<short-name>`.
- Hotfix branches: `hotfix/<ticket-id>-<short-name>` from `main`.

## Workflow

1. Pull latest `develop`.
2. Create a feature/fix branch from `develop`.
3. Commit with conventional style:
   - `feat: add auth refresh endpoint`
   - `fix: handle invalid reset token`
   - `chore: update ci workflow`
4. Push and open a PR into `develop`.
5. Merge only after all checks and approvals pass.

## Pull Request Rules

- Keep PRs focused and small (prefer under 500 changed lines excluding migrations).
- Link the ticket/issue in PR description.
- Include API contract changes (request/response examples) for endpoint updates.
- Add or update tests for changed behavior.
- No direct push to `main` or `develop`.

## Required Checks

- Build succeeds for `Ticketing.slnx`.
- Lint/analyzers pass.
- Security scan passes (if enabled in CI).

## Review Policy

- Minimum 1 reviewer approval for normal changes.
- Minimum 2 approvals for auth/permissions changes.
- Resolve all review comments before merge.
