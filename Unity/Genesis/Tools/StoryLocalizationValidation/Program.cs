using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Genesis;
using Newtonsoft.Json;

var root = @"E:\ZendaGame\Genesis";
var uiTranslations = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(root + @"\Assets\Genesis\Resources\Genesis\Data\localization.json"));
var uiLocalization = new GenesisLocalization(uiTranslations);
foreach (var language in GenesisLocalization.Languages)
{
    uiLocalization.Select(language);
    foreach (var entry in uiTranslations)
    {
        var placeholders = System.Text.RegularExpressions.Regex.Matches(entry.Key, @"\{\d+\}")
            .Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).OrderBy(x => x).ToArray();
        foreach (var translation in entry.Value)
        {
            if (System.Text.RegularExpressions.Regex.Matches(entry.Key, @"\{#\}").Count !=
                System.Text.RegularExpressions.Regex.Matches(translation, @"\{#\}").Count)
                throw new Exception("UI numeric placeholder mismatch: " + entry.Key);
            var translatedPlaceholders = System.Text.RegularExpressions.Regex.Matches(translation, @"\{\d+\}")
                .Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).OrderBy(x => x).ToArray();
            if (!placeholders.SequenceEqual(translatedPlaceholders)) throw new Exception("UI placeholder mismatch: " + entry.Key);
        }
        if (placeholders.Length > 0)
        {
            var result = uiLocalization.Format(entry.Key, "NAME", "VALUE", 2, 3);
            if (result.Contains("{0}")) throw new Exception("Unresolved UI placeholder: " + entry.Key);
        }
    }
    if (language != "en" && uiLocalization.Format("Shared by {0}", "Aster 123").Contains("Shared by"))
        throw new Exception("Dynamic UI caption was not translated: " + language);
    if (!uiLocalization.Format("Shared by {0}", "Aster 123").Contains("Aster 123"))
        throw new Exception("Dynamic UI caption changed a proper name.");
    foreach (var caption in new[] { "Fairy halos / 2 of 14", "Fairy halos / 2 unlocked",
        "Speed set to 1,000,000x, including offline gathering and journeys. Codes do not stack." })
        if (language != "en" && uiLocalization.Text(caption) == caption)
            throw new Exception("Numeric UI caption was not translated: " + caption);
}
Console.WriteLine("UI translations validated: " + uiTranslations.Count + " entries across four languages.");
var catalog = GenesisCatalog.FromJson(File.ReadAllText(root + @"\Assets\Genesis\Resources\Genesis\Data\catalog.json"));
const int fullyTranslatedChapterCount = 13;
var sources = new HashSet<string>();
foreach (var chapter in catalog.chapters)
{
    sources.Add(chapter.title);
    sources.Add(chapter.goal);
    sources.Add(chapter.lore);
    foreach (var page in GenesisStorytelling.Build(catalog, chapter, null, false)) sources.Add(page.Text);
}
foreach (var generator in catalog.generators)
{
    sources.Add(generator.name);
    sources.Add(generator.dimension);
    sources.Add(generator.desc);
    foreach (var friend in generator.resident_friends ?? new List<ResidentFriendDefinition>())
    {
        sources.Add(friend.home);
        sources.Add(friend.title);
        sources.Add(new GenesisLadyGalleryEntry(generator, friend, null).History);
    }
}
sources.RemoveWhere(string.IsNullOrWhiteSpace);
var names = catalog.generators.SelectMany(g => (g.resident_friends ?? new List<ResidentFriendDefinition>()).Select(f => f.name))
    .Concat(catalog.generators.Select(g => g.fairy)).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderByDescending(n => n.Length).ToArray();
File.WriteAllText(root + @"\Temp\story-sources.json", JsonConvert.SerializeObject(new { sources = sources.OrderBy(s => s).ToArray(), names }, Formatting.Indented));
File.WriteAllText(root + @"\Temp\opening-stories.json", JsonConvert.SerializeObject(catalog.chapters.Take(fullyTranslatedChapterCount).Select(c => new {c.id, c.title, c.goal, c.lore, passages=GenesisStorytelling.Build(catalog,c,null,false).Select(p=>p.Text)}),Formatting.Indented));
Console.WriteLine($"Exported {sources.Count} sources / {sources.Sum(s => s.Length)} characters; {names.Length} names.");
File.WriteAllText(root + @"\Temp\next-complete-chapters.json", JsonConvert.SerializeObject(catalog.chapters.Skip(fullyTranslatedChapterCount).Take(2).Select(c => new {
    c.id, c.title, c.goal, c.lore,
    passages = GenesisStorytelling.Build(catalog, c, null, false).Select(p => p.Text)
}), Formatting.Indented));
var translated = JsonConvert.DeserializeObject<Dictionary<string,string[]>>(File.ReadAllText(root + @"\Assets\Genesis\Resources\Genesis\Data\story-localization.json"));
var localization = new GenesisLocalization(translated);
foreach (var entry in translated)
{
    if (!sources.Contains(entry.Key)) throw new Exception("Unknown story translation key: " + entry.Key);
    foreach (var value in entry.Value)
    {
        foreach (var name in names.Where(n => entry.Key.Contains(n)))
            if (!value.Contains(name)) throw new Exception("Translated story changed name: " + name);
        if (value.Contains('\uFFFD')) throw new Exception("Invalid Unicode in story translation: " + entry.Key);
        if (entry.Key.Count(c => c == '\n') != value.Count(c => c == '\n'))
            throw new Exception("Story paragraph breaks changed: " + entry.Key);
    }
}
int checks = 0;
foreach (var language in GenesisLocalization.Languages)
{
    localization.Select(language);
    foreach (var chapter in catalog.chapters)
    {
        var english = GenesisStorytelling.Build(catalog, chapter);
        var pages = GenesisStorytelling.Build(catalog, chapter, localization.Text);
        if (pages.Count == 0 || pages.Any(p => p.Text.Length > GenesisStorytelling.PageLength || string.IsNullOrWhiteSpace(p.Text))) throw new Exception("Invalid pages " + chapter.id);
        if (!english.Select(p=>p.Speaker).Distinct().SequenceEqual(pages.Select(p=>p.Speaker).Distinct())) throw new Exception("Speaker changed");
        if (language == "en" && !english.Select(p=>p.Text).SequenceEqual(pages.Select(p=>p.Text))) throw new Exception("English regression");
        if (language != "en" && catalog.chapters.Take(fullyTranslatedChapterCount).Contains(chapter))
            foreach (var text in GenesisStorytelling.Build(catalog, chapter, null, false).Select(p => p.Text)
                .Concat(new[] { chapter.title, chapter.goal, chapter.lore }).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!translated.ContainsKey(text) || localization.Text(text) == text)
                    throw new Exception("Untranslated " + chapter.id + " / " + language + ": " + text);
                foreach (var name in names.Where(n => text.Contains(n)))
                    if (!localization.Text(text).Contains(name)) throw new Exception("Translated story changed name: " + name);
            }
        checks++;
    }
}
Console.WriteLine($"PASS: {checks} chapter/language pagination and metadata checks; first {fullyTranslatedChapterCount} chapters fully translated in es/ru/zh, including titles, goals, lore and all conversation passages.");
var missing = sources.Where(s=>!translated.ContainsKey(s)).OrderBy(s=>s).ToArray();
File.WriteAllText(root + @"\Temp\story-translations-missing.json",JsonConvert.SerializeObject(missing,Formatting.Indented));
Console.WriteLine($"Coverage: {translated.Count} translated entries; {missing.Length} remaining source strings. Full-library translation is NOT complete.");