# 2026-10-25 03 — Shared contracts draft (docs/architecture.md)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #6, #7, #14, #15, #16, #18, #19, #27, #43

## Prompt (verbatim)
> let me check #6. I can't find the docs/architecture.md

> i believe camera goes to Bahadir. Score and Danger go to Pongpat. The Ball and Shot is mine.

## What the AI did
- Drafted `docs/architecture.md`: ownership table, rules, public API for GolfBall / ShotController + ClubProfile (Henry), CameraRig (Bahadir), DangerManager + DangerConfig, Cup + ScoreKeeper, LevelManager (Pongpat), shot event-flow diagram, open questions.
- Contracts are written in the doc only; each owner writes their own stub code (LOC per author).
- Added a pointer in CLAUDE.md.

## Decisions / open questions
- Ownership: Ball + Shot = Henry, Camera = Bahadir, Score + Danger = Pongpat (PO). Level flow and cup placed with Pongpat (matches #18, #27).
- Q1 per-club danger cost, Q2 fail timing while ball rolls, Q3 owner of GameSession: in the doc, for review.
