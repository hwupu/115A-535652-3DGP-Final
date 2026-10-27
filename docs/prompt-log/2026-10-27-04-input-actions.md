# 2026-10-27 04 — Input System actions (#13)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #13

## Prompt (verbatim)
> what ticket should i do next?

> start #13. mouse wheel and 1-3 can switch

Answers to the agent's questions: keyboard + mouse only (no gamepad); add a "Toggle View" action on V.

## What the AI did
- `Settings/Input/GolfControls.inputactions`: maps `Gameplay` (Orbit = right mouse, Look = mouse delta, Shoot = left click, Club1-3 = keys 1-3, ClubScroll = wheel, ToggleView = V) and `System` (Pause = Esc, Retry = R); one Keyboard&Mouse scheme.
- `Scripts/Game/Controls/GolfInput` (ScriptableObject): the only class that touches the actions; plain C# events and values for ShotController, CameraRig and LevelManager. Acquire/Release reference count; `GameplayEnabled` locks gameplay during flight/pause while Esc/R stay on. Wheel: only the sign counts plus a 0.15 s cooldown (Windows notches vs Mac trackpad).
- `GolfInputDebugOverlay` (dev helper) + Magic > Game > Create Golf Input Asset / Open Input Test Scene (unsaved scene).
- First edit-mode tests: `Tests/EditMode/Project3DGP.Game.Tests` with `InputTestFixture` (9 tests); `testables: com.unity.inputsystem` added to the manifest. #14 reuses this test assembly.
- README controls table; architecture §1 row, rule 6, new §4.11 Input contract.

## Decisions / open questions
- PO: mouse wheel and keys 1-3 both switch clubs; keyboard + mouse only; V = toggle view (Bahadir may use it for an overview, #19).
- Agent: wheel up = previous, wheel down = next (macOS "natural scrolling" flips it).
- Contract addition §4.11 affects Bahadir (CameraRig) and Pongpat (LevelManager pause/retry): they review the PR.
