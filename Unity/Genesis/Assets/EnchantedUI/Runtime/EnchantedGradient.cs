using UnityEngine;
using UnityEngine.UI;

namespace EnchantedUI
{
    // Four vertices, no custom shader, no per-frame texture generation.
    [AddComponentMenu("UI/Enchanted Gradient")]
    public sealed class EnchantedGradient : MaskableGraphic
    {
        public Color top = new Color32(57, 35, 81, 255);
        public Color bottom = new Color32(17, 12, 30, 255);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            vh.AddVert(new Vector3(r.xMin, r.yMin), bottom * color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), top * color, Vector2.up);
            vh.AddVert(new Vector3(r.xMax, r.yMax), top * color, Vector2.one);
            vh.AddVert(new Vector3(r.xMax, r.yMin), bottom * color, Vector2.right);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
