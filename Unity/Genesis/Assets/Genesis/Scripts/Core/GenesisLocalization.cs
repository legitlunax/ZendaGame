using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Genesis
{
    // English source strings are stable translation keys. Unknown content falls back
    // unchanged; localization never modifies catalog IDs, player names or progression.
    public sealed class GenesisLocalization
    {
        public static readonly string[] Languages = { "en", "es", "ru", "zh" };
        public static readonly string[] NativeNames = { "English", "Español", "Русский", "简体中文" };
        private readonly Dictionary<string, string[]> entries;
        private const int CacheLimit = 2048;
        private readonly Dictionary<string, string> textCache = new Dictionary<string, string>();
        public string Language { get; private set; } = "en";

        public GenesisLocalization(Dictionary<string, string[]> translations)
        {
            entries = new Dictionary<string, string[]>();
            AddTranslations(translations);
        }

        public void AddTranslations(Dictionary<string, string[]> translations)
        {
            if (translations == null) return;
            foreach (var entry in translations)
                if (entry.Value == null || entry.Value.Length != 3 || Array.Exists(entry.Value, string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Incomplete translation: " + entry.Key);
            foreach (var entry in translations) entries[entry.Key] = entry.Value;
            textCache.Clear();
        }

        public bool Select(string language)
        {
            if (Array.IndexOf(Languages, language) < 0) return false;
            if (Language != language) textCache.Clear();
            Language = language;
            return true;
        }

        // Translate the stable sentence before substituting names and live values.
        public string Format(string source, params object[] values)
        { return string.Format(CultureInfo.InvariantCulture, Text(source), values); }

        public string Text(string source)
        {
            if (string.IsNullOrEmpty(source) || Language == "en") return source;
            int index = Array.IndexOf(Languages, Language) - 1;
            if (entries.TryGetValue(source, out string[] translated)) return translated[index];
            if (textCache.TryGetValue(source, out string cached)) return cached;
            string result = TranslateCaption(source, index);
            // Dynamic counters must not retain an unbounded history of captions.
            if (textCache.Count >= CacheLimit) textCache.Clear();
            textCache[source] = result;
            return result;
        }

        private string TranslateCaption(string source, int index)
        {
            string trimmed = source.Trim();
            if (entries.TryGetValue(trimmed, out string[] translated)) return source.Replace(trimmed, translated[index]);
            // Numeric captions use templates so counts, costs and timers remain live.
            string template = Regex.Replace(source, @"\d+(?:[.,]\d+)*", "{#}");
            if (entries.TryGetValue(template, out translated))
            {
                var numbers = Regex.Matches(source, @"\d+(?:[.,]\d+)*");
                int number = 0;
                return Regex.Replace(translated[index], @"\{#\}", _ => numbers[number++].Value);
            }
            if (source.Contains("\n"))
                return string.Join("\n", Array.ConvertAll(source.Split('\n'), Text));
            return source;
        }
    }
}