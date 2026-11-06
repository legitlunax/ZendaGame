using EnchantedUI;
using UnityEngine;

namespace Genesis
{
    /// <summary>
    /// Shared IMGUI renderer for the Enchanted UI kit artwork.
    ///
    /// The kit ships sprites, not a renderer, so every nine-slice frame is
    /// drawn by hand. Both the Sanctum screens and the boot loading screen use
    /// this class, which guarantees that a panel, a progress bar or an ornament
    /// looks exactly the same wherever it appears in the game.
    /// </summary>
    public static class GenesisEnchantedArt
    {
        private const string ThemeResourcePath = "Genesis/UI/EnchantedGameTheme";
        private static EnchantedTheme theme;
        private static Texture2D gradientTexture;
        private static Color gradientTop, gradientBottom;

        /// <summary>The kit theme that ships with the game, or null when it is missing.</summary>
        public static EnchantedTheme Theme
        {
            get
            {
                if (theme == null) theme = Resources.Load<EnchantedTheme>(ThemeResourcePath);
                return theme;
            }
        }

        /// <summary>Gold used for accents when the theme cannot be read.</summary>
        public static Color Gold
        {
            get
            {
                EnchantedTheme current = Theme;
                return current != null ? current.gold : new Color32(240, 204, 145, 255);
            }
        }

        /// <summary>Cream body text colour, with a neutral fallback.</summary>
        public static Color Text
        {
            get
            {
                EnchantedTheme current = Theme;
                return current != null ? current.text : new Color32(255, 244, 225, 255);
            }
        }

        /// <summary>Muted lavender text colour, with a neutral fallback.</summary>
        public static Color MutedText
        {
            get
            {
                EnchantedTheme current = Theme;
                return current != null ? current.mutedText : new Color32(214, 195, 222, 255);
            }
        }

        /// <summary>Deep amethyst colour at the top of the screen.</summary>
        public static Color BackgroundTop
        {
            get
            {
                EnchantedTheme current = Theme;
                return current != null ? current.backgroundTop : new Color32(57, 35, 81, 255);
            }
        }

        /// <summary>Near black purple colour at the bottom of the screen.</summary>
        public static Color BackgroundBottom
        {
            get
            {
                EnchantedTheme current = Theme;
                return current != null ? current.backgroundBottom : new Color32(17, 12, 30, 255);
            }
        }

        /// <summary>The four point star of the kit.</summary>
        public static Sprite Spark
        {
            get { EnchantedTheme current = Theme; return current != null ? current.spark : null; }
        }

        /// <summary>The small diamond of the kit.</summary>
        public static Sprite Diamond
        {
            get { EnchantedTheme current = Theme; return current != null ? current.diamond : null; }
        }

        /// <summary>The optional floral branch of the kit.</summary>
        public static Sprite Blossom
        {
            get { EnchantedTheme current = Theme; return current != null ? current.blossom : null; }
        }

        /// <summary>The hexagonal character socket of the kit.</summary>
        public static Sprite HexSocket
        {
            get { EnchantedTheme current = Theme; return current != null ? current.hexSocket : null; }
        }

        /// <summary>Display font of the kit.</summary>
        public static Font TitleFont
        {
            get { EnchantedTheme current = Theme; return current != null ? current.titleFont : null; }
        }

        /// <summary>Body font of the kit.</summary>
        public static Font BodyFont
        {
            get { EnchantedTheme current = Theme; return current != null ? current.bodyFont : null; }
        }

        /// <summary>IMGUI equivalent of Image.Type.Sliced. Source border pixels stay intact.</summary>
        public static void Slice(Rect rect, Sprite sprite, float sliceScale, Color tint)
        {
            if (sprite == null || Event.current == null || Event.current.type != EventType.Repaint) return;
            if (rect.width <= 0 || rect.height <= 0) return;
            Rect source = sprite.rect;
            Vector4 border = sprite.border;
            float left = border.x / sliceScale, right = border.z / sliceScale;
            float top = border.w / sliceScale, bottom = border.y / sliceScale;
            float fit = Mathf.Min(1, Mathf.Min(rect.width / Mathf.Max(1, left + right), rect.height / Mathf.Max(1, top + bottom)));
            float[] dx = { rect.x, rect.x + left * fit, rect.xMax - right * fit, rect.xMax };
            float[] dy = { rect.y, rect.y + top * fit, rect.yMax - bottom * fit, rect.yMax };
            float[] sx = { source.x, source.x + border.x, source.xMax - border.z, source.xMax };
            float[] sy = { source.yMax, source.yMax - border.w, source.y + border.y, source.y };

            // The nine tiles have to cover the screen pixel grid exactly. Their
            // borders normally land on fractional pixels, and the uncovered
            // sliver between the stretched middle and the fixed border shows
            // the background as a stray hairline down the right of every panel.
            // Snapping the tile borders to whole screen pixels closes that gap.
            Matrix4x4 matrix = GUI.matrix;
            bool snap = Mathf.Abs(matrix.determinant) > 1e-8f;
            Matrix4x4 inverse = snap ? matrix.inverse : Matrix4x4.identity;
            int[] column = new int[4];
            int[] row = new int[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 point = matrix.MultiplyPoint3x4(new Vector3(dx[i], dy[i], 0));
                column[i] = snap ? Mathf.RoundToInt(point.x) : Mathf.RoundToInt(dx[i]);
                row[i] = snap ? Mathf.RoundToInt(point.y) : Mathf.RoundToInt(dy[i]);
            }
            column[1] = Mathf.Clamp(column[1], column[0], column[2]);
            column[2] = Mathf.Clamp(column[2], column[1], column[3]);
            row[1] = Mathf.Clamp(row[1], row[0], row[2]);
            row[2] = Mathf.Clamp(row[2], row[1], row[3]);

            Color previous = GUI.color;
            GUI.color *= tint;
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                {
                    if (column[x + 1] <= column[x] || row[y + 1] <= row[y]) continue;
                    Vector3 from = snap ? inverse.MultiplyPoint3x4(new Vector3(column[x], row[y], 0)) : new Vector3(column[x], row[y], 0);
                    Vector3 to = snap ? inverse.MultiplyPoint3x4(new Vector3(column[x + 1], row[y + 1], 0)) : new Vector3(column[x + 1], row[y + 1], 0);
                    Rect target = new Rect(from.x, from.y, to.x - from.x, to.y - from.y);
                    Rect uv = new Rect(sx[x] / sprite.texture.width, sy[y + 1] / sprite.texture.height,
                        (sx[x + 1] - sx[x]) / sprite.texture.width, (sy[y] - sy[y + 1]) / sprite.texture.height);
                    GUI.DrawTextureWithTexCoords(target, sprite.texture, uv);
                }
            GUI.color = previous;
        }

        /// <summary>Draws a kit sprite stretched into the rect without changing its proportions.</summary>
        public static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            Rect source = sprite.rect;
            GUI.DrawTextureWithTexCoords(rect, sprite.texture,
                new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height,
                    source.width / sprite.texture.width, source.height / sprite.texture.height));
        }

        /// <summary>Draws a kit icon fitted into the rect, keeping its aspect ratio.</summary>
        public static void Icon(Rect rect, Sprite sprite)
        {
            if (sprite == null) return;
            Rect source = sprite.rect;
            float scale = Mathf.Min(rect.width / source.width, rect.height / source.height);
            DrawSprite(new Rect(rect.center.x - source.width * scale * 0.5f,
                rect.center.y - source.height * scale * 0.5f, source.width * scale, source.height * scale), sprite);
        }

        /// <summary>The optional floral ornament, faded so it never competes with the text.</summary>
        public static void Ornament(Rect rect, Sprite sprite, float alpha, float angle = 0f, bool flipHorizontal = false)
        {
            if (sprite == null || alpha <= 0.001f) return;
            Color previous = GUI.color;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.color = new Color(previous.r, previous.g, previous.b, previous.a * alpha);
            if (angle != 0f || flipHorizontal)
            {
                Vector3 scale = new Vector3(flipHorizontal ? -1f : 1f, 1f, 1f);
                // Rotate and mirror about the centre of the rect, not the origin.
                GUI.matrix = Matrix4x4.TRS(rect.center, Quaternion.Euler(0f, 0f, angle), scale)
                    * Matrix4x4.Translate(-rect.center) * previousMatrix;
            }
            Icon(rect, sprite);
            GUI.matrix = previousMatrix;
            GUI.color = previous;
        }

        /// <summary>The gold rule with its central diamond. Never stretched out of shape.</summary>
        public static void Divider(Rect rect)
        {
            EnchantedTheme current = Theme;
            if (current == null || current.divider == null) return;
            Rect source = current.divider.rect;
            float aspect = source.width / Mathf.Max(1f, source.height);
            float width = rect.width;
            float height = width / aspect;
            if (height > rect.height) { height = rect.height; width = height * aspect; }
            DrawSprite(new Rect(rect.center.x - width * 0.5f, rect.center.y - height * 0.5f, width, height), current.divider);
        }

        /// <summary>The kit panel: an amethyst frame with illustrated gold corners.</summary>
        public static void Panel(Rect rect)
        {
            EnchantedTheme current = Theme;
            if (current == null || current.panel == null) return;
            Slice(rect, current.panel, current.panelSliceScale * 3, Color.white);
        }

        /// <summary>The kit progress bar: a dark track filled with the luminous amethyst pill.</summary>
        public static void Progress(Rect rect, float progress)
        {
            EnchantedTheme current = Theme;
            if (current == null) return;
            Slice(rect, current.progressTrack, 8, Color.white);
            float width = rect.width * Mathf.Clamp01(progress);
            if (width <= 0) return;
            // Slice the fill instead of clipping it, so the lit end keeps its cap.
            Slice(new Rect(rect.x, rect.y, width, rect.height), current.progressFill, 8, Color.white);
        }

        /// <summary>The hexagonal character socket, gold when it is the active choice.</summary>
        public static void Socket(Rect rect, bool selected)
        {
            EnchantedTheme current = Theme;
            if (current == null || current.hexSocket == null) return;
            Color previous = GUI.color;
            GUI.color *= selected ? current.gold : current.secondaryTint;
            DrawSprite(rect, current.hexSocket);
            GUI.color = previous;
        }

        /// <summary>The amethyst button with its gold corners and its hover states.</summary>
        public static void Button(Rect rect, bool compact = false)
        {
            EnchantedTheme current = Theme;
            if (current == null || current.button == null) return;
            bool hover = Event.current != null && rect.Contains(Event.current.mousePosition);
            Color tint = !GUI.enabled ? (compact ? new Color(0.72f, 0.62f, 0.86f, 1f) : current.disabledTint)
                : hover && GUIUtility.hotControl != 0 ? current.secondaryTint
                : hover ? new Color(1.12f, 1.08f, 1.14f) : Color.white;
            float sliceScale = compact ? 32 : current.buttonSliceScale * 3;
            bool previous = GUI.enabled;
            try
            {
                if (compact) GUI.enabled = true;
                Slice(rect, current.button, sliceScale, tint);
            }
            finally { GUI.enabled = previous; }
        }

        /// <summary>Vertical gradient fill, the soft amethyst backdrop of the kit.</summary>
        public static void Gradient(Rect rect, Color top, Color bottom)
        {
            if (gradientTexture == null || gradientTop != top || gradientBottom != bottom)
            {
                gradientTexture = new Texture2D(1, 64, TextureFormat.RGBA32, false)
                {
                    name = "Genesis enchanted gradient", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
                };
                for (int y = 0; y < gradientTexture.height; y++)
                    gradientTexture.SetPixel(0, y, Color.Lerp(bottom, top, y / (float)(gradientTexture.height - 1)));
                gradientTexture.Apply(false, true);
                gradientTop = top;
                gradientBottom = bottom;
            }
            GUI.DrawTexture(rect, gradientTexture);
        }
    }
}
