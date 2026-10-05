# Changelog

## 1.3.1

- Fixed arm blending: native expression weights are relative, so held poses now reach their intended angles.
- Put palms against the sides of the head or in front of the mouth for the two shocked expressions, using the actual rig endpoints.
- Made Sad’s arms hang straight down with shoulders forward, and rotated Closed Eyes’ arms further behind the back.

## 1.3.0

- Added Self-view in REPOConfig, default V: hold to see your character from the front, release to restore first person.
- Self-view works with the wheel and number-key expressions, pauses mouse-look, and pulls the camera inward near walls.
- Restore the camera on menus, focus loss, death and rebinding.

## 1.2.0

- Added one REPOConfig setting: wheel button, default Mouse2. Duration is fixed at five seconds.
- Added six matching arm poses driven by native expressions, including number-key emotes.
- Blend poses with the eye expressions and restore normal movement afterward.
- Let grabbing, map use, crawling and tumbling take priority over poses.

## 1.1.1

- Removed the duplicate bottom expression preview and duration bar. The game supplies the expression display.
- Added a distributable package and installation instructions.

## 1.1.0

- Replaced text labels with eye previews derived from native expression settings.
- Added subdued native Unity UI, warm selection highlights and hover animation.

## 1.0.1

- Added startup and input diagnostics.
- Fixed the local loader configuration so Unity executes the plugin callbacks.

## 1.0.0

- Initial six-expression wheel: hold MMB, hover, release to play for five seconds.
