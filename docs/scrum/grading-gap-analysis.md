# Grading gap analysis (2026-10-25)

This compares `docs/references/proposal.md`, `docs/scrum/roadmap.md` and the backlog with the instructor's requirements in `docs/references/course/`. The PO decided on 2026-10-25 (ADR-0010). The resolutions are in the table below, and the backlog has been updated. The tracker is #84. The sections after the table are the original analysis.

## Resolutions (2026-10-25)
| Gap | Decision | Issue(s) |
|---|---|---|
| C1 AI vs "on your own" | Keep the AI-native workflow; each member refines the code by hand afterwards and does their own 4 exercises | ADR-0010, `docs/mini-exercises.md` |
| C2 two cat monsters | The L3 guardian must not be a cat; Pongpat decides | #26, #37 |
| C3 character leveling | Start with the standard club; unlock other clubs and skills by leveling; collectibles give XP | #70, #16 |
| C4 NPC skills | Non-violent skills noted per NPC (team target ≥ 5) | #21, #24, #26 |
| C5 numbering / simulation | 4.B.1 numbering fixed in issues; simulation and composition are optional | #34, #81 |
| C6 video length | Ignored for now | — |
| C7 proposal says packaged build | Unchanged (team-owned proposal) | — |
| C8 god keys | Added | #71 |
| Particles ≥ 5 per member | Per-level FX stories updated | #39, #40, #41 |
| Sounds ≥ 4 per member | Audio split per owner | #42, #72, #73 |
| ≥ 100 objects, textures, lighting, scene effects | Art pass per level | #9–#11, #74, #75, #76 |
| Misc items ≥ 5 | Settings, character selection, pause | #77 |
| GUI ≥ 5 | XP bar, minimap, club highlight | #78 |
| FSMs ≥ 5 × ≥ 4 states | Inventory and diagrams | #79 |
| Own artwork | ≥ 1 per member | #80 |
| Mouse + keyboard | Added to input story | #13 |
| LOC per member | Report tool | #82 |
| Report and evidence | Story | #83 |
| Slides and presentation rules | Added to presentation story | #54 |
| Mini-exercises | Proposal §12 assignment; standalone scenes, generic Core code; demos Nov 16 and Dec 21 | #57–#68, #69 |
| Our own Form B "Others" items | Proposed in the tracker | #84 |
| Walk mode | Not adopted | — |
| pptx template, title, Group ID | Ignored here (handled by the team) | — |

Legend: ✅ covered · ⚠️ partly / at risk · ❌ missing · ⛔ conflict

## 1. Conflicts

| # | Conflict | Where | Suggested handling |
|---|---|---|---|
| C1 ⛔ | Mini-exercises must be implemented **"on your own"** and the code **"written on your own"**, with live modification at TA request. Our workflow is AI-native (ADR-0001, proposal §2). | mini-exercises.md §Rules | **PO:** confirm with the instructor/TA whether AI-assisted code is acceptable for mini-exercises. Until then, agents act as **tutor/reviewer only** for mini-exercise code (explain, review, debug), and each member writes their own. This also protects the live-modification check. |
| C2 ⛔ | Form B: **"monsters must be totally different in different levels"**. L1 has a sleeping **cat** and L3 a clockwork **cat** guardian. | proposal §7 | Make the L3 guardian a non-cat (e.g., a snow owl, polar bear or clockwork penguin). **PO** |
| C3 ⛔ | Form B "Character leveling: **≥ 5 levels, ≥ 5 skills**" (6 pts). The proposal has no character progression. | proposal §6 | Add hamster XP/level (from par, collectibles and time left) that unlocks shot skills. Clubs and special shots (standard, iron, sledgehammer, lob/jump shot, curve, backspin, ...) can count as skills. **PO** |
| C4 ⛔ | Form B "Monster/NPC skills **≥ 5**". The proposal says scarabs "block but do not attack" and the game is non-violent. | proposal §7 | Skills don't have to be attacks. Example NPC skills: cat wakes, searches and swipes the ball back; scarab patrols, burrows, pushes the ball; guardian freezes a zone and turns walls. Count across all levels. **PO** to confirm tone. |
| C5 ⛔ | Mini-exercise catalogue numbering in the proposal is wrong: "4.B.4 A\*" and "4.B.a" should be **4.B.1**. Proposal §12 also says mass-spring and crowd steering are "out of scope", but Form B gives **+2 per self-implemented simulation technique**. | proposal §12, roadmap | Fix the numbering. Reconsider simulation exercises (see §4): they are cheap extra points and some fit the game. |
| C6 ⚠️ | Teaser video length: the instruction says the uploaded video is **at least 3–5 min**; Form A says the teaser in the slides is **1–3 min**. | instruction vs Form A | Plan two cuts: a 1–3 min teaser for the slides and a 3–5+ min gameplay video for E3. **PO** may ask the TA. |
| C7 ⚠️ | The proposal still says "stable packaged build" (§9). ADR-0009 says Editor-only demos. | proposal §9 | Update the proposal text when the team next edits it (team-owned). |
| C8 ⚠️ | The live demo must **finish all levels** and show **every** particle, sound and Form B item. The danger timer and difficulty make that risky on stage. | Form A, instruction | Add **god/debug keys** (skip level, refill timer, trigger each FX, spawn each NPC state). Not in the backlog. |

## 2. Missing or under-specified for Form B

| Form B item (above average) | Current plan | Status | Gap / idea |
|---|---|---|---|
| 1 level per member, all different | 3 levels, 3 owners | ✅ / ⚠️ | Monster overlap (C2) |
| Team LOC 800+ | Easily | ✅ | |
| Character leveling 5 levels / 5 skills | None | ❌ | C3 |
| Scene ≥ 100 objects | Not specified | ⚠️ | Add "≥ 100 objects per level" to each greybox/dress story's acceptance criteria (loop-placed props allowed) |
| Particles ≥ 5 **per member** (15+) | ~7 listed, uneven | ⚠️ | Each owner lists 5 in their level (e.g., Garden: impact sparks, water splash, leaves, confetti, cat-alert "Zzz"/"!"). One FX story per member. |
| Sounds ≥ 4 **per member** (12+) | All audio is one story (#42, Bahadir) | ⚠️ | Split audio per owner, ≥ 4 each |
| Full texture mapping | Greybox, assets TBD (#12) | ⚠️ | Art-pass story per level, "all surfaces textured" in the DoD |
| Misc items ≥ 5 | Menu, scene select, save/load | ⚠️ | Add **settings** (volume, brightness), **character selection** (hamster skins/variants), **pause menu**, shop/club upgrade or **item composition** (see Enhancement) |
| GUI ≥ 5 on screen | Time bar, danger icon, power bar, strokes, wind compass, noise gauge | ✅ | Add **XP/level bar** and **minimap** (also counts for 4.E.3) |
| FSMs ≥ 5, each ≥ 4 states | Danger (4), cat, shot sequence, puzzle, guardian | ⚠️ | Make them explicit with diagrams: GameFlow, Shot, Danger, Cat, Scarab, Guardian, Puzzle. Ensure **≥ 4 states each**. |
| NPC skills ≥ 5 | Few, non-aggressive | ❌ | C4 |
| Env interaction ≥ 5 ways | Lever/branch, water trap, target switch, wind, ice zones, gate | ✅ / ⚠️ | Add **item collection** (seeds/acorns) and a **portal**. Both are explicitly listed by the rubric. |
| Mouse + keyboard | Input System, not specified | ⚠️ | Story #13: mouse aim/camera + keyboard explicitly |
| Enhancement: simulation +2 each (≤ 10) | None planned | ❌ | Steering (scarab group), deformable flag cloth at the cup (wind!), rope/vine mass-spring, our own collision handling |
| Enhancement: own artwork ≥ 3 | Blender MCP mentioned | ⚠️ | Hamster, guardian, club models made by us. Needed in the report per member. |
| Enhancement: lighting ≥ 3 per level | None | ❌ | e.g., Garden: sun, pond spotlights, fireflies; Desert: harsh sun, torch-lit ruins, storm dim; Lake: moonlight, aurora, tower lamp |
| Enhancement: scene effects ≥ 5 | Sandstorm only | ⚠️ | Fog (storm), shadows, skybox per level, day/night or dusk, lens flare (desert sun) |
| Enhancement: item composition ≥ 3 | None | ❌ | Optional: combine collectibles into club upgrades (e.g., 3 acorns → power grip) |
| Individual LOC bonus (≥ 3000 = +10) | — | ⚠️ | Count per member by git author. Consider an Editor tool or script. Mini-exercise code that lives in the repo can count. |

## 3. Missing for presentation and report (Forms A + report)
- ❌ **pptx template** is not among the provided files. **PO:** get it from E3 or the instructor.
- ❌ **Game title / name** and **Group ID** are still blank in the proposal (needed for the zip name and every document).
- ⚠️ **Evidence pipeline**: ≥ 4 screenshots per level (report), ~30 s video per level, particle and sound clips, FSM and architecture diagrams (≥ 15 components for the report, ≥ 10 for the slides), LOC per member, per-member artwork figures, Scrum write-up (≥ 500 words). Suggestion: a sprint-end checklist item "capture evidence" and a `docs/evidence/` index (binaries via LFS, or kept outside git).
- ⚠️ Presentation: **each member ≥ 3 min** (≤ 3 min = −50), each answers ≥ 1 question about the code. Story #54 should cover a rehearsal with timing.
- ✅ Hero's Journey and SWOT already exist in the proposal and can be reused. The story needs to be expanded to ≥ 300 words for the report.

## 4. Mini-exercises: fit with the game

The main gap: most exercises (4.A.2, 4.A.3, 4.B.1, 4.B.2, 4.C.4, 4.D.1–3, 4.E.1) need a **walking, leg-animated, player-controlled character**. In our golf game the player controls a **ball**, and the hamster only stands and swings.

### Proposed approach: standalone mini-exercise scenes that share generic code
As suggested by Henry: every exercise gets **its own demo scene**. The reusable logic is written **generic** (no golf types) so the game can use it later.

```
Assets/_Project/
  Runtime/Core/            # generic, reusable (asmdef ThreeDGP.Core): steering, mass-spring, save system, FSM base, portal, ...
  Runtime/Game/            # golf game code (asmdef ThreeDGP.Game -> refs Core)
  MiniExercises/<Member>/<Exercise>/   # scripts (asmdef ThreeDGP.MiniEx.<Member> -> refs Core only)
  Scenes/MiniExercises/<Member>_<Exercise>.unity
```
- Assembly definitions enforce the direction: Game and MiniEx depend on Core, **never on each other**. A mini-exercise scene still runs if the game is broken, and the reverse holds too.
- Exercise-specific demo glue (keys, spawn loops, the "≥ 20/50/100 objects" requirement) stays in MiniExercises. Only the reusable technique moves to Core.
- Scenes are authored in the Editor (CLAUDE.md rule 3). Spawning many objects with a loop at runtime **is part of the exercise requirement**, so it is allowed inside mini-exercise scenes. This needs a short ADR because it is an exception to "no runtime scene building".
- Each member's mini-exercise code is committed under their own git author, which helps the LOC count and the "on your own" evidence.

### Candidate exercises that also feed the game (to choose at planning; **PO**)
| Exercise | Game reuse | Form B items it supports |
|---|---|---|
| 4.A.1 Hierarchical model | Hamster arm + club swing, or a **mini-golf windmill/rotating obstacle** (A rotates x, B rotates y, C bobs on sin) | Animation, env interaction |
| 4.C.3 Deformable surfaces | **Hole flags / desert banners in the wind** (2 cloths of different sizes on poles) | Simulation +2, scene effects |
| 4.C.1 Mass-spring | Hanging vines (Garden), rope bridge, icicles | Simulation +2 |
| 4.C.2 Group steering | **Scarab swarm** (Desert), fish under the ice, birds | Simulation +2, NPC skills |
| 4.B.1 A\* | Scarab/guardian route finding toward the ball | NPC skills, FSM |
| 4.D.3 / 4.D.2 Particle + sound effects | Shot, impact, collect, splash FX + SFX | Particles and sounds per member |
| 4.E.1 Save & Load | Progress and mid-level save (ball pos, NPC states) | Misc item |
| 4.E.2 Function selection | Main menu: Play / Settings / Character select / Demo video | Misc items (several at once) |
| 4.E.3 Game UI | HUD with XP bar, minimap, club selection with highlight FX | GUI ≥ 5 |
| 4.C.4 Object interaction | Ball knocks over ≥ 100 props (e.g., Garden flowerpots, Lake ice blocks) | Simulation (collision), env interaction |
| 4.A.2 / 4.A.3 / 4.D.1 | Only fit if we add a **walk mode** (below) | Env interaction, portal |

**Option (PO decision):** add a short **walk mode** where the hamster walks to the ball between shots or explores to collect items. That would make 4.A.2 navigation, 4.A.3 portals, 4.D.1 speed/jump pads and 4.C.4 pushing obstacles reusable in the game. It also adds item collection, portals and leveling hooks for Form B. Cost: a rigged, walking hamster plus a third-person controller.

The current proposal assignment lists 4 or more exercises per member, but 4.C.4 is shared between Henry and Pongpat. Overlap is allowed (each person's work is individual), but spreading the exercises gives the game more distinct reusable pieces.

## 5. Questions for the PO / instructor
1. **AI use for mini-exercises** (C1). Also, is AI use for the main project acceptable as described in the proposal (was the proposal accepted)?
2. **Mini-exercise demo dates** and the E3 deadline. What were our **assignments**, so we avoid overlap?
3. pptx template, Group ID, game title.
4. Accept C2 (non-cat L3 guardian), C3 (leveling + skills), C4 (non-violent NPC skills)?
5. Walk mode yes or no?
6. Which 4 exercises per member?
7. Teaser video length (C6).
