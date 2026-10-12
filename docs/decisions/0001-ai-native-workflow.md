# 0001-ai-native-workflow — AI-native workflow
- Status: Accepted
- Date: 2026-10-12
- Deciders: Product Owner (team) with Claude as scrum master

## Context
The team of 3 uses AI agents throughout. Reasoning and prompts would otherwise be lost.

## Decision
Keep a verbatim prompt log (`docs/prompt-log/`) and ADRs (`docs/decisions/`) in the repo. `CLAUDE.md`/`AGENTS.md` hold standing rules for agents. Agents ask the humans when requirements are unclear.

## Consequences
Small overhead per session; decisions are reviewable in PRs and reproducible for the course report.
