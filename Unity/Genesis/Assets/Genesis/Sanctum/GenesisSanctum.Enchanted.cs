using System.Linq;
using EnchantedUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Genesis
{
    // Presentation adapter only: the chronicle controller still owns every transition,
    // unlock, read flag, reward and save. No demo story is instantiated in production.
    public sealed partial class GenesisSanctum
    {
        private EnchantedDialogueView enchantedView;
        private RawImage enchantedZenda, enchantedGuest, enchantedBackground;
        private EventSystem enchantedEventSystem;
        private GameObject enchantedPreviousSelection;
        private string enchantedPageKey;
        private bool enchantedMissingReported;
        private Text[] enchantedTexts;
        private string enchantedFontLanguage;
        private Rect enchantedLayoutSafeArea;
        private Vector2Int enchantedLayoutScreen;
        public bool EnchantedConversationVisible => enchantedView != null && enchantedView.gameObject.activeSelf;

        private void UpdateEnchantedConversation()
        {
            bool visible = isActiveAndEnabled && Game != null && StorytellingActive && !ProtagonistPickerOpen;
            if (visible && enchantedView == null && !enchantedMissingReported)
            {
                GameObject prefab = Resources.Load<GameObject>("Genesis/UI/EnchantedConversation");
                if (prefab == null)
                {
                    enchantedMissingReported = true;
                    Debug.LogWarning("Generate the Genesis Enchanted conversation prefab. Using the original reader until then.");
                    return;
                }
                enchantedView = Instantiate(prefab, transform).GetComponent<EnchantedDialogueView>();
                enchantedTexts = enchantedView.GetComponentsInChildren<Text>(true);
                enchantedFontLanguage = null;
                enchantedLayoutScreen = Vector2Int.zero;
                // History is a reading surface, not a second gameplay HUD.
                enchantedView.resource.text = string.Empty;
                enchantedView.resource.gameObject.SetActive(false);
                RectTransform historyTitle = enchantedView.title.rectTransform;
                historyTitle.anchorMin = Vector2.zero;
                historyTitle.anchorMax = Vector2.one;
                historyTitle.offsetMin = new Vector2(52, 28);
                historyTitle.offsetMax = new Vector2(-52, -28);
                enchantedView.continueRequested.AddListener(EnchantedContinue);
                enchantedView.previousRequested.AddListener(EnchantedPrevious);
                enchantedView.backRequested.AddListener(EnchantedBack);
                enchantedZenda = CreateEnchantedPortrait(enchantedView.leftPortrait, "Original Zenda portrait");
                enchantedGuest = CreateEnchantedPortrait(enchantedView.rightPortrait, "Original Lady portrait");
                enchantedBackground = CreateEnchantedPortrait(enchantedView.backdrop, "Original world background");
                enchantedBackground.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                // The prefab's demo navigation must never replace the global navbar.
                Transform navigation = enchantedView.transform.Find("SafeArea/CenteredColumn/PageScroll/Viewport/Content/NavigationTray");
                if (navigation != null) navigation.gameObject.SetActive(false);
                enchantedView.GetComponentInChildren<EnchantedSafeArea>(true).enabled = false;
                enchantedView.gameObject.SetActive(false);
            }
            if (enchantedView == null) return;
            if (visible != enchantedView.gameObject.activeSelf)
            {
                if (visible)
                {
                    EnsureEnchantedInput();
                    enchantedPageKey = null;
                }
                enchantedView.gameObject.SetActive(visible);
                if (enchantedEventSystem != null) enchantedEventSystem.gameObject.SetActive(visible);
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(visible ? enchantedView.backButton.gameObject : enchantedPreviousSelection);
            }
            if (!visible) return;
            LayoutEnchantedConversation();
            SetEnchantedTexture(enchantedBackground, AppBackground, new Rect(0, 0, 1, 1));
            enchantedView.title.text = L("Our storyline");
            enchantedView.chapter.text = L(ReadingChapter.title);
            enchantedView.leftName.text = "Zenda / " + L(StorySpeaker == "Zenda" ? "speaking" : "listening");
            enchantedView.rightName.text = StoryGuestName + " / " + L(StorySpeaker == StoryGuestName ? "speaking" : "listening");
            enchantedView.backButton.GetComponentInChildren<Text>().text = L(HasPendingWorldUnlock ? "< Return to world unlock" : storyOpenedFromGallery ? "< Lady gallery" : "< Storyline");
            enchantedView.previousButton.GetComponentInChildren<Text>().text = L("Previous");
            string key = OpenChronicleId + ":" + ChroniclePage;
            bool lastRead = ChroniclePage + 1 == ChroniclePageCount && HasReadChronicle(OpenChronicleId);
            bool claimed = Game.CompletedChapters.Contains(OpenChronicleId);
            bool canContinue = !lastRead || HasPendingWorldUnlock || ReadingWorldStories || !claimed;
            if (enchantedPageKey != key)
            {
                enchantedPageKey = key;
                enchantedView.SetLine(StoryPageCaption,
                    L(StoryText), ChroniclePage > 0, canContinue);
            }
            enchantedView.previousButton.interactable = ChroniclePage > 0;
            enchantedView.continueButton.interactable = canContinue;
            enchantedView.continueButton.GetComponentInChildren<Text>().text = L(lastRead
                ? HasPendingWorldUnlock
                    ? Game.RequiredWorldStory(pendingWorldGenerator) == null ? "Return / unlock next world" : "Read next required chapter"
                    : ReadingWorldStories ? NextReadingWorldStory == null ? "Go to Gather" : "Read next required chapter"
                    : claimed ? "Memory treasured" : LF("Receive gift / +{0}", Num(ReadingChapter.reward))
                : ChroniclePage + 1 == ChroniclePageCount ? "Finish their story" : "Continue conversation");
            if (enchantedFontLanguage != Language)
            {
                enchantedFontLanguage = Language;
                foreach (Text text in enchantedTexts)
                {
                    text.font = Language == "en" ? rpgBodyFont : LanguageFont;
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    if (text == enchantedView.body)
                    {
                        // Prose keeps its normal size and scrolls instead of shrinking.
                        text.resizeTextForBestFit = false;
                        text.verticalOverflow = VerticalWrapMode.Overflow;
                    }
                    else
                    {
                        text.resizeTextForBestFit = true;
                        text.resizeTextMaxSize = text.fontSize;
                        text.resizeTextMinSize = Mathf.Max(18, Mathf.RoundToInt(text.fontSize * 0.6f));
                        text.verticalOverflow = VerticalWrapMode.Truncate;
                    }
                }
            }
            SetEnchantedTexture(enchantedZenda, HasStoryIllustration ? (Texture)storyIllustration : storyTexture,
                HasStoryIllustration ? new Rect(0, 0, 1, 1) : new Rect(0.25f, 0, 0.5f, 1));
            string iconId = StoryGuestIconId;
            Texture2D guest = null;
            if (!string.IsNullOrEmpty(iconId) && !ladyGalleryIcons.TryGetValue(iconId, out guest))
            {
                guest = Resources.Load<Texture2D>("Genesis/Art/icons/" + iconId);
                ladyGalleryIcons[iconId] = guest;
            }
            SetEnchantedTexture(enchantedGuest, guest, new Rect(0, 0, 1, 1));
        }

        private void EnsureEnchantedInput()
        {
            EventSystem existing = EventSystem.current;
            if (existing != null) { enchantedPreviousSelection = existing.currentSelectedGameObject; return; }
            if (enchantedEventSystem == null)
            {
                // The player runs on the classic input manager. The new input
                // system is not used anywhere in the game, and its native
                // backend is what the Windows player hangs on when it is
                // started through a shortcut.
                GameObject input = new GameObject("Genesis conversation input", typeof(EventSystem), typeof(StandaloneInputModule));
                input.transform.SetParent(transform, false);
                enchantedEventSystem = input.GetComponent<EventSystem>();
            }
            enchantedEventSystem.gameObject.SetActive(true);
        }

        private void LayoutEnchantedConversation()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (enchantedLayoutScreen == screen && enchantedLayoutSafeArea == Screen.safeArea) return;
            enchantedLayoutScreen = screen;
            enchantedLayoutSafeArea = Screen.safeArea;
            Rect safe = MobileSafeArea(Screen.width, Screen.height, Screen.safeArea);
            float scale = Mathf.Min(safe.width / 390f, safe.height / 620f);
            // Convert the shared IMGUI navbar's top edge to bottom-origin Canvas anchors.
            float bottom = Screen.height - safe.yMax + 66 * scale;
            RectTransform area = (RectTransform)enchantedView.transform.Find("SafeArea");
            area.anchorMin = new Vector2(safe.xMin / Screen.width, bottom / Screen.height);
            area.anchorMax = new Vector2(safe.xMax / Screen.width, (Screen.height - safe.yMin) / Screen.height);
            area.offsetMin = area.offsetMax = Vector2.zero;
            RectTransform background = (RectTransform)enchantedView.transform.Find("Background");
            background.anchorMin = new Vector2(0, bottom / Screen.height);
            background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero;
        }

        private static RawImage CreateEnchantedPortrait(Image placeholder, string name)
        {
            placeholder.enabled = false;
            RawImage image = new GameObject(name, typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter)).GetComponent<RawImage>();
            image.transform.SetParent(placeholder.transform, false);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            image.raycastTarget = false;
            image.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            return image;
        }

        private static void SetEnchantedTexture(RawImage target, Texture texture, Rect uv)
        {
            if (target.texture == texture && target.uvRect == uv && target.enabled == (texture != null)) return;
            target.texture = texture;
            target.uvRect = uv;
            target.enabled = texture != null;
            if (texture != null) target.GetComponent<AspectRatioFitter>().aspectRatio = texture.width * uv.width / (texture.height * uv.height);
        }

        private void EnchantedPrevious() { PlayButtonSound(); PreviousStoryPage(); }
        private void EnchantedBack() { PlayButtonSound(); ReturnFromStory(); }
        private void EnchantedContinue()
        {
            if (!StorytellingActive) return;
            PlayButtonSound();
            if (ChroniclePage + 1 != ChroniclePageCount || !HasReadChronicle(OpenChronicleId)) { ContinueChronicle(); return; }
            if (HasPendingWorldUnlock)
            {
                if (Game.RequiredWorldStory(pendingWorldGenerator) == null || !ContinueWorldUnlockStory()) ReturnToWorldUnlock();
            }
            else if (ReadingWorldStories) ContinueReadingWorldStories();
            else ClaimChronicleReward(OpenChronicleId);
        }

        private void ReleaseEnchantedConversation()
        {
            if (enchantedView != null) Destroy(enchantedView.gameObject);
            if (enchantedEventSystem != null) Destroy(enchantedEventSystem.gameObject);
        }

        private void HideEnchantedConversation()
        {
            if (enchantedView != null) enchantedView.gameObject.SetActive(false);
            if (enchantedEventSystem != null) enchantedEventSystem.gameObject.SetActive(false);
        }
    }
}
