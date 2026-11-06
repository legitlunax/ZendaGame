using System.Collections.Generic;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private readonly Dictionary<string, Texture2D> interfaceIcons = new Dictionary<string, Texture2D>();
        private Texture2D glassDisc, glassRing;
        private Texture2D gameLogo;
        private GUIStyle invisibleButton;
        private bool iconHelp;
        private bool backgroundPicker, reactionPicker;
        public bool GoddessPopupOpen => ProtagonistPickerOpen || haloPicker || backgroundPicker || reactionPicker || CodePanelOpen || CreditsOpen || AssetCreditsOpen;
        private static readonly string[] GoddessReactionIds = { "happy", "relaxed", "sad", "angry", "surprised" };
        private static readonly string[] GoddessReactionNames = { "Happy / joyful wave", "Relaxed / gentle breath", "Sad / tender bow", "Angry / little pout", "Surprised / delighted gasp" };
        private float stageAspect;
        private bool goddessPointerDown, goddessPointerMoved;
        private Vector2 goddessPointerStart;
        private float goddessPointerStartYaw;
        private int goddessPointerControl;
        private Quaternion goddessRestRotation;
        private bool goddessRotationCaptured;
        public float GoddessYaw { get; private set; }
        private const float GoddessDragThreshold = 8f;
        private const float GoddessDegreesPerPixel = 0.6f;
        private static readonly string[] InterfaceIconIds =
        {
            "stellar_nursery", "veiled_sight", "reality", "universal_heart",
            "first_intention", "hope_fluctuation", "soul_dawn", "time_flow"
        };

        public int LoadedInterfaceIconCount => interfaceIcons.Count;

        private void EnsureIconStyles()
        {
            if (glassDisc == null) glassDisc = FairyHexagon(false);
            if (glassRing == null) glassRing = FairyHexagon(true);
            if (invisibleButton == null) invisibleButton = new GUIStyle();
            foreach (string id in InterfaceIconIds)
                if (!interfaceIcons.ContainsKey(id))
                {
                    Texture2D icon = Resources.Load<Texture2D>("Genesis/Art/icons/" + id);
                    if (icon != null) interfaceIcons.Add(id, icon);
                }
        }

        private Texture2D FairyHexagon(bool ring)
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = ring ? "Fairy hexagon selected outline" : "Fairy mellow hexagon", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < vertices.Length; i++)
            {
                float angle = i * Mathf.PI / 3;
                vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 43;
            }
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f - 48, y + 0.5f - 48);
                    float distance = float.MaxValue;
                    bool inside = true;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector2 edge = vertices[(i + 1) % vertices.Length] - vertices[i];
                        Vector2 offset = point - vertices[i];
                        float along = Mathf.Clamp01(Vector2.Dot(offset, edge) / edge.sqrMagnitude);
                        distance = Mathf.Min(distance, (offset - edge * along).magnitude);
                        if (edge.x * offset.y - edge.y * offset.x < 0) inside = false;
                    }
                    // Rounded corners, six readable facets and a one-pixel antialiased edge.
                    float signedDistance = (inside ? -distance : distance) - 2;
                    float alpha = Mathf.Clamp01(0.5f - signedDistance);
                    if (ring) alpha *= Mathf.Clamp01(signedDistance + 3.5f);
                    Color fill = Color.Lerp(new Color(0.79f, 0.73f, 0.82f), new Color(0.94f, 0.83f, 0.78f), y / 95f);
                    // An inset pearl bevel keeps the pastel surface distinct from pale gardens.
                    fill = Color.Lerp(fill, new Color(1f, 0.93f, 0.85f), Mathf.Clamp01(1 - Mathf.Abs(signedDistance + 3)) * 0.6f);
                    fill = Color.Lerp(fill, new Color(0.64f, 0.51f, 0.60f), Mathf.Clamp01(signedDistance + 1.5f));
                    Color pixel = ring ? new Color(0.46f, 0.34f, 0.44f) : fill;
                    pixel.a = alpha;
                    texture.SetPixel(x, y, pixel);
                }
            texture.Apply();
            return texture;
        }

        private bool FloatingIcon(Rect rect, string id, string description, bool selected = false)
        {
            DrawEnchantedSocket(rect, selected);
            if (interfaceIcons.TryGetValue(id, out Texture2D icon))
                GUI.DrawTexture(new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20), icon, ScaleMode.ScaleToFit);
            // Tooltips name icon actions without drawing words on the button face.
            return SoundButton(rect, new GUIContent("", description), invisibleButton);
        }

        // Reserve distinct header, portrait and action bands: UI never covers the avatar.
        public static Rect GoddessStageRect(float height) { return new Rect(0, 78, 390, Mathf.Max(1, height - 246)); }
        public static Rect GoddessHeaderRect => new Rect(8, 6, 374, 52);
        public static Rect GoddessAudioRect => new Rect(14, 78, 52, 52);
        public static Rect GoddessTitleRect => new Rect(20, 12, 350, 24);
        public static Rect GoddessInspirationRect(float height) => new Rect(16, height - 152, 132, 66);
        public static Rect GoddessHopePanelRect(float height) => new Rect(242, height - 152, 132, 66);

        public static Rect[] GoddessControlRects(float height)
        {
            float middle = GoddessStageRect(height).center.y;
            return new[]
            {
                new Rect(14, middle - 64, 52, 52), new Rect(14, middle + 8, 52, 52),
                new Rect(324, middle + 8, 52, 52),
                new Rect(324, 78, 52, 52), new Rect(157, height - 154, 76, 76),
                new Rect(324, middle - 64, 52, 52), GoddessHaloButtonRect
            };
        }

        private void DrawGoddessStage(float height)
        {
            EnsureIconStyles();
            Rect stage = GoddessStageRect(height);
            if (stageCamera != null && stageTexture != null)
            {
                float aspect = stage.width / stage.height;
                if (!Mathf.Approximately(stageAspect, aspect))
                {
                    stageAspect = aspect;
                    stageCamera.aspect = aspect;
                    // Full-body mode fits width too; closeup intentionally crops the lower body.
                    SetPortraitCloseup(PortraitCloseup);
                    if (!PortraitCloseup) stageCamera.orthographicSize = Mathf.Max(stageSize * 1.05f, stageSize * 0.72f / aspect);
                }
                GUI.DrawTexture(stage, stageTexture, ScaleMode.StretchToFill);
                DrawProtagonistHalo(stage);
            }
            DrawProtagonistHeader();
            DrawAudioToggle(GoddessAudioRect);
            Rect[] controls = GoddessControlRects(height);
            if (FloatingIcon(controls[0], "stellar_nursery", "Choose background style", backgroundPicker)) OpenGoddessPopup(true);
            if (FloatingIcon(controls[1], "veiled_sight", "Toggle portrait and full body", PortraitCloseup)) { SetPortraitCloseup(!PortraitCloseup); stageAspect = 0; }
            if (FloatingIcon(controls[2], "universal_heart", "Choose Zenda's reaction", reactionPicker)) OpenGoddessPopup(false);
            if (FloatingIcon(controls[3], "first_intention", "Help / Creator credits", iconHelp)) iconHelp = !iconHelp;
            if (FloatingIcon(controls[4], "hope_fluctuation", "Invoke possibility", Inspired)) Invoke();
            if (FloatingIcon(controls[5], "veiled_sight", "Secret fairy", CodePanelOpen)) OpenCodePanel();
            DrawHaloCollectionButton();
            if (Game.CampaignCompleted && FloatingIcon(new Rect(324, 140, 52, 52), "soul_dawn", "Completion credits / Continue or reset", CreditsOpen)) OpenCompletionCredits();
            HandleGoddessPointer(height, Event.current, GUIUtility.GetControlID(FocusType.Passive));
            DrawLadyCompanion(height);
            float progress = InspirationRemaining / MaxInspirationSeconds;
            GUI.DrawTexture(new Rect(141, height - 72, 108, 3), glassDisc);
            GUI.DrawTexture(new Rect(141, height - 72, 108 * progress, 3), gold);
            Rect inspiration = GoddessInspirationRect(height);
            DrawEnchantedPanel(inspiration);
            LocalizedLabel(new Rect(inspiration.x + 8, inspiration.y + 8, inspiration.width - 16, inspiration.height - 16), Inspired ? "Inspiration / 2x\n" + Mathf.CeilToInt(InspirationRemaining) + "s / 60s" : "Tap Zenda\n+2s of inspiration", mobileSmall);
            Rect hope = GoddessHopePanelRect(height);
            DrawEnchantedPanel(hope);
            LocalizedLabel(new Rect(hope.x + 8, hope.y + 8, hope.width - 16, hope.height - 16), HomepageHopeText + "\n" + HomepageHopeRateText, mobileSmall);
            if (iconHelp)
            {
                DrawReadingSurface(new Rect(24, 84, 290, 262));
                LocalizedLabel(new Rect(36, 94, 266, 158), "Tap Zenda: +2s boost, max 60s\nDrag Zenda left/right: rotate\nStar cluster: backgrounds\nEye: portrait / full body\nHeart: emotion reactions\nNavbar: Goddess / RPG / Storyline", mobileSmall);
                if (gameLogo == null) gameLogo = Resources.Load<Texture2D>("Genesis/Art/Branding/ZendaGenesisLogo");
                if (gameLogo != null) GUI.DrawTexture(new Rect(36, 250, 36, 36), gameLogo, ScaleMode.ScaleToFit);
                LocalizedLabel(new Rect(80, 256, 222, 28), "ZendaGenesis / Creator", mobileText);
                LocalizedLabel(new Rect(36, 286, 266, 50), "Created by Gonzalo Ciocca\ngonzalo.ciocca@hotmail.com", mobileSmall);
                LocalizedLabel(new Rect(24, 354, 342, 60), L(PersistenceStatus) + "\n" + LF("Offline gathering: full rate / {0}x speed.", GameSpeed), mobileSmall);
                if (MobileButton(new Rect(36, 420, 278, 44), "Music, fonts & licenses")) { AssetCreditsOpen = true; iconHelp = false; }
            }
            if (GUI.tooltip.Length > 0)
                LocalizedLabel(new Rect(16, height - 82, 358, 20), GUI.tooltip, mobileSmall);
        }

        private void OpenGoddessPopup(bool backgrounds)
        {
            haloPicker = false;
            CancelGoddessPointer();
            backgroundPicker = backgrounds;
            if (backgrounds) SelectBackgroundStyle(SceneStyle);
            reactionPicker = !backgrounds;
            iconHelp = false;
            companionPointerDown = false;
        }

        private void CancelGoddessPointer()
        {
            if (goddessPointerDown && GUIUtility.hotControl == goddessPointerControl) GUIUtility.hotControl = 0;
            goddessPointerDown = goddessPointerMoved = false;
        }

        private void ApplyGoddessRotation()
        {
            if (goddess == null || !goddessRotationCaptured) return;
            // Story portraits always face the reader; retain the chosen angle for the Goddess tab.
            goddess.transform.localRotation = goddessRestRotation * Quaternion.Euler(0, SelectedTab == 0 && !StorytellingActive ? GoddessYaw : 0, 0);
        }

        private void HandleGoddessPointer(float height, Event input, int controlId)
        {
            if (input == null) return;
            if (!GUI.enabled || GoddessPopupOpen || iconHelp || SelectedTab != 0 || StorytellingActive || goddess == null)
            {
                CancelGoddessPointer();
                return;
            }
            if (input.type == EventType.Ignore) { CancelGoddessPointer(); return; }
            if (input.type == EventType.MouseDown && input.button == 0)
            {
                if (GUIUtility.hotControl != 0 || companionPointerDown || !GoddessTapRect(height).Contains(input.mousePosition)) return;
                if (CompanionLady != null && LadyCompanionHitRect(height).Contains(input.mousePosition)) return;
                if (GoddessAudioRect.Contains(input.mousePosition)) return;
                foreach (Rect control in GoddessControlRects(height)) if (control.Contains(input.mousePosition)) return;
                goddessPointerDown = true;
                goddessPointerMoved = false;
                goddessPointerStart = input.mousePosition;
                goddessPointerStartYaw = GoddessYaw;
                goddessPointerControl = controlId;
                GUIUtility.hotControl = controlId;
                input.Use();
            }
            else if (goddessPointerDown && (input.type == EventType.MouseDrag || input.type == EventType.MouseUp) && input.button == 0)
            {
                Vector2 delta = input.mousePosition - goddessPointerStart;
                if (delta.sqrMagnitude > GoddessDragThreshold * GoddessDragThreshold) goddessPointerMoved = true;
                if (goddessPointerMoved)
                {
                    GoddessYaw = Mathf.Repeat(goddessPointerStartYaw - delta.x * GoddessDegreesPerPixel, 360);
                    ApplyGoddessRotation();
                }
                bool invoke = input.type == EventType.MouseUp && !goddessPointerMoved && GoddessTapRect(height).Contains(input.mousePosition);
                if (input.type == EventType.MouseUp) CancelGoddessPointer();
                input.Use();
                if (invoke) Invoke();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) CancelGoddessPointer();
            SetSuspension(!focused, appPaused);
            RefreshAudioPlayback();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) CancelGoddessPointer();
            SetSuspension(lostFocus, paused);
            RefreshAudioPlayback();
        }

        private void OnDisable()
        {
            HideEnchantedConversation();
            CancelGoddessPointer();
            PauseGameAudio();
            if (goddess != null && goddessRotationCaptured) goddess.transform.localRotation = goddessRestRotation;
        }

        public static Rect GoddessPopupRect(float height, bool backgrounds)
        {
            float popupHeight = backgrounds ? 238 : 394;
            return new Rect(20, (height - popupHeight) / 2, 350, popupHeight);
        }

        private void DrawGoddessPopup(float height)
        {
            if (!backgroundPicker && !reactionPicker) return;
            Rect popup = GoddessPopupRect(height, backgroundPicker);
            Color previous = GUI.color;
            GUI.color = new Color(0.29f, 0.16f, 0.23f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, 390, height), Texture2D.whiteTexture);
            GUI.color = previous;
            if (!popup.Contains(Event.current.mousePosition) && SoundButton(new Rect(0, 0, 390, height), GUIContent.none, invisibleButton))
            {
                // A transparent full-screen button catches taps outside the dialog.
                if (!popup.Contains(Event.current.mousePosition)) backgroundPicker = reactionPicker = false;
            }
            DrawEnchantedPanel(popup);
            LocalizedLabel(new Rect(popup.x + 14, popup.y + 12, 322, 30), backgroundPicker ? "App background style" : "Zenda's reactions", mobileTitle);
            if (backgroundPicker)
            {
                DrawBackgroundSelector(popup);
            }
            else
            {
                for (int i = 0; i < GoddessReactionIds.Length; i++)
                    if (SoundButton(new Rect(popup.x + 14, popup.y + 54 + i * 54, 322, 48), GoddessReactionNames[i], mobileButton))
                    {
                        if (PlayGoddessReaction(GoddessReactionIds[i])) reactionPicker = false;
                    }
            }
            if (!backgroundPicker && reactionPicker && SoundButton(new Rect(popup.x + 14, popup.yMax - 56, 322, 44), "Close", mobileButton)) reactionPicker = false;
        }

        public Rect GoddessTapRect(float height)
        {
            if (goddess == null || stageCamera == null) return Rect.zero;
            Rect stage = GoddessStageRect(height);
            Vector2 min = new Vector2(1, 1), max = Vector2.zero;
            foreach (SkinnedMeshRenderer mesh in goddess.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!mesh.enabled || !mesh.gameObject.activeInHierarchy) continue;
                Bounds bounds = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 viewport = stageCamera.WorldToViewportPoint(corner);
                    if (viewport.z <= 0) continue;
                    min = Vector2.Min(min, new Vector2(viewport.x, viewport.y));
                    max = Vector2.Max(max, new Vector2(viewport.x, viewport.y));
                }
            }
            return Rect.MinMaxRect(stage.x + Mathf.Clamp01(min.x) * stage.width, stage.y + (1 - Mathf.Clamp01(max.y)) * stage.height,
                stage.x + Mathf.Clamp01(max.x) * stage.width, stage.y + (1 - Mathf.Clamp01(min.y)) * stage.height);
        }

        private void ReleaseIconStyles()
        {
            if (glassDisc != null) Destroy(glassDisc);
            if (glassRing != null) Destroy(glassRing);
            interfaceIcons.Clear();
        }
    }
}