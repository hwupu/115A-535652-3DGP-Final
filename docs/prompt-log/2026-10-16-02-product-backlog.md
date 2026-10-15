# 2026-10-16 02 — Product backlog from the proposal

- **Who:** Henry (@hwupu) + Claude (Sonnet 5.5, Claude Code)
- **Related:** ADR-0008, `docs/scrum/roadmap.md`, PR for branch `feature/proposal`

## Prompt (verbatim)
> I have checkout to new branch. Added our first draft of [@proposal.md](docs/references/proposal.md). I have setup the default repo to this repo on GitHub Project (our sprint board), so future issues will be added automatically. FYI, I'm Henry (吳普軒) and github id is hwupu; Baha (柯昊瀚) is bahadir-koksal; and, Pongpat (余蓬勃) is empongpat. Please setup the product backlog.

## What the AI did
- Read the proposal; created 9 sprint milestones (Sprint 1..9) with due dates taken from the proposal's milestone table (Sprint 0 due Oct 19).
- Created 49 user-story issues (labels type/level/area, assignees per the proposal's level ownership, acceptance criteria) and added them to Project #2 with Status = Backlog (auto-add did not fire for API-created issues, so they were added explicitly).
- Closed #2 (concept decided). Added ADR-0008, `docs/scrum/roadmap.md`, `sprint-01.md` draft, README/CLAUDE.md updates.

## Assumptions to confirm
- Sprint due date = the course date listed per week. Owners/sprint placement are provisional. Club set default: standard, iron, sledgehammer.
