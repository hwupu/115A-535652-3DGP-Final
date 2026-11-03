# 2026-10-28 08 — HUD and level dialogs against the interfaces (#20)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #20, #77, #30, #7, #18, #27, #44, #117, #118

## Prompt (verbatim)
> merge. note in their tickets (even create new tickets for them). /compact

> proceed

> all green. i don't have cat-face sprites yet. commit the changed in L2 and L3 together.

> ah, i see it is in #117 and #118. revert the L2 L3 files.

## What the AI did
- After merging #116, opened **#117** (L2, Bahadir) and **#118** (L3, Pongpat): make the level playable with the Magic tools (skeleton, ball/shot/avatar, level exit). Commented on #27, #38 and #19.
- Step 2, first part: the HUD and the level dialogs, built against the interfaces from #115:
  - `UI/HudView`: time bar (time left) + stage icon (one sprite slot per `DangerState` for Henry's cat faces; a coloured square until then), stroke count, menu button. The bar grows briefly when a shot costs time.
  - `UI/LevelMenus`: pause dialog (Resume, Retry level, Settings, Save, Main menu, Quit), save confirm (Overwrite, Cancel), time-up dialog after the fail scene (Retry, Main menu, Close). Esc opens and closes them.
  - `UI/SettingsPanel`: master, music and SFX sliders in `GameSession.Data.Settings`.
  - `UI/GameQuit`, `Flow/LevelFlowFinder` (finds LevelManager in the scene, so no re-wiring later).
  - `GameSession.SaveStore` (the `ISaveStore` on the prefab, #44) and `GameSession.ApplySettings` (master volume).
- Stand-ins for trying it: `UI/UiTestLevelFlow` (fake level, timer and strokes; S = shot, F = time up) and `UI/UiTestSaveStore` (in-memory slot).
- Magic tools: *Magic > Game > Open UI Test Area* and *Magic > Level > Add HUD and Menus* (every level; shared builder `GameUiBuilder`).
- Play-mode tests: `GameUiTests` (7).
- Architecture §4.8 implementation note.
- *Add HUD and Menus* ran in L1 (saved). L2 and L3 changes (HUD plus the #117/#118 setup) were committed, then reverted at Henry's request: the owners do the setup in #117/#118 and also run *Add HUD and Menus* there.

## Decisions / open questions
- Henry: no cat-face sprites yet; the stage icon shows coloured squares until they are set in `HudView > Stage Sprites`.
- Agent: the main menu and the Ending screen come in a second PR (#30).
- Agent: the level dialogs need LevelManager (#27); until it is in a level, the HUD timer and strokes are hidden there and the menu does nothing. The UI Test Area shows the full flow.
