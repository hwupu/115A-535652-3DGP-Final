# 2026-10-27 08 — Character and tool prefabs; keep mini-exercises working (#88)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #88, #16, #80, #36

## Prompt (verbatim)
> i don't see the prefabs for the clubs

> also, the hamsters. we can barrow from 4.A.1. starting from now, our modifications (that is depended on) should reflect to mini execrises, so that we will not break mini execrises during demo. but, this project is the main. i just want all the prefabs, so that i can start working on 3d models.

## What the AI did
- Explained: `ClubProfile.ToolPrefab` was empty; tool prefabs were planned for #88.
- Magic > Prefabs > Create Placeholder Prefabs now also makes `Characters/Hamster` (hero), `Characters/Princess` (crown, skirt) and `Tools/StandardClub`, `Tools/Sledgehammer`, `Tools/Mysterious` (purple wand + glowing orb). Hamster shapes borrowed from 4.A.1 (copied, the 4.A.1 tool is untouched); right arm as pivots `ShoulderR > HandR > ToolSocket`; Visual x1.5; no collider.
- Magic > Game > Create Club Profiles fills an empty `ToolPrefab` with the matching tool prefab.
- New rule (PO): changes to anything a mini-exercise depends on are reflected in the exercises in the same PR; the game stays the main project → `docs/mini-exercises.md` rule 13, CLAUDE.md, agent memory. Checked: since the mini-exercises merged, shared code only gained additions (RestDetector, a menu constant), so no exercise is affected.

## Decisions / open questions
- PO: borrow the 4.A.1 hamster for the placeholder; all prefabs now so Henry can start the 3D models (#80, #36).
- PO: mini-exercises must keep working; the game is the main project.
- Agent: Mysterious placeholder = purple wand with an orb (look TBD); princess = lighter fur + gold crown + pink skirt.
