using System.Collections.Generic;
using UnityEngine;

namespace Genesis
{
    /// <summary>One asset the boot screen should load, and how urgent it is.</summary>
    public sealed class GenesisBootEntry
    {
        /// <summary>Resources path without extension, for example "Genesis/Art/halos/sun".</summary>
        public string Path;
        /// <summary>0 blocks the first playable frame, 1 is warmed up in the background.</summary>
        public int Tier;
        /// <summary>Size of the source file, used to weight the progress bar.</summary>
        public long Bytes;
    }

    /// <summary>
    /// Reads the manifest that the editor generates from the Resources folder.
    /// The manifest is plain text on purpose: the boot path must not need a JSON
    /// parser before the game is playable, and a text asset parses in microseconds.
    /// </summary>
    public static class GenesisBootManifest
    {
        public const string ResourcePath = "Genesis/Data/boot_manifest";

        /// <summary>Loads the generated manifest, or an empty list when it is missing.</summary>
        public static List<GenesisBootEntry> Load()
        {
            var entries = new List<GenesisBootEntry>();
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null) return entries;
            string[] lines = asset.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                int tier;
                long bytes;
                if (!int.TryParse(parts[0], out tier)) continue;
                long.TryParse(parts[1], out bytes);
                entries.Add(new GenesisBootEntry { Path = parts[2], Tier = tier, Bytes = bytes });
            }
            return entries;
        }
    }
}
