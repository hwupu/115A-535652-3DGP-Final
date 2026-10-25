# 115A-535652 3D Game Programming — Final Project

Unity game built by a team of 3 using an **AI-native, lightweight Scrum** workflow.
Humans are the Product Owners; AI agents act as Scrum Master first, then Developer.

> **Concept:** single-player 3D fantasy golf. A hamster hero rescues the hamster princess from Sir Grumblepaws across three levels (Sunny Garden, Windy Desert, Frozen Lake) while a hidden time bar fills and wakes the villain. See [`docs/references/proposal.md`](docs/references/proposal.md), [ADR-0008](docs/decisions/0008-game-concept.md) and the [roadmap](docs/scrum/roadmap.md).

**Team (Product Owners):** Henry 吳普軒 ([@hwupu](https://github.com/hwupu)) · Bahadir Emre Koksal 柯昊瀚 ([@bahadir-koksal](https://github.com/bahadir-koksal)) · Pongpat Weesommai 余蓬勃 ([@empongpat](https://github.com/empongpat))

## Start here (onboarding, 2026-10-26)
New to the repo, or catching up? Do these in order.

1. **Install and clone:** follow [Requirements](#requirements) and [Setup](#setup-windows--mac) below.
2. **Set your git identity to your GitHub account.** Lines of code are graded **per git author** (Form B individual bonus, report), so every commit must be yours:
   ```bash
   git config user.name  "Your Name"
   git config user.email "the-email-on-your-github-account"
   ```
3. **Claude Code (optional but recommended):** install it, run `gh auth login` once so agents can read issues and the board, then start `claude` in the repo folder. The agent reads `CLAUDE.md` automatically.
4. **Read, in this order (15 min):**
   - [`docs/references/proposal.md`](docs/references/proposal.md): the game.
   - [`docs/architecture.md`](docs/architecture.md): shared contracts and **who owns which code**.
   - [`docs/references/course/`](docs/references/course/): grading rules (Form A/B, report, mini-exercises).
   - [`docs/mini-exercises.md`](docs/mini-exercises.md): your 4 individual exercises.
   - [Board](https://github.com/users/hwupu/projects/2) and [`docs/scrum/roadmap.md`](docs/scrum/roadmap.md): sprints and tickets.
5. **In Unity (manual, once after pulling):**
   1. Unity Hub → open the project with **6000.3.26f1**. Wait for the import to finish.
   2. Check the top menu bar has **Magic** (our Editor tools, ADR-0011). If it's missing, look at the Console for compile errors.
   3. Open your **level scene** in `Assets/_Project/Scenes/`: `L1_Garden` (Henry), `L2_Desert` (Bahadir), `L3_Lake` (Pongpat). Only you edit your scene.
   4. Open your **mini-exercise scenes** in `Assets/_Project/MiniExercises/<You>/<Code>_<Name>/Scenes/`. Each folder has a `README.md` with the requirement checklist.
   5. You don't need to run the Magic > Setup / Scenes commands again; their output is committed. Running them again is safe (they only create what's missing).
   6. Import the Asset Store packs listed in `docs/assets.md` into `Assets/AssetStore/` (local only, never committed).

### Where we are
| Done | In review / next |
|---|---|
| Proposal, course rubrics in markdown, rubric-aligned backlog (#55) | Shared contracts `docs/architecture.md` (#6): **all three must approve** |
| Magic menu, game scenes in Build Settings, 12 mini-exercise scaffolds, Core/Game asmdefs (#85, #69) | Sprint 2: ball, shot, clubs, camera, avatar ([board](https://github.com/users/hwupu/projects/2)) |

Mini-exercise demos: **Nov 16** (mini-1, mini-2) and **Dec 21** (mini-3, mini-4).

### Magic menu (Editor tools)
| Command | What it does |
|---|---|
| Magic > Setup > Create Project Folders | Standard folders under `Assets/_Project/` |
| Magic > Setup > Create Core and Game Assemblies | `Project3DGP.Core` and `Project3DGP.Game` asmdefs |
| Magic > Scenes > Create Game Scenes | `MainMenu`, `L1_Garden`, `L2_Desert`, `L3_Lake`, `Ending` + Build Settings |
| Magic > Scenes > Sync Build Settings | Game scenes first, in order |
| Magic > Mini-Exercises > Create All Planned / New Exercise... | Exercise folder, README checklist, demo scene, member asmdef |

Every command only creates what's missing and logs `[Magic] ...` in the Console. Need a new one? Ask an agent (prompt below).

## Prompts for Claude Code
Copy, then replace `<...>`. The agent follows `CLAUDE.md`: it asks when something is a product decision, logs the session in `docs/prompt-log/`, and works on a branch.

**Briefing (start of a session)**
```text
I'm <Bahadir/Pongpat/Henry>. Brief me: what's the project state, what's on the current sprint
for me (board + milestone), what's blocked or waiting for my review, and what changed since <date>?
Keep it short.
```

**Pick my next ticket**
```text
I'm <name>. Look at my open issues on the board and the current sprint. Recommend the one ticket I
should do next and why (dependencies, demo dates, mini-exercise deadlines). Don't start yet.
```

**Start a ticket**
```text
Let's do #<n>. Read the issue, docs/architecture.md and anything it links. Tell me your plan and any
product questions first. Then create the branch feat/<n>-<slug> and implement. If Unity Editor work is
needed, write a Magic menu tool for it and give me step-by-step manual Unity instructions.
```

**Write my contract stub** (from `docs/architecture.md`)
```text
I own <CameraRig / DangerManager + ScoreKeeper + LevelGoal + LevelManager / GolfBall + ShotController>.
Create the C# stub with exactly the public API in docs/architecture.md (empty bodies are fine), in
Assets/_Project/Scripts/Game/<Folder>/, so the others can compile against it. Small PR.
```

**Mini-exercise**
```text
I'm <name>. Start my mini-exercise #<n> (<code> <title>). Follow docs/mini-exercises.md: standalone
scene in my folder, generic technique code in Core, requirement checklist in the README. Generate a
clear first version I can refine by hand and explain to the TA, and list the "TA modification points".
```

**I need an Editor tool / Unity setup**
```text
I need to <set up X in my scene>. Write a Magic menu tool (one file, create only what's missing,
Undo-able) and give me the step-by-step manual Unity steps to run it and check the result.
After I run it, I'll tell you, and you check the generated files before committing.
```

**Check my work against grading**
```text
Check #<n> / my level against docs/references/course/ (Form B items, report needs, mini-exercise
rules). What's missing for "above average"? Put the gaps on the relevant tickets.
```

**Review a PR**
```text
Review PR #<n> against its issue's acceptance criteria and docs/architecture.md. Point out contract
breaks, scene/prefab conflicts and missing .meta files. Don't push changes.
```

**Wrap up**
```text
I'm done for today. Make sure the prompt log for this session is complete, the issue has a short
progress note, the board status is right, and tell me what's left. Commit and push only if I say so.
```

**Sprint check-in (scrum master)**
```text
Act as scrum master. Summarize the current sprint: done / in progress / at risk, per member, and draft
the check-in notes in docs/scrum/sprints/sprint-<NN>.md. Flag anything that needs a PO decision.
```

## Requirements
| Tool | Version |
|------|---------|
| Unity Hub + Unity Editor | **6000.3.26f1** (exact; install via Hub, add Windows / Mac build support as needed) |
| Git | 2.40+ |
| Git LFS | 3.x |
| IDE | Rider, Visual Studio 2022 (Windows), or VS Code / Rider (Mac) |
| GitHub CLI (optional) | `gh` |

## Setup (Windows & Mac)
1. **Install Git LFS** (once per machine)
   - Mac: `brew install git-lfs`
   - Windows: included with Git for Windows, or `winget install GitHub.GitLFS`
   - Then run: `git lfs install`
2. **Clone** (LFS files download automatically):
   ```bash
   git clone https://github.com/hwupu/115A-535652-3DGP-Final.git
   cd 115A-535652-3DGP-Final
   git lfs pull        # only if assets look like tiny text pointer files
   ```
3. **Open in Unity Hub** → *Add project from disk* → select the folder, using Editor **6000.3.26f1**. First import takes a few minutes (`Library/` is local and git-ignored).
4. **Editor settings** (already committed, don't change): *Edit > Project Settings > Editor* → Version Control = *Visible Meta Files*, Asset Serialization = *Force Text*.
5. **Recommended: UnityYAMLMerge** for scene/prefab conflicts. Add to your *local* `.git/config` (the `.gitattributes` already points scenes/prefabs at it):
   ```ini
   [merge "unityyamlmerge"]
       name = Unity SmartMerge
       driver = "<UnityYAMLMerge path>" merge -p %O %B %A %A
   ```
   - Mac: `/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/Tools/UnityYAMLMerge`
   - Windows: `C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Data\Tools\UnityYAMLMerge.exe`
6. Windows only: `git config core.autocrlf false` (the repo normalizes to LF via `.gitattributes`).

## Daily workflow
```bash
git switch main && git pull
git switch -c feat/<issue#>-short-name
# work in Unity; File > Save Project before committing
git add -A && git commit -m "feat: ... (#<issue>)"
git push -u origin HEAD     # open a PR, link the issue
```
- Commit `.meta` files with their assets. Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- **One person per scene at a time.** Put reusable pieces in prefabs.
- Binaries go through Git LFS automatically (see `.gitattributes`). Check with `git lfs ls-files`.
- GitHub free LFS quota is 1 GB storage + 1 GB/month bandwidth. Don't commit huge raw packs (skip demo scenes and unused content).

## Repository layout
```
Assets/_Project/      our editor tools, scripts, scenes, prefabs
Assets/ThirdParty/    redistributable non-Asset-Store packs only (CC0/CC-BY...)
Assets/AssetStore/    Asset Store packs: LOCAL ONLY, git-ignored
docs/decisions/       ADRs (major decisions)
docs/prompt-log/      verbatim prompt log of AI sessions
docs/scrum/           process, sprint files
docs/assets.md        asset policy & license register (+ CREDITS.md)
CLAUDE.md / AGENTS.md rules for AI agents
```
In Unity, run **Magic > Setup > Create Project Folders** once to create the standard folders.

## Assets & credits
- **Never commit Unity Asset Store assets, free or paid** (license + public repo). Import them locally into `Assets/AssetStore/`; the folder is git-ignored. Each member imports the same packs/versions listed in `docs/assets.md`.
- Non-Asset-Store assets may be committed under `Assets/ThirdParty/` only if the license allows redistribution.
- Every asset must be listed in `docs/assets.md` and `CREDITS.md`.

## Working with AI agents
- Agents read `CLAUDE.md`, log sessions in `docs/prompt-log/`, and record major decisions in `docs/decisions/`.
- **Scenes are built in the Editor**, not at runtime. Agents help through menu items under **Magic** (see `Assets/_Project/Editor/Magic/`). Ask an agent for a tool, then run it from the menu.
- Agents ask when unsure; answer on the issue (label `needs-po`).

## Process
See [`docs/scrum/process.md`](docs/scrum/process.md): 1-week sprints, GitHub Issues + [Project board](https://github.com/users/hwupu/projects/2), combined kickoff/review/retro.
