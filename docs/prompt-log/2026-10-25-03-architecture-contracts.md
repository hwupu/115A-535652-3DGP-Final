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

## Follow-up (2026-10-26)
> If I remember correctly, we will have ordinary gulf club, that has mid shooting range and mid accuracy. sledgehammer has high shooting range, but low accuracy. the third tool is undecided, but it should have low shooting range, but high accuracy. There are multiple states during the game, when player is taking a shot, when player pan around with mouse, it should rotate the camera origin at the ball. A degree range angle (projection) should originate from the ball, pointing forward and follow the camera. The width of degree may depends on the tools chosen. The power meter (shooting range) will be placed at the left of canvas. at this moment, we do not mark the ideal range (one-shot) on the meter, but we may consider to add it later. A marker will run up and down. player will left click to take a shot. at the moment marker is pointing is the power player takes. The min and max power range is depended on tool chosen. Ideally, we will have animated character model from Blender, but we don't have it now, so use simple box as representation. Make it easy to replace at later stage. I think one of the mini-exercise is animated character with at least two bones? Character with swappable tools may count? please note in the ticket when we prepare for the character animation, the animation clips that we needed. let use simple GUI, if we have time later, we can replace it with sprites later. FYI, the timer countdown may be in seconds and we will assign different initial values for difference levels/scenes based on its difficulties (manually). Once player take a shot, it will deduct, say 10 seconds, so that we can control the amount of shoots they takes. similar to over par. we should able to set the shoot cost later for each levels/scenes. We will let Unity's physics to do the simulation, I'm not sure if it includes rolling? or it is fake animation? Anyway, lets keep it as simple as possible. I think i own GameSession. do you have enough info? else, ask me for clarification.

Clarifications asked (answers verbatim):
- Pan input: "Hold right mouse + drag" — "once the shot taken, the camera should follow the ball. player will then, teleport to the next shooting position."
- Power meter: "Always while aiming; one click shoots".
- Fail timing: "let the ball roll, and player can take another shot. The level fail critiria is timeout. however, if the ball fall outside of game boundary, or other invalid marking in the scene, the ball will reset its position to its nearest position. Also, it may not be a cup also. it could be any target that ball hits. we may decided it later, use cup for now."

What the AI did:
- Rewrote `docs/architecture.md` with the gameplay loop, clubs table, aim cone, seconds-based timer, `InvalidZone` reset, generic `LevelGoal`, `GolferAvatar` (box placeholder) and `GameSession` (Henry).
- New issues #88 (avatar, Sprint 2) and #89 (GameSession, Sprint 3); updated #7, #13, #14, #15, #16, #18, #19, #20, #36 (animation clip list), #59 (4.A.1 hamster-with-tools idea), #70.
- Interpretation to confirm: the level fails only at timeout, even mid-roll.
