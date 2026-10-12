# Asset Register & Policy

## Policy
1. **Unity Asset Store assets (free or paid) are NEVER committed.** Their licenses forbid redistribution, and this repo is public. They live in `Assets/AssetStore/` which is git-ignored. Each member imports them locally: *Window > Package Manager > My Assets* (sign in with the account that owns/claimed the asset; free assets must be "Added to My Assets" by each member).
2. **Assets from other sources** (Kenney, Quaternius, OpenGameArt, Poly Haven, own work, ...) may be committed to `Assets/ThirdParty/<Pack>/` **only if the license allows redistribution** (e.g. CC0, CC-BY). Binary files go through LFS automatically.
3. **Every asset gets a row below and in `CREDITS.md`** before the PR is merged: name, author, URL, version, license, who added it, where it lives, and whether it is committed.
4. Scenes/prefabs may reference Asset Store content. Package GUIDs are stable across machines, but a member without the pack will see missing references — so list the exact pack **and version** here so everyone imports the same one.
5. Don't modify Asset Store files in place; make copies/variants under `Assets/_Project/`. (Our copies of derived content must also respect the license — ask if unsure.)
6. Unsure about a license? Mark `needs-po`, don't commit.

## Register
| Asset | Author / Publisher | Source URL | Version | License | In git? | Path | Added by |
|-------|--------------------|-----------|---------|---------|---------|------|----------|
