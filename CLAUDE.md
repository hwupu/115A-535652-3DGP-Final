# Agent Guide (read first)

Course project: 535652 3D Game Programming Final (115A). Unity 6000.3.26f1, URP, Input System. Team of 3 humans; humans are the **Product Owner (PO)**. AI agents are **Scrum Master (SM)** first, **Developer** later (see `docs/scrum/process.md`).

## Project context
Hamster fantasy golf, 3 levels, one owner each (Henry/@hwupu Garden, Bahadir/@bahadir-koksal Desert, Pongpat/@empongpat Lake). Vision: `docs/references/proposal.md`. Plan: `docs/scrum/roadmap.md`. Shared contracts and code ownership: `docs/architecture.md`. Grading rules (rubrics, report, individual mini-exercises): `docs/references/course/` — check work against them; gaps in `docs/scrum/grading-gap-analysis.md`. Individual mini-exercises (4 per member, standalone scenes, generic Core code): follow `docs/mini-exercises.md`. Backlog = GitHub issues; sprints = milestones; board = GitHub Project #2 (Status: Todo, In Progress, Done; add new issues to it and set Todo). Owners in issues are provisional until sprint planning.

## Non-negotiables
1. **Ask the human** when a requirement, scope, art/gameplay choice, or licensing question is unclear. Don't guess on product decisions.
2. **Log it.** Every session that changes the repo adds/updates a file in `docs/prompt-log/` (see its README). Every major decision gets an ADR in `docs/decisions/`.
3. **No runtime scene building.** Scenes are authored in the Unity Editor. Agents help via Editor tools under the top-level menu **Magic > ...** (scripts in `Assets/_Project/Editor/Magic/`, one file per function, create only what is missing, never reset existing work; ADR-0011) that humans click. Gameplay code must not construct the scene at runtime. Only exception: loop-spawned objects inside mini-exercise demo scenes (ADR-0010).
4. **Never hand-edit** `.unity`/`.prefab`/`.asset`/`.meta` files unless asked; prefer Editor scripts. Never delete or regenerate `.meta` files (GUIDs break references).
5. **Don't commit** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds, or OS junk (`.gitignore` covers this). Binary assets go through Git LFS (`.gitattributes`).
6. **Assets:** never commit Unity Asset Store content (free or paid; `Assets/AssetStore/` is git-ignored). Other third-party assets only if the license allows redistribution, under `Assets/ThirdParty/`. Always add credits to `docs/assets.md` and `CREDITS.md`. Never add assets without the human confirming the source/license.
7. Work on a branch (`feat/<issue>-slug`), open a PR, link the issue. Don't push to `main` directly (after initial setup).
8. Keep it small and fast: 3 people, short sprints, minimal ceremony.

## How to work with the team
- **Onboarding and prompts:** README "Start here" and "Prompts for Claude Code". Keep the "Where we are" table current when a milestone-level item lands.
- **Code ownership:** implement contracts exactly as in `docs/architecture.md`; changing one needs a PR reviewed by everyone it affects. Work only on the requesting member's code. LOC is graded **per git author**, so commits are made in that member's checkout under their identity (agent adds the Co-Authored-By line).
- **Unity Editor work = Magic tool + manual steps.** When a task needs Editor changes: (1) write a Magic menu tool (rule 3, ADR-0011) if setup is repeatable, (2) end your reply with a numbered **"In Unity"** guide: which menu to click, what dialogs mean, what to check (Console `[Magic]` lines, Hierarchy, Build Settings), (3) after the human says it's done, **verify the generated files** (git status, scene/asset contents, `Editor.log` errors) before committing. Empty folders need a `.gitkeep` so their `.meta` doesn't orphan.
- **Compile check:** if the Editor is closed, `Unity -batchmode -nographics -quit -projectPath . -logFile <log>` and grep the log for `error CS`. Never run it while the project is open.
- **Product decisions** go in the issue (and `docs/architecture.md` §6 when they change a contract), plus the prompt log. Ask, don't guess.

## Layout
- `Assets/_Project/` — all our own content (Editor, Scenes, Prefabs, ...; run Magic > Setup > Create Project Folders). Third-party assets stay untouched in `Assets/ThirdParty/<Pack>` (redistributable only) or `Assets/AssetStore/` (local only).
- `docs/` — process, ADRs, prompt log, scrum artifacts.
