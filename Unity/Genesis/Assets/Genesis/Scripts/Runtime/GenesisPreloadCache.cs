using System.Collections.Generic;
using UnityEngine;

namespace Genesis
{
    /// <summary>
    /// Keeps assets that were preloaded during boot resident for the whole session.
    /// Unity frees a Resources asset as soon as nothing references it, so the boot
    /// screen has to hold on to whatever it warmed up. Every later Resources.Load
    /// for one of these paths then resolves from memory instead of touching the APK
    /// again, which is what used to freeze the game on Android's first launch.
    /// </summary>
    public static class GenesisPreloadCache
    {
        private static readonly List<Object> held = new List<Object>();
        private static readonly Dictionary<string, Object> byPath = new Dictionary<string, Object>();

        /// <summary>Number of assets currently kept alive by the cache.</summary>
        public static int HeldCount { get { return held.Count; } }

        /// <summary>Keeps an already loaded asset alive for the rest of the session.</summary>
        public static void Hold(string path, Object asset)
        {
            if (asset == null) return;
            if (!byPath.ContainsKey(path)) byPath[path] = asset;
            held.Add(asset);
        }

        /// <summary>Loads a resource once and keeps it resident for the session.</summary>
        public static T Load<T>(string path) where T : Object
        {
            Object cached;
            if (byPath.TryGetValue(path, out cached))
            {
                T typed = cached as T;
                if (typed != null) return typed;
            }
            T loaded = Resources.Load<T>(path);
            Hold(path, loaded);
            return loaded;
        }

        /// <summary>Drops every cached reference. Only used by editor tooling.</summary>
        public static void Release()
        {
            held.Clear();
            byPath.Clear();
        }
    }
}
