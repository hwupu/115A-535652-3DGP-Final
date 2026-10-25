# 0010 — Mini-exercises as standalone scenes; plan aligned with course rubrics
- Status: Accepted
- Date: 2026-10-25
- Deciders: Product Owner (Henry, on behalf of the team)

## Context
The instructor's handouts (`docs/references/course/`) add grading rules the proposal didn't cover. Each member must do 4 individual mini-exercises, demoed to TAs, and many of them don't match a golf game, which is ball-controlled with no walking character. Form B scores items the proposal lacked: character leveling, monster/NPC skills, per-member particle and sound counts, and more. See `docs/scrum/grading-gap-analysis.md`.

## Decision
1. **AI-native workflow stays**, including for mini-exercises. Each member then **refines and polishes the code by hand**, so they understand it, can modify it live for the TAs, and contribute manual code. Each member does their own 4 exercises.
2. **Mini-exercise assignment follows proposal §12:** Henry 4.A.1, 4.A.3, 4.C.4, 4.E.3; Bahadir 4.A.2, 4.B.1, 4.B.2, 4.D.1; Pongpat 4.C.4, 4.D.3, 4.E.1, 4.E.2. Demo days: **mini-1 and mini-2 on Nov 16, mini-3 and mini-4 on Dec 21**.
3. **Each mini-exercise gets its own self-contained demo scene and folder.** Reusable techniques are written as **generic code** (no golf types) in a shared Core assembly. Assembly definitions enforce that the game and the mini-exercises never depend on each other. Agent rules: `docs/mini-exercises.md`.
4. **Exception to ADR-0005 (no runtime scene building):** inside mini-exercise scenes only, objects may be spawned by a loop at runtime, because the exercises require it ("create ≥ 20/50/100 objects with a loop"). The base scene is still authored in the Editor. Game level scenes are unchanged; large prop counts there are placed with Editor tools.
5. **Rubric gaps become backlog items:** character leveling (start with the standard club, unlock other clubs as the hamster levels up), ≥ 5 NPC skills across levels (non-violent), a non-cat Level 3 guardian (L1 already uses a cat), per-member particles (≥ 5) and sounds (≥ 4), art passes (texture mapping, ≥ 100 objects, lighting, scene effects), misc menu items, HUD additions, FSM documentation, god keys, LOC counting, and report and evidence work. A Form B coverage tracker issue lists our own proposed "Others" items.

## Consequences
- 12 mini-exercise issues (label `area:mini-exercise`), plus new and updated stories. The roadmap snapshot is updated.
- Mini-exercise code by the two members doing 4.C.4 (Henry, Pongpat) must be separate implementations. Shared Core code counts only for its author's exercise.
- Teaser video length (Form A vs instruction) is deferred. The walk-mode idea is not adopted.
