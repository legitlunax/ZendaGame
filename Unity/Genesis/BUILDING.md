# Genesis platform builds

The supported build targets are Windows x64, Android ARM64 APK and WebGL 2.
Build setup is implemented in `Assets/Genesis/Editor/GenesisBuild.cs`.
Only the saved `Assets/Genesis/Scenes/ZendaSanctum.unity` scene is included;
the older demo and character-preview scenes are excluded.

## Unity editor

Use Unity **6000.6.7f1**. Install the required modules in Unity Hub:

- Windows Build Support (Mono).
- Android Build Support **including SDK & NDK Tools and OpenJDK**.
- Web Build Support.

Stop Play mode. Run **Genesis > Build > Configure Windows, Android and WebGL**.
In Build Profiles select/switch to the desired platform, then run the corresponding
command under **Genesis > Build**. Commands deliberately refuse to switch platforms
automatically or save scenes. Preserve unsaved preview work yourself before a
platform switch. A dirty Sanctum scene blocks building rather than silently shipping
an outdated saved scene.

## Command line

Close the project in Unity first, preserving unsaved work yourself. In PowerShell:

```powershell
& '.\Tools\Build-Genesis.ps1' -Platform Windows
& '.\Tools\Build-Genesis.ps1' -Platform Android
& '.\Tools\Build-Genesis.ps1' -Platform WebGL
```

The wrapper uses `-buildTarget` before invoking `GenesisBuild.Windows`,
`GenesisBuild.Android` or `GenesisBuild.WebGL`. An optional `-Unity` argument selects
another installation of the same editor version. Missing modules or failed builds
are errors, not reported as successful exports.

| Target | Output |
| --- | --- |
| Windows | `Builds/Windows/ZendaGenesis.exe` |
| Android | `Builds/Android/ZendaGenesis.apk` |
| WebGL | `Builds/WebGL/index.html` |

Distribute the **whole Windows output folder**, not just the EXE.
Desktop, Start-menu and taskbar shortcuts point **directly at
`ZendaGenesis.exe`**. No launcher script, batch file or wrapper of any kind is
needed or wanted: the game opens from a plain shortcut.

### Why the player must stay on the classic input manager

The Windows player runs with **Active Input Handling = Input Manager (Old)**
(`activeInputHandler: 0` in `ProjectSettings.asset`). Do not switch it back to the
new Input System without repeating the launch test below.

When a program is started through a shortcut, Explorer loads `linkinfo.dll` and
`Windows.FileExplorer.Common.dll` into it. The Unity player's native input
start-up asks Windows for the `Windows.Gaming.Input` / game bar classes, that
request is served by the injected shell library, and the shell library never
finishes it. The player then waits forever, before any managed code runs, so the
game never opens and no managed watchdog can rescue it. The classic input manager
has no such native activation, and the player starts normally from every
shortcut.

The game itself never needed the new Input System: the whole UI is IMGUI, and
the only place that used it was the uGUI conversation view, which now uses
`StandaloneInputModule`. `WindowsGamepadBackendHint` is kept on **XInput** as a
second line of defence.

After changing the input stack, repeat desktop-shortcut and game-input checks on
the target Windows system before release.

Android defaults
to ARM64, minimum API 26, OpenGL ES 3 and IL2CPP. APK export is explicit (not AAB or
Gradle-project export). The application ID is `com.zendagame.genesis`. Configure a
private release keystore in Player Settings for signed releases; no passwords or
private keys belong in source control.

WebGL uses IL2CPP, single-threaded execution, growable 128–1024 MB memory and gzip
with decompression fallback. Serve the **whole WebGL folder over HTTP(S)**, not
`file://`. A local smoke test can use:

```powershell
py -m http.server 8000 --directory 'Builds\WebGL'
```

Open localhost port 8000 in a WebGL-2-capable desktop browser. Browser memory,
GPU support and hosting policies still require testing. This is not a promise of
support for every mobile browser; Android APK is the native phone target.

## Microsoft Store package (MSIX)

The MSIX packaging pipeline is maintained separately and is not included in
this repository. Package outputs and development signing certificates must be
kept out of source control; use a private signing certificate for local release
packaging.

## Acceptance checklist

- Start from a fresh install/load and verify all available resources. Character
  rendering requires a model with redistribution rights; the restricted local
  model is intentionally not included.
- Check full-body/close-up reactions, return to idle and drag rotation.
- Test Gather/Journey/Worlds, guide/tab navigation, scrolling without accidental
  purchases, journey rewards and locked-world departure gating.
- Resize Windows/browser windows; rotate Android and test notches/safe areas.
- Verify mouse and physical-device touch, background/resume, browser focus loss,
  low-memory behavior and sustained phone frame rate.
- Review each build's timestamped result and runtime logs. Compilation or settings
  checks alone are not proof of a successful player build or device compatibility.

The Sanctum currently keeps progression **in memory for the current session**.
This build setup does not add persistence, offline rewards or browser save syncing.
The separate older `GenesisSession` file-save component is not added to Sanctum.
Known portrait/mobile regressions in PLAYTEST.md remain separate acceptance work.
