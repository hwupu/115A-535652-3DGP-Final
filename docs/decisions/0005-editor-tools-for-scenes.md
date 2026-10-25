# 0005-editor-tools-for-scenes — Scene setup via Editor tools
- Status: Accepted; menu name and tool structure amended by [0011](0011-magic-menu-editor-tools.md) (now **Magic**, `Assets/_Project/Editor/Magic/`)
- Date: 2026-10-12
- Deciders: Product Owner (team) with Claude as scrum master

## Context
Humans pick assets and assemble scenes. Agents could generate scenes at runtime, which hides state from the Editor and hurts debugging and merging.

## Decision
Scenes live as saved `.unity` assets authored in the Editor. Agents assist by writing Editor scripts exposed under **Tools > 3DGP** (`Assets/_Project/Editor/`), which humans run to set up/validate scene content (create objects, wire references, validate setup). No gameplay code builds scenes at runtime.

## Consequences
Scene state is visible, diffable and reviewable. Editor tools must be idempotent and undoable (use `Undo`).
