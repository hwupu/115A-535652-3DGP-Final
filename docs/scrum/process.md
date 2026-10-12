# Scrum Lite (3 people + AI)

## Roles
| Role | Who |
|------|-----|
| Product Owner | The 3 humans (one named "PO of the sprint" per sprint, rotating if wanted) |
| Scrum Master | AI agent now (grooms backlog, drafts sprint plan, keeps docs/logs, flags blockers) |
| Developers | Humans for scenes/assets; AI agents for code once PO switches them on |

## Cadence (1-week sprints)
- **Sprint kickoff (≤20 min, with the humans):** review goal, pick issues from `Backlog`, set the sprint milestone. Agent drafts `docs/scrum/sprints/sprint-NN.md` beforehand.
- **Async check-in:** update the sprint file / issue comments; no standup meeting.
- **Sprint end (≤20 min):** demo what runs in Unity, accept/reject against acceptance criteria, short retro (1 keep, 1 change). Agent records it in the sprint file.

## Work items
- GitHub **Issues** are the backlog. Use the *User story* template: "As a ..., I want ..., so that ..." + acceptance criteria.
- Labels: `type:story`, `type:bug`, `type:chore`, `type:asset`, `area:scene`, `area:gameplay`, `area:ui`, `area:tools`, `owner:human`, `owner:ai`, `blocked`, `needs-po`.
- `needs-po` = agent needs a human decision; answer in the issue, then remove the label.
- Board columns: Backlog → Ready → In progress → Review → Done (a GitHub Project; the repo works without it).

## Definition of Done
Acceptance criteria met; opens without console errors on Windows and Mac; PR reviewed by one other person (or the PO for agent PRs); scenes/prefabs saved with no unrelated diffs; relevant docs/ADR/prompt log updated.

## Working agreements
- Small PRs, branch `feat/<issue>-slug`. Squash merge.
- **One person edits a given scene at a time** (say so in the issue). Prefer prefabs for shared pieces to reduce conflicts.
- Commit asset imports with their `.meta` files.
- Agents never merge their own PRs while acting as Developer.
