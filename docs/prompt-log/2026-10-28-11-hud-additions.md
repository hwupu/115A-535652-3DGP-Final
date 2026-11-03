# 2026-10-28 11 — HUD additions: XP bar, minimap, club cards (#78)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #78, #70, #58, #19, #117, #118

## Prompt (verbatim)
> please move the tickets that are closed to done column, it is getting messy. then start #78

## What the AI did
- Board (Project #2): moved the closed issues that were still in Todo / In Progress to Done: #20, #28, #30, #38, #58, #59, #60, #77.
- Asked Henry for the layout, the XP curve, the minimap behaviour and what the club panel shows (answers below).
- `GameSession`: the XP curve (`xpToReachLevel`, Inspector), `AddXp` (levels up as often as the XP allows), `LeveledUp`, `XpForLevel`, `LevelProgress01`, `MaxLevel`/`IsMaxLevel`. `Xp` is the total of the game.
- `UI/XpBarView`: "Lv N", a bar to the next level with "x / y XP"; the label pulses at a level-up.
- `UI/ClubPanelView`: one card per usable club (`ShotController.AvailableClubs`), the current one highlighted (SelectableCard from 4.E.3) with the CC0 select sound (times the SFX volume); click = pick, only while aiming (`ShotController.CanChangeClub`, now public).
- Minimap: `Core/Cameras/MinimapIcon` (a flat quad that ignores the parent's rotation and scale) and the editor helper `LevelMinimap` (camera following the ball, `UI/Minimap/Minimap.renderTexture`, icons for ball, avatar, cup, exit and hazards; hides the Minimap layer from other cameras).
- Magic tools: *Add HUD and Menus* now also adds the XP bar, club panel and minimap; new *Magic > Level > Add Minimap Icon to Selection*. The UI Test Area has the XP bar (X = +40 XP).
- Play-mode tests: `HudAdditionsTests` (4).
- Architecture §4.8 (HUD table, implementation note) and §4.9 (GameSession contract, XP curve).

## Decisions / open questions
- Henry: minimap upper right, club cards right side centred (XP bar under the strokes, upper left).
- Henry: a simple XP curve now in GameSession (100 / 250 / 450 / 700 total XP for levels 2–5); #70 only awards XP and unlocks clubs.
- Henry: the minimap follows the ball, north up; hazards get icons.
- Henry: the club panel shows only the usable clubs.
- Agent: CameraRig (#19) must not render the Minimap layer (the tool removes it from cameras that exist when it runs).
