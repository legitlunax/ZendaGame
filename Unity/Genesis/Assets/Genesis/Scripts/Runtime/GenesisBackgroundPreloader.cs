using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Genesis
{
    /// <summary>
    /// Keeps warming up assets after the game is already interactive.
    ///
    /// The gallery, the story chapters and the thirty painted worlds are only
    /// needed when the player actually opens them, but each one is a large
    /// texture that used to be decoded inside a single frame. Loading them
    /// gently in the background moves that cost away from the frame the player
    /// is waiting for, so no screen ever locks up when it opens for the first
    /// time.
    ///
    /// The work is capped by both a per-frame time budget and a total byte
    /// budget: a phone should not have to hold the whole art library in RAM.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GenesisBackgroundPreloader : MonoBehaviour
    {
        private const long DesktopByteBudget = 64L * 1024L * 1024L;
        private const long MobileByteBudget = 24L * 1024L * 1024L;
        private const long WebByteBudget = 16L * 1024L * 1024L;

        private static GenesisBackgroundPreloader instance;

        private readonly List<GenesisBootEntry> entries = new List<GenesisBootEntry>();
        private int entryIndex;
        private long byteBudget;
        private long bytesLoaded;
        private bool running;
        private string bootSceneName = string.Empty;

        /// <summary>Fraction of the deferred tier that is already resident.</summary>
        public static float Progress { get; private set; }

        /// <summary>Creates the persistent preloader, if it does not exist yet.</summary>
        public static GenesisBackgroundPreloader Create(string bootSceneName)
        {
            if (instance != null) return instance;
            var host = new GameObject("Genesis background preloader");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<GenesisBackgroundPreloader>();
            instance.bootSceneName = bootSceneName ?? string.Empty;
            return instance;
        }

        private void Start()
        {
            byteBudget = ByteBudgetForPlatform();
            List<GenesisBootEntry> manifest = GenesisBootManifest.Load();
            for (int i = 0; i < manifest.Count; i++)
                if (manifest[i].Tier == 1) entries.Add(manifest[i]);
        }

        /// <summary>
        /// How many bytes of art the background may hold at once.
        ///
        /// The budget is the size of the files on disk, but a picture that is
        /// three megabytes on disk is six megabytes of memory once it is
        /// decoded. A desktop player has the room for it, a phone and a
        /// browser do not, and a browser that runs out of memory during the
        /// boot stops moving altogether. The smaller budgets keep the
        /// device alive and the art that is left out is simply loaded when
        /// the player asks for it.
        /// </summary>
        private static long ByteBudgetForPlatform()
        {
#if UNITY_WEBGL
            return WebByteBudget;
#elif UNITY_ANDROID || UNITY_IOS
            return MobileByteBudget;
#else
            return DesktopByteBudget;
#endif
        }

        private void Update()
        {
            if (!running)
            {
                // Stay idle while the boot scene is still on screen: the loading
                // screen must not compete with the critical preload.
                if (bootSceneName.Length > 0 && SceneManager.GetActiveScene().name == bootSceneName) return;
                running = true;
            }
            // Do not spend the players battery on assets nobody asked for yet.
            if (!Application.isFocused) return;
            RunSlice();
        }

        private void RunSlice()
        {
            if (entryIndex >= entries.Count) return;
            float deadline = Time.realtimeSinceStartup + 0.003f;
            while (entryIndex < entries.Count)
            {
                GenesisBootEntry entry = entries[entryIndex];
                entryIndex++;
                if (bytesLoaded + entry.Bytes > byteBudget) continue;
                UnityEngine.Object asset = Resources.Load(entry.Path);
                if (asset == null) continue;
                GenesisPreloadCache.Hold(entry.Path, asset);
                bytesLoaded += entry.Bytes;
                Progress = entries.Count > 0 ? (float)entryIndex / entries.Count : 1f;
                if (Time.realtimeSinceStartup >= deadline) return;
            }
            Progress = 1f;
        }
    }
}
