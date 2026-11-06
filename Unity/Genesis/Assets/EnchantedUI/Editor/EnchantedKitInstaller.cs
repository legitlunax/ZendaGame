using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EnchantedUI.Editor
{
    public static class EnchantedKitInstaller
    {
        private const string Root = "Assets/EnchantedUI";

        [MenuItem("Tools/Enchanted UI/1 - Configure Sprites")]
        public static void ConfigureSprites()
        {
            string[] paths = Directory.GetFiles(Root + "/Art", "*.png", SearchOption.AllDirectories);
            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                if (importer == null) continue;
                EnchantedArtImporter.Configure(importer, Path.GetFileNameWithoutExtension(path));
                importer.SaveAndReimport();
            }
            Debug.Log("Enchanted UI: sprites configured with alpha, Full Rect meshes and 9-slice borders.");
        }

        [MenuItem("Tools/Enchanted UI/2 - Build Prefabs and Demo Scene")]
        public static void BuildKit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Enchanted UI: exit Play Mode before building the kit.");
                return;
            }
            ConfigureSprites();
            EnsureFolder(Root + "/Generated");
            string output = UniqueBuildFolder();
            EnsureFolder(output + "/Prefabs");
            EnchantedTheme theme = CreateTheme(output);
            // The user's open scenes are kept. Nothing is added to or saved in them.
            Scene previousScene = SceneManager.GetActiveScene();
            Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(demoScene);
            try
            {
                SavePart(Panel("Panel", null, theme, 560, theme.panelSliceScale), output);
                SavePart(Header(null, theme), output);
                SavePart(Portrait(null, theme, "Character / speaking"), output);
                SavePart(Dialogue(null, theme), output);
                SavePart(ActionButton("PrimaryButton", null, theme, "Continue conversation", false, true, 132), output);
                SavePart(ActionButton("SecondaryButton", null, theme, "Storyline", true, true, 120), output);
                SavePart(ActionButton("DisabledButton", null, theme, "Previous", true, false, 120), output);
                SavePart(HexTab(null, theme, "I"), output);
                SavePart(NavigationTray(null, theme), output);

                GameObject canvas = BuildScreen(theme);
                PrefabUtility.SaveAsPrefabAsset(canvas, output + "/Prefabs/ConversationScreen.prefab");
                // A demo driver is scene-only; production prefab contains presentation only.
                EnchantedDemoStory story = canvas.AddComponent<EnchantedDemoStory>();
                story.view = canvas.GetComponent<EnchantedDialogueView>();
                CreateEventSystem();
                GameObject cameraObject = new GameObject("DemoCamera", typeof(Camera));
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = theme.backgroundBottom;
                camera.cullingMask = 0;
                EditorSceneManager.SaveScene(demoScene, output + "/ConversationDemo.unity");
                AssetDatabase.SaveAssets();
                if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
                EditorSceneManager.CloseScene(demoScene, true);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(output + "/ConversationDemo.unity");
                Debug.Log("Enchanted UI: 10 prefabs, theme and example scene created at " + output +
                    ". Open ConversationDemo.unity on its own, then press Play. Existing scenes were not changed.");
            }
            catch
            {
                if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
                throw;
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
        private static string UniqueBuildFolder()
        {
            int index = 1;
            string path;
            do { path = Root + "/Generated/Build_" + index.ToString("000"); index++; }
            while (Directory.Exists(path));
            EnsureFolder(path);
            return path;
        }
        private static EnchantedTheme CreateTheme(string output)
        {
            EnchantedTheme theme = ScriptableObject.CreateInstance<EnchantedTheme>();
            theme.panel = LoadSprite("panel_enchanted");
            theme.button = LoadSprite("button_amethyst");
            theme.blossom = LoadSprite("ornament_blossom");
            theme.hexSocket = LoadSprite("hex_portrait_socket");
            theme.titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Genesis/Resources/Genesis/Fonts/Almendra-Bold.ttf");
            theme.bodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Genesis/Resources/Genesis/Fonts/CrimsonText-Regular.ttf");
            if (theme.titleFont == null) theme.titleFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/DejaVuSerif.ttf");
            if (theme.bodyFont == null) theme.bodyFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/DejaVuSans.ttf");
            if (theme.panel == null || theme.button == null || theme.titleFont == null || theme.bodyFont == null)
                throw new InvalidOperationException("Missing Enchanted UI assets. Keep the folder at Assets/EnchantedUI.");
            AssetDatabase.CreateAsset(theme, output + "/EnchantedTheme.asset");
            return theme;
        }
        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + name + ".png");
        }
        private static RectTransform Rect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            if (parent != null) rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }
        private static void Stretch(RectTransform rect, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
        private static void Height(RectTransform rect, float height, float flexible = 0)
        {
            rect.sizeDelta = new Vector2(960, height);
            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = flexible;
        }
        private static Image Art(RectTransform rect, Sprite sprite, bool sliced, float scale = 1)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.pixelsPerUnitMultiplier = scale;
            image.raycastTarget = false;
            return image;
        }
        private static Text Label(string name, Transform parent, EnchantedTheme theme, string value,
            int size, bool title = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            RectTransform rect = Rect(name, parent);
            Stretch(rect);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = title ? theme.titleFont : theme.bodyFont;
            text.text = value;
            text.fontSize = size;
            text.color = theme.text;
            text.alignment = alignment;
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
        private static RectTransform Panel(string name, Transform parent, EnchantedTheme theme, float height, float scale)
        {
            RectTransform panel = Rect(name, parent);
            Height(panel, height);
            Art(panel, theme.panel, true, scale);
            return panel;
        }
        private static RectTransform Header(Transform parent, EnchantedTheme theme)
        {
            RectTransform panel = Panel("Header", parent, theme, 196, 5f);
            Text title = Label("Title", panel, theme, "Our storyline", 56, true);
            title.rectTransform.anchorMin = new Vector2(0, 0.45f);
            title.rectTransform.offsetMin = new Vector2(52, 0);
            title.rectTransform.offsetMax = new Vector2(-52, -28);
            Text resource = Label("Resource", panel, theme, "4.51 K Hope / 0.7 per sec", 30);
            resource.color = theme.mutedText;
            resource.rectTransform.anchorMax = new Vector2(1, 0.48f);
            resource.rectTransform.offsetMin = new Vector2(54, 30);
            resource.rectTransform.offsetMax = new Vector2(-54, 0);
            return panel;
        }
        private static RectTransform ActionButton(string name, Transform parent, EnchantedTheme theme,
            string caption, bool secondary, bool interactable, float height)
        {
            RectTransform root = Rect(name, parent);
            Height(root, height);
            // A full rectangular hit target; decorative alpha never shrinks the tappable area.
            Image hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;
            RectTransform art = Rect("Artwork", root);
            Stretch(art);
            Image image = Art(art, theme.button, true, theme.buttonSliceScale);
            image.color = secondary ? theme.secondaryTint : Color.white;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.94f, 1f, 1);
            colors.pressedColor = new Color(0.72f, 0.63f, 0.84f, 1);
            colors.selectedColor = new Color(1f, 0.94f, 1f, 1);
            colors.disabledColor = theme.disabledTint;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.interactable = interactable;
            Text label = Label("Label", root, theme, caption, 42, true, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, 56, 18, 56, 18);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 30;
            label.resizeTextMaxSize = 42;
            EnchantedButtonFeedback feedback = root.gameObject.AddComponent<EnchantedButtonFeedback>();
            feedback.artwork = art;
            feedback.label = label;
            return root;
        }
        private static RectTransform Portrait(Transform parent, EnchantedTheme theme, string caption)
        {
            RectTransform panel = Panel("PortraitCard", parent, theme, 440, 4f);
            LayoutElement layout = panel.GetComponent<LayoutElement>();
            layout.minWidth = 100;
            layout.flexibleWidth = 1;
            RectTransform inner = Rect("PortraitArea", panel);
            Stretch(inner, 36, 108, 36, 36);
            inner.gameObject.AddComponent<RectMask2D>();
            RectTransform portrait = Rect("Portrait", inner);
            Stretch(portrait);
            Image image = Art(portrait, null, false);
            image.preserveAspect = true;
            image.enabled = false;
            Text placeholder = Label("Placeholder", inner, theme, "Your portrait", 26, false, TextAnchor.MiddleCenter);
            placeholder.color = theme.mutedText;
            RectTransform strip = Panel("NameStrip", panel, theme, 92, 12f);
            strip.GetComponent<LayoutElement>().ignoreLayout = true;
            strip.anchorMin = Vector2.zero;
            strip.anchorMax = new Vector2(1, 0);
            strip.pivot = new Vector2(0.5f, 0);
            strip.anchoredPosition = new Vector2(0, 16);
            strip.sizeDelta = new Vector2(-32, 92);
            Text name = Label("CharacterName", strip, theme, caption, 30, true, TextAnchor.MiddleCenter);
            Stretch(name.rectTransform, 32, 14, 32, 14);
            return panel;
        }
        private static RectTransform Dialogue(Transform parent, EnchantedTheme theme)
        {
            RectTransform panel = Panel("DialoguePanel", parent, theme, 560, 4f);
            Text speaker = Label("Speaker", panel, theme, "Zenda / happy / 1 of 11", 34, true);
            speaker.rectTransform.anchorMin = new Vector2(0, 1);
            speaker.rectTransform.anchorMax = Vector2.one;
            speaker.rectTransform.pivot = new Vector2(0.5f, 1);
            speaker.rectTransform.sizeDelta = new Vector2(-108, 64);
            speaker.rectTransform.anchoredPosition = new Vector2(0, -35);
            RectTransform divider = Rect("Divider", panel);
            divider.anchorMin = new Vector2(0, 1);
            divider.anchorMax = Vector2.one;
            divider.pivot = new Vector2(0.5f, 1);
            divider.sizeDelta = new Vector2(-100, 16);
            divider.anchoredPosition = new Vector2(0, -100);
            Art(divider, LoadSprite("divider_gold"), false);

            RectTransform scroll = Rect("DialogueScroll", panel);
            Stretch(scroll, 54, 48, 54, 130);
            ScrollRect sr = scroll.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 34;
            RectTransform viewport = Rect("Viewport", scroll);
            Stretch(viewport);
            Image input = viewport.gameObject.AddComponent<Image>();
            input.color = Color.clear;
            input.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            Text body = Label("Body", viewport, theme,
                "Then Lady Riyara arrived like the warmth of a sunrise. I had learned how to shelter a spark; now I had to let it shine beyond my hands.",
                42, false, TextAnchor.UpperLeft);
            body.lineSpacing = 1.18f;
            RectTransform content = body.rectTransform;
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            ContentSizeFitter fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewport;
            sr.content = content;

            RectTransform flowers = Rect("OptionalBlossom", panel);
            flowers.anchorMin = new Vector2(0, 1);
            flowers.anchorMax = new Vector2(0, 1);
            flowers.pivot = new Vector2(0, 1);
            flowers.anchoredPosition = new Vector2(-8, 8);
            flowers.sizeDelta = new Vector2(100, 100);
            Art(flowers, theme.blossom, false).preserveAspect = true;
            return panel;
        }
        private static RectTransform HexTab(Transform parent, EnchantedTheme theme, string caption)
        {
            RectTransform tab = Rect("HexTab", parent);
            tab.sizeDelta = new Vector2(144, 144);
            LayoutElement layout = tab.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = 144;
            layout.minHeight = 144;
            layout.preferredWidth = 144;
            layout.preferredHeight = 144;
            Image image = Art(tab, theme.hexSocket, false);
            image.preserveAspect = true;
            image.raycastTarget = true;
            Button button = tab.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.pressedColor = new Color(0.7f, 0.6f, 0.85f, 1);
            colors.selectedColor = new Color(1f, 0.88f, 1f, 1);
            button.colors = colors;
            Text label = Label("Label", tab, theme, caption, 30, true, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, 32, 32, 32, 32);
            return tab;
        }
        private static RectTransform NavigationTray(Transform parent, EnchantedTheme theme)
        {
            RectTransform tray = Panel("NavigationTray", parent, theme, 192, 6f);
            RectTransform row = Rect("Tabs", tray);
            Stretch(row, 44, 24, 44, 24);
            HorizontalLayoutGroup group = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;
            group.spacing = 32;
            HexTab(row, theme, "I");
            HexTab(row, theme, "II");
            HexTab(row, theme, "III");
            return tray;
        }
        private static GameObject BuildScreen(EnchantedTheme theme)
        {
            RectTransform canvasRect = Rect("EnchantedConversation", null);
            Canvas canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            // Balanced scaling plus a centered width limit for desktop windows.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasRect.gameObject.AddComponent<GraphicRaycaster>();
            EnchantedDialogueView view = canvasRect.gameObject.AddComponent<EnchantedDialogueView>();
            RectTransform backdropContainer = Rect("Background", canvasRect);
            Stretch(backdropContainer);
            EnchantedGradient gradient = backdropContainer.gameObject.AddComponent<EnchantedGradient>();
            gradient.top = theme.backgroundTop;
            gradient.bottom = theme.backgroundBottom;
            gradient.raycastTarget = false;
            RectTransform world = Rect("WorldIllustration", backdropContainer);
            Stretch(world);
            view.backdrop = Art(world, null, false);
            view.backdrop.enabled = false;
            AspectRatioFitter worldFit = world.gameObject.AddComponent<AspectRatioFitter>();
            worldFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            RectTransform safe = Rect("SafeArea", canvasRect);
            Stretch(safe);
            safe.gameObject.AddComponent<EnchantedSafeArea>();
            RectTransform column = Rect("CenteredColumn", safe);
            Stretch(column);
            column.gameObject.AddComponent<EnchantedMaxWidth>();
            RectTransform scrollRoot = Rect("PageScroll", column);
            Stretch(scrollRoot);
            ScrollRect pageScroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            pageScroll.horizontal = false;
            pageScroll.movementType = ScrollRect.MovementType.Clamped;
            pageScroll.scrollSensitivity = 48;
            RectTransform viewport = Rect("Viewport", scrollRoot);
            Stretch(viewport);
            Image input = viewport.gameObject.AddComponent<Image>();
            input.color = Color.clear;
            input.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 24, 24);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            pageScroll.content = content;
            pageScroll.viewport = viewport;

            RectTransform header = Header(content, theme);
            view.title = header.Find("Title").GetComponent<Text>();
            view.resource = header.Find("Resource").GetComponent<Text>();
            RectTransform back = ActionButton("BackButton", content, theme, "< Storyline", true, true, 112);
            view.backButton = back.GetComponent<Button>();
            RectTransform chapter = Panel("ChapterBanner", content, theme, 112, 10f);
            view.chapter = Label("Chapter", chapter, theme, "06 / Riyara awakens", 34, true);
            Stretch(view.chapter.rectTransform, 48, 22, 48, 22);

            RectTransform portraitRow = Rect("Portraits", content);
            Height(portraitRow, 440);
            HorizontalLayoutGroup portraits = portraitRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            portraits.spacing = 12;
            portraits.childControlWidth = true;
            portraits.childControlHeight = true;
            portraits.childForceExpandWidth = true;
            portraits.childForceExpandHeight = true;
            RectTransform left = Portrait(portraitRow, theme, "Zenda / speaking");
            left.name = "LeftPortraitCard";
            RectTransform right = Portrait(portraitRow, theme, "Lady Riyara / listening");
            right.name = "RightPortraitCard";
            view.leftPortrait = left.Find("PortraitArea/Portrait").GetComponent<Image>();
            view.rightPortrait = right.Find("PortraitArea/Portrait").GetComponent<Image>();
            // Hide the placeholders in the full screen; standalone portrait prefab retains them.
            left.Find("PortraitArea/Placeholder").gameObject.SetActive(false);
            right.Find("PortraitArea/Placeholder").gameObject.SetActive(false);
            view.leftName = left.Find("NameStrip/CharacterName").GetComponent<Text>();
            view.rightName = right.Find("NameStrip/CharacterName").GetComponent<Text>();

            RectTransform dialogue = Dialogue(content, theme);
            view.speaker = dialogue.Find("Speaker").GetComponent<Text>();
            view.body = dialogue.Find("DialogueScroll/Viewport/Body").GetComponent<Text>();
            view.dialogueScroll = dialogue.Find("DialogueScroll").GetComponent<ScrollRect>();
            RectTransform actions = Rect("Actions", content);
            Height(actions, 132);
            HorizontalLayoutGroup actionLayout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 12;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = false;
            actionLayout.childForceExpandHeight = true;
            RectTransform previous = ActionButton("PreviousButton", actions, theme, "Previous", true, false, 132);
            previous.GetComponent<LayoutElement>().flexibleWidth = 1;
            RectTransform next = ActionButton("ContinueButton", actions, theme, "Continue conversation", false, true, 132);
            next.GetComponent<LayoutElement>().flexibleWidth = 2.2f;
            view.previousButton = previous.GetComponent<Button>();
            view.continueButton = next.GetComponent<Button>();
            RectTransform caption = Panel("MemoryCaption", content, theme, 88, 12f);
            Text memory = Label("Label", caption, theme, "Remembering together / portrait + chibi", 26, false, TextAnchor.MiddleCenter);
            Stretch(memory.rectTransform, 32, 16, 32, 16);
            NavigationTray(content, theme);
            return canvasRect.gameObject;
        }
        private static void SavePart(RectTransform part, string output)
        {
            PrefabUtility.SaveAsPrefabAsset(part.gameObject, output + "/Prefabs/" + part.name + ".prefab");
            UnityEngine.Object.DestroyImmediate(part.gameObject);
        }
        private static void CreateEventSystem()
        {
            GameObject go = new GameObject("EnchantedDemoEventSystem", typeof(EventSystem));
            // Reflection keeps the Input System package optional and Editor-only.
            // The resulting component is serialized normally into the scene.
            Type modern = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
#if ENABLE_INPUT_SYSTEM
            if (modern != null)
            {
                Component module = go.AddComponent(modern);
                System.Reflection.MethodInfo defaults = modern.GetMethod("AssignDefaultActions", Type.EmptyTypes);
                if (defaults != null) defaults.Invoke(module, null);
                return;
            }
#endif
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
