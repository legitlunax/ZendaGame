using UnityEngine;

namespace EnchantedUI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public sealed class EnchantedSafeArea : MonoBehaviour
    {
        private Rect lastSafe;
        private Vector2Int lastSize;

        private void OnEnable() { Apply(); }
        private void Update()
        {
            if (lastSafe != Screen.safeArea || lastSize.x != Screen.width || lastSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastSafe = Screen.safeArea;
            lastSize = new Vector2Int(Screen.width, Screen.height);
            RectTransform target = (RectTransform)transform;
            // Attach to a direct child of a full-screen overlay Canvas.
            target.anchorMin = new Vector2(lastSafe.xMin / Screen.width, lastSafe.yMin / Screen.height);
            target.anchorMax = new Vector2(lastSafe.xMax / Screen.width, lastSafe.yMax / Screen.height);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }
    }
}
