using UnityEngine;

namespace EnchantedUI
{
    // Only attached to the example scene. Not attached to production prefabs.
    public sealed class EnchantedDemoStory : MonoBehaviour
    {
        public EnchantedDialogueView view;
        private int index;
        private static readonly string[] Lines =
        {
            "Then Lady Riyara arrived like the warmth of a sunrise. I had learned how to shelter a spark; now I had to let it shine beyond my hands.",
            "These are editable Unity text components. Assign your own character sprites to the portrait images and connect Continue to your existing story system.",
            "Your world background stays separate from the interface. This sample uses a lightweight gradient so you can replace it with any of your world's illustrations.\n\nLonger dialogue scrolls inside its own panel. On short desktop windows, the whole page can scroll as well.",
            "El mismo kit admite español: esperanza, corazón, diálogo y próximo capítulo. Los adornos no bloquean los botones.\n\nThis is the final sample line. Previous remains available."
        };
        private void OnEnable()
        {
            if (view == null) return;
            view.continueRequested.AddListener(Next);
            view.previousRequested.AddListener(Previous);
            Show();
        }
        private void OnDisable()
        {
            if (view == null) return;
            view.continueRequested.RemoveListener(Next);
            view.previousRequested.RemoveListener(Previous);
        }
        private void Next() { index = Mathf.Min(index + 1, Lines.Length - 1); Show(); }
        private void Previous() { index = Mathf.Max(index - 1, 0); Show(); }
        private void Show()
        {
            view.SetLine("Zenda / happy / " + (index + 1) + " of " + Lines.Length,
                Lines[index], index > 0, index < Lines.Length - 1);
        }
    }
}
