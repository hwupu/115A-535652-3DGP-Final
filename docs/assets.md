# Asset Register & Policy

## Policy
This is a non-commercial class project (decided in #12). **There is no allow list of sources.** **CC0 and CC-BY are preferred.** Other assets may be used as fair use for this course, as long as they are credited.

1. **Unity Asset Store assets (free or paid) are NEVER committed.** Their licenses forbid redistribution, and this repo is public. They live in `Assets/AssetStore/`, which is git-ignored. Each member imports them locally: *Window > Package Manager > My Assets* (sign in with the account that owns/claimed the asset; free assets must be "Added to My Assets" by each member).
2. **Assets from other sources** (Kenney, Quaternius, Poly Haven, OpenGameArt, Sketchfab, own work, ...) go in `Assets/ThirdParty/<Pack>/`. Binary files go through LFS automatically.
3. **No row, no commit.** An asset is committed only after it has a row below **and** in `CREDITS.md`, in the same PR. Each row gives the name, author, URL, version, license (or "fair use"), who added it, where it lives, and whether it is committed. AI agents remind the team of this, and they never commit an asset that is not registered.
4. **Commit only what we use (1 GB LFS quota).** Don't commit whole packs. Copy in only the models, textures and sounds the game actually uses, and remove unused ones before the PR. Keep the full download outside the repo or in a git-ignored folder. Prefer compressed formats (`.png`/`.jpg` over `.tga`/`.psd`, `.ogg` over `.wav`) and textures at 2K or smaller. Check usage with `git lfs ls-files -s`, and flag any PR that adds more than about 50 MB.
5. Scenes/prefabs may reference Asset Store content. Package GUIDs are stable across machines, but a member without the pack will see missing references. So list the exact pack **and version** here so everyone imports the same one.
6. Don't modify third-party files in place. Make copies or variants under `Assets/_Project/`.
7. Unsure where an asset comes from? Mark `needs-po`, and don't commit it.

## Register
| Asset | Author / Publisher | Source URL | Version | License | In git? | Path | Added by |
|-------|--------------------|-----------|---------|---------|---------|------|----------|
