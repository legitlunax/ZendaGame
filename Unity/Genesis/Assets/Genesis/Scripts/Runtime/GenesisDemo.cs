using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genesis
{
    // Deliberately self-contained playtest presentation. No player-save access.
    public sealed partial class GenesisDemo : MonoBehaviour
    {
        public GenesisGame Game { get; private set; }
        private GenesisCatalog catalog;
        private string snapshots;
        private Texture2D background;
        private readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, AudioClip> cues = new Dictionary<string, AudioClip>();
        private readonly List<GameObject> orbiters = new List<GameObject>();
        private readonly List<Material> ownedMaterials = new List<Material>();
        private AudioSource audioSource;
        private Camera worldCamera;
        private RenderTexture worldTexture;
        private GameObject core;
        public GenesisFairy Fairy { get; private set; }
        private Camera fairyCamera;
        private RenderTexture fairyTexture;
        private int hairStyle = 1, shoeStyle = 2, wingStyle;
        private bool showCrown = true, showJewelry = true;
        private GUIStyle title, heading, text, small, button, bigNumber;
        private Texture2D panelTexture, cardTexture, accentTexture;
        private Vector2 scroll;
        private string toast = "Tap the void. Give possibility a beginning.";
        private float toastTime = 6;
        private float speed = 1;
        private int tab, preset, visualLevel = -1;
        private bool muted;
        private bool ready;
        private float pulse;
        private readonly Color ink = new Color(0.83f, 0.87f, 1);
        private readonly Color accent = new Color(0.61f, 0.86f, 1);

        private void Awake()
        {
            TextAsset data = Resources.Load<TextAsset>("Genesis/Data/catalog");
            TextAsset demo = Resources.Load<TextAsset>("Genesis/Data/demo_snapshots");
            if (data == null || demo == null) throw new InvalidOperationException("Genesis demo data missing.");
            catalog = GenesisCatalog.FromJson(data.text);
            snapshots = demo.text;
            Game = new GenesisGame(catalog);
            background = Resources.Load<Texture2D>("Genesis/Art/backgrounds/nebula");
            foreach (GeneratorDefinition generator in catalog.generators)
                icons[generator.id] = Resources.Load<Texture2D>("Genesis/Art/icons/" + generator.id);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.25f;
            foreach (string cue in new[] { "tap", "purchase", "chapter", "miracle", "evolution" })
                cues[cue] = Resources.Load<AudioClip>("Genesis/Audio/" + cue);
            CreateWorld();
            CreateFairy();
            ready = true;
            Debug.Log("GENESIS_PLAYTEST_READY: 106 discoveries, demo-only state, no player saves.");
        }

        private Material Material(Color color)
        {
            Material template = Resources.Load<Material>("Genesis/DemoUnlit");
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (template == null && shader == null) throw new InvalidOperationException("No compatible demo shader found.");
            Material material = template != null ? new Material(template) : new Material(shader);
            material.color = color;
            ownedMaterials.Add(material);
            return material;
        }

        private void CreateWorld()
        {
            GameObject cameraObject = new GameObject("Genesis Camera");
            cameraObject.transform.SetParent(transform);
            worldCamera = cameraObject.AddComponent<Camera>();
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
            worldCamera.backgroundColor = new Color(0.015f, 0.02f, 0.055f);
            worldCamera.transform.position = new Vector3(0, 0, -12);
            worldCamera.orthographic = true;
            worldCamera.orthographicSize = 5.5f;
            worldTexture = new RenderTexture(768, 512, 16);
            worldTexture.Create();
            worldCamera.targetTexture = worldTexture;
            worldCamera.gameObject.AddComponent<AudioListener>();
            core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "First Light";
            core.transform.SetParent(transform);
            core.GetComponent<Renderer>().sharedMaterial = Material(new Color(0.8f, 0.92f, 1));
            Destroy(core.GetComponent<Collider>());
            core.SetActive(false);
            // Deterministic geometry avoids global Random state and downloaded art.
            System.Random random = new System.Random(1047);
            for (int i = 0; i < 100; i++)
            {
                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = "Creation spark " + i;
                star.transform.SetParent(transform);
                star.transform.localPosition = new Vector3((float)(random.NextDouble() - 0.5) * 15,
                    (float)(random.NextDouble() - 0.5) * 9, (float)random.NextDouble() * 4);
                star.transform.localScale = Vector3.one * (0.025f + (float)random.NextDouble() * 0.065f);
                star.GetComponent<Renderer>().sharedMaterial = Material(Color.Lerp(new Color(0.4f, 0.6f, 1), Color.white, (float)random.NextDouble()));
                Destroy(star.GetComponent<Collider>());
                star.SetActive(false);
                orbiters.Add(star);
            }
        }

        private void Update()
        {
            if (!ready) return;
            Game.Tick(Time.deltaTime * speed);
            if (Fairy != null && Fairy.AppearanceId != Game.EquippedAppearance)
                Fairy.ApplyAppearance(catalog.wardrobe.Find(design => design.id == Game.EquippedAppearance));
            toastTime = Mathf.Max(0, toastTime - Time.unscaledDeltaTime);
            pulse = Mathf.Max(0, pulse - Time.deltaTime * 2);
            int level = Game.CreationLevel;
            if (visualLevel != level)
            {
                visualLevel = level;
                core.SetActive(level > 0);
                for (int i = 0; i < orbiters.Count; i++) orbiters[i].SetActive(level > 0 && i < Math.Min(100, level * 3));
            }
            core.transform.localScale = Vector3.one * (0.5f + Mathf.Sin(Time.time * 1.5f) * 0.04f + pulse * 0.12f);
            core.transform.Rotate(Vector3.up, Time.deltaTime * 12);
            for (int i = 0; i < orbiters.Count; i++)
                if (orbiters[i].activeSelf) orbiters[i].transform.RotateAround(Vector3.zero, Vector3.forward, Time.deltaTime * (i % 2 == 0 ? 0.6f : -0.4f));
        }

        private Texture2D Solid(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private void Styles()
        {
            // Native textures may be released during an editor assembly reload while managed styles survive.
            if (panelTexture == null) panelTexture = Solid(new Color(0.035f, 0.045f, 0.10f, 0.96f));
            if (cardTexture == null) cardTexture = Solid(new Color(0.075f, 0.09f, 0.18f, 0.98f));
            if (accentTexture == null) accentTexture = Solid(new Color(0.18f, 0.30f, 0.43f));
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, wordWrap = true };
            title.normal.textColor = ink;
            heading = new GUIStyle(title) { fontSize = 18 };
            text = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            text.normal.textColor = ink;
            small = new GUIStyle(text) { fontSize = 12 };
            small.normal.textColor = new Color(0.6f, 0.68f, 0.84f);
            button = new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true, padding = new RectOffset(10, 10, 8, 8) };
            button.normal.background = accentTexture;
            button.normal.textColor = Color.white;
            button.hover.textColor = accent;
            bigNumber = new GUIStyle(title) { fontSize = 38, alignment = TextAnchor.MiddleCenter };
        }

        private string Num(double value) { return EconomyMath.Format(value, catalog.number_suffixes); }
        private void Say(string message, string cue = null)
        {
            toast = message; toastTime = 4;
            if (Fairy != null && cue != null)
                Fairy.PlayReaction(cue == "miracle" ? "Casting" : cue == "chapter" || cue == "evolution" ? "Celebration" : "Happy", cue == "tap" ? 0.65f : 3);
            if (!muted && cue != null && cues.TryGetValue(cue, out AudioClip clip) && clip != null) audioSource.PlayOneShot(clip);
        }

        private bool Button(Rect rect, string label, bool enabled = true)
        {
            bool previous = GUI.enabled;
            GUI.enabled = enabled;
            bool clicked = GUI.Button(rect, label, button);
            GUI.enabled = previous;
            return clicked;
        }

        private void OnGUI()
        {
            if (!ready) return;
            Styles();
            float scale = Mathf.Max(0.1f, Screen.width / 1280f);
            float height = Screen.height / scale;
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            try
            {
                // The world camera renders offscreen; explicitly clear the GUI surface each frame.
                GUI.DrawTexture(new Rect(0, 0, 1280, height), panelTexture);
                bool portraitMode = height > 900;
                GUI.DrawTexture(new Rect(0, 0, 1280, 100), panelTexture);
                GUI.Label(new Rect(24, 12, 500, 36), "GENESIS / Goddess of Creation", title);
                GUI.Label(new Rect(26, 52, 650, 32), "PLAYTEST 01   /   A universe begins with hope   /   No player saves", small);
                if (Button(new Rect(1040, 20, 96, 44), muted ? "Sound off" : "Sound on")) muted = !muted;
                if (Button(new Rect(1144, 20, 112, 44), "Restart")) LoadPreset(0);
                float leftWidth = portraitMode ? 1232 : 590;
                float leftHeight = portraitMode ? 650 : height - 190;
                DrawCreation(new Rect(24, 112, leftWidth, leftHeight));
                float rightX = portraitMode ? 24 : 638;
                float rightY = portraitMode ? 780 : 112;
                float rightWidth = portraitMode ? 1232 : 618;
                float rightHeight = height - rightY - 80;
                DrawPanel(new Rect(rightX, rightY, rightWidth, Mathf.Max(220, rightHeight)));
                GUI.DrawTexture(new Rect(0, height - 64, 1280, 64), panelTexture);
                GUI.Label(new Rect(24, height - 54, 290, 44), "PREVIEW / " + catalog.demo_presets[preset].name, small);
                if (Button(new Rect(320, height - 52, 80, 40), "< Era")) LoadPreset((preset + catalog.demo_presets.Count - 1) % catalog.demo_presets.Count);
                if (Button(new Rect(408, height - 52, 80, 40), "Era >")) LoadPreset((preset + 1) % catalog.demo_presets.Count);
                if (Button(new Rect(500, height - 52, 120, 40), "Speed " + speed + "x")) speed = speed == 1 ? 10 : speed == 10 ? 100 : 1;
                GUI.Label(new Rect(644, height - 54, 610, 44), toastTime > 0 ? toast : "Discoveries persist through the story. Preview changes reset demo state.", small);
            }
            finally { GUI.matrix = previous; GUI.color = Color.white; GUI.enabled = true; }
        }

        private void DrawCreation(Rect area)
        {
            GUI.BeginGroup(area);
            GUI.DrawTexture(new Rect(0, 0, area.width, area.height), panelTexture);
            GUI.Label(new Rect(20, 18, area.width - 40, 28), Game.CreationLevel == 0 ? "NO SPACE. NO TIME. ONLY HOPE." : "THE LIVING UNIVERSE", heading);
            GUI.Label(new Rect(20, 54, area.width - 40, 52), Num(Game.Quanta) + " " + Game.ResourceName, bigNumber);
            GUI.Label(new Rect(20, 110, area.width - 40, 32), Num(Game.TotalQps) + " / sec   •   " + Game.CreationLevel + " discoveries   •   " + Num(Game.GlobalMultiplier) + "x harmony", text);
            float artHeight = Mathf.Max(90, area.height - 310);
            Rect art = new Rect(20, 152, area.width - 40, artHeight);
            if (worldTexture != null) GUI.DrawTexture(art, worldTexture, ScaleMode.ScaleAndCrop);
            if (Game.CreationLevel > 0 && background != null)
            {
                GUI.color = new Color(1, 1, 1, 0.45f);
                GUI.DrawTexture(art, background, ScaleMode.ScaleAndCrop);
                GUI.color = Color.white;
            }
            if (fairyTexture != null) GUI.DrawTexture(art, fairyTexture, ScaleMode.ScaleToFit);
            string prompt = Game.CreationLevel == 0 ? "✦\nTAP THE VOID\nLet there be possibility" : "✦\nCREATE HOPE\n+" + Num(Game.GlobalMultiplier + Game.TotalQps * (0.01 + Game.Relics["relic_seraph_grace"] * 0.02)) + " per tap";
            if (Button(new Rect(art.x + 8, art.yMax - 62, art.width - 16, 58), prompt.Replace("\n", " ")))
            { double gain = Game.Tap(); pulse = 1; Say("+" + Num(gain) + " " + Game.ResourceName, "tap"); }
            float bottom = area.height - 132;
            GUI.Label(new Rect(20, bottom, area.width - 40, 30), catalog.goddess_forms[Game.GoddessStage].title, heading);
            GUI.Label(new Rect(20, bottom + 36, area.width - 40, 60), Game.CreationLevel == 0 ? "Nothing is visible yet. Your first discovery costs 5 Hope." : catalog.generators[Game.CreationLevel - 1].desc, text);
            GUI.EndGroup();
        }

        private void DrawPanel(Rect area)
        {
            GUI.BeginGroup(area);
            GUI.DrawTexture(new Rect(0, 0, area.width, area.height), panelTexture);
            ChapterDefinition activeChapter = Game.ActiveChapter;
            bool rewardReady = activeChapter != null && Game.ChapterProgress(activeChapter) >= activeChapter.target;
            string[] tabs = { "Discover", rewardReady ? "Story / Reward!" : "Story", "Miracles", "Wardrobe" };
            for (int i = 0; i < tabs.Length; i++)
                if (Button(new Rect(12 + i * (area.width - 24) / 4, 12, (area.width - 32) / 4, 44), (tab == i ? "• " : "") + tabs[i])) { tab = i; scroll = Vector2.zero; }
            if (tab == 0) DrawDiscoveryControls(area.width);
            float contentTop = tab == 0 ? 162 : 70;
            float contentHeight = tab == 0 ? VisibleDiscoveryCount * 125 + 110 : tab == 3 ? catalog.wardrobe.Count * 110 + 360 : 620;
            scroll = GUI.BeginScrollView(new Rect(12, contentTop, area.width - 24, Mathf.Max(40, area.height - contentTop - 12)), scroll, new Rect(0, 0, area.width - 48, contentHeight));
            float width = area.width - 60;
            if (tab == 0) DrawGenerators(width);
            if (tab == 1) DrawStory(width);
            if (tab == 2) DrawMiracles(width);
            if (tab == 3) DrawWardrobe(width);
            GUI.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawGenerators(float width)
        {
            int[] modes = { 1, 10, 100, -1 };
            for (int i = 0; i < modes.Length; i++)
                if (Button(new Rect(i * 85, 0, 78, 38), Game.BuyMode == modes[i] ? "[" + (modes[i] == -1 ? "MAX" : modes[i].ToString()) + "]" : modes[i] == -1 ? "MAX" : modes[i].ToString())) Game.BuyMode = modes[i];
            int visible = 0;
            for (int i = 0; i < catalog.generators.Count; i++)
            {
                GeneratorDefinition generator = catalog.generators[i];
                if (!MatchesDiscovery(generator)) continue;
                float y = 52 + visible++ * 125;
                bool discovered = Game.IsDiscovered(generator.id);
                GUI.DrawTexture(new Rect(0, y, width, 116), cardTexture);
                if (discovered && icons[generator.id] != null) GUI.DrawTexture(new Rect(10, y + 12, 60, 60), icons[generator.id], ScaleMode.ScaleToFit);
                GUI.Label(new Rect(80, y + 8, width - 220, 32), discovered ? generator.fairy + " / " + generator.name : "Unseen possibility", heading);
                GUI.Label(new Rect(80, y + 44, width - 230, 58), discovered ? generator.era + "\nOwned " + Game.Counts[generator.id] + "  •  " + Num(generator.base_prod) + " base/sec" : "Discover the previous layer to awaken this fairy.", small);
                BulkPurchase info = Game.PurchaseInfo(generator.id);
                if (Button(new Rect(width - 140, y + 22, 128, 72), discovered ? "Create x" + info.Amount + "\n" + Num(info.Cost) : "Locked", discovered && info.CanAfford))
                    if (Game.Buy(generator.id)) Say(generator.fairy + " awakens. " + generator.desc, "purchase");
            }
            if (visible == 0) GUI.Label(new Rect(8, 58, width - 16, 60), "No matching discoveries. Clear search or show all possibilities.", text);
        }

        private void DrawStory(float width)
        {
            ChapterDefinition chapter = Game.ActiveChapter;
            GUI.Label(new Rect(8, 4, width - 16, 36), Game.CompletedChapters.Count + " / 107 memories of creation", heading);
            if (chapter == null) { GUI.Label(new Rect(8, 70, width - 16, 90), "Every chapter is remembered. Your living universe continues.", text); return; }
            GUI.Label(new Rect(8, 60, width - 16, 72), chapter.title, title);
            GUI.Label(new Rect(8, 144, width - 16, 90), chapter.goal, text);
            GUI.Label(new Rect(8, 242, width - 16, 110), chapter.lore, text);
            double progress = Game.ChapterProgress(chapter);
            GUI.Label(new Rect(8, 360, width - 16, 36), Num(progress) + " / " + Num(chapter.target) + "  •  Reward " + Num(chapter.reward), heading);
            if (Button(new Rect(8, 416, width - 16, 64), "Remember this chapter", progress >= chapter.target))
                if (Game.ClaimChapter()) Say(chapter.lore, "chapter");
        }

        private void DrawMiracles(float width)
        {
            GUI.Label(new Rect(8, 0, width - 16, 40), "Gifts of creation", title);
            for (int i = 0; i < catalog.miracles.Count; i++)
            {
                MiracleDefinition miracle = catalog.miracles[i];
                if (Button(new Rect(8, 58 + i * 88, width - 16, 76), miracle.name + "\n" + miracle.desc, Game.HasReached("hadron_forge")))
                    Say(Game.ActivateMiracle(miracle.id) ? miracle.name + " activated" : "This miracle is still recovering.", "miracle");
            }
            GUI.Label(new Rect(8, 244, width - 16, 42), "Genesis seeds: " + Game.Seeds + "  /  Bloom reward: " + Game.PendingSeeds, heading);
            if (Button(new Rect(8, 296, width - 16, 58), "Big Bloom — restart the run, preserve memories", Game.PendingSeeds > 0))
                if (Game.Bloom()) Say("A new beginning. Your memories and wardrobe remain.", "evolution");
            for (int i = 0; i < catalog.relics.Count; i++)
            {
                RelicDefinition relic = catalog.relics[i];
                long level = Game.Relics[relic.id];
                if (Button(new Rect(8, 372 + i * 76, width - 16, 66), relic.name + " / level " + level + "\n" + relic.base_seed_cost * (level + 1) + " seeds"))
                    Say(Game.BuyRelic(relic.id) ? "Relic strengthened" : "Not enough seeds, or maximum level reached.", "purchase");
            }
        }

        private void DrawWardrobe(float width)
        {
            for (int i = 0; i < catalog.wardrobe.Count; i++)
            {
                WardrobeDefinition design = catalog.wardrobe[i];
                bool unlocked = Game.IsAppearanceUnlocked(design.id);
                GUI.DrawTexture(new Rect(0, i * 110, width, 100), cardTexture);
                GUI.Label(new Rect(12, i * 110 + 10, width - 150, 38), design.name, heading);
                GUI.Label(new Rect(12, i * 110 + 48, width - 170, 44), design.requirement + "\nLive outfit palette and wing size.", small);
                if (Button(new Rect(width - 130, i * 110 + 18, 118, 62), Game.EquippedAppearance == design.id ? "Selected" : unlocked ? "Select" : "Locked", unlocked))
                    if (Game.Equip(design.id)) Say(design.name + " equipped", "purchase");
            }
            float y = catalog.wardrobe.Count * 110 + 8;
            GUI.Label(new Rect(8, y, width - 16, 28), "Astra / original adult celestial heroine", heading);
            if (Button(new Rect(8, y + 38, width / 2 - 12, 42), "Hair: " + new[] { "Violet long", "Golden long", "Ice short" }[hairStyle])) { hairStyle = (hairStyle + 1) % 3; UpdateFairyCosmetics(); }
            if (Button(new Rect(width / 2, y + 38, width / 2 - 8, 42), "Shoes: " + new[] { "Bare", "Slippers", "Boots" }[shoeStyle])) { shoeStyle = (shoeStyle + 1) % 3; UpdateFairyCosmetics(); }
            if (Button(new Rect(8, y + 88, width / 2 - 12, 42), showCrown ? "Crown on" : "Crown off")) { showCrown = !showCrown; UpdateFairyCosmetics(); }
            if (Button(new Rect(width / 2, y + 88, width / 2 - 8, 42), showJewelry ? "Jewelry on" : "Jewelry off")) { showJewelry = !showJewelry; UpdateFairyCosmetics(); }
            if (Button(new Rect(8, y + 138, width - 16, 42), "Wings: " + new[] { "Butterfly", "Petite", "Hidden" }[wingStyle])) { wingStyle = (wingStyle + 1) % 3; UpdateFairyCosmetics(); }
            for (int i = 0; i < GenesisFairy.Reactions.Length; i++)
                if (Button(new Rect(8 + i % 3 * (width - 16) / 3, y + 190 + i / 3 * 48, (width - 24) / 3, 42), GenesisFairy.Reactions[i]))
                    Fairy.PlayReaction(GenesisFairy.Reactions[i]);
        }

        private void CreateFairy()
        {
            GameObject fairy = new GameObject("Genesis Fairy / original free prototype");
            fairy.transform.SetParent(transform, false);
            fairy.transform.position = new Vector3(1000, 0, 0);
            Fairy = fairy.AddComponent<GenesisFairy>();
            Fairy.ApplyAppearance(catalog.wardrobe.Find(design => design.id == Game.EquippedAppearance));
            UpdateFairyCosmetics();
            GameObject cameraObject = new GameObject("Fairy portrait camera");
            cameraObject.transform.SetParent(transform, false);
            fairyCamera = cameraObject.AddComponent<Camera>();
            fairyCamera.clearFlags = CameraClearFlags.SolidColor;
            fairyCamera.backgroundColor = new Color(0, 0, 0, 0);
            fairyCamera.orthographic = true;
            fairyCamera.orthographicSize = 1.95f;
            fairyCamera.transform.position = new Vector3(1000, 1.72f, -8);
            fairyCamera.nearClipPlane = 0.1f; fairyCamera.farClipPlane = 20;
            fairyTexture = new RenderTexture(768, 768, 24);
            fairyTexture.Create();
            fairyCamera.targetTexture = fairyTexture;
        }

        private void UpdateFairyCosmetics()
        {
            Fairy.SetCosmetics(hairStyle, shoeStyle, wingStyle, showCrown, showJewelry);
        }

        private void LoadPreset(int index)
        {
            preset = index;
            Game.LoadDemoSnapshot(snapshots, index);
            scroll = Vector2.zero;
            Say("Preview: " + catalog.demo_presets[index].name + ". Demo state reset; no player save touched.");
        }

        private void OnDestroy()
        {
            if (Fairy != null) Destroy(Fairy.gameObject);
            if (fairyCamera != null) Destroy(fairyCamera.gameObject);
            if (fairyTexture != null) { fairyTexture.Release(); Destroy(fairyTexture); }
            foreach (Material material in ownedMaterials) if (material != null) Destroy(material);
            if (panelTexture != null) Destroy(panelTexture);
            if (cardTexture != null) Destroy(cardTexture);
            if (accentTexture != null) Destroy(accentTexture);
            if (core != null) Destroy(core);
            foreach (GameObject star in orbiters) if (star != null) Destroy(star);
            if (worldCamera != null) Destroy(worldCamera.gameObject);
            if (worldTexture != null) { worldTexture.Release(); Destroy(worldTexture); }
        }
    }
}