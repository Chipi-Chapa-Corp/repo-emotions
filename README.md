<p align="center">
  <img src="docs/preview.png" alt="R.E.P.O. emote wheel with six eye previews" width="720">
</p>

<h1 align="center">MMB Emote Wheel</h1>

Hold **middle mouse**, point at an expression, and release to play it for **5 seconds**. Release in the center or press **Escape** to cancel.

Six native expressions, eye previews, and a subdued UI. Uses the game's existing expression display.

## Install

Requires **BepInEx 5**. Copy `RepoEmoteWheel.dll` into `BepInEx/plugins/RepoEmoteWheel/`.

Set `Type = MonoBehaviour` under `[Preloader.Entrypoint]` in `BepInEx/config/BepInEx.cfg`, then restart. See [installation and configuration](package/README.md) for mod managers and Proton.

## Build

```bash
./scripts/bootstrap.sh
./scripts/build.sh       # DLL
./scripts/test.sh
python3 scripts/package.py  # Build + Thunderstore ZIP
```

Outputs go to `dist/`. Builds against your installed game; set `GameDir=/path/to/REPO` if needed.

Tested with **R.E.P.O. 0.4.4.3** on Linux/Proton. [Changelog](package/CHANGELOG.md).
