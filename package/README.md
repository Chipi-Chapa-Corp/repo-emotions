# MMB Emote Wheel

Hold **middle mouse** to open an eye-icon wheel for R.E.P.O.'s six built-in expressions. Point toward an expression and release to play it for five seconds.

- Six eye previews drawn from the game's expression settings.
- Subdued radial UI, warm yellow selection and gentle hover animation.
- Release in the center or press Escape to cancel.
- Camera aiming pauses while the wheel is open.
- Uses the game's existing expression display. No duplicate preview or countdown.
- Original number-key controls continue to work.

## Installation

Install with a Thunderstore-compatible mod manager; BepInEx is listed as a dependency. Launch the game through the same mod-manager profile.

**Loader startup setting:** with the game closed, check the profile's `BepInEx/config/BepInEx.cfg` and use:

```ini
[Preloader.Entrypoint]
Assembly = UnityEngine.CoreModule.dll
Type = MonoBehaviour
Method = .cctor
```

Change the existing section rather than adding a duplicate. On the tested R.E.P.O. build, the default `Type = Application` loaded the plugin but never ran its input or UI callbacks. The package does not overwrite your shared BepInEx configuration.

If the file does not exist yet, launch and quit the modded game once. After changing it, restart the game.

### Manual installation

Install [BepInEx 5](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/) first. Copy the ZIP's `BepInEx` folder into the game directory, merging folders, so the plugin is at:

```text
REPO/BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll
```

Apply the loader startup setting above. If migrating from a previous manual/mod-manager install, keep only one copy of the plugin.

### Linux / Steam Deck

Proton also needs the native `winhttp` override. Follow the [BepInEx Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html). For a manual Steam installation, launch options can be:

```text
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

Preserve any existing launch options or overrides; a mod manager may already configure them.

## Configuration

After first launch, duration can be changed in `BepInEx/config/local.repo.mmbemotewheel.cfg`:

```ini
[General]
DurationSeconds = 5
```

Supported values: 0.5 to 30 seconds. Restart after editing. The duration is not displayed on screen.

## Compatibility

Built and tested against R.E.P.O. **0.4.4.3** on Linux/Proton with BepInEx 5.4.23.2. The manifest requests the current Thunderstore BepInEx pack, 5.4.2305; that exact mod-manager installation and Windows have not been tested yet.

This is a local input/UI mod using the game's native expression synchronization. A second-player multiplayer reception test is still pending. Cosmetics and rewards are not modified.

## Troubleshooting

Check `BepInEx/LogOutput.log`. A working launch logs `Wheel Update running; mouse=Mouse` and the three installed patches. If it only logs `MMB Emote Wheel ready`, check the startup setting above.

The wheel is available during active play. It closes in menus, chat, loading, after death, or when the window loses focus.

## Uninstall

Remove the mod in your mod manager, or delete `BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll` with the game closed.
