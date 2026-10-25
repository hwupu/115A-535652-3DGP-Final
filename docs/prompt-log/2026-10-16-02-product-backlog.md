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

## Follow-up prompt (verbatim)
> lets just keep the board simple. Just ToDo, In Progress, and Done. And, sorry I  forgot to remove the build Windows/Mac parts. We will just play in Editor for Demo, so remove all the related tickets/tasks

### What the AI did
- Board Status options changed to Todo / In Progress / Done; all 53 items re-set (open -> Todo, closed -> Done).
- Closed #53 (packaged builds) as not planned; rewrote #29, #48, #49, #52 for Editor Play mode; added #56 (final demo rehearsal from a clean clone).
- ADR-0009, process/roadmap/milestone text updated. `docs/references/proposal.md` was left untouched (team-owned) and still mentions packaged builds.
