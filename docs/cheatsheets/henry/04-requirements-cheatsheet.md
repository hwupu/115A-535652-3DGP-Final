# Cheat Sheet 4: The requirements, exercise by exercise (what / where / how to demo)

Spec: `docs/references/course/mini-exercises.md`. The evaluation is: **demo the program, present the source code, explain the method, modify it live, and clearly point out which parts are new**.
**Each block below has:** the spec's words · how it's built · key code (file + method) · a 20-second demo · what to say / risks.

---

## 0. General rules that apply to all four

| Rule (course) | Our answer |
|---|---|
| Individual, all code written by the member, real time | Henry's asmdef and folder. AI-assisted code is logged in `docs/prompt-log/`. Be ready to explain every line (sheets 1, 2, 6). |
| **No overlap with assignments; point out what's new** | Each README has a **"What's new"** list. See §5 below for the **PA1 overlap** you must address. |
| Third-party code → ≥ 3 new features + a ≥ 20-object loop | **No third-party code** in any of the four (README "References"). Only the CC0 sounds in 4.E.3. |
| Upload to E3 before the deadline | Export the exercise folder + `Scripts/Core/` (sheet 5 §7). |
| Demo one exercise per demo day | Nov 16: 4.C.4 + 4.E.3 · Dec 14: 4.A.1 + 4.A.3 (team plan). Ask the TA which one first. |

---

## 1. 4.C.4 Character interaction with objects (Nov 16)
*Spec: ≥ 100 obstacles around a character controlled by mouse or keyboard. Obstacles are pushed away on contact, and there must be situations where ≥ 10 obstacles collide with each other. Extra: obstacles vanish after 2 s and the same number respawn randomly without overlapping.*

| Requirement | Built as | Key code | Proof on screen |
|---|---|---|---|
| ≥ 100 obstacles | 10 lines × 10 dominoes, created by a loop | `DominoArena.Start` (loop), `BuildLines` (polar layout) | HUD "Obstacles: 100 / 100"; Hierarchy `Arena/Domino_000..099` |
| Keyboard/mouse character | CharacterController, camera-relative WASD, Shift, Space; right-drag orbit | `ThirdPersonWalker.Update`, `OrbitFollowCamera.LateUpdate` | walk around |
| Pushed on contact | force at contact point, scaled by walking speed | `RigidbodyPusher.OnControllerColliderHit` | walk into a tile: it tips |
| ≥ 10 collide with each other | each line is a 10-tile chain; `OnCollisionEnter` counted per tile | `VanishAfterHit.OnCollisionEnter`, `DominoArena.OnBodyContact` | HUD "Collided with another obstacle" jumps by ~10 per line |
| Extra: vanish + respawn, no overlap | timer then `SetActive(false)`; respawn at home if `Physics.CheckBox` is free | `VanishAfterHit.Update`, `DominoArena.TryRespawn` | tiles turn orange, vanish, reappear; HUD "Respawned" |

**Key formula** (`RigidbodyPusher`):
```csharp
var speedFactor = Mathf.Clamp01(speed / referenceSpeed);                 // 0 standing … 1 at 3 m/s
var impulse = direction * (pushForce * speedFactor * Time.deltaTime);   // N·s per frame of contact
body.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);         // off-centre → torque → tips over
```
**20-second demo:** walk into the first tile of a line → whole line falls → point at the HUD count → wait 5 s → tiles vanish and reappear in line → press V (vanish off) → R (rebuild).
**Risks / what to say:**
- **Spec says 2 s; ours is 5 s.** Before the demo, either set `Arena → DominoArena → Vanish Delay = 2`, or say "5 s so you can see the chain; it's one Inspector value" and change it live.
- **Spec says respawn *randomly*; ours respawns at home.** Say: "Random respawn is written (`NonOverlapPlacement`) and commented out in `TryRespawn`. I chose home so the lines can be knocked down again." Be ready to uncomment it (sheet 6 B-C3).
- "No overlap" is guaranteed because `CheckBox` (tile size + padding, rotated like the tile) must find nothing. Blocked tiles wait in `_waitingForSpot` and retry every frame.

---

## 2. 4.E.3 Game UI (Nov 16)
*Spec: an interactive UI for character/gear selection or portraits. Highlight the selected item with particle or sound effects. Attribute bars (stamina, energy, XP, health, ...) update with events (damage, healing, ...). A minimap shows a top-down view.*

| Requirement | Built as | Key code | Proof |
|---|---|---|---|
| Interactive gear selection | 3 club cards: mouse click or keys 1–3 | `SelectionPanel.Select`, `SelectableCard`, `ClubSelector.Awake` | click cards / press 1 2 3 |
| Highlight with particle **or** sound | select **sound** (half volume) + yellow frame + scale-up | `SelectionPanel.Select` → `PlayOneShot(selectSound, selectVolume)`; `SelectableCard.SetSelected` | audible + visible; plus confetti particles on every hit |
| Attribute bars update with events | **Time** meter: `Meter.Changed` → HUD; −10 s per shot event; colour green/yellow/red | `ShotTimer.ChargeShot`, `Meter.Set`, `GameUiHud.OnTimeChanged`, `BarView` | take a shot: bar slides down 10 s and changes colour |
| (second bar) | **Power** meter (ping-pong marker) | `PingPongGauge.Tick`, `GolfSwing.Swing` | F, F |
| Minimap top-down | orthographic camera 30 m above → RenderTexture → RawImage; icons on "Minimap" layer | `MinimapCamera.LateUpdate`, `MinimapIcon` | top-right; walk and watch; `-` / `=` zoom |

**20-second demo:** select Sledgehammer (2) → sound + highlight → walk to a ball → F, F at the top → confetti + sound, ball slides, time bar −10 s → minimap shows the ball's new spot.
**Risks / what to say:**
- The spec lists *health/stamina/XP*. Ours is **time**, a game attribute that drops on an event (each shot), which is the same mechanism. Show that `Meter` is generic, and offer the stamina bar snippet (sheet 6 B-E3) if they ask.
- "Bars update with events" means the time bar redraws **only** in `OnTimeChanged`, never by polling. The power bar is polled every frame on purpose, because it follows a moving marker. Say so.
- The minimap technique overlaps with PA1 (see §5). Point to the **selection panel, event-driven Meter, power gauge and club data** as the new parts.

---

## 3. 4.A.1 Animation of a hierarchical model (Dec 14)
*Spec: a hierarchical model with ≥ 2 joints and 3 objects (A, B, C), plus at least one model with ≥ 5 components. Rotate A about one axis and B about a different axis. Animate C periodically (sin/cos) along B's local y-axis. Keys change the joint rotation directions.*

| Requirement | Built as | Key code |
|---|---|---|
| ≥ 2 joints, A > B > C | `A_Shoulder > B_Wrist > C_Club` (children in the Hierarchy) | scene; `AxisJoint` on A and B |
| Model with ≥ 5 components | hamster: body, 2 ears, 2 eyes, nose, 2 legs, 2 arms, hand, club | scene |
| A about one axis, B about another | A: local **Z** (the swing), B: local **Y** (club face) | `AxisJoint.LateUpdate`: `localRotation = rest * AngleAxis(Angle*dir, axis)` |
| C periodic sin/cos | C turns 8° × sin(2π·1.2·t) about its local Z (aiming wiggle) | `SineMotion.Update` |
| Keys change directions | 1 reverses A, 2 reverses B | `AxisJoint.Update` → `Reverse()` |

**20-second demo:** HUD shows the chain → F swing (slow back, fast down, ball flies, bounces, resets after 5 s) → press 1, F (mirrored swing, ball flies the other way) → press 2, F (ball curves the other way) → T (sledgehammer, flies further) → P pause to show a pose.
**Risks / what to say:**
- **The spec says C moves *along B's local y-axis*; ours *turns*** (PO choice: in our model B's y-axis is the arm, so a slide made the club pump up and down). The handout version is **in `SineMotion.cs`, commented out**. Uncomment the SLIDE fields + 2 lines and comment out the 2 TURN lines to show it (sheet 6 B-A2). **Consider showing the slide version first** to remove any doubt, then explain why the game uses the turn.
- The core idea to say: "Each joint only sets its **local** rotation. Children inherit the parent's transform, so world pose = parent world × local. Nobody computes the club's world position."

---

## 4. 4.A.3 Game portals (Dec 14)
*Spec: control a character. ≥ 3 portals teleport the character to other locations. ≥ 1 particle system at each portal, plus a particle burst around the character after teleporting.*

| Requirement | Built as | Key code |
|---|---|---|
| Player-controlled character | hamster, `ThirdPersonWalker` | — |
| ≥ 3 portals | 3 in the hub (Pond Island, High Platform, Hedge Maze) + 3 return portals = 6 | `Portal.OnTriggerEnter` → `PortalTraveler.Travel` |
| ≥ 1 particle system per portal | looping **Swirl** child (ring emitter, rising + orbital velocity) | `Portal.ApplyLook` (play/stop, colour) |
| Burst around the character after teleport | one world-space system moved to the hamster, `Emit(40)` | `PortalTraveler.TravelRoutine` |
| Extras | locked portal + floor switch; fade to black; cooldown; HUD; commented scene switch | `PortalUnlockSwitch`, `ScreenFader`, `Portal.Unlock` |

**20-second demo:** walk into the Pond Island portal → fade, arrive, burst → return portal → try the High Platform portal (grey, nothing happens) → step on the red switch (turns green, portal lights up) → teleport up to the platform (4 m, can't be reached on foot).
**Risks / what to say:** walls are higher than the jump, so portals are the only route. The cooldown (1 s) is why you don't bounce back. Locked portals have their particles **stopped and cleared**.

---

## 5. What's new vs PA1 (important: "no overlap with assignments")
PA1 (The Boomerang Guardian) already had: a **RenderTexture minimap with a Minimap layer**, targets that **vanish after 2 s**, `AddForceAtPosition` impulses, `Physics.CheckBox` **rejection-sampling spawns**, trigger platforms, a score HUD. The TA may know this. Reused content **doesn't count**, so lead with the new parts:

| Exercise | Similar to PA1 (say it openly) | New here (count this) |
|---|---|---|
| 4.C.4 | impulse at a point; vanish timer; CheckBox overlap test | **CharacterController pushing** via `OnControllerColliderHit` + `IPushReceiver` (PA1's player was a Rigidbody); **chain collisions** of 10 obstacles and counting them; **object pool** with respawn-at-home and a wait queue; polar domino layout |
| 4.E.3 | minimap camera + RenderTexture + layer icons | **gear selection panel** (mouse + keys, sound, animated highlight); **generic event-driven `Meter`** + smoothed `BarView` with colour thresholds; **ping-pong power gauge**; **ScriptableObject club data**; scripted ball slide with ease-out and hop |
| 4.A.1 | — | everything (joints, sine motion, swing state machine, hand-integrated ball flight) |
| 4.A.3 | trigger detection | portals, locks, coroutine teleport with fade, CharacterController-safe teleport, swirl/burst particles |

Phrase: "The minimap technique is the same idea as my PA1 minimap, so I don't count it. The new parts of this exercise are ...".

---

## 6. One-page demo checklist (per exercise, ~3 min)
1. Open the exercise scene (`<folder>/Scenes/<Code>_<Name>.unity`), Console clear, Play.
2. Read the requirement list aloud and prove each one with the HUD (tables above).
3. Say what's new (§5 / README "What's new").
4. Open the main script and walk through the key method (sheet 6 Part A).
5. Ask: "Would you like me to change something?" Inspector first, then code (sheet 6 Part B).
