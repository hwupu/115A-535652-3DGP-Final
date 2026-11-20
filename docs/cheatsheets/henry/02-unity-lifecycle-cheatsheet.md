# Cheat Sheet 2: Unity event functions, with the mini-exercise code

Unity calls methods with special names on every enabled `MonoBehaviour`. You never call `Update()` yourself.
Companion: [01-csharp-cheatsheet.md](01-csharp-cheatsheet.md).

---

## 1. Master timeline (one object, one frame)

```
Scene loads / object created
  Awake()          set up MYSELF (cache components, remember rest pose)          once; runs if the GameObject is active, even if the component is disabled
  OnEnable()       connect to others (subscribe events); runs again every time it is re-enabled
  Start()          set up using OTHER objects (they have all run Awake by now)    once, before the first Update

Every frame
  FixedUpdate()    0..n times per frame, fixed 0.02 s steps (physics)            (not used in Henry's exercises)
  [physics step]   -> OnTriggerEnter / OnCollisionEnter
  Update()         per-frame logic: input, timers, movement
                   CharacterController.Move() inside Update -> OnControllerColliderHit (synchronously)
  [coroutines resume after Update: yield return null]
  LateUpdate()     after everyone's Update: cameras, applying joint angles
  [render]
  OnGUI()          immediate-mode GUI, can run several times per frame

Disable / destroy
  OnDisable()      disconnect (unsubscribe events)
  OnDestroy()      (not used in Henry's exercises)
```
**Order between two different objects' `Update`s is not guaranteed.** The exercises avoid depending on it by using `LateUpdate` where order matters (§3.5).

---

## 2. Who uses what (index)

| Method | 4.C.4 dominoes | 4.E.3 game UI | 4.A.1 hierarchy | 4.A.3 portals |
|---|---|---|---|---|
| `Awake` | `RigidbodyPusher`, `VanishAfterHit` | `ClubSelector`, `SelectableCard`, `GolfBall`, `MinimapCamera` | `AxisJoint`, `SineMotion`, `FlyingBall` | `PortalTraveler`, `ScreenFader`, `PortalDemoHud` |
| `OnEnable/OnDisable` | – | `GameUiHud` | `SineMotion` | `PortalDemoHud` |
| `Start` | `DominoArena` | `SelectionPanel`, `GolfSwing`, `GameUiHud` | `ToolSwitcher` | `Portal` |
| `Update` | `ThirdPersonWalker`, `VanishAfterHit`, `DominoArena`, `ArenaHud` | `ShotTimer`, `GolfSwing`, `GolfBall`, `BarView`, `SelectableCard`, `SelectionPanel`, `GameUiHud` | `SwingAnimator`, `SineMotion`, `FlyingBall`, `AxisJoint` (key), `ToolSwitcher` | `ThirdPersonWalker` |
| `LateUpdate` | `OrbitFollowCamera` | `OrbitFollowCamera`, `MinimapCamera`, `MinimapIcon` | `AxisJoint` (apply angle), `OrbitFollowCamera` | `OrbitFollowCamera` |
| Physics callbacks | `OnControllerColliderHit` (pusher), `OnCollisionEnter` (tile) | – | – | `OnTriggerEnter` (`Portal`, `PortalUnlockSwitch`) |
| `OnGUI` | `ArenaHud` | – (uses a uGUI Canvas) | `HierarchyDemoHud` | `PortalDemoHud` |
| Coroutine | – | – | – | `PortalTraveler.TravelRoutine`, `ScreenFader.FadeTo` |

---

## 3. One method at a time

### 3.1 `Awake`: "set up myself"
```csharp
void Awake() => _restRotation = transform.localRotation;   // AxisJoint: the pose built in the Editor = angle 0
void Awake() => _tee = transform.position;                  // FlyingBall: remember the tee
void Awake() => Body = GetComponent<Rigidbody>();           // VanishAfterHit: cache the component once
```
`ClubSelector.Awake` writes the card texts. It runs before any `Start`, so the cards already show the right text on the first frame.

### 3.2 `OnEnable` / `OnDisable`: "connect / disconnect", or "start / stop"
- `GameUiHud`: subscribes to `Meter.Changed` and `GolfSwing.Shot`, and unsubscribes in `OnDisable`.
- `SineMotion` uses them as **start/stop**: `OnEnable` restarts the sine at t = 0 (so it starts at the rest pose, no jump), and `OnDisable` puts the club back to its rest pose. `SwingAnimator.Enter` simply writes `waggle.enabled = phase == Phase.Address;`, and Unity calls these two methods for you.

### 3.3 `Start`: "set up using others"
```csharp
void Start() => _balls = FindObjectsByType<GolfBall>(FindObjectsSortMode.None);   // GolfSwing: find once
```
`DominoArena.Start` creates the 100 tiles (the `Instantiate` loop) and calls `BuildLines()`. `SelectionPanel.Start` hooks up the card clicks and selects the first card **silently** (`playEffects: false`, so no sound at scene start). `GameUiHud.Start` draws the bar once before any event arrives.

### 3.4 `Update`: per-frame logic
- **Input** (Input System, polled): `Keyboard.current.fKey.wasPressedThisFrame` (pressed this frame) vs `.isPressed` (held down).
- **Timers**: `_timeLeft -= Time.deltaTime` (VanishAfterHit), `time.Subtract(Time.deltaTime)` (ShotTimer), `_time += Time.deltaTime` (SwingAnimator).
- **Movement without physics**: `GolfBall.Update` (ease-out + hop), `FlyingBall.Update` (Euler integration), `ThirdPersonWalker.Update` (`CharacterController.Move`).
- `Time.deltaTime` = seconds since the last frame (~0.016 at 60 fps). Multiplying by it makes speeds **per second** instead of per frame.

### 3.5 `LateUpdate`: "after everyone has moved"
Two uses here:
1. **Cameras** (`OrbitFollowCamera`, `MinimapCamera`): the hamster moved in its `Update`, so the camera follows its final position. No one-frame lag, no jitter.
2. **4.A.1 joints**: `SwingAnimator.Update` *sets* `shoulder.Angle`. `AxisJoint.LateUpdate` *applies* it:
   ```csharp
   void LateUpdate() => transform.localRotation = _restRotation * Quaternion.AngleAxis(Angle * direction, axis);
   ```
   This way it doesn't matter which `Update` Unity runs first: by `LateUpdate` the angle for this frame is final.

### 3.6 Physics callbacks
| Callback | When | Requirement | Used in |
|---|---|---|---|
| `OnControllerColliderHit(hit)` | **during** `CharacterController.Move()`, for every collider touched | object has a CharacterController | `RigidbodyPusher` pushes the tile |
| `OnCollisionEnter(collision)` | two solid colliders start touching | at least one has a non-kinematic Rigidbody; called on **both** | `VanishAfterHit` (tile hits tile) |
| `OnTriggerEnter(other)` | a collider enters a trigger (`isTrigger`) | a Rigidbody or CharacterController involved (the portal has a kinematic Rigidbody) | `Portal`, `PortalUnlockSwitch` |

**Key fact for 4.C.4:** a CharacterController moves kinematically, so the physics engine doesn't push anything for it, and pushing a tile does **not** raise `OnCollisionEnter` on that tile. That's why `RigidbodyPusher` applies the force itself and tells the tile through `IPushReceiver.OnPushed`.
Trigger = detect overlap but don't block. Collision = solid contact with a physics response.

### 3.7 `OnGUI`: debug HUDs
`ArenaHud`, `HierarchyDemoHud` and `PortalDemoHud` draw a `GUI.Box` with text, with no Canvas needed. `OnGUI` can run several times per frame, so `ArenaHud` builds the string in `Update` only every 0.1 s and `OnGUI` just draws it. 4.E.3 is the UI exercise, so it uses a real **uGUI Canvas** (`BarView`, `SelectableCard`, `RawImage` minimap).

### 3.8 Coroutines in the frame
`StartCoroutine` runs the coroutine up to its first `yield` immediately. After that, `yield return null` resumes it **once per frame, after `Update`**. A disabled or destroyed object stops its coroutines.

---

## 4. Time: scaled vs unscaled (4.A.1 pause key P)
`HierarchyDemoHud`: `Time.timeScale = 0` → `Time.deltaTime` becomes 0 → every `Update` that multiplies by it freezes (swing, sine wiggle, ball).
Things that must keep animating while paused use **`Time.unscaledDeltaTime`**: `ScreenFader.FadeTo`, `BarView` (SmoothDamp), `SelectableCard` (scale). `Update` itself still runs when paused, which is why P can un-pause.

---

## 5. Walkthroughs (frame by frame)

### 5.1 4.C.4: one domino line falls
1. `ThirdPersonWalker.Update` → `_controller.Move(...)` into tile 0.
2. Inside `Move`: `RigidbodyPusher.OnControllerColliderHit` → `AddForceAtPosition(impulse, hit.point, Impulse)` (force at the contact point gives torque, so the tile tips) → `IPushReceiver.OnPushed` → `VanishAfterHit.MarkHit` → `Hit` event → `DominoArena.OnHit` colours it orange.
3. Next physics steps: tile 0 falls onto tile 1 → `OnCollisionEnter` on **both** → `BodyContact` → `_collided.Add` (HUD count) and `MarkHit` → ... the chain continues to tile 9.
4. Each tile's `Update` counts down `_timeLeft` from 5 s → `SetActive(false)` → `Vanished` → `DominoArena.OnVanished` → `TryRespawn`: `Physics.CheckBox` at the home spot. Free: `Place` (reset, white, zero velocity, active). Blocked: added to `_waitingForSpot`, which `DominoArena.Update` retries every frame.

### 5.2 4.E.3: one shot
1. `GolfSwing.Update` → `FindBallInReach` (distance < reach and `Dot(forward, offset) > 0.3`).
2. F → `powerGauge.Begin()`; each frame `powerGauge.Tick(dt)` → `Value01 = PingPong(phase*2, 1)`; `GameUiHud.Update` polls it for the power bar.
3. F again → `Swing()`: `power = Stop()`, `distance = club.maxDistance * power`, direction rotated by a random angle (accuracy) → particles `Emit` + `ball.Hit` + `timer.ChargeShot()` + sound + `Shot` event.
4. `ChargeShot` → `Meter.Subtract(10)` → `Set` → `Changed` → `GameUiHud.OnTimeChanged` → `BarView.SetValue` (slides with SmoothDamp in `BarView.Update`), colour, label.
5. `GolfBall.Update` animates the slide over `_duration` seconds. `MinimapCamera.LateUpdate` follows the hamster, and the RenderTexture shows it in the corner.

### 5.3 4.A.1: one swing
1. Address: `SineMotion.Update` turns C_Club by `8° × sin(2π·1.2·t)`.
2. F → `Enter(Backswing)` → `waggle.enabled = false` → `SineMotion.OnDisable` restores the rest pose.
3. Backswing (1.2 s): `k = SmoothStep(0,1,t/1.2)`, `shoulder.Angle = 120k`, `wrist.Angle = 60k`. `AxisJoint.LateUpdate` applies them.
4. Downswing (0.2 s): `eased = k²`, `shoulder.Angle = Lerp(120, -110, eased)`. When it crosses 0, `LaunchBall()` → `FlyingBall.Launch(velocity, side)`.
5. `FlyingBall.Update`: gravity + curve, bounce, roll; after 5 s `ResetToTee`. FollowThrough (0.6 s hold) → Return (0.8 s) → Address (`waggle.enabled = true` → `OnEnable` restarts t).

### 5.4 4.A.3: one teleport (0.5 s)
1. The hamster's CharacterController enters the portal's trigger → `Portal.OnTriggerEnter` → not locked, has `PortalTraveler` → `traveler.Travel(this)`.
2. `Travel` → `StartCoroutine(TravelRoutine)` → `FadeTo(1, 0.25)` takes about 15 frames.
3. Screen black: CharacterController **off** → `SetPositionAndRotation(destination)` → `Physics.SyncTransforms()` → controller **on**. Burst at the hamster, `_readyTime = now + 1 s`, `Teleported` → HUD.
4. `FadeTo(0, 0.25)` → `IsTraveling = false`. The cooldown stops the arrival portal from sending the hamster straight back.

---

## 6. Quick reference: which method for which job?
| Job | Method |
|---|---|
| cache `GetComponent`, remember rest pose | `Awake` |
| subscribe / unsubscribe events | `OnEnable` / `OnDisable` |
| find other objects, spawn the pool | `Start` |
| input, timers, non-physics movement | `Update` (× `Time.deltaTime`) |
| camera follow, apply joint angles | `LateUpdate` |
| Rigidbody forces every step | `FixedUpdate` (not needed here: pushes are one-off impulses) |
| push from a CharacterController | `OnControllerColliderHit` |
| something entered an area | `OnTriggerEnter` |
| a multi-step sequence over time | coroutine |

---

## 7. Likely TA questions → short answers
- **"Why is the push in `OnControllerColliderHit` and not `OnCollisionEnter`?"** The hamster is a CharacterController, not a Rigidbody. Unity reports its contacts only through `OnControllerColliderHit`.
- **"Why multiply the impulse by `Time.deltaTime`?"** The callback fires every frame while touching. Scaling by dt gives the same total push at 30 or 144 fps.
- **"Why `LateUpdate` for `AxisJoint`?"** So the joint applies the angle the animator set in this frame's `Update`, whichever script runs first.
- **"Why turn the CharacterController off to teleport?"** It keeps its own position. Moving the transform while it's on gets overwritten by the next `Move`.
- **"Why `Physics.SyncTransforms()`?"** Physics queries (`CheckBox`) and the trigger system only see transform changes after a sync. Without it, two tiles respawned in the same frame could land on each other.
- **"What if the game is paused?"** Scaled timers stop. UI fades and bars use unscaled time and keep working.
