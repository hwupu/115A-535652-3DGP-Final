# 2026-10-27 02 — Mini-exercise 4.A.3 Game portals (#60)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #60, #38, #59

## Prompt (verbatim)
> i delete teh tutorialinfo. commit

> we may as will finish the other two mini together. what are them?

> yea, both are related, so we should do those before moving on the sprint 2, so we can reuse the code (maybe)? start #60

Answers to the agent's questions: hub + 3 areas ("add comment (comment out) for scene (levels) switching. just for demo to TA"); walk in; quick fade; one locked portal.

## What the AI did
- PR #100: removed the URP template's TutorialInfo + Readme.asset; committed the URP asset auto-upgrade.
- Core: `Portals/Portal`, `Portals/PortalTraveler`, `UI/ScreenFader`. Scene switching written but commented out (for #38).
- Demo (`4A3_GamePortals/Scripts`): `PortalUnlockSwitch`, `PortalDemoHud`.
- Magic tool *Mini-Exercises > Henry 4.A.3: Set Up Scene*: Portal prefab (trigger, pad, frame, swirl particles), Hub + Pond Island + High Platform + Hedge Maze, 6 portals (Hub → High Platform locked), switch, hamster, arrival particles, fader, HUD, camera follow.
- Exercise README with checklist and TA modification points.
- After Henry tested it ("it is fantastic. add comments and export list"): TA talking-point comments in all 4.A.3 scripts (code unchanged).

## Decisions / open questions
- PO: hub + 3 areas with return portals; walk-in trigger; quick fade; one locked portal; commented-out scene switch for the TA.
- Agent's choice (easy to change): the locked portal unlocks with a floor switch in the hub.
- Henry wants to finish 4.A.1 (#59) too before Sprint 2 work, to reuse the code.
