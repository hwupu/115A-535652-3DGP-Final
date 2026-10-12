# 0003-git-lfs-and-serialization — Git LFS, Force Text, visible metas
- Status: Accepted
- Date: 2026-10-12
- Deciders: Product Owner (team) with Claude as scrum master

## Context
Windows + Mac contributors, binary art assets, YAML scenes.

## Decision
Binary asset types (textures, models, audio, video, fonts, ...) are tracked by Git LFS via `.gitattributes`. Asset Serialization = Force Text; `.meta` files are committed. Line endings normalized to LF. `Library/ Temp/ Logs/ UserSettings/`, builds and OS files are ignored. UnityYAMLMerge is the recommended merge tool.

## Consequences
Everyone must run `git lfs install`. GitHub free LFS quota is 1 GB storage / 1 GB bandwidth per month — watch large packs; scenes should have one owner at a time to avoid merge conflicts.
