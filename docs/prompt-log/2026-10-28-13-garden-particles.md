# 2026-10-28 13 — Garden particle effects (#39)

- **Who:** Henry (@hwupu) + Claude (Opus 5.5)
- **Related:** #39, #70, #31

## Prompt (verbatim)
> all green proceed

## What the AI did
- Merged #126 (#70). Started #39 with the set the PO chose (2026-10-28): five Garden particle systems as prefabs Henry can replace.
- Placeholder prefabs in `Prefabs/FX` (made by `ParticlePrefabs`, never overwritten): `FX_ImpactSparks`, `FX_Confetti` (shared), `FX_WaterSplash`, `FX_CatZzz`, `FX_FallingLeaves` (Garden); materials `Materials/FX/ParticleSoft` (transparent dot) and `ParticleSolid`.
- Runtime: `Effects/ShotImpactEffect` (sparks at the ball on every shot), `Effects/BallTriggerEffect` (splash in the pond, confetti in the cup), `Levels/Garden/CatSleepEffect` (Zzz while the cat sleeps). One-shot effects are one reused scene instance each (moved and replayed), so nothing is spawned at runtime.
- *Magic > Level > Garden: Add Particle Effects* places and wires them in L1.
- Play-mode test: `EffectsTests` (1). Architecture §1.

## Decisions / open questions
- Henry: the five effects as prefabs he can replace later.
- Agent: the cup confetti plays when the ball enters the cup's trigger (works before Pongpat's Cup/ScoreKeeper; if #124 replaces the cup object, run the tool again).
