# 2026-10-26 01 — Asset policy (#12)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #12, ADR-0006

## Prompt (verbatim)
> i'm not sure what I need to do for #12?

> I don't think we need an allow list. this is a class project, and will not go for sell. Ofcause CC0 and CC-BY is preferred. Else, it is fair-use. The key here is, AI agents should remind and never, commit unlicensed assets (unless noted in the assets.md and CREDITS.md). And, should watch our for 1GB LFS quota. Remind us to not commit everything, should keep the actual assets in use. please update, then close this ticket

## What the AI did
- Explained #12: the Asset Store rule and the `.gitignore` entry were already in place; the open point was the source policy.
- Updated the `docs/assets.md` policy:
  - no allow list; CC0/CC-BY preferred, otherwise fair use;
  - "no row, no commit";
  - commit only the assets in use (1 GB LFS quota, check with `git lfs ls-files -s`, flag PRs over ~50 MB).
- Updated CLAUDE.md rule 6, the README "Assets & credits" section, and the PR template checklist.
- Added an amendment to ADR-0006.

## Decisions / open questions
- PO: no allow list. CC0/CC-BY preferred, fair use otherwise (non-commercial class project).
- No placeholder packs chosen yet. Greyboxes use primitives, and each owner registers packs when they add them.
