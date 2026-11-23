# 2026-11-04 04 — Danger timer (#7)

- **Who:** Pongpat + Codex
- **Related:** [#7](https://github.com/hwupu/115A-535652-3DGP-Final/issues/7), [architecture §4.5](../architecture.md#45-danger-timer--pongpat), ADR-0011
- **Branch:** `feat/7-danger-timer`

## Prompt (verbatim)
> Implement according to attached ticket.

The attached screenshot describes #7: a per-level time budget, continuous countdown plus per-shot
cost, danger state/progress/events, edit-mode tests, flight suspension, and a final exhausted shot
that can still win before the ball stops. Its PO decisions agree with the approved architecture.

## What the AI did
- Confirmed Pongpat's checkout identity and ownership, read the Scrum process, shared contracts,
  Editor tooling rules, prompt-log conventions, and program rubric. Created an issue-specific branch.
- Added `DangerConfig` with the four approved seconds fields and validation. Level owners choose
  the actual balance values by hand; no new per-level gameplay values were invented.
- Implemented `DangerManager : IDangerTimer`: scaled-time countdown; normalized consumed progress;
  shot cost independent of club; `StateChanged`, `ProgressJumped`, `OnWarning`, `OnCritical`, `OnFailed`;
  reset/retry; independent external and flight pauses; deferred final-shot failure. No scene construction
  or automatic subscriptions occur at runtime. LevelManager (#27) owns subscriptions and completion pause.
- Added separate create-only Magic commands for the three config assets and shared timer prefab.
  The tools preserve existing assets and do not modify scenes or Build Settings.
- Added 38 edit-mode test cases for countdown, thresholds/events, shot jumps, flight/menu overlap,
  final-shot failure/win, reset/retry, configuration validation, invalid deltas, and event-triggered retry.
- Added [danger-timer.md](../danger-timer.md) with Unity steps, config constraints, event semantics,
  and the LevelManager handoff. Two agents independently reviewed the timer/integration behavior.
- Compiled the Game, Editor, and edit-mode test assemblies with Unity's bundled Roslyn compiler,
  cached project response files, and outputs redirected to `/tmp`. All compiled successfully; only
  the existing unused `UiTestLevelFlow.LevelCompleted` event warning remained.
- Preserved all pre-existing third-party/model import metadata and `.vscode/` changes. Added no binary
  assets and did not hand-edit serialized Unity files or metadata.

## Decisions / open questions
- This implements the existing approved decisions; no new contract or product decision needs an ADR.
- The Editor is open (`Temp/UnityLockfile` exists), so no batch Unity process was launched. Computer
  use timed out while inspecting Unity; edit-mode test execution and generated-asset checks remain
  for the human Editor handoff. Compilation is verified, not test execution.
- The live GitHub issue required sign-in in yab-browser, and `gh` is not installed. Used the supplied
  screenshot and approved local contract; no issue comments, push, or PR were sent.
- Pending: human runs the Magic commands, tunes their own level's config, and executes the edit-mode
  tests. After confirmation, verify generated assets/metadata and Editor.log before committing as
  required by AGENTS.md. PR creation remains pending that verification.
- HUD/cat/tutorial connection is owned by LevelManager (#27), exposed through `ILevelFlow.Danger`;
  the current `AvatarWiring_TEMP` still returns null. Shared scene slot placement belongs to #95.

## Follow-up — visible config creation feedback

### Prompt (verbatim)
> I don't see anything running Magic > Game > Create Danger Configs.

- Confirmed that Unity generated all three per-level `.asset` files and their metadata. The command
  had run successfully; its result was difficult to see when the Console was closed.
- Updated the command to focus the Project window, select/highlight the Danger folder, and log a
  created/existing summary on every run. Existing config values remain untouched. Updated setup notes.
- Rechecked the Editor tool with Unity's bundled compiler; no serialized Unity files were edited.
