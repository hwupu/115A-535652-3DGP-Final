# 0008 — Game concept: hamster fantasy golf (three levels)
- Status: Accepted
- Date: 2026-10-16
- Deciders: Product Owner (Henry, Bahadir, Pongpat)

## Context
Sprint 0 needed a game concept (issue #2). The team drafted `docs/references/proposal.md`.

## Decision
Build a single-player 3D fantasy golf game: a hamster rescues a princess from Sir Grumblepaws across three levels (Sunny Garden, Windy Desert, Frozen Lake). Core loop: choose club, aim yaw, stop a cycling power bar; a hidden time budget fills continuously and jumps per shot (Warning/Critical/Failed). Clubs differ in Power/Control/Accuracy. One owner per level scene (Henry: Garden, Bahadir: Desert, Pongpat: Lake), shared contracts agreed by Week 8. Scope is MVP-first; the proposal is the source of truth for the vision and milestones.

## Consequences
The product backlog (GitHub issues) is derived from the proposal. Changes to scope get a new ADR or an update to the proposal PR. The AI is used per proposal §2: humans validate all merged code.
