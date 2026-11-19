# 2026-11-19 01 — First-time setup on Windows; tool and ball models from Sketchfab, club icons, HUD layout, game title

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #139, #80 (manual artwork, still open in spirit), #88 (tool placeholders), #14 (golf ball prefab), #78 (HUD additions)

## Prompts (verbatim)
> i just pull the project. please handle first time setup. I think Baha made some changes, what did he made? and my next ticket?

> do we have the models for the golf club and other tools?

> i think it is a good time to get all the models that we will need. lets work on this ticket. I have just installed Blender, can you check if you have access to it?

> (scope: Tools (3); method:) i'll let them from sketchfab

> i'll place under \\192.168.11.2\...\Final\assets. I have downloaded golf-club-and-ball.zip. It has both club and ball, seperate them. and update ball prefab too.

> i now logged in. please check. I have added cardcaptor-sakura-wand and sledgehammer-weapon-free-download. the sakura-wand has fx and card, i don't want, just pick the wand.
>
> "Cardcaptor Sakura wand" (https://skfb.ly/6YvrV) by Akiko.Tomiyoshi is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
> "Sledgehammer Weapon [Free Download]" (https://skfb.ly/6vQU7) by scrampunk is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
> "Golf Club And Ball" (https://skfb.ly/6ARt8) by Yanez Designs is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
>
> after this, fix the GUI where tool picker is located in the right middle of canvas, which will overlay with minimap, please change it back to recommended layout, where tool picker is below minimap (on upper right corner). Update the thumbnails for tools. Update the title of main menu, it should be "Hole in One for Her Heart"

> i have adjusted L1 scene, please make my modification as default for editor script. I added background image Gemini_Generated_Image_4iuxpi4iuxpi4iux.jpeg in assets folder, please use it for MainMenu bg. I have title.png also.

## What the AI did
- Setup: the clone had stopped during checkout (stale `.git/index.lock`, empty index, LFS files missing). Removed the lock, rebuilt the index from HEAD (`git reset`), `git lfs pull` (160 files). Local config: `core.autocrlf false`, UnityYAMLMerge driver. Installed `gh` (winget); Henry signed in. Unity 6000.3.26f1 installed by Henry. Not done: Asset Store pack.
- Summarised Bahadir's PR #138 (mini-exercise 4.D.1) and recommended #74 as the next ticket (#31 waits on #123).
- Audited prefabs still using primitives: tools, course props, GolfBall, XpSeed.
- Blender 5.3 headless, scripts in `Art/Tools/BlenderScripts~/`:
  - `golf_club_and_ball.py`: `Art/Tools/StandardClub/StandardClub.fbx` (2,482 tris; grip at the origin, hanging down to -0.30 m, toe toward Unity +Z, thickened 2.5x) and `Art/Ball/GolfBall.fbx` (960 tris; unit sphere, so the prefab's 0.2 scale and collider stay).
  - `tool_models.py`: Sledgehammer (392 tris, thickened 1.8x) and Mysterious = the Sakura wand only (card and glow planes dropped, 17k → 4k tris). Long axis found by PCA, head = wider end, head's wide side across X.
  - `tool_icons.py`: 256 px transparent card icons rendered from the exported FBX files → `UI/ClubIcons/`.
- `ToolModelsMenu.cs` (*Magic > Prefabs > Use Tool and Ball Models*): import settings, URP materials (textures, normal map, linear → sRGB colours, glowing wand gem), the three tool `Visual`s, GolfBall mesh + material, club icons → `ClubProfile.Icon` if empty.
- `ClubProfile.Icon` (optional Sprite; architecture §4.2), `SelectableCard.SetContent(..., Sprite, Color)` overload (Core; old overload kept for 4.E.3), `ClubPanelView` shows the icon.
- HUD: `GameUiBuilder` builds the club cards anchored upper right under the minimap (shared constants); `ClubPanelLayoutMenu.cs` (*Magic > Level > Update Club Panel*) moves an existing panel (L1) and enlarges the card icons to 60 px.
- Title: `GameUiBuilder.GameTitle` = "Hole in One for Her Heart"; `GameTitleMenu.cs` (*Magic > Scenes > Apply Game Title*) updates the existing MainMenu scene.
- Credits and asset register rows (3 models + icons), Art README note, issue #139.
- Henry ran the tools; the log showed the wand 100x too big (its source scene has unit scale 100). `tool_models.py` now resets the unit scale; the wand FBX was re-exported (0.34 m).
- Henry's L1 edits became the tool defaults: Gate at z 25.44 (`GardenGreyboxMenu`); the Kenney build switches off the placeholder fences (Environment/Boundary) and fills the pond basin (water 10.38 x 1.68 x 19.58, trigger 9.59 x 2.34 x 19.05 in pond space) (`GardenKenneyLevelMenu`).
- Main menu art: `UI/MainMenu/MainMenu_Background.jpg` (Gemini) and `Title.png`; `GameUiBuilder.ApplyMainMenuArt` (background stretched to the screen; an AspectRatioFitter crop left the saved rect at size 0, so the menu looked blank; title picture over the switched-off Title text), used by Add Main Menu UI and by the new *Magic > Scenes > Apply Main Menu Art* (`MainMenuArtMenu.cs`). Credits and register rows.

## Decisions / open questions
- Title lettering `title.png`: assumed generated by Henry with Google Gemini like the background; correct the credit if not.
- Third-party models don't count as hand-made artwork (#80).
- The Sakura wand is fan art of CLAMP's *Cardcaptor Sakura*: the model is CC BY, the design is CLAMP's; registered as fair use for the class (asset policy allows it). Swap it if the course or the team prefers fully licensed art.
- Window/product name (`ProjectSettings` productName) still "115A-535652-3DGP-Final": PO to decide whether it becomes the game title.
