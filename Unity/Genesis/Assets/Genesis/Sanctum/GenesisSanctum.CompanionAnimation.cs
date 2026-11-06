using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public const float SocialSadAfter = 90f;
        public const float SocialPoutAfter = 180f;
        public float SecondsWithoutInvocation { get; private set; }
        public string SocialMood => socialAffectionRemaining > 0 || socialWelcomeRemaining > 0 ? "happy"
            : SecondsWithoutInvocation >= SocialPoutAfter ? "angry"
            : SecondsWithoutInvocation >= SocialSadAfter ? "sad"
            : Inspired ? "happy" : "relaxed";
        public string SocialGesture => socialAffectionRemaining > 0 ? socialAffection
            : socialWelcomeRemaining > 0 ? "welcome" : "idle";
        public float SocialAnimationTime { get; private set; }
        private float socialAffectionRemaining, socialWelcomeRemaining;
        private string socialAffection = "kiss";
        private bool fullInspirationCelebrated, nextAffectionHug;
        private Texture2D companionHeart, companionEmote;
        private string companionEmoteKey;
        private bool companionPointerDown, companionPointerDragged;
        private Vector2 companionPointerStart;

        private void AdvanceSocialAnimation(float seconds)
        {
            // Time spent reading, exploring menus or away from the app is not neglect.
            if (SelectedTab != 0) return;
            SocialAnimationTime = Mathf.Repeat(SocialAnimationTime + seconds, 120f);
            SecondsWithoutInvocation = Mathf.Min(SocialPoutAfter, SecondsWithoutInvocation + seconds);
            socialAffectionRemaining = Mathf.Max(0, socialAffectionRemaining - seconds);
            socialWelcomeRemaining = Mathf.Max(0, socialWelcomeRemaining - seconds);
            if (InspirationRemaining < MaxInspirationSeconds * 0.8f) fullInspirationCelebrated = false;
        }

        private void OnSocialInvocation()
        {
            SecondsWithoutInvocation = 0;
            socialWelcomeRemaining = 2.5f;
            if (InspirationRemaining < MaxInspirationSeconds || fullInspirationCelebrated) return;
            fullInspirationCelebrated = true;
            socialAffection = nextAffectionHug ? "hug" : "kiss";
            nextAffectionHug = !nextAffectionHug;
            socialAffectionRemaining = 4f;
        }

        private void ApplySocialPresentation()
        {
            if (goddess == null || !goddess.IsReady) return;
            bool active = SelectedTab == 0 && !StorytellingActive;
            if (active && reactionRemaining > 0 && selectedGoddessReaction != null)
            {
                goddess.SetAffection("idle", 0);
                goddess.SetMood(selectedGoddessReaction);
                return;
            }
            goddess.SetAffection(active ? SocialGesture : "idle", active && socialAffectionRemaining > 0 ? 1f : 0f);
            if (active && (reactionRemaining <= 0 || SocialGesture != "idle")) goddess.SetMood(SocialMood);
        }

        public static Rect LadyCompanionHitRect(float height)
        { return new Rect(218, height - 318, 144, 150); }

        public static Rect AnimatedLadyCompanionRect(float height, float time, string mood, string gesture)
        {
            Rect rect = LadyCompanionRect(height);
            float bounce = mood == "happy" ? 7 : mood == "sad" ? 2 : 4;
            rect.y += Mathf.Sin(time * (mood == "happy" ? 4 : 2)) * bounce;
            rect.x += gesture == "hug" ? -8 * Mathf.Abs(Mathf.Sin(time * 2)) : Mathf.Sin(time * 1.5f) * 3;
            float pulse = 1 + Mathf.Sin(time * 3) * (gesture == "idle" ? 0.025f : 0.06f);
            rect = new Rect(rect.center.x - rect.width * pulse / 2, rect.center.y - rect.height / pulse / 2,
                rect.width * pulse, rect.height / pulse);
            return rect;
        }

        private Texture2D MakeCompanionSymbol(bool heart, string mood)
        {
            const int size = 48;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { name = heart ? "Companion affection heart" : "Companion expression " + mood, filterMode = FilterMode.Bilinear };
            Color ink = new Color(0.18f, 0.1f, 0.2f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float px = (x + 0.5f - 24) / 21f, py = (y + 0.5f - 24) / 21f;
                    Color color = Color.clear;
                    if (heart)
                    {
                        float a = px * px + py * py - 0.65f;
                        if (a * a * a - px * px * py * py * py < 0) color = new Color(1, 0.38f, 0.64f);
                    }
                    else if (px * px + py * py < 0.92f)
                    {
                        color = mood == "sad" ? new Color(0.68f, 0.83f, 1)
                            : mood == "angry" ? new Color(1, 0.71f, 0.64f) : new Color(1, 0.89f, 0.58f);
                        bool eye = Mathf.Abs(Mathf.Abs(px) - 0.34f) < 0.075f && Mathf.Abs(py - 0.2f) < 0.12f;
                        float mouth = mood == "sad" ? -0.28f - px * px * 0.7f : -0.5f + px * px * 0.9f;
                        bool lips = Mathf.Abs(px) < 0.45f && Mathf.Abs(py - mouth) < 0.055f;
                        bool brow = mood == "angry" && Mathf.Abs(px) > 0.17f && Mathf.Abs(px) < 0.53f
                            && Mathf.Abs(py - (0.32f + Mathf.Abs(px) * 0.45f)) < 0.055f;
                        if (eye || lips || brow) color = ink;
                        if (mood == "sad" && px > 0.43f && px < 0.55f && py < 0.05f && py > -0.25f)
                            color = new Color(0.2f, 0.5f, 0.95f);
                    }
                    texture.SetPixel(x, y, color);
                }
            texture.Apply();
            return texture;
        }

        private void DrawCompanionAnimation(float height, Texture2D icon)
        {
            string mood = SocialMood, gesture = SocialGesture;
            Rect rect = AnimatedLadyCompanionRect(height, SocialAnimationTime, mood, gesture);
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            try
            {
                GUIUtility.RotateAroundPivot(Mathf.Sin(SocialAnimationTime * (mood == "angry" ? 8 : 2)) * (mood == "angry" ? 7 : 4), rect.center);
                if (icon != null) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
            }
            finally { GUI.matrix = previousMatrix; }
            if (companionEmoteKey != mood || companionEmote == null)
            {
                if (companionEmote != null) Destroy(companionEmote);
                companionEmote = MakeCompanionSymbol(false, mood);
                companionEmoteKey = mood;
            }
            GUI.DrawTexture(new Rect(rect.xMax - 22, rect.y - 5, 28, 28), companionEmote);
            if (gesture != "idle")
            {
                if (companionHeart == null) companionHeart = MakeCompanionSymbol(true, mood);
                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float t = Mathf.Repeat(SocialAnimationTime * 0.55f + i * 0.25f, 1);
                        float x = Mathf.Lerp(rect.center.x, 174, t);
                        float y = rect.center.y - Mathf.Sin(t * Mathf.PI) * (gesture == "hug" ? 44 : 28);
                        GUI.color = new Color(1, 1, 1, Mathf.Sin(t * Mathf.PI));
                        GUI.DrawTexture(new Rect(x - 10, y - 10, 20 + t * 10, 20 + t * 10), companionHeart);
                    }
                }
                finally { GUI.color = previousColor; }
            }
        }

        private void HandleCompanionPointer(float height, string name, Event input)
        {
            if (input == null || GoddessPopupOpen || !GUI.enabled) return;
            Rect hit = LadyCompanionHitRect(height);
            if (input.type == EventType.MouseDown && hit.Contains(input.mousePosition))
            {
                companionPointerDown = true;
                companionPointerDragged = false;
                companionPointerStart = input.mousePosition;
                input.Use();
            }
            else if (input.type == EventType.MouseDrag && companionPointerDown)
            {
                if ((input.mousePosition - companionPointerStart).sqrMagnitude > 64) companionPointerDragged = true;
                input.Use();
            }
            else if (input.type == EventType.MouseUp && companionPointerDown)
            {
                bool select = !companionPointerDragged && !dragging && hit.Contains(input.mousePosition)
                    && (input.mousePosition - companionPointerStart).sqrMagnitude <= 64;
                companionPointerDown = false;
                input.Use();
                if (select) SelectLady(name);
            }
        }

        private void ReleaseCompanionAnimation()
        {
            if (companionHeart != null) Destroy(companionHeart);
            if (companionEmote != null) Destroy(companionEmote);
        }
    }
}