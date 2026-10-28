# 2026-10-28 10 — L1 tutorial prompts in a dialog bar (#28)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #28, #20, #77, #58, #59, #60, #51, #7

## Prompt (verbatim)
> close them. what ticket to do next?

> we can close mini-exercise. do #28

## What the AI did
- Closed #20 and #77 (done in #119 and #120), and Henry's mini-exercise tickets #58, #59, #60.
- Recommended #28 next (Demo 1 sprint, builds on the HUD).
- `Core/UI/DialogBar`: a generic, non-blocking text bar; lines are queued, each stays for its seconds (game time) or until clicked.
- `Levels/Garden/GardenTutorial`: L1 hints, each once per attempt:
  - at the start: look around, power marker, switching tools (only with more than one tool);
  - first shot: every shot takes time;
  - ball stops for the first time: every goal, cup last;
  - first Warning and Critical: the cat (needs DangerManager #7 through ILevelFlow).
  
  The first shot and Critical drop hints still waiting. Texts are editable in the Inspector.
- *Magic > Level > Add HUD and Menus* now also adds the dialog bar (all levels; story lines in #51 can use it). *Magic > Level > Garden: Add Tutorial* adds the tutorial to L1.
- Play-mode tests: `TutorialTests` (2). Architecture §4.8 notes.

## Decisions / open questions
- Agent (Henry didn't pick): plain hint text, no Mentor character; texts drafted by the agent; skip = click on the bar, or it hides after 6 s. Open: does the Mentor speak these lines (portrait, name)?
