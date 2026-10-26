# 4.A.1 Animation of a hierarchical model — Henry

Issue: #59 · Demo: Dec 14
Scene: `Scenes/4A1_HierarchicalModel.unity` · Guide: `docs/mini-exercises.md`
Setup: **Magic > Mini-Exercises > Henry 4.A.1: Set Up Scene** (creates the hamster, the golf ball, the HUD and the camera orbit if missing).

The hamster stands on the ground holding a golf club **in one hand** (right arm) over the ball, wiggling it like aiming. Press **F**: it swings back **slowly**, then down **fast** through the ball. The ball flies, bounces and rolls, and after **5 s** it goes back to the tee for the next round.

The right arm is a 3-level hierarchy:

```
Hamster                      (model: body, ears, eyes, nose, legs ... = 10+ parts)
├── ArmL                     (left arm, resting at the side)
└── A_Shoulder               A: rotates about its local Z axis (the swing)            key 1 reverses
    ├── ArmR
    └── B_Wrist              B: rotates about its local Y axis (turns the club face)  key 2 reverses
        ├── Hand
        └── C_Club           C: aiming wiggle, turns 8° × sin(2π·1.2·t) about its local Z
            ├── GolfClub     (T swaps the tool)
            └── Sledgehammer
```

Each joint only sets its **local** rotation or position. Because B is a child of A and C is a child of B, every motion is added on top of its parents' motion. That's the point of a hierarchical model.

## What's new (developed for this exercise)
- `AxisJoint`: a joint that turns about one chosen local axis by an angle set by an animator; `Direction` (±1) mirrors it, and a key reverses it.
- `SineMotion`: periodic sin/cos turn about a local axis (C's aiming wiggle); returns to rest when disabled. The handout's slide along B's local y-axis is kept in the same file, **commented out**, with the reason for the change.
- `SwingAnimator`: the swing as a small state machine (Address → Backswing → Downswing → Follow-through → Return). The backswing eases slowly (SmoothStep), the downswing accelerates (k²), and impact is the moment A passes 0°.
- `FlyingBall`: ball flight computed by hand (Euler integration: gravity, sideways curve, bounce, rolling friction), reset to the tee after 5 s.
- Reversing A makes a mirrored (left-handed) swing, so the ball flies the other way. Reversing B curves the ball the other way.
- Tool swapping (`ToolSwitcher`, extra): the sledgehammer hits 1.6× harder.

## Controls
| Key | Action |
|---|---|
| F | Swing (when the ball is on the tee) |
| 1 | Reverse A: mirrored swing, the ball flies the other way |
| 2 | Reverse B: the club face turns the other way, the ball curves the other way |
| T | Swap tool (golf club ↔ sledgehammer) |
| P | Pause / resume |
| Right mouse drag, scroll | Orbit / zoom the camera |

## Requirement checklist
- [x] Hierarchical model with >= 2 joints and 3 objects A, B, C: A_Shoulder > B_Wrist > C_Club
- [x] At least one model with >= 5 components: the hamster (body, 2 ears, 2 eyes, nose, 2 legs, 2 arms, hand, club)
- [x] A rotates about one axis (local Z), B about a different axis (local Y)
- [x] C animates periodically (sin): the aiming wiggle at address. **PO choice:** it turns (fits the golf theme) instead of sliding along B's local y-axis. The handout's slide along B's y-axis is in `SineMotion.cs`, commented out, with the reason: in our model B's y-axis is the arm, so the slide made the club pump up and down. Uncomment it to show it.
- [x] Keys change the joint rotation directions: 1 (A), 2 (B)

## Reusable code in Core
- `Core.Motion.AxisJoint`: any script-driven joint (swinging gates, levers, rotating obstacles).
- `Core.Motion.SineMotion`: bobbing pickups, floating icons, moving platforms.

## TA modification points
| What the TA may ask | Where |
|---|---|
| Swing size | **Hamster** → SwingAnimator → `Backswing Angle`, `Finish Angle` |
| Swing speed (slow back, fast down) | SwingAnimator → `Backswing Time`, `Downswing Time`; easing in `SwingAnimator.Update()` |
| A's / B's axis | **A_Shoulder** / **B_Wrist** → AxisJoint → `Axis` |
| Reverse keys | AxisJoint → `Reverse Key` |
| C's wiggle size / speed / sin vs cos | **C_Club** → SineMotion → `Amplitude` (degrees), `Frequency`, `Use Cosine` |
| C along B's local Y (handout version) | `SineMotion.cs`: uncomment the SLIDE lines (fields + 2 lines in `Update`), comment out the 2 TURN lines |
| Ball distance / height / curve | SwingAnimator → `Launch Speed`, `Launch Height`, `Curve`, `Tool Power` |
| Flight time before reset, bounce | **GolfBall** → FlyingBall → `Flight Seconds`, `Bounciness`, `Rolling Friction` |
| Add a tool | Add a child under C_Club, add it to **Hamster** → ToolSwitcher → `Tools` (and a `Tool Power` entry) |

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Third-party code: none
