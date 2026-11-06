using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnchantedUI.Editor
{
    // Scoped to this kit. Does not modify your game's other texture settings.
    public sealed class EnchantedArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/EnchantedUI/Art/") || !assetPath.EndsWith(".png")) return;
            Configure((TextureImporter)assetImporter, Path.GetFileNameWithoutExtension(assetPath));
        }

        internal static void Configure(TextureImporter importer, string name)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spriteBorder = BorderFor(name);
        }

        internal static Vector4 BorderFor(string name)
        {
            // Unity order: left, bottom, right, top; original source pixels.
            if (name == "panel_enchanted") return new Vector4(320, 320, 320, 320);
            if (name == "button_amethyst") return new Vector4(540, 268, 540, 268);
            if (name == "progress_track" || name == "progress_fill") return new Vector4(24, 24, 24, 24);
            return Vector4.zero;
        }
    }
}
