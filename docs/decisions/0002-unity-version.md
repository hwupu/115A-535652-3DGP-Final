# 0002-unity-version — Unity version and render stack
- Status: Accepted
- Date: 2026-10-12
- Deciders: Product Owner (team) with Claude as scrum master

## Context
Project was created as Unity 6000.3.25f1 with URP 17.3 and Input System 1.20.

## Decision
Everyone uses exactly **6000.3.26f1** (pinned by `ProjectSettings/ProjectVersion.txt`). URP and the new Input System are the defaults. Upgrading the editor is a team decision (new ADR).

## Consequences
Matching versions avoid reserialization churn across Windows/Mac. Store assets must support URP (convert materials if not).

## Update 2026-10-16
Bumped from 6000.3.25f1 to **6000.3.26f1** (PR #5 branch `feature/update-unity-version`). Packages updated by Unity: AI Navigation 2.0.14 → 2.0.15. Everyone must install 6000.3.26f1 before pulling.
