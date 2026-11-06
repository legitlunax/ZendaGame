using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Genesis.EditorTools
{
    /// <summary>
    /// Generates the list of assets the boot screen preloads.
    ///
    /// The manifest is derived from the Resources folder so it never drifts away
    /// from the art that actually ships. Tier 0 blocks the first playable frame
    /// and therefore holds everything the character creator and the first screen
    /// touch. Tier 1 is warmed up gently in the background once the game runs.
    /// </summary>
    public static class GenesisBootManifestBuilder
    {
        public const string ResourcesRoot = "Assets/Genesis/Resources/";
        public const string ManifestPath = "Assets/Genesis/Resources/Genesis/Data/boot_manifest.txt";

        /// <summary>Assets the first playable frame depends on.</summary>
        private static readonly string[] CriticalExact =
        {
            "Genesis/Data/catalog",
            "Genesis/Data/localization",
            "Genesis/Data/story-localization",
            "Genesis/Data/fairy-halos",
            "Genesis/Fonts/Almendra-Bold",
            "Genesis/Fonts/CrimsonText-Regular",
            "Genesis/Fonts/NotoSans-Regular",
            "Genesis/Art/backgrounds/ivory-story-garden",
            "Genesis/Art/backgrounds/nebula",
            "Genesis/Art/backgrounds/rosewater-garden",
            "Genesis/Art/backgrounds/sage-friendship-garden",
            "Genesis/Art/backgrounds/environments/world_00",
            "Genesis/Audio/MysticalPiano",
            "Genesis/Audio/tap",
            "Genesis/Audio/purchase",
            "Genesis/Audio/chapter",
            "Genesis/Audio/miracle",
            "Genesis/Audio/evolution",
            "Genesis/DemoUnlit.mat"
        };

        private static readonly string[] CriticalPrefixes =
        {
            "Genesis/Art/Branding/",
            "Genesis/Art/portraits/",
            "Genesis/Art/halos/",
            "Genesis/UI/",
            "Genesis/Characters/",
            "Genesis/Licenses/"
        };

        /// <summary>The interface icons the goddess header draws on the first frame.</summary>
        private static readonly string[] CriticalIcons =
        {
            "stellar_nursery", "veiled_sight", "reality", "universal_heart",
            "first_intention", "hope_fluctuation", "soul_dawn", "time_flow"
        };

        /// <summary>
        /// Assets that are never preloaded. Either nothing in the shipped game
        /// reads them, or they are so large that preloading them would cost more
        /// memory than the stall they would prevent.
        /// </summary>
        private static readonly string[] NeverPreload =
        {
            "Genesis/Data/boot_manifest",
            "Genesis/Data/emotion_pose_samples",
            "Genesis/Data/character_hierarchy",
            "Genesis/Data/parity_fixtures",
            "Genesis/Data/save_schema",
            "Genesis/Data/save_template_godot",
            "Genesis/Data/save_template_unity",
            "Genesis/Data/export_report",
            "Genesis/Data/demo_snapshots",
            "Genesis/Character/fairy",
            "Genesis/Fonts/NotoSansCJKsc-Regular"
        };

        [MenuItem("Genesis/Build/Rebuild boot manifest")]
        public static void Rebuild()
        {
            string manifest = Build();
            File.WriteAllText(ManifestPath, manifest);
            AssetDatabase.ImportAsset(ManifestPath, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Genesis boot manifest rebuilt: " + ManifestPath);
        }

        /// <summary>Returns the manifest text without touching the project.</summary>
        public static string Build()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# ZendaGenesis boot manifest");
            builder.AppendLine("# tier bytes resourcePath");
            builder.AppendLine("# tier 0 blocks the first playable frame, tier 1 is warmed up later");

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { ResourcesRoot });
            var paths = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith(ResourcesRoot)) continue;
                if (path.EndsWith(".meta")) continue;
                // Folders are searchable too, but they are not loadable assets.
                if (AssetDatabase.IsValidFolder(path)) continue;
                paths.Add(path);
            }
            paths.Sort();

            long criticalBytes = 0;
            long deferredBytes = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                string resourcePath = ToResourcePath(paths[i]);
                int tier = Classify(resourcePath);
                if (tier < 0) continue;
                long bytes = File.Exists(paths[i]) ? new FileInfo(paths[i]).Length : 0;
                if (tier == 0) criticalBytes += bytes; else deferredBytes += bytes;
                builder.AppendLine(tier + " " + bytes + " " + resourcePath);
            }
            builder.AppendLine("# critical bytes " + criticalBytes);
            builder.AppendLine("# deferred bytes " + deferredBytes);
            return builder.ToString();
        }

        private static string ToResourcePath(string assetPath)
        {
            string relative = assetPath.Substring(ResourcesRoot.Length);
            int dot = relative.LastIndexOf('.');
            return dot > 0 ? relative.Substring(0, dot) : relative;
        }

        /// <summary>Returns 0 for critical, 1 for deferred or -1 to skip the asset.</summary>
        private static int Classify(string resourcePath)
        {
            for (int i = 0; i < NeverPreload.Length; i++)
                if (resourcePath == NeverPreload[i]) return -1;
            for (int i = 0; i < CriticalExact.Length; i++)
                if (resourcePath == CriticalExact[i]) return 0;
            for (int i = 0; i < CriticalPrefixes.Length; i++)
                if (resourcePath.StartsWith(CriticalPrefixes[i])) return 0;
            for (int i = 0; i < CriticalIcons.Length; i++)
                if (resourcePath == "Genesis/Art/icons/" + CriticalIcons[i]) return 0;
            return 1;
        }
    }
}
