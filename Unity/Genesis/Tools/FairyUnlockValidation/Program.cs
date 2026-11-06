using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Genesis;
using Newtonsoft.Json.Linq;

internal static class Program
{
    private static int checks;

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
        checks++;
    }

    private static void Main(string[] args)
    {
        if (args.Length < 1 || args.Length > 2) throw new ArgumentException("Provide the absolute catalog.json path and optionally localization.json.");
        int localizationChecks = checks;
        if (args.Length == 2)
        {
            var entries = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(args[1]));
            var localization = new GenesisLocalization(entries);
            Check(localization.Text("Gather") == "Gather", "English source preserved");
            foreach (string language in GenesisLocalization.Languages.Skip(1))
            {
                Check(localization.Select(language), "select " + language);
                int index = Array.IndexOf(GenesisLocalization.Languages, language) - 1;
                foreach (var entry in entries)
                    Check(localization.Text(entry.Key) == entry.Value[index], "translation " + language + ": " + entry.Key);
                string expectedCount = entries["{#} / {#} fairies"][index];
                int firstPlaceholder = expectedCount.IndexOf("{#}", StringComparison.Ordinal);
                expectedCount = expectedCount.Remove(firstPlaceholder, 3).Insert(firstPlaceholder, "0").Replace("{#}", "8");
                Check(localization.Text("0 / 8 fairies") == expectedCount, "live fairy counts " + language);
                Check(localization.Text("Gather\n+123") == entries["Gather"][index] + "\n+123", "multiline caption " + language);
                Check(localization.Text("Aurelia") == "Aurelia", "proper name fallback");
                Check(localization.Text("     Gather") == "     " + entries["Gather"][index], "icon caption whitespace");
            }
            Check(!localization.Select("invalid") && localization.Language == "zh", "unknown language rejected");
            Console.WriteLine("PASS: " + checks + " localization table, numeric caption, fallback and language selection checks.");
            localizationChecks = checks;
        }
        GenesisCatalog catalog = GenesisCatalog.FromJson(File.ReadAllText(args[0]));
        var worlds = GenesisDimensions.Build(catalog);
        var game = new GenesisGame(catalog);
        game.AddQuanta(1e200);
        Check(!game.IsDiscovered("unknown") && !game.Buy("unknown"), "unknown fairy rejected");
        Check(game.IsDiscovered(catalog.generators[0].id), "starter available");
        Check(!game.Buy(catalog.generators[1].id), "Hope cannot bypass starter");
        Check(game.Buy(catalog.generators[0].id), "starter purchase");
        Check(game.IsDiscovered(catalog.generators[1].id), "next creation revealed");
        Check(!game.IsDiscovered(catalog.generators[2].id), "later creation hidden");

        foreach (GenesisDimensionLayer world in worlds)
        {
            // Give every original creation to isolate each world's resident chain.
            game = new GenesisGame(catalog);
            var saved = JObject.Parse(game.SaveJson(1000));
            saved["quanta"] = 1e200;
            foreach (JToken row in (JArray)saved["counts"])
                row["value"] = string.IsNullOrEmpty(catalog.generators.First(g => g.id == (string)row["id"]).recruit_after) ? 1 : 0;
            game.Restore(saved.ToString(), 1000);
            var residents = world.Generators.Where(g => !string.IsNullOrEmpty(g.recruit_after)).ToList();
            for (int i = 0; i < residents.Count; i++)
            {
                Check(game.IsDiscovered(residents[i].id), "next resident revealed " + residents[i].id);
                for (int later = i + 1; later < residents.Count; later++)
                {
                    double hope = game.Quanta;
                    Check(!game.IsDiscovered(residents[later].id), "later resident hidden " + residents[later].id);
                    Check(!game.Buy(residents[later].id), "later resident purchase rejected " + residents[later].id);
                    Check(game.Quanta == hope && game.Counts[residents[later].id] == 0, "rejected purchase unchanged");
                }
                Check(game.Buy(residents[i].id), "sequential resident purchase " + residents[i].id);
                Check(game.Buy(residents[i].id), "owned resident upgrade " + residents[i].id);
            }
            if (residents.Count > 1)
            {
                // Older saves can already own a later resident; do not revoke it.
                saved = JObject.Parse(game.SaveJson(1000));
                foreach (JToken row in (JArray)saved["counts"])
                    if ((string)row["id"] == residents[0].id) row["value"] = 0;
                game.Restore(saved.ToString(), 1000);
                Check(game.IsDiscovered(residents[1].id) && game.Buy(residents[1].id), "legacy owned resident preserved");
            }
        }
        game = new GenesisGame(catalog);
        var boundary = JObject.Parse(game.SaveJson(1000));
        boundary["quanta"] = 1e200;
        foreach (JToken row in (JArray)boundary["counts"])
            if (worlds[0].Generators.Any(g => g.id == (string)row["id"] && string.IsNullOrEmpty(g.recruit_after))) row["value"] = 1;
        game.Restore(boundary.ToString(), 1000);
        string nextWorld = worlds[1].Generators[0].id;
        Check(game.IsDiscovered(nextWorld), "optional residents do not block next world discovery");
        Check(game.RequiredWorldStory(nextWorld) != null && !game.Buy(nextWorld), "next world still requires stories");
        Console.WriteLine("PASS: " + (checks - localizationChecks) + " sequential fairy unlock checks across " + worlds.Count + " worlds.");
    }
}