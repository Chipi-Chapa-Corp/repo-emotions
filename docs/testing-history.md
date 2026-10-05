# Historical test records

These records describe versions through 1.3.6 and the initial 1.3.7 candidate. Loader changes mentioned below are historical workarounds, not current installation instructions. See [current verification](../TESTING.md).

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

## 1.2.0 settings and arm poses

- Release build: 0 warnings/errors. All 24 state-machine checks passed, including cancellation and fresh-press behavior when rebinding.
- Thunderstore ZIP validated; declares REPOConfig 1.2.6, which installs MenuLib transitively.
- Exactly one BepInEx entry is bound: `Emote wheel / Button`, default `Mouse2`. Duration is fixed at five seconds.
- Added arm poses driven by native expression weights on world and HUD avatars. Each frame restores the previous underlying pose before normal game animation, then blends in the expression pose. Grab/map/crawl/tumble states take priority.
- Installed into the existing r2modman Default profile with the loader entrypoint fix (previous config backed up). Distribution and installed DLL match.
- Live settings, binding changes, visual hand placement and multiplayer reception still need verification. SomeEmotesREPO is currently also bound to Mouse2; resolve the shared binding before the interactive test.

- Follow-up: registered the package as `Local-MMB_Emote_Wheel` in the manager, moved the DLL into its managed directory, and added cache/package metadata. Verified one DLL and exact release bytes. The profile now contains only BepInEx, MenuLib, REPOConfig and our mod, so the competing MMB binding is gone. Live verification remains pending.

## Gale clean profile verification

- Created and selected Gale profile `MMB Emote Wheel`. Loaded the full official Thunderstore catalog and imported the release ZIP with Gale CLI. Gale automatically installed BepInExPack 5.4.2305, REPOConfig 1.2.6 and MenuLib 2.5.2 from the manifest dependency graph. Each appears as a normal Thunderstore package; our wheel is a local package.
- Applied MonoBehaviour entrypoint to the profile configuration, preserving a backup. Launched through Gale. Live BepInEx 5.4.23.5 log confirms all three plugins loaded, REPOConfig hooked MenuPageMain.Start, our Update ran and all four Harmony patches registered. Arm appearance and multiplayer reception still require interactive verification.

## 1.3.0 held self-view

- Added `Emote wheel / Self-view`, default `V`, through the existing REPOConfig integration. Config generation verified in the live Gale profile; the previous wheel binding is preserved.
- Moves only the rendered main camera and its children in LateUpdate, restores its original local pose before the next gameplay Update, and uses native ShowSelfOverride/third-person tool visuals. CameraAim and CameraNoise (the gameplay/network camera ancestors) remain unchanged.
- Spherecast shortens the three-meter front offset near walls/props. Native camera overrides take priority.
- Release, rebinding, menus, focus loss, death/loading and component teardown restore normal view. Interrupted holds require release before restarting.
- Release build: zero warnings/errors. All 33 behavior checks passed, including nine hold/release/interruption/rebinding checks. Thunderstore ZIP contents, versions and archive integrity validated.
- Imported 1.3.0 through Gale into `MMB Emote Wheel`; installed DLL hash matches the release. Live startup confirms 1.3.0 and the existing dependencies load, Update runs and patches register.
- Interactive check requested: hold V in a level, select an emote with MMB while holding V, release V. Front framing, arm appearance and collision behavior still await interactive verification.

## 1.3.1 pose correction

- Found the primary cause of incomplete poses: PlayerExpression raises each held expression toward 100 and decays it toward zero in the same Update. Its steady raw weight is about 46–49 depending on frame rate. Native eyes normalize across all weights; the previous arm code divided by 100, leaving roughly half of the native arm pose in place.
- Normalize neutral and expression weights together, preserving native blends and transitions while allowing a held pose to reach its complete rotation.
- Confirmed native mapping: 1 angry, 2 sad, 3 unimpressed/suspicious, 4 closed eyes, 5 crazy/full-open shocked, 6 happy/half-open shocked.
- Confirmed actual rig controls are `code_arm_l`/`code_arm_r`, their forward axes run along the arms, and the native endpoint sockets are left (0,-0.04,0.471) and right (0,0,0.5133). Read head mesh bounds and attachment positions from installed game assets; nothing extracted is distributed.
- Angry now fully reaches 80 degrees raised from down and 10 degrees swept backward. Sad uses straight-down arm directions and 0.08 units of forward shoulder offset in visual-local space. Suspicious points only the character's left arm forward. Closed Eyes sweeps both arms behind the back, slightly inward.
- Full-open shocked targets the sides of the animated head; half-open shocked targets two adjacent points in front of the jaw. Solve arm reach to the native palm socket instead of only aiming the arm at the target. Change longitudinal scale only, preserving arm width; restore rotation, position and scale before native animation and on disable/destroy.
- Release build: zero warnings/errors. All 45 checks passed, including the native weight recurrence at 30/60/144 FPS, neutral/multiple-expression blends, and contact reach for both measured socket offsets at three distances.
- Validated 1.3.1 ZIP and installed through Gale while the game was closed; installed DLL matches release hash. Visual contact/framing and multiplayer still require interactive verification.
- Live 1.3.1 startup and in-level log verified: wheel selections for sad, full-open shocked, angry and half-open shocked, plus repeated self-view holds; no plugin exceptions. Passive screenshots confirmed self-view renders the avatar, but did not capture sustained contact poses, so those are not marked visually passed.

## 1.3.2 shoulder placement

- User accepted the 1.3.1 poses except Sad's subtle shoulder movement and the pointing arm's side origin.
- Sad now moves both shoulder pivots 0.20 units forward in animated torso space, drawing them 30% inward for visible rounded shoulders. The arms still hang straight down.
- Pointing moves only the left shoulder to the torso centerline (x = 0) and front (z = 0.30), preserving shoulder height and the forward arm direction. Normalized expression weights blend these translations; existing restoration removes them before native animation and on teardown.
- Release build: zero warnings/errors; all 45 existing behavior/pose-math checks passed. Package validation and whitespace check passed.
- Installed 1.3.2 through Gale with the game closed; installed DLL hash matches the release. These two visual adjustments await in-game confirmation.

## 1.3.3 pose tuning

- Sad keeps the forward, rounded shoulders and angles the hanging arms outward/slightly forward to give fists more clearance from 3D clothing.
- Pointing now moves the left shoulder halfway from its natural position toward the 1.3.2 front-center target. The right shoulder moves 0.15 torso-local units backward; its arm shares the Angry direction.
- Angry raises arms 130 degrees from straight down with the existing 10-degree backward sweep. The shared direction keeps the pointing pose's second arm consistent.
- Build: zero warnings/errors. All 45 existing checks pass; ZIP validation and whitespace check pass. Installed 1.3.3 through Gale while the game was closed; installed and release DLL hashes match. Clothing clearance and these revised poses await interactive confirmation.

## 1.3.4 mouth clearance and pointing direction

- Mouth-covering pose advances both shoulder pivots 0.18 units in animated torso space before the existing palm contact solve; mouth targets remain anchored to the jaw.
- Interpreted the user's second adjustment as the pointing pose: restore the left shoulder's natural position and aim the arm toward a centerline point 0.75 torso-local units ahead, at shoulder height. This produces a visible inward angle rather than translating the shoulder to the center.
- Pointing's supporting right shoulder keeps its 0.15-unit backward offset; its arm is raised 100 degrees from down. Angry keeps 130 degrees.
- Release build: zero warnings/errors; all 45 existing checks, package validation, and whitespace check pass. Installed 1.3.4 through Gale with the game closed; installed DLL hash matches release. Revised visual clearance/direction await user confirmation.

## 1.3.5 pointing refinement

- Pointing now converges on the torso centerline 1.5 local units ahead (previously 0.75), reducing the inward angle from roughly 19 to 10 degrees on the stock rig.
- Its supporting arm retains the 100-degree lift and backward shoulder offset, but increases the backward sweep from 10 to 30 degrees. The separate Angry pose remains at 130 degrees/10-degree sweep.
- Build: zero warnings/errors; all 45 existing checks, ZIP validation and whitespace check pass. Installed through Gale with the game closed; installed and release DLL hashes match. Latest visual refinement awaits in-game confirmation.

## 1.3.6 Refined Emotions rename

- Renamed the BepInEx display name, startup log/UI object name, README headings/settings paths, and Thunderstore package to Refined Emotions / Refined_Emotions. Kept the plugin GUID and DLL identity for configuration and manual-upgrade compatibility.
- Build succeeded with zero warnings/errors; renamed ZIP passed package validation; installer syntax and whitespace checks passed. No gameplay changes.
- Renamed the existing Gale profile and local package entry while Gale/the game were closed, with database, package and configuration backups under `.tools/gale-rename-backup`. Imported the new ZIP through Gale; verified one plugin DLL, exact release bytes, unchanged config bytes and all original dependencies retained.
- Launched the renamed Gale profile successfully. Live log confirms `Loading [Refined Emotions 1.3.6]`, normal input Update and all four patches registered.

## 1.3.7 startup repair candidate

- Root cause recorded in earlier tests: default Application entrypoint ran plugin Awake but no Update; previous local testing changed the loader setting, while the distributed dependency retained Application. The release therefore depended on an undocumented-in-code local workaround.
- Plugin Awake now subscribes to scene loading. A separate persistent runtime object is created after a scene loads and owns wheel Update/LateUpdate dispatch and SelfView. No runtime component is added to BepInEx_Manager. Repeated scene notifications retain one runtime. Plugin teardown unsubscribes, cancels the camera and wheel, and disables the object before deferred destruction.
- Removed the installer code that rewrote BepInEx.cfg. Fixture installations preserve absent, Application and MonoBehaviour configurations, and copy the exact distribution DLL.
- Release build: zero warnings/errors. All 56 checks pass: 45 existing checks plus 11 lifecycle checks using production RuntimeHost with a Unity stand-in. These verify deferred creation, one runtime across scenes, tick/render dispatch, focus cancellation, disposal and dead-owner handling; they do not prove Unity engine integration.
- Thunderstore ZIP 1.3.7 passes metadata/version, file-list, DLL-byte and archive integrity validation. Existing user README change preserved.
- Not installed or published. The user's game was running, so it was not restarted. Required next verification: launch a clean profile with the declared dependencies and default Application entrypoint; confirm both the input heartbeat and visible wheel/self-view, repeat across a scene change, then repeat with MonoBehaviour. Windows and two-player pose reception remain unverified.
