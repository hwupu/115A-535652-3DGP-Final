# 0012 — Explore mode: the player walks to the ball
- Status: Accepted (amends the gameplay loop of architecture §3, step 4)
- Date: 2026-11-01
- Deciders: Bahadir (camera owner, #19) with Claude; Henry and Pongpat informed in the PR

## Context
In the agreed loop (architecture §3) the camera always follows the ball and the avatar teleports to the ball after every shot. Bahadir wanted more freedom and a real character-navigation part in the game (it also feeds his mini-exercise 4.A.2, #61, and later the A* path hint and NPC pathing, #34 / #123): walk to the ball, look around with several cameras, then shoot.

## Decision
- Add an **explore mode**: `Explore/PlayerModeController` (state machine Walking / Shooting / BallInFlight / Finished) on top of the existing shot code. Henry's shot sequence (aim, power, cone, clubs) is unchanged and runs in the Shooting state.
- Walking uses a generic Core `CharacterWalker` (CharacterController; WASD, Shift sprint, Space jump; input set by the owner, so AI can drive it later).
- The shot starts automatically within 1.2 m of the resting ball (snap into the stance with `GolferAvatar.TeleportTo`); **Q** leaves it. The player stands still during the flight.
- `CameraRig` (#19) gets explore views besides Aim and Follow: ThirdPerson, FirstPerson, FreeFly, BallView (V cycles them while walking).
- The danger timer keeps running while walking (no change to `DangerManager`).
- Opt-in per level through *Magic > Level > Add Camera Rig and Explore Mode*; `AvatarWiring_TEMP` gets a **Teleport To Ball** switch (default on = classic loop).
- Small additive edits to Henry's input (`GolfControls.inputactions`, `GolfInput`: Move, ExitShot, FlyVertical, Jump, Sprint) and to `GolferAvatarTestWiring` (the switch).

## Consequences
- LevelManager (#27, Pongpat) must keep the same logic: no `TeleportTo` on `CameToRest` and `FaceYaw` only while shooting when the level uses explore mode (architecture §4.7).
- Walking costs time: level owners should check that tee-to-ball distances fit the time budget (#46).
- Level owners run the Magic tool in their own scene and save it.
- Open for later: ball reset key (R is Retry), camera zoom (the wheel switches clubs), camera collision in third person, a path hint to the ball, the walk animation.
