using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public const int DesktopWindowWidth = 390;
        public const int DesktopWindowHeight = 700;

        public static Vector2Int DesktopWindowSize(int displayWidth, int displayHeight)
        {
            // Leave room for the OS title bar and taskbar on smaller displays.
            float scale = Mathf.Min(1f, Mathf.Max(1, displayWidth - 64) / (float)DesktopWindowWidth,
                Mathf.Max(1, displayHeight - 100) / (float)DesktopWindowHeight);
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(DesktopWindowWidth * scale)),
                Mathf.Max(1, Mathf.RoundToInt(DesktopWindowHeight * scale)));
        }

        private void ConfigureDesktopWindow()
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            // The window is created at the size that the build settings bake
            // in and then shaped like a phone by the bootstrap. Resizing it
            // from Awake rebuilds the swap chain while the first frame is
            // still being presented, which leaves a broken window behind on
            // some drivers. Only report a mismatch: the window is resizable
            // and the layout scales itself to whatever size it has.
            if (Screen.width != DesktopWindowWidth || Screen.height != DesktopWindowHeight)
                Debug.Log("[Genesis] window is " + Screen.width + "x" + Screen.height
                    + "; the layout scales itself to the " + DesktopWindowWidth + "x" + DesktopWindowHeight + " design.");
#endif
        }
    }
}
