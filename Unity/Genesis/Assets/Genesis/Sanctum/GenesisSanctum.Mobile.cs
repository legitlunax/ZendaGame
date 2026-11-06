using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public int SelectedTab { get; private set; }
        public int SceneStyle { get; private set; }
        public int ExpeditionWins { get; private set; }
        public float ExpeditionRemaining { get; private set; }
        public bool ExpeditionReady { get; private set; }
        public bool ExpeditionRunning { get; private set; }
        public double ExpeditionReward { get; private set; }
        public int SelectedRoute { get; private set; }
        public int ActiveRoute { get; private set; }
        public int BondRank => 1 + ExpeditionWins / 3;
        public float ExpeditionDuration { get; private set; } = 30;
        public double NextExpeditionReward => JourneyRewardForRoute(SelectedRoute);
        public double JourneyRewardForRoute(int index)
        {
            if (index < 0 || index >= RouteDurations.Length) throw new System.ArgumentOutOfRangeException(nameof(index));
            return System.Math.Max(10, Game.TotalQps * RouteDurations[index] / 2) * (1 + (BondRank - 1) * 0.1);
        }
        private static readonly string[] RouteNames = { "First Light", "Moon Garden", "Astral Sea" };
        private static readonly float[] RouteDurations = { 30, 60, 90 };
        public bool IsRouteUnlocked(int index) { return index >= 0 && index < RouteNames.Length && ExpeditionWins >= index * 3; }

        public bool SelectRoute(int index)
        {
            if (ExpeditionRunning || ExpeditionReady || !IsRouteUnlocked(index)) return false;
            SelectedRoute = index;
            return true;
        }
        public bool PortraitCloseup { get; private set; }
        private Camera stageCamera;
        private RenderTexture stageTexture;
        private Vector3 stageCenter;
        private float stageSize;
        private Rect previousCameraRect;
        private RenderTexture previousCameraTarget;
        private Color previousCameraColor;
        private bool previousCameraEnabled;
        private Light[] stageLights;
        private Color[] originalLightColors;
        private Vector2 chronicleScroll;
        private bool dragging;
        private bool scrollPointerDown;
        private Vector2 dragStart;
        private float dragScrollStart;
        private GUIStyle mobileTitle, mobileText, mobileSmall, mobileButton;

        private void DrawGlobalNavbar(float height)
        {
            EnsureIconStyles();
            DrawEnchantedPanel(new Rect(12, height - 66, 366, 64));
            if (FloatingIcon(new Rect(59, height - 60, 52, 52), "soul_dawn", "Main Goddess", SelectedTab == 0)) SelectTab(0);
            if (FloatingIcon(new Rect(169, height - 60, 52, 52), "reality", "Idle RPG", SelectedTab == 1)) SelectTab(1);
            if (FloatingIcon(new Rect(279, height - 60, 52, 52), "time_flow", "Storyline", SelectedTab == 2)) SelectTab(2);
        }

        public void SelectTab(int index)
        {
            CancelGoddessPointer();
            if (index == 2 && SelectedTab != 2) FollowGatheringStoryWorld();
            SelectedTab = Mathf.Clamp(index, 0, 2);
            backgroundPicker = reactionPicker = false;
            companionPointerDown = false;
            dragging = false;
            scrollPointerDown = false;
            UpdateStoryPresentation(0);
            SavePlayerProgress();
        }

        public static Rect MobileSafeArea(int width, int height, Rect safe)
        {
            Rect screen = new Rect(0, 0, Mathf.Max(1, width), Mathf.Max(1, height));
            float left = Mathf.Clamp(safe.xMin, 0, screen.width);
            float right = Mathf.Clamp(safe.xMax, left, screen.width);
            float bottom = Mathf.Clamp(safe.yMin, 0, screen.height);
            float top = Mathf.Clamp(safe.yMax, bottom, screen.height);
            if (right - left < 1 || top - bottom < 1) return screen;
            return new Rect(left, screen.height - top, right - left, top - bottom);
        }

        private void SetupMobileStage()
        {
            stageCamera = GetComponentInChildren<Camera>();
            if (stageCamera == null || goddess == null) return;
            goddessRestRotation = goddess.transform.localRotation;
            goddessRotationCaptured = true;
            previousCameraRect = stageCamera.rect;
            previousCameraTarget = stageCamera.targetTexture;
            previousCameraColor = stageCamera.backgroundColor;
            previousCameraEnabled = stageCamera.enabled;
            SkinnedMeshRenderer[] meshes = goddess.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshes.Length == 0) return;
            Bounds bounds = meshes[0].bounds;
            foreach (SkinnedMeshRenderer mesh in meshes) bounds.Encapsulate(mesh.bounds);
            stageCenter = bounds.center;
            // Fit both vertical body height and horizontal T-pose width in a portrait stage.
            stageSize = Mathf.Max(bounds.extents.y * 1.12f, bounds.extents.x * 1.12f);
            stageTexture = new RenderTexture(600, 1100, 24) { name = "Zenda mobile stage" };
            stageTexture.Create();
            stageCamera.targetTexture = stageTexture;
            stageCamera.rect = new Rect(0, 0, 1, 1);
            stageLights = GetComponentsInChildren<Light>();
            originalLightColors = new Color[stageLights.Length];
            for (int i = 0; i < stageLights.Length; i++) originalLightColors[i] = stageLights[i].color;
            SetSceneStyle(0);
            SetPortraitCloseup(false);
        }

        public void SetSceneStyle(int index)
        {
            if (!IsBackgroundStyleUnlocked(index)) return;
            SceneStyle = index;
            SavePlayerProgress();
            if (stageCamera != null) stageCamera.backgroundColor = Color.clear;
            if (stageLights == null) return;
            for (int i = 0; i < stageLights.Length; i++)
                stageLights[i].color = originalLightColors[i];
        }

        public void SetPortraitCloseup(bool value)
        {
            PortraitCloseup = value;
            SavePlayerProgress();
            if (stageCamera == null) return;
            Vector3 focus = stageCenter + (value ? Vector3.up * stageSize * 0.35f : Vector3.zero);
            stageCamera.orthographicSize = stageSize * (value ? 0.62f : 1.05f);
            stageCamera.transform.position = focus + Vector3.forward * 4;
            stageCamera.transform.LookAt(focus);
        }

        public bool BeginExpedition()
        {
            if (ExpeditionRunning || ExpeditionReady || !IsRouteUnlocked(SelectedRoute) || !ViewedDimension.IsFormed(Game)) return false;
            ExpeditionRunning = true;
            ExpeditionDimension = SelectedDimension;
            ActiveRoute = SelectedRoute;
            ExpeditionDuration = RouteDurations[ActiveRoute];
            ExpeditionRemaining = ExpeditionDuration;
            // Snapshot the reward so upgrades during a journey cannot change its promise.
            ExpeditionReward = NextExpeditionReward;
            SavePlayerProgress();
            return true;
        }

        private void AdvanceExpedition(float seconds)
        {
            if (!ExpeditionRunning) return;
            ExpeditionRemaining = Mathf.Max(0, ExpeditionRemaining - seconds);
            if (ExpeditionRemaining > 0) return;
            ExpeditionRunning = false;
            ExpeditionReady = true;
            message = RouteNames[ActiveRoute] + " in " + Dimensions[ExpeditionDimension].Name + " is complete. Claim in Idle RPG / Journeys.";
        }

        public bool ClaimExpedition()
        {
            if (!ExpeditionReady) return false;
            Game.AddQuanta(ExpeditionReward);
            ExpeditionWins++;
            ExpeditionReady = false;
            message = "A starlight journey remembered. Our bond grows.";
            React("happy");
            SavePlayerProgress();
            return true;
        }

        private void MobileStyles()
        {
            Styles();
            if (mobileTitle == null)
            {
                mobileTitle = new GUIStyle(title) { fontSize = 20 };
                mobileText = new GUIStyle(label) { fontSize = 17 };
                mobileSmall = new GUIStyle(muted) { fontSize = 15 };
                mobileButton = new GUIStyle(action) { fontSize = 15 };
            }
            foreach (GUIStyle style in new[] { mobileTitle, mobileText, mobileSmall })
            {
                style.wordWrap = true;
                style.normal.textColor = style == mobileSmall ? WarmMuted : WarmInk;
            }
            mobileTitle.fontSize = 20;
            mobileTitle.fontStyle = FontStyle.Bold;
            if (warmHeadingFont != null) mobileTitle.font = warmHeadingFont;
            mobileText.fontSize = 17;
            mobileSmall.fontSize = 15;
            // Native GUIStyle state can be cleared by editor reloads; restore the theme.
            ApplyWarmButton(mobileButton);
            mobileButton.fontSize = 15;
            mobileButton.wordWrap = true;
            rpgInputStyle.font = Language == "en" ? rpgBodyFont : LanguageFont;
        }

        private bool MobileButton(Rect rect, string caption, bool enabled = true)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && enabled && !dragging;
            Texture2D background = mobileButton.normal.background;
            if (!GUI.enabled && gameTheme == null) mobileButton.normal.background = warmButtonDisabled;
            bool result = SoundButton(rect, caption, mobileButton);
            mobileButton.normal.background = background;
            GUI.enabled = previous;
            return result;
        }

        public static Rect IdleHudRect => new Rect(8, 6, 374, 66);
        public static Rect IdleHudTitleRect => new Rect(47, 11, 269, 23);
        public static Rect IdleHudBalanceRect => new Rect(48, 38, 268, 27);
        public static Rect IdleHudHelpRect => new Rect(326, 14, 44, 44);

        private void IdleHudText(Rect rect, string caption, GUIStyle source, int size, Color color, TextAnchor alignment)
        {
            var style = new GUIStyle(LocalizedStyle(source))
            {
                fontSize = size, alignment = alignment, wordWrap = false,
                padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0, fixedWidth = 0, clipping = TextClipping.Clip
            };
            style.normal.textColor = color;
            ClearButtonBackgrounds(style);
            var content = new GUIContent(caption);
            while (style.fontSize > 10 && (style.CalcSize(content).x > rect.width || style.CalcSize(content).y > rect.height))
                style.fontSize--;
            GUI.Label(rect, content, style);
        }

        private void DrawIdleRpgHeader()
        {
            EnsureIconStyles();
            DrawEnchantedPanel(IdleHudRect);
            Color accent = gameTheme != null ? gameTheme.gold : new Color32(240, 204, 145, 255);
            // The kit's illustrated panel supplies the ornamentation; no flat rail.
            DrawKitIcon(new Rect(18, 12, 23, 23), gameTheme.diamond);
            IdleHudText(IdleHudTitleRect, L("Idle RPG"), mobileTitle, 18, accent, TextAnchor.MiddleLeft);

            DrawEnchantedSocket(new Rect(13, 35, 32, 32), true);
            DrawKitIcon(new Rect(18, 40, 22, 22), gameTheme.spark);
            IdleHudText(IdleHudBalanceRect, Num(Game.Quanta) + " " + L(Game.ResourceName)
                + " (+" + ProductionRate + "/s)", mobileTitle, 19, WarmInk, TextAnchor.MiddleLeft);

            if (MobileButton(IdleHudHelpRect, "?"))
            {
                gatheringGuide = !gatheringGuide;
                chronicleScroll = Vector2.zero;
            }
        }

        private void OnGUI()
        {
            if (Game == null) return;
            MobileStyles();
            Rect safe = MobileSafeArea(Screen.width, Screen.height, Screen.safeArea);
            // Limit landscape width rather than shrinking text to fit a desktop canvas.
            float scale = Mathf.Min(safe.width / 390f, safe.height / 620f);
            float height = safe.height / scale;
            float left = safe.x + (safe.width - 390 * scale) / 2;
            Matrix4x4 previous = GUI.matrix;
            bool previousEnabled = GUI.enabled;
            bool conversationVisible = EnchantedConversationVisible && !ProtagonistPickerOpen;
            if (!conversationVisible) DrawWarmBackground(new Rect(0, 0, Screen.width, Screen.height));
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, safe.y, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            try
            {
                if (ProtagonistPickerOpen)
                {
                    DrawProtagonistPicker(height);
                    return;
                }
                // Modal dialogs own input, including navbar and companion controls.
                GUI.enabled = previousEnabled && !GoddessPopupOpen;
                if (conversationVisible)
                {
                    // The reader Canvas stops above this same global navigation band.
                    DrawGlobalNavbar(height);
                    return;
                }
                if (SelectedTab == 2) DrawEnchantedPanel(new Rect(8, 6, 374, 66));
                if (SelectedTab == 0) DrawGoddessMobile(height);
                else
                {
                    if (SelectedTab == 1) DrawIdleRpgHeader();
                    else
                    {
                        LocalizedLabel(new Rect(16, 10, 358, 52), "Our storyline", mobileTitle);
                    }
                    if (SelectedTab == 1) DrawIdleRpgMobile(height);
                    else DrawChroniclesMobile(height);
                }
                DrawGlobalNavbar(height);
                GUI.enabled = previousEnabled;
                DrawGoddessPopup(height);
                DrawCodePanel(height);
                DrawCompletionCredits(height);
                DrawAssetCredits(height);
                DrawHaloPicker(height);
            }
            finally { GUI.matrix = previous; GUI.enabled = previousEnabled; }
        }

        private void DrawGoddessMobile(float height)
        {
            DrawGoddessStage(height);
        }

        private void ReleaseMobileStage()
        {
            if (stageCamera != null)
            {
                stageCamera.targetTexture = previousCameraTarget;
                stageCamera.rect = previousCameraRect;
                stageCamera.backgroundColor = previousCameraColor;
                stageCamera.enabled = previousCameraEnabled;
            }
            if (stageLights != null)
                for (int i = 0; i < stageLights.Length; i++)
                    if (stageLights[i] != null) stageLights[i].color = originalLightColors[i];
            if (stageTexture != null) { stageTexture.Release(); Destroy(stageTexture); }
        }
    }
}