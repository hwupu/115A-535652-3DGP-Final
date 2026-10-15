# 115A-535652 3D Game Programming — Final Project

Unity game built by a team of 3 using an **AI-native, lightweight Scrum** workflow.
Humans are the Product Owners; AI agents act as Scrum Master first, then Developer.

> Game concept: _TBD by the team in Sprint 0._

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
In Unity, run **Tools > 3DGP > Create Project Folders** once to create the standard folders.

## Assets & credits
- **Never commit Unity Asset Store assets, free or paid** (license + public repo). Import them locally into `Assets/AssetStore/`; the folder is git-ignored. Each member imports the same packs/versions listed in `docs/assets.md`.
- Non-Asset-Store assets may be committed under `Assets/ThirdParty/` only if the license allows redistribution.
- Every asset must be listed in `docs/assets.md` and `CREDITS.md`.

## Working with AI agents
- Agents read `CLAUDE.md`, log sessions in `docs/prompt-log/`, and record major decisions in `docs/decisions/`.
- **Scenes are built in the Editor**, not at runtime. Agents help through menu items under **Tools > 3DGP** (see `Assets/_Project/Editor/`). Ask an agent for a tool, then run it from the menu.
- Agents ask when unsure; answer on the issue (label `needs-po`).

## Process
See [`docs/scrum/process.md`](docs/scrum/process.md): 1-week sprints, GitHub Issues + [Project board](https://github.com/users/hwupu/projects/2), combined kickoff/review/retro.
