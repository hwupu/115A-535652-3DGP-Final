# 2026-10-26 02 — Level scene skeleton tool (#8), README cover

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #8, #9, PR #93

## Prompt (verbatim)
> i have place a jpeg as cover image for README. Please move the file to suitable folder, then update README.md. You can push and merge it. the, lets do #8 or #9? tell me what to do

> go #8

> my other project did better with hierarchy organization, it has some like this: (Main > Environment/Gameplay/Cameras/Systems tree) can you do better?

> should i do the same for Level 2 and 3 scenes? so that it will be a easier start for other teammates?

## What the AI did
- Cover image: moved to `docs/images/cover.jpg` (LFS), added at the top of the README, and registered in `docs/assets.md` + `CREDITS.md` as AI-generated (PR #93).
- #8:
  - `Editor/Magic/LevelSkeleton.cs`: the slot list.
  - `LevelSkeletonMenu.cs`: *Magic > Level > Create Level Scene Skeleton*. It adds only the missing slots, moves the default camera, light and Global Volume from the scene root into their group, and is a single Undo step.
  - `ValidateLevelSceneMenu.cs`: reports missing slots, a cup without a trigger, no camera or light, and a scene missing from Build Settings.
- Hierarchy reworked after the PO's example:
  - groups `Systems / Cameras / Environment / Gameplay / UI`;
  - no single `Main` root;
  - `Environment` holds static objects only;
  - `Props` is separate from `Course`.
  - Documented in `docs/architecture.md` §4.10.
- Henry ran the tool on L1, L2 and L3 (21 created, 2 moved, Validate OK). The agent checked the scene files.

## Decisions / open questions
- PO: the skeleton is also applied to L2_Desert and L3_Lake. These are untouched empty scenes, so this is setup and an exception to "only the owner edits a scene".
- The slots stay empty until the prefabs exist. Filling them is a follow-up in Sprint 3.
- Issue: Unity didn't recompile after the files changed on disk, so the old tool ran first. The fix was Undo, then Cmd+R.
