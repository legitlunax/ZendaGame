using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public bool CreditsOpen { get; private set; }
        private bool creditsSeen;
        private Vector2 completionCreditsScroll;
        private const string CompletionCreditsText =
            "You have completed every chapter. Your universe and fairies can keep growing.\n\nZendaGenesis\nCreated by Gonzalo Ciocca\ngonzalo.ciocca@hotmail.com\n\n"
            + "Inspiration of Love from Agnia Barskaya\nInspiration of Fun from Alicia_Neni from everlastingmc\nInspiration from the Dreams\n\n"
            + "Thank you for sharing this journey with Zenda.";

        private void CheckCompletion()
        {
            if (!Game.CampaignCompleted || creditsSeen) return;
            OpenCompletionCredits();
            creditsSeen = true;
            SavePlayerProgress();
        }

        public bool OpenCompletionCredits()
        {
            if (Game == null || !Game.CampaignCompleted) return false;
            // The overlay reader must release the screen before the IMGUI credits draw.
            CloseChronicle();
            HideEnchantedConversation();
            CloseCodePanel();
            backgroundPicker = reactionPicker = iconHelp = false;
            CancelGoddessPointer();
            CreditsOpen = true;
            completionCreditsScroll = Vector2.zero;
            return true;
        }

        public void CloseCompletionCredits() { CreditsOpen = false; SavePlayerProgress(); }

        private void DrawCompletionCredits(float height)
        {
            if (!CreditsOpen) return;
            DrawReadingSurface(new Rect(8, 78, 374, height - 150));
            LocalizedLabel(new Rect(28, 98, 334, 44), "A universe remembered", mobileTitle);
            // Keep the acknowledgements readable at normal size on short screens.
            Rect viewport = new Rect(28, 150, 334, Mathf.Max(1, height - 364));
            const float textWidth = 310;
            string credits = L(CompletionCreditsText);
            string help = L("Reopen these credits using the fairy hexagon on the main page. Reset erases all progress and speed codes.");
            float creditsHeight = LocalizedStyle(mobileText).CalcHeight(new GUIContent(credits), textWidth);
            float helpHeight = LocalizedStyle(mobileSmall).CalcHeight(new GUIContent(help), textWidth);
            completionCreditsScroll = GUI.BeginScrollView(viewport, completionCreditsScroll,
                new Rect(0, 0, textWidth, creditsHeight + helpHeight + 24));
            LocalizedLabel(new Rect(0, 0, textWidth, creditsHeight), credits, mobileText);
            LocalizedLabel(new Rect(0, creditsHeight + 24, textWidth, helpHeight), help, mobileSmall);
            GUI.EndScrollView();
            if (MobileButton(new Rect(28, height - 202, 334, 48), "Close / Continue playing")) CloseCompletionCredits();
            if (MobileButton(new Rect(28, height - 144, 334, 48), "Reset / Start again"))
            {
                CloseCompletionCredits();
                OpenCodePanel();
                SubmitUnlockCode("RESET");
            }
        }
    }
}