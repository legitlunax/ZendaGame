using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private IReadOnlyList<GenesisDimensionLayer> dimensions;
        public IReadOnlyList<GenesisDimensionLayer> Dimensions
        {
            get
            {
                if (dimensions == null && Game != null) dimensions = GenesisDimensions.Build(Game.Catalog);
                return dimensions;
            }
            private set { dimensions = value; }
        }
        public int SelectedDimension { get; private set; }
        public int ExpeditionDimension { get; private set; }
        private bool dimensionMapExpanded;
        public GenesisDimensionLayer ViewedDimension => Dimensions[SelectedDimension];
        public int FormedDimensionCount => Dimensions.Count(d => d.IsFormed(Game));
        private string pendingWorldGenerator;
        public bool HasPendingWorldUnlock => pendingWorldGenerator != null;

        private bool RedirectToRequiredWorldStory(string generatorId)
        {
            ChapterDefinition required = Game.RequiredWorldStory(generatorId);
            if (required == null || !Game.IsDiscovered(generatorId)) return false;
            pendingWorldGenerator = generatorId;
            message = "Finish the previous world's storyline before unlocking a new world: " + required.title;
            if (!OpenChronicle(required.id))
            {
                CloseLadyGallery();
                CloseChronicle();
                SelectTab(2);
            }
            return true;
        }

        public bool ContinueWorldUnlockStory()
        {
            if (pendingWorldGenerator == null) return false;
            ChapterDefinition required = Game.RequiredWorldStory(pendingWorldGenerator);
            return required != null && OpenChronicle(required.id);
        }

        public bool ReturnToWorldUnlock()
        {
            if (pendingWorldGenerator == null) return false;
            string generatorId = pendingWorldGenerator;
            pendingWorldGenerator = null;
            CloseChronicle();
            SelectTab(1);
            SelectGatheringSection(0);
            gatheringWorld = -1;
            FocusGatheringFairy(generatorId);
            message = Game.RequiredWorldStory(generatorId) == null
                ? "Storyline finished. You can now unlock the next world."
                : "Complete the previous world's story objectives, then finish reading before unlocking.";
            return true;
        }

        public bool SelectDimension(int index)
        {
            if (index < 0 || index >= Dimensions.Count) return false;
            SelectedDimension = index;
            chronicleScroll = Vector2.zero;
            return true;
        }

        private float DimensionHeaderHeight => dimensionMapExpanded ? 184 + Dimensions.Count * 66 : 184;

        private void DrawDimensionHeader()
        {
            LocalizedLabel(new Rect(4, 0, 336, 26), "DIMENSION PROGRESSION", mobileTitle);
            LocalizedLabel(new Rect(4, 30, 336, 40), FormedDimensionCount + " Dimensions formed / production continues\nCo-located universes have distinct laws and time.", mobileSmall);
            if (MobileButton(new Rect(0, 78, 48, 44), "<")) SelectDimension(Mathf.Max(0, SelectedDimension - 1));
            if (MobileButton(new Rect(54, 78, 232, 44), ViewedDimension.Name)) dimensionMapExpanded = !dimensionMapExpanded;
            if (MobileButton(new Rect(292, 78, 48, 44), ">")) SelectDimension(Mathf.Min(Dimensions.Count - 1, SelectedDimension + 1));
            LocalizedLabel(new Rect(4, 130, 336, 46), ViewedDimension.Remembered(Game) + "/" + ViewedDimension.Chapters.Count +
                " chapters / " + Num(ViewedDimension.Income(Game)) + " per sec\n" +
                (ViewedDimension.IsFormed(Game) ? "Formed / upgrade discoveries to unlock Chronicles" : "Unformed / discover its first creation"), mobileSmall);
            if (!dimensionMapExpanded) return;
            for (int i = 0; i < Dimensions.Count; i++)
            {
                GenesisDimensionLayer layer = Dimensions[i];
                string state = layer.IsFormed(Game) ? "Formed" : "Unformed";
                if (MobileButton(new Rect(0, 184 + i * 66, 340, 58), layer.Name + "\n" + state + " / " + layer.Remembered(Game) + "/" + layer.Chapters.Count + " chapters"))
                {
                    SelectDimension(i);
                    dimensionMapExpanded = false;
                }
            }
        }
    }
}