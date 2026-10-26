# 2026-10-27 03 — Mini-exercise 4.A.1 Hierarchical model (#59)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #59

## Prompt (verbatim)
> lets start #59

Answers to the agent's questions: A spins, B swings; tools = golf club + sledgehammer; no axis helpers; hamster stands still.

> similar but not what i want. remove the pedestal. hamster stand on the ground. hamster hold the club still. gulf ball on ground. i press F to hit, this is when we have the animation. should swing backward slow, then farword fast to hit the ball. ball fly... maybe 5s, then reset its position back for new round.

> for demo purpose, he hold club with one arm.

> should rotate hammer 90deg. make hud shorter in width, taller in height becuase it is covering the hamster. i'm not sure why is club/hammer moving up and down? maybe wiggle make more sence, like aimming

> maybe comment out the part of "periodic pattern in the local frame of object B along the y-axis of object B" so i can explain to TA and why i change to rotation

Answer to the agent's question (C along B's local y vs a rotation): "Aim wiggle as a rotation" — "i see. i can explain to TA, so lets make it suite our game theme."

## What the AI did
- First version (pedestal, continuous windmill) replaced after Henry's feedback by a golf swing on demand.
- Core: `Motion/AxisJoint` (turns about one local axis by an angle set by an animator; direction ±1, reverse key), `Motion/SineMotion` (sin/cos along the parent's local axis, back to rest when disabled).
- Demo (`4A1_HierarchicalModel/Scripts`): `SwingAnimator` (Address → Backswing slow → Downswing fast → Follow-through → Return; impact launches the ball), `FlyingBall` (hand-computed flight, bounce, roll, reset after 5 s), `ToolSwitcher` (T), `HierarchyDemoHud` (live values, P pause).
- Magic tool *Mini-Exercises > Henry 4.A.1: Set Up Scene*: hamster on the ground (scale 2), one-handed: right arm A_Shoulder (Z) > B_Wrist (Y) > C_Club (waggle along B's Y) > GolfClub / Sledgehammer, golf ball, HUD, face-on camera orbit.
- Exercise README with the hierarchy diagram, checklist and TA modification points.
- After Henry's OK ("i think it is good. comment and export list"): TA talking-point comments in all 4.A.1 scripts (code unchanged); removed the unused pedestal material.
- Hammer head turned 90° (face hits the ball); HUD narrow and tall (300 × 400) so it doesn't cover the hamster.
- `SineMotion` turns (aiming wiggle, 8° about Z); the handout's slide along B's local y-axis is kept commented out in the same file with the reason (Henry's request: "comment out the part ... so i can explain to TA and why i change to rotation").

## Decisions / open questions
- PO (2026-10-26 on #59, confirmed today): hamster with swappable tools as the 4.A.1 model.
- PO: golf swing on F (slow back, fast down), ball flies ~5 s then resets; hamster on the ground holding the club; golf club + sledgehammer; no axis helpers.
- Agent's mapping to the spec: A = right shoulder about Z (the swing, one-handed), B = wrist about Y (club face), C = waggle along B's Y at address (periodic sin); key 1 mirrors the swing (ball flies the other way), key 2 reverses the face turn (curve).
- PO (accepted risk): C's periodic motion is an aiming wiggle (rotation) instead of sliding along B's local y-axis; Henry explains it to the TA, and can uncomment the slide version in SineMotion if asked.
