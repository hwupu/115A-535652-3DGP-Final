# 2026-11-01 01 — CameraRig and explore mode: walk to the ball (#19)

- **Who:** Bahadir (@bahadir-koksal) + Claude (Opus 5.5)
- **Related:** #19, #125, #27, #61

## Prompt (verbatim)
> make the necessary code updates on the project folder. Edit partially to existing files and create new ones if needed for navigation things. Then tell me what should I do on th eunity window.

(Design agreed earlier in the session: the camera starts on the player in third person; it follows the ball only while it moves; the player walks to the ball instead of teleporting; close to the ball the shot starts by itself (snap into the stance with `TeleportTo`); Q leaves the shot; V cycles Third person → First person → Free fly → Ball view while walking; no walking in free fly / ball view; the shot view is always the aim view; the danger timer keeps running while walking; the player stands still during the flight; free fly can't pass through colliders; fixed camera distance, no zoom, no camera collision for now; no ball reset for now.)

## What the AI did
- New `Core/Movement/CharacterWalker` (CharacterController walker; input set by the owner; sprint (Left Shift) and jump (Space, v = sqrt(2gh)); `IsWalking`, `IsGrounded`, `Speed` for animations).
- New `Game/Cameras/CameraRig` (contract §4.4, extended): views Aim, Follow, ThirdPerson, FirstPerson, FreeFly, BallView; smooth blend; `AimYaw`, `SetView`, `SetAimYaw`, `Shake`, `ViewChanged`. Namespace `Game.Cameras` (a `Game.Camera` namespace would hide `UnityEngine.Camera` in all game code).
- New `Game/Explore/PlayerModeController` (FSM: Walking, Shooting, BallInFlight, Finished).
- Henry's files, small additive edits: `GolfControls.inputactions` + `GolfInput` (Move = WASD, ExitShot = Q, FlyVertical = Space / Left Ctrl, Jump = Space, Sprint = Left Shift); `GolferAvatarTestWiring` gets `teleportToBall` (default on = old behaviour).
- New Magic tool *Magic > Level > Add Camera Rig and Explore Mode*.

## Decisions / open questions
- Changes the gameplay loop of architecture §3 step 4 (teleport → walk): ADR-0012, written into `docs/architecture.md` (§1, §2, §3, §4.4, §4.7, §4.10, §4.11, §6) and the README controls; Henry and Pongpat are told in the PR (LevelManager #27 must keep the same logic: no teleport on `CameToRest`, `FaceYaw` only while shooting).
- Open: camera collision / zoom, ball reset key, path hint to the ball, walk animation.

## Follow-up prompt (verbatim)
> Ok, it worked pretty well. Can you add jumping with space and sprinting with shift button?

- Tested by Bahadir in L1 (scene not saved). Added Jump (Space, only while walking in third / first person; in free fly Space still flies up) and Sprint (Left Shift).

## Follow-up prompts (verbatim)
> Everything works fine. Just the thing is first person camera is too high. Can you lower it? Maybe 1/3 of what it is now

- First-person eye height: 0.3 x model height (was 0.85), set by the Magic tool.

> Ok, that looks nice. Works well. Can you properly add the notes in the achritecutre md and relevant places and push it back properly?

- Docs: architecture.md, ADR-0012, decisions index, README controls. Committed under Bahadir, pushed the branch.

> Ok, do this legs movement. Just lets, no arms. Idk whats implemented already about walking to the gate but yes, it would also work in such case. Please implement it and tell me what I can do on Unity editor for you

- New Core `Movement/ProceduralWalkCycle`: legs swing as pendulums (sin of a phase advanced by distance walked, weight fades in / out), each leg around the top of its mesh; speed measured from movement, so it also plays in `WalkTo` (exit); off when an Animator controller exists. The Magic tool adds it with LegL / LegR. Legs only (no arms), as asked. Reusable for mini-exercise 4.A.2 (#61).
- Tested by Bahadir in L1: works, but the placeholder legs (0.16 m spheres in Henry's Hamster prefab) are too small to see the swing well. Not changed: the prefab is Henry's and his four mini-exercises use it; longer legs are a later topic (or the Blender model, #36).

> I got confirmation from teammates about pushing changes. They said "Make PR then self-merge" ... Do it on behalf of my name.

- Pushed the walk cycle to PR #130 and squash-merged it (team OK to self-merge).
