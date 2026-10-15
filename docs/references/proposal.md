# **3D Game Programming: Project Proposal**

2026 Fall  
Members:

* Henry (吳普軒) 314553047  
* Bahadir Emre Koksal (柯昊瀚) 314540047  
* Pongpat Weesommai (余蓬勃) 310540028

Game title: \*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*\*  
Group ID: xxxx  
Instructor: Sai-Keung Wong

1. # **Introduction**

Game name is a single-player 3D fantasy golf game with an underlying hamster kingdom storyline. Instead of a conventional golf tournament, the player guides a courageous hamster through self-contained courses in order to rescue the scared hamster princess kidnapped by Sir Grumblepaws. Each level combines a golf objective (complete the hole using as few strokes as possible before the Sir Grumblepaws wakes up) with a distinct environmental puzzle, enemy or hazard, and a colorful 3D setting.  
The project emphasizes 3D programming: Rigidbody-based \[4\] ball movement and collision, camera transitions, hierarchical character animation, environment-dependent physics, simple character AI, interactive triggers, particle effects, and 3D spatial audio \[6\]. The scope is intentionally limited to three playable levels initially, a single-player game loop, and few selectable golf clubs to match the course schedule.

2. # **AI Tools**

The team plans to adopt the AI-native development workflow described in its original draft. Claude Code will assist with brainstorming, backlog organization, sprint planning, code scaffolding, debugging, and documentation. Where available and compatible, Blender MCP \[10\] workflows will help with 3D model setup, materials, rigging and animation; Unity CLI integrations will assist with compile-error inspection and repeatable editor tasks.  
AI agents may support Scrum facilitation and coding, but all project decisions, asset rights, merged code, testing, and deliverable quality remain the responsibility of the three human team members. AI-generated scripts must be reviewed, run in Unity, tested against acceptance criteria, and committed with human-authored explanations. Prompts, material AI contributions, and validation results will be documented in associated GitHub issues or pull requests. All assets must comply with its license and be properly referenced.

3. # **Game Story**

In the cheerful Hamster Kingdom, the curious princess is taken away by a mischievous Sir Grumblepaws, who leaves behind a trail of elaborate golf-course puzzles. A young hamster inventor sets out to rescue her, carrying a magical golf ball and a small set of unusual clubs. Rather than fighting enemies, the hero uses accuracy, clever angles and harmless environmental contraptions to distract guardians, open passages and recover clues.  
The journey begins in a sunny garden, continues through a wind-swept desert, and ends at a frozen lake beside the cat’s hideout. The princess occasionally calls out encouragingly as the hero approaches. In the finale, the hamster completes the decisive shot and activates a comic distraction that lets the princess escape safely. Sir Grumblepaws learns to cooperate with the hamsters, reinforcing the themes of courage, creativity, kindness, and perseverance. However, be careful. Missing the stage’s time limit allowance or making too much noise might wake the Sir Grumblepaws. Who would want to scare the princess hamster at the end, right?

4. # **Hero Journey**

| Hero’s Journey phase | Story and gameplay connection |
| :---- | :---- |
| Ordinary World / Call to Adventure | A peaceful Hamster Kingdom is disrupted when the princess disappears; the hero sees the first trail of golf puzzles. |
| Refusal / Mentor / Aid | The inexperienced hero hesitates, then learns aiming, shot timing and safe environmental interactions through on-screen tutorial guidance. |
| Crossing the Threshold | Completing the Sunny Garden course opens the first passage and commits the hero to the rescue. |
| Tests, Allies and Enemies | The Windy Desert tests adaptation to wind and moving hazards; each second passing and each shot heightens the danger as the princess encourages the player. |
| Ordeal / Reward | The Frozen Lake requires planned ricochets and timing around the final guardian to reach the rescue mechanism. |
| Return with the Elixir | The hero returns with the princess and the knowledge that patience and nonviolent problem-solving can overcome obstacles. |

5. # **System Requirement** \[3\]

| OS | OS Version | CPU | Graphic API |
| :---- | :---- | :---- | :---- |
| Windows | Windows 10 version 21H1 or newer | X64 architecture with SSE2 | Vulkan capable GPUs |
| macOS | Ventura 13 or newer | Apple M1 or above | Metal-capable |

Table 5.1: System requirement table from Unity Documentation

6. # **Game Play**

   1. Core gameplay loop \[7\]:  
* The player chooses a shooting club to shoot the ball, inspects the 3D environments, realizes the hazards like traps, enemies, or wind direction.  
* The player can switch between views, start shooting sequences with choosing yaw angle and power to shoot the ball. Pitch angle selection will be left as an optional addition to the game. For now, it will be kept fixed.  
* In the shooting sequence, there will be three elements playing role: “power” will decide the maximum speed a player can shoot, “control” will affect how easy it will be to choose desired power by changing the cycling cursor speed on the power bar, lastly, “accuracy” will be the element affecting the yaw angle randomization range which player will not be in control of selecting a fixed angle.  
* Ball motion will be simulated with Unity's built-in physics engine. There will be different settings in different materials/terrains. Examples could include but are not limited to friction and surface roughness. There will be level specific scenarios in the design.  
* There will be progressive bar displayed on top which will be proceeding continuously with time and with discrete jumps after each shot. Before the progressive bar runs out of space, the player will play for the winning scenario.  
  2. Losing scenario and danger system:  
     The losing scenario for a game level will be based on a bar shown to the player on top of the UI. The bar will be gradually filling over time where a fully filled bar representing Sir Grumblepaws is woken up and level is lost. Each shot to the ball by player will trigger discrete jumps on this time bar. On the background (not shown to the player), the bar will designate a total time to finish that level where each shot costs a specific amount of time unit to be used which creates the discrete jumps on the time bar.		  
  3. Shooting club differentiation:  
     The player will be given the option to use shooting tools other than a standard golf club. One of the options instead of a golf club will be hitting the ball with a sledgehammer. These different tools will change the shooting dynamics over 3 previously explained parameters: Power, Control, and Accuracy. In this scenario, sledgehammer shot will be delivering the ball to further distances while lacking the desired aiming capabilities.

7. # **Game Levels**

The three stages are deliberately distinct in setting, active obstacles, core mechanic and objective, satisfying the course requirement that different levels cannot be only re-skins \[1\]. Each member owns one self-contained level scene and its level-specific scripts. The assignments below are provisional until the team decides to make updates.

## **Level 1 \- Sunny Garden: The Sleeping Cat (Henry)**

Environment: warm garden, grass, flowers, a tree with a weakened branch, a small pond, and tutorial signs. Primary challenge: a sleeping cat and a noise-awareness gauge. Player objective: learn shot timing, use a precise ball strike to activate a harmless water-splash distraction near the cat, then complete the cup and unlock the garden gate. Unique mechanic: ball-triggered lever/branch interaction and stealth-like noise meter. Over the time escalation: on “warning” the cat twitches, opens its eyes and becomes suspicious; on “critical” phase, it fully awakens and searches near the trap. If the status bar is filled fully, it escapes with the princess and the scene resets on Retry. A loud Sledgehammer shot can raise the cat’s noise gauge, creating risk/reward without automatically ending the attempt. Programming focus: tutorial UI, interaction triggers, alert-state animations, and the water-splash trap.

## **Level 2 \- Windy Desert: The Scarab Pass (Bahadir)**

Environment: sand dunes, canyon walls, ancient stone arches and moving sand. Primary challenge: desert scarab guardians that patrol the route, intermittent gusts, and a gate activated by a target switch. Player objective: control the ball under changing wind and strike a switch to open the pass before sinking the cup. Unique mechanic: directional wind force applied to an airborne ball, plus moving obstacles/AI patrol timing. The scarabs block shots but do not attack the hero. Over the time escalation: on “warning” a sandstorm begins; on “critical” phase gusts strengthen and visibility drops while the cup remains reachable. If the status bar is filled fully, the storm closes the path and the player retries. Programming focus: wind controller, danger-driven weather stages, enemy navigation/path behavior, and environmental particles \[5\].

## **Level 3 \- Frozen Lake: The Last Ricochet (Pongpat)**

Environment: frozen lake, ice shelves, snowy trees and a small tower where the princess is held. Primary challenge: low-friction ice, reflective walls, and a clockwork cat guardian that guards a rescue switch. Player objective: use a bank shot to reach the switch, safely distract the guardian, then complete the final hole to free the princess. Unique mechanic: slippery terrain, deliberate ricochets and a timed multi-trigger puzzle; it is not simply a windy version of Level 2\. Over the time escalation: on “warning” visible cracks spread across the ice; on “critical” phase selected surface regions become slippery or hazardous but the final shot remains possible. If the status bar is filled fully, the rescue route collapses and the player retries. Programming focus: physics materials, danger-driven ice states, puzzle state manager, guardian behavior, ending sequence and save/level completion.

| Criterion | Sunny Garden | Windy Desert | Frozen Lake |
| :---- | :---- | :---- | :---- |
| Distinct world | Grass / tree / pond | Dunes / canyon / ruins | Ice / snow / tower |
| Primary obstacle | Sleeping cat / alert | Patrolling scarabs / gusts | Clockwork guardian / ice |
| New gameplay | Noise \+ water-splash trap | Wind correction \+ moving target | Ricochet \+ chained switches |
| Overtime Warning / Critical | Cat suspicious / searching | Storm rises / strong gusts | Ice cracks / danger zones |
| Owner | Henry | Bahadir | Pongpat |

Level independence: the shared golf/score/camera interfaces are agreed by Week 8\. Each owner works in a separate Unity scene with its own prefabs, level manager and tests. If a level is delayed or its owner becomes unavailable, the other two levels remain runnable through the scene-selection menu. A generic “complete hole” fallback is kept for demo stability.

8. # **Special Effects**

* Particles: ball impact sparks, celebratory confetti, garden water splash, desert sand gusts, icy skid or snow puffs, and final rescue sparkle \[5\].  
* Audio: distinct club swing and ball collision sounds; perfect-shot cue; wind, bird and winter ambience; comic enemy reactions; success, failure and princess encouragement \[6\].  
* Animation: hamster idle/aim/swing, cat sleep/awake/react, scarab patrol, clockwork guardian turn, and trap or gate movement using hierarchical rigging/animation.  
* Cameras/UI: smooth aim-to-ball-follow transitions, camera shake on heavy impact, timeout progress bar, Warning/Critical icon, power/timing bar, wind compass, noise gauge and tutorial prompts.  
* Danger-state effects: cat twitch/wake/search animation; sandstorm particles and stronger wind loops; progressive ice crack decals and cracking sounds; explicit last-shot cue and retry feedback.  
* Visual accessibility: readable HUD contrast, independent audio volume settings if schedule allows, and non-color-only feedback on hazards.

9. # **Milestones**

| Wk | Date | Goals |
| :---: | :---: | :---- |
| 8 | Oct 26 | Shared scene architecture, danger-state contract, level greyboxing starts |
| 9 | Nov 2 | Ball physics, club profiles, camera MVP |
| 10 | Nov 9 | Three scenes; Warning/Critical triggers and puzzle prototypes |
| 11 | Nov 16 | DEMO 1: playable loop \+ three distinct greybox levels |
| 12 | Nov 23 | Wind/ice/alert escalation and distinct level puzzle integration |
| 13 | Nov 30 | FX, animation, sound, scoring, polish |
| 14 | Dec 7 | Integration; QA |
| 15 | Dec 14 | Demo 2: feature-complete three-level build |
| 16 | Dec 28 | Final Demo: stable packaged build, presentation and evidence |

Table 9.1: Weekly goal

| Workstream | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 |
| :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- |
| Proposal & planning | ■ | ■ |  |  |  |  |  |  |  |  |
| Core golf \+ danger logic |  | ■ | ■ | ■ | ■ |  |  |  |  |  |
| Three independent scenes |  | ■ | ■ | ■ | ■ | ■ | ■ |  |  |  |
| Enemies / hazards / FX |  |  |  | ■ | ■ | ■ | ■ | ■ |  |  |
| Integration & gameplay QA |  |  |  |  | ■ | ■ | ■ | ■ | ■ |  |
| Packaging & final demo |  |  |  |  |  |  |  |  | ■ | ■ |

Table 9.2: Gantt Chart

10. # **Collaboration Record Tacking**

Git and GitHub will store code, development records and review history. Git LFS \[8\] is planned for binary Unity/Blender assets \[10\]. Issues will map to the product backlog; pull requests link issues and include a brief test checklist. Branches will follow feature/\<level-or-task\>, with protected main where permissions permit. Each member commits their own programming work and documents level-specific changes.

GitHub \[9\] repo: [https\://github.com/hwupu/115A-535652-3DGP-Final](https://github.com/hwupu/115A-535652-3DGP-Final)  
Sprint board: [https\://github.com/users/hwupu/projects/2/views/1](https://github.com/users/hwupu/projects/2/views/1)

11. # **Scrum and Sprint**

The latest Scrum framework \[11\]\[12\] will be adopted for this project. We will play in multiple roles as stages progress. First, we will act as Product Owner, who decides requirements and product backlog. AI agents will act as Scrum Master that assist us for spring planning, then will act as coding partner that assist with Unity and code development.

12. # **Workload Distribution**

All three members must contribute to programming. The proposed division maintains one owner per level while spreading cross-cutting code fairly. Assignments must be confirmed at the first planning session.

| Member | Owned level | Programming ownership / course-relevant mini-project links |
| :---- | :---- | :---- |
| Henry | Sunny Garden | BallController and shot timing; tutorial/interaction triggers; alert/noise AI, cat reaction and water-splash trap; 4.A.1 hierarchical animation, 4.A.3 portal, 4.C.4 object interaction, 4.E.3 UI. |
| Bahadir | Windy Desert | Wind/sandstorm stages, desert AI patrols and obstacles; camera behavior; 4.A.2 navigation, 4.B.a pathfinding where suitable, 4.B.2 AI characters, 4.D.1 environment effects. |
| Pongpat | Frozen Lake | Ice/ricochet mechanics, ice-crack stages, score and DangerManager state transitions, puzzle/save/level flow; 4.C.4 object interaction, 4.D.3 particles, 4.E.1 Save & Load, 4.E.2 function selection. |

Shared responsibility: each member integrates and tests their own scene, contributes to common architecture discussions, participates in peer reviews, and prepares a short programming explanation for the final assessment. Tasks not relevant to the golf concept, such as mass-spring deformation or crowd steering, are intentionally out of MVP scope unless requested by the instructor.

Backlog:  
4.A.1 Animation of a hierarchical model  
4.A.2 Character navigation  
4.A.3 Game portals  
4.B.4 A\* algorithm. Path finding  
4.B.2 AI characters  
4.C.4 Character Interaction with objects  
4.D.1 Environmental effects on character  
4.D.3 Character Particle Effects  
4.E.1 Save & Load  
4.E.2 Function selection  
4.E.3 Game UI

13. # **SWOT**

| Strengths Game levels are extendablei.g. difficulties, enrichments Can independently develop scenes by following the GitHub workflow. | Weaknesses Limited resource with three students only 3D asset production and animation are time-consuming Physics-based puzzles require careful difficulty tuning. |
| :---- | :---- |
| **Opportunities** Demonstrate ball physics AI, environment response and VFX from course mini-projects AI-assisted iteration may speed up boilerplate distinctive levels aid grading. | **Threats** Merge conflicts with Unity files AI-generated defects unfair late-game hazard spikes Physics issues Game crush Violation of asset license Schedule slippage |

Table 13.1: SWOT analysis of project roadmap.

14. # **Risk Management Statement**

| Risk | Likelihood / Impact | Mitigation and Fallback |
| :---- | :---- | :---- |
| Scene or asset conflicts | Medium / High | Separate scenes/prefabs; use Git LFS and pull-request reviews; share contracts before parallel work; integrate at least weekly. |
| Golf physics feels unstable | Medium / High | Prototype by Week 9; constrain physics materials and velocities; test outcomes on all three levels. |
| Danger mechanic is frustrating | Medium / High | Test timeout and failure on all levels; signal state changes clearly and keep the last permitted shot achievable. |
| Three levels not sufficiently distinct | Medium / High | Require different scenes, primary hazard and mechanic in level review; demo three greyboxes by Nov 16\. |
| AI-assisted code is unreliable | Medium / Medium | The human owner validates all changes with runtime tests; small PRs, version pinning, rollback and provenance notes. |
| Asset/animation schedule slips | High / Medium | Use primitive greybox and licensed placeholder assets first; favor simple low-poly art; postpone detailed cutscenes. |
| Member unavailable | Low / High | Isolate levels; document scene contracts; weekly shared builds; at least two people can build and demo shared systems. |
| Late defects / performance | Medium / High | Feature freeze by Demo 2; keep Windows as primary deliverable; profile on target hardware; final period reserved for QA. |

Table 14.1: Risk mitigation table

15. # **References**

1\. Wong, Sai-Keung. (2026). Project Proposal Guideline, 3D Game Programming. Course handout, National Yang Ming Chiao Tung University (provided by instructor).

2\. Project team. (2026). 3D Gaming Proposal. Internal concept presentation; attached draft.

3\. Unity Technologies. (2026). System requirements for Unity 6.3. Unity Documentation. https\://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements

4\. Unity Technologies. (2026). Rigidbody. Unity 6.3 Manual. https\://docs.unity3d.com/6000.3/Documentation/Manual/class-Rigidbody.html

5\. Unity Technologies. (2026). Particle systems. Unity 6.3 Manual. https\://docs.unity3d.com/6000.3/Documentation/Manual/ParticleSystems.html

6\. Unity Technologies. (2026). Audio Source. Unity 6.3 Manual. https\://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioSource.html

7\. Unity Technologies. (n.d.). Input System. Unity Manual. https\://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html

8\. GitHub. (n.d.). About Git Large File Storage. GitHub Docs. https\://docs.github.com/en/repositories/working-with-files/managing-large-files/about-git-large-file-storage

9\. GitHub. (n.d.). About Projects. GitHub Docs. https\://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects

10\. Blender Foundation. (n.d.). Animation & Rigging. Blender Manual. https\://docs.blender.org/manual/en/dev/animation/

11\. Schwaber, K., & Sutherland, J. (2020). The Scrum Guide. https\://scrumguides.org/scrum-guide

12\. Ku, D. (2026). Agile Software Engineering: Product Discovery, Development Process, and Testing Practices. Odd-e Agile Coach, course slides.

13\. Campbell, J. (1949). The Hero with a Thousand Faces. Pantheon Books. (Narrative-framework inspiration.)

![][image1]

[image1]: <data:image/png;base64,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>