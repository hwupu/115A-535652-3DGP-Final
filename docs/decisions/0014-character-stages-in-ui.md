# 0014 — Animated characters in UI screens via off-screen RenderTexture stages
- Status: Accepted
- Date: 2026-11-19
- Deciders: Henry (PO), Claude

## Context
The Ending scene and the level time-up dialog should show the hamsters acting (#141): both celebrate on the Ending screen, and the hamster sits down defeated in the time-up popup. Both are Screen Space - Overlay canvases with a full-screen background or dim layer, so a 3D model placed in the world is hidden behind them. Switching the Ending canvas to Screen Space - Camera would only solve the Ending, not the popup, and would change its layout.

## Decision
- A **stage** is a small set of scene objects (character prefab instances, a point key light, a camera) 500 m below the scene. The camera renders into a RenderTexture asset (`UI/Stages/*.renderTexture`, sRGB, alpha) with a transparent background, no shadows and no post-processing, so the course and the sun's shadows never reach it.
- A RawImage with `UI/CharacterStageView` shows it. The stage is active only while the RawImage is (OnEnable / OnDisable). Each time it shows, every actor's Animator is rebound and gets its trigger. The Animators run on unscaled time.
- Magic tools build the stages (ADR-0011, no runtime scene building): *Scenes > Add Ending Victory Stage*, *Level > Add Time-Up Defeat Stage* (also part of *Add HUD and Menus*, so every level gets it).
- The clips are new Blender clips (`Victory`, `Defeat` → `DefeatLoop`) on Any State transitions in the existing controllers. The in-level `Celebrate` / `Fail` stay as they are.

## Consequences
- The same pattern works for any screen or dialog (e.g. the L3 ending cut-scene, #50): add a stage, a RawImage and the triggers.
- Each visible stage costs one extra camera render. It is off while hidden.
- The stages are prefab instances, so model changes reach them. Framing is the stage camera's transform, adjustable in the scene.
