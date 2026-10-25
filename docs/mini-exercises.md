# Mini-exercises — guide for members and agents

The rules are in [`references/course/mini-exercises.md`](references/course/mini-exercises.md). The decision behind this guide is [ADR-0010](decisions/0010-mini-exercises-and-grading-alignment.md). Each exercise has a GitHub issue labelled `area:mini-exercise`.

## Assignment and schedule
| Member | Mini-1 (Nov 16) | Mini-2 (Nov 16) | Mini-3 (Dec 14) | Mini-4 (Dec 14) |
|---|---|---|---|---|
| Henry (@hwupu) | 4.C.4 Object interaction | 4.E.3 Game UI | 4.A.1 Hierarchical model | 4.A.3 Game portals |
| Bahadir (@bahadir-koksal) | 4.A.2 Character navigation | 4.D.1 Environment effects | 4.B.1 A\* path finding | 4.B.2 AI characters |
| Pongpat (@empongpat) | 4.E.2 Function selection | 4.C.4 Object interaction | 4.D.3 Character particle effects | 4.E.1 Save & Load |

The set per member is fixed (proposal §12). The **order is provisional**: a member may swap which two go first. If they do, update the issue milestones (Sprint 4 = Nov 16 demo, Sprint 8 = Dec 14 demo, together with DEMO 2).

## Scaffolding
**Magic > Mini-Exercises > Create All Planned** creates the folder, README (with the requirement checklist) and demo scene for all 12 planned exercises, plus each member's asmdef. **New Exercise...** creates one (e.g., after a swap). The plan lives in `Assets/_Project/Editor/Magic/MiniExerciseCatalog.cs`; update it if the plan changes.

## Scaffolding
**Magic > Mini-Exercises > Create All Planned** creates the folder, README (with the requirement checklist) and demo scene for all 12 planned exercises, plus each member's asmdef. **New Exercise...** creates one (e.g., after a swap). The plan lives in `Assets/_Project/Editor/Magic/MiniExerciseCatalog.cs`; update it if the plan changes.

## Rules for agents working on a mini-exercise
1. **Work for the assigned member only**, on a branch `feat/<issue>-mini-<code>-<slug>`. The member's git identity commits the work, because LOC is counted per author.
2. **Generate a clear, readable first version, then hand over.** The member refines it by hand and must be able to explain and modify it live for the TAs. Keep the code simple and well-named, avoid cleverness, and add short comments on the technique (the math or algorithm). In the PR, list the "TA modification points": parameters or functions the TA is likely to ask to change.
3. **Self-contained folder and scene.** Each exercise lives in its own folder with its own demo scene and runs on its own:
   ```
   Assets/_Project/MiniExercises/<Member>/<Code>_<Name>/   e.g. Henry/4A1_HierarchicalModel/
     Scripts/   (covered by the member's asmdef Project3DGP.MiniEx.<Member> in MiniExercises/<Member>/, references Project3DGP.Core only)
     Scenes/<Code>_<Name>.unity
     Prefabs/, Materials/ ...
     README.md  (what's new, controls, requirement checklist, references)
   ```
4. **Generic, reusable technique code goes into Core:** `Assets/_Project/Scripts/Core/` (asmdef `Project3DGP.Core`, namespace `Project3DGP.Core.*`). No golf or level types, no scene assumptions, configuration through serialized fields or ScriptableObjects. The game (`Project3DGP.Game`) and the mini-exercises both reference Core, **never each other**.
5. **Meet every requirement number explicitly** (e.g., ≥ 3 portals, ≥ 100 obstacles, ≥ 30 agents) and keep a requirement checklist in the exercise README. Add key bindings where the exercise asks for them, using the Input System.
6. **Loop-spawned objects are allowed** in mini-exercise scenes (exception in ADR-0010). The base environment is still authored in the Editor. Scene setup helpers go under **Magic > Mini-Exercises** (Editor scripts in `Assets/_Project/Editor/Magic/`, not inside the exercise folder, because the member asmdef is runtime-only).
7. **No overlap with assignments**, and mark what's new: the README states which parts are newly developed for this exercise.
8. **Third-party code or samples:** modify them with at least 3 new features, add a self-written loop that creates at least 20 objects, and list the links in the README. Assets follow CLAUDE.md rule 6 (license confirmed, credited).
9. **4.C.4 is done by both Henry and Pongpat.** Their implementations must be separate. Don't let one reuse the other's exercise code.
10. **Real-time performance:** test it in Play mode. Avoid per-frame allocations in simulations.
11. **Export for E3:** the folder must be exportable on its own (e.g., Assets > Export Package of the folder plus Core).
12. When an exercise is done, note in its issue which parts can be reused in the game and link the game story that reuses them.

## Reuse in the game (where the game story lives)
| Exercise | Reuse target |
|---|---|
| Henry 4.C.4 | #22 L1 lever/branch, ball knocking props |
| Henry 4.E.3 | #20 HUD, HUD additions (XP bar, minimap) |
| Henry 4.A.1 | Stays standalone: the game's hamster uses Blender clips (#36). Optional: rotating obstacles |
| Henry 4.A.3 | #38 garden gate portal |
| Bahadir 4.A.2 | Character controller base for 4.D.1 and 4.B.x; #34 |
| Bahadir 4.D.1 | #23 wind, #45 environmental effects |
| Bahadir 4.B.1 | #34 scarab A\* navigation |
| Bahadir 4.B.2 | #24 scarab patrol and chase |
| Pongpat 4.E.2 | #27 / #30 menus, settings and character selection |
| Pongpat 4.C.4 | #26 lake puzzle, ice blocks reacting to the ball |
| Pongpat 4.D.3 | #41 lake particles, FX and sound pairing |
| Pongpat 4.E.1 | #44 save and load |
