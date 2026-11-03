# 2026-10-28 09 — Main menu and Ending screen (#30)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #30, #20, #44, #27

## Prompt (verbatim)
> ok merge

> disable the continue button first, let pongpat work afterward. proceed

> all green. when i run main menu, click start, it move to L1. both click on menu button and press ESC does not work. when i click menu button, it takes a shot instead.

## What the AI did
- Merged #119 (HUD and level dialogs).
- `UI/MainMenuView`: Start (fresh session, loads L1, no save), Continue (disabled; #44 enables it), Settings (the shared `SettingsPanel`), Quit.
- `UI/EndingView`: this run's results per level and the totals (strokes, time left, score); Main menu, Close.
- `GameSessionData.RunResults`: `RecordResult` adds every finished level; a new game starts empty. Edit-mode test added.
- `Flow/GameScenes`: scene names (MainMenu, L1_Garden, Ending).
- *Magic > Scenes > Add Main Menu UI* and *Add Ending UI* (`MenuScenesUiMenu`, builders in `GameUiBuilder`); `GameSessionMenu.AddToScene` shared.
- Fixes from Henry's test in L1:
  - `ShotController` ignores clicks over UI (the Menu button also took a shot).
  - L1 has no LevelManager yet, so the menu had nothing to pause. `GolferAvatarTestWiring` (TEMP #27, already in the levels) now implements a minimal `ILevelFlow`: pause, Retry (reload), Main menu.
  - Esc belongs to the pause dialog; architecture §4.11 tells LevelManager not to toggle pause on `GolfInput.Pause`.
- Play-mode test for the Ending totals; architecture §4.8 and §4.9 notes.

## Decisions / open questions
- Henry: the Continue button is there but disabled; Pongpat wires it in #44.
- Agent: placeholder title "Hamster Golf" (the game title in the proposal is still blank) and Ending title "The princess is safe!"; both are plain Text in the scene, easy to change.
