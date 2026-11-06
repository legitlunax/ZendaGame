using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public bool LadyGalleryOpen { get; private set; }
        public IReadOnlyList<GenesisLadyGalleryEntry> MetLadies => GenesisLadyGallery.Build(Game);
        private Vector2 ladyGalleryScroll;
        private readonly Dictionary<string, Texture2D> ladyGalleryIcons = new Dictionary<string, Texture2D>();
        private const float LadyCardHeight = 286;
        private string selectedLadyName, companionLadyName;
        private Vector2 ladyHistoryScroll;
        public GenesisLadyGalleryEntry SelectedLady => MetLadies.FirstOrDefault(l => l.Name == selectedLadyName);
        public GenesisLadyGalleryEntry CompanionLady => MetLadies.FirstOrDefault(l => l.Name == companionLadyName);

        public bool SelectLady(string name)
        {
            name = Game.Catalog.ResolveFairyName(name);
            if (!MetLadies.Any(l => l.Name == name)) return false;
            selectedLadyName = name;
            ladyHistoryScroll = Vector2.zero;
            dragging = false;
            scrollPointerDown = false;
            OpenLadyGallery();
            return true;
        }

        public bool EquipLadyCompanion(string name)
        {
            name = Game.Catalog.ResolveFairyName(name);
            if (!MetLadies.Any(l => l.Name == name)) return false;
            companionLadyName = name;
            companionPointerDown = false;
            SavePlayerProgress();
            return true;
        }

        public void UnequipLadyCompanion() { companionLadyName = null; companionPointerDown = false; SavePlayerProgress(); }

        private Texture2D LadyIcon(GenesisLadyGalleryEntry lady)
        {
            if (!ladyGalleryIcons.TryGetValue(lady.IconId, out Texture2D icon))
            {
                icon = Resources.Load<Texture2D>("Genesis/Art/icons/" + lady.IconId);
                ladyGalleryIcons[lady.IconId] = icon;
            }
            return icon;
        }

        public void OpenLadyGallery()
        {
            CloseChronicle();
            LadyGalleryOpen = true;
            SelectTab(2);
        }

        public void CloseLadyGallery()
        {
            LadyGalleryOpen = false;
            dragging = false;
            scrollPointerDown = false;
        }

        private void DrawLadyGallery(float height)
        {
            GenesisLadyGalleryEntry selected = SelectedLady;
            if (selected != null) { DrawLadyHistory(height, selected); return; }
            var ladies = MetLadies;
            DrawReadingSurface(new Rect(12, 126, 366, 44));
            LocalizedLabel(new Rect(16, 128, 358, 40), ladies.Count + " Ladies met / chibi fairy collection", mobileText);
            Rect viewport = new Rect(12, 174, 366, height - 250);
            DrawReadingSurface(viewport);
            float contentHeight = Mathf.Max(viewport.height, Mathf.Ceil(ladies.Count / 2f) * LadyCardHeight);
            Event input = Event.current;
            if (input.type == EventType.MouseDown && viewport.Contains(input.mousePosition))
            {
                dragStart = input.mousePosition;
                dragScrollStart = ladyGalleryScroll.y;
                scrollPointerDown = true;
            }
            if (input.type == EventType.MouseDrag && scrollPointerDown && (input.mousePosition - dragStart).magnitude > 8)
            {
                dragging = true;
                ladyGalleryScroll.y = Mathf.Clamp(dragScrollStart + dragStart.y - input.mousePosition.y, 0, Mathf.Max(0, contentHeight - viewport.height));
                input.Use();
            }
            bool release = input.type == EventType.MouseUp;
            ladyGalleryScroll = GUI.BeginScrollView(viewport, ladyGalleryScroll, new Rect(0, 0, 344, contentHeight));
            if (ladies.Count == 0)
                LocalizedLabel(new Rect(16, 24, 308, 100), "Your first friend is waiting. Unlock a creation in Idle RPG to welcome its Lady here.", mobileText);
            for (int i = 0; i < ladies.Count; i++)
            {
                GenesisLadyGalleryEntry lady = ladies[i];
                float x = i % 2 * 172, y = i / 2 * LadyCardHeight;
                DrawEnchantedPanel(new Rect(x, y, 166, LadyCardHeight - 8));
                Texture2D icon = LadyIcon(lady);
                if (icon != null) GUI.DrawTexture(new Rect(x + 35, y + 8, 96, 96), icon, ScaleMode.ScaleToFit);
                else LocalizedLabel(new Rect(x + 12, y + 30, 142, 60), "Chibi portrait unavailable", mobileSmall);
                LocalizedLabel(new Rect(x + 8, y + 108, 150, 40), lady.Name, mobileText);
                LocalizedLabel(new Rect(x + 8, y + 150, 150, 40), lady.Home, mobileSmall);
                LocalizedLabel(new Rect(x + 8, y + 192, 150, 38), string.IsNullOrWhiteSpace(lady.Title) ? "Zenda's friend" : lady.Title, mobileSmall);
                if (MobileButton(new Rect(x + 8, y + 234, 150, 44), companionLadyName == lady.Name ? "Friend / History" : "Select / History"))
                {
                    SelectLady(lady.Name);
                    break;
                }
            }
            GUI.EndScrollView();
            if (release) { dragging = false; scrollPointerDown = false; }
        }

        private void DrawLadyHistory(float height, GenesisLadyGalleryEntry lady)
        {
            if (MobileButton(new Rect(16, 128, 358, 44), "< All Ladies"))
            {
                selectedLadyName = null;
                dragging = false;
                scrollPointerDown = false;
                return;
            }
            Texture2D icon = LadyIcon(lady);
            DrawReadingSurface(new Rect(12, 176, 366, 96));
            if (icon != null) GUI.DrawTexture(new Rect(16, 180, 88, 88), icon, ScaleMode.ScaleToFit);
            LocalizedLabel(new Rect(114, 180, 260, 42), lady.Name, mobileText);
            LocalizedLabel(new Rect(114, 224, 260, 44), L(lady.Home) + (string.IsNullOrWhiteSpace(lady.Title) ? "" : "\n" + L(lady.Title)), mobileSmall);
            Rect viewport = new Rect(20, 280, 350, height - 466);
            DrawReadingSurface(new Rect(12, viewport.y - 4, 366, viewport.height + 8));
            float textHeight = Mathf.Max(viewport.height, LocalizedStyle(mobileText).CalcHeight(new GUIContent(L(lady.History)), 326));
            Event input = Event.current;
            if (input.type == EventType.MouseDown && viewport.Contains(input.mousePosition))
            {
                dragStart = input.mousePosition;
                dragScrollStart = ladyHistoryScroll.y;
                scrollPointerDown = true;
            }
            if (input.type == EventType.MouseDrag && scrollPointerDown && (input.mousePosition - dragStart).magnitude > 8)
            {
                dragging = true;
                ladyHistoryScroll.y = Mathf.Clamp(dragScrollStart + dragStart.y - input.mousePosition.y, 0, Mathf.Max(0, textHeight - viewport.height));
                input.Use();
            }
            bool release = input.type == EventType.MouseUp;
            ladyHistoryScroll = GUI.BeginScrollView(viewport, ladyHistoryScroll, new Rect(0, 0, 326, textHeight));
            LocalizedLabel(new Rect(0, 0, 326, textHeight), lady.History, mobileText);
            GUI.EndScrollView();
            bool equipped = companionLadyName == lady.Name;
            if (MobileButton(new Rect(16, height - 176, 358, 44), equipped ? "Send companion home" : "Accompany Zenda as a friend"))
            {
                if (equipped) UnequipLadyCompanion(); else EquipLadyCompanion(lady.Name);
            }
            if (MobileButton(new Rect(16, height - 126, 174, 44), "Visit Zenda")) SelectTab(0);
            if (MobileButton(new Rect(198, height - 126, 176, 44), "Full story", lady.ChapterId != null && IsChronicleUnlocked(lady.ChapterId))) OpenChronicle(lady.ChapterId);
            if (release) { dragging = false; scrollPointerDown = false; }
        }

        public static Rect LadyCompanionRect(float height)
        { return new Rect(244, height - 292, 104, 104); }

        private void DrawLadyCompanion(float height)
        {
            GenesisLadyGalleryEntry lady = CompanionLady;
            if (lady == null) return;
            Texture2D icon = LadyIcon(lady);
            DrawCompanionAnimation(height, icon);
            HandleCompanionPointer(height, lady.Name, Event.current);
        }
    }
}