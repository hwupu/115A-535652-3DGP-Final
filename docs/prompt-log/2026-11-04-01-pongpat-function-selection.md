# 2026-11-04 01 — Pongpat function selection (#65)

- **Who:** Pongpat (@empongpat) + Codex (GPT-6)
- **Related:** [#65](https://github.com/hwupu/115A-535652-3DGP-Final/issues/65), ADR-0010, ADR-0011; reuse candidates #27 / #30

## Prompt (verbatim)

> Implement according to the attached ticket.

Attachment: `Screenshot 2569-11-04 at 14.25.00.png`, showing #65, “Mini-ex (Pongpat) 4.E.2 Function selection - standalone demo scene (Nov 16)”. It requires at least three labelled menu buttons, reachable content with menu return, standalone scene/folder, generic Core code, Pongpat demo glue without Game dependencies, and an exercise README. The screenshot ends partway through acceptance criteria; the local course requirements and mini-exercise guide were also read.

PO answer to the menu-scope question (verbatim):

> Game level + Settings + Character selection (recommended; no video asset needed)

PO answer to the proposed design question (verbatim):

> Use this placeholder demo design (recommended)

The approved design was: “a primitive arena, WASD movement, three collectibles and two moving monsters; two coloured capsule avatars; and working volume/brightness settings”.

## What the AI did

- Read the process, architecture, course requirements, mini-exercise rules, asset policy and Magic ADR before implementation. The ticket and configured git identity both identify Pongpat.
- Created branch `feat/65-mini-4e2-function-selection` from the existing checkout; preserved all pre-existing #66 files and unrelated asset changes. No existing shared component or Game contract was changed.
- Added generic `Core/UI/PageRouter.cs`: one visible authored page, menu return, invalid-destination handling, page-change event and optional keyboard focus.
- Added `FunctionSelectionDemo.cs`: three content pages, movement/collectibles/patrol monsters, avatar choice, local audio volume with a generated preview tone, and directional-light brightness.
- Added `PongpatFunctionSelectionSetupMenu.cs`, under **Magic > Mini-Exercises > Pongpat 4.E.2: Set Up Scene**. It opens the existing scaffold with a save/cancel prompt, authors missing UI/world objects, preserves existing values, repairs missing references and persistent button callbacks, and supports Undo. It leaves saving the exercise scene to the human and preserves Build Settings.
- Updated the exercise README with scope, controls, new work, exact requirement coverage, TA modification points, export guidance and pending manual acceptance.
- Added focused PlayMode tests under `Tests/FunctionSelection/`, separate from the unfinished #66 tests.

## Decisions / open questions

- **PO scope:** implement Game Level, Settings and Character Selection. Video is outside this approved three-function scope. No new game behavior or shared contract decision; ADR-0010 and ADR-0011 apply without amendment.
- **Assets:** Unity primitives, six locally authored solid-color materials and a runtime-generated sine tone; no third-party imports. Register generated materials in the asset/credits tables before committing them.
- **Verification:** the project has `Temp/UnityLockfile`; no batch-mode Editor was launched. Standalone API compilation uses Unity's bundled Roslyn and actual installed Unity references, with output under `/private/tmp`. This does not replace Unity import or Play-mode checks.
- **Human handoff:** run Magic, save, test all content/return paths, settings, both avatars, monster contact, collection/restart, repeat setup and the PlayMode tests. Generated scene/material/.meta files and the project-specific Editor log must be verified after the human reports completion, before committing or opening the PR.
- **GitHub:** Yab redirected the issue URL to sign-in; `gh` is unavailable. The attachment and local course text supplied the implementation requirements. Mirror the approved scope/design in #65 when authenticated GitHub access is available; link #65 in the eventual PR and list TA modification points.

## Validation results

- Standalone API compilation passed for all Core code, all Pongpat runtime code, the new Magic tool (with `MagicMenuUtil`) and the new test assembly, using actual installed Unity references. This caught and corrected a `CharacterController` API mistake before handoff. No Unity process was launched.
- Eight PlayMode tests were added: initial/hidden page navigation, invalid destinations, menu return/events, same-page visibility repair, avatar persistence, scoped settings, collection/completion/restart, and monster respawn preserving progress. They compile; execution remains pending.
- Reviewed script fields against Editor wiring; the router alone owns keyboard focus, Start refreshes initial focus, the character page previews both capsules, and arrow movement cannot navigate the level buttons.
- Formatting, assembly JSON and runtime dependency/scene-authoring checks passed. Independent source review found no blocking issue; export guidance now lists package dependencies and optional setup tooling.
- Unity scene generation, Play-mode tests and TA acceptance evidence remain pending; no scene, prefab, asset or `.meta` file was hand-edited.

## Follow-up — integration review after main update

Prompt (verbatim):

> I have updated main and merged the latest code to this branch, check again whether we have to update anything before closing this ticket.

- Current branch is `feat/mini-exercise-function-selection`, at `5f1e576` (same committed tree as local `main` / `origin/main`). #65's implementation remains in the working tree. #66 is now merged; the previous session's description of it as unfinished is historical.
- Compared the merged changes with #65's dependencies: Core UI, Core/Pongpat assembly definitions and `MagicMenuUtil` are unchanged. New game art/audio and added Postprocessing/ShaderGraph packages require no #65 compatibility change. The approved primitive-demo scope remains valid.
- The saved `4E2_FunctionSelection.unity` still contains only Directional Light, Main Camera and Ground. No saved router, demo, menu pages or generated exercise materials exist yet. Script imports and generated `.meta` files alone do not satisfy the standalone-scene criterion.
- Actual project compilation was rechecked with Unity's Bee response files, flags, analyzers and package references, with outputs redirected to `/private/tmp`: Core, Pongpat runtime, #65 tests and all 54 Editor sources compiled with zero warnings/errors. `Temp/UnityLockfile` is present; no Editor was launched. No #65 test-execution evidence was found. The global Editor.log belongs to a different 6000.6.2f1 launch and is not treated as project validation.
- Added the six planned original-material credits/register entries now, marked pending generation/commit, so the eventual generated materials can be committed with provenance.
- Fixed a defensive issue identified during review: disabling the demo after an incomplete setup did not prevent persistent UI buttons from invoking callbacks and throwing further null-reference exceptions. A readiness flag now guards these callbacks; the valid setup behavior is unchanged. Added one regression PlayMode test using a real Button with all ten persistent callbacks. The suite now has nine tests; all current source groups, including the new regression, compile cleanly against actual project references. Execution remains pending.
- Found unrelated dirty asset metadata with changed GUIDs and removed import settings (for example the cat FBX loses its material mapping and animation-clip import settings). These files were not modified by this review and must not be included in #65's PR without separate owner resolution. The scaffold's ground material GUID resolves to URP's package `Lit.mat`; it is not a missing-material defect.
- Before closure: human runs Magic and saves the scene, completes all manual acceptance checks and PlayMode tests, then generated files/logs are verified, #65 is committed with its metadata and reviewed through an issue-linked PR. Keep the ticket open until those steps are complete.

## Follow-up — test assembly name and location

Prompt (verbatim):

> Rename `Project3DGP.FunctionSelection.PlayModeTests` to Project3DGP.MiniEx.Pongpat.PongpatFunctionSelectionTests so it's located in the same place with my previous mini exercise.

- Renamed the assembly and its `.asmdef` file to `Project3DGP.MiniEx.Pongpat.PongpatFunctionSelectionTests`.
- Moved the complete test folder to `Assets/_Project/Tests/Pongpat/FunctionSelection/`, alongside Pongpat's previous exercise tests. Its own PlayMode assembly remains isolated; the object-interaction test assembly is unchanged.
- Moved all existing folder/script/assembly `.meta` files without editing or regenerating their contents; metadata hashes were checked before and after the move.
- Updated the exercise README's Test Runner and export instructions. Earlier paths in this log describe the historical implementation before this requested move.
- Validation: the renamed assembly compiles against the project's actual Unity references with all three source files (nine tests). No active source/documentation references retain the old name/path; remaining mentions are historical log entries. PlayMode test execution remains pending.

## Correction — shared Test Runner parent, separate exercise fixture

Prompt (verbatim):

> No it should be under the same parent but separated in the Tests.

- The attached Test Runner screenshot clarified that the requested grouping is one existing `Project3DGP.MiniEx.Pongpat.PlayModeTests` assembly, with `PongpatFunctionSelectionTests` beside `PongpatObjectInteractionTests` under `Project3DGP.MiniEx.Pongpat.Tests`. The previous separate-assembly interpretation was incorrect.
- Replaced the redundant function-selection assembly definition with an assembly reference to the existing Pongpat test assembly. Its metadata GUID was preserved; Unity can update its importer type on refresh. Added the explicit uGUI dependency to the shared member test assembly.
- Consolidated the function-selection test classes into one partial `PongpatFunctionSelectionTests` fixture across three files, preserving all nine cases and their assertions. Runtime-demo setup is called explicitly only by its four tests, keeping router and incomplete-setup cases independent. The existing object-interaction fixture and its seven cases remain unchanged.
- Renamed paired source metadata files without changing GUIDs, and updated the README's Test Runner instructions. This correction supersedes the previous separate test-assembly name and grouping.
- Validation: current Core/runtime and the combined four-source Pongpat test assembly compile with zero warnings/errors against actual project references. Static inspection of the emitted DLL confirms exactly two public fixtures in `Project3DGP.MiniEx.Pongpat.Tests`: `PongpatObjectInteractionTests` (seven cases) and `PongpatFunctionSelectionTests` (nine cases). The asmref resolves to the existing assembly's GUID and no separate function-selection asmdef remains. Unity must refresh/recompile to update its cached Test Runner tree; no Editor or tests were launched during validation.

## Verification and commit follow-up

Pongpat subsequently confirmed the PlayMode tests and manual demo, and saved the generated scene after the initial on-disk scaffold was noticed. Saved test evidence and the scoped commit are recorded in [the commit session](2026-11-04-04-pongpat-function-selection-commit.md); earlier pending statuses above describe the implementation's history.
