using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private readonly Dictionary<int, Texture2D> dimensionBackgrounds = new Dictionary<int, Texture2D>();
        private readonly HashSet<Texture2D> importedEnvironmentBackgrounds = new HashSet<Texture2D>();
        public int PendingBackgroundStyle { get; private set; }
        public int BackgroundStyleCount => Dimensions == null ? 0 : Dimensions.Count;

        public string BackgroundStyleName(int index)
        {
            if (index < 0 || index >= BackgroundStyleCount) return "Unknown";
            switch (Dimensions[index].Name)
            {
                case "Primordial Possibility": return "Primordial";
                case "Interwoven Elemental Dimension": return "Elements";
                case "Omniverse Confluence Dimension": return "Omniverse";
                case "Three Thousand Dao Dimensions": return "Dao";
                case "Divine Awareness Dimension": return "Awareness";
                case "Elves and Fairies Realm": return "Fae";
                case "Concord of Living Realms Dimension": return "Concord";
                case "Universal Heart Dimension": return "Heart";
                case "Embodied Soul Dimension": return "Embodied Soul";
                default: return Dimensions[index].Name.Replace(" Dimension", "").Replace(" Realm", "");
            }
        }

        public bool IsBackgroundStyleUnlocked(int index)
        {
            if (Game == null || index < 0 || index >= BackgroundStyleCount) return false;
            if (index == 0) return true;
            // Match the dimension's own story milestone, not an earlier creation
            // in its layer (for example Yin must not unlock the Duality theme).
            DimensionDefinition milestone = Game.Catalog.dimensions.FirstOrDefault(d => d.name == Dimensions[index].Name);
            return milestone != null ? IsChronicleUnlocked(milestone.chapter_id) :
                Dimensions[index].Chapters.Any(c => IsChronicleUnlocked(c.id));
        }

        public bool SelectBackgroundStyle(int index)
        {
            if (index < 0 || index >= BackgroundStyleCount) return false;
            PendingBackgroundStyle = index;
            return true;
        }

        public bool ConfirmBackgroundStyle()
        {
            if (!IsBackgroundStyleUnlocked(PendingBackgroundStyle)) return false;
            SetSceneStyle(PendingBackgroundStyle);
            backgroundPicker = false;
            return true;
        }

        private Color BackgroundPalette(int index)
        {
            switch (BackgroundStyleName(index))
            {
                case "Primordial": return new Color(0.82f, 0.57f, 0.72f);
                case "Duality": return new Color(0.53f, 0.54f, 0.72f);
                case "Wood": return new Color(0.38f, 0.64f, 0.43f);
                case "Fire": return new Color(0.88f, 0.43f, 0.30f);
                case "Earth": return new Color(0.65f, 0.49f, 0.32f);
                case "Metal": return new Color(0.55f, 0.65f, 0.71f);
                case "Water": return new Color(0.26f, 0.61f, 0.78f);
                case "Time": return new Color(0.73f, 0.58f, 0.28f);
                case "Space": return new Color(0.39f, 0.43f, 0.72f);
                case "Soul": return new Color(0.67f, 0.44f, 0.76f);
                case "Angel": return new Color(0.76f, 0.69f, 0.46f);
                case "Demon": return new Color(0.63f, 0.28f, 0.42f);
                case "Fae": return new Color(0.39f, 0.69f, 0.57f);
                case "Shadow": return new Color(0.39f, 0.36f, 0.56f);
                case "Heart": return new Color(0.84f, 0.43f, 0.57f);
                default: return Color.HSVToRGB(Mathf.Repeat(0.09f + index * 0.618034f, 1), 0.44f, 0.73f);
            }
        }

        private Texture2D DimensionBackground(int index)
        {
            if (index < 0 || index >= BackgroundStyleCount) return null;
            if (dimensionBackgrounds.TryGetValue(index, out Texture2D existing)) return existing;
            Texture2D environment = Resources.Load<Texture2D>(EnvironmentBackgroundPath(index));
            if (environment != null)
            {
                importedEnvironmentBackgrounds.Add(environment);
                dimensionBackgrounds.Add(index, environment);
                return environment;
            }
            // Small, generated-once UI textures: no external art, shaders or OS APIs.
            const int width = 192, height = 320;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = BackgroundStyleName(index) + " app theme", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Color hue = BackgroundPalette(index);
            Color ink = Color.Lerp(hue, new Color(0.12f, 0.13f, 0.22f), 0.26f);
            Color pearl = new Color(1, 0.94f, 0.78f);
            string name = BackgroundStyleName(index);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1), v = y / (float)(height - 1);
                    float edge = Mathf.Min(u, 1 - u);
                    Color pixel = Color.Lerp(Color.Lerp(hue, Color.white, 0.24f), Color.Lerp(hue, Color.white, 0.65f), v);
                    float wave = Mathf.Sin(v * Mathf.PI * (4 + index % 4) + index);
                    float ribbon = Mathf.Abs(edge - (0.065f + 0.028f * wave));
                    bool mark = false;
                    if (name == "Wood" || name == "Fae" || name == "Living Universe")
                        mark = edge < 0.18f && Mathf.Pow((edge - 0.09f) / 0.065f, 2) + Mathf.Pow((Mathf.Repeat(v * 9, 1) - 0.5f) / 0.24f, 2) < 1;
                    else if (name == "Water" || name == "Fire")
                        mark = edge < 0.18f && Mathf.Abs(Mathf.Sin(edge * 65 + v * (name == "Water" ? 32 : 58))) < 0.18f;
                    else if (name == "Earth" || name == "Metal")
                        mark = edge < 0.16f && Mathf.Abs(Mathf.Repeat(v * 12 + edge * 10, 1) - 0.5f) < 0.07f;
                    else if (name == "Duality")
                        mark = edge < 0.15f && Mathf.Sin(v * 30 + (u < 0.5f ? 0 : Mathf.PI)) > 0;
                    else
                    {
                        float dx = (edge - 0.1f) * 1.6f;
                        float dy = Mathf.Repeat(v * (5 + index % 5) + index * 0.13f, 1) - 0.5f;
                        mark = Mathf.Abs(dx) + Mathf.Abs(dy) < 0.037f;
                    }
                    if (ribbon < 0.008f) pixel = Color.Lerp(pixel, ink, 0.6f);
                    if (mark) pixel = Color.Lerp(ink, pearl, (index % 3) * 0.3f);
                    pixels[y * width + x] = pixel;
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            dimensionBackgrounds.Add(index, texture);
            return texture;
        }

        public static string EnvironmentBackgroundPath(int index)
        { return "Genesis/Art/backgrounds/environments/world_" + index.ToString("D2"); }

        private void DrawBackgroundSelector(Rect popup)
        {
            int index = PendingBackgroundStyle;
            bool unlocked = IsBackgroundStyleUnlocked(index);
            GUI.DrawTexture(new Rect(popup.x + 14, popup.y + 48, 76, 112), GoddessBackground(index), ScaleMode.StretchToFill);
            if (SoundButton(new Rect(popup.x + 102, popup.y + 48, 44, 44), "<", mobileButton))
                SelectBackgroundStyle((index + BackgroundStyleCount - 1) % BackgroundStyleCount);
            LocalizedLabel(new Rect(popup.x + 152, popup.y + 50, 116, 44), BackgroundStyleName(index), mobileText);
            if (SoundButton(new Rect(popup.x + 274, popup.y + 48, 44, 44), ">", mobileButton))
                SelectBackgroundStyle((index + 1) % BackgroundStyleCount);
            LocalizedLabel(new Rect(popup.x + 102, popup.y + 100, 216, 60), (index + 1) + " / " + BackgroundStyleCount + "   " +
                L(unlocked ? (SceneStyle == index ? "Applied" : "Unlocked") : "Locked / unlock this world's story"), mobileSmall);
            if (SoundButton(new Rect(popup.x + 14, popup.yMax - 58, 148, 44), "Cancel", mobileButton)) backgroundPicker = false;
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && unlocked;
            if (SoundButton(new Rect(popup.x + 170, popup.yMax - 58, 148, 44), "Confirm", mobileButton)) ConfirmBackgroundStyle();
            GUI.enabled = enabled;
        }
    }
}