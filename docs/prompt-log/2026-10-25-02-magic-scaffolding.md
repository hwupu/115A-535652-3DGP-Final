# 2026-10-25 02 — Magic menu, game scenes and mini-exercise scaffolding

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #85, #69, #6 (layout part), #8, ADR-0011

## Prompt (verbatim)
> what is my first ticket? I'm thinking that I should setup the project with empty scenes files for menu, setting, level 1, 2, 3, maybe ending. and, 12 mini exercise?

> what is the convention for project organization? _Project? or other name? i like the Editor tool menu called "Magic", like Magic> Create Game Scenes. We can add hierarchy if needed. And you should break up the tool based on function, instead of one giant script that reset evey scene objects for every runs.

> use _Project, use Magic. Show me step by step what to do in Unity (manually)

## What the AI did
- First ticket: #6 (Sprint 1). Proposed no separate Settings scene (settings panel in menu/pause instead) and an Ending scene.
- Created #85 (game scenes + Build Settings); moved #69 to Sprint 1; #85 and #69 In Progress on the board.
- New ADR-0011: top-level **Magic** menu, one file per function in `Assets/_Project/Editor/Magic/`, commands only create what is missing.
- Editor tools: `MagicMenuUtil`, `ProjectFoldersMenu` (moved from `ProjectSetupMenu`, meta kept), `AssembliesMenu`, `GameScenesMenu`, `MiniExerciseCatalog`, `MiniExerciseScaffold`, `NewMiniExerciseWindow`. Compile-checked in Unity batch mode (no errors).
- Updated CLAUDE.md, README, ADR-0005 status, docs/mini-exercises.md, roadmap, sprint-01, issues #6, #8, #69, #84.

## Decisions / open questions
- Keep `Assets/_Project/` (community convention; underscore sorts first).
- The human runs the Magic commands and commits the generated scenes/asmdefs/READMEs.
