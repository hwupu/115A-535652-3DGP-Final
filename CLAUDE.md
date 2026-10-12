# Agent Guide (read first)

Course project: 535652 3D Game Programming Final (115A). Unity 6000.3.25f1, URP, Input System. Team of 3 humans; humans are the **Product Owner (PO)**. AI agents are **Scrum Master (SM)** first, **Developer** later (see `docs/scrum/process.md`).

## Non-negotiables
1. **Ask the human** when a requirement, scope, art/gameplay choice, or licensing question is unclear. Don't guess on product decisions.
2. **Log it.** Every session that changes the repo adds/updates a file in `docs/prompt-log/` (see its README). Every major decision gets an ADR in `docs/decisions/`.
3. **No runtime scene building.** Scenes are authored in the Unity Editor. Agents help via Editor tools under the menu **Tools > 3DGP > ...** (scripts in `Assets/_Project/Editor/`) that humans click. Gameplay code must not construct the scene at runtime.
4. **Never hand-edit** `.unity`/`.prefab`/`.asset`/`.meta` files unless asked; prefer Editor scripts. Never delete or regenerate `.meta` files (GUIDs break references).
5. **Don't commit** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds, or OS junk (`.gitignore` covers this). Binary assets go through Git LFS (`.gitattributes`).
6. Work on a branch (`feat/<issue>-slug`), open a PR, link the issue. Don't push to `main` directly (after initial setup).
7. Keep it small and fast: 3 people, short sprints, minimal ceremony.

## Layout
- `Assets/_Project/` — all our own content (Editor, Scenes, Prefabs, ...; run Tools > 3DGP > Create Project Folders). Third-party/store assets stay in their own top-level folders (e.g. `Assets/ThirdParty/<Pack>`), untouched, so they're easy to update.
- `docs/` — process, ADRs, prompt log, scrum artifacts.
