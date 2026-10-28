# 2026-10-28 04 — Ball-hit goals: LevelGoal base + BallHitTarget (#22)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #22, #111, #18, #24

## Prompt (verbatim)
> we can merge #111 then work on #22. #111 is what we have discussed, but forgot to update in proposal.

Answers: L1 goals = "one cup for now"; LevelGoal = "Write LevelGoal per §4.6 (Recommended)".

## What the AI did
- Merged PR #111 by PO decision (contract: several goals per level, cup last).
- `Scoring/LevelGoal` (base class per §4.6):
  - `IsMet`, `Accepting`, `ResetGoal`, `Reached(LevelGoal)`;
  - `MarkMet()` for subclasses;
  - `OnMet`/`OnGoalReset` hooks.
- `Golf/BallHitTarget`: met when the GolfBall touches it (collision or trigger). It shows/hides objects and flips a part (the lever handle).
- Placeholder prefabs `Course/BranchTarget` (bullseye turns green) and `Course/Lever` (handle flips, knob turns green), added in `PlaceholderPrefabsMenu`.
- The Shot Test Area now has a lever and a target on a post.
- Play-mode tests: `BallHitTargetTests` (3).
- Fixed a timing-dependent bug in `GolfBall` (#14), found by Henry's test run (`FallsOffTheEdge_ResetsToLastValidGround`: x = 15.03 > 15). The newest ground sample before rolling off an edge can have its centre past the edge, so the reset could leave the ball half over a cliff. `FindLastValidPosition` now also requires ground directly under the sample (`HasGroundBelow`, a raycast).
- Architecture §4.6 implementation note; #22 renamed and its scope updated.

## Decisions / open questions
- Henry: Level 1 has only the cup for now, so nothing is placed in L1.
- Henry: write `LevelGoal` per §4.6; Pongpat owns it from here (#18).
