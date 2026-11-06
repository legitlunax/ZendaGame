using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private Vector2 storyScroll;
        private float storySpeakingRemaining;
        private bool storyCameraActive;
        private Vector3 storyPreviousCameraPosition;
        private Quaternion storyPreviousCameraRotation;
        private float storyPreviousCameraSize, storyPreviousCameraAspect;
        private RenderTexture storyTexture;
        private Texture2D storyIllustration;
        private string storyIllustrationKey;

        public bool HasStoryIllustration => storyIllustration != null;
        public static string StoryIllustrationPath(string chapterId, int page)
        { return "Genesis/Art/stories/" + chapterId + "/page_" + (page + 1).ToString("D2"); }

        private void LoadStoryIllustration()
        {
            if (ReadingChapter == null) { storyIllustration = null; storyIllustrationKey = null; return; }
            string key = StoryIllustrationPath(ReadingChapter.id, ChroniclePage);
            if (storyIllustrationKey == key) return;
            storyIllustrationKey = key;
            storyIllustration = Resources.Load<Texture2D>(key);
            if (storyIllustration == null)
                storyIllustration = Resources.Load<Texture2D>("Genesis/Art/stories/" + ReadingChapter.id + "/scene");
        }

        private void LateUpdate()
        {
            UpdateEnchantedConversation();
            // An open preview scene can own screen rendering. Refresh the portrait before
            // rendering begins, never inside OnGUI (which would recursively enter URP).
            if (StorytellingActive && !HasStoryIllustration && storyTexture != null && stageCamera != null)
                stageCamera.Render();
        }
        public bool StorytellingActive => SelectedTab == 2 && ReadingChapter != null;
        public string StoryText => storyPages.Count == 0 ? "" : storyPages[ChroniclePage].Text;
        public string StoryMood => storyPages.Count == 0 ? "Neutral" : storyPages[ChroniclePage].Mood;
        public string StorySpeaker => storyPages.Count == 0 ? "Zenda" : storyPages[ChroniclePage].Speaker;
        private string StoryPageCaption => LF("{0} / {1} / {2} of {3}", StorySpeaker, L(StoryMood), ChroniclePage + 1, ChroniclePageCount);
        private GenesisStoryPage StoryGuestPage => storyPages.Take(ChroniclePage + 1).LastOrDefault(p => p.Speaker != "Zenda")
            ?? storyPages.FirstOrDefault(p => p.Speaker != "Zenda");
        public string StoryGuestName => StoryGuestPage == null ? "" : StoryGuestPage.Speaker;
        public string StoryGuestIconId => StoryGuestPage == null ? null : StoryGuestPage.SpeakerIconId;

        public void CloseChronicle()
        {
            readingWorldIndex = -1;
            OpenChronicleId = null;
            storyIllustration = null;
            storyIllustrationKey = null;
            storySpeakingRemaining = 0;
            UpdateStoryPresentation(0);
        }

        public void ReturnFromStory()
        {
            if (HasPendingWorldUnlock) { ReturnToWorldUnlock(); return; }
            bool returnToGallery = storyOpenedFromGallery;
            CloseChronicle();
            storyOpenedFromGallery = false;
            if (returnToGallery) OpenLadyGallery();
        }

        public bool PreviousStoryPage()
        {
            if (ReadingChapter == null || ChroniclePage <= 0) return false;
            ChroniclePage--;
            PresentStoryPage();
            return true;
        }

        private void PresentStoryPage()
        {
            storyScroll = Vector2.zero;
            LoadStoryIllustration();
            storySpeakingRemaining = Mathf.Clamp(StoryText.Length / 22f, 3, 12);
            UpdateStoryPresentation(0);
        }

        private void UpdateStoryPresentation(float seconds)
        {
            ApplyGoddessRotation();
            bool active = StorytellingActive;
            bool portraitActive = active && !HasStoryIllustration && !ProtagonistPickerOpen;
            if (stageCamera != null)
            {
                if (portraitActive && !storyCameraActive)
                {
                    storyPreviousCameraPosition = stageCamera.transform.position;
                    storyPreviousCameraRotation = stageCamera.transform.rotation;
                    storyPreviousCameraSize = stageCamera.orthographicSize;
                    storyPreviousCameraAspect = stageCamera.aspect;
                    storyCameraActive = true;
                }
                else if (!portraitActive && storyCameraActive)
                {
                    stageCamera.transform.SetPositionAndRotation(storyPreviousCameraPosition, storyPreviousCameraRotation);
                    stageCamera.orthographicSize = storyPreviousCameraSize;
                    stageCamera.aspect = storyPreviousCameraAspect;
                    stageCamera.targetTexture = stageTexture;
                    storyCameraActive = false;
                    stageAspect = 0;
                }
                stageCamera.enabled = ProtagonistPickerOpen || SelectedTab == 0 || portraitActive;
                if (portraitActive && goddess != null && goddess.IsReady)
                {
                    Rect safe = MobileSafeArea(Screen.width, Screen.height, Screen.safeArea);
                    float scale = Mathf.Min(safe.width / 390f, safe.height / 620f);
                    Rect portrait = StoryPortraitRect(safe.height / scale);
                    int textureHeight = Mathf.RoundToInt(600 * portrait.height / portrait.width);
                    if (storyTexture == null || storyTexture.height != textureHeight)
                    {
                        if (storyTexture != null) { storyTexture.Release(); Destroy(storyTexture); }
                        storyTexture = new RenderTexture(600, textureHeight, 24) { name = "Zenda storytelling portrait" };
                        storyTexture.Create();
                    }
                    stageCamera.targetTexture = storyTexture;
                    Transform head = goddess.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                    Vector3 focus = head != null ? head.position - Vector3.up * stageSize * 0.1f : stageCenter;
                    stageCamera.aspect = portrait.width / portrait.height;
                    stageCamera.orthographicSize = stageSize * 0.4f;
                    stageCamera.transform.position = focus + Vector3.forward * 4;
                    stageCamera.transform.LookAt(focus);
                    stageCamera.ResetProjectionMatrix();
                }
            }
            storySpeakingRemaining = Mathf.Max(0, storySpeakingRemaining - seconds);
            if (goddess == null || !goddess.IsReady) return;
            goddess.SetStorytelling(portraitActive, portraitActive && StorySpeaker == "Zenda" && storySpeakingRemaining > 0);
            if (active && !goddess.SetMood(StorySpeaker == "Zenda" ? StoryMood : "relaxed")) goddess.SetMood("Neutral");
            else if (!active && reactionRemaining == 0) goddess.SetMood("Neutral");
            ApplySocialPresentation();
        }

        public static Rect StoryPortraitRect(float height)
        { return new Rect(16, 176, 358, Mathf.Clamp(height * 0.25f, 140, 240)); }

        public static Rect StoryParticipantRect(float height, bool lady)
        {
            Rect portrait = StoryPortraitRect(height);
            return new Rect(portrait.x + (lady ? 183 : 0), portrait.y, 175, portrait.height);
        }

        private void DrawStoryParticipants(float height)
        {
            Rect zenda = StoryParticipantRect(height, false);
            Rect lady = StoryParticipantRect(height, true);
            DrawEnchantedPanel(zenda);
            DrawEnchantedPanel(lady);
            Rect zendaImage = new Rect(zenda.x + 4, zenda.y + 4, zenda.width - 8, zenda.height - 48);
            if (HasStoryIllustration)
                GUI.DrawTexture(zendaImage, storyIllustration, ScaleMode.ScaleToFit);
            else if (stageCamera != null && storyTexture != null && goddess != null && goddess.IsReady)
            {
                GUI.DrawTextureWithTexCoords(zendaImage, storyTexture, new Rect(0.25f, 0, 0.5f, 1));
            }
            else LocalizedLabel(zendaImage, "Zenda's portrait is unavailable. Her words are still here.", mobileSmall);

            string iconId = StoryGuestIconId;
            Texture2D icon = null;
            if (!string.IsNullOrEmpty(iconId) && !ladyGalleryIcons.TryGetValue(iconId, out icon))
            {
                icon = Resources.Load<Texture2D>("Genesis/Art/icons/" + iconId);
                ladyGalleryIcons[iconId] = icon;
            }
            Rect ladyImage = new Rect(lady.x + 4, lady.y + 4, lady.width - 8, lady.height - 48);
            if (icon != null) GUI.DrawTexture(ladyImage, icon, ScaleMode.ScaleToFit);
            else LocalizedLabel(ladyImage, "Our friend is here in words; her portrait is unavailable.", mobileSmall);
            LocalizedLabel(new Rect(zenda.x + 5, zenda.yMax - 42, zenda.width - 10, 40),
                "Zenda / " + L(StorySpeaker == "Zenda" ? "speaking" : "listening"), mobileSmall);
            LocalizedLabel(new Rect(lady.x + 5, lady.yMax - 42, lady.width - 10, 40),
                StoryGuestName + " / " + L(StorySpeaker == StoryGuestName ? "speaking" : "listening"), mobileSmall);
        }

        private void DrawStorytellingReader(float height, ChapterDefinition chapter)
        {
            if (MobileButton(new Rect(16, 78, 358, 44), HasPendingWorldUnlock ? "< Return to world unlock" : storyOpenedFromGallery ? "< Lady gallery" : "< Storyline")) { ReturnFromStory(); return; }
            DrawReadingSurface(new Rect(12, 124, 366, 50));
            LocalizedLabel(new Rect(20, 126, 350, 46), chapter.title, mobileText);
            Rect portrait = StoryPortraitRect(height);
            DrawStoryParticipants(height);

            float top = portrait.yMax + 8;
            DrawReadingSurface(new Rect(12, top, 366, height - 154 - top));
            LocalizedLabel(new Rect(24, top + 4, 340, 42), StoryPageCaption, mobileSmall);
            Rect viewport = new Rect(24, top + 48, 342, height - 208 - top);
            float textHeight = Mathf.Max(viewport.height, LocalizedStyle(mobileText).CalcHeight(new GUIContent(L(StoryText)), 318));
            Event input = Event.current;
            if (input.type == EventType.MouseDown && viewport.Contains(input.mousePosition))
            {
                dragStart = input.mousePosition;
                dragScrollStart = storyScroll.y;
                scrollPointerDown = true;
            }
            if (input.type == EventType.MouseDrag && scrollPointerDown && (input.mousePosition - dragStart).magnitude > 8)
            {
                dragging = true;
                storyScroll.y = Mathf.Clamp(dragScrollStart + dragStart.y - input.mousePosition.y, 0, Mathf.Max(0, textHeight - viewport.height));
                input.Use();
            }
            bool release = input.type == EventType.MouseUp;
            storyScroll = GUI.BeginScrollView(viewport, storyScroll, new Rect(0, 0, 318, textHeight));
            LocalizedLabel(new Rect(0, 0, 318, textHeight), StoryText, mobileText);
            GUI.EndScrollView();
            if (MobileButton(new Rect(16, height - 144, 90, 44), "Previous", ChroniclePage > 0)) PreviousStoryPage();
            bool last = ChroniclePage + 1 == ChroniclePageCount;
            bool read = HasReadChronicle(chapter.id);
            bool claimed = Game.CompletedChapters.Contains(chapter.id);
            if (last && read)
            {
                if (HasPendingWorldUnlock)
                {
                    if (MobileButton(new Rect(114, height - 144, 260, 44), Game.RequiredWorldStory(pendingWorldGenerator) == null ? "Return / unlock next world" : "Read next required chapter"))
                    {
                        if (Game.RequiredWorldStory(pendingWorldGenerator) == null) ReturnToWorldUnlock();
                        else if (!ContinueWorldUnlockStory()) ReturnToWorldUnlock();
                    }
                }
                else if (ReadingWorldStories)
                {
                    if (MobileButton(new Rect(114, height - 144, 260, 44), NextReadingWorldStory == null ? "Go to Gather" : "Read next required chapter")) ContinueReadingWorldStories();
                }
                else if (MobileButton(new Rect(114, height - 144, 260, 44), claimed ? "Memory treasured" : LF("Receive gift / +{0}", Num(chapter.reward)), !claimed)) ClaimChronicleReward(chapter.id);
            }
            else if (MobileButton(new Rect(114, height - 144, 260, 44), last ? "Finish their story" : "Continue conversation")) ContinueChronicle();
            DrawReadingSurface(new Rect(12, height - 98, 366, 26));
            LocalizedLabel(new Rect(20, height - 96, 350, 20), LF("Remembering together / {0}", L(HasStoryIllustration ? "illustrated memory" : "portrait + chibi")), mobileSmall);
            if (release) { dragging = false; scrollPointerDown = false; }
        }
    }
}