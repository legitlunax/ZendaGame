using EnchantedUI;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private EnchantedTheme gameTheme;
        private bool gameThemeLoaded;
        private Texture2D enchantedTransparent;

        // The kit artwork is rendered by GenesisEnchantedArt so the Sanctum
        // screens and the boot loading screen are guaranteed to match.
        private void DrawKitIcon(Rect rect, Sprite sprite)
        {
            GenesisEnchantedArt.Icon(rect, sprite);
        }

        private void DrawKitProgress(Rect rect, float progress)
        {
            GenesisEnchantedArt.Progress(rect, progress);
        }

        private void EnsureEnchantedGameTheme()
        {
            if (gameThemeLoaded) return;
            gameThemeLoaded = true;
            gameTheme = GenesisEnchantedArt.Theme;
        }

        private void DrawEnchantedSlice(Rect rect, Sprite sprite, float sliceScale, Color tint)
        {
            GenesisEnchantedArt.Slice(rect, sprite, sliceScale, tint);
        }

        private void DrawEnchantedPanel(Rect rect)
        {
            GenesisEnchantedArt.Panel(rect);
        }

        private void ClearButtonBackgrounds(GUIStyle style)
        {
            // A null background can fall back to Unity skin in player IMGUI.
            // Explicit transparency is input/text plumbing, never a visible surface.
            if (enchantedTransparent == null) enchantedTransparent = Solid(Color.clear);
            foreach (GUIStyleState state in new[] { style.normal, style.hover, style.active, style.focused,
                style.onNormal, style.onHover, style.onActive, style.onFocused })
            {
                state.background = enchantedTransparent;
#if UNITY_EDITOR
                // High-DPI skin backgrounds are only exposed in the editor API.
                state.scaledBackgrounds = new Texture2D[0];
#endif
            }
        }

        private void DrawEnchantedButton(Rect rect, bool compact = false)
        {
            GenesisEnchantedArt.Button(rect, compact);
        }

        private void DrawEnchantedSocket(Rect rect, bool selected)
        {
            GenesisEnchantedArt.Socket(rect, selected);
        }
    }
}
