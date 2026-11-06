using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EnchantedUI
{
    // Presentation only. Connect the events to your existing story controller.
    public sealed class EnchantedDialogueView : MonoBehaviour
    {
        public Text title;
        public Text resource;
        public Text chapter;
        public Text speaker;
        public Text body;
        public Text leftName;
        public Text rightName;
        public Image leftPortrait;
        public Image rightPortrait;
        public Image backdrop;
        public ScrollRect dialogueScroll;
        public Button continueButton;
        public Button previousButton;
        public Button backButton;
        public UnityEvent continueRequested = new UnityEvent();
        public UnityEvent previousRequested = new UnityEvent();
        public UnityEvent backRequested = new UnityEvent();

        private void OnEnable()
        {
            if (continueButton != null) continueButton.onClick.AddListener(ForwardContinue);
            if (previousButton != null) previousButton.onClick.AddListener(ForwardPrevious);
            if (backButton != null) backButton.onClick.AddListener(ForwardBack);
        }
        private void OnDisable()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(ForwardContinue);
            if (previousButton != null) previousButton.onClick.RemoveListener(ForwardPrevious);
            if (backButton != null) backButton.onClick.RemoveListener(ForwardBack);
        }
        private void ForwardContinue() { continueRequested.Invoke(); }
        private void ForwardPrevious() { previousRequested.Invoke(); }
        private void ForwardBack() { backRequested.Invoke(); }

        public void SetLine(string speakerText, string dialogue, bool canGoBack, bool canContinue)
        {
            if (speaker != null) speaker.text = speakerText ?? string.Empty;
            if (body != null) body.text = dialogue ?? string.Empty;
            if (previousButton != null) previousButton.interactable = canGoBack;
            if (continueButton != null) continueButton.interactable = canContinue;
            Canvas.ForceUpdateCanvases();
            if (dialogueScroll != null) dialogueScroll.verticalNormalizedPosition = 1f;
        }

        public void SetPortraits(Sprite left, string leftCaption, Sprite right, string rightCaption)
        {
            SetPortrait(leftPortrait, left);
            SetPortrait(rightPortrait, right);
            if (leftName != null) leftName.text = leftCaption ?? string.Empty;
            if (rightName != null) rightName.text = rightCaption ?? string.Empty;
        }
        private static void SetPortrait(Image target, Sprite sprite)
        {
            if (target == null) return;
            target.sprite = sprite;
            target.enabled = sprite != null;
        }
        public void SetBackground(Sprite sprite)
        {
            if (backdrop == null) return;
            backdrop.sprite = sprite;
            backdrop.enabled = sprite != null;
            // Center-crop to fill without distorting the world's aspect ratio.
            AspectRatioFitter fit = backdrop.GetComponent<AspectRatioFitter>();
            if (fit != null && sprite != null && sprite.rect.height > 0)
                fit.aspectRatio = sprite.rect.width / sprite.rect.height;
        }
    }
}
