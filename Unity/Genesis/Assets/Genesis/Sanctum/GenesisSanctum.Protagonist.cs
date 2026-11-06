using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private GenesisProtagonistProfile protagonist = new GenesisProtagonistProfile();
        private GenesisVrmIdle lightProtagonist, darknessProtagonist;
        private bool protagonistPicker;
        private string pendingProtagonist = "light", pendingCharacterName = "", pendingLightName, pendingDarknessName;
        private string displayedProtagonist = "light";
        private string protagonistStatus = "";
        private int protagonistReframeFrames;
        public string CharacterName => protagonist.Name;
        public string ProtagonistId => protagonist.Selected;
        public bool ProtagonistConfirmed => protagonist.Confirmed;
        public bool ProtagonistPickerOpen => protagonistPicker || !protagonist.Confirmed;
        public string CurrentWorldName => Game == null || Dimensions == null || Dimensions.Count == 0
            ? "Primordial Possibility" : GatheringDimension.Name;
        public string HomepageHopeText => Num(Game.Quanta) + " " + L(Game.ResourceName);
        public string HomepageHopeRateText => ProductionRate + " " + L(Game.ResourceName) + "/s";

        private void InitializeProtagonist()
        {
            lightProtagonist = goddess;
            pendingLightName = protagonist.LightName;
            pendingDarknessName = protagonist.DarknessName;
            pendingCharacterName = pendingLightName;
        }

        public void OpenProtagonistPicker()
        {
            CancelGoddessPointer();
            haloPicker = backgroundPicker = reactionPicker = iconHelp = false;
            CloseCodePanel();
            companionPointerDown = dragging = scrollPointerDown = false;
            pendingProtagonist = protagonist.Selected;
            pendingLightName = protagonist.LightName;
            pendingDarknessName = protagonist.DarknessName;
            pendingCharacterName = protagonist.Name;
            protagonistStatus = "";
            protagonistPicker = true;
            stageAspect = 0;
        }

        public bool PreviewProtagonist(string id)
        {
            if (!GenesisProtagonistProfile.IsValid(id)) return false;
            if (!TryDisplayProtagonist(id)) return false;
            if (pendingProtagonist == "darkness") pendingDarknessName = pendingCharacterName;
            else pendingLightName = pendingCharacterName;
            pendingProtagonist = id;
            pendingCharacterName = id == "darkness" ? pendingDarknessName : pendingLightName;
            protagonistStatus = "";
            return true;
        }

        public bool ConfirmProtagonist(string id, string name)
        {
            if (!GenesisProtagonistProfile.IsValid(id) || GenesisProtagonistProfile.CleanName(name).Length == 0)
            { protagonistStatus = "Please enter a character name."; return false; }
            if (!TryDisplayProtagonist(id)) return false;
            // Keep both edited names, even if only one protagonist is currently selected.
            var names = protagonist.Save();
            names["lightName"] = pendingLightName ?? protagonist.LightName;
            names["darknessName"] = pendingDarknessName ?? protagonist.DarknessName;
            protagonist.Restore(names);
            protagonist.Choose(id, name);
            protagonistPicker = false;
            GUIUtility.keyboardControl = 0;
            stageAspect = 0;
            SavePlayerProgress();
            return true;
        }

        public void CancelProtagonistPicker()
        {
            if (!protagonist.Confirmed) return;
            TryDisplayProtagonist(protagonist.Selected);
            protagonistPicker = false;
            GUIUtility.keyboardControl = 0;
            stageAspect = 0;
        }

        private bool TryDisplayProtagonist(string id)
        {
            if (!GenesisProtagonistProfile.IsValid(id)) return false;
            // Headless regression fixtures have no stage/model; profile still works.
            if (lightProtagonist == null) lightProtagonist = goddess;
            if (lightProtagonist == null) return true;
            if (id == displayedProtagonist && goddess != null) return true;
            if (id == "darkness" && darknessProtagonist == null)
            {
                GameObject asset = Resources.Load<GameObject>("Genesis/Characters/ZendaGenesis");
                if (asset == null)
                { protagonistStatus = "The Darkness model could not be loaded. Please choose Light or retry."; return false; }
                GameObject model = Instantiate(asset, lightProtagonist.transform.parent);
                model.name = "Darkness protagonist";
                model.transform.localPosition = lightProtagonist.transform.localPosition;
                model.transform.localRotation = goddessRestRotation;
                model.transform.localScale = lightProtagonist.transform.localScale;
                foreach (Transform child in model.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = lightProtagonist.gameObject.layer;
                darknessProtagonist = model.GetComponent<GenesisVrmIdle>();
                if (darknessProtagonist == null) darknessProtagonist = model.AddComponent<GenesisVrmIdle>();
                darknessProtagonist.ShowControls = false;
            }
            lightProtagonist.gameObject.SetActive(id == "light");
            if (darknessProtagonist != null) darknessProtagonist.gameObject.SetActive(id == "darkness");
            goddess = id == "darkness" ? darknessProtagonist : lightProtagonist;
            goddess.ShowControls = false;
            displayedProtagonist = id;
            goddess.transform.localRotation = goddessRestRotation;
            ApplyGoddessRotation();
            protagonistReframeFrames = 3;
            stageAspect = 0;
            selectedGoddessReaction = null;
            reactionRemaining = 0;
            return true;
        }

        private void UpdateProtagonistPresentation()
        {
            if (protagonistReframeFrames <= 0 || goddess == null || !goddess.IsReady) return;
            if (--protagonistReframeFrames > 0) return;
            var meshes = goddess.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshes.Length == 0) return;
            Bounds bounds = meshes[0].bounds;
            foreach (var mesh in meshes) if (mesh.enabled) bounds.Encapsulate(mesh.bounds);
            stageCenter = bounds.center;
            stageSize = Mathf.Max(.1f, Mathf.Max(bounds.extents.y * 1.12f, bounds.extents.x * 1.12f));
            stageAspect = 0;
            SetPortraitCloseup(PortraitCloseup);
        }

        private void RestoreProtagonist(JObject data)
        {
            protagonist = new GenesisProtagonistProfile();
            protagonist.Restore(data);
            InitializePendingProtagonist();
            if (!TryDisplayProtagonist(protagonist.Selected)) protagonistPicker = true;
        }

        private void InitializePendingProtagonist()
        {
            pendingProtagonist = protagonist.Selected;
            pendingLightName = protagonist.LightName;
            pendingDarknessName = protagonist.DarknessName;
            pendingCharacterName = protagonist.Name;
            protagonistPicker = !protagonist.Confirmed;
        }

        private void ResetProtagonist()
        {
            protagonist = new GenesisProtagonistProfile();
            InitializePendingProtagonist();
            TryDisplayProtagonist("light");
        }

        private void DrawProtagonistPicker(float height)
        {
            DrawEnchantedPanel(new Rect(8, 6, 374, height - 12));
            LocalizedLabel(new Rect(24, 22, 342, 32), "Choose your protagonist", mobileTitle);
            LocalizedLabel(new Rect(24, 58, 342, 42), "Light or Darkness / the same journey, your own identity.", mobileSmall);
            DrawLanguageSelector(102);
            if (MobileButton(new Rect(24, 180, 166, 48), pendingProtagonist == "light" ? "Light / selected" : "Choose Light")) PreviewProtagonist("light");
            if (MobileButton(new Rect(200, 180, 166, 48), pendingProtagonist == "darkness" ? "Darkness / selected" : "Choose Darkness")) PreviewProtagonist("darkness");
            Rect preview = new Rect(32, 238, 326, Mathf.Max(110, height - 500));
            if (stageTexture != null && stageCamera != null)
            {
                float aspect = preview.width / preview.height;
                if (!Mathf.Approximately(stageAspect, aspect))
                {
                    stageAspect = aspect;
                    stageCamera.aspect = aspect;
                    stageCamera.orthographicSize = Mathf.Max(stageSize * 1.12f, stageSize * .8f / aspect);
                    stageCamera.transform.position = stageCenter + Vector3.forward * 4;
                    stageCamera.transform.LookAt(stageCenter);
                }
                stageCamera.enabled = true;
                GUI.DrawTexture(preview, stageTexture, ScaleMode.StretchToFill);
                DrawProtagonistHalo(preview);
            }
            float bottom = height - 250;
            LocalizedLabel(new Rect(24, bottom - 2, 342, 30), "Character name (up to 24 characters)", new GUIStyle(mobileText) { fontSize = 15 });
            DrawEnchantedButton(new Rect(24, bottom + 32, 246, 44));
            GUI.SetNextControlName("ProtagonistName");
            pendingCharacterName = GUI.TextField(new Rect(24, bottom + 32, 246, 44), pendingCharacterName ?? "", 64, rpgInputStyle);
            if (MobileButton(new Rect(278, bottom + 32, 88, 44), "Random")) pendingCharacterName = protagonist.SuggestName(pendingProtagonist);
            LocalizedLabel(new Rect(24, bottom + 82, 342, 42), string.IsNullOrEmpty(protagonistStatus)
                ? "Your name appears on the homepage. Tap it later to edit or switch characters." : protagonistStatus, mobileSmall);
            if (MobileButton(new Rect(24, height - 116, 342, 48), protagonist.Confirmed ? "Save character" : "Begin your journey"))
                ConfirmProtagonist(pendingProtagonist, pendingCharacterName);
            if (protagonist.Confirmed && MobileButton(new Rect(24, height - 60, 342, 40), "Cancel")) CancelProtagonistPicker();
        }

        private void DrawProtagonistHeader()
        {
            DrawEnchantedPanel(GoddessHeaderRect);
            var nameStyle = new GUIStyle(mobileTitle) { fontSize = 17, alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Clip };
            var detailStyle = new GUIStyle(mobileSmall) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Clip };
            LocalizedLabel(GoddessTitleRect, CharacterName, nameStyle);
            LocalizedLabel(new Rect(GoddessTitleRect.x, 34, GoddessTitleRect.width, 20), CurrentWorldName, detailStyle);
            if (SoundButton(GoddessTitleRect, new GUIContent("", "Edit character name / Light or Darkness"), invisibleButton)) OpenProtagonistPicker();
        }
    }
}