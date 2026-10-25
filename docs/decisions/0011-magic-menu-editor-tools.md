# 0011-magic-menu-editor-tools — "Magic" menu, one tool per function
- Status: Accepted (amends 0005)
- Date: 2026-10-25
- Deciders: Product Owner (Henry) with Claude as scrum master

## Context
ADR-0005 put agent-written Editor tools under **Tools > 3DGP**. The PO prefers a short top-level menu and wants tools split by function, because one big setup script that re-runs over every scene would reset work people did by hand.

## Decision
- Editor tools live under the top-level menu **Magic** (`[MenuItem("Magic/...")]`), grouped in submenus: `Magic > Setup`, `Magic > Scenes`, `Magic > Mini-Exercises`, later `Magic > Level`, ...
- Scripts live in `Assets/_Project/Editor/Magic/`, **one file per function**; shared helpers in `MagicMenuUtil.cs`.
- Every command **only creates what is missing**. It never deletes, resets or overwrites existing files, scenes or objects, and it logs what it created and what it skipped (`[Magic] ...`). Commands that edit the open scene use `Undo`.
- Our content stays in `Assets/_Project/` (unchanged).

## Consequences
Running a command twice is safe. Each tool is small enough for a member to read and explain. Old docs and prompt logs that mention Tools > 3DGP are history; current docs and open issues use Magic.
