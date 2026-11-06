using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Genesis
{
    // Field names deliberately match the exported Godot JSON and stable save IDs.
    [Serializable]
    public sealed class GeneratorDefinition
    {
        public string id, name, fairy, era, desc, dimension, motif, scale;
        // Optional residents use a stable home anchor, never the linear world-unlock chain.
        public string recruit_after;
        public List<ResidentFriendDefinition> resident_friends;
        public double base_cost, base_prod, cost_mult;
        public CatalogColor color;
    }

    [Serializable]
    public sealed class ResidentFriendDefinition
    {
        public string name, home, title;
    }

    [Serializable]
    public sealed class ChapterDefinition
    {
        public string id, title, goal, kind, lore, dimension;
        public double target, reward;
    }

    [Serializable]
    public sealed class CatalogColor { public float r, g, b, a; }
    [Serializable]
    public sealed class GoddessForm
    {
        public int stage, wings;
        public string title, lore;
        public double req_quanta, mult_bonus;
        public CatalogColor aura_color;
    }
    [Serializable]
    public sealed class RelicDefinition
    {
        public string id, name, desc;
        public long base_seed_cost, max_lvl = 1000000;
        public double effect_per_lvl;
    }
    [Serializable]
    public sealed class MiracleDefinition
    {
        public string id, name, desc;
        public double cooldown, duration;
    }
    [Serializable]
    public sealed class WardrobeDefinition
    {
        public string id, name, chapter, requirement, accent, outfit, wings;
        public double scale;
    }
    [Serializable]
    public sealed class EmotionDefinition
    {
        public string id, chapter_id;
        public bool always_available;
    }
    [Serializable]
    public sealed class DimensionDefinition
    {
        public string generator_id, chapter_id, name;
        public int order;
    }
    [Serializable]
    public sealed class DemoPreset
    {
        public string name, through;
        public bool fund_next;
    }

    [Serializable]
    public sealed class GenesisCatalog
    {
        public int schema_version;
        public List<GeneratorDefinition> generators;
        public List<ChapterDefinition> chapters;
        public List<GoddessForm> goddess_forms;
        public List<RelicDefinition> relics;
        public List<MiracleDefinition> miracles;
        public List<WardrobeDefinition> wardrobe;
        public List<EmotionDefinition> emotions;
        public List<DimensionDefinition> dimensions;
        public List<DemoPreset> demo_presets;
        public List<string> number_suffixes;
        // GenesisCatalog is a plain POCO built with JsonConvert.DeserializeObject and is
        // never stored by Unity: its only owner field (GenesisDemo.catalog) is private
        // and unserialized. The UAC1015 serialization analyzer therefore does not apply
        // here. [NonSerialized] is not an option because Json.NET would then skip the
        // field and the catalog would lose its display names on load.
#pragma warning disable UAC1015
        public Dictionary<string, string> fairy_names;
        // Display-name migrations only; progression continues to use generator IDs.
        public Dictionary<string, string> fairy_name_aliases;
#pragma warning restore UAC1015

        public string ResolveFairyName(string name)
        {
            if (name != null && fairy_name_aliases != null && fairy_name_aliases.TryGetValue(name, out string current))
                return current;
            return name;
        }

        public static GenesisCatalog FromJson(string json)
        {
            GenesisCatalog result = JsonConvert.DeserializeObject<GenesisCatalog>(json);
            if (result == null || result.schema_version != 1 || result.generators == null ||
                result.chapters == null || result.goddess_forms == null || result.goddess_forms.Count == 0 ||
                result.relics == null || result.miracles == null || result.wardrobe == null ||
                result.emotions == null || result.number_suffixes == null)
                throw new FormatException("Incomplete or unsupported Genesis catalog.");
            HashSet<string> ids = new HashSet<string>();
            foreach (GeneratorDefinition generator in result.generators)
            {
                if (string.IsNullOrEmpty(generator.id) || !ids.Add(generator.id) ||
                    !EconomyMath.IsFinite(generator.base_cost) || generator.base_cost <= 0 ||
                    !EconomyMath.IsFinite(generator.base_prod) || generator.base_prod < 0 ||
                    !EconomyMath.IsFinite(generator.cost_mult) || generator.cost_mult <= 1)
                    throw new FormatException("Invalid generator: " + generator.id);
            }
            foreach (GeneratorDefinition generator in result.generators)
                if (!string.IsNullOrEmpty(generator.recruit_after)
                    && !result.generators.Exists(g => g.id == generator.recruit_after && string.IsNullOrEmpty(g.recruit_after)))
                    throw new FormatException("Invalid resident home anchor: " + generator.id);
            ids.Clear();
            foreach (ChapterDefinition chapter in result.chapters)
                if (string.IsNullOrEmpty(chapter.id) || !ids.Add(chapter.id))
                    throw new FormatException("Duplicate/invalid chapter ID.");
            return result;
        }
    }
}
