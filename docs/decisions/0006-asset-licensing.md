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

## Amendment (2026-10-26, #12)
This is a non-commercial class project, so there is no allow list of sources. CC0 and CC-BY are preferred; other assets are used as fair use. Two rules still apply to every asset:
- **No row, no commit:** an asset is committed only once it is registered in `docs/assets.md` and `CREDITS.md`. Agents remind the team of this and never commit an unregistered asset.
- **LFS quota (1 GB):** commit only the assets in use, not whole packs.

The Asset Store rule is unchanged.
