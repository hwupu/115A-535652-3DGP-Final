# 2026-10-28 01 — Level 1: ball, shot and avatar in the level (#95, first half)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #95, #19, #27, #7, #18, #20

## Prompt (verbatim)
> i haven't do #80 yet. i have a question, is it possible to bring everything we had work on into level 1? or we are still missing something?

> start the first half of #95

## What the AI did
- Listed what can go into L1 now (Henry's Sprint 2 work) and what's still missing (CameraRig #19, LevelManager #27, Cup/strokes #18, Danger #7, HUD #20), with a stand-in for each.
- Added *Magic > Level > Add Ball, Shot and Avatar* (`LevelPlayerSlotsMenu`), which works on the open level scene and adds only what's missing:
  - `ShotController` on `Systems/ShotController` (3 clubs);
  - `GolfBall` on the tee, plus `GolferAvatar` and `AimCone`, under `Gameplay/Player`;
  - `UI/HUD` with the power meter;
  - `TEMP(#19)` `OrbitFollowCamera` on the Main Camera, with wheel zoom off because the wheel switches clubs;
  - `TEMP(#27)` `Systems/AvatarWiring_TEMP`.
- Updated the Pond prefab: added `InvalidZone` and raised the trigger to 0.5 m above the water. The old trigger ended at the water surface, so a rolling ball never touched it.
- `ShotTestSceneMenu.BuildPowerMeter` is now shared, and it returns the canvas.
- Added a note to architecture §4.10.

## Decisions / open questions
- Henry: do the first half of #95 now. The second half comes when #19, #27, #7 and #18 are merged.
- Agent: all 3 clubs for testing; unlocking comes from #70 and #89. Each owner runs the tool in their own scene. Only L1 here.
