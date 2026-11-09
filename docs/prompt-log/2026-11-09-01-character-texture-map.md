# 2026-11-09 01 — Character texture map adjustments

- **Who:** Henry + Claude Opus 5.5
- **Related:** branch `feat/character-texture-map` (no issue)

## Prompt (verbatim)
> i want to adjust character texture map in photoshop. create a branch for me

> done. create pr and merge

## What the AI did
- Created the branch and listed the character base color textures.
- After Henry edited them in Photoshop, checked the result: Hamster, Cat and Princess `*_BaseColor.jpg` changed, still 2048x2048, `.meta` files untouched, still tracked by LFS, about 0.5 MB added in total.
- Committed the textures and this log, opened the PR and merged it.

## Decisions / open questions
- The texture edits are Henry's own art decisions, made by hand in Photoshop. The AI did not review how they look in Unity.
- No new assets, so `docs/assets.md` and `CREDITS.md` are unchanged.
