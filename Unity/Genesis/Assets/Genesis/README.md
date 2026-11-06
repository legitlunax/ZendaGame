# Genesis Unity gameplay foundation

This folder contains Unity runtime data, native reusable assets, and C# systems
ported from the Godot Genesis project. Existing Unity scenes were not changed.

- `Resources/Genesis/Data/catalog.json`: full progression, story, forms, relics,
  miracles, fairy names, dimensions, wardrobe, emotions, and demo definitions.
- Other JSON files: demo states, numerical fixtures, hierarchy and procedural pose
  references, opening save template, export metadata.
- `Art`: native PNG particles, five WAV cues, and asset provenance notes.
- `Scripts/Core`: engine-independent catalog parsing, economy, game state, and
  JSON save serialization. Currency uses double; counts and seeds use Int64.
- `Scripts/Runtime/GenesisSession.cs`: optional MonoBehaviour lifecycle adapter.

## Use in a Unity scene

Create a new scene and attach `GenesisSession` to one GameObject. It loads the
catalog using Resources and exposes `Game` and `StateChanged` to future UI/VFX
presenters. Enable its demoMode field for a fresh nonpersistent session. Only use
one session instance per scene. No gameplay scene or automatic bootstrap is
installed, so existing tutorial scenes do not unexpectedly start the game.

The core exposes Tap, Buy, ClaimChapter, Equip, ActivateMiracle, BuyRelic, Bloom,
Restore, and SaveJson. Presentation must explicitly call these actions and refresh
from the state. Demo snapshots can be loaded with Restore and awardOffline=false.
The adapter does not yet provide a preset-selection UI, miracle/relic UI, audio
playback, input binding, background VFX, or character rendering.

The Unity adapter writes `genesis_unity_v1.json` to Application.persistentDataPath,
not the Godot save location. It never discovers/imports a real Godot save. It
immediately persists offline rewards on load, autosaves every 15 seconds, and
saves on pause/focus loss/quit. Malformed saves disable persistence to avoid
overwriting the original; inspect the Unity Console if this happens.

The core depends on Newtonsoft.Json, already present in this Unity project's
package cache via the MCP package. The project manifest now declares this runtime
dependency explicitly. No Godot runtime is required to execute the C# core.

## Art conversion boundaries

The original Godot game and its migration handoff are maintained in the
repository root and `UnityMigration/`. SVG/GLB files are deliberately not placed
in this Unity project's Assets without a configured importer. There are no
authored rigged characters or animation clips in the original to import. Pose
JSON is reference data, not Unity AnimationClips. Godot shaders and scene files
need rebuilding.