using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnchantedUI
{
    [RequireComponent(typeof(Button))]
    public sealed class EnchantedButtonFeedback : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerExitHandler, IDeselectHandler
    {
        public RectTransform artwork;
        public Text label;
        [Range(0.85f, 1f)] public float pressedScale = 0.97f;
        private Button button;
        private Vector3 originalScale = Vector3.one;
        private Color originalText = Color.white;
        private bool wasInteractable = true;
        private bool initialized;

        private void Awake() { Initialize(); }
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            button = GetComponent<Button>();
            if (artwork != null) originalScale = artwork.localScale;
            if (label != null) originalText = label.color;
            wasInteractable = button.IsInteractable();
            RefreshLabel();
        }
        private void OnEnable() { Initialize(); Restore(); RefreshLabel(); }
        private void OnDisable() { Restore(); }
        private void Update()
        {
            bool current = button.IsInteractable();
            if (current == wasInteractable) return;
            wasInteractable = current;
            Restore();
            RefreshLabel();
        }
        private void RefreshLabel()
        {
            if (label == null || button == null) return;
            Color c = originalText;
            if (!button.IsInteractable()) c.a *= 0.5f;
            label.color = c;
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left && button.IsInteractable() && artwork != null)
                artwork.localScale = originalScale * pressedScale;
        }
        public void OnPointerUp(PointerEventData e) { Restore(); }
        public void OnPointerExit(PointerEventData e) { Restore(); }
        public void OnDeselect(BaseEventData e) { Restore(); }
        private void Restore() { if (artwork != null) artwork.localScale = originalScale; }
    }
}
