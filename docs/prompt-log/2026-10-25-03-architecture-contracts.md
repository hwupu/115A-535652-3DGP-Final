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

## Follow-up 2 (2026-10-26)
> we haven't decide the thrid tool, lets use codename mysterious for now. timer can pause when ball is in flight, please update draft. Also, please update README, include onboarding for other teammates to pickup current progress, and provide sample prompt they can use for breifing, or pick their next ticket? include other helpful prompts is welcomed. update CLAUDE.md if needed. similar to how i ask you to provide editor tools to run (Magic), and provide step by step guide for manual Unity setup.
>
> FYI, we will do #59 by follow its grading requirment, but it stay with that mini-exercise. for this project, we prefer to use animation clips in blender. and it will be easy to handle in Unity for animation states too. We may need handoff document on clips and timing. I plan to use Blender MCP for assistant, so we can handle it during that ticket.
>
> let me know if there has anything missing, so that we can close this ticket?

What the AI did:
- Architecture: third tool codename **Mysterious**; timer pauses during flight (`DangerManager.OnBallAtRest` resumes); Blender clips for the hamster; new open question on the last shot when its cost reaches 0.
- Issues: #7 (pause), #16 (Mysterious), #36 (Blender clips + Animator, handoff doc `docs/art/hamster-animation-handoff.md`, retitled), #59 (stays standalone), #88. `docs/mini-exercises.md` reuse row for 4.A.1 updated.
- README: "Start here" onboarding (git identity for per-author LOC, Claude Code, reading order, manual Unity steps), "Where we are", Magic menu table, prompt cookbook.
- CLAUDE.md: "How to work with the team" (ownership, Magic tool + "In Unity" steps + verify, compile check, decisions).
- Merged `feat/85-magic-scaffolding` into this branch so README/CLAUDE.md edits don't conflict with PR #86.

## Follow-up 3 (2026-10-26)
> time bar is visible and should be at the upper left of canvas. upper right should be menu button. oncy menu dialog open, game should pause. menu items undecided.
>
> for "Read, in this order (15 min):", i don't think any human will read lengthy documents. it is for AI agents. just provide simple brief on the workflow, what has been setup (which i did in this ticket), where to find the files, how can they get started with their first ticket. thinking it like a onboarding message for new commer.
>
> that says, we are not going to update README frequent, especially for the steps i did, and needed once, such as "Where we are" table and "Magic menu (Editor tools)" table. they are not going to repeat those steps (neither will I too), so no need to repeat. all we need to know is the method (editor tools will be provided for time saving, all we need to do is to follow step by step guide). Focus on future roadmap.
>
> you just prepend the Start here section, but some are repeated in the sections below. please restructure README to make it human readable

What the AI did:
- Architecture: HUD layout table (§4.8: time bar upper left, menu button upper right, power meter left), `LevelManager.SetPaused` / `PausedChanged`; menu items as an open question.
- Issues #20, #77, #27 updated.
- README rewritten for humans: Welcome (how we work, Magic tools + step-by-step guides, what's set up, where things are, first ticket), Roadmap, One-time setup, Daily workflow, Prompts. Removed the status and Magic tables and duplicate sections.
- CLAUDE.md: README is for humans; no one-time steps or status tables there.

## Follow-up 4 (2026-10-26)
> move `Team: Henry 吳普軒 (@hwupu, Garden) · Bahadir Emre Koksal 柯昊瀚 (@bahadir-koksal, Desert) · Pongpat Weesommai 余蓬勃 (@empongpat, Lake)` to CLAUDE.md, AI agent may need to know who and what id we are, but I don't want to show in README, too obvious.
>
> Include sprint 0 and 1, just for record.
>
> `... an IDE (Rider / Visual Studio / VS Code), and optionally the GitHub CLI gh.` change to Agentic IDE, Claude Code, and gh are required.
>
> i'm not sure if step 6 is needed? it should handle automatically? if not, say it is okay to skip if there has none in the list yet.
>
> 7. we mentioned it is required in step 1, so maybe change to auth, for both claude code and gh.
>
> remind teammates to pull/rebase often.
>
> the rest are good.
>
> I'll include little Repository layout, and brief example why our codes live inside _Project
>
> we should have the Assets & credits section, it is good reminder to not commit Unity Asset Store assets, and need to keep track for every assets obtained for credits.
>
> make it fun, please add emoji

What the AI did:
- README: team line removed; Sprint 0/1 in the roadmap; tools all required (agentic IDE, Claude Code, gh); step 6 kept (Asset Store packs are never in git, each member adds them by hand; skip if none listed); step 7 = sign in to gh and Claude Code; pull/rebase reminder; Repository layout with "Why `_Project`?"; Assets & credits section; emoji.
- CLAUDE.md: team table (names, GitHub, level, mini-exercise folder).
