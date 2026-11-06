using System;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public const float MaximumGameSpeed = 1000000f;
        public float GameSpeed { get; private set; } = 1f;
        public bool CodePanelOpen { get; private set; }
        public bool ResetCodePending { get; private set; }
        public string CodeStatus { get; private set; } = "Enter an unlock code.";
        private string enteredCode = "";

        public void OpenCodePanel()
        {
            haloPicker = false;
            CancelGoddessPointer();
            backgroundPicker = reactionPicker = iconHelp = false;
            dragging = scrollPointerDown = companionPointerDown = false;
            enteredCode = "";
            ResetCodePending = false;
            CodeStatus = "Enter an unlock code. Current speed: " + GameSpeed + "x.";
            CodePanelOpen = true;
        }

        public void CloseCodePanel()
        {
            CodePanelOpen = ResetCodePending = false;
            enteredCode = "";
            GUIUtility.keyboardControl = 0;
        }

        public bool SubmitUnlockCode(string code)
        {
            ResetCodePending = false;
            string normalized = (code ?? "").Trim();
            int vip = string.Equals(normalized, "vipx10", StringComparison.OrdinalIgnoreCase) ? 10 :
                string.Equals(normalized, "vipx30", StringComparison.OrdinalIgnoreCase) ? 30 :
                string.Equals(normalized, "vipx50", StringComparison.OrdinalIgnoreCase) ? 50 : 0;
            if (vip > 0)
            {
                GameSpeed = vip;
                CodeStatus = "Speed set to " + vip + "x, including offline gathering and journeys. Codes do not stack.";
                SavePlayerProgress();
                return true;
            }
            if (string.Equals(normalized, "GONZALO", StringComparison.OrdinalIgnoreCase))
            {
                GameSpeed = 100f;
                CodeStatus = "Speed set to 100x gathering and journeys. Codes do not stack.";
                SavePlayerProgress();
                return true;
            }
            if (string.Equals(normalized, "zendaxhope", StringComparison.OrdinalIgnoreCase))
            {
                GameSpeed = MaximumGameSpeed;
                CodeStatus = "Speed set to 1,000,000x, including offline gathering and journeys. Codes do not stack.";
                SavePlayerProgress();
                return true;
            }
            if (string.Equals(normalized, "RESET", StringComparison.OrdinalIgnoreCase))
            {
                ResetCodePending = true;
                CodeStatus = "Reset everything to the beginning? This clears this game's progress and speed boost. This cannot be undone.";
                return true;
            }
            CodeStatus = "Code not recognized. Check the code and try again.";
            return false;
        }

        public bool ConfirmResetCode()
        {
            if (!ResetCodePending) return false;
            resetting = true;
            CloseChronicle();
            CloseLadyGallery();
            Game = new GenesisGame(Game.Catalog);
            Gathering = new GenesisGathering(Game);
            unlockedChronicles.Clear();
            pendingWorldGenerator = null;
            storyOpenedFromGallery = false;
            storyPages.Clear();
            ChroniclePage = 0;
            SelectedDimension = ExpeditionDimension = SelectedRoute = ActiveRoute = ExpeditionWins = 0;
            ExpeditionRemaining = 0;
            ExpeditionReward = 0;
            ExpeditionDuration = 30;
            ExpeditionRunning = ExpeditionReady = false;
            InspirationRemaining = reactionRemaining = 0;
            selectedGoddessReaction = null;
            SecondsWithoutInvocation = SocialAnimationTime = socialAffectionRemaining = socialWelcomeRemaining = 0;
            socialAffection = "kiss";
            fullInspirationCelebrated = nextAffectionHug = false;
            selectedLadyName = companionLadyName = null;
            EquippedHalo = "angel";
            haloPicker = false;
            ladyGalleryScroll = ladyHistoryScroll = discoveryScroll = Vector2.zero;
            dimensionMapExpanded = gatheringGuide = iconHelp = false;
            gatheringWorld = -1;
            GoddessYaw = 0;
            SetPortraitCloseup(false);
            stageAspect = 0;
            SetSceneStyle(0);
            SelectBackgroundStyle(0);
            SelectGatheringSection(0);
            SelectTab(0);
            FollowGatheringStoryWorld();
            if (goddess != null && goddess.IsReady)
            {
                // Restore the captured rig and clear every transient reaction timer.
                goddess.enabled = false;
                goddess.enabled = true;
                goddess.ShowControls = false;
            }
            GameSpeed = 1;
            if (suspended) suspendedAt = UtcSeconds;
            creditsSeen = CreditsOpen = false;
            message = "I am Zenda. Together, let us give the void a beginning.";
            CloseCodePanel();
            CodeStatus = "Reset complete. A new beginning at 1x speed.";
            UpdateStoryPresentation(0);
            ResetProtagonist();
            resetting = false;
            SavePlayerProgress();
            return true;
        }

        private void DrawCodePanel(float height)
        {
            if (!CodePanelOpen) return;
            DrawEnchantedPanel(new Rect(0, 0, 390, height));
            float top = 96;
            LocalizedLabel(new Rect(24, top, 342, 36), "Unlock code / " + GameSpeed + "x", mobileTitle);
            LocalizedLabel(new Rect(24, top + 44, 342, 88), CodeStatus, mobileText);
            if (!ResetCodePending)
            {
                GUI.SetNextControlName("UnlockCode");
            DrawEnchantedButton(new Rect(24, top + 144, 342, 48));
            enteredCode = GUI.TextField(new Rect(24, top + 144, 342, 48), enteredCode, 32, rpgInputStyle);
                if (MobileButton(new Rect(200, top + 208, 166, 48), "Apply code")) SubmitUnlockCode(enteredCode);
                if (MobileButton(new Rect(24, top + 208, 166, 48), "Close")) CloseCodePanel();
            }
            else
            {
                if (MobileButton(new Rect(24, top + 208, 166, 48), "Cancel reset"))
                {
                    ResetCodePending = false;
                    enteredCode = "";
                    CodeStatus = "Reset cancelled. Your progress is unchanged.";
                }
                if (MobileButton(new Rect(200, top + 208, 166, 48), "Confirm RESET")) ConfirmResetCode();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                CloseCodePanel();
                Event.current.Use();
            }
        }
    }
}