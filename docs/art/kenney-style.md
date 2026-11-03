# Kenney style for all levels (proposal for L2 and L3)

L1 uses the **Kenney Platformer Kit** and **Mini Forest** (CC0) at ×2 scale: a 4 m grass block, a 4 m tree, next to the 1.3 m hamster (*Magic > Prefabs > Create Kenney Prefabs*, *Magic > Level > Garden: Build Kenney Level*). Using the same kits in L2 and L3 keeps the game looking like one game. The owners decide (#75, #76).

## How the Kenney models are coloured
Every model of a kit uses **one small palette texture** (`colormap.png`, 512 px of colour stripes). The UVs point at those stripes, so a model gets new colours from a different palette. The mesh doesn't change. That's why we never need duplicate models:

- **One recoloured palette per theme.** Copy `colormap.png` and repaint the green grass stripes in sand colours for the desert. Then make one material on it (like `Materials/Kenney/KenneyPlatformer.mat`).
- **Prefab variants, not copies.** Make a *Prefab Variant* of each `Prefabs/Kenney/Platformer/*.prefab` that overrides only the material. A fix to the base prefab (collider, scale) reaches every variant.

## L3 Frozen Lake: snow comes with the kit
The Platformer Kit has snow versions of the blocks and trees: `block-snow-*` (42 models), `tree-snow`, `tree-pine-snow`, `tree-pine-snow-small`. Copy the ones used from `kenney_platformer-kit.zip` (Henry has it; free at https://kenney.nl/assets/platformer-kit) into `Assets/ThirdParty/Kenney/PlatformerKit/Models`, and add them to the catalog in `KenneyPrefabsMenu`. They need no recolouring. For ice, a light-blue palette variant of the same blocks works.

## L2 Windy Desert: recolour, plus a few props
- Ground and border: the same `block-grass-*` models on a **desert palette** (grass stripes become sand, dirt stripes become sandstone).
- Props from the kits that suit a desert: `rocks-low`, `stones`, `barrel`, `crate`, `sign`, `fence-straight`, `poles`, `spike-block`, `trap-spikes`, `flag`, `tent` (Mini Forest).
- No cactus or palm in these two kits. If needed, look for another CC0 Kenney kit (kenney.nl) and register it in `docs/assets.md` and `CREDITS.md`.

## Rules that keep it uniform
- Scale ×2 (`KenneyPrefabsMenu.Scale`). Blocks on a 3.125 m grid overlap ~1 m, so the rounded corners leave no holes.
- URP Lit on the kit palette, smoothness 0.15. No other textures on Kenney models.
- Ground blocks get box colliders; small decor (flowers, grass, stones) gets none.
- Sky: the Kenney skyboxes (`Assets/ThirdParty/Kenney/Skyboxes`, see `KenneySky`).
- Commit only the model files you use (`Assets/ThirdParty/Kenney/...`), each pack with a row in `docs/assets.md` and `CREDITS.md`.

A shared Magic tool could make the desert and ice palettes and the prefab variants for both levels (ask Henry / Claude).
