# 0006 — Asset Store content is never committed; credits are mandatory
- Status: Accepted
- Date: 2026-10-12
- Deciders: Product Owner (team)

## Context
The repo is public. Unity Asset Store licenses (free and paid) prohibit redistributing the source assets.

## Decision
`Assets/AssetStore/` is git-ignored; members import Asset Store packs locally at the same versions listed in `docs/assets.md`. Other assets are committed under `Assets/ThirdParty/` only when their license allows. All assets are credited in `docs/assets.md` and `CREDITS.md`.

## Consequences
Setup needs one extra step per member (import packs). Scenes may show missing references until packs are imported. Reviewers check the PR checklist item.
