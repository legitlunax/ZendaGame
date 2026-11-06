using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public int StoryWorldIndex { get; private set; }
        private Vector2 storyJournalScroll;
        public GenesisDimensionLayer StoryWorld => Dimensions[Mathf.Clamp(StoryWorldIndex, 0, Dimensions.Count - 1)];
        public int StoryReadCount(GenesisDimensionLayer world) => world.Chapters.Count(c => HasReadChronicle(c.id));
        public int StoryWorldReadCount => StoryReadCount(StoryWorld);
        public ChapterDefinition StoryQuest => HasPendingWorldUnlock ? Game.RequiredWorldStory(pendingWorldGenerator)
            : StoryWorld.Chapters.FirstOrDefault(c => !HasReadChronicle(c.id));

        public void FollowGatheringStoryWorld()
        {
            StoryWorldIndex = GatheringDimensionIndex;
            if (HasPendingWorldUnlock)
            {
                ChapterDefinition required = Game.RequiredWorldStory(pendingWorldGenerator);
                if (required != null) StoryWorldIndex = Dimensions.ToList().FindIndex(d => d.Chapters.Any(c => c.id == required.id));
            }
            storyJournalScroll = Vector2.zero;
        }

        public bool SelectStoryWorld(int index)
        {
            if (index < 0 || index >= Dimensions.Count) return false;
            StoryWorldIndex = index;
            storyJournalScroll = Vector2.zero;
            dragging = scrollPointerDown = false;
            return true;
        }

        public bool GoToStoryObjective(string chapterId)
        {
            ChapterDefinition chapter = Game.Catalog.chapters.FirstOrDefault(c => c.id == chapterId);
            if (chapter == null) return false;
            // Preserve the pending unlock's precise return target. Never buy on navigation.
            if (HasPendingWorldUnlock) return ReturnToWorldUnlock();
            CloseChronicle();
            CloseLadyGallery();
            SelectTab(1);
            SelectGatheringSection(0);
            gatheringWorld = Dimensions.ToList().FindIndex(w => w.Chapters.Any(c => c.id == chapterId));
            if (GatheringFairies.Any(g => g.id == chapter.kind)) FocusGatheringFairy(chapter.kind);
            message = chapter.goal;
            return true;
        }

        private void DrawStoryQuestJournal(float height)
        {
            Rect viewport = new Rect(12, 130, 366, Mathf.Max(1, height - 206));
            float headerHeight = 196;
            const float rowHeight = 240;
            float contentHeight = Mathf.Max(viewport.height, headerHeight + StoryWorld.Chapters.Count * rowHeight + 68);
            Event input = Event.current;
            if (input.type == EventType.MouseDown && viewport.Contains(input.mousePosition))
            {
                dragStart = input.mousePosition;
                dragScrollStart = storyJournalScroll.y;
                scrollPointerDown = true;
            }
            if (input.type == EventType.MouseDrag && scrollPointerDown && (input.mousePosition - dragStart).magnitude > 8)
            {
                dragging = true;
                storyJournalScroll.y = Mathf.Clamp(dragScrollStart + dragStart.y - input.mousePosition.y, 0, Mathf.Max(0, contentHeight - viewport.height));
                input.Use();
            }
            bool release = input.type == EventType.MouseUp;
            storyJournalScroll = GUI.BeginScrollView(viewport, storyJournalScroll, new Rect(0, 0, 344, contentHeight), false, false, GUIStyle.none, GUIStyle.none);
            DrawReadingSurface(new Rect(0, 0, 340, 188));
            LocalizedLabel(new Rect(12, 8, 316, 22), LF("WORLD QUESTS / {0} of {1}", StoryWorldIndex + 1, Dimensions.Count), mobileSmall);
            LocalizedLabel(new Rect(12, 34, 316, 48), StoryWorld.Name, mobileTitle);
            if (MobileButton(new Rect(8, 88, 44, 44), "<", StoryWorldIndex > 0)) SelectStoryWorld(StoryWorldIndex - 1);
            if (MobileButton(new Rect(60, 88, 220, 44), "Current RPG world")) FollowGatheringStoryWorld();
            if (MobileButton(new Rect(288, 88, 44, 44), ">", StoryWorldIndex + 1 < Dimensions.Count)) SelectStoryWorld(StoryWorldIndex + 1);
            int read = StoryWorldReadCount;
            GatheringFill(new Rect(12, 140, 316, 6), StoryWorld.Chapters.Count == 0 ? 1 : read / (float)StoryWorld.Chapters.Count, GatheringSage);
            LocalizedLabel(new Rect(12, 154, 316, 28), read + "/" + StoryWorld.Chapters.Count + " stories finished / gifts optional", mobileSmall);

            // Snapshot the selected world's quests so arrow navigation cannot mix worlds in one GUI pass.
            var chapters = StoryWorld.Chapters.ToList();
            ChapterDefinition quest = StoryQuest;
            for (int i = 0; i < chapters.Count; i++)
            {
                ChapterDefinition chapter = chapters[i];
                float y = headerHeight + i * rowHeight;
                bool finished = HasReadChronicle(chapter.id);
                bool unlocked = IsChronicleUnlocked(chapter.id);
                bool claimed = Game.CompletedChapters.Contains(chapter.id);
                DrawReadingSurface(new Rect(0, y, 340, rowHeight - 8));
                GatheringFill(new Rect(0, y, 4, rowHeight - 8), 1, finished ? GatheringSage : GatheringRose);
                GatheringArtwork(new Rect(12, y + 10, 38, 38), chapter.kind == "quanta" ? "hope_fluctuation" : chapter.kind);
                LocalizedLabel(new Rect(60, y + 8, 268, 24), "QUEST " + (i + 1) + " / " + (finished ? "FINISHED" : unlocked ? "READY TO READ" : "IN PROGRESS"), mobileSmall);
                LocalizedLabel(new Rect(12, y + 48, 316, 48), chapter.title, mobileText);
                LocalizedLabel(new Rect(12, y + 98, 316, 46), chapter.goal, mobileSmall);
                double progress = Game.ChapterProgress(chapter);
                float ratio = chapter.target <= 0 ? 1 : (float)System.Math.Min(1, progress / chapter.target);
                GatheringFill(new Rect(12, y + 148, 316, 5), finished || unlocked ? 1 : ratio, GatheringSage);
                LocalizedLabel(new Rect(12, y + 157, 316, 22), finished ? (claimed ? "Gift received" : LF("Optional gift: +{0}", Num(chapter.reward)))
                    : unlocked ? "Objective complete / finish the conversation" : LF("{0} / {1} / live RPG progress", Num(progress), Num(chapter.target)), mobileSmall);
                if (MobileButton(new Rect(12, y + 184, 316, 44), unlocked ? (finished ? "Revisit story" : quest == chapter ? "Continue main quest" : "Read story") : "Play objective in Idle RPG"))
                {
                    if (unlocked) OpenChronicle(chapter.id); else GoToStoryObjective(chapter.id);
                    break;
                }
            }
            float footer = headerHeight + chapters.Count * rowHeight;
            if (MobileButton(new Rect(8, footer + 8, 324, 48), HasPendingWorldUnlock ? "Return to world unlock" : "Back to Idle RPG"))
            {
                if (HasPendingWorldUnlock) ReturnToWorldUnlock(); else { SelectTab(1); SelectGatheringSection(0); }
            }
            GUI.EndScrollView();
            if (contentHeight > viewport.height)
            {
                float thumbHeight = Mathf.Max(24, viewport.height * viewport.height / contentHeight);
                float offset = Mathf.Clamp01(storyJournalScroll.y / (contentHeight - viewport.height)) * (viewport.height - thumbHeight);
                // The kit pill needs a body between its rounded caps, otherwise the
                // nine slice collapses and the thumb reads as two loose hairlines.
                GatheringFill(new Rect(viewport.xMax - 10, viewport.y + offset, 8, thumbHeight), 1, GatheringRose);
            }
            if (release) { dragging = false; scrollPointerDown = false; }
        }
    }
}
