# 4.E.2 Function selection — Pongpat

Issue: [#65](https://github.com/hwupu/115A-535652-3DGP-Final/issues/65) · Demo: Nov 16
Scene: `Scenes/4E2_FunctionSelection.unity` · Guide: `docs/mini-exercises.md`

## Scope and what's new

PO-approved scope (2026-11-04): three menu functions — **Game Level**, **Settings** and **Character Selection**. A demo video is omitted by that decision.

New for this exercise:

- Generic page navigation in `Project3DGP.Core.UI.PageRouter`, including menu return and keyboard focus.
- `FunctionSelectionDemo`: settings and character selection applied to a small standalone arena; pickup progress, moving monsters and restart behavior.
- A Magic tool that authors the base environment and UI in the Editor, preserving existing work when run again.

No assignment scripts/models or third-party code/media are used. Geometry uses Unity primitives; the preview tone is generated with a sine wave. The demo does not reference game code or load a game level scene.

## Setup in Unity

1. Stop Play mode and wait for scripts to compile.
2. Click **Magic > Mini-Exercises > Pongpat 4.E.2: Set Up Scene**. If Unity asks to save open scenes, save your work; cancelling stops setup. The tool opens the exercise's existing scaffold scene.
3. Check Console `[Magic]` messages and the generated arena/UI in the Hierarchy. Running the tool again creates missing objects and preserves existing authored values.
4. Save the scene, then press Play. Build Settings are unchanged; this scene runs directly in the Editor.

## Controls and behavior

- Click **Game Level**, **Settings** or **Character Selection** to open that content. Each has a **Back to Menu** button; **Escape** also returns to the menu.
- In the level, use **WASD** or the **arrow keys** to move. Collect the three pickups while avoiding two moving monsters. Monster contact returns the avatar to its start without resetting collected pickups.
- **Restart Level** resets the avatar, monsters and all pickups. Leaving and reopening the level preserves pickup progress.
- In Settings, adjust **Volume** and **Brightness**. **Preview Sound** plays a generated tone through the demo's local AudioSource at the selected volume. Brightness changes the demo's directional light intensity from 0.4 to 1.6; these sliders do not change project or game settings.
- Select **Amber** or **Frost**, the two colored capsule avatars. The chosen avatar is used in the arena.

## Requirement coverage in the implementation

The source, setup tool and saved scene provide the following. Play-mode acceptance is recorded below.

- [x] At least three labelled menu buttons, each opening its content.
- [x] Game-level content: arena environment, selected avatar, two moving monsters and three collectible objects.
- [x] At least two functional settings: volume and brightness.
- [x] Character selection with at least two characters: Amber and Frost.
- [x] A return path from every content page to the menu.
- [x] Reusable technique in Core; demo glue in `Project3DGP.MiniEx.Pongpat`, with no Game dependency.
- [x] Controls, new work, requirement coverage, references and TA modification points documented here.
- Video: outside the PO-approved three-function scope.

## Acceptance — verified 2026-11-04

- [x] Run the Magic tool, save the generated scene and confirm no compiler/Console errors.
- [x] Enter Play directly from this scene; open all three functions and return from each.
- [x] Move, collect all three pickups, touch a monster, reopen the level and restart; verify the documented reset behavior.
- [x] Hear the preview volume change and see brightness change in the arena.
- [x] Select each avatar and verify it appears in the level.
- [x] Run the Magic tool again outside Play mode and check that manual scene edits remain intact.
- [x] In the Test Runner's PlayMode tab, open `Project3DGP.MiniEx.Pongpat.PlayModeTests` → `Project3DGP.MiniEx.Pongpat.Tests` → `PongpatFunctionSelectionTests` and run its nine tests. The seven `PongpatObjectInteractionTests` are a separate fixture under the same parent.

Pongpat confirmed the manual demo checks. Unity's saved test report records **9/9 function-selection tests passed** at 18:29:30 Asia/Taipei on 2026-11-04. The scene was saved and its serialized references checked after setup was repeated. Keep screenshots/recordings for the TA demo and report.

## Reusable technique and TA modification points

`PageRouter` holds page roots and shows exactly one configured page. `Show(index)` switches content, `ReturnToMenu()` uses `menuIndex`, and `PageChanged` reports navigation. Invalid destinations preserve the current page. Keep the router on an always-active object outside its page roots.

- Change page order, `initialIndex`, `menuIndex` or first selected controls in the router Inspector. Demo indices are 0 = menu, 1 = level, 2 = settings and 3 = characters; update demo callbacks when changing that mapping.
- Adjust movement/monster parameters in `FunctionSelectionDemo`, or move authored pickups and obstacles in the scene.
- Change the settings range or preview tone in the demo code and explain how the UI values map to light intensity/audio volume.
- Change avatar colors, selection callbacks or the level restart behavior.

Potential game reuse: the generic router can support menus in #27 / #30. This exercise does not alter those game contracts or Henry's menu implementation. Pongpat should refine this first version and be able to explain and modify it live.

## Export and references

For E3, export this exercise folder together with `MiniExercises/Pongpat/Project3DGP.MiniEx.Pongpat.asmdef` and the Core code it uses. Enable **Include dependencies** and inspect the package contents; no Game code is required. Tests under `Assets/_Project/Tests/Pongpat/FunctionSelection/` are optional for export.

The receiving project needs Unity 6000.3.26f1 with URP 17.3.0, Input System 1.20.0, uGUI 2.0.0 (including TMP) and AI Navigation 2.0.15, matching the Core/member assembly references. Package Manager dependencies are not bundled by Export Package. The saved scene runs without Editor tooling; to regenerate it, also export `Editor/Magic/PongpatFunctionSelectionSetupMenu.cs` and `Editor/Magic/MagicMenuUtil.cs`.

- Course requirements: `docs/references/course/mini-exercises.md`, §4.E.2.
- Project rules: `docs/mini-exercises.md`, ADR-0010 and ADR-0011.
- API references: Unity's built-in documentation for uGUI Buttons/Sliders, EventSystem, CharacterController, AudioSource and Light.
- Third-party code and media: none.
