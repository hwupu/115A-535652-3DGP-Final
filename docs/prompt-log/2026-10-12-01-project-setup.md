# 2026-10-12 01 — Project setup

- **Who:** Pu-Hsuan Wu + Claude (Sonnet 5.5, Claude Code)
- **Related:** ADR-0001..0005

## Prompt (verbatim)
> Please help to setup this project. This will be AI-native approach, so keep major decision and prompt log. This will be a Unity project and we are going to develop it on Windows and Mac. Check the gitignore, make sure it handles OS specific files, builds and temp, etc. We will use LFS. Include README with setup instructions. We will use scrum, but we have only 3 members, so we should adopt and keep it simple and fast. Please set up project on Gihub for use to collaborate. We (human) will act as production owner. AI agents will act as scrum master frist, then developer later. We will find the assests and setup the Unity scene. AI agents should assist scene setup (perhaps via menu > tools >...), instead of building scenes during runtime. Ask human for clarification is needed.

## What the AI did
- Inspected the existing Unity 6000.3.25f1 URP project (Force Text serialization already on) and the existing public GitHub repo `hwupu/115A-535652-3DGP-Final`.
- Added `.gitignore` (Unity, IDE, macOS/Windows/Linux, builds/temp), `.gitattributes` (LF normalization, UnityYAMLMerge, LFS patterns), `.editorconfig`.
- Added `README.md`, `CLAUDE.md`/`AGENTS.md`, ADRs, lightweight Scrum docs, GitHub issue/PR templates, labels, and a `Tools > 3DGP` editor menu skeleton.
- Initial commit and push to GitHub.

## Decisions / open questions
- See ADRs. Open: game concept, collaborator usernames, repo visibility vs. asset licenses, LFS quota (see README).

## Follow-up prompt (verbatim)
> 1. yes, we should keep track of credits and never commit free and paid assets from Unity Assets.
> 5. Change to GitHub Project board.

### What the AI did
- ADR-0006: Asset Store content git-ignored (`Assets/AssetStore/`), credits policy in `docs/assets.md` + `CREDITS.md`, rule in `CLAUDE.md`, README, PR checklist.
- ADR-0007: created GitHub Project #2 with columns Backlog/Ready/In progress/Review/Done, linked to the repo, added issues #1-#4.
