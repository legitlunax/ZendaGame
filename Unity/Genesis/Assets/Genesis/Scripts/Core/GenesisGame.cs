using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Genesis
{
    // Pure domain model: no Unity scene, frame, renderer, or storage dependencies.
    public sealed class GenesisGame
    {
        // Permanent income pacing; separate from codes that scale simulated time.
        public const double BaseHopeMultiplier = 5;
        private static readonly string[] ElementIds = { "element_wood", "element_fire", "element_earth", "element_metal", "element_water" };
        private static readonly string[] LivingIds = { "gaia_biosphere", "all_seeing", "omnipresence", "omnipotence" };
        public GenesisCatalog Catalog { get; }
        public double Quanta { get; private set; }
        public double LifetimeQuanta { get; private set; }
        public double RunQuanta { get; private set; }
        public long Seeds { get; private set; }
        public int GoddessStage { get; private set; }
        public string EquippedAppearance { get; private set; } = "ivory_queen";
        public int BuyMode { get; set; } = 1;
        public double ChronoTimer { get; private set; }
        public bool CampaignCompleted { get; private set; }
        private readonly Dictionary<string, long> counts = new Dictionary<string, long>();
        private readonly Dictionary<string, long> relics = new Dictionary<string, long>();
        private readonly Dictionary<string, double> cooldowns = new Dictionary<string, double>();
        private readonly HashSet<string> completed = new HashSet<string>();
        private readonly HashSet<string> readStories = new HashSet<string>();
        private IReadOnlyList<GenesisDimensionLayer> worldLayers;
        public IReadOnlyDictionary<string, long> Counts => counts;
        public IReadOnlyDictionary<string, long> Relics => relics;
        public IReadOnlyCollection<string> CompletedChapters => completed;

        public GenesisGame(GenesisCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            foreach (GeneratorDefinition generator in Catalog.generators) counts.Add(generator.id, 0);
            foreach (RelicDefinition relic in Catalog.relics) relics.Add(relic.id, 0);
            foreach (MiracleDefinition miracle in Catalog.miracles) cooldowns.Add(miracle.id, 0);
        }

        private long Count(string id) { return counts.TryGetValue(id, out long value) ? value : 0; }
        private long Relic(string id) { return relics.TryGetValue(id, out long value) ? value : 0; }

        public int CreationLevel
        {
            get
            {
                int level = 0;
                for (int i = 0; i < Catalog.generators.Count; i++)
                    if (string.IsNullOrEmpty(Catalog.generators[i].recruit_after) && Count(Catalog.generators[i].id) > 0) level = i + 1;
                return level;
            }
        }

        public bool HasReached(string id)
        {
            int index = Catalog.generators.FindIndex(generator => generator.id == id);
            return index >= 0 && CreationLevel > index;
        }

        public bool IsDiscovered(string id)
        {
            int index = Catalog.generators.FindIndex(generator => generator.id == id);
            if (index < 0) return false;
            // Preserve owned fairies from older saves, but never reveal a new recruit
            // just because its home world's anchor has been purchased.
            if (Count(id) > 0) return true;
            if (!string.IsNullOrEmpty(Catalog.generators[index].recruit_after))
            {
                if (worldLayers == null) worldLayers = GenesisDimensions.Build(Catalog);
                foreach (GenesisDimensionLayer world in worldLayers)
                    for (int i = 1; i < world.Generators.Count; i++)
                        if (world.Generators[i].id == id)
                            return Count(world.Generators[i - 1].id) > 0;
                return false;
            }
            // World progression follows its original creation chain; optional
            // residents do not become prerequisites for entering the next world.
            GeneratorDefinition previous = Catalog.generators.Take(index)
                .LastOrDefault(generator => string.IsNullOrEmpty(generator.recruit_after));
            return previous == null || Count(previous.id) > 0;
        }

        public double GlobalMultiplier
        {
            get
            {
                long duality = Math.Min(100, Math.Min(Count("yin"), Count("yang")));
                long elements = 100;
                foreach (string id in ElementIds)
                    elements = Math.Min(elements, Count(id));
                long living = 50;
                foreach (string id in LivingIds)
                    living = Math.Min(living, Count(id));
                return BaseHopeMultiplier * Catalog.goddess_forms[GoddessStage].mult_bonus * (1 + Seeds * 0.25) *
                    (1 + Relic("relic_chronos") * 0.5) * (1 + duality * 0.01) * (1 + elements * 0.02) *
                    (1 + living * 0.02) * (ChronoTimer > 0 ? 3 : 1);
            }
        }

        public double TotalQps
        {
            get
            {
                double multiplier = GlobalMultiplier;
                double total = 0;
                foreach (GeneratorDefinition generator in Catalog.generators)
                    total += EconomyMath.Production(generator, Count(generator.id), multiplier);
                return total;
            }
        }
        public string ResourceName => HasReached("quark_filament") ? "Creation Potential" : "Hope";
        public double CostDiscount => Math.Max(0.5, Math.Min(1, 1 - Relic("relic_wormhole_loom") * 0.05));
        public ChapterDefinition ActiveChapter => Catalog.chapters.FirstOrDefault(chapter => !completed.Contains(chapter.id));

        public void AddQuanta(double amount)
        {
            if (!EconomyMath.IsFinite(amount) || amount <= 0 || !EconomyMath.IsFinite(Quanta + amount) ||
                !EconomyMath.IsFinite(RunQuanta + amount) || !EconomyMath.IsFinite(LifetimeQuanta + amount)) return;
            Quanta += amount;
            RunQuanta += amount;
            LifetimeQuanta += amount;
            while (GoddessStage + 1 < Catalog.goddess_forms.Count && RunQuanta >= Catalog.goddess_forms[GoddessStage + 1].req_quanta)
                GoddessStage++;
        }

        public double Tap()
        {
            double gain = GlobalMultiplier + TotalQps * (0.01 + Relic("relic_seraph_grace") * 0.02);
            AddQuanta(gain);
            return gain;
        }

        public void Tick(double delta, bool produce = true)
        {
            if (!EconomyMath.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            ChronoTimer = Math.Max(0, ChronoTimer - delta);
            foreach (MiracleDefinition miracle in Catalog.miracles)
                cooldowns[miracle.id] = Math.Max(0, cooldowns[miracle.id] - delta);
            if (produce) AddQuanta(TotalQps * delta);
        }

        public BulkPurchase PurchaseInfo(string id)
        {
            GeneratorDefinition generator = Catalog.generators.FirstOrDefault(row => row.id == id);
            if (generator == null) throw new ArgumentException("Unknown generator", nameof(id));
            return EconomyMath.Bulk(generator, Count(id), Quanta, BuyMode, CostDiscount);
        }

        public bool Buy(string id)
        {
            if (!IsDiscovered(id)) return false;
            if (RequiredWorldStory(id) != null) return false;
            BulkPurchase info = PurchaseInfo(id);
            if (!info.CanAfford) return false;
            Quanta -= info.Cost;
            counts[id] += info.Amount;
            return true;
        }

        public bool HasReadStory(string id) { return readStories.Contains(id) || completed.Contains(id); }

        // Called only after the reader's final-page confirmation. Gifts are separate.
        public bool FinishStory(string id)
        {
            ChapterDefinition chapter = Catalog.chapters.FirstOrDefault(c => c.id == id);
            if (chapter == null || (!completed.Contains(id) && ChapterProgress(chapter) < chapter.target)) return false;
            readStories.Add(id);
            if (Catalog.chapters.All(c => HasReadStory(c.id))) CampaignCompleted = true;
            return true;
        }

        public ChapterDefinition RequiredWorldStory(string generatorId)
        {
            if (!counts.ContainsKey(generatorId) || Count(generatorId) > 0) return null;
            if (!string.IsNullOrEmpty(Catalog.generators.First(g => g.id == generatorId).recruit_after)) return null;
            if (worldLayers == null) worldLayers = GenesisDimensions.Build(Catalog);
            int world = -1;
            for (int i = 0; i < worldLayers.Count; i++)
                if (worldLayers[i].Generators.Any(g => g.id == generatorId)) { world = i; break; }
            if (world <= 0 || worldLayers[world].IsFormed(this)) return null;
            // Include earlier worlds too, so imported progress cannot skip an unread history.
            return worldLayers.Take(world).SelectMany(w => w.Chapters).FirstOrDefault(c => !HasReadStory(c.id));
        }

        public double ChapterProgress(ChapterDefinition chapter)
        { return chapter == null ? 0 : Math.Min(chapter.target, chapter.kind == "quanta" ? RunQuanta : Count(chapter.kind)); }

        public bool ClaimChapter()
        {
            ChapterDefinition chapter = ActiveChapter;
            return chapter != null && ClaimChapter(chapter.id);
        }

        public bool ClaimChapter(string id)
        {
            ChapterDefinition chapter = Catalog.chapters.FirstOrDefault(row => row.id == id);
            if (chapter != null && completed.Contains(chapter.id)) return false;
            if (chapter == null || ChapterProgress(chapter) < chapter.target) return false;
            completed.Add(chapter.id);
            if (completed.Count == Catalog.chapters.Count) CampaignCompleted = true;
            AddQuanta(chapter.reward);
            return true;
        }

        public bool IsEmotionUnlocked(string id)
        { return Catalog.emotions.Any(row => row.id == id && (row.always_available || completed.Contains(row.chapter_id))); }

        public bool IsAppearanceUnlocked(string id)
        { return Catalog.wardrobe.Any(row => row.id == id && (string.IsNullOrEmpty(row.chapter) || completed.Contains(row.chapter))); }

        public bool Equip(string id)
        {
            if (!IsAppearanceUnlocked(id)) return false;
            EquippedAppearance = id;
            return true;
        }

        public bool ActivateMiracle(string id)
        {
            MiracleDefinition miracle = Catalog.miracles.FirstOrDefault(row => row.id == id);
            if (miracle == null || !HasReached("hadron_forge") || cooldowns[id] > 0) return false;
            cooldowns[id] = miracle.cooldown;
            if (id == "genesis_burst") AddQuanta(Math.Max(50, TotalQps * 30));
            if (id == "chrono_harmony") ChronoTimer = miracle.duration;
            return true;
        }

        public long PendingSeeds
        {
            get
            {
                if (!HasReached("wormhole_nexus") || RunQuanta < 1e6) return 0;
                double value = Math.Floor(Math.Pow(RunQuanta / 1e6, 0.35));
                return EconomyMath.IsFinite(value) && value < long.MaxValue ? Math.Max(1, (long)value) : 0;
            }
        }

        public bool Bloom()
        {
            long earned = PendingSeeds;
            if (earned <= 0 || Seeds > long.MaxValue - earned) return false;
            Seeds += earned;
            Quanta = RunQuanta = ChronoTimer = 0;
            GoddessStage = 0;
            foreach (string id in counts.Keys.ToArray()) counts[id] = 0;
            foreach (string id in cooldowns.Keys.ToArray()) cooldowns[id] = 0;
            return true;
        }

        public bool BuyRelic(string id)
        {
            RelicDefinition relic = Catalog.relics.FirstOrDefault(row => row.id == id);
            if (relic == null || relics[id] >= relic.max_lvl) return false;
            long cost = checked(relic.base_seed_cost * (relics[id] + 1));
            if (Seeds < cost) return false;
            Seeds -= cost;
            relics[id]++;
            return true;
        }

        private static double Number(JToken token)
        {
            if (token != null && (token.Type == JTokenType.Float || token.Type == JTokenType.Integer))
            {
                double numeric = token.Value<double>();
                return EconomyMath.IsFinite(numeric) ? Math.Max(0, numeric) : 0;
            }
            double value;
            return token != null && double.TryParse(token.ToString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value) && EconomyMath.IsFinite(value) ? Math.Max(0, value) : 0;
        }

        private static long Integer(JToken token, long maximum = long.MaxValue)
        {
            // Preserve 64-bit integral JSON counts; never round-trip them through double.
            if (token != null && token.Type == JTokenType.Integer)
            {
                long value;
                return long.TryParse(token.ToString(), out value) ? Math.Min(maximum, Math.Max(0, value)) : maximum;
            }
            double number = Number(token);
            return number >= maximum ? maximum : (long)number;
        }

        private static JToken Entry(JToken token, string id)
        {
            if (token is JObject map) return map[id];
            if (token is JArray list) return list.FirstOrDefault(row => (string)row["id"] == id)?["value"];
            return null;
        }

        // Handles native Godot dictionaries and the handoff's list-based converted format.
        // Offline rewards are explicit: fixture/demo imports must always leave this false.
        public double Restore(string json, double now, bool awardOffline = false)
        {
            JObject data = JObject.Parse(json);
            Quanta = Number(data["quanta"]);
            RunQuanta = Number(data["current_run_quanta"]);
            LifetimeQuanta = Number(data["lifetime_quanta"]);
            Seeds = Integer(data["genesis_seeds"]);
            GoddessStage = (int)Integer(data["goddess_stage"], Catalog.goddess_forms.Count - 1);
            completed.Clear();
            readStories.Clear();
            HashSet<string> savedStories = new HashSet<string>((data["read_stories"] as JArray ?? new JArray()).Values<string>());
            foreach (ChapterDefinition chapter in Catalog.chapters) if (savedStories.Contains(chapter.id)) readStories.Add(chapter.id);
            HashSet<string> saved = new HashSet<string>((data["completed_chapters"] as JArray ?? new JArray()).Values<string>());
            foreach (ChapterDefinition chapter in Catalog.chapters) if (saved.Contains(chapter.id)) completed.Add(chapter.id);
            foreach (string id in counts.Keys.ToArray()) counts[id] = Integer(Entry(data["counts"], id));
            foreach (RelicDefinition relic in Catalog.relics) relics[relic.id] = Integer(Entry(data["relic_levels"], relic.id), relic.max_lvl);
            EquippedAppearance = (string)data["equipped_appearance"] ?? "ivory_queen";
            if (!IsAppearanceUnlocked(EquippedAppearance)) EquippedAppearance = "ivory_queen";
            CampaignCompleted = (bool?)data["campaign_completed"] == true || Catalog.chapters.All(c => HasReadStory(c.id));
            ChronoTimer = Number(data["chrono_timer"]);
            int mode = (int?)data["buy_mode"] ?? 1;
            BuyMode = mode == 1 || mode == 10 || mode == 100 || mode == -1 ? mode : 1;
            foreach (string id in cooldowns.Keys.ToArray()) cooldowns[id] = Number(Entry(data["cooldowns"], id));
            double timestamp = Number(data["timestamp"]);
            double elapsed = timestamp > 0 && EconomyMath.IsFinite(now) ? Math.Max(0, now - timestamp) : 0;
            double before = Quanta;
            if (awardOffline)
            {
                double boosted = Math.Min(elapsed, ChronoTimer);
                AddQuanta(TotalQps * boosted);
                Tick(boosted, false);
                Tick(elapsed - boosted);
            }
            double gain = Quanta - before;
            return gain;
        }

        public string SaveJson(double timestamp)
        {
            if (!EconomyMath.IsFinite(timestamp) || timestamp < 0) throw new ArgumentOutOfRangeException(nameof(timestamp));
            return JObject.FromObject(new
            {
                schema_version = 1, quanta = Quanta, lifetime_quanta = LifetimeQuanta,
                campaign_completed = CampaignCompleted, buy_mode = BuyMode, chrono_timer = ChronoTimer,
                cooldowns = cooldowns.Select(row => new { id = row.Key, value = row.Value }).ToArray(),
                current_run_quanta = RunQuanta, genesis_seeds = Seeds, goddess_stage = GoddessStage,
                equipped_appearance = EquippedAppearance,
                completed_chapters = Catalog.chapters.Where(row => completed.Contains(row.id)).Select(row => row.id).ToArray(),
                read_stories = Catalog.chapters.Where(row => readStories.Contains(row.id)).Select(row => row.id).ToArray(),
                counts = counts.Select(row => new { id = row.Key, value = row.Value }).ToArray(),
                relic_levels = relics.Select(row => new { id = row.Key, value = row.Value }).ToArray(), timestamp
            }).ToString();
        }

        public void LoadDemoSnapshot(string snapshotsJson, int index)
        {
            JArray snapshots = (JArray)JObject.Parse(snapshotsJson)["snapshots"];
            if (index < 0 || index >= snapshots.Count) throw new ArgumentOutOfRangeException(nameof(index));
            Restore(snapshots[index]["state"].ToString(), 0, false);
        }
    }
}