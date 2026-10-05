# Refined Emotions

Hold **middle mouse** to open an eye-icon wheel for R.E.P.O.'s six built-in expressions. Point toward an expression and release to play it for five seconds.

- Six eye previews with matching arm poses: angry, sad, pointing, hands behind the back, hands on the head and hands at the mouth.
- Subdued radial UI, warm yellow selection and gentle hover animation.
- Release in the center or press Escape to cancel.
- Camera aiming pauses while the wheel is open.
- Uses the game's existing expression display. No duplicate preview or countdown.
- Hold V to see yourself from the front, including while using the wheel. Release to return to first person.
- Original number-key controls get the same poses.
- Arm poses yield to grabbing, map use, crawling and tumbling.

## Installation

Install with a Thunderstore-compatible mod manager; BepInEx and REPOConfig are dependencies; REPOConfig installs MenuLib. Launch the game through the same mod-manager profile.

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

Install [BepInEx 5](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/) and [REPOConfig](https://thunderstore.io/c/repo/p/nickklmao/REPOConfig/) with its MenuLib dependency first. Copy the ZIP's `BepInEx` folder into the game directory, merging folders, so the plugin is at:

```text
REPO/BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll
```

Apply the loader startup setting above. If upgrading from MMB Emote Wheel, remove its old mod-manager entry before installing; keep your config. Keep only one copy of the plugin.

### Linux / Steam Deck

Proton also needs the native `winhttp` override. Follow the [BepInEx Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html). For a manual Steam installation, launch options can be:

```text
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

Preserve any existing launch options or overrides; a mod manager may already configure them.

## Configuration

Open **Mods → Refined Emotions → Emote wheel** in REPOConfig.

- **Button**: hold to open the wheel, default **Mouse2** (middle mouse).
- **Self-view**: hold for a front-facing third-person view, default **V**.

Changes apply immediately. Release Self-view to restore first person. Mouse-look pauses while viewing yourself. Escape remains the wheel cancel key. Emotes last five seconds.

You can also edit `BepInEx/config/local.repo.mmbemotewheel.cfg` with the game closed:

```ini
[Emote wheel]
Button = Mouse2
Self-view = V
```

## Compatibility

Built and tested against R.E.P.O. **0.4.4.3** on Linux/Proton with BepInEx 5.4.23.2. Also verified loading through Gale with BepInExPack 5.4.2305, REPOConfig 1.2.6 and MenuLib 2.5.2. Windows has not been tested.

Arm poses follow the game's native expression synchronization, including number-key expressions. Other players need this mod to see the added poses; players without it still see the native eye expressions. Multiplayer pose reception has not yet been tested. Cosmetics and rewards are not modified.

## Troubleshooting

Check `BepInEx/LogOutput.log`. A working launch logs `Wheel Update running; mouse=Mouse` and the installed patches. If it only logs `Refined Emotions ready`, check the startup setting above.

The wheel and self-view are available during active play. They cancel in menus, chat, loading, after death, or when the window loses focus. Release and press the binding again to resume. Self-view yields to native special camera modes.

## Uninstall

Remove the mod in your mod manager, or delete `BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll` with the game closed.
