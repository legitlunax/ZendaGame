using System;
using UniVRM10;
using UnityEngine;

namespace Genesis
{
    // Goddess-centered idle RPG with local, offline-capable player persistence.
    public sealed partial class GenesisSanctum : MonoBehaviour
    {
        public GenesisGame Game { get; private set; }
        public GenesisGathering Gathering { get; private set; }
        public const float InspirationSecondsPerTap = 2f;
        public const float MaxInspirationSeconds = 60f;
        public float InspirationRemaining { get; private set; }
        public bool Inspired => InspirationRemaining > 0;
        public int TapMultiplier => Inspired ? 2 : 1;
        private GenesisVrmIdle goddess;
        private float reactionRemaining;
        private string selectedGoddessReaction;

        public bool PlayGoddessReaction(string mood)
        {
            if (SelectedTab != 0 || StorytellingActive || goddess == null || !goddess.PlayEmotionReaction(mood)) return false;
            selectedGoddessReaction = mood;
            reactionRemaining = GenesisVrmIdle.EmotionReactionSeconds;
            ApplySocialPresentation();
            return true;
        }
        private Vector2 discoveryScroll;
        private GUIStyle title, label, muted, action, number;
        private Texture2D panel, card, gold;
        private string message = "I am Zenda. Together, let us give the void a beginning.";

        private void Awake()
        {
            try
            {
                InitializeGame();
            }
            catch (Exception error)
            {
                // A broken asset must never leave a black window that cannot
                // recover. Report it and let the bootstrap close the player.
                Debug.LogError("Genesis failed to prepare the Sanctum: " + error);
                GenesisBootstrap.ReportFailure("The Sanctum could not be prepared: " + error.Message);
                enabled = false;
            }
        }

        // Every step of the start up is ordered here so a failure names the
        // stage that broke instead of leaving a half built game behind.
        private void InitializeGame()
        {
            ConfigureDesktopWindow();
            TextAsset data = Resources.Load<TextAsset>("Genesis/Data/catalog");
            if (data == null) throw new InvalidOperationException("Sanctum catalog missing.");
            Game = new GenesisGame(GenesisCatalog.FromJson(data.text));
            Gathering = new GenesisGathering(Game);
            Dimensions = GenesisDimensions.Build(Game.Catalog);
            goddess = GetComponentInChildren<GenesisVrmIdle>();
            if (goddess != null) goddess.ShowControls = false;
            InitializeProtagonist();
            SetupMobileStage();
            LoadPlayerProgress();
        }

        private void Update()
        {
            // Unity script reload does not rerun Awake or preserve the session-only model.
            if (Game == null || suspended) return;
            UpdateGameAudio();
            UpdateProtagonistPresentation();
            if (ProtagonistPickerOpen) return;
            AdvanceSession(Time.deltaTime, Application.isFocused);
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= 5) { saveTimer = 0; SavePlayerProgress(); }
            CheckCompletion();
        }

        public void Advance(float seconds)
        { AdvanceSession(seconds, true); }

        private void AdvanceSession(float seconds, bool socialActive)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            float gameSeconds = seconds * GameSpeed;
            Game.Tick(gameSeconds, false);
            Gathering.Advance(gameSeconds);
            RefreshChronicleUnlocks();
            AdvanceExpedition(gameSeconds);
            InspirationRemaining = Mathf.Max(0, InspirationRemaining - seconds);
            reactionRemaining = Mathf.Max(0, reactionRemaining - seconds);
            if (socialActive) AdvanceSocialAnimation(seconds);
            UpdateStoryPresentation(seconds);
        }

        public double Invoke()
        {
            PlayButtonSound();
            InspirationRemaining = Mathf.Min(MaxInspirationSeconds, InspirationRemaining + InspirationSecondsPerTap);
            OnSocialInvocation();
            double gain = Game.Tap();
            if (Inspired) Game.AddQuanta(gain);
            int multiplier = TapMultiplier;
            message = "+" + Num(gain * multiplier) + " " + Game.ResourceName + ". Inspiration: " + Mathf.CeilToInt(InspirationRemaining) + "s / 60s.";
            if (goddess != null) goddess.PlayTapReaction();
            selectedGoddessReaction = null;
            React("happy");
            ApplySocialPresentation();
            RefreshChronicleUnlocks();
            return gain * multiplier;
        }

        public bool Awaken(string id)
        {
            if (RedirectToRequiredWorldStory(id)) return false;
            int previousWorld = GatheringDimensionIndex;
            if (!Game.Buy(id)) return false;
            if (GatheringDimensionIndex != previousWorld) { gatheringWorld = -1; SelectGatheringSection(0); }
            RefreshChronicleUnlocks();
            message = "A new spark joins our universe. Its Hope now grows on its own.";
            React("happy");
            SavePlayerProgress();
            return true;
        }

        public bool Remember()
        {
            ChapterDefinition chapter = Game.ActiveChapter;
            if (chapter == null || !HasReadChronicle(chapter.id) || !Game.ClaimChapter()) return false;
            message = chapter.lore;
            React("relaxed");
            CheckCompletion();
            SavePlayerProgress();
            return true;
        }

        private void React(string mood)
        {
            selectedGoddessReaction = null;
            if (goddess != null && goddess.IsReady) goddess.SetMood(mood);
            reactionRemaining = 2.5f;
        }

        private string Num(double amount) { return EconomyMath.Format(amount, Game.Catalog.number_suffixes); }
        private string ProductionRate => EconomyMath.FormatRate(Game.TotalQps * GameSpeed, Game.Catalog.number_suffixes);

        private Texture2D Solid(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void Styles()
        {
            EnsureWarmTheme();
            if (panel == null) panel = MellowGradient("Mellow peach panel", new Color(0.91f, 0.80f, 0.74f), new Color(0.84f, 0.71f, 0.72f));
            if (card == null) card = WarmSurface("Dusty rose keepsake card", new Color(0.88f, 0.76f, 0.72f, 1), new Color(0.65f, 0.47f, 0.48f), 256);
            if (gold == null) gold = Solid(new Color32(240, 204, 145, 255));
            if (title != null) return;
            label = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, font = rpgBodyFont };
            label.normal.textColor = WarmInk;
            title = new GUIStyle(label) { fontSize = 26, fontStyle = FontStyle.Bold };
            if (warmHeadingFont != null) title.font = warmHeadingFont;
            muted = new GUIStyle(label) { fontSize = 14 };
            muted.normal.textColor = WarmMuted;
            number = new GUIStyle(title) { fontSize = 32 };
            if (rpgBodyFont != null) number.font = rpgBodyFont;
            action = new GUIStyle(GUI.skin.button) { fontSize = 16, wordWrap = true };
            ApplyWarmButton(action);
            rpgInputStyle = new GUIStyle(GUI.skin.textField) { font = rpgBodyFont, fontSize = 18 };
            ApplyWarmButton(rpgInputStyle);
            rpgInputStyle.alignment = TextAnchor.MiddleLeft;
        }

        private bool Button(Rect rect, string text, bool enabled = true)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && enabled;
            bool clicked = SoundButton(rect, text, action);
            GUI.enabled = previous;
            return clicked;
        }

        private void DrawLegacyDesktop()
        {
            if (Game == null) return;
            Styles();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            try
            {
                DrawEnchantedPanel(new Rect(0, 0, 1280, 92));
                LocalizedLabel(new Rect(28, 14, 410, 34), "ZENDA / SANCTUM OF CREATION", title);
                LocalizedLabel(new Rect(28, 55, 650, 24), "Goddess of possibility  /  Local design prototype  /  No player saves", muted);
                LocalizedLabel(new Rect(760, 14, 490, 42), Num(Game.Quanta) + " " + Game.ResourceName, number);
                LocalizedLabel(new Rect(764, 58, 460, 26), ProductionRate + " / sec   |   " + Game.CreationLevel + " discoveries", label);
                DrawEnchantedPanel(new Rect(740, 92, 540, 628));
                DrawObjective();
                DrawDiscoveries();
                DrawEnchantedPanel(new Rect(20, 510, 700, 190));
                LocalizedLabel(new Rect(40, 523, 660, 53), message, label);
                float progress = InspirationRemaining / MaxInspirationSeconds;
                GUI.DrawTexture(new Rect(40, 583, 660, 6), card);
                GUI.DrawTexture(new Rect(40, 583, 660 * progress, 6), gold);
                LocalizedLabel(new Rect(40, 597, 660, 24), Inspired ? "INSPIRATION / double invocation / " + Mathf.CeilToInt(InspirationRemaining) + "s" : "Tap Zenda to replenish Inspiration", muted);
                if (Button(new Rect(40, 632, 660, 48), "INVOKE POSSIBILITY  /  +" + Num((Game.GlobalMultiplier + Game.TotalQps * (0.01 + Game.Relics["relic_seraph_grace"] * 0.02)) * TapMultiplier))) Invoke();
            }
            finally { GUI.matrix = previous; }
        }

        private void DrawObjective()
        {
            ChapterDefinition chapter = Game.ActiveChapter;
            LocalizedLabel(new Rect(764, 108, 490, 30), "YOUR NEXT MEMORY", title);
            if (chapter == null)
            {
                LocalizedLabel(new Rect(764, 150, 490, 110), "Every memory is remembered. Your universe continues to grow.", label);
                return;
            }
            LocalizedLabel(new Rect(764, 146, 490, 30), chapter.title, label);
            LocalizedLabel(new Rect(764, 181, 490, 48), chapter.goal, muted);
            double progress = Game.ChapterProgress(chapter);
            LocalizedLabel(new Rect(764, 234, 490, 24), Num(progress) + " / " + Num(chapter.target) + "   |   Reward +" + Num(chapter.reward), muted);
            if (Button(new Rect(764, 266, 490, 44), "REMEMBER / claim chapter", progress >= chapter.target)) Remember();
        }

        private void DrawDiscoveries()
        {
            LocalizedLabel(new Rect(764, 330, 490, 32), "AWAKEN YOUR UNIVERSE", title);
            int count = 0;
            foreach (GeneratorDefinition generator in Game.Catalog.generators)
                if (Game.IsDiscovered(generator.id)) count++;
            discoveryScroll = GUI.BeginScrollView(new Rect(756, 374, 510, 326), discoveryScroll, new Rect(0, 0, 486, count * 118));
            int row = 0;
            foreach (GeneratorDefinition generator in Game.Catalog.generators)
            {
                if (!Game.IsDiscovered(generator.id)) continue;
                float y = row++ * 118;
                DrawEnchantedPanel(new Rect(0, y, 482, 108));
                LocalizedLabel(new Rect(12, y + 8, 302, 28), generator.fairy + " / " + generator.name, label);
                LocalizedLabel(new Rect(12, y + 42, 292, 55), "Owned " + Game.Counts[generator.id] + "  |  " + Num(generator.base_prod) + " base/sec\n" + generator.era, muted);
                BulkPurchase info = Game.PurchaseInfo(generator.id);
                if (Button(new Rect(320, y + 18, 150, 70), "Awaken x" + info.Amount + "\n" + Num(info.Cost), info.CanAfford)) Awaken(generator.id);
            }
            GUI.EndScrollView();
        }

        private void OnDestroy()
        {
            ReleaseEnchantedConversation();
            SavePlayerProgress();
            ReleaseGameAudio();
            ReleaseWarmTheme();
            ReleaseCompanionAnimation();
            if (storyTexture != null) { storyTexture.Release(); Destroy(storyTexture); }
            ReleaseIconStyles();
            ReleaseMobileStage();
            if (panel != null) Destroy(panel);
            if (card != null) Destroy(card);
            if (gold != null) Destroy(gold);
        }
    }
}
