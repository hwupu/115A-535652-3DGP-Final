# 4.C.4 Character interaction with objects — Henry

Issue: #57 · Demo: Nov 16
Scene: `Scenes/4C4_ObjectInteraction.unity` · Guide: `docs/mini-exercises.md`
Setup: **Magic > Mini-Exercises > Henry 4.C.4: Set Up Scene** (creates the hamster, arena, Domino prefab, camera follow and HUD if missing).

A hamster walks around a walled arena. **10 domino lines × 10 tiles = 100 obstacles** stand around it like the spokes of a wheel. Walking into the first tile of a line pushes it, and the whole line topples (10 obstacles colliding with each other). Hit tiles turn orange, vanish 5 s later, and come back to their starting spot in the line once that spot is free.

## What's new (developed for this exercise)
- Pushing Rigidbodies with a CharacterController (`RigidbodyPusher`): force from walking speed and direction, applied at the contact point so tall objects tip over.
- Hit tracking and timed vanish (`VanishAfterHit`) with events for the spawner.
- Respawn at the starting spot only when it is free (`Physics.CheckBox`); blocked tiles wait and retry every frame.
- Random placement without overlap (`NonOverlapPlacement`, rejection sampling with `Physics.CheckBox`): written first, now commented out in `DominoArena.TryRespawn` because respawning at home was chosen.
- Domino-line layout in a fan around the centre, constant obstacle count with respawn at home, and a HUD that counts obstacle-to-obstacle collisions (`DominoArena`, `ArenaHud`).
- Camera-relative walking with jump (`ThirdPersonWalker`) and an orbit follow camera (`OrbitFollowCamera`), moved to Core in #58 so 4.E.3 reuses them.

## Controls
| Key | Action |
|---|---|
| WASD / arrows | Walk (relative to the camera) |
| Shift | Run |
| Space | Jump |
| Right mouse drag | Orbit the camera |
| Scroll | Zoom |
| R | Rebuild the 10 domino lines |
| V | Toggle vanish-after-hit on/off |

## Requirement checklist
- [x] >= 100 obstacles around a mouse/keyboard-controlled character: 10 × 10 dominoes around the hamster (HUD "Obstacles: 100 / 100")
- [x] Obstacles are pushed away on contact: `RigidbodyPusher`
- [x] Situations where >= 10 obstacles collide with each other: each domino line is a 10-tile chain (HUD "Collided with another obstacle")
- [x] Extra: obstacles vanish after a delay (5 s, the spec says 2 s; set `Vanish Delay` to 2 to match) and respawn without overlapping, at their starting spot instead of a random one (random version kept commented out in `TryRespawn`; HUD "Respawned"; toggle with V)

## Reusable code in Core
- `Project3DGP.Core.Interaction.RigidbodyPusher` + `IPushReceiver`: any CharacterController that should push props (golfer bumping garden props, #22).
- `Project3DGP.Core.Interaction.VanishAfterHit`: props that disappear after being hit.
- `Project3DGP.Core.Movement.ThirdPersonWalker`, `Project3DGP.Core.Cameras.OrbitFollowCamera`: walking and camera (shared with 4.E.3).
- `Project3DGP.Core.Spawning.NonOverlapPlacement`: random spawning without overlap (pickups, hazards). Not used by the demo right now.

## TA modification points
| What the TA may ask | Where |
|---|---|
| Number of obstacles | `DominoArena` → `Line Count`, `Dominoes Per Line` (Inspector on **Arena**) |
| Line shape / spacing | `DominoArena.BuildLines()`; `First Domino Radius`, `Spacing` |
| Push strength | `RigidbodyPusher` → `Push Force` (on **Hamster**); formula in `OnControllerColliderHit` |
| Heavier / lighter obstacles | Domino prefab → Rigidbody `Mass`; pusher `Max Mass` |
| Vanish delay, on/off | `DominoArena` → `Vanish Delay`, `Vanish Enabled` (or V in Play mode) |
| Respawn at home vs random | `DominoArena.TryRespawn()`: home check active, random version commented out (uncomment it and the `respawnArea`/`maxTries` fields) |
| Respawn gap | `DominoArena` → `Respawn Padding` |
| Walk / run speed, jump height | `ThirdPersonWalker` on **Hamster** |
| Hit colour | `DominoArena` → `Hit Color` |

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Unity manual: CharacterController, `OnControllerColliderHit`, `Physics.CheckBox`, `Rigidbody.AddForceAtPosition`
- Third-party code: none
