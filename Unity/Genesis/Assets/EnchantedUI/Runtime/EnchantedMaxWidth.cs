using UnityEngine;

namespace EnchantedUI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public sealed class EnchantedMaxWidth : MonoBehaviour
    {
        [Min(200)] public float maxWidth = 1040f;
        private float lastWidth = -1f;
        private float lastMaximum = -1f;

        private void OnEnable() { Refresh(); }
        private void Update() { Refresh(); }
        private void Refresh()
        {
            RectTransform parent = transform.parent as RectTransform;
            if (parent == null) return;
            float width = parent.rect.width;
            if (Mathf.Approximately(width, lastWidth) && Mathf.Approximately(maxWidth, lastMaximum)) return;
            lastWidth = width;
            lastMaximum = maxWidth;
            float inset = Mathf.Max(0, (width - maxWidth) * 0.5f);
            RectTransform target = (RectTransform)transform;
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.offsetMin = new Vector2(inset, 0);
            target.offsetMax = new Vector2(-inset, 0);
        }
    }
}
