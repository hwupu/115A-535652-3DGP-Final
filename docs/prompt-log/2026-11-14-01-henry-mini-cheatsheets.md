# 2026-11-14 01 — Cheat sheets for Henry's mini-exercises

- **Who:** Henry (@hwupu) + Claude Code (Opus 5.5)
- **Related:** mini-exercises #57 (4.C.4), #58 (4.E.3), #59 (4.A.1), #60 (4.A.3); `docs/mini-exercises.md`

## Prompt (verbatim)
> follow similar pattern of 11A-535652-3DGP-PA1 (sits next to this project), creat cheatsheets for MiniExercises/Henry.

## What the AI did
- Read the PA1 cheat sheets (`115A-535652-3DGP-PA1/docs/cheatsheets/01..06`) for structure and tone, and all of Henry's exercise scripts, the Core scripts they use, the READMEs and the course rules.
- Added `docs/cheatsheets/henry/` (outside `Assets/`, so no `.meta` files and not part of the E3 export): `README.md` index + 6 sheets mirroring PA1: 01 C#, 02 Unity lifecycle, 03 architecture, 04 requirements (replaces PA1's rubric sheet, since mini-exercises are graded against the spec), 05 demo prep, 06 TA questions with live-modification snippets.
- Code is referenced by file + method name (no line numbers). Snippets in sheet 6 were written against the current code but **not compiled**; the sheet says so and marks the ones to try first.

## Decisions / open questions
- Flagged for Henry: **overlap with PA1** (RenderTexture minimap, 2 s vanish, `AddForceAtPosition`, `CheckBox` spawning). Sheet 4 §5 lists what to present as new instead.
- Flagged spec gaps to decide before the demos: 4.C.4 vanish delay 5 s vs spec 2 s, home vs random respawn; 4.A.1 C turns instead of sliding along B's y-axis (handout version commented out in `SineMotion`).
