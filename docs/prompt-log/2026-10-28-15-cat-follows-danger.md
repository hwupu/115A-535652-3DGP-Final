# 2026-10-28 15 — The L1 cat follows the danger timer (#31)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #31, #51, #7, #123

## Prompt (verbatim)
> all green. tested. i decided not to add ending story. can remove the codes also.

## What the AI did
- Removed the Ending outro (code and tool step) before merging #128 (#51); the Ending scene was never saved with it.
- #31: the time-up behaviour the PO chose ("the cat just wakes up", keep it simple) already exists from #21 (Awake pose: stands, eyes wide, tail up, stares at the ball; `WokeUp`).
- Added the missing wiring: `GardenCat` finds the level's `ILevelFlow` at Start and follows its danger timer (Normal / Warning / Critical / Failed → SetAlert), so it reacts by itself once DangerManager (#7) is in. Until then the TEMP(#7) F1–F4 keys still drive it.
- Play-mode test: `GardenCatFlowTests` (1).

## Decisions / open questions
- Henry: on time-up the cat just wakes up; animation may come later (#37).
- Open: the Critical search walk needs Bahadir's shared pathing (#123); then `GardenCat` calls it in Searching and the last shot must stay reachable.
