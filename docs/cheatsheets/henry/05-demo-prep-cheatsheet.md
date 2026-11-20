# Cheat Sheet 5: Demo prep (tools, speeches, drills, survival)

The course says the TA may ask you to "demo the program, present the source code, explain the method, and modify the program live". This sheet gets you ready for that.

---

## 1. Before the demo: 10-minute pre-flight

| ✔ | Check | How |
|---|---|---|
| ☐ | Project opens, **no red errors** | Unity Hub → Unity **6000.3.26f1** → Console → Clear → no `error CS` |
| ☐ | On `main`, up to date, clean | `git checkout main && git pull`; `git status` clean |
| ☐ | Each exercise scene runs | open `Assets/_Project/MiniExercises/Henry/<Code>_<Name>/Scenes/<Code>_<Name>.unity` → Play → HUD appears, no errors |
| ☐ | 4.C.4 decision made: **Vanish Delay 2 or 5?** | `Arena → DominoArena → Vanish Delay` (spec says 2 s) |
| ☐ | 4.A.1 decision made: **show the SLIDE (handout) version too?** | know the lines in `SineMotion.cs` (sheet 6 B-A2) |
| ☐ | Audio on (4.E.3 select + hit sounds) | Game view: Mute Audio off; system volume up |
| ☐ | Game view not "Maximize on Play", Console visible | so you can show logs and errors |
| ☐ | Know the setup menu: **Magic > Mini-Exercises > Henry <code>: Set Up Scene** | it only creates what's missing, so it's safe to re-run |
| ☐ | Sheet 6 snippets tried once (at least: B-C3, B-E3, B-A2) | then **discard**: `git restore .` |
| ☐ | E3 package exported and uploaded (§7) | before the deadline |
| ☐ | Backup: a 1-minute screen recording of each exercise | in case the Editor misbehaves |

**Compile check without the Editor** (only when the Editor is **closed**, i.e. no `Temp/UnityLockfile`):
`/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath . -logFile /tmp/u.log` then `grep "error CS" /tmp/u.log`.

---

## 2. Unity Editor map (show things fast)

| Panel | Use in the demo |
|---|---|
| **Hierarchy** | 4.A.1: expand `Hamster > A_Shoulder > B_Wrist > C_Club` (the hierarchy *is* the exercise). 4.C.4: `Arena/Domino_000..099` appear at Play |
| **Inspector** | tunables with tooltips (hover). Values changed **during Play are lost when you stop**: good for showing, not for keeping |
| **Scene view** | select a joint during Play and watch its local gizmo turn (set the tool handle to **Local**, not Global) |
| **Game view** | the HUDs |
| **Project** | 4.E.3 `Data/*.asset` clubs; prefabs (`Domino`, `Portal`) |
| **Console** | errors; add a `Debug.Log` live if asked "prove it runs" |

Pause ⏸ + Step ⏭ freeze the game frame by frame. Good for showing a domino mid-fall or the swing at impact. 4.A.1 also has **P**.

---

## 3. The speeches (memorise the shape, not the words)

### 3.1 Common opening (20 s)
"Unity 6, URP, new Input System. Each exercise is its own folder and scene. The reusable technique is in a generic Core library that our golf game also uses, and the exercise folder holds the demo glue. Everything communicates through C# events."

### 3.2 4.C.4 (60 s)
1. "100 obstacles: a loop creates 10 lines of 10 dominoes in polar coordinates, angle per line, radius per tile."
2. "The hamster is a CharacterController, so physics won't push for it. I catch `OnControllerColliderHit` and apply an impulse **at the contact point**, scaled by walking speed. Off-centre force means torque, so the tile tips."
3. "The tile falls into the next: real Rigidbody collisions, so 10 obstacles collide in a chain. Each tile counts its `OnCollisionEnter` with another body, and the HUD shows it."
4. "Hit tiles turn orange and, after the delay, deactivate (pooling, no Destroy). The arena puts them back home only if `Physics.CheckBox` finds the spot free; otherwise they wait and retry."

### 3.3 4.E.3 (60 s)
1. "Gear selection: a generic `SelectionPanel` of cards, mouse or number keys. One `Select` method highlights exactly one card and plays a sound."
2. "Attribute bar: a generic `Meter` raises `Changed` whenever its value changes. The HUD subscribes and redraws the bar, so a shot costs 10 s through an event and the UI never polls it."
3. "Power meter: a ping-pong gauge 0→1→0, and the second F freezes it. Distance = club max × power. Club stats are ScriptableObject assets."
4. "Minimap: an orthographic camera above the hamster renders to a RenderTexture shown in a RawImage. Icons live on a layer only the map camera sees."

### 3.4 4.A.1 (60 s)
1. "Shoulder A → wrist B → club C, parented in the Hierarchy."
2. "Each joint only sets its **local** rotation: rest × AngleAxis(angle, axis). A about Z, B about Y. Children inherit, so the club follows both without any world-space math."
3. "The swing is a state machine: Address, Backswing (SmoothStep, slow), Downswing (k², accelerating), Follow-through, Return. Impact is when A passes 0°."
4. "C wiggles with amplitude × sin(2πft). Keys 1 and 2 flip the joint directions: a mirrored swing, and a curve the other way. The ball is integrated by hand: gravity, side acceleration, bounce, rolling friction."

### 3.5 4.A.3 (60 s)
1. "Six portals, each a trigger with a destination transform. Portal decides who and where; the traveler decides how."
2. "Teleport is a coroutine: fade out, disable the CharacterController, move, sync physics, enable, burst particles, fade in. A 1 s cooldown prevents bouncing back."
3. "Each portal has a looping swirl: a ring emitter with upward plus orbital velocity. The arrival burst is one world-space system moved to the hamster."
4. "Extra: a locked portal (grey, particles cleared) that a floor switch unlocks through `Portal.Unlock()`, the same call the game's garden gate will use."

### 3.6 "Which parts are new?" (always asked)
Read the README "What's new" list, then sheet 4 §5 (the PA1 overlap). Never claim the minimap or vanish timer as new.

### 3.7 AI usage (if asked)
"An AI assistant generated a first readable version under my direction. I reviewed and refined it, and I can explain and change every part. All prompts are logged in `docs/prompt-log/`." (Match what you actually did.)

---

## 4. "Explain this code" drills (45 s each, open the file first)
1. `RigidbodyPusher.OnControllerColliderHit`: the three early returns (no/kinematic/heavy body; standing on it), direction, speed factor, impulse × dt, `IPushReceiver`.
2. `DominoArena.BuildLines` + `TryRespawn`: polar layout, index math, `CheckBox` with padding, `SyncTransforms`.
3. `VanishAfterHit`: two ways to get hit, first hit only, countdown, `SetActive(false)` + event.
4. `Meter.Set`: clamp, no event if unchanged, `Changed`, `Emptied` only on the step to 0.
5. `GolfSwing.Swing` + `FindBallInReach`: power, accuracy angle, dot-product "in front" test.
6. `BarView.Apply` + `Update`: anchorMax trick, SmoothDamp, struct copy.
7. `AxisJoint.LateUpdate`: quaternion order (rest × delta = local axis).
8. `SwingAnimator.Update`: phases, easing curves, impact detection.
9. `FlyingBall.Update`: Euler step, bounce, rolling friction, reset.
10. `PortalTraveler.TravelRoutine`: coroutine timeline, why disable the controller.

---

## 5. Live-change drills (Inspector only, no code)
| TA says | Do |
|---|---|
| "Make obstacles vanish after 2 s" | Arena → `Vanish Delay` = 2 (before Play, or press R after changing) |
| "200 obstacles" | Arena → `Line Count` 20 (before Play: the pool is created in `Start`) |
| "Push harder / weaker" | Hamster → RigidbodyPusher → `Push Force` (live) |
| "Heavier tiles" | Domino prefab → Rigidbody `Mass` (above the pusher's `Max Mass` 20 = can't push) |
| "Shot costs 20 s" | Game → ShotTimer → `Shot Cost` |
| "Putter goes further" | `Data/Putter.asset` → `Max Distance` (the card text updates on the next Play) |
| "Bar turns yellow at 50 %" | Canvas → GameUiHud → `Warning At` 0.5 |
| "North-up / rotating minimap" | Minimap camera → `Rotate With Target` |
| "A about X instead of Z" | A_Shoulder → AxisJoint → `Axis` (1, 0, 0) |
| "Faster downswing / bigger swing" | Hamster → SwingAnimator → `Downswing Time`, `Backswing Angle` |
| "C with cos / bigger wiggle" | C_Club → SineMotion → `Use Cosine`, `Amplitude`, `Frequency` |
| "Reverse key for A on Q" | A_Shoulder → AxisJoint → `Reverse Key` |
| "Portal 1 goes somewhere else" | Portal → `Destination` (drag another Arrival) |
| "Instant teleport" | Hamster → PortalTraveler → clear `Fader` |
| "Bigger arrival burst" | PortalTraveler → `Arrival Particle Count` |

---

## 6. Rapid-fire Q&A
- **Rigidbody vs CharacterController?** A Rigidbody is moved by physics (forces, gravity, gets pushed). A CharacterController is moved by `Move()`: precise, slides on walls, does steps and slopes, isn't pushed, and doesn't push.
- **Kinematic Rigidbody?** Not moved by physics, but moved by code. It still takes part in triggers and collisions as the "mover". The portal uses one so trigger events fire.
- **`ForceMode.Impulse` vs `Force`?** An impulse is an instant change in momentum (N·s). Force is applied over the physics step (N).
- **Quaternion vs Euler angles?** Quaternions avoid gimbal lock and interpolate smoothly. `Quaternion.Euler`/`AngleAxis` build them, and `*` composes rotations (order matters).
- **Local vs world?** Local = relative to the parent. A child's world transform = parent world × child local.
- **Orthographic camera?** No perspective: size doesn't shrink with distance. `orthographicSize` = half the visible height.
- **RenderTexture?** A texture a camera draws into instead of the screen. A RawImage shows it.
- **Culling mask / layers?** The camera renders only the chosen layers. Minimap icons are on "Minimap", so the main camera excludes them.
- **ScriptableObject?** An asset that holds data. Shared, editable, no scene object needed.
- **Why pooling?** Instantiate/Destroy allocate memory and trigger the GC. Reusing objects avoids frame spikes.
- **`Physics.CheckBox`?** "Would a box here overlap any collider?" Returns a bool and allocates nothing.
- **`Time.deltaTime` vs `fixedDeltaTime`?** Variable frame time vs the fixed physics step (0.02 s).
- **SmoothStep / k² / ease-out?** Easing curves that remap 0..1 time: slow-slow, slow-fast, fast-slow.
- **`Mathf.PingPong(t, 1)`?** A triangle wave 0→1→0.
- **Dot product > 0.3?** cos(angle) > 0.3, so within about 72° of facing.

## 7. E3 export (upload)
1. Project window → select the exercise folder **and** `Assets/_Project/Scripts/Core`.
2. Right-click → **Export Package…** → leave **Include dependencies** on (pulls in materials and the CC0 sounds) → check the list has no other members' folders → Export `Henry_<Code>.unitypackage`.
3. Test: new empty URP project with the Input System → import → open the scene → Play. (Layers such as "Minimap" are project settings, so re-run the Magic setup or add the layer if icons show in the main view.)
4. Upload with the README.

## 8. If something breaks during the demo
| Symptom | Fix |
|---|---|
| Red compile error after a live edit | Cmd+Z in the IDE, or `git restore <file>`; wait for recompile |
| A scene object is missing or broken | run **Magic > Mini-Exercises > Henry <code>: Set Up Scene** (only creates what's missing) |
| Keys do nothing | click inside the **Game view** first (focus) |
| Everything frozen in 4.A.1 | you pressed P (`timeScale` 0): press P again |
| Dominoes don't come back | something lies on their spot (by design); press R |
| Hamster falls through the floor after an edit | stop, Play again; check the CharacterController wasn't removed |
| No sound | Game view "Mute Audio"; AudioSource assigned on ClubPanel / Hamster |
| Unity freezes | play the backup recording, then explain the code |

## 9. Mindset
Show the HUD proof before you explain. Say the trade-offs before the TA finds them (sheet 3 §8). For a code change, say what you'll change first, keep it to 1–5 lines, stop Play Mode before editing, and press Play again.
