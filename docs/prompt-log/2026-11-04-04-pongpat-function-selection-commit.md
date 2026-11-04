# 2026-11-04 04 — Verify and commit Pongpat 4.E.2 (#65)

- **Who:** Pongpat + Codex
- **Related:** [#65](https://github.com/hwupu/115A-535652-3DGP-Final/issues/65), [implementation and follow-ups](2026-11-04-01-pongpat-function-selection.md), ADR-0010, ADR-0011
- **Branch:** `feat/mini-exercise-function-selection`

## Prompt (verbatim)

> I've verified both Test Runner's PlayMode and running the demo Scene. Everything is working correctly, please commit the changes related to this specific mini exercise. I will open the PR by myself.

## Verification and scope

- Author identity is Em Pongpat <empongpat@gmail.com>. The authorized commit includes only #65's Magic setup tool, generic Core PageRouter, exercise scene/materials/scripts/documentation and metadata, function-selection tests/assembly reference, the shared member test assembly's explicit uGUI dependency, related asset-credit rows, and prompt logs.
- Read the saved Unity `TestResults.xml` under `Library/Application Support/DefaultCompany/115A-535652-3DGP-Final`: all nine `PongpatFunctionSelectionTests` passed at 18:29:30 Asia/Taipei on 2026-11-04, with zero failures. The shared assembly discovers 16 cases; this latest run executed only the nine function-selection cases, so no new seven-test object-interaction result is claimed.
- The latest project-scoped Unity 6000.3.26f1 Editor log records the Magic setup and successful test run without C# compilation errors or unexpected null-reference exceptions. The missing-setup test intentionally expects the demo's setup diagnostic. Earlier transient duplicate-assembly errors do not recur in this session.
- Pongpat confirms the demo's controls, visuals, settings, avatar choice, gameplay and real-time behavior work. Core, runtime, shared tests and Editor sources were previously compiled against actual project references; emitted test metadata also confirmed the requested shared assembly/namespace with separate exercise fixtures.
- Audited 20 #65 metadata GUIDs: valid, unique, with matching assets. The six original primitive materials total 22,942 bytes and reference resolving URP package assets. Their entries in `docs/assets.md` and `CREDITS.md` are included. No binary/LFS assets are added.
- Initial disk inspection found that the scene was still the original scaffold despite generated materials. Asked Pongpat to run the existing Magic tool outside Play mode and save the authored scene before committing. Pongpat replied (verbatim): “saved”. The generated scene is now present on disk, including the four page roots and configured router, demo, player, two avatars, two monsters, three seeds, settings and button callbacks. The Magic command ran again; no hand edits were made to serialized content.
- Updated the exercise README to record the confirmed manual acceptance and actual 9/9 test result, and updated the material register with the generated assets' size.
- Final saved-scene audit passed: 73 GameObjects, four ordered page roots and first-selected controls, two avatar visuals, two monsters, three collectibles and ten correct persistent button callbacks. All nonzero local file references and 20 external GUIDs resolve; all six materials are used. No Game, Henry-exercise or Asset Store dependency is present.
- All unrelated ThirdParty/character-art metadata and local `.vscode` files remain outside the commit. Existing object-interaction test code and all game code remain unchanged. No scene, prefab, material or metadata file is hand-edited.

## Delivery

- Commit locally under Pongpat's identity with a Codex co-author trailer after verifying the saved scene and staged allowlist. Pongpat will open the PR; no push, PR creation, merge or ticket closure is requested.
