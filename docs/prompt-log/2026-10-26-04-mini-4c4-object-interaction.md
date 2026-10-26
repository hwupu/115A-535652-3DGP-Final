# 2026-10-26 04 — Mini-exercise 4.C.4 Object interaction (#57)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #57, #22

## Prompt (verbatim)
> lets start #57

> i don't want it to respawn randomly, instead, respawn back to initial position. please comment out the random function, not remove. increase time to 5 seconds

Answers to the agent's questions: character = walking hamster (CharacterController); collisions = "Domino line, 10 lines, and 10 obstacles per line"; extra credit = yes, as a toggle.

## What the AI did
- Core (generic): `Interaction/RigidbodyPusher` + `IPushReceiver`, `Interaction/VanishAfterHit`, `Spawning/NonOverlapPlacement`.
- Demo (`MiniEx.Henry`, `4C4_ObjectInteraction/Scripts`): `HamsterWalker`, `FollowCamera`, `DominoArena` (10 × 10 domino fan, respawn, R/V keys), `ArenaHud` (counts for the TA).
- Magic tool *Mini-Exercises > Henry 4.C.4: Set Up Scene*: Domino prefab and materials in the exercise folder, Hamster, walled Arena, camera follow, HUD (only what is missing, one Undo step).
- Exercise README: controls, checklist, reusable code, TA modification points.
- `docs/mini-exercises.md`: removed a duplicated "Scaffolding" section.
- Respawn changed to the home spot (random version commented out), delay 5 s.
- Added explanatory comments to all scripts (talking points for the TA demo; code unchanged).

## Decisions / open questions
- PO: walking hamster; 10 domino lines × 10 tiles = the 100 obstacles; extra credit included (V toggles it).
- PO: respawn back at the starting spot (when free), not at random; the random version stays commented out in `DominoArena.TryRespawn`. Vanish delay 5 s.
- Note: the spec's extra says 2 s and random respawn. Henry may need to explain the change to the TA, or set the delay to 2 / uncomment random for the demo.
- Henry refines the code by hand before the Nov 16 demo.
