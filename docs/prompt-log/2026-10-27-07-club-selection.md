# 2026-10-27 07 — Club profiles and selection (#16)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #16, #70

## Prompt (verbatim)
> yes, go ahead for #16. test area is fine for now.

> all green

(Answer to the agent's question: all 3 clubs switchable in the test area for now; unlocking is #70.)

## What the AI did
- Magic > Game > Create Club Profiles now also creates `Sledgehammer.asset` and `Mysterious.asset` (`ClubProfilesMenu.AllPaths` = slot order).
- `ShotController`: keys 1–3 (`GolfInput.ClubSlotSelected`) select a slot, the wheel (`ClubCycled`) steps with wrap-around; only while Aiming and InputEnabled; empty slots ignored.
- Shot Test Area gets all 3 clubs (loaded after NewScene); the power meter label shows the club name.
- Tests: edit-mode `ClubProfileEffectsTests` (range, accuracy, control, noise, valid ranges on the real assets); play-mode club switching tests (keys, wheel wrap, no change in flight).
- Architecture §4.2: shipped default values.
- Henry: all green (EditMode 28, PlayMode 11).

## Decisions / open questions
- PO (2026-10-26): 3 tools — Standard, Sledgehammer, Mysterious (codename). Ticket's "iron" is replaced by Mysterious.
- PO: all clubs switchable in the test area for now; locking/unlocking is #70.
- Agent defaults (Inspector-tunable, #46): see architecture §4.2. Sledgehammer marker faster (harder), Mysterious slower (easier).
