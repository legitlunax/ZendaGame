using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private GenesisLocalization localization;
        private bool storyTranslationsLoaded;
        private Font multilingualFont, chineseFont;
        public string Language => Localization.Language;
        private GenesisLocalization Localization
        {
            get
            {
                if (localization == null)
                {
                    TextAsset data = Resources.Load<TextAsset>("Genesis/Data/localization");
                    localization = new GenesisLocalization(data == null ? null :
                        JsonConvert.DeserializeObject<Dictionary<string, string[]>>(data.text));
                }
                // English uses source prose. Defer story JSON parsing until needed,
                // including when a saved non-English language is restored directly.
                if (!storyTranslationsLoaded && localization.Language != "en")
                {
                    TextAsset stories = Resources.Load<TextAsset>("Genesis/Data/story-localization");
                    if (stories != null)
                        localization.AddTranslations(JsonConvert.DeserializeObject<Dictionary<string, string[]>>(stories.text));
                    storyTranslationsLoaded = true;
                }
                return localization;
            }
        }

        public bool SelectLanguage(string language)
        {
            if (!Localization.Select(language)) return false;
            if (ReadingChapter != null)
            {
                storyPages = GenesisStorytelling.Build(Game.Catalog, ReadingChapter, L);
                ChroniclePage = 0;
                PresentStoryPage();
            }
            enchantedPageKey = null;
            SavePlayerProgress();
            return true;
        }

        private string L(string source) => Localization.Text(source);
        private string LF(string source, params object[] values) => Localization.Format(source, values);
        private Font LanguageFont
        {
            get
            {
                if (Language == "zh")
                    return chineseFont != null ? chineseFont : (chineseFont = Resources.Load<Font>("Genesis/Fonts/NotoSansCJKsc-Regular"));
                return multilingualFont != null ? multilingualFont : (multilingualFont = Resources.Load<Font>("Genesis/Fonts/NotoSans-Regular"));
            }
        }

        private GUIStyle LocalizedStyle(GUIStyle style)
        {
            return Language == "en" || style == null || (chineseFont != null && style.font == chineseFont)
                ? style : new GUIStyle(style) { font = LanguageFont };
        }

        private void LocalizedLabel(Rect rect, string text, GUIStyle style)
        {
            var content = new GUIContent(L(text));
            GUI.Label(rect, content, FittedLocalizedStyle(rect, content, style));
        }

        private void LocalizedLabel(Rect rect, GUIContent text, GUIStyle style)
        {
            var content = new GUIContent(L(text.text), text.image, L(text.tooltip));
            GUI.Label(rect, content, FittedLocalizedStyle(rect, content, style));
        }

        // Compact UI can wrap and reduce its type size; scrolling prose already
        // receives its measured full height and therefore keeps its reading size.
        private GUIStyle FittedLocalizedStyle(Rect rect, GUIContent content, GUIStyle source)
        {
            var style = new GUIStyle(LocalizedStyle(source))
            {
                fixedWidth = 0, fixedHeight = 0, clipping = TextClipping.Clip
            };
            if (string.IsNullOrEmpty(content.text)) return style;
            int size = style.fontSize > 0 ? style.fontSize : (style.font != null ? style.font.fontSize : 15);
            style.fontSize = Mathf.Max(10, size);
            while (style.fontSize > 10 && !LocalizedTextFits(rect, content, style)) style.fontSize--;
            // Single-line metadata may need a second line even at the minimum size.
            if (!LocalizedTextFits(rect, content, style) && !style.wordWrap)
            {
                style.wordWrap = true;
                style.fontSize = Mathf.Max(10, size);
                while (style.fontSize > 10 && !LocalizedTextFits(rect, content, style)) style.fontSize--;
            }
            return style;
        }

        private static bool LocalizedTextFits(Rect rect, GUIContent content, GUIStyle style)
        {
            if (style.CalcHeight(content, Mathf.Max(1, rect.width)) > rect.height) return false;
            if (!style.wordWrap && style.CalcSize(content).x > rect.width) return false;
            return true;
        }

        private void DrawLanguageSelector(float y)
        {
            LocalizedLabel(new Rect(24, y, 342, 24), "Language", mobileSmall);
            for (int i = 0; i < GenesisLocalization.Languages.Length; i++)
            {
                string language = GenesisLocalization.Languages[i];
                Rect rect = new Rect(24 + i * 87, y + 27, 81, 40);
                // Always show native names with the bundled font, even in English.
                var style = new GUIStyle(mobileButton) { font = language == "zh"
                    ? (chineseFont ?? (chineseFont = Resources.Load<Font>("Genesis/Fonts/NotoSansCJKsc-Regular")))
                    : (multilingualFont ?? (multilingualFont = Resources.Load<Font>("Genesis/Fonts/NotoSans-Regular"))), fontSize = 12 };
                DrawEnchantedButton(rect);
                if (SoundButton(rect, GenesisLocalization.NativeNames[i], style)) SelectLanguage(language);
                if (Language == language) GatheringFill(new Rect(rect.x + 8, rect.yMax - 3, rect.width - 16, 2), 1, GatheringSage);
            }
        }
    }
}