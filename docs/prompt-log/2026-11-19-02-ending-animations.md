# 2026-11-19 02 — Ending animations: hamster + princess Victory on the Ending screen, hamster Defeat in the time-up dialog

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #141, ADR-0014, #36 (clips), #30 (Ending), #20 (level dialogs)

## Prompt (verbatim)
> I want to add character animation for ending. please add animation clips for success for both hamster. add fail for prince hamster. the sucess animations will be place in ending scene. If i remember correctly, ending is popup?, place ending animation in it.

Answers to the AI's questions: new ending clips (not the existing Celebrate / Cheer / Fail); success in the Ending scene, fail in the level's time-up popup; shown through a RenderTexture. "there is empty space in center of canvas. characters should be place inbetween the text (above) and buttons (bottom)."

## What the AI did
- Checked the current state: Celebrate / Fail (hamster) and Cheer (princess) already existed. Ending is a separate scene, not a popup. The only popup is the level time-up dialog (only L1 has it so far).
- Blender 5.3 (Windows), by script: `anim.py` can now rebuild the clips from a saved `Source~/<Name>.blend` (old actions replaced, texture step skipped), the contact sheet goes to the out folder and can be limited to some clips. New clips: hamster **Victory** (loop: alternating fist-pump hops, then a big jump), **Defeat** (shock, knees give way, sits down, head hung) → **DefeatLoop** (sobbing, head shake); princess **Victory** (paws up in a V, bouncing, swaying, tail wag). Checked with front / side preview renders and fixed: a raised paw hides behind the wide head (arm now sideways with the forearm up), the slump bent into a face-plant, the princess's paws pressed to her cheeks.
- Re-exported `Hamster.fbx` / `Princess.fbx` (+ `.blend`). Unity batch import: no compile errors, the FBXs import.
- `CharacterModelsMenu` (*Use Character Models*): clip table entries; an already-configured FBX gets only its missing clips appended; an existing controller gets only the missing ending states (Any State on triggers `Victory`, `Defeat` → `DefeatLoop`).
- New `UI/CharacterStageView` (Game) and `CharacterStageMenu` (*Magic > Scenes > Add Ending Victory Stage*, *Magic > Level > Add Time-Up Defeat Stage*, also called by *Add HUD and Menus*): off-screen stage 500 m below with prefab instances, key light, camera → `UI/Stages/*.renderTexture` → RawImage. Ending: 960 × 400 between the results and the buttons. Time-up: 420 × 300 above the dialog box.
- Docs: handoff doc (clips, rebuild without `src/`), architecture §4.2 / §4.8, ADR-0014.

## Decisions / open questions
- RenderTexture stages instead of a camera-space canvas, so the popup works too (ADR-0014).
- L2 / L3 have no time-up dialog yet. *Add HUD and Menus* adds the defeat stage when their owners run it.
- Framing, light and positions are first guesses from the reference layout. Tune them in the scene (stage camera, Key Light, actors).
- Issue #141 could not be added to Project #2 (`gh` token lacks the `project` scope: `gh auth refresh -s read:project,project`).
