# Enchanted conversation integration

The first production integration is the real chronicle conversation reader. The
main Goddess page, Idle RPG and story journal remain unchanged for review before
extending the theme. No production scene is replaced or saved by the installer.

## Entry points

- Unity menu: **Genesis > Presentation > Build Enchanted Conversation**.
- Validation: **Genesis > Presentation > Validate Enchanted Conversation**.
- Production prefab: `E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\UI\EnchantedConversation.prefab`.
- Presentation adapter: `E:\ZendaGame\Genesis\Assets\Genesis\Sanctum\GenesisSanctum.Enchanted.cs`.

The installer generates ten reusable kit prefabs and its separate demo scene,
then creates a production prefab without EnchantedDemoStory. The adapter forwards
navigation and reward requests to the existing controller. No new persistence or
progression system was introduced. Existing Almendra and Crimson Text fonts remain
in use. The production interface previously used IMGUI; unrelated TextMeshPro
learning-sample scripts/assets were not replaced or modified.

Panel/button frames use the kit's manifest borders and uGUI Sliced images with
PPU multipliers. Original world textures, live Zenda RenderTexture, and Lady icons
are reused. Safe-area, maximum-width and scrolling components come from the kit.
An InputSystemUIInputModule is created only when there is no active EventSystem;
an existing active EventSystem is reused, not replaced.

## Evidence and limitations

- 29 prefab/import checks passed in Unity 6000.6.7f1.
- 9 live Play-mode checks passed: bindings, previous/continue controller calls,
  first-page disabled state, single EventSystem, modern input and no demo driver.
- Captured actual Game View output at 390x700, 1080x1920, 1080x2400 and 1280x720.
- Screenshots and reports are under `E:\ZendaGame\Genesis\Builds` with `enchanted-` prefixes.
- Original editor progress was exported before smoke tests and imported afterward.
- Original open scenes reported clean after testing; no original scenes were saved.
- Existing URP shadow-array and audio API warnings were observed.
- Physical touch/keyboard navigation, long localized text, notches, pause/resume,
  reward/world-unlock branches and actual device/browser execution still need
  manual review. Programmatic onClick invocation is not a physical pointer test.
- A WebGL build was requested on the already-active target. Confirm a fresh
  timestamp in `E:\ZendaGame\Genesis\Builds\WebGL-build-result.txt` before claiming success.
- Windows/Android release builds and execution are not validated by this work.
  Android custom release signing was not configured at preflight.

Do not import the style reference or contact sheet as a full-screen UI. They remain
outside Assets. Keep font/code notices copied with the kit. Texture compression and
atlas tuning remain measurement-driven follow-up work, not unverified defaults.