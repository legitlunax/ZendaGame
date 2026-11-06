# Unity integration notes

## Package layout

Import `EnchantedUI.unitypackage` OR copy `Assets/EnchantedUI` into your project. Keep that exact folder name: the importer and builder intentionally use a fixed, scoped path. These are alternate routes to the same GUIDs, not two different kits. Python and image-generation tools are not required to use the kit.

The archive contains C# and assets, not pre-generated scene YAML. Running **Tools > Enchanted UI > 2 - Build Prefabs and Demo Scene** in your installed Unity version creates the actual `.prefab`, `.asset` and `.unity` files through Unity's own APIs. This avoids hand-authored scene serialization and lets the builder choose the project's available input module. It does not modify ProjectSettings, install packages or replace any scene.

Unity UI (`com.unity.ugui`) is required. The new Input System is optional. When ENABLE_INPUT_SYSTEM is defined, the editor builder detects InputSystemUIInputModule and assigns its default actions. Otherwise the demo uses StandaloneInputModule. An existing project's input configuration should be reused when integrating individual prefabs. Open the sample scene on its own before testing: an additive production scene may contain another camera or EventSystem.

## Sprite import contract

- Texture Type: Sprite (2D and UI), Single.
- Mesh Type: Full Rect. PNGs retain their original transparent margins and illustration detail.
- Pixels Per Unit: 100. Filter: Bilinear. Wrap: Clamp.
- Alpha Source: Input Texture Alpha; Alpha Is Transparency enabled.
- Mipmaps and Read/Write disabled. Non Power of 2: None.
- Maximum source import size: 4096. Initial compression: Uncompressed, to preserve golden details for review.
- No custom materials or render-pipeline-specific effects.

The source button is 2172 × 724 with transparent space above and below its illuminated shape. That space is intentional and kept in the preset; the full control remains a rectangular click/tap target. Avoid trimming or resizing the PNG without recalculating its borders.

| Asset | Border L, B, R, T (source pixels) | Image mode | Typical PPU multiplier |
| --- | --- | --- | --- |
| panel_enchanted | 320, 320, 320, 320 | Sliced | 4 for dialogue/portrait; 5–6 for panels; 10–12 for compact strips |
| button_amethyst | 540, 268, 540, 268 | Sliced | 8 |
| hex_portrait_socket | 0, 0, 0, 0 | Simple, Preserve Aspect | 1 |
| ornament_blossom | 0, 0, 0, 0 | Simple, Preserve Aspect | 1 |
| progress_track / progress_fill | 24, 24, 24, 24 | Sliced | 1 |
| divider_gold | 0, 0, 0, 0 | Simple | 1 |
| six icons | 0, 0, 0, 0 | Simple, Preserve Aspect | 1 |

Corners keep their geometry with slicing. Interior textures are stretched, so very wide headers naturally have a different texture distribution from portrait cards. See the actual preview rather than expecting pixel-identical illustration at every size. For a progress bar, place fill inside track and size its RectTransform using progress; do not combine Image.Type.Filled with an assumption that 9-slicing still applies.

## Components and wiring

`ConversationScreen.prefab` contains an overlay Canvas, scaler (1080 × 1920, match 0.5), gradient background, disabled world Image, direct-child safe-area container, maximum-width column and page ScrollRect. A nested ScrollRect handles long dialogue. Short desktop windows use page scrolling rather than squeezing the whole mobile screen into unreadable text.

`EnchantedDialogueView` exposes component references and forwards button events. It does not save data, choose story branches or award resources. Call:

```csharp
view.SetPortraits(zendaSprite, "Zenda / speaking", riyaraSprite, "Lady Riyara / listening");
view.SetBackground(worldSprite);
view.SetLine("Zenda / happy / 1 of 11", localizedDialogue, false, true);
```

`SetBackground` maintains aspect ratio and center-crops to fill. Change the background child placement/fitter if the illustration needs a different focal point. Set title/resource/chapter via the public Text references or adapt them to your binding/localization system. Connect `continueRequested`, `previousRequested` and `backRequested` using Inspector UnityEvents or code. Remove code listeners you add when your controller is disabled.

The scene-only `EnchantedDemoStory` has four demonstration lines and is not attached to `ConversationScreen.prefab`. Standalone portrait prefab includes a placeholder label; remove or hide it after assigning your character artwork. The complete screen already hides those placeholder labels. Hex tabs have no game behavior assigned: wire their Button.onClick events to your own navigation and add your own selection state. The socket is a decorative background, not a portrait clipping mask; use a separate mask if clipping is needed.

The sample uses UI.Text with bundled fonts to keep TMP optional. If you already use TMP, replace the text creation and fields with TMP_Text/TextMeshProUGUI and preserve your font assets. DejaVu covers Spanish/English; use appropriate fonts/fallbacks for additional scripts. Disable or adapt best-fit if your accessibility/localization policy requires fixed minimum text sizes.

The generated theme is a build snapshot, not a live global skin manager. Defaults are centralized in EnchantedTheme.cs. Regenerate after changing defaults, or edit prefab components in Unity. Existing generated prefabs are not updated automatically when a theme asset changes.

## Performance and shipping

The four illustrated source textures use approximately 24 MiB of uncompressed RGBA texture data before runtime overhead; font atlases and utility sprites add more. PNG download size is not GPU memory usage. This is a quality-first source kit. Profile before shipping, especially on mobile WebGL.

Tune platform import overrides for your device/browser targets. Test ASTC or ETC2 RGBA only on supported targets; keep a suitable fallback. Check gold edge artifacts at the final UI scale. A sprite atlas is optional and is not created automatically. If you add one, disable rotation and tight packing for predictable UI slicing, use padding, and measure memory: packing large sources into a 4096 atlas may increase allocation. Do not enable Read/Write for runtime unless you actually need it.

Decorative graphics have raycastTarget disabled. Buttons have full rectangular hit targets and ColorTint states. Press feedback scales only the art child, never the LayoutGroup-controlled root. No hover action is required for gameplay. Keyboard navigation uses Unity's Selectable system; choose the first selected control and navigation ordering in your game's controller as needed.

## Manual checks in your Unity project

- Resolve compile errors first; confirm uGUI is installed. This kit's source syntax was parsed, but Unity compilation was not run in the authoring environment.
- Generate prefabs and inspect Console for missing sprites, fonts or references.
- Open the sample scene alone; verify all four sample lines, Previous/Continue and disabled states.
- Assign real portrait sprites, background, story data and button callbacks.
- Test 1080 × 1920, 1080 × 2400, 1280 × 720 and resizable desktop/browser windows.
- Test notches with Unity Device Simulator or actual Android hardware.
- Test Spanish accents, longer names and very long dialogue; scroll dialogue and page independently.
- Confirm a single active EventSystem and the correct input module in production scenes.
- Make and run Windows, Android and WebGL builds; profile memory and rendering on representative devices.

## Reference documentation

- [Unity: 9-slicing](https://learn.unity.com/tutorial/using-9-slicing-for-scalable-sprites)
- [Unity UI Image API](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.Image.html)
- [Unity Input System UI module](https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/using-ui-input-module.md)
- [Sprite atlas packing settings](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/U2D.SpriteAtlasPackingSettings.html)
