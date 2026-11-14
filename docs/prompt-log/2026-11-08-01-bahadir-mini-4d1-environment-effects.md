# 2026-11-08 01 — Bahadir mini-exercise 4.D.1 Environment effects on character (#62)

- **Who:** Bahadir + Claude Sonnet 5.5
- **Related:** [#62](https://github.com/hwupu/115A-535652-3DGP-Final/issues/62), `docs/mini-exercises.md`, ADR-0010
- **Branch:** `feat/62-mini-4d1-environment-effects`

## Prompt (verbatim, key steering prompts)
> Ok, now I will go wil my environmental effects on character miniexercise. In mini exercises folder of the project, I will create a scene for that (under Bahadir, 4d1, scenes folder). There, I will make a simple scene with 3 different environmental conditions. (Ice surface, windy region, and hot plate region). Relatively, in these places, player will slide when player stops moving on icy surface, player will be pushed constantly with a directional velocity bias in windy region, and player will be constantly jumping on the hot plate area. Each of those regions will be on a flat simple scene that I will not pay to much effort on. Priority is to complete the miniexercise requirement with minimum necessities. On Icy surface, there will be blocks that are also sliding when player pushes them. On windy surface, there will be repetatively spawning one  sphere object (representation of the bush that usually rolls over in dessert regions, can be just a slightly transparent sphere object that we can collide). As soon as it leaves this wind region, it will be dissapeared and spawned again on the other side. In fire place, there will be magma block objects whcih also pushes us sideways when touched. So each touch will impose player a other direction impulse or velocity (which ever is easier). We will look into animation later. For the character control, its not necessary to apply controls here so we can use the previously implemented controls on this scene right away. How does that sound? Make me brief summary of this path with bullet points that needs to be achieved one by one before reaching this goal.

> Ok, actually, lets make a simple controller for the character from stracts if its gonna be easier. wasd will give acceleration to the body regardless of its touching groud. So when character is airborne, we can still move the character with controls. Space will be jumping and shift holding will be for sprint. Very basic such controller. So that we can arrange things including momentum, sliding, pushes easier that way. Am I right? We can be more free this way.

> For the animation part, lets add two stick underneath this capsule. I want them to move async forward and backward depending on my keyboard inputs (maybe +-22.5 degree is fine) and its frequency relevant to the walking/sprinting. Each jump should make both of them go forward with 45 degree angle. To confirm, yes there will not be knee joints, they will be just single cylinders.

> Can you properly commit my changes and push to github according to the agreed rules.

## What the AI did
- Guided Bahadir step by step (folders, scene objects, Inspector values). The script drafts were written by the AI in the chat; Bahadir entered them in Unity, tested the behaviour in Play mode and tuned it. Scene, prefab and material work was done by Bahadir in the Editor.
- On request, filled in the exercise README (what's new, controls, checklist, TA modification points).
- On request, committed the exercise folder only (no unrelated files such as the `Hole.mat` re-save or `.vscode/`), registered the project-authored materials in `docs/assets.md` and `CREDITS.md`, and added this log. No LFS binary assets were added.

## Decisions / open questions
- Own Rigidbody controller (`MomentumController`) instead of the game's `CharacterWalker`: the velocity moves towards `input * speed` with a limited acceleration, so sliding, wind and impulses are easy to add. Code lives in the exercise folder (not Core) for now.
- Zones pass their effect to the controller every physics step (speed factor, grip factor, wind velocity); the controller clears them each step, so overlapping zones combine and there is no enter/exit bookkeeping.
- Leg animation is code-driven (two stick legs, +-22.5 deg swing, 45 deg forward in the air).
- Open: scenery props around the zones, possible promotion of `EnvironmentZone` / `MomentumController` to Core for the game's wind and ice effects (#45, #23).
