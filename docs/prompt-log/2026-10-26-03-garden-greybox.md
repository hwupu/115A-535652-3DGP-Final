# 2026-10-26 03 — Garden greybox tools (#9)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #9, #28, #38, #74

## Prompt (verbatim)
> this level should be simple and serve as tutorial. so we may have some floaty (or like dialog at the bottom of canvas). i'm not sure if we need to do some state management. lets use l-shap route. player should be able to see the hole with flag, just like ordinary gulf course (for this level). I plan for two par, first with standard club, then switch to hammer. this is intended as tutorial. please make prefabs for models, so we can replace with real animated models later. allow me to move around in the editor, idk where to place them right now. I think the gate could be "portal" from mini-exercise? that me jump to other level/scenes? idk open condition yet. i think current size is good. how to add hills and pond to the ground? use terrian tool?

> i think I'll scrupt the landscape later. can you do step 7 and 8 for me as temporatly? i want to get the code running first.

> We can close #9 for now. we will come back to touch on artistic later after we get the code running. i have to do mini-1 and mini-2 first before move on to #13 and #14.

## What the AI did
- New Magic tools:
  - *Prefabs > Create Placeholder Prefabs*: 8 prefabs (Cup with flag, Pond, Fence, Gate, Tree, Bush, Rock, Cat). Each has a root with the colliders plus a `Visual` child, so a real model replaces `Visual` later.
  - Placeholder URP materials.
  - *Level > Create Level Terrain*: 50 × 50 m terrain, surface at y = 0 with 2 m of depth below; layers Grass, Fairway, Sand, Snow. It disables the Ground plane; the skeleton slot `Ground` counts as present when a Terrain exists.
  - *Level > Garden: Place Temporary Layout*: an L-shaped route (tee bottom-left, hole + gate top-right), pond, cat, props and fences. It adds only missing objects, by name.
- Validate now accepts any trigger collider on the Cup.
- `docs/architecture.md`: bottom dialog bar in the HUD layout (§4.8); the prefab, `Visual` and terrain conventions (§4.10).
- Decisions recorded on #9, #28 (tutorial: bottom dialog, step list, par 2 = standard club then hammer) and #38 (gate = portal reusing #60; open condition to confirm).
- Henry ran the tools on L1. The agent checked: 31 objects placed, Validate OK, assets ~0.6 MB.

## Decisions / open questions
- PO:
  - L1 is a simple tutorial: L-shaped route, flag visible from the tee, par 2.
  - Hills and the pond use Terrain.
  - Models are replaceable prefabs.
- Terrain sculpting and art are deferred until the code runs (#74).
- Open: tutorial texts and mentor (#28); gate open condition (#38).

## Follow-up: terrain data in LFS
- Found after the merge: Unity always saves `TerrainData` as binary, but `*.asset` is set to `text eol=lf` in `.gitattributes`. A commit from Windows could corrupt the file through line-ending conversion. The committed copy was checked and is intact.
- Added `*TerrainData.asset` to LFS in `.gitattributes` and renormalized the L1 terrain data.
