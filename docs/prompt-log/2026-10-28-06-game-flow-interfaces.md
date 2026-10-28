# 2026-10-28 06 — HUD, menus and game flow: decisions and interfaces (#20, #30, #77)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #20, #30, #77, #80, #7, #18, #27, #44, #50

## Prompt (verbatim)
> can we work on HUD, UI, main menu and basic game flow? i prefer to list out all the assets that i needed for #80, including cat animations in #31 and hamster #36

> ask me questions. i can provide interface, then let Pongpat do the implementation.

> for the HUD time, i will use cat sleep, half open eye faces image/icon for stage icon (shape changes at Warning/Critical), so conceptually, there should not be modification to the code. the rest seems good.

## What the AI did
- Posted the full asset list (models, hamster/cat/princess clips) to #80.
- Asked Henry about each screen in three rounds (decisions below).
- Wrote four interfaces, so the UI can be built and tested before Pongpat's code exists:
  - `Danger/IDangerTimer` (+ `DangerState`), implemented by DangerManager (#7);
  - `Scoring/IStrokeCounter`, implemented by ScoreKeeper (#18);
  - `Flow/ILevelFlow` (+ `LevelPhase`), implemented by LevelManager (#27);
  - `Session/ISaveStore` + `SaveGame`, implemented by Save & Load (#44).
- Architecture: ownership rows, `: IDangerTimer` / `: IStrokeCounter` / `: ILevelFlow` on the contracts, `LevelManager.GoToMainMenu`, §4.8 rewritten (HUD, screens and dialogs, saving), §6 decision, §7 menu question closed.

## Decisions / open questions
- Henry: HUD time = bar + stage icon (cat faces, one sprite per stage, his art). No numbers.
- Henry: no result screen in L1/L2 (walk to the next level). L3 → Ending with this run's totals (strokes, time left, score), Main menu and Close.
- Henry: time up = cat wake-up animation, then Retry (same level), Main menu, Close. Close = quit the game.
- Henry: pause dialog = Resume, Retry level, Settings, Main menu, Quit, plus Save.
- Henry: main menu = Start (new game → L1), Continue, Settings, Quit.
- Henry: one save slot, manual from the pause dialog, asks before overwriting; a new game doesn't save; Continue loads it. Agent: Continue restarts the saved level from the tee.
- Henry: Henry writes the interfaces and all UI (the pause dialog moves from #77); Pongpat implements the interfaces.
- Next: Step 2, the UI against these interfaces, tested with fakes.
