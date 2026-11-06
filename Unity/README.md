# Unity project

Open `Genesis` in Unity Hub using Unity **6000.6.7f1**. Platform build
instructions are in [`Genesis/BUILDING.md`](Genesis/BUILDING.md).

The Unity package manifest uses two checked-in local dependencies:

- `Tools/UniVRM/Packages/UniGLTF` and `Tools/UniVRM/Packages/VRM10`
- `Tools/unity-mcp/MCPForUnity`

Unity caches, generated projects, logs, and build outputs are intentionally
ignored. The editor-only character preview scripts are included, but the VRM
model and preview scene are not: the model's metadata prohibits redistribution
and modification. The same restricted model is also used by the Darkness
protagonist path; that option reports an unavailable-model message until a
redistributable replacement is supplied. Optional UniVRM sample models are
excluded as well. Installer packages, signing certificates, local editor
configuration, and generated builds are also excluded.

The Unity gameplay foundation is documented in
[`Genesis/Assets/Genesis/README.md`](Genesis/Assets/Genesis/README.md). The
existing Godot game remains at the repository root; the Unity project is a
separate implementation.
