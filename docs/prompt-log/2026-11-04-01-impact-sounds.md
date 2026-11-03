# 2026-11-04 01 — Kenney Impact Sounds for all levels and the GUI (#72); Kenney style for L2/L3

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #72, #42, #73, #74, #75, #76, PR #132

## Prompts (verbatim)
> i'm happy with it. i have downloaded impact sounds, please use them. pick the sounds for all L1, L2, L3 scenes, and GUI SFX.
>
> Similarly, for the platformerkit... kenney assets. when team build L2 and L3, it is prefer to keep in similar style. however, because L2 is dessert, so probably has to suggest that we duplidate some prefabs, then change its texture (to dessert) ourselve?

## What the AI did
- Checked the results of Henry's Kenney L1 run (830 objects, clean log) and committed them to PR #132.
- **Picked sounds** from `kenney_impact-sounds.zip` (CC0): 21 groups × 5 variants (1 MB) in `Assets/ThirdParty/Kenney/ImpactSounds`.
- **Code (Henry's areas: Effects, UI, Garden):**
  - `SoundCue`: random variant, volume, pitch jitter.
  - `SoundSet`: one asset per level, holding the shared events plus named extras.
  - `GameplaySounds`: plays shot (by swing style), bounce (ground or hard, by impact speed), out of bounds, footsteps, goal, gate, XP, level up and time up through 8 reused 3D voices, scaled by the SFX volume. Exposes `Play` and `PlayExtra` for level code.
  - `GardenSounds`: cat suspicious and cat wake.
  - `BallTriggerEffect` gained an optional sound (pond splash).
  - `UiSounds`: button clicks (a softer sound for back), slider ticks while dragging; plays while paused.
- **Magic > Game > Add Sounds:**
  - Creates `Settings/Audio/Sounds_L1_Garden`, `_L2_Desert` and `_L3_Lake` with the picks (grass, sand, ice).
  - Adds `Systems/Sounds` to levels and `UiSounds` to every canvas.
  - Wires the L1 cat and pond.
- **Docs:**
  - `docs/audio/sound-list.md`: event → sound per level, the report list, and how L2/L3 play their extras.
  - Register and credits rows; architecture §1 (Effects row).
- **Kenney style for L2/L3:** `docs/art/kenney-style.md`. Kenney models share one palette texture, so a level recolours them with one palette and material plus prefab *variants*, not duplicated models. L3 can use the kit's snow blocks and trees as they are.

## Decisions / open questions
- `GameplaySounds` plays only events from Henry's code (and public events such as `LevelGoal.Reached`). The shared audio manager (#42, Bahadir) can take over its playback later. L2/L3 extras are picked, but the owners wire them in their own code.
- The pack has no wind or ambience loops: wind (L2) and birds (L1) still need another source.
- Open (PO): a shared tool for desert and ice palettes and prefab variants, if the team agrees with the style proposal.
