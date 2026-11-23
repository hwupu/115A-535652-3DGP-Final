# 2026-11-04 05 — Package Pongpat 4.E.2 for submission

- **Who:** Pongpat + Codex
- **Related:** #65, [implementation](2026-11-04-01-pongpat-function-selection.md), [verification and commit](2026-11-04-04-pongpat-function-selection-commit.md)
- **Source revision:** `2a2fb61` (main's merge of #135); current checkout is `feat/7-danger-timer`

## Prompts (verbatim)

> Zip this Mini Exercise so I can submit it.

> Continue

## What the AI did

- Created `Builds/Submissions/Pongpat_4E2_FunctionSelection.zip`, an ignored submission artifact containing a reduced Unity project with 84 files (109,507 bytes).
- Included the saved exercise scene, six materials, runtime script, PageRouter, original Core/member assembly definitions, nine function-selection tests and their shared test-assembly configuration, the optional Magic setup/helper, required URP assets, essential original project settings, package manifest/lock, and matching metadata for assets and ancestor folders.
- Excluded all Game source/assets, other exercises, Asset Store content, third-party art/audio, caches, local editor state, and unrelated danger-timer work. Omitted the source game's EditorBuildSettings so it does not reference absent game scenes; the submission README directs Unity Hub to open the project and then the included demo scene.
- Copied source assets, scenes, settings and metadata byte-for-byte. Added only archive-local `README_SUBMISSION.md`, `SOURCE_MANIFEST.json` and `SHA256SUMS.txt`; no original Unity serialized file was hand-edited.

## Validation

- ZIP CRC validation and per-file SHA-256 verification passed. All 81 copied files match their originals; 44 metadata GUIDs are unique with matching assets, and all 20 external GUIDs used by the saved demo resolve to included assets or declared Unity-package resources.
- The seven staged C# files compile against actual Unity project references with zero warnings/errors. Static DLL inspection confirms exactly one exported test fixture with nine cases; the source project previously passed all nine cases and Pongpat verified the demo.
- The reduced archive was not imported in a fresh Unity Editor instance, and tests were not rerun. Eleven legacy render-resource GUIDs already present in the source URP template settings are absent from the current package cache; those settings were preserved unchanged, confirmed against HEAD, and the inherited references are recorded in the archive's source manifest. No new unresolved demo-scene references were introduced.
- Archive contents, test assembly references, path safety and exclusions were checked. No upload, commit, push or PR operation was requested or performed.
