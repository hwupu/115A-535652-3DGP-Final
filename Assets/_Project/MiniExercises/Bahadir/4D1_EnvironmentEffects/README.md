# 4.D.1 Environment effects on character — Bahadir

Issue: #62 · Demo: Nov 16
Scene: `Scenes/4D1_EnvironmentEffects.unity` · Guide: `docs/mini-exercises.md`

## What's new (developed for this exercise)
Everything in this folder is new and written for this exercise. The character is a primitive capsule with two stick legs; no assignment scripts, models or third-party code/assets are used.
- `MomentumController`: Rigidbody character with momentum. The keys accelerate the velocity towards `input * speed` (`MoveTowards`), also in the air (smaller air acceleration). Shift sprints, Space jumps (`v = sqrt(2 g h)`).
- `EnvironmentZone` (base class) and three zones. Each zone gives the controller a speed factor, a grip factor, a wind velocity or a launch:
  - **Ice** (`IceZone`): faster (x1.5) but low grip, so the character slides. Loose blocks slide when pushed (low-friction physics material).
  - **Wind** (`SandWindZone`): constant velocity bias on the character. A rolling bush (`BushSpawner`) is pushed by the wind and respawned when it leaves the area.
  - **Hot plate** (`HotPlateZone`): launches the character whenever it is grounded, so it hops continuously. Magma blocks (`MagmaBlock`) shove it away with a random direction on touch.
- `StickLegs`: code-driven leg animation (no clips). Two stick legs swing +-22.5 degrees in opposite phase, the step frequency follows the speed; both legs go 45 degrees forward while airborne. The visual turns towards the movement.
- `FollowCamera`: simple third-person camera.

## Controls
- **W A S D**: move (relative to the camera) · **Shift**: sprint · **Space**: jump (also steerable in the air)
- **Right mouse button + drag**: orbit the camera

## Requirement checklist
- [x] Controllable character
- [x] >= 3 places that drastically change walking speed or launch the character (ice, wind, hot plate)
- [x] At least one jump place (hot plate)
- [x] Objects placed around those locations (sliding blocks, rolling bush, magma blocks); smooth animation (stick legs). Scenery props: still to add

## Reusable code in Core
- None yet. The scripts live in `Scripts/` of this exercise. `EnvironmentZone` and `MomentumController` are candidates to move to Core for the game's wind and ice effects (#45, #23).

## TA modification points
- `MomentumController`: `walkSpeed`, `sprintSpeed`, `groundAcceleration`, `airAcceleration` (air control), `jumpHeight`
- `IceZone`: `gripFactor` (slipperiness), `speedFactor`
- `SandWindZone`: `windVelocity` (direction and strength), `bodyAcceleration` (bush)
- `HotPlateZone`: `launchSpeed` (jump height)
- `MagmaBlock`: `shoveSpeed`, `upKick`, `directionJitter`
- `StickLegs`: `swingAngle`, `jumpAngle`, `strideLength` (step frequency)
- `BushSpawner`: `respawnDelay`

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Third-party code: none (if used: >= 3 new features, a loop creating >= 20 objects, links here)
