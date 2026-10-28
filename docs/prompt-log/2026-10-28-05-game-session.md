# 2026-10-28 05 — GameSession: cross-scene data (#89)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #89, #70, #44, #27, #77, #18

## Prompt (verbatim)
> proceed

(after the agent recommended #89 next, then #80)

> what instrutor mean is to avoid merge conflict, it is easier to just tell everyone to work on their own scene files. however, we are a collab team, and should share the same framework/interface. i used added GameSession to L2, L3, and ending. It is for progrme quality. lets move on

## What the AI did
- `Session/GameSessionData`: a plain `[Serializable]` class (JsonUtility) with:
  - unlocked clubs, stored by asset name;
  - level and XP;
  - the best result per level (higher score, then fewer strokes, then more time left);
  - settings (volumes).
- `Session/GameSession` per §4.9:
  - singleton; the first instance wins, it uses DontDestroyOnLoad, and later copies destroy themselves;
  - starts with the standard club;
  - `UnlockedClubs` stays in slot order;
  - API: `Unlock`, `SetProgress`, `RecordResult`, `GetBestResult`, `Data`/`Load` for #44, and the `Changed` event.
- Added `Scoring/LevelResult` per §4.6, as was done with `LevelGoal`. Pongpat owns it from here.
- Added *Magic > Game > Add Game Session*. It creates `Prefabs/Shared/GameSession.prefab` (3 clubs, starting club = Standard) and adds it to the open scene.
- Tests: `GameSessionDataTests` (4, edit mode) and `GameSessionTests` (3, play mode).
- Updated the architecture §4.9 contract and added an implementation note.

## Decisions / open questions
- Agent: the session is a prefab in every playable scene (no runtime scene building, rule 3) instead of being created in code.
- Agent: ShotController doesn't read `UnlockedClubs` yet, so L1 keeps all 3 clubs for testing. Hooking it up belongs to #70 (leveling) or #27 (LevelManager wiring).
- Henry: shared framework objects go into **every** scene, not only the owner's. He added GameSession to L1, L2, L3, Ending and MainMenu. The rule is now in CLAUDE.md (Layout > Scenes).
