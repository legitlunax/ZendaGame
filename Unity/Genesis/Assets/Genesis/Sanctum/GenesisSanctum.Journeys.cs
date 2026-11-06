using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private const float JourneyContentHeight = 1160;
        private static readonly string[] JourneyDescriptions =
        {
            "Follow lanterns through the morning mist.",
            "Gather silver dew among moonlit flowers.",
            "Sail the constellations beyond the garden."
        };

        private void DrawJourneyDashboard(float y)
        {
            bool busy = ExpeditionRunning || ExpeditionReady;
            int route = busy ? ActiveRoute : SelectedRoute;
            double reward = busy ? ExpeditionReward : NextExpeditionReward;
            DrawEnchantedPanel(new Rect(0, y, 340, 126));
            LocalizedLabel(new Rect(14, y + 10, 312, 28), "Journeys / bonus Hope", mobileTitle);
            LocalizedLabel(new Rect(14, y + 44, 312, 70),
                "Choose a trip, let its timer finish, then claim extra Hope. Free and optional; your fairies keep gathering. No story unlocks are required here.", mobileSmall);

            float cardY = y + 138;
            DrawEnchantedPanel(new Rect(0, cardY, 340, 292));
            LocalizedLabel(new Rect(14, cardY + 10, 312, 24), ExpeditionReady ? "YOUR GIFT IS READY" : ExpeditionRunning ? "JOURNEY IN PROGRESS" : "PLAN YOUR NEXT TRIP", mobileSmall);
            LocalizedLabel(new Rect(14, cardY + 40, 312, 28), RouteNames[route], mobileTitle);
            LocalizedLabel(new Rect(14, cardY + 76, 312, 44), L("From: ") + L(busy ? Dimensions[ExpeditionDimension].Name : ViewedDimension.Name), mobileSmall);
            LocalizedLabel(new Rect(14, cardY + 126, 312, 28), "+" + Num(reward) + " " + L(Game.ResourceName) + L(busy ? " / reserved" : " / reward"), mobileText);
            float progress = ExpeditionReady ? 1 : ExpeditionRunning ? 1 - ExpeditionRemaining / ExpeditionDuration : 0;
            GatheringFill(new Rect(14, cardY + 166, 312, 7), progress, GatheringSage);
            LocalizedLabel(new Rect(14, cardY + 182, 312, 24), ExpeditionReady ? "Trip complete. Claim once to receive your Hope."
                : ExpeditionRunning ? Mathf.CeilToInt(ExpeditionRemaining) + " game seconds left. You can leave this tab."
                : RouteDurations[route] + " game seconds / no entry cost", mobileSmall);
            bool formed = ViewedDimension.IsFormed(Game);
            string caption = ExpeditionReady ? L("Claim +") + Num(reward) + " " + L(Game.ResourceName)
                : ExpeditionRunning ? "Travelling..." : formed ? L("Start ") + L(RouteNames[route]) : "Recruit a fairy first";
            if (MobileButton(new Rect(14, cardY + 224, 312, 50), caption, ExpeditionReady || (!ExpeditionRunning && formed)))
            {
                if (ExpeditionReady) ClaimExpedition(); else BeginExpedition();
            }

            float routesY = y + 444;
            LocalizedLabel(new Rect(12, routesY, 316, 30), "Choose your route", mobileTitle);
            for (int i = 0; i < RouteNames.Length; i++)
            {
                float rowY = routesY + 38 + i * 122;
                bool unlocked = IsRouteUnlocked(i);
                DrawEnchantedPanel(new Rect(0, rowY, 340, 112));
                LocalizedLabel(new Rect(14, rowY + 8, 208, 26), RouteNames[i], mobileText);
                LocalizedLabel(new Rect(14, rowY + 39, 208, 40), JourneyDescriptions[i], mobileSmall);
                LocalizedLabel(new Rect(14, rowY + 82, 312, 22), unlocked
                    ? LF("{0}s / +{1} {2} at current income", RouteDurations[i], Num(JourneyRewardForRoute(i)), L(Game.ResourceName))
                    : "Unlock after " + i * 3 + " claimed trips (" + ExpeditionWins + "/" + i * 3 + ")", mobileSmall);
                if (MobileButton(new Rect(232, rowY + 18, 94, 48), !unlocked ? "Locked" : route == i ? "Selected" : "Choose",
                    unlocked && !busy && route != i)) SelectRoute(i);
            }

            float bondY = y + 860;
            DrawEnchantedPanel(new Rect(0, bondY, 340, 284));
            LocalizedLabel(new Rect(14, bondY + 10, 312, 28), "Travel bond / rank " + BondRank, mobileTitle);
            LocalizedLabel(new Rect(14, bondY + 48, 312, 80), LF("{0} trips claimed. Every 3 claims adds 10% to future journey rewards. Current bonus: +{1}%.", ExpeditionWins, (BondRank - 1) * 10), mobileSmall);
            GatheringFill(new Rect(14, bondY + 138, 312, 7), (ExpeditionWins % 3) / 3f, GatheringRose);
            LocalizedLabel(new Rect(14, bondY + 158, 312, 114), LF("{0} more claims to the next rank. Timers use game speed and advance offline. Longer routes pay more as your income grows; rewards are fixed when you start.", 3 - ExpeditionWins % 3), mobileSmall);
        }
    }
}