using System;
using System.Collections.Generic;
using System.Linq;

namespace Genesis
{
    public sealed class GenesisLadyGalleryEntry
    {
        public string GeneratorId { get; }
        public string ChapterId { get; }
        public string Name { get; }
        public string Home { get; }
        public string Title { get; }
        public string IconId { get; }
        public string History { get; }

        public GenesisLadyGalleryEntry(GeneratorDefinition generator, ResidentFriendDefinition friend, string chapterId, string lore = null)
        {
            GeneratorId = generator.id;
            ChapterId = chapterId;
            Name = friend.name;
            Home = friend.home;
            Title = friend.title;
            // Reuse the existing chibi art, never the main goddess portraits.
            IconId = friend.home == "Death Dimension" ? "shadow_realm" : generator.id;
            History = friend.home == "Death Dimension"
                ? "Zenda: I met " + friend.name + " beyond the Soul Dimension. Together we built a quiet garden of farewells in the Death Dimension. She tends changes with compassion, but cannot erase a living being. In our imagined creation, Life keeps everyone safely themselves, beyond death and destruction."
                : "Zenda: I met " + friend.name + " in " + friend.home + ". We worked side by side to help construct her home, and our friendship grew as we learned from one another.\n\n" + friend.name + ": This is how I remember our adventure, Zenda.\n\n" + (lore ?? generator.desc);
            History += "\n\n" + friend.name + ": " + GenesisStorytelling.LadyRecollection(generator, friend);
            string[] challenge = GenesisStoryChallenges.Build(generator);
            History += "\n\nZenda: " + challenge[0] + "\n\n" + friend.name + ": " + challenge[1] + "\n\nZenda: " + challenge[2];
            string continuityStory = GenesisStorytelling.LifeContinuityStory(generator.id);
            if (!string.IsNullOrEmpty(continuityStory)) History += "\n\nZenda: " + continuityStory;
            History += "\n\n" + friend.name + ": I am glad we can tell it together, Zenda. When you visit " + friend.home + ", we can make more memories.\n\nZenda: When she accompanies me, she travels as my friend; her home remains hers.";
        }
    }

    public static class GenesisLadyGallery
    {
        public static IReadOnlyList<GenesisLadyGalleryEntry> Build(GenesisGame game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            var entries = new List<GenesisLadyGalleryEntry>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (GeneratorDefinition generator in game.Catalog.generators)
            {
                ChapterDefinition chapter = game.Catalog.chapters.FirstOrDefault(c => c.kind == (generator.recruit_after ?? generator.id));
                if (game.Counts[generator.id] <= 0 && (!string.IsNullOrEmpty(generator.recruit_after)
                    || chapter == null || !game.CompletedChapters.Contains(chapter.id))) continue;
                if (generator.resident_friends == null) continue;
                foreach (ResidentFriendDefinition friend in generator.resident_friends)
                    if (!string.IsNullOrWhiteSpace(friend.name) && names.Add(friend.name))
                        entries.Add(new GenesisLadyGalleryEntry(generator, friend, chapter == null ? null : chapter.id,
                            !string.IsNullOrEmpty(generator.recruit_after) || chapter == null ? generator.desc : chapter.lore));
            }
            return entries.AsReadOnly();
        }
    }
}