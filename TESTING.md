# Release verification — 1.3.7

## Installation contract

Install Refined Emotions in a fresh Thunderstore-compatible mod-manager profile and launch modded. The package declares BepInExPack and REPOConfig; REPOConfig declares MenuLib. There must be no copied profile settings, manual loader edits, or pre-existing mod configuration. Middle mouse and V are supplied by the plugin on first launch. Changed bindings remain optional.

The plugin registers for scene loading in Awake and creates its wheel/self-view runtime after a scene loads. It neither reads nor writes BepInEx entrypoint settings. The development installer also preserves existing loader configuration. The release ZIP contains the plugin DLL and package metadata, with no shared configuration files.

## Completed checks

- Release build: zero warnings/errors.
- 56 automated checks pass, including 11 lifecycle checks against production RuntimeHost using a Unity stand-in. These do not verify engine behavior.
- Installer fixtures preserve absent, Application and MonoBehaviour loader configurations and copy the exact distribution DLL.
- ZIP validation checks versions, required dependencies, exact contents, DLL bytes and archive integrity.

## Required clean-profile game test

Pending. A fresh launch is needed; the existing running game cannot load the new DLL.

1. Create a new profile and import the 1.3.7 ZIP through the manager. Let the manager install declared dependencies. Do not copy old configs or edit loader settings.
2. Launch modded. Confirm 1.3.7 loads, `Wheel Update running` appears, and the generated loader entrypoint remains the dependency's default.
3. Enter Tutorial or active play. Confirm middle mouse opens the wheel, moving the mouse selects an expression without moving the camera, and releasing plays it for five seconds.
4. Hold V to see the character and release to return. Verify center/Escape cancellation and the six original number-key expressions.
5. Change scenes and repeat. Confirm only one runtime and wheel exist. Open menus/chat and lose focus to check cancellation.
6. Restart the same profile and repeat without editing anything. Separately check an existing profile with the old loader workaround and custom bindings.

Windows and two-player pose reception remain unverified. Do not treat earlier successful launches with modified loader settings as clean-install verification for this release.

[Historical test records](docs/testing-history.md) retain prior results and superseded workarounds for development reference.
