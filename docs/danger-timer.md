# Danger timer (#7)

`DangerManager` implements the approved [architecture §4.5](architecture.md#45-danger-timer--pongpat)
and `IDangerTimer`. It counts down in scaled game seconds while aiming or walking. Every shot deducts
the level's cost immediately and suspends the countdown until the ball stops. Out-of-bounds resets
already raise `GolfBall.CameToRest`, so they need no separate failure rule.

## In Unity

1. Run **Magic > Game > Create Danger Configs**. This creates only missing assets in
   `Assets/_Project/Settings/Danger/`: `L1_Garden`, `L2_Desert`, `L3_Lake`.
   The command focuses the Project window and selects/highlights the Danger folder, including on reruns.
   Each level owner enters their four values in the Inspector. No difficulty values are chosen by the tool.
   Use finite values with `StartSeconds > 0`, `ShotCostSeconds >= 0`, and
   `0 <= CriticalBelowSeconds <= WarningBelowSeconds <= StartSeconds`. Invalid or missing configs
   log a `[Danger]` error and leave the timer stopped.
2. Run **Magic > Game > Create Danger Manager Prefab**. It creates
   `Assets/_Project/Prefabs/Shared/DangerManager.prefab` without touching open scenes.
   Its Config is intentionally empty: assign the matching config to each level's scene instance
   when filling the shared systems slots (#95). Both commands log `[Magic]` Created / Skipped lines;
   rerunning them preserves existing assets and tuning. There are no confirmation dialogs or Build Settings changes.
3. Open **Window > General > Test Runner**, select **EditMode**, and run `DangerManagerTests`.
   Verify all tests pass and the Console has no unexpected errors. Let Unity generate the new scripts' `.meta` files.

## LevelManager handoff (#27)

The timer does not start itself or discover other components. The level manager holds its serialized
reference, exposes it through `ILevelFlow.Danger`, and owns these connections:

```csharp
shot.ShotTaken += danger.ApplyShot;
ball.CameToRest += danger.OnBallAtRest;
danger.StateChanged += OnDangerChanged; // Failed -> level failure
danger.Begin();
```

Unsubscribe on disable. Call `Pause` / `Resume` for the menu; call `Pause` when all goals are met
**before** handling a later ball-rest event. Flight suspension and external pause are independent:
closing a menu during flight cannot restart the clock, and a ball stopping while the menu is open
cannot unpause it. At zero remaining time, the final shot publishes Critical (and full progress),
then publishes Failed only on rest if the level has not paused the timer for completion. `Resume`
after a zero-budget shot has stopped also fails. `ResetDanger` restores the configured values and
leaves the timer stopped; `Begin` resets and starts an attempt. Failure cannot be resumed.

`StateChanged` reports the newly entered state; `OnWarning`, `OnCritical`, and `OnFailed` are
convenience events for entry into those states. A large time step or shot can skip intermediate
states. `Progress01` is 0 at a full budget and 1 at exhaustion. `ProgressJumped` fires once for every
accepted shot (including a zero-cost shot), after the state and remaining time update. Config values
are sampled at reset/start, so Inspector tuning takes effect on the next attempt.

Production HUD, cat, and tutorial wiring depends on `ILevelFlow.Danger` from #27. The existing
`AvatarWiring_TEMP` returns null for that property; adding the prefab alone does not connect those views.

### Scripted demo shots and god keys (#143)

`ShotController.TakeScriptedShot` publishes the same `ShotTaken` event as a player shot, so the
connections above charge it once and suspend countdown during both its launch delay and flight.
Keep this suspension tied to `ShotTaken`, rather than the later `GolfBall.Launched` event.

The current `GolfBall.FlyTo` publishes `CameToRest` before its landing callback. F5 then waits another
0.4 seconds before forcing the goals. When #27 connects the real timer, this demo path must arrange
completion/pause before rest if the shot exhausts the budget; otherwise the required failure-at-rest
behavior wins that race. Coordinate this ordering with the ball/god-key owner; do not defer ordinary
shot failure to accommodate the demo.

F6 uses `ILevelCheats.ForceFail` on the level flow. Preserve that interface when replacing
`AvatarWiring_TEMP` with LevelManager. It is separate from the read-only `IDangerTimer` contract;
the approved timer API does not currently expose a forced-failure method.
