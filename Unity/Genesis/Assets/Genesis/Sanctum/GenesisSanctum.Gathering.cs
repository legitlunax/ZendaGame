using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private int gatheringSection;
        private int gatheringWorld = -1;
        private const float FairyVillageHeight = 132;
        public int GatheringViewedWorldIndex => gatheringWorld < 0 ? GatheringDimensionIndex : gatheringWorld;
        public GenesisDimensionLayer GatheringViewedWorld => Dimensions[GatheringViewedWorldIndex];
        public bool BrowseGatheringWorld(int index)
        {
            if (index < 0 || index >= Dimensions.Count) return false;
            gatheringWorld = index;
            SelectedDimension = index;
            SelectGatheringSection(0);
            return true;
        }
        private bool gatheringGuide;
        private readonly Dictionary<string, Texture2D> gatheringIcons = new Dictionary<string, Texture2D>();
        private GUIStyle gatheringBarLabel;
        private GUIStyle gatheringDetailLabel;
        public const float GatheringRowHeight = 112;
        private static readonly Color GatheringSage = new Color(0.58f, 0.72f, 0.60f);
        private static readonly Color GatheringRose = new Color(0.75f, 0.56f, 0.65f);

        public bool BeginGathering() { return Gathering.BeginStarter(); }
        public int GatheringDimensionIndex
        {
            get
            {
                for (int i = Dimensions.Count - 1; i > 0; i--)
                    if (Dimensions[i].IsFormed(Game)) return i;
                return 0;
            }
        }
        public GenesisDimensionLayer GatheringDimension => Dimensions[GatheringDimensionIndex];
        public IEnumerable<GeneratorDefinition> GatheringFairies
        {
            get
            {
                foreach (GeneratorDefinition generator in GatheringViewedWorld.Generators)
                    if (Game.IsDiscovered(generator.id)) yield return generator;
                int next = GatheringViewedWorldIndex + 1;
                if (next < Dimensions.Count && Game.IsDiscovered(Dimensions[next].Generators[0].id))
                    yield return Dimensions[next].Generators[0];
            }
        }
        public int VisibleGatheringRows => GatheringFairies.Count();
        private GeneratorDefinition NextGatheringFairy => GatheringViewedWorld.Generators.FirstOrDefault(g => !Game.IsDiscovered(g.id));
        public ChapterDefinition GatheringWorldUnreadStory => GatheringViewedWorld.Chapters.FirstOrDefault(c => !HasReadChronicle(c.id));

        public bool ReadGatheringWorldStory()
        {
            ChapterDefinition chapter = GatheringWorldUnreadStory;
            if (chapter == null) return Game.CampaignCompleted && OpenCompletionCredits();
            if (!IsChronicleUnlocked(chapter.id)) return GoToStoryObjective(chapter.id);
            int world = GatheringViewedWorldIndex;
            if (!OpenChronicle(chapter.id)) return false;
            readingWorldIndex = world;
            return true;
        }

        private void FocusGatheringFairy(string generatorId)
        {
            int row = GatheringFairies.TakeWhile(g => g.id != generatorId).Count();
            chronicleScroll = new Vector2(0, (gatheringGuide ? 142 : 0) + FairyVillageHeight + row * GatheringRowHeight);
        }
        public static Rect GatheringViewport(float height) => new Rect(12, 206, 366, Mathf.Max(1, height - 282));
        public static Rect GatheringWorldNameRect => new Rect(64, 80, 166, 34);
        public static Rect GatheringStarterRect => new Rect(278, 82, 92, 48);

        private bool GatheringActionButton(Rect rect, string caption, string amount = "", bool enabled = true)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && enabled && !dragging;
            DrawEnchantedButton(rect, true);
            // Artwork owns every visual state. An empty hit style cannot cover it
            // with a default/disabled GUI skin texture after the frame is drawn.
            var hitStyle = new GUIStyle();
            bool clicked = SoundButton(rect, "", hitStyle);
            var textStyle = new GUIStyle(LocalizedStyle(mobileButton))
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                fixedHeight = 0,
                fixedWidth = 0
            };
            ClearButtonBackgrounds(textStyle);
            const float horizontalInset = 16;
            const float verticalInset = 4;
            float lineHeight = string.IsNullOrEmpty(amount) ? rect.height - verticalInset * 2 : (rect.height - verticalInset * 2) / 2;
            // Spanish's full unlock translation exceeds the illustrated compact face.
            string actionCaption = caption == "Unlock" && Language == "es" ? "Abrir" : L(caption);
            string[] lines = string.IsNullOrEmpty(amount) ? new[] { actionCaption } : new[] { actionCaption, amount };
            for (int i = 0; i < lines.Length; i++)
            {
                textStyle.fontSize = 15;
                var content = new GUIContent(lines[i]);
                while (textStyle.fontSize > 10 && (textStyle.CalcSize(content).x > rect.width - horizontalInset * 2 || textStyle.CalcSize(content).y > lineHeight))
                    textStyle.fontSize--;
                GUI.Label(new Rect(rect.x + horizontalInset, rect.y + verticalInset + i * lineHeight, rect.width - horizontalInset * 2, lineHeight), content, textStyle);
            }
            GUI.enabled = previous;
            return clicked;
        }

        private Texture2D GatheringIcon(string id)
        {
            if (!gatheringIcons.TryGetValue(id, out Texture2D icon))
            {
                icon = Resources.Load<Texture2D>("Genesis/Art/icons/" + id);
                gatheringIcons[id] = icon;
            }
            return icon;
        }

        private void GatheringArtwork(Rect rect, string id)
        {
            Texture2D icon = GatheringIcon(id);
            if (icon != null) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
        }

        private void DrawGatheringFairy(Rect rect, GeneratorDefinition fairy)
        {
            // Bind the portrait to the same catalog entry as the displayed name.
            // Never substitute a navigation icon or another fairy when art is missing.
            Texture2D icon = GatheringIcon(fairy.id);
            if (icon == null) return;
            float size = Mathf.Min(rect.width, rect.height);
            Rect frame = new Rect(rect.center.x - size / 2, rect.center.y - size / 2, size, size);
            DrawEnchantedSocket(frame, false);
            float inset = size * 0.14f;
            GUI.DrawTexture(new Rect(frame.x + inset, frame.y + inset, size - inset * 2, size - inset * 2),
                icon, ScaleMode.ScaleToFit);
        }

        private void GatheringFill(Rect rect, float progress, Color color)
        {
            DrawKitProgress(rect, progress);
        }

        private void GatheringBar(Rect rect, float progress, string caption)
        {
            GatheringFill(rect, progress, GatheringSage);
            if (gatheringBarLabel == null) gatheringBarLabel = new GUIStyle(mobileSmall) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            gatheringBarLabel.alignment = TextAnchor.MiddleCenter;
            gatheringBarLabel.normal.textColor = WarmInk;
            LocalizedLabel(rect, caption, gatheringBarLabel);
        }

        public void SelectGatheringSection(int index)
        {
            gatheringSection = Mathf.Clamp(index, 0, 2);
            chronicleScroll = Vector2.zero;
            dragging = scrollPointerDown = false;
        }

        private void DrawIdleRpgMobile(float height)
        {
            EnsureIconStyles();
            // Compact labels need their full font line height, without inherited skin padding.
            if (gatheringDetailLabel == null)
                gatheringDetailLabel = new GUIStyle(mobileSmall);
            gatheringDetailLabel.padding = new RectOffset(0, 0, 0, 0);
            gatheringDetailLabel.wordWrap = false;
            gatheringDetailLabel.alignment = TextAnchor.MiddleLeft;
            gatheringDetailLabel.normal.textColor = WarmMuted;
            DrawEnchantedPanel(new Rect(12, 76, 366, 64));
            GenesisDimensionLayer activeWorld = GatheringViewedWorld;
            int ownedFairies = activeWorld.Generators.Count(g => Game.Counts[g.id] > 0);
            if (MobileButton(new Rect(18, 82, 44, 44), "")) BrowseGatheringWorld(Mathf.Max(0, GatheringViewedWorldIndex - 1));
            DrawKitIcon(new Rect(30, 94, 20, 20), gameTheme.back);
            var worldStyle = new GUIStyle(mobileSmall) { fontStyle = FontStyle.Normal };
            IdleHudText(GatheringWorldNameRect, L(activeWorld.Name), worldStyle, 13, WarmInk, TextAnchor.MiddleLeft);
            if (MobileButton(new Rect(236, 82, 40, 44), "")) BrowseGatheringWorld(Mathf.Min(Dimensions.Count - 1, GatheringViewedWorldIndex + 1));
            DrawKitIcon(new Rect(246, 94, 20, 20), gameTheme.next);
            LocalizedLabel(new Rect(64, 114, 166, 22), ownedFairies + " / " + activeWorld.Generators.Count + " fairies", gatheringDetailLabel);
            GatheringFill(new Rect(64, 136, 164, 3), (float)ownedFairies / activeWorld.Generators.Count, GatheringSage);
            if (GatheringActionButton(GatheringStarterRect, Gathering.StarterRunning
                ? Mathf.CeilToInt((float)Gathering.StarterRemaining) + "s" : "Gather",
                Gathering.StarterRunning ? "" : "+" + Num(Gathering.StarterRewardAmount), !Gathering.StarterRunning)) BeginGathering();
            if (Gathering.StarterRunning) GatheringFill(new Rect(286, 124, 76, 3), (float)(1 - Gathering.StarterRemaining / GenesisGathering.StarterDuration), GatheringRose);

            string[] sectionNames = { "Gather", "Journey", "Worlds" };
            string[] sectionIcons = { "hope_fluctuation", "stellar_nursery", "reality" };
            for (int i = 0; i < sectionNames.Length; i++)
            {
                Rect tab = new Rect(12 + i * 124, 148, 118, 48);
                if (MobileButton(tab, "")) SelectGatheringSection(i);
                DrawGatheringFairy(new Rect(tab.x + 5, tab.y + 8, 32, 32),
                    Game.Catalog.generators.First(g => g.id == sectionIcons[i]));
                IdleHudText(new Rect(tab.x + 39, tab.y + 6, 72, 34), L(sectionNames[i]),
                    mobileButton, 15, WarmInk, TextAnchor.MiddleCenter);
                if (gatheringSection == i) GatheringFill(new Rect(tab.x + 12, tab.yMax - 4, 94, 3), 1, GatheringRose);
                if (i == 1 && ExpeditionReady) DrawKitIcon(new Rect(tab.xMax - 16, tab.y + 6, 10, 10), gameTheme.check);
            }

            Rect viewport = GatheringViewport(height);
            float guideHeight = gatheringGuide ? 142 : 0;
            float contentHeight = guideHeight + (gatheringSection == 0 ? FairyVillageHeight + VisibleGatheringRows * GatheringRowHeight + 72
                : gatheringSection == 1 ? JourneyContentHeight : Dimensions.Count * 72);
            contentHeight = Mathf.Max(viewport.height, contentHeight);
            Event input = Event.current;
            if (input.type == EventType.MouseDown && viewport.Contains(input.mousePosition))
            {
                dragStart = input.mousePosition;
                dragScrollStart = chronicleScroll.y;
                scrollPointerDown = true;
            }
            if (input.type == EventType.MouseDrag && scrollPointerDown && (input.mousePosition - dragStart).magnitude > 8)
            {
                dragging = true;
                chronicleScroll.y = Mathf.Clamp(dragScrollStart + dragStart.y - input.mousePosition.y, 0, Mathf.Max(0, contentHeight - viewport.height));
                input.Use();
            }
            bool release = input.type == EventType.MouseUp;
            chronicleScroll = GUI.BeginScrollView(viewport, chronicleScroll, new Rect(0, 0, 344, contentHeight), false, false, GUIStyle.none, GUIStyle.none);
            if (gatheringGuide)
            {
                DrawEnchantedPanel(new Rect(0, 0, 340, 132));
                LocalizedLabel(new Rect(12, 8, 316, 118), gatheringSection == 0
                    ? "All owned fairies earn even while closed.\nOffline time earns at the full saved speed.\nProgress saves locally; no login needed.\nGreen: gathering. Rose: next x2 milestone.\nRead this world's story before the next world."
                    : gatheringSection == 1
                        ? "Journeys are optional trips for bonus Hope.\n1. Choose an available route.\n2. Start the trip; keep playing or close the game.\n3. Return here to claim its reward.\nNo Hope is spent and no fairy leaves your roster."
                        : "Choose a world as your journey origin.\nGreen bar: finished stories, gifts optional.\nA lock means its first creation is not owned.\nAll formed worlds keep producing together.", mobileSmall);
            }
            float y = guideHeight;
            if (gatheringSection == 0)
            {
                DrawFairyVillage(y);
                y += FairyVillageHeight;
                // Snapshot before drawing: a successful unlock changes the active world.
                foreach (GeneratorDefinition generator in GatheringFairies.ToList())
                {
                    DrawGatheringRow(generator, y);
                    y += GatheringRowHeight;
                }
                GeneratorDefinition next = NextGatheringFairy;
                DrawEnchantedPanel(new Rect(0, y, 340, 64));
                DrawGatheringLock(new Rect(18, y + 18, 22, 26), next == null);
                bool finalWorld = GatheringViewedWorldIndex == Dimensions.Count - 1;
                LocalizedLabel(new Rect(56, y + 10, finalWorld ? 170 : 272, 44), next == null ? "This world's fairies revealed" : GenesisStorytelling.ResidentFriends(next), mobileText);
                if (finalWorld && MobileButton(new Rect(236, y + 9, 96, 46),
                    Game.CampaignCompleted ? "A universe remembered" : "Read",
                    enabled: GatheringWorldUnreadStory != null || Game.CampaignCompleted)) ReadGatheringWorldStory();
            }
            else if (gatheringSection == 1) DrawGatheringJourney(y);
            else DrawGatheringWorlds(y);
            GUI.EndScrollView();
            if (contentHeight > viewport.height)
            {
                float thumbHeight = Mathf.Max(24, viewport.height * viewport.height / contentHeight);
                float offset = Mathf.Clamp01(chronicleScroll.y / (contentHeight - viewport.height)) * (viewport.height - thumbHeight);
                // The kit pill needs a body between its rounded caps, otherwise the
                // nine slice collapses and the thumb reads as two loose hairlines.
                GatheringFill(new Rect(viewport.xMax - 10, viewport.y + offset, 8, thumbHeight), 1, GatheringRose);
            }
            if (release) { dragging = false; scrollPointerDown = false; }
        }

        private void DrawGatheringRow(GeneratorDefinition generator, float y)
        {
            long level = Game.Counts[generator.id];
            double duration = Gathering.Duration(generator.id);
            double rate = EconomyMath.Production(generator, level, Game.GlobalMultiplier);
            DrawEnchantedPanel(new Rect(0, y, 340, 104));
            DrawGatheringFairy(new Rect(8, y + 10, 72, 78), generator);
            LocalizedLabel(new Rect(88, y + 6, 244, 24), new GUIContent(GenesisStorytelling.ResidentFriends(generator), generator.name), mobileText);
            LocalizedLabel(new Rect(88, y + 30, 244, 24), generator.name, gatheringDetailLabel);
            LocalizedLabel(new Rect(88, y + 54, 144, 22), "Lv " + level + " / +" + Num(rate) + "/s", gatheringDetailLabel);
            GatheringBar(new Rect(88, y + 76, 140, 16), level > 0 ? (float)Gathering.Progress(generator.id) : 0,
                level > 0 ? (duration * (1 - Gathering.Progress(generator.id))).ToString("F1") + "s" : "");
            GatheringFill(new Rect(88, y + 98, 244, 3), (level % 25) / 25f, GatheringRose);
            LocalizedLabel(new Rect(88, y + 94, 244, 9), new GUIContent("", "x2 at Lv " + ((level / 25 + 1) * 25) + " / gathered " + Num(Gathering.Gathered(generator.id))), mobileSmall);
            BulkPurchase purchase = Game.PurchaseInfo(generator.id);
            bool needsStory = Game.RequiredWorldStory(generator.id) != null;
            if (GatheringActionButton(new Rect(240, y + 48, 92, 46), needsStory ? "Read"
                : level == 0 ? "Unlock" : "+" + purchase.Amount,
                needsStory ? "" : Num(purchase.Cost), needsStory || purchase.CanAfford)) Awaken(generator.id);
        }

        public IEnumerable<GeneratorDefinition> VisibleVillageFairies => GatheringViewedWorld.Generators
            .Where(g => Game.Counts[g.id] > 0).Take(12);

        private void DrawFairyVillage(float y)
        {
            DrawEnchantedPanel(new Rect(0, y, 340, FairyVillageHeight - 8));
            LocalizedLabel(new Rect(12, y + 4, 316, 24), "Fairy neighbourhood / tap a friend", mobileSmall);
            int count = 0;
            foreach (GeneratorDefinition fairy in VisibleVillageFairies)
            {
                int i = count++;
                float sway = Mathf.Sin(Time.unscaledTime * 1.7f + i * 1.9f) * 2;
                float bob = Mathf.Sin(Time.unscaledTime * 2.1f + i * 1.3f) * 3;
                Rect tile = new Rect(9 + i % 6 * 54 + sway, y + 28 + i / 6 * 44 + bob, 48, 44);
                DrawGatheringFairy(tile, fairy);
                if (SoundButton(tile, new GUIContent("", fairy.fairy + " / " + fairy.name), invisibleButton)
                    && !dragging)
                {
                    SelectLady(fairy.fairy);
                    break;
                }
            }
            if (count == 0)
                LocalizedLabel(new Rect(18, y + 38, 304, 64), "Recruit this world's fairies below.\nThey will gather here as neighbours.", mobileText);
        }

        private void DrawGatheringLock(Rect rect, bool unlocked)
        {
            EnsureEnchantedGameTheme();
            if (gameTheme != null) DrawKitIcon(rect, unlocked ? gameTheme.check : gameTheme.diamond);
        }

        private void DrawGatheringJourney(float y)
        {
            DrawJourneyDashboard(y);
        }

        private void DrawGatheringWorlds(float y)
        {
            for (int i = 0; i < Dimensions.Count; i++)
            {
                var layer = Dimensions[i];
                bool formed = layer.IsFormed(Game);
                Rect row = new Rect(0, y + i * 72, 340, 64);
                if (MobileButton(row, "")) { BrowseGatheringWorld(i); break; }
                DrawGatheringLock(new Rect(12, row.y + 16, 24, 28), formed);
                LocalizedLabel(new Rect(48, row.y + 6, 276, 24), layer.Name, mobileText);
                GatheringFill(new Rect(48, row.y + 38, 174, 6), layer.Chapters.Count > 0 ? (float)StoryReadCount(layer) / layer.Chapters.Count : 0, GatheringSage);
                LocalizedLabel(new Rect(232, row.y + 30, 96, 24), Num(layer.Income(Game)) + "/s", mobileSmall);
                if (SelectedDimension == i) GatheringFill(new Rect(4, row.y + 8, 3, 48), 1, GatheringRose);
            }
        }
    }
}
