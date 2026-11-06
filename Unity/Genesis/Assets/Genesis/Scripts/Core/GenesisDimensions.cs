using System;
using System.Collections.Generic;
using System.Linq;

namespace Genesis
{
    // Projection of the existing catalog, not a second save format or economy.
    public sealed class GenesisDimensionLayer
    {
        public string Name { get; }
        public IReadOnlyList<GeneratorDefinition> Generators { get; }
        public IReadOnlyList<ChapterDefinition> Chapters { get; }

        public GenesisDimensionLayer(string name, List<GeneratorDefinition> generators, List<ChapterDefinition> chapters)
        {
            Name = name;
            Generators = generators.AsReadOnly();
            Chapters = chapters.AsReadOnly();
        }

        public bool IsFormed(GenesisGame game) { return Generators.Any(g => game.Counts[g.id] > 0); }
        public int Remembered(GenesisGame game) { return Chapters.Count(c => game.CompletedChapters.Contains(c.id)); }
        public double Income(GenesisGame game)
        {
            return Generators.Sum(g => EconomyMath.Production(g, game.Counts[g.id], game.GlobalMultiplier));
        }
    }

    public static class GenesisDimensions
    {
        public static IReadOnlyList<GenesisDimensionLayer> Build(GenesisCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            // Optional recruits belong to their anchor's world, not a new final world.
            var source = new GenesisCatalog { generators = catalog.generators.Where(g => string.IsNullOrEmpty(g.recruit_after)).ToList(),
                chapters = catalog.chapters, dimensions = catalog.dimensions };
            var layers = BuildProgression(source);
            return layers.Select(layer => new GenesisDimensionLayer(layer.Name,
                layer.Generators.Concat(catalog.generators.Where(g => layer.Generators.Any(a => a.id == g.recruit_after))).ToList(),
                layer.Chapters.ToList())).ToList().AsReadOnly();
        }

        private static IReadOnlyList<GenesisDimensionLayer> BuildProgression(GenesisCatalog catalog)
        {
            var milestones = (catalog.dimensions ?? new List<DimensionDefinition>())
                .OrderBy(d => catalog.generators.FindIndex(g => g.id == d.generator_id)).ToList();
            var result = new List<GenesisDimensionLayer>();
            int start = 0;
            // Yin/Yang's synthesis closes Duality; the preceding roots form the opening layer.
            int foundationEnd = catalog.generators.FindIndex(g => g.id == "yin");
            if (foundationEnd > 0)
            {
                AddLayer(result, catalog, "Primordial Possibility", 0, foundationEnd - 1);
                start = foundationEnd;
            }
            foreach (DimensionDefinition milestone in milestones)
            {
                int end = catalog.generators.FindIndex(g => g.id == milestone.generator_id);
                if (end < start) throw new FormatException("Invalid or overlapping dimension milestone: " + milestone.name);
                AddLayer(result, catalog, milestone.name, start, end);
                start = end + 1;
            }
            if (start < catalog.generators.Count)
                AddLayer(result, catalog, "Unfolding Reality", start, catalog.generators.Count - 1);
            if (result.Sum(d => d.Chapters.Count) != catalog.chapters.Count)
                throw new FormatException("Every chapter must belong to exactly one dimension.");
            return result.AsReadOnly();
        }

        private static void AddLayer(List<GenesisDimensionLayer> layers, GenesisCatalog catalog, string name, int start, int end)
        {
            var generators = catalog.generators.GetRange(start, end - start + 1);
            var ids = new HashSet<string>(generators.Select(g => g.id));
            var chapters = catalog.chapters.Where(c => ids.Contains(c.kind) || (start == 0 && c.kind == "quanta")).ToList();
            layers.Add(new GenesisDimensionLayer(name, generators, chapters));
        }
    }
}