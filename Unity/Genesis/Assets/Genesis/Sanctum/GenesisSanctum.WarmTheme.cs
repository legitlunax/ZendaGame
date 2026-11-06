using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private static readonly Color WarmInk = new Color32(255, 244, 225, 255);
        private static readonly Color WarmMuted = new Color32(214, 195, 222, 255);
        private Texture2D warmButtonNormal, warmButtonHover, warmButtonPressed, warmButtonDisabled;
        private Texture2D warmReadingSurface;
        private Font warmHeadingFont;
        private Font rpgBodyFont;
        private bool warmThemeLoaded;

        private void EnsureWarmTheme()
        {
            if (warmThemeLoaded) return;
            warmThemeLoaded = true;
            EnsureEnchantedGameTheme();
            // Bundled OFL fonts work offline on Android/WebGL as well as desktop.
            warmHeadingFont = Resources.Load<Font>("Genesis/Fonts/Almendra-Bold");
            rpgBodyFont = Resources.Load<Font>("Genesis/Fonts/CrimsonText-Regular");
            warmButtonNormal = WarmSurface("Rose pearl button", new Color(0.88f, 0.73f, 0.69f), new Color(0.66f, 0.43f, 0.45f));
            warmButtonHover = WarmSurface("Rose pearl hover", new Color(0.94f, 0.81f, 0.72f), new Color(0.66f, 0.42f, 0.36f));
            warmButtonPressed = WarmSurface("Rose pearl pressed", new Color(0.88f, 0.71f, 0.72f), new Color(0.57f, 0.34f, 0.39f));
            warmButtonDisabled = WarmSurface("Rose pearl unavailable", new Color(0.78f, 0.72f, 0.70f), new Color(0.62f, 0.55f, 0.55f));
            warmReadingSurface = MellowGradient("Mellow parchment reading surface",
                new Color(0.91f, 0.79f, 0.73f), new Color(0.83f, 0.70f, 0.72f));
        }

        private Texture2D MellowGradient(string textureName, Color top, Color bottom)
        {
            Texture2D texture = new Texture2D(1, 64, TextureFormat.RGBA32, false)
            { name = textureName, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < texture.height; y++)
                texture.SetPixel(0, y, Color.Lerp(bottom, top, y / (float)(texture.height - 1)));
            texture.Apply(false, true);
            return texture;
        }

        private Texture2D WarmSurface(string textureName, Color fill, Color edge, int size = 64)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = textureName, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float center = (size - 1) * 0.5f;
                    Vector2 q = new Vector2(Mathf.Abs(x - center) - (center - 13.5f), Mathf.Abs(y - center) - (center - 13.5f));
                    float distance = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - 12;
                    Color pixel = Color.Lerp(fill * new Color(0.97f, 0.94f, 0.94f, 1), Color.Lerp(fill, Color.white, 0.16f), y / (float)(size - 1));
                    pixel = Color.Lerp(pixel, edge, Mathf.Clamp01(distance + 2));
                    pixel.a = fill.a * Mathf.Clamp01(-distance);
                    texture.SetPixel(x, y, pixel);
                }
            texture.Apply(false, true);
            return texture;
        }

        private void ApplyWarmButton(GUIStyle style)
        {
            style.border = new RectOffset(18, 18, 18, 18);
            style.padding = new RectOffset(10, 10, 5, 5);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            if (warmHeadingFont != null) style.font = warmHeadingFont;
            style.normal.background = style.onNormal.background = warmButtonNormal;
            style.hover.background = style.onHover.background = warmButtonHover;
            style.active.background = style.onActive.background = warmButtonPressed;
            style.focused.background = style.onFocused.background = warmButtonHover;
            if (gameTheme != null)
            {
                if (enchantedTransparent == null) enchantedTransparent = Solid(Color.clear);
                foreach (GUIStyleState state in new[] { style.normal, style.hover, style.active, style.focused,
                    style.onNormal, style.onHover, style.onActive, style.onFocused }) state.background = enchantedTransparent;
            }
            foreach (GUIStyleState state in new[] { style.normal, style.hover, style.active, style.focused,
                style.onNormal, style.onHover, style.onActive, style.onFocused }) state.textColor = WarmInk;
        }

        public Texture2D GoddessBackground(int index)
        {
            return DimensionBackground(index);
        }

        public Texture2D AppBackground => GoddessBackground(SceneStyle);

        // Fully opaque text surfaces: artwork and scene brightness must not affect legibility.
        private void DrawReadingSurface(Rect rect)
        {
            EnsureWarmTheme();
            Color previous = GUI.color;
            GUI.color = Color.white;
            DrawEnchantedPanel(rect);
            GUI.color = previous;
        }

        private void DrawWarmBackground(Rect rect)
        {
            if (SelectedTab == 1)
            {
                DrawEnchantedPanel(rect);
                return;
            }
            Texture2D background = AppBackground;
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, background != null ? background : Texture2D.whiteTexture, ScaleMode.ScaleAndCrop);
            GUI.color = previous;
        }

        private void ReleaseWarmTheme()
        {
            foreach (Texture2D texture in dimensionBackgrounds.Values)
                if (texture != null && !importedEnvironmentBackgrounds.Contains(texture)) Destroy(texture);
            dimensionBackgrounds.Clear();
            importedEnvironmentBackgrounds.Clear();
            foreach (Texture2D texture in new[] { warmButtonNormal, warmButtonHover, warmButtonPressed, warmButtonDisabled, warmReadingSurface, enchantedTransparent })
                if (texture != null) Destroy(texture);
            // Fonts are shared Resources assets, not generated runtime objects.
        }
    }
}