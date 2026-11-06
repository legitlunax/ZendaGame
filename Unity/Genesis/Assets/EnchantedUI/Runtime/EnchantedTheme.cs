using UnityEngine;

namespace EnchantedUI
{
    [CreateAssetMenu(menuName = "Enchanted UI/Theme", fileName = "EnchantedTheme")]
    public sealed class EnchantedTheme : ScriptableObject
    {
        public Sprite panel;
        public Sprite button;
        public Sprite blossom;
        public Sprite hexSocket;
        public Sprite progressTrack, progressFill, divider, spark, diamond, check, back, next;
        public Font titleFont;
        public Font bodyFont;
        public Color text = new Color32(255, 244, 225, 255);
        public Color mutedText = new Color32(214, 195, 222, 255);
        public Color gold = new Color32(240, 204, 145, 255);
        public Color secondaryTint = new Color32(197, 183, 213, 255);
        public Color disabledTint = new Color32(125, 120, 132, 200);
        public Color backgroundTop = new Color32(57, 35, 81, 255);
        public Color backgroundBottom = new Color32(17, 12, 30, 255);
        [Min(0.1f)] public float panelSliceScale = 5f;
        [Min(0.1f)] public float buttonSliceScale = 8f;
    }
}
