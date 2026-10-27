# 2026-10-27 06 — Shot sequence: aim cone + cycling power bar (#15)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #15, #19

## Prompt (verbatim)
> yes, please let Bahadir know it is temporary fallback, and should remove after his implementation in this ticket too.

> i think there is one fail: Click_ShootsAtMarkerPower_ThenBackToAimingAtRest ... Expected: 1 But was: 0

> yes. all green

## What the AI did
- Commented on #15 and #19 (to @bahadir-koksal): aim yaw uses a temporary main-camera fallback in `Golf/TemporaryAimYaw.cs`; delete it and switch to `CameraRig.AimYaw` once #19 is merged.
- `Golf/ClubProfile` (SO, §4.2 fields), `ShotPhase` + `ShotInfo`, `ShotMath` (pure formula), `ShotController` (Aiming/InFlight, PingPongGauge marker, left click → random direction inside the cone → `GolfBall.Hit`, `ShotTaken`; back to Aiming on `CameToRest`; `InputEnabled`), `AimCone` (LineRenderer V at the ball), `UI/PowerMeterView` (vertical BarView, club + speed label).
- Magic > Game > Create Club Profiles (Standard club) and Open Shot Test Area (reuses the ball test green via `GolfBallTestSceneMenu.OpenArea`).
- Tests: edit-mode `ShotMathTests`; play-mode `ShotControllerTests` (InputTestFixture + real ball).
- Test fix: in a `[UnityTest]`, InputTestFixture only queues Press/Release; the event is processed next frame, so the test click now yields a frame after press and release. All green (EditMode 23, PlayMode 8).

## Decisions / open questions
- Henry: temporary aim-yaw fallback until CameraRig; Bahadir is told to remove it after #19.
- Agent: club switching (keys / wheel) and the other two clubs stay in #16; ShotController does not change `GolfInput.GameplayEnabled` (LevelManager owns that), it just ignores clicks while InFlight.
- Standard club defaults: 4–16 m/s, cone 10°, marker cycle 1.6 s, pitch 25°, noise 0.3.
