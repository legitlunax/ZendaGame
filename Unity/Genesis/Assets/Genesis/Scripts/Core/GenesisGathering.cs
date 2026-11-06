using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Genesis
{
    // Session-only production cycles. The legacy game retains continuous production by default.
    public sealed class GenesisGathering
    {
        private sealed class Cycle
        {
            public double Elapsed, Pending, Gathered;
            public double Duration;
        }

        private readonly GenesisGame game;
        private readonly Dictionary<string, Cycle> cycles = new Dictionary<string, Cycle>();
        public const double StarterDuration = 2;
        public const double StarterReward = 5;
        public double StarterRewardAmount
        {
            get
            {
                int stories = game.Catalog.chapters.Count(chapter => game.HasReadStory(chapter.id));
                return Math.Max(StarterReward * GenesisGame.BaseHopeMultiplier * (1 + stories),
                    game.TotalQps * (StarterDuration + Math.Min(100, stories) * 0.05));
            }
        }
        public double StarterPendingReward { get; private set; }
        public double StarterRemaining { get; private set; }
        public bool StarterRunning => StarterRemaining > 0;

        public JObject SaveState()
        {
            var rows = new JObject();
            foreach (var row in cycles)
                rows[row.Key] = new JObject { ["elapsed"] = row.Value.Elapsed, ["pending"] = row.Value.Pending, ["gathered"] = row.Value.Gathered };
            return new JObject { ["starter"] = StarterRemaining, ["starterReward"] = StarterPendingReward, ["cycles"] = rows };
        }

        public void RestoreState(JObject state)
        {
            if (state == null) return;
            StarterRemaining = Math.Min(StarterDuration, Safe(state["starter"]));
            StarterPendingReward = StarterRunning
                ? (state["starterReward"] == null ? StarterReward : Safe(state["starterReward"])) : 0;
            foreach (var row in cycles)
            {
                JToken saved = state["cycles"]?[row.Key];
                row.Value.Elapsed = Math.Min(row.Value.Duration, Safe(saved?["elapsed"]));
                row.Value.Pending = Safe(saved?["pending"]);
                row.Value.Gathered = Safe(saved?["gathered"]);
            }
        }

        private static double Safe(JToken token)
        {
            double value;
            return double.TryParse((string)token, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value) && EconomyMath.IsFinite(value) ? Math.Max(0, value) : 0;
        }

        public GenesisGathering(GenesisGame game)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game));
            for (int i = 0; i < game.Catalog.generators.Count; i++)
                cycles.Add(game.Catalog.generators[i].id, new Cycle { Duration = 2 + i % 5 });
        }

        public double Duration(string id)
        {
            if (!cycles.TryGetValue(id, out Cycle cycle)) throw new ArgumentException("Unknown discovery", nameof(id));
            return cycle.Duration;
        }

        public double Progress(string id) { return cycles[id].Elapsed / Duration(id); }
        public double Pending(string id) { return cycles[id].Pending; }
        public double Gathered(string id) { return cycles[id].Gathered; }

        public bool BeginStarter()
        {
            if (StarterRunning) return false;
            StarterRemaining = StarterDuration;
            StarterPendingReward = StarterRewardAmount;
            return true;
        }

        public void Advance(double seconds)
        {
            if (!EconomyMath.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (seconds == 0) return;
            double starterPayout = 0;
            if (StarterRunning)
            {
                StarterRemaining = Math.Max(0, StarterRemaining - seconds);
                if (!StarterRunning)
                {
                    starterPayout = StarterPendingReward;
                    StarterPendingReward = 0;
                }
            }
            double multiplier = game.GlobalMultiplier;
            double deposit = 0;
            foreach (GeneratorDefinition generator in game.Catalog.generators)
            {
                Cycle cycle = cycles[generator.id];
                long owned = game.Counts[generator.id];
                if (owned <= 0) { cycle.Elapsed = cycle.Pending = 0; continue; }
                double duration = Duration(generator.id);
                double rate = EconomyMath.Production(generator, owned, multiplier);
                double untilCompletion = duration - cycle.Elapsed;
                if (seconds < untilCompletion)
                {
                    cycle.Elapsed += seconds;
                    cycle.Pending += rate * seconds;
                    continue;
                }
                // Integrate partial work at the old rate before an upgrade; no retroactive repricing.
                double remaining = seconds - untilCompletion;
                double remainder = remaining % duration;
                double payout = cycle.Pending + rate * (seconds - remainder);
                if (EconomyMath.IsFinite(payout) && payout > 0)
                {
                    deposit += payout;
                    cycle.Gathered += payout;
                }
                cycle.Elapsed = remainder;
                cycle.Pending = rate * remainder;
            }
            game.AddQuanta(deposit);
            game.AddQuanta(starterPayout);
        }
    }
}