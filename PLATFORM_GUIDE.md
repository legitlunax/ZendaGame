# Android and browser targets

## Implemented in the project

- Compatibility renderer, shared by desktop, native Android, and WebGL web exports.
- Width-based responsive UI: columns on wide screens; stacked content and vertically
  scrolling page on small screens. Tested at 360x800, 390x844, 800x360, 1280x720.
- Touch-to-mouse emulation for existing buttons and the tap-to-create canvas;
  mouse-to-touch emulation stays disabled to avoid duplicate taps.
- At least 48px purchase controls; wrapping headers, cards, and demo navigation.
- Dialogs constrained to the viewport width; orientation allows sensor rotation.
- Autosave every 15 seconds and when losing application focus or pausing.
- Web single-threaded preset and ARM64 Android debug APK preset.
- Export filters exclude development tests/tools, screenshots, editor addons, and
  unused runtime AI scripts. The core game has no online AI requirement.

## Actual verification status

Windows runs using the Compatibility renderer and automated responsive layout tests.
Android and Web exports were attempted, but neither could complete:

- Matching `4.7.2.stable` export templates are not installed.
- Godot's Android Java SDK path is not configured.
- An Android SDK is installed, but its package compatibility is not verified.
- No physical Android device test or exported browser smoke test has happened yet.

Do not treat this project as certified mobile/browser compatible until those builds
run and pass the device/browser checklist below.

## Export prerequisites

1. In Godot, use Editor > Manage Export Templates to install templates matching
   the exact editor version, 4.7.2.stable. Do not mix engine/template versions.
2. For Android, configure Java SDK Path and Android SDK Path in Editor Settings >
   Export > Android. Official documentation recommends OpenJDK 17; verify the SDK
   packages required by the installed editor version. Java on PATH alone does not
   configure Godot's SDK setting.
3. Review the Android package name before public distribution. The current
   `org.genesisidle.creation` name is a development identifier, not a registered brand.

## Build

```powershell
& 'E:\Godot\GenesisIdleRPG\tools\build_platform.ps1' -Target Web
& 'E:\Godot\GenesisIdleRPG\tools\build_platform.ps1' -Target Android
```

These produce a normal saved game, not a `--demo` era preview.
Web output: `E:\Godot\GenesisIdleRPG\build\web\index.html` plus accompanying
JavaScript, WebAssembly and data files. Keep those files together.
Android output: `E:\Godot\GenesisIdleRPG\build\android\genesis.apk` (debug build).
For Play Store release use a properly configured Gradle/AAB build and private release
signing credentials; never put signing secrets into the repository.

## Serve the browser build

After a successful web export:

```powershell
python -m http.server 8060 --bind 127.0.0.1 --directory 'E:\Godot\GenesisIdleRPG\build\web'
```

Open localhost port 8060 in a browser. Do not double-click the HTML using file://.
For public hosting use HTTPS and serve .wasm as application/wasm. The preset has
thread support disabled, so it does not require the threaded export's COOP/COEP
headers. WebAssembly and WebGL 2.0 must be available in the browser.

Godot's web user:// storage uses browser IndexedDB. Browser restrictions, private
browsing, site-data clearing, or eviction can prevent or erase persistence. It is
not cloud storage and does not synchronize with Android. Do not promise a save on
abrupt tab closure; periodic/focus-loss saves reduce the risk but are not a backup.
Browser audio can require a user gesture: tap before expecting sound.
Audio cues are pre-generated WAV files, not unsupported runtime synthesis.

## Release checks still required

- Android device: portrait/landscape rotation, screen cutouts, display scaling,
  back navigation, touch dragging versus tapping, multitouch, app switching,
  audio/mute, battery/performance, offline save and resume.
- Browsers: Chromium and Firefox desktop; Android Chrome; Safari separately.
  Test first-gesture audio, resize, touch scrolling, storage permissions, reload,
  background-tab throttling, slow downloads, WebGL-disabled error handling.
- Accessibility: device font scaling, contrast, zoom and screen-reader strategy.
- The current layout uses pixel-sized controls, not a completed density-aware
  mobile design system. Safe-area/density validation on real devices remains work.

## Reference documentation

Consult Godot's official documentation sections **Exporting for the Web**,
**Exporting for Android**, and **Multiple resolutions** for the installed version.