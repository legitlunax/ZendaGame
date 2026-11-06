using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private static readonly string[] HaloNames =
        {
            "Angel", "Fire", "Water", "Earth", "Wind", "Ice", "Lightning", "Nature",
            "Demon", "Shadow", "Moon", "Sun", "Stars", "Heart", "Time", "Arcane"
        };
        private readonly Dictionary<string, string> fairyHaloStyles = new Dictionary<string, string>();
        private readonly Dictionary<string, Texture2D> haloTextures = new Dictionary<string, Texture2D>();
        private string[] haloStyles;
        private bool haloPicker;
        private Vector2 haloScroll;
        public string EquippedHalo { get; private set; } = "angel";
        public bool HaloPickerOpen => haloPicker;
        public int HaloStyleCount { get { EnsureHalos(); return haloStyles.Length; } }
        public int UnlockedHaloCount
        {
            get
            {
                EnsureHalos();
                int count = 0;
                foreach (string style in haloStyles) if (IsHaloUnlocked(style)) count++;
                return count;
            }
        }

        private void EnsureHalos()
        {
            if (haloStyles != null) return;
            TextAsset asset = Resources.Load<TextAsset>("Genesis/Data/fairy-halos");
            if (asset == null) { haloStyles = new[] { "angel" }; return; }
            JObject data = JObject.Parse(asset.text);
            haloStyles = data["styles"].ToObject<string[]>();
            foreach (JObject fairy in data["fairies"])
                fairyHaloStyles[(string)fairy["id"]] = (string)fairy["style"];
        }

        public string FairyHaloStyle(string generatorId)
        {
            EnsureHalos();
            return generatorId != null && fairyHaloStyles.TryGetValue(generatorId, out string style) ? style : null;
        }

        public bool IsHaloUnlocked(string style)
        {
            EnsureHalos();
            if (Array.IndexOf(haloStyles, style) < 0) return false;
            // Zenda's own angel halo is the starter; others require recruitment, not discovery.
            if (style == "angel") return true;
            if (Game == null) return false;
            foreach (var pair in fairyHaloStyles)
                if (pair.Value == style && Game.Counts.TryGetValue(pair.Key, out long count) && count > 0) return true;
            return false;
        }

        public bool EquipHalo(string style)
        {
            if (!IsHaloUnlocked(style)) return false;
            EquippedHalo = style;
            SavePlayerProgress();
            return true;
        }

        public void OpenHaloPicker()
        {
            CancelGoddessPointer();
            backgroundPicker = reactionPicker = iconHelp = false;
            CloseCodePanel();
            companionPointerDown = dragging = scrollPointerDown = false;
            haloScroll = Vector2.zero;
            haloPicker = true;
        }

        public static Rect GoddessHaloButtonRect => new Rect(14, 140, 52, 52);

        private void DrawHaloCollectionButton()
        {
            Rect rect = GoddessHaloButtonRect;
            EnsureEnchantedGameTheme();
            DrawEnchantedSocket(rect, haloPicker);
            if (interfaceIcons.TryGetValue("soul_dawn", out Texture2D fairy))
                GUI.DrawTexture(new Rect(rect.x + 9, rect.y + 14, 34, 32), fairy, ScaleMode.ScaleToFit);
            Texture2D halo = HaloTexture(EquippedHalo);
            if (halo != null) GUI.DrawTexture(new Rect(rect.x + 7, rect.y + 3, 38, 14), halo, ScaleMode.ScaleToFit);
            if (SoundButton(rect, new GUIContent("", "Fairy halos / " + UnlockedHaloCount + " unlocked"), invisibleButton)) OpenHaloPicker();
        }

        private Texture2D HaloTexture(string style)
        {
            if (style == null) return null;
            if (!haloTextures.TryGetValue(style, out Texture2D texture))
            {
                texture = Resources.Load<Texture2D>("Genesis/Art/halos/" + style);
                haloTextures[style] = texture;
            }
            return texture;
        }

        // Project the animated head into the existing stage: follows idle motion,
        // rotation, portrait/full-body framing without allocating a second 3D rig.
        private void DrawProtagonistHalo(Rect stage)
        {
            if (goddess == null || stageCamera == null) return;
            Animator animator = goddess.GetComponentInChildren<Animator>();
            Transform head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            Vector3 position = head != null ? head.position + Vector3.up * stageSize * .27f :
                stageCenter + Vector3.up * stageSize * 1.05f;
            Vector3 viewport = stageCamera.WorldToViewportPoint(position);
            if (viewport.z <= 0) return;
            Texture2D texture = HaloTexture(IsHaloUnlocked(EquippedHalo) ? EquippedHalo : "angel");
            if (texture == null) return;
            float width = Mathf.Clamp(stage.height * stageSize * .56f / (stageCamera.orthographicSize * 2), 48, 124);
            Rect halo = new Rect(viewport.x * stage.width - width / 2,
                (1 - viewport.y) * stage.height - width * .3125f / 2, width, width * .3125f);
            GUI.BeginGroup(stage);
            GUI.DrawTexture(halo, texture, ScaleMode.ScaleToFit);
            GUI.EndGroup();
        }

        private void DrawHaloPicker(float height)
        {
            if (!haloPicker) return;
            EnsureHalos();
            DrawEnchantedPanel(new Rect(8, 78, 374, height - 148));
            LocalizedLabel(new Rect(22, 90, 346, 30), "Fairy halos / " + UnlockedHaloCount + " of " + HaloStyleCount, mobileTitle);
            LocalizedLabel(new Rect(22, 124, 346, 48), "Recruit fairies to unlock their halos for Zenda. Tap an unlocked halo to wear it.", mobileSmall);
            float visible = Mathf.Max(70, height - 310);
            haloScroll = GUI.BeginScrollView(new Rect(16, 180, 358, visible), haloScroll,
                new Rect(0, 0, 334, haloStyles.Length * 96));
            for (int i = 0; i < haloStyles.Length; i++)
            {
                string style = haloStyles[i];
                bool unlocked = IsHaloUnlocked(style);
                float y = i * 96;
                DrawEnchantedPanel(new Rect(0, y, 330, 90));
                Texture2D texture = HaloTexture(style);
                if (texture != null) GUI.DrawTexture(new Rect(6, y + 12, 78, 32), texture, ScaleMode.ScaleToFit);
                string name = i < HaloNames.Length ? HaloNames[i] : style;
                LocalizedLabel(new Rect(90, y + 5, 230, 24), L(name) + " / " + L(EquippedHalo == style ? "wearing" : unlocked ? "unlocked" : "locked"), mobileText);
                string source = style == "angel" ? "Zenda's starter halo" : "";
                if (style != "angel")
                    foreach (var generator in Game.Catalog.generators)
                        if (FairyHaloStyle(generator.id) == style)
                        { source = LF(unlocked ? "Shared by {0}" : "Recruit {0}", generator.fairy); break; }
                LocalizedLabel(new Rect(90, y + 31, 232, 52), source, mobileSmall);
                if (MobileButton(new Rect(8, y + 48, 76, 34), EquippedHalo == style ? "Worn" : "Wear", unlocked && EquippedHalo != style)) EquipHalo(style);
            }
            GUI.EndScrollView();
            if (MobileButton(new Rect(24, height - 122, 342, 44), "Close")) haloPicker = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            { haloPicker = false; Event.current.Use(); }
        }
    }
}