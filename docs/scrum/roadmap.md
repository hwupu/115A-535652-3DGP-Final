# Roadmap & Product Backlog

Source of truth for work items is **GitHub Issues** (labels `type:*`, `level:*`, `area:*`); sprints are **milestones**; the board is [GitHub Project #2](https://github.com/users/hwupu/projects/2). This file is a readable snapshot of the plan, derived from `docs/references/proposal.md` §9 (milestones) and §12 (workload).

Owners and sprint assignments are **provisional** (per the proposal) until confirmed at planning. A sprint ends on the course date shown.

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
| 8 | Dec 14 | Wk15 | Demo 2: feature-complete three-level build |
| 9 | Dec 28 | Wk16 | Final Demo: stable packaged build, presentation and evidence |

## Backlog by sprint

### Sprint 1 — Shared scene architecture, danger-state contract, level greyboxing starts (due Oct 26)
- [#6](https://github.com/hwupu/115A-535652-3DGP-Final/issues/6) Shared contracts: ball, shot, score, camera, danger interfaces — Henry
- [#7](https://github.com/hwupu/115A-535652-3DGP-Final/issues/7) DangerManager: time bar model and Normal/Warning/Critical/Failed states — Pongpat
- [#8](https://github.com/hwupu/115A-535652-3DGP-Final/issues/8) Editor tool: Tools > 3DGP > Create Level Scene Skeleton — Henry
- [#9](https://github.com/hwupu/115A-535652-3DGP-Final/issues/9) Greybox Level 1 - Sunny Garden scene — Henry
- [#10](https://github.com/hwupu/115A-535652-3DGP-Final/issues/10) Greybox Level 2 - Windy Desert scene — Bahadir
- [#11](https://github.com/hwupu/115A-535652-3DGP-Final/issues/11) Greybox Level 3 - Frozen Lake scene — Pongpat
- [#12](https://github.com/hwupu/115A-535652-3DGP-Final/issues/12) Choose placeholder asset sources and record licenses — Henry

### Sprint 2 — Ball physics, club profiles, camera MVP (due Nov 2)
- [#13](https://github.com/hwupu/115A-535652-3DGP-Final/issues/13) Input System actions for aim, power, club switch, view switch — Henry
- [#14](https://github.com/hwupu/115A-535652-3DGP-Final/issues/14) BallController: Rigidbody ball, hit impulse, rest detection, reset — Henry
- [#15](https://github.com/hwupu/115A-535652-3DGP-Final/issues/15) Shot sequence: yaw aim + cycling power bar — Henry
- [#16](https://github.com/hwupu/115A-535652-3DGP-Final/issues/16) Club profiles (Power / Control / Accuracy) and club selection — Henry
- [#17](https://github.com/hwupu/115A-535652-3DGP-Final/issues/17) Surface physics materials and terrain presets — Pongpat
- [#18](https://github.com/hwupu/115A-535652-3DGP-Final/issues/18) Cup/hole detection, stroke counter, level complete event — Pongpat
- [#19](https://github.com/hwupu/115A-535652-3DGP-Final/issues/19) Camera MVP: aim view, ball-follow view, smooth transitions — Bahadir

### Sprint 3 — Three scenes; Warning/Critical triggers and puzzle prototypes (due Nov 9)
- [#20](https://github.com/hwupu/115A-535652-3DGP-Final/issues/20) HUD: time progress bar, Warning/Critical icon, power bar, stroke count — Henry
- [#21](https://github.com/hwupu/115A-535652-3DGP-Final/issues/21) L1: Cat alert state machine and noise gauge — Henry
- [#22](https://github.com/hwupu/115A-535652-3DGP-Final/issues/22) L1: Branch/lever trigger and water-splash trap prototype — Henry
- [#23](https://github.com/hwupu/115A-535652-3DGP-Final/issues/23) L2: WindController and wind compass — Bahadir
- [#24](https://github.com/hwupu/115A-535652-3DGP-Final/issues/24) L2: Scarab patrol prototype and target-switch gate — Bahadir
- [#25](https://github.com/hwupu/115A-535652-3DGP-Final/issues/25) L3: Ice friction zones and ricochet walls — Pongpat
- [#26](https://github.com/hwupu/115A-535652-3DGP-Final/issues/26) L3: Puzzle state manager (switch chain) and clockwork guardian prototype — Pongpat

### Sprint 4 — DEMO 1: playable loop + three distinct greybox levels (due Nov 16)
- [#27](https://github.com/hwupu/115A-535652-3DGP-Final/issues/27) Level flow: retry, level complete, scene-selection menu — Pongpat
- [#28](https://github.com/hwupu/115A-535652-3DGP-Final/issues/28) Tutorial UI prompts for Level 1 — Henry
- [#29](https://github.com/hwupu/115A-535652-3DGP-Final/issues/29) DEMO 1 integration build and rehearsal (Nov 16) — Bahadir
- [#30](https://github.com/hwupu/115A-535652-3DGP-Final/issues/30) Main menu and basic game flow — Henry

### Sprint 5 — Wind/ice/alert escalation and distinct level puzzle integration (due Nov 23)
- [#31](https://github.com/hwupu/115A-535652-3DGP-Final/issues/31) L1: Danger escalation - cat wakes, searches near trap, escapes — Henry
- [#32](https://github.com/hwupu/115A-535652-3DGP-Final/issues/32) L2: Danger escalation - sandstorm, gust strength, visibility — Bahadir
- [#33](https://github.com/hwupu/115A-535652-3DGP-Final/issues/33) L3: Danger escalation - ice cracks and slippery danger zones — Pongpat
- [#34](https://github.com/hwupu/115A-535652-3DGP-Final/issues/34) L2: Navigation and A* path finding for scarabs — Bahadir
- [#35](https://github.com/hwupu/115A-535652-3DGP-Final/issues/35) Integrate distinct level puzzles end to end — Pongpat

### Sprint 6 — FX, animation, sound, scoring, polish (due Nov 30)
- [#36](https://github.com/hwupu/115A-535652-3DGP-Final/issues/36) Hamster hierarchical animation (idle, aim, swing) — Henry
- [#37](https://github.com/hwupu/115A-535652-3DGP-Final/issues/37) Cat, scarab and clockwork guardian animations — Bahadir
- [#38](https://github.com/hwupu/115A-535652-3DGP-Final/issues/38) Garden gate portal and level transition — Henry
- [#39](https://github.com/hwupu/115A-535652-3DGP-Final/issues/39) Particle effects: impact sparks, confetti, water splash — Henry
- [#40](https://github.com/hwupu/115A-535652-3DGP-Final/issues/40) Particle effects: sand gusts and sandstorm — Bahadir
- [#41](https://github.com/hwupu/115A-535652-3DGP-Final/issues/41) Particle effects: icy skid, snow puffs, crack decals — Pongpat
- [#42](https://github.com/hwupu/115A-535652-3DGP-Final/issues/42) Audio: shots, collisions, ambience, danger cues (3D spatial) — Bahadir
- [#43](https://github.com/hwupu/115A-535652-3DGP-Final/issues/43) Scoring and results screen — Pongpat
- [#44](https://github.com/hwupu/115A-535652-3DGP-Final/issues/44) Save & Load progress — Pongpat
- [#45](https://github.com/hwupu/115A-535652-3DGP-Final/issues/45) Environmental effect on character (wind/ice affects hamster and ball) — Bahadir

### Sprint 7 — Integration; QA (due Dec 7)
- [#46](https://github.com/hwupu/115A-535652-3DGP-Final/issues/46) Balance and tuning pass: time budgets and shot costs — Pongpat
- [#47](https://github.com/hwupu/115A-535652-3DGP-Final/issues/47) Accessibility: HUD contrast, non-color-only hazards, volume — Henry
- [#48](https://github.com/hwupu/115A-535652-3DGP-Final/issues/48) Full playthrough QA on Windows and Mac — Bahadir
- [#49](https://github.com/hwupu/115A-535652-3DGP-Final/issues/49) Performance profiling pass — Bahadir

### Sprint 8 — Demo 2: feature-complete three-level build (due Dec 14)
- [#50](https://github.com/hwupu/115A-535652-3DGP-Final/issues/50) Ending sequence: princess rescue and comic distraction — Pongpat
- [#51](https://github.com/hwupu/115A-535652-3DGP-Final/issues/51) Story intro and between-level text — Henry
- [#52](https://github.com/hwupu/115A-535652-3DGP-Final/issues/52) Feature freeze and DEMO 2 build (Dec 14) — Bahadir

### Sprint 9 — Final Demo: stable packaged build, presentation and evidence (due Dec 28)
- [#53](https://github.com/hwupu/115A-535652-3DGP-Final/issues/53) Final stable build: Windows (primary) and Mac packages — Bahadir
- [#54](https://github.com/hwupu/115A-535652-3DGP-Final/issues/54) Final presentation, evidence and programming explanations — Henry

## Course mini-project coverage (proposal §12)
4.A.1 hierarchical animation, 4.A.2 navigation, 4.A.3 portals, 4.B.4 A*, 4.B.2 AI characters, 4.C.4 object interaction, 4.D.1 environmental effects, 4.D.3 particles, 4.E.1 save/load, 4.E.2 function selection, 4.E.3 game UI — each is referenced in the notes of the matching issue.

## Open questions for the PO (`needs-po`)
- Club set and names (default: standard, iron, sledgehammer) and shot time costs.
- Meaning/scope of course item 4.E.2 "Function selection" and 4.B.4 A* requirement wording — confirm with the instructor.
- Asset sources per level (issue linked from Sprint 1).
- Confirm owners and sprint placement at the Sprint 1 planning.
