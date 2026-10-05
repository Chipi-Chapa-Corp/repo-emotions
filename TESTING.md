# Test record

Environment: Fedora Linux, Steam Flatpak / Proton, R.E.P.O. 0.4.4.3, Unity 2022.3.67, BepInEx 5.4.23.2.

- Release build: passed, 0 warnings, 0 errors.
- Production state-machine tests: 20 checks passed (`./scripts/test.sh`).
- Real game launch: passed. BepInEx reports `Loading [MMB Emote Wheel 1.0.1]`, followed by `MMB Emote Wheel ready` and `Chainloader startup complete`.
- Initial interactive verification: failed; the user reported no wheel in Tutorial. A diagnostic build confirmed that plugin Update never ran, despite successful Awake/startup.
- Loader fix: changed `[Preloader.Entrypoint] Type` from `Application` to `MonoBehaviour`. Restart verified: `Wheel Update running; mouse=Mouse`, all three expected patches registered, and gate changes logged. Interactive verification passed: the user confirmed the wheel works. Runtime logs show MMB press/release, multiple native expressions starting for 5 seconds, expiry, and cancellation when the pause menu opens.
- Multiplayer reception: not yet tested with a second player.

Interactive checklist:

1. In Tutorial or an active game, hold MMB. Wheel appears; moving the mouse changes the highlighted expression without moving the camera.
2. Release over an expression. Wheel closes, the native expression HUD reacts, and the expression expires after approximately five seconds (plus the game's short blend-out).
3. Repeat for all six expressions.
4. Release in the center: no new expression.
5. Press Escape while holding: selection cancels; reopening the game menu does not leave an expression running.
6. Open chat, pause, or alt-tab: selection cancels. MMB works again after returning and pressing it afresh.
7. Original number-key expressions continue working.

## 1.1.0 visual update

- Replaced IMGUI/text panels with a native uGUI canvas, subdued radial background, ochre selection edge and icon-only eye previews.
- Eye art is built at runtime from the six actual expression settings, including the closed-eyes expression. No game art is redistributed.
- Reviewed `artifacts/wheel-preview.png`, generated from the production rasterizer using eye settings read from the installed game.
- Release build: 0 warnings/errors. All 20 behavior checks still pass.
- In-game runtime verification: 1.1.0 loaded with all three patches, and live logs recorded selection/playback of sad, crazy, closed-eyes and unimpressed expressions. No plugin exceptions appeared in Player.log. Reviewed `artifacts/wheel-open-in-game.png`: all six eye previews, radial segments, center cancel mark and pointer render correctly in the live game. `artifacts/wheel-in-game.png` also confirms the active eye/timer indicator. The user accepted the visual update.

## 1.1.1 indicator removal and packaging

- Removed the active-expression preview, duration track and duration bar entirely from the view. The native game expression HUD remains.
- The wheel canvas is disabled after its close animation, regardless of the active expression timer.
- Release build: 0 warnings/errors. All 20 behavior checks passed.
- Updated DLL installed while the game was closed; installed and distribution copies match.
- Thunderstore ZIP checked for required root files, 256×256 PNG icon, matching versions, exact DLL bytes, expected file list and archive integrity.
- Downloaded the current declared BepInExPack dependency and confirmed its default entrypoint is still Application. Public installation instructions document the required MonoBehaviour setting without shipping a replacement shared config.
- The 1.1.1 UI-only removal has not had a new interactive game test. Cross-platform/mod-manager and second-player multiplayer tests remain outstanding.
