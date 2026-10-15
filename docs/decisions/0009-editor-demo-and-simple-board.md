# 0009 — Demos run in the Unity Editor; three-column board
- Status: Accepted
- Date: 2026-10-16
- Deciders: Product Owner (Henry, Bahadir, Pongpat)

## Context
The proposal and first backlog assumed packaged Windows/Mac builds for Demo 1/2 and the final. The team only needs to demo by playing in the Editor. The five-column board was also more than three people need.

## Decision
- No packaged builds: all demos are played in the Unity Editor (Play mode). Build/packaging tickets are removed; remaining demo/QA/perf tickets target Editor Play mode. Build-related git-ignore rules stay as a safeguard.
- Project board columns: **Todo, In Progress, Done** (supersedes the columns in ADR-0007).

## Consequences
The demo machine needs the pinned Unity version, Git LFS and any local-only Asset Store packs installed (see README/assets policy). Editor-only conveniences are acceptable, but scenes must still open without console errors.
