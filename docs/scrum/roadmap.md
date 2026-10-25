# Release Plan (master plan) & Product Backlog

**Status: DRAFT for PO review** (2026-10-25). Review it together before Sprint 1 planning. Once accepted, change the status to "Accepted <date>" and record major changes in an ADR.

In Scrum terms this file combines the **Product Goal**, a **release plan** (the dates and targets this term must hit) and a readable snapshot of the **Product Backlog**. The source of truth for work items is still **GitHub Issues** (labels `type:*`, `level:*`, `area:*`). Sprints are **milestones**, and the board is [GitHub Project #2](https://github.com/users/hwupu/projects/2). Inputs: `docs/references/proposal.md` (vision), `docs/references/course/` (grading rules), `docs/scrum/grading-gap-analysis.md`, ADR-0008/0009/0010.

## 1. Product Goal
A complete, single-player hamster fantasy golf game with **three distinct levels** (one per member), played **in the Unity Editor**. It must be finishable in a live demo and must hit **"above average" on every Form B core item**. Alongside it, each member delivers **4 individual mini-exercises** as standalone scenes.

## 2. Release targets (what "done" means for the term)
| Area | Target | Tracked in |
|---|---|---|
| Levels | 3 levels, different scenes **and monsters** (L3 guardian is not a cat) | #9–#11, #26 |
| Form B core items | 6 pts each: leveling 5×5, ≥ 100 objects per level, 5 particles + 4 sounds **per member**, full textures, ≥ 5 misc, ≥ 5 GUI, ≥ 5 FSMs (≥ 4 states each), ≥ 5 NPC skills, ≥ 5 environment interactions, mouse + keyboard | #84 |
| Form B "Others" | **Accepted:** golf shot system, hidden time-budget danger system. Others proposed, to be discussed | #84 |
| Enhancements | Own artwork ≥ 3, lighting ≥ 3 per level, scene effects ≥ 5. Simulation and composition optional | #74–#76, #80, #81 |
| Mini-exercises | 12 total (4 per member), standalone scenes, generic Core code | #57–#68, `docs/mini-exercises.md` |
| Individual LOC | Aim for ≥ 1500 (bonus starts), stretch ≥ 3000 per member | #82 |
| Final deliverables | Slides (template), report PDF (template), video(s), source zip, Forms A/B filled by hand | #54, #83 |

## 3. Key dates
| Date | Event |
|---|---|
| Oct 26 | Sprint 1 ends (contracts + greybox start) |
| **Nov 16** | **Demo 1** (playable loop, 3 greybox levels) **+ mini-exercise demo 1** (mini-1, mini-2 per member) |
| Dec 14 | **Demo 2**: feature-complete, feature freeze |
| **Dec 21** | **Mini-exercise demo 2** (mini-3, mini-4 per member) |
| Dec 28 | **Final presentation and live demo**; E3 upload (deadline TBC) |

## 4. Phases
| Phase | Sprints | Outcome |
|---|---|---|
| A. Foundation | 1–2 | Shared contracts, ball/shot/club/camera, greybox scenes, mini-exercise scaffolding |
| B. Playable | 3–4 | Each level playable with its hazard prototype; Demo 1; first two mini-exercises |
| C. Content | 5–6 | Danger escalation, leveling, puzzles, FX/animation/audio per member |
| D. Complete | 7–8 | Art passes, menus, HUD additions, FSM docs, QA; Demo 2 and feature freeze |
| E. Deliver | 9 | Last two mini-exercises, report, slides, video, rehearsal |

## 5. Workload per member (open issues, provisional)
| Sprint | Henry | Bahadir | Pongpat | Shared |
|---|---|---|---|---|
| 1 | 4 | 1 | 2 | |
| 2 | 5 | 1 | 2 | |
| 3 | 3 | 2 | 2 | |
| 4 | 2 + 2 mini | 2 + 2 mini | 1 + 2 mini | |
| 5 | 2 | 2 | 2 | |
| 6 | 4 | 4 | 4 | |
| 7 | 4 | 3 | 3 | 2 |
| 8 | 1 | 1 | 1 | |
| 9 | 2 + 2 mini | 1 + 2 mini | 2 mini | 1 |
| **Total** | **~31** | **~21** | **~21** | 3 |

## 6. Review points for the PO (before running Sprint 1)
1. **Henry is overloaded** (~10 more issues, mostly shared code in Sprints 1–2). Candidates to move: #13 input → Bahadir (camera owner), #30 main menu → Pongpat (reuses his 4.E.2 #65), #16 or #70 → Bahadir or Pongpat. #69 and #82 are AI-heavy chores.
2. **Sprint 1 ends tomorrow (Oct 26) and hasn't started.** Option: shift sprint goals by one week (re-date milestones) or merge Sprints 1–2. Demo 1 on Nov 16 is fixed.
3. **Sprint 4 is the crunch:** Demo 1 and two mini-exercises per member land on the same day. Suggest starting mini-1 in Sprint 2 and mini-2 in Sprint 3 (milestone stays Sprint 4).
4. **Sprint 9 crunch:** mini-3 and mini-4 (Dec 21) overlap report and slides. Suggest building them in Sprints 6–7, since several reuse game work (#36, #38, #34, #24, #41, #44).
5. Confirm owners, the club/skill list and XP curve (#70), the L3 guardian creature (#26, Pongpat), and the remaining "Others" items (#84).
6. Sprint 0 leftovers: #1 still open; #3 overlaps #12 (close #3 into #12?).
7. Deferred: walk mode (ask Henry), teaser video length, optional enhancements (#81).

## 7. Top risks
| Risk | Mitigation |
|---|---|
| Rubric items missed at grading | Tracker #84 reviewed at each sprint end |
| Live demo fails or can't finish | God keys #71, rehearsals #29 #52 #56 |
| Mini-exercise questions from TAs | Hand refinement and "TA modification points" in every mini PR |
| Uneven LOC or contribution | Rebalance owners (§6.1); LOC report #82 |
| Evidence missing for report and slides | Capture at each sprint end (process.md, #83) |

## 8. Sprint plan
Owners and sprint assignments are **provisional** until confirmed at planning. A sprint ends on the course date shown.

| Sprint | Due | Proposal week | Goal |
|---|---|---|---|
| 0 | Oct 19 | Wk7 | Foundation: setup + proposal ([#1](https://github.com/hwupu/115A-535652-3DGP-Final/issues/1), [#3](https://github.com/hwupu/115A-535652-3DGP-Final/issues/3), [#4](https://github.com/hwupu/115A-535652-3DGP-Final/issues/4)) |
| 1 | Oct 26 | Wk8 | Shared scene architecture, danger-state contract, level greyboxing starts |
| 2 | Nov 2 | Wk9 | Ball physics, club profiles, camera MVP |
| 3 | Nov 9 | Wk10 | Three scenes; Warning/Critical triggers and puzzle prototypes |
| 4 | Nov 16 | Wk11 | DEMO 1: playable loop + three distinct greybox levels |
| 5 | Nov 23 | Wk12 | Wind/ice/alert escalation and distinct level puzzle integration |
| 6 | Nov 30 | Wk13 | FX, animation, sound, scoring, polish |
| 7 | Dec 7 | Wk14 | Integration; QA |
| 8 | Dec 14 | Wk15 | Demo 2: feature-complete three-level game (played in Editor) |
| 9 | Dec 28 | Wk16 | Final Demo: stable Editor demo, presentation and evidence |

## Backlog by sprint

### Sprint 1 - Contracts & greybox start
- [#6](https://github.com/hwupu/115A-535652-3DGP-Final/issues/6) Shared contracts: ball, shot, score, camera, danger interfaces — Henry
- [#7](https://github.com/hwupu/115A-535652-3DGP-Final/issues/7) DangerManager: time bar model and Normal/Warning/Critical/Failed states — Pongpat
- [#8](https://github.com/hwupu/115A-535652-3DGP-Final/issues/8) Editor tool: Magic > Level > Create Level Scene Skeleton — Henry
- [#9](https://github.com/hwupu/115A-535652-3DGP-Final/issues/9) Greybox Level 1 - Sunny Garden scene — Henry
- [#10](https://github.com/hwupu/115A-535652-3DGP-Final/issues/10) Greybox Level 2 - Windy Desert scene — Bahadir
- [#11](https://github.com/hwupu/115A-535652-3DGP-Final/issues/11) Greybox Level 3 - Frozen Lake scene — Pongpat
- [#12](https://github.com/hwupu/115A-535652-3DGP-Final/issues/12) Choose placeholder asset sources and record licenses — Henry
- [#85](https://github.com/hwupu/115A-535652-3DGP-Final/issues/85) Game scenes (MainMenu, L1-L3, Ending) + Build Settings via Magic menu — Henry
- [#69](https://github.com/hwupu/115A-535652-3DGP-Final/issues/69) Mini-exercise scaffolding: Core/Game assemblies + Magic > Mini-Exercises tools (moved from Sprint 2) — Henry

### Sprint 2 - Ball, clubs, camera
- [#13](https://github.com/hwupu/115A-535652-3DGP-Final/issues/13) Input System actions for aim, power, club switch, view switch — Henry
- [#14](https://github.com/hwupu/115A-535652-3DGP-Final/issues/14) BallController: Rigidbody ball, hit impulse, rest detection, reset — Henry
- [#15](https://github.com/hwupu/115A-535652-3DGP-Final/issues/15) Shot sequence: yaw aim + cycling power bar — Henry
- [#16](https://github.com/hwupu/115A-535652-3DGP-Final/issues/16) Club profiles (Power / Control / Accuracy) and club selection — Henry
- [#17](https://github.com/hwupu/115A-535652-3DGP-Final/issues/17) Surface physics materials and terrain presets — Pongpat
- [#18](https://github.com/hwupu/115A-535652-3DGP-Final/issues/18) Cup/hole detection, stroke counter, level complete event — Pongpat
- [#19](https://github.com/hwupu/115A-535652-3DGP-Final/issues/19) Camera MVP: aim view, ball-follow view, smooth transitions — Bahadir

### Sprint 3 - Three scenes & prototypes
- [#20](https://github.com/hwupu/115A-535652-3DGP-Final/issues/20) HUD: time progress bar, Warning/Critical icon, power bar, stroke count — Henry
- [#21](https://github.com/hwupu/115A-535652-3DGP-Final/issues/21) L1: Cat alert state machine and noise gauge — Henry
- [#22](https://github.com/hwupu/115A-535652-3DGP-Final/issues/22) L1: Branch/lever trigger and water-splash trap prototype — Henry
- [#23](https://github.com/hwupu/115A-535652-3DGP-Final/issues/23) L2: WindController and wind compass — Bahadir
- [#24](https://github.com/hwupu/115A-535652-3DGP-Final/issues/24) L2: Scarab patrol prototype and target-switch gate — Bahadir
- [#25](https://github.com/hwupu/115A-535652-3DGP-Final/issues/25) L3: Ice friction zones and ricochet walls — Pongpat
- [#26](https://github.com/hwupu/115A-535652-3DGP-Final/issues/26) L3: Puzzle state manager (switch chain) and clockwork guardian prototype — Pongpat

### Sprint 4 - DEMO 1
- [#27](https://github.com/hwupu/115A-535652-3DGP-Final/issues/27) Level flow: retry, level complete, scene-selection menu — Pongpat
- [#28](https://github.com/hwupu/115A-535652-3DGP-Final/issues/28) Tutorial UI prompts for Level 1 — Henry
- [#29](https://github.com/hwupu/115A-535652-3DGP-Final/issues/29) DEMO 1 integration and in-Editor rehearsal (Nov 16) — Bahadir
- [#30](https://github.com/hwupu/115A-535652-3DGP-Final/issues/30) Main menu and basic game flow — Henry
- [#71](https://github.com/hwupu/115A-535652-3DGP-Final/issues/71) God/debug keys for live demos — Bahadir
- Mini-exercise demo **Nov 16** (mini-1, mini-2): [#57](https://github.com/hwupu/115A-535652-3DGP-Final/issues/57) Henry 4.C.4, [#58](https://github.com/hwupu/115A-535652-3DGP-Final/issues/58) Henry 4.E.3, [#61](https://github.com/hwupu/115A-535652-3DGP-Final/issues/61) Bahadir 4.A.2, [#62](https://github.com/hwupu/115A-535652-3DGP-Final/issues/62) Bahadir 4.D.1, [#65](https://github.com/hwupu/115A-535652-3DGP-Final/issues/65) Pongpat 4.E.2, [#66](https://github.com/hwupu/115A-535652-3DGP-Final/issues/66) Pongpat 4.C.4

### Sprint 5 - Escalation & puzzles
- [#31](https://github.com/hwupu/115A-535652-3DGP-Final/issues/31) L1: Danger escalation - cat wakes, searches near trap, escapes — Henry
- [#32](https://github.com/hwupu/115A-535652-3DGP-Final/issues/32) L2: Danger escalation - sandstorm, gust strength, visibility — Bahadir
- [#33](https://github.com/hwupu/115A-535652-3DGP-Final/issues/33) L3: Danger escalation - ice cracks and slippery danger zones — Pongpat
- [#34](https://github.com/hwupu/115A-535652-3DGP-Final/issues/34) L2: Navigation and A* path finding for scarabs — Bahadir
- [#35](https://github.com/hwupu/115A-535652-3DGP-Final/issues/35) Integrate distinct level puzzles end to end — Pongpat
- [#70](https://github.com/hwupu/115A-535652-3DGP-Final/issues/70) Character leveling: XP, >= 5 levels, club/skill unlocks — Henry

### Sprint 6 - FX, anim, sound, polish
- [#36](https://github.com/hwupu/115A-535652-3DGP-Final/issues/36) Hamster hierarchical animation (idle, aim, swing) — Henry
- [#37](https://github.com/hwupu/115A-535652-3DGP-Final/issues/37) Cat, scarab and clockwork guardian animations — Bahadir
- [#38](https://github.com/hwupu/115A-535652-3DGP-Final/issues/38) Garden gate portal and level transition — Henry
- [#39](https://github.com/hwupu/115A-535652-3DGP-Final/issues/39) Particle effects: impact sparks, confetti, water splash — Henry
- [#40](https://github.com/hwupu/115A-535652-3DGP-Final/issues/40) Particle effects: sand gusts and sandstorm — Bahadir
- [#41](https://github.com/hwupu/115A-535652-3DGP-Final/issues/41) Particle effects: icy skid, snow puffs, crack decals — Pongpat
- [#42](https://github.com/hwupu/115A-535652-3DGP-Final/issues/42) Audio: shared audio manager (3D spatial) + Windy Desert SFX (>= 4) — Bahadir
- [#43](https://github.com/hwupu/115A-535652-3DGP-Final/issues/43) Scoring and results screen — Pongpat
- [#44](https://github.com/hwupu/115A-535652-3DGP-Final/issues/44) Save & Load progress — Pongpat
- [#45](https://github.com/hwupu/115A-535652-3DGP-Final/issues/45) Environmental effect on character (wind/ice affects hamster and ball) — Bahadir
- [#72](https://github.com/hwupu/115A-535652-3DGP-Final/issues/72) Audio: Sunny Garden SFX (>= 4) — Henry
- [#73](https://github.com/hwupu/115A-535652-3DGP-Final/issues/73) Audio: Frozen Lake SFX (>= 4) — Pongpat

### Sprint 7 - Integration & QA
- [#46](https://github.com/hwupu/115A-535652-3DGP-Final/issues/46) Balance and tuning pass: time budgets and shot costs — Pongpat
- [#47](https://github.com/hwupu/115A-535652-3DGP-Final/issues/47) Accessibility: HUD contrast, non-color-only hazards, volume — Henry
- [#48](https://github.com/hwupu/115A-535652-3DGP-Final/issues/48) Full playthrough QA in the Unity Editor — Bahadir
- [#49](https://github.com/hwupu/115A-535652-3DGP-Final/issues/49) Performance profiling pass (Editor Play mode) — Bahadir
- [#74](https://github.com/hwupu/115A-535652-3DGP-Final/issues/74) Art pass: Sunny Garden — Henry
- [#75](https://github.com/hwupu/115A-535652-3DGP-Final/issues/75) Art pass: Windy Desert — Bahadir
- [#76](https://github.com/hwupu/115A-535652-3DGP-Final/issues/76) Art pass: Frozen Lake — Pongpat
- [#77](https://github.com/hwupu/115A-535652-3DGP-Final/issues/77) Misc menu items: settings, character selection, pause — Pongpat
- [#78](https://github.com/hwupu/115A-535652-3DGP-Final/issues/78) HUD additions: XP bar, minimap, club highlight — Henry
- [#79](https://github.com/hwupu/115A-535652-3DGP-Final/issues/79) FSM inventory and diagrams — all
- [#80](https://github.com/hwupu/115A-535652-3DGP-Final/issues/80) Manual artwork (>= 1 per member) — all
- [#82](https://github.com/hwupu/115A-535652-3DGP-Final/issues/82) LOC report per member — Henry

### Sprint 8 - DEMO 2
- [#50](https://github.com/hwupu/115A-535652-3DGP-Final/issues/50) Ending sequence: princess rescue and comic distraction — Pongpat
- [#51](https://github.com/hwupu/115A-535652-3DGP-Final/issues/51) Story intro and between-level text — Henry
- [#52](https://github.com/hwupu/115A-535652-3DGP-Final/issues/52) Feature freeze and DEMO 2 in-Editor rehearsal (Dec 14) — Bahadir

### Sprint 9 - Final Demo
- [#54](https://github.com/hwupu/115A-535652-3DGP-Final/issues/54) Final presentation, evidence and programming explanations — Henry
- [#56](https://github.com/hwupu/115A-535652-3DGP-Final/issues/56) Final demo rehearsal from a clean clone (Editor) — Bahadir
- [#83](https://github.com/hwupu/115A-535652-3DGP-Final/issues/83) Final report (template) and evidence — all
- [#84](https://github.com/hwupu/115A-535652-3DGP-Final/issues/84) Form B coverage tracker and proposed 'Others' items — Henry
- Mini-exercise demo **Dec 21** (mini-3, mini-4): [#59](https://github.com/hwupu/115A-535652-3DGP-Final/issues/59) Henry 4.A.1, [#60](https://github.com/hwupu/115A-535652-3DGP-Final/issues/60) Henry 4.A.3, [#63](https://github.com/hwupu/115A-535652-3DGP-Final/issues/63) Bahadir 4.B.1, [#64](https://github.com/hwupu/115A-535652-3DGP-Final/issues/64) Bahadir 4.B.2, [#67](https://github.com/hwupu/115A-535652-3DGP-Final/issues/67) Pongpat 4.D.3, [#68](https://github.com/hwupu/115A-535652-3DGP-Final/issues/68) Pongpat 4.E.1

## Course mini-exercises (ADR-0010)
Individual, 4 per member, each in its own standalone scene with generic code in Core (`docs/mini-exercises.md`). Henry: 4.C.4, 4.E.3, 4.A.1, 4.A.3 · Bahadir: 4.A.2, 4.D.1, 4.B.1, 4.B.2 · Pongpat: 4.E.2, 4.C.4, 4.D.3, 4.E.1. Demos Nov 16 (first two) and Dec 21 (last two).

## Not scheduled
- [#81](https://github.com/hwupu/115A-535652-3DGP-Final/issues/81) Enhancement points: simulation techniques and item composition (needs-po)

## Open questions for the PO (`needs-po`)
- Club/skill list and XP curve for leveling (#70; start with the standard club) and shot time costs.
- Asset sources per level (issue linked from Sprint 1).
- Confirm owners and sprint placement at the Sprint 1 planning.
