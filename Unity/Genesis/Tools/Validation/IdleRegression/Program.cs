using System;
using System.IO;
using System.Linq;
using Genesis;

static class Program
{
    static int checks;
    static void Check(bool result, string label)
    {
        checks++;
        if (!result) throw new Exception("FAIL: " + label);
    }
    static GenesisCatalog Catalog() => GenesisCatalog.FromJson(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "../../../../../../Assets/Genesis/Resources/Genesis/Data/catalog.json")));

    static void Main()
    {
        TestProtagonistProfile();
        TestGatheringRewards();
        TestBaseHopeSpeed();
        TestLocalizationCache();
        var catalog = Catalog();
        Check(EconomyMath.Format(0.005, catalog.number_suffixes) == "0.005", "small Hope balance");
        Check(EconomyMath.FormatRate(0.005, catalog.number_suffixes) == "0.005", "starter production precision");
        Check(EconomyMath.FormatRate(0, catalog.number_suffixes) == "0.000", "zero rate precision");
        Check(EconomyMath.FormatRate(0.00001, catalog.number_suffixes) == "1E-5", "sub-milli rate not rounded to zero");
        Check(EconomyMath.FormatRate(double.NaN, catalog.number_suffixes) == "0.000", "invalid rate sanitized");
        Check(EconomyMath.Format(1000, catalog.number_suffixes).StartsWith("1.00"), "large formatting retained");
        var game = new GenesisGame(catalog);
        game.AddQuanta(5);
        Check(game.Buy(catalog.generators[0].id), "starter purchase");
        game.BuyMode = -1;
        string json = game.SaveJson(1000);
        var restored = new GenesisGame(Catalog());
        double gain = restored.Restore(json, 1000 + 86400 * 30, true);
        Check(Math.Abs(gain - game.TotalQps * 86400 * 30) < gain * 1e-10, "30 days full offline income without cap");
        Check(restored.BuyMode == -1, "buy mode persisted");
        Check(restored.Counts[catalog.generators[0].id] == 1, "owned fairies persisted");
        Check(new GenesisGame(Catalog()).Restore(json, 900, true) == 0, "backward clock no earnings");
        Check(new GenesisGame(Catalog()).Restore(json, 1000, true) == 0, "same timestamp no earnings");
        var gather = new GenesisGathering(game);
        gather.Advance(0.75);
        var second = new GenesisGathering(game);
        second.RestoreState(gather.SaveState());
        Check(second.Progress(catalog.generators[0].id) == gather.Progress(catalog.generators[0].id), "partial cycle persisted");
        double before = game.Quanta;
        second.Advance(1.25);
        Check(Math.Abs(game.Quanta - before - game.TotalQps * 2) < 1e-8, "pending cycle paid once");
        foreach (double interval in new[] { 6 * 3600.0, 12 * 3600.0, 24 * 3600.0 }) Simulate(interval);
        Console.WriteLine("PASS " + checks + " idle regression checks");
    }

    static void TestGatheringRewards()
    {
        var catalog = Catalog();
        var game = new GenesisGame(catalog);
        var gather = new GenesisGathering(game);
        Check(gather.StarterRewardAmount == 25, "first gather receives fivefold Hope");
        Check(gather.BeginStarter() && !gather.BeginStarter(), "gather rejects duplicate start");
        gather.Advance(1);
        Check(game.Quanta == 0, "gather cannot pay early");
        var saved = gather.SaveState();
        var resumed = new GenesisGathering(game);
        resumed.RestoreState(saved);
        resumed.Advance(1);
        Check(game.Quanta == 25 && resumed.StarterPendingReward == 0, "pending gather survives save and pays once");
        resumed.Advance(10);
        Check(game.Quanta == 25, "gather does not repeat automatically");
        var progress = Newtonsoft.Json.Linq.JObject.Parse(game.SaveJson(1000));
        progress["read_stories"] = new Newtonsoft.Json.Linq.JArray(catalog.chapters.Take(3).Select(c => c.id));
        game.Restore(progress.ToString(), 1000);
        Check(gather.StarterRewardAmount == 100, "reading story without claiming gifts increases gather");
        gather.RestoreState(saved);
        Check(gather.StarterPendingReward == 25, "pending reward is not repriced by story progress");
        saved["starterReward"] = 5;
        gather.RestoreState(saved);
        Check(gather.StarterPendingReward == 5, "old explicit pending reward is not multiplied retroactively");
        saved.Remove("starterReward");
        gather.RestoreState(saved);
        Check(gather.StarterPendingReward == 5, "legacy pending gathers preserve five Hope");
        var counts = (Newtonsoft.Json.Linq.JArray)progress["counts"];
        counts.First(row => (string)row["id"] == catalog.generators[0].id)["value"] = 10000;
        game.Restore(progress.ToString(), 1000);
        Check(gather.StarterRewardAmount >= game.TotalQps * GenesisGathering.StarterDuration && gather.StarterRewardAmount > 20,
            "later gather scales with production and story progress");
        double reward = gather.StarterRewardAmount;
        var copy = new GenesisGathering(game);
        Check(copy.BeginStarter() && copy.StarterPendingReward == reward, "gather snapshots advertised reward");
        var cycleState = copy.SaveState();
        var controlGame = new GenesisGame(catalog);
        controlGame.Restore(game.SaveJson(1000), 1000);
        var control = new GenesisGathering(controlGame);
        var restoredGather = new GenesisGathering(game);
        restoredGather.RestoreState(cycleState);
        restoredGather.Advance(2);
        control.Advance(2);
        Check(Math.Abs(game.Quanta - controlGame.Quanta - reward) < Math.Max(1e-8, reward * 1e-10),
            "scaled reward pays exactly once alongside passive cycles");
    }

    static void TestBaseHopeSpeed()
    {
        var catalog = Catalog();
        var game = new GenesisGame(catalog);
        Check(game.GlobalMultiplier == 5, "base multiplier is fivefold");
        Check(game.Tap() == 5, "base tap pays five Hope");
        Check(game.Buy(catalog.generators[0].id), "costs unchanged at five Hope");
        double originalRate = EconomyMath.Production(catalog.generators[0], 1, 1);
        Check(Math.Abs(game.TotalQps - originalRate * 5) < 1e-12, "passive production exactly fivefold");
        double balance = game.Quanta;
        game.Tick(2);
        Check(Math.Abs(game.Quanta - balance - originalRate * 10) < 1e-12, "continuous production uses fivefold rate");
        balance = game.Quanta;
        var gathering = new GenesisGathering(game);
        gathering.Advance(2);
        Check(Math.Abs(game.Quanta - balance - originalRate * 10) < 1e-12, "gathering production uses fivefold rate");
    }

    static void TestLocalizationCache()
    {
        var entries = new System.Collections.Generic.Dictionary<string, string[]> {
            ["Hope / {#}"] = new[] { "Esperanza / {#}", "Надежда / {#}", "希望 / {#}" }
        };
        var loc = new GenesisLocalization(entries);
        loc.Select("es");
        Check(loc.Text("Hope / 123") == "Esperanza / 123" && loc.Text("Hope / 123") == "Esperanza / 123", "numeric cache preserves numbers");
        loc.Select("zh");
        Check(loc.Text("Hope / 123") == "希望 / 123", "cache invalidated on language change");
        Check(loc.Text("Unknown") == "Unknown", "unknown source fallback cached");
        loc.AddTranslations(new System.Collections.Generic.Dictionary<string, string[]> {
            ["Unknown"] = new[] { "Nuevo", "Новое", "新" }
        });
        Check(loc.Text("Unknown") == "新", "added translations invalidate fallback cache");
        for (int i = 0; i < 3000; i++) loc.Text("Hope / " + i);
        Check(loc.Text("Hope / 123") == "希望 / 123", "bounded cache eviction preserves translation");
        loc.Select("en");
        Check(loc.Text("Hope / 123") == "Hope / 123", "English source preserved after switching");
    }

    static void TestProtagonistProfile()
    {
        var profile = new GenesisProtagonistProfile();
        Check(!profile.Confirmed && profile.Selected == "light", "first start requires protagonist confirmation");
        Check(!string.IsNullOrWhiteSpace(profile.LightName) && !string.IsNullOrWhiteSpace(profile.DarknessName), "both characters have suggested names");
        Check(!profile.Choose("invalid", "Name") && !profile.Choose("darkness", " \n "), "invalid identity and blank name rejected");
        Check(profile.Choose("darkness", "  Nýra 月  ") && profile.Name == "Nýra 月", "editable Unicode character name");
        Check(profile.Choose("light", "Aurora") && profile.DarknessName == "Nýra 月", "individual character names retained");
        var restored = new GenesisProtagonistProfile();
        restored.Restore(profile.Save());
        Check(restored.Confirmed && restored.Selected == "light" && restored.Name == "Aurora" && restored.DarknessName == "Nýra 月", "profile roundtrip");
        Check(GenesisProtagonistProfile.CleanName("<b>A\nB\t</b>\u200b") == "bAB/b", "control and rich-text delimiters removed");
        Check(new System.Globalization.StringInfo(GenesisProtagonistProfile.CleanName(new string('a', 40))).LengthInTextElements == 24, "long names bounded");
        Check(GenesisProtagonistProfile.CleanName(string.Concat(Enumerable.Repeat("🌙", 30))).Length == 48, "name truncation preserves surrogate pairs");
        restored.Restore(null);
        Check(!restored.Confirmed && restored.Selected == "light", "legacy profile defaults require selection");
        restored.Restore(Newtonsoft.Json.Linq.JObject.Parse("{\"selected\":\"unknown\",\"confirmed\":true}"));
        Check(!restored.Confirmed && restored.Selected == "light", "invalid saved character is not confirmed");
    }

    static void Simulate(double interval)
    {
        var game = new GenesisGame(Catalog());
        var gather = new GenesisGathering(game);
        // Five starting taps, then no active tap income, journeys, relics, prestige or codes.
        for (int i = 0; i < 5; i++) game.Tap();
        double time = 0;
        int actions = 0;
        while (time <= 120 * 86400 && (!game.CampaignCompleted || game.Counts.Any(x => x.Value == 0)))
        {
            // A short visit: recruit each affordable fairy, read and claim available stories.
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (var chapter in game.Catalog.chapters)
                    if (game.ChapterProgress(chapter) >= chapter.target)
                    {
                        game.FinishStory(chapter.id);
                        if (game.ClaimChapter(chapter.id)) actions++;
                    }
                foreach (var generator in game.Catalog.generators)
                {
                    // Recruit everyone once, then invest in the latest progression fairy.
                    bool latest = string.IsNullOrEmpty(generator.recruit_after) && game.CreationLevel > 0 &&
                        game.Catalog.generators[game.CreationLevel - 1].id == generator.id;
                    if (game.Counts[generator.id] == 0 || latest)
                        if (game.Buy(generator.id)) actions++;
                }
            }
            if (game.CampaignCompleted && game.Counts.All(x => x.Value > 0)) break;
            gather.Advance(interval);
            game.Tick(interval, false);
            time += interval;
        }
        Console.WriteLine("SIM check-in=" + interval / 3600 + "h days=" + time / 86400 +
            " chapters=" + game.CompletedChapters.Count + "/" + game.Catalog.chapters.Count +
            " fairies=" + game.Counts.Count(x => x.Value > 0) + " actions=" + actions);
        Check(game.CampaignCompleted && time <= 30 * 86400, "campaign within 30 days at " + interval / 3600 + "h visits");
        Check(game.Counts.All(x => x.Value > 0), "all 255 fairies recruited within 30 days");
        var restored = new GenesisGame(Catalog());
        restored.Restore(game.SaveJson(100), 100);
        Check(restored.CampaignCompleted, "completion persists");
    }
}