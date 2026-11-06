using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private readonly HashSet<string> unlockedChronicles = new HashSet<string>();
        public string OpenChronicleId { get; private set; }
        public int ChroniclePage { get; private set; }
        private bool storyOpenedFromGallery;
        private int readingWorldIndex = -1;
        public bool ReadingWorldStories => readingWorldIndex >= 0;
        private ChapterDefinition NextReadingWorldStory => ReadingWorldStories
            ? Dimensions[readingWorldIndex].Chapters.FirstOrDefault(c => !HasReadChronicle(c.id)) : null;

        public bool ContinueReadingWorldStories()
        {
            if (!ReadingWorldStories) return false;
            ChapterDefinition next = NextReadingWorldStory;
            if (next != null)
            {
                if (!IsChronicleUnlocked(next.id)) return GoToStoryObjective(next.id);
                int world = readingWorldIndex;
                if (!OpenChronicle(next.id)) return false;
                readingWorldIndex = world;
                return true;
            }
            CloseChronicle();
            SelectTab(1);
            return true;
        }
        private List<GenesisStoryPage> storyPages = new List<GenesisStoryPage>();
        private ChapterDefinition ReadingChapter => Game.Catalog.chapters.FirstOrDefault(c => c.id == OpenChronicleId);
        public int ChroniclePageCount => ReadingChapter == null ? 0 : storyPages.Count;
        public bool HasUnreadChronicles => Game.Catalog.chapters.Any(c => IsChronicleUnlocked(c.id) && !HasReadChronicle(c.id));
        public ChapterDefinition CurrentStorylineChapter => Game.Catalog.chapters.FirstOrDefault(c => !HasReadChronicle(c.id));

        public bool IsChronicleUnlocked(string id)
        {
            ChapterDefinition chapter = Game.Catalog.chapters.FirstOrDefault(c => c.id == id);
            if (chapter == null) return false;
            if (Game.CompletedChapters.Contains(id) || Game.ChapterProgress(chapter) >= chapter.target) unlockedChronicles.Add(id);
            return unlockedChronicles.Contains(id);
        }

        public bool HasReadChronicle(string id) { return Game.HasReadStory(id); }

        private void RefreshChronicleUnlocks()
        {
            foreach (ChapterDefinition chapter in Game.Catalog.chapters)
                if (Game.CompletedChapters.Contains(chapter.id) || Game.ChapterProgress(chapter) >= chapter.target) unlockedChronicles.Add(chapter.id);
        }

        public bool OpenChronicle(string id)
        {
            if (!IsChronicleUnlocked(id)) return false;
            readingWorldIndex = -1;
            storyOpenedFromGallery = LadyGalleryOpen;
            CloseLadyGallery();
            storyPages = GenesisStorytelling.Build(Game.Catalog, Game.Catalog.chapters.First(c => c.id == id), L);
            OpenChronicleId = id;
            ChroniclePage = 0;
            SelectTab(2);
            PresentStoryPage();
            return true;
        }

        public bool ContinueChronicle()
        {
            if (ReadingChapter == null || !IsChronicleUnlocked(OpenChronicleId)) return false;
            if (ChroniclePage + 1 < ChroniclePageCount) { ChroniclePage++; PresentStoryPage(); }
            else
            {
                bool finished = Game.FinishStory(OpenChronicleId);
                CheckCompletion();
                SavePlayerProgress();
                return finished;
            }
            return true;
        }

        public bool ClaimChronicleReward(string id)
        {
            if (!HasReadChronicle(id) || !IsChronicleUnlocked(id) || !Game.ClaimChapter(id)) return false;
            message = "Chronicle remembered. Its reward is yours.";
            React("relaxed");
            CheckCompletion();
            SavePlayerProgress();
            return true;
        }

        private void DrawChroniclesMobile(float height)
        {
            ChapterDefinition chapter = ReadingChapter;
            if (chapter != null)
            {
                DrawStorytellingReader(height, chapter);
                return;
            }
            if (MobileButton(new Rect(16, 78, 174, 44), LadyGalleryOpen ? "Storyline" : "> Storyline")) CloseLadyGallery();
            if (MobileButton(new Rect(198, 78, 176, 44), LadyGalleryOpen ? "> Lady gallery" : "Lady gallery")) OpenLadyGallery();
            if (LadyGalleryOpen) { DrawLadyGallery(height); return; }
            DrawStoryQuestJournal(height);
        }
    }
}