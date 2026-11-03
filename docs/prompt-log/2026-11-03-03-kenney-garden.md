# 2026-11-03 03 — Kenney look for L1: block island with a raised border, plateaus, sunken pond (#74)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #74, PR #132

## Prompts (verbatim)
> it is working great. I want to do a litle expriment. can you use kenney_platformer-kit that i have downloaded to create an alternative L1 scene? use the skybox and forest from kenny too. it is CC0.

> i feel kenney is more uniform. lets use kenney instead. delete Handpainted Grass & Ground Textures, Pandazole - Nature Environment Low poly Pack, and Free Stylized Hand-Painted Skybox
>
> also, we create the L1 ground, can you make the outer border higher as barrier? and make some elevations on flat ground also (may need to selectively add mesh collider?). pond shoud be lower elevation. don't place mushroom here. the block-grass-large has rounded corner, so when placed together without overlaps, will create openning at the corners. so overlay 1 unit will fix.

## What the AI did
- Checked the results of Henry's tool runs and committed them, then opened PR #132.
- **Kenney experiment:** unpacked Platformer Kit 4.1, Mini Forest 1.0 and Skyboxes 1.0 (all CC0) and measured them in Blender. One Kenney unit is one block, so they are used at ×2. A tool built a copy of L1 with a block island. Henry preferred it.
- **Switched to Kenney:**
  - Deleted the local Pandazole, Chromisu and BD Studios packs.
  - Removed their tools (terrain, scatter, sky, Pandazole models), the 53 Pandazole prefabs, the material and the terrain layers.
  - Restored L1_Garden, MainMenu, Ending, the Nature prefabs and the L1 terrain data to their state on main.
  - Only the Farm UI theme stays from the store packs (install guide updated to one pack).
- **`KenneyPrefabsMenu`** (*Create Kenney Prefabs*):
  - 23 prefabs, including tall and low blocks.
  - Swaps the Nature/Tree, Bush, Rock and Course/Fence placeholders to Kenney models, so L1's existing props and fences follow.
- **`GardenKenneyLevelMenu`** (*Garden: Build Kenney Level*, run in L1 itself):
  - Ground: 17 × 17 blocks of 4.16 m on a 3.125 m grid, so they overlap about 1 m and the rounded corners leave no holes. Each block has a box collider; the old terrain is switched off.
  - Border: a wall of tall blocks one cell outside the course, 2–4 m high, with colliders.
  - Plateaus: up to five raised areas of 1 or 2 m, each with one box collider, away from the fairway, tee, cup and hazards.
  - Pond: the blocks under the pond sit 1 m lower, and the pond moves down with them.
  - Decor: a dirt trail along the fairway markers; trees, hedges, rocks, barrels and crates along the edge; flowers, grass and stones off the fairway, with **no mushrooms**; a sign at the tee; floating islands around.
  - Look: Kenney day sky, fog and a soft-shadow sun.
- **`KenneySky`:** Kenney day sky for L1 and MainMenu, morning sky for Ending. *Apply Farm UI Theme* uses it behind the menus.

## Decisions / open questions
- Box colliders instead of mesh colliders: the blocks are boxes, and boxes are cheaper and steadier for a rolling ball. If the ball bumps at block seams, the ground can use one merged collider per row.
- "Overlap 1 unit" was read as about 1 m (3.125 m pitch for the 4.16 m blocks). Overlapping by half a block would also close the corners, but with four times as many blocks.
- The ball no longer leaves the course over the edge (the wall stops it); the pond is still out of bounds.
- A ball landing in the dry part of the pond basin stays there: it plays like a sunken pit.
