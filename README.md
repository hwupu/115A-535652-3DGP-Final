# 115A-535652 3D Game Programming — Final Project

**Hamster fantasy golf** in Unity. A hamster hero rescues the hamster princess from Sir Grumblepaws across three levels (Sunny Garden, Windy Desert, Frozen Lake) before the time bar runs out and wakes the villain.

**Team:** Henry 吳普軒 ([@hwupu](https://github.com/hwupu), Garden) · Bahadir Emre Koksal 柯昊瀚 ([@bahadir-koksal](https://github.com/bahadir-koksal), Desert) · Pongpat Weesommai 余蓬勃 ([@empongpat](https://github.com/empongpat), Lake)

## Welcome
Hi! Here's how we work and how to get going.

**How we work.** Lightweight Scrum with 1-week sprints. Tickets are GitHub issues, sprints are milestones, and progress lives on the [board](https://github.com/users/hwupu/projects/2) (Todo → In Progress → Done). We are the Product Owners; an AI agent (Claude Code) does most of the planning, coding and paperwork, and asks us when a decision is ours to make. You review, refine by hand, and own what gets merged.

**You don't hand-build setups in Unity.** When something needs Editor work, the agent writes a small tool under the **Magic** menu and gives you a step-by-step guide: click this, check that. Follow it, tell the agent you're done, and it checks the result before committing.

**What's already set up.**
- Unity project, Git LFS, docs and the backlog for all sprints.
- The game scenes: `MainMenu`, `L1_Garden`, `L2_Desert`, `L3_Lake`, `Ending` (in Build Settings).
- Your 4 mini-exercise folders, each with its own demo scene and a README checklist of the grading requirements.
- The code layout: `Scripts/Core` (generic, reusable) and `Scripts/Game` (the golf game).
- The shared contracts: who owns which code, and how the pieces talk to each other.

**Where things are.**
| What | Where |
|---|---|
| Your level scene | `Assets/_Project/Scenes/L1_Garden` (Henry), `L2_Desert` (Bahadir), `L3_Lake` (Pongpat) |
| Your mini-exercises | `Assets/_Project/MiniExercises/<You>/` |
| Code | `Assets/_Project/Scripts/Core/`, `Assets/_Project/Scripts/Game/` |
| Editor tools (Magic menu) | `Assets/_Project/Editor/Magic/` |
| Third-party assets | `Assets/ThirdParty/` (redistributable only), `Assets/AssetStore/` (local only, never committed) |
| Docs for agents | `CLAUDE.md`, `docs/` (architecture, grading rules, decisions, prompt log, scrum) |

**Your first ticket.**
1. Do the [one-time setup](#one-time-setup) below.
2. Start Claude Code in the repo and use the **Briefing** and **Pick my next ticket** prompts below.
3. Use **Start a ticket**. The agent makes a branch, implements, and gives you the Unity steps.
4. Refine and test in Unity, then open a PR that links the issue.

## Roadmap
| Date | Milestone |
|---|---|
| Nov 2 | Sprint 2: ball, shot, clubs, camera, golfer avatar |
| Nov 9 | Sprint 3: three greybox levels, danger escalation and puzzle prototypes |
| **Nov 16** | **DEMO 1**: playable loop + three levels; **mini-exercises 1 and 2** |
| Nov 23 | Sprint 5: escalation and puzzles, character leveling |
| Nov 30 | Sprint 6: particles, animation, sound |
| Dec 7 | Sprint 7: art pass, integration and QA |
| **Dec 14** | **DEMO 2**: feature-complete game |
| **Dec 21** | **Mini-exercises 3 and 4** |
| **Dec 28** | **Final demo**: presentation, report, live demo |

Details per sprint: [`docs/scrum/roadmap.md`](docs/scrum/roadmap.md) and the [board](https://github.com/users/hwupu/projects/2).

## One-time setup
1. **Install** Unity Hub + Unity **6000.3.26f1** (exact version), Git 2.40+, Git LFS 3.x, an IDE (Rider / Visual Studio / VS Code), and optionally the GitHub CLI `gh`.
2. **Git LFS:** `brew install git-lfs` (Mac) or `winget install GitHub.GitLFS` (Windows), then `git lfs install`.
3. **Clone:**
   ```bash
   git clone https://github.com/hwupu/115A-535652-3DGP-Final.git
   cd 115A-535652-3DGP-Final
   ```
4. **Use your own git identity.** Lines of code are graded per git author:
   ```bash
   git config user.name  "Your Name"
   git config user.email "the-email-on-your-github-account"
   ```
5. **Open in Unity Hub** (*Add project from disk*, Editor 6000.3.26f1). The first import takes a few minutes. Check that the **Magic** menu appears in the top bar.
6. **Asset Store packs:** import the ones listed in `docs/assets.md` into `Assets/AssetStore/`.
7. **Claude Code:** install it, run `gh auth login` once, then run `claude` in the repo folder.
8. **Recommended, scene merge tool:** add to your local `.git/config`:
   ```ini
   [merge "unityyamlmerge"]
       name = Unity SmartMerge
       driver = "<UnityYAMLMerge path>" merge -p %O %B %A %A
   ```
   - Mac: `/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/Tools/UnityYAMLMerge`
   - Windows: `C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Data\Tools\UnityYAMLMerge.exe`
9. Windows only: `git config core.autocrlf false`.

## Daily workflow
```bash
git switch main && git pull
git switch -c feat/<issue#>-short-name
# work in Unity; File > Save Project before committing
git add -A && git commit -m "feat: ... (#<issue>)"
git push -u origin HEAD     # open a PR that links the issue
```
- **Only edit your own scenes.** Shared pieces go in prefabs.
- Commit `.meta` files together with their assets.
- Never commit Unity Asset Store content. Other assets only if the license allows it, and always credit them in `docs/assets.md` and `CREDITS.md`.
- Keep big files small: the free LFS quota is 1 GB.

## Prompts for Claude Code
Copy one, replace the `<...>` parts.

**Briefing**
```text
I'm <name>. Brief me: what's on the current sprint for me, what's blocked or waiting for my
review, and what changed since <date>. Keep it short.
```

**Pick my next ticket**
```text
I'm <name>. Look at my open issues and the current sprint. Recommend the one ticket I should do
next and why (dependencies, demo dates, mini-exercise deadlines). Don't start yet.
```

**Start a ticket**
```text
Let's do #<n>. Tell me your plan and any questions first. Then create the branch and implement.
If Unity Editor work is needed, write a Magic menu tool and give me step-by-step Unity instructions.
```

**Write my contract stub**
```text
I own <my part in docs/architecture.md>. Create the C# stub with exactly that public API
(empty bodies are fine) so the others can compile against it. Small PR.
```

**Mini-exercise**
```text
I'm <name>. Start my mini-exercise #<n>. Follow docs/mini-exercises.md. Give me a clear first
version I can refine by hand and explain to the TA, and list the "TA modification points".
```

**I need something set up in Unity**
```text
I need to <set up X in my scene>. Write a Magic menu tool for it and give me the step-by-step
Unity steps. I'll tell you when I'm done so you can check the result before committing.
```

**Check against grading**
```text
Check #<n> / my level against the course grading rules. What's missing for "above average"?
Note the gaps on the relevant tickets.
```

**Review a PR**
```text
Review PR #<n> against its issue and docs/architecture.md. Point out contract breaks, scene
conflicts and missing .meta files. Don't push changes.
```

**Wrap up**
```text
I'm done for today. Complete the prompt log, add a progress note to the issue, fix the board
status, and tell me what's left. Commit and push only if I say so.
```

**Sprint check-in**
```text
Act as scrum master. Summarize the current sprint per member (done / in progress / at risk),
draft the check-in notes in the sprint file, and flag anything that needs a PO decision.
```
