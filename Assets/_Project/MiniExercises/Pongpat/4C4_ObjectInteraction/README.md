# 4.C.4 Character interaction with objects — Pongpat

Issue: #66 · Demo: Nov 16
Scene: `Scenes/4C4_ObjectInteraction.unity` · Guide: `docs/mini-exercises.md`

## What's new (developed for this exercise)
- Independent Rigidbody implementation: a heavy blue block character transfers momentum to lighter orange cubes through Unity physics. It does not use Henry's `RigidbodyPusher`, `VanishAfterHit`, `NonOverlapPlacement`, or exercise scripts.
- A 120-cube arena: 100 surrounding cubes and a dense 20-cube pack directly north of the character. Only the Editor setup loop creates objects; Play mode reuses the same pool.
- First-impact countdown, random placement with clearance, bounded retries when crowded, and reset without duplicating bodies.
- An on-screen counter counts distinct pool obstacles in actual obstacle-to-obstacle contacts during the last two seconds. It excludes the character, ground and walls; the peak stays visible until reset. This measures a recent contact window, not simultaneous contact pairs.
- No assignment scripts/models or third-party code/assets were used. Existing assembly/scaffold/Magic infrastructure is reused and is not claimed as newly developed technique code.

## In Unity
1. Open the project in Unity **6000.3.26f1**. If needed, activate an Editor license through Unity Hub first. Let script compilation/import finish.
2. Click **Magic > Mini-Exercises > Pongpat 4.C.4: Set Up Scene**. The save dialog refers to your currently open scene; cancel to stop setup, or save/continue to open this exercise's scene. The tool authors the missing objects and materials and saves the exercise scene. Rerunning preserves existing work and repairs missing references.
3. Check Console for `[Magic] Pongpat 4.C.4 ready: 120 obstacles`, no errors, and the `Pongpat Object Interaction` root with `Character`, `Obstacles` (120 children), `Boundary`, and `Interaction Overview Camera`. This standalone scene does not need to be added to the game's Build Settings.
4. Press Play and focus the Game view. Check the HUD total is **120**. With recycling OFF, hold **Shift + W** into the dense pack. Confirm cubes move and the mutual-contact peak reaches **10 or more**. Press R to repeat.
5. Press **V** to enable recycling, then hit fresh cubes. Confirm they vanish after two seconds and return at random clear positions. `active + waiting` stays 120; a full spawn area leaves bodies waiting and retries instead of overlapping them. R restores the original layout while keeping the selected recycling mode.
6. Run **Window > General > Test Runner > PlayMode**, filtering `PongpatObjectInteractionTests`. After stopping Play, rerun setup once to check that the count and hand edits remain intact. Save the scene. Report completion so generated scene/material/`.meta` files and `Editor.log` can be verified before commit.

If Henry's setup was accidentally run in this scene, **Magic > Mini-Exercises > Pongpat 4.C.4: Remove Henry Demo Objects** removes its old Hamster/Arena/HUD roots and camera follow component, with Undo support. It requires this scene to be open outside Play mode and preserves Pongpat's arena. The normal setup command never performs this removal.

## Controls
| Input | Action |
|---|---|
| WASD / arrow keys | Move on world X/Z (W/up is north on the overview) |
| Either Shift | Sprint; transfers more momentum into the dense pack |
| R | Restore character/cubes to their authored positions; clear statistics |
| V | Toggle two-second recycling (initially OFF); clear previous hit timers |

## Requirement checklist
Pongpat confirmed the Test Runner and manual scene checks on 2026-11-04:
- [x] Standalone scene generated and runs in real time with no Console errors
- [x] >= 100 obstacles around a mouse/keyboard-controlled character (setup creates 120)
- [x] Obstacles are pushed away on contact (dynamic character mass 30 kg; cubes 1 kg)
- [x] Situations where >= 10 obstacles collide with each other (20-cube pack; actual-contact HUD)
- [x] Extra: obstacles vanish after 2 s and the same number respawn randomly without overlapping
- [x] Generic techniques are in `Project3DGP.Core`; glue is in `Project3DGP.MiniEx.Pongpat`, with no Game or Henry exercise dependency
- [x] Controls, new work, TA modification points, and references documented

## Reusable code in Core
- `Movement/PlanarBodyMotor`: caller supplies planar input and sprint; accelerates a dynamic Rigidbody while preserving vertical motion. Reads no keys and assumes no scene types.
- `Interaction/ImpactRecycleBody`: ignores static scenery, reports dynamic-body impacts, and optionally deactivates once two seconds after the first qualifying impact. Later hits do not restart the timer. The pool owner listens to `Recycled`, clears velocity, calls `ResetImpact`, and reactivates it. Turning recycling off pauses its timer; the demo's V key resets timers for a fresh demonstration.
- `Spawning/RandomBoxPlacement`: samples upright box positions inside an inset rectangle, tests all solid colliders (including the character and rotated cubes), and returns false when no clear candidate is found. The caller must synchronize transforms between placements. The spawn centre must be above the floor by at least the box half-height plus clearance.

Reuse target: [#26 lake puzzle](https://github.com/hwupu/115A-535652-3DGP-Final/issues/26), for physical props reacting to the ball and safe prop recycling. Game integration is separate; these classes contain no golf types. Export this exercise folder, the Pongpat member asmdef, and its Core dependencies for E3.

## TA modification points
- Character Inspector: motor walk/sprint speed and acceleration; Rigidbody mass. Explain acceleration toward a target X/Z velocity and why Y is preserved.
- Obstacle Inspector: Rigidbody mass, `ImpactRecycleBody` delay/impulse threshold. Explain why ground/wall impacts do not start the timer and why the first hit owns the countdown.
- Demo root Inspector: spawn rectangle, cube half extents, clearance, attempts per retry, retry interval, and contact window. The half extents must match the cubes; change spawn height if changing cube height/clearance. Respawns use upright cubes.
- Setup source: `ObstacleCount` and `InitialPosition` control the Editor-authored layout. Rerunning only adds missing content; change existing objects in the Editor rather than expecting the tool to reset them.
- `TryRespawns`: explain deactivation/pooling, clearing both velocities, `Physics.SyncTransforms`, and retaining a pending object instead of forcing a blocked spawn.

## Validation status
- Static C# compilation of the new Core components, demo glue, Magic tool and test assembly passed against installed Unity 6000.3.26f1 assemblies (using the bundled Input System/Test Runner references).
- All seven `PongpatObjectInteractionTests` passed on 2026-11-04, as recorded in Unity's `TestResults.xml` (the full PlayMode run passed 63/63). They cover the physical >=10-cube cascade, static-contact exclusion, two-second recycle/reset, blocked/clear/sequential placement, and demo pool lifecycle.
- Pongpat confirmed the generated demo scene works correctly in Play mode on 2026-11-04, including the requirement checks above. The saved scene, materials, script references and generated metadata were inspected before commit; Editor.log contains no C# compiler errors.
- The scene and three primitive materials are generated through the Magic tool and included with their Unity-generated `.meta` files. The initial license blocker from 2026-11-02 is resolved.
- After Pongpat authorized cleanup, Henry's accidentally added demo objects were removed through Unity. The saved scene was rechecked: 120 unique cube references, resolving script/material GUIDs, and no Henry, Game or Asset Store dependencies.

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Project guide: `docs/mini-exercises.md`; ADR-0010 (independent work), ADR-0011 (Editor-authored setup)
- Third-party code/assets: none
