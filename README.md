<p align="center">
  <img src="./docs/preview.png" alt="R.E.P.O. emote wheel with six eye previews" width="720">
</p>

<h1 align="center">Refined Emotions</h1>

<p align="center"><i><b>Express</i></b> yourself to your mates in R.E.P.O</p>

## Usage
Hold `Middle Mouse Button` (scroll wheel) to choose an emote.  
Hold `V` to see yourself.  
Optional: change keys via `Menu` > `Mods` > `Refined Emotions`.

## Install

[![Install from Thunderstore](https://img.shields.io/badge/Install_from-Thunderstore-00B4E6?style=for-the-badge)](https://thunderstore.io/c/repo/p/ChipiChapaCorp/Refined_Emotions/)

Click **Install with Mod Manager**, then **Launch modded** from that profile. Dependencies install automatically. The mod supplies its default controls on first launch; no config edits or previous setup are needed.

Everyone who wants to see the added arm poses needs the mod.

## Build

```bash
./scripts/bootstrap.sh # Download the local .NET SDK and BepInEx build dependencies
./scripts/test.sh      # Run automated checks

./scripts/build.sh     # Build dist/RepoEmoteWheel.dll
# Or build the DLL and a ready-to-upload Thunderstore ZIP in dist/:
python3 scripts/package.py
```
