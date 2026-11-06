using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Collects the shader variants the game actually uses into a shader variant
/// collection and registers that collection in the graphics settings.
///
/// Two things were wrong before this tool existed.
///
/// The boot screen called Shader.WarmupAllShaders to get the shaders ready.
/// That call compiles every variant in one blocking step on the main thread.
/// A desktop player survives it, a browser or a phone does not: there the
/// loading screen froze at eighty percent until the boot failed with the
/// scene timeout.
///
/// The build also stripped shaders that the game still needs, so they were
/// reported as "not supported or has been stripped from the build" and the
/// ones that survived had to be compiled on the device while the player was
/// already looking at the game.
///
/// A shader variant collection fixes both. The variants are known at build
/// time, so they stay in the build, and the boot warms them a few at a time
/// instead of in one long block.
/// </summary>
public static class GenesisShaderVariantCollector
{
    public const string CollectionFolder = "Assets/Genesis/Resources/Genesis/Shaders";
    public const string CollectionPath = CollectionFolder + "/GenesisCriticalVariants.asset";
    public const string CollectionName = "GenesisCriticalVariants";

    /// <summary>
    /// Milliseconds of every frame the warm up may use. The warm up runs in
    /// the background while the loading screen is on, so it must never take
    /// a whole frame away from the game.
    /// </summary>
    private const int WarmupBatchMilliseconds = 2;

    /// <summary>
    /// The folders the materials of the game live in.
    /// </summary>
    private static readonly string[] MaterialFolders =
    {
        "Assets/Genesis",
        "Assets/EnchantedUI",
        "Assets/TextMesh Pro"
    };

    /// <summary>
    /// Shaders the game needs that no material in the project points at, so
    /// they would be stripped from the build.
    /// </summary>
    private static readonly string[] CriticalShaderNames =
    {
        "Universal Render Pipeline/Lit",
        "Universal Render Pipeline/Simple Lit",
        "Universal Render Pipeline/Unlit",
        "Universal Render Pipeline/Particles/Unlit",
        "Universal Render Pipeline/2D/Sprite-Unlit-Default",
        "UI/Default",
        "UI/Default Font",
        "Sprites/Default",
        "Skybox/Procedural",
        "TextMeshPro/Distance Field",
        "TextMeshPro/Mobile/Distance Field",
        "VRM/MToon",
        "VRM/MToon10",
        "Hidden/InternalErrorShader"
    };

    /// <summary>One shader and the variants of it that the game asks for.</summary>
    private sealed class ShaderEntry
    {
        public Shader Shader;
        public string Guid;
        public long FileId;
        public readonly List<string[]> Keywords = new List<string[]>();
        public readonly List<PassType> PassTypes = new List<PassType>();
    }

    [MenuItem("Genesis/Shaders/Build Shader Variant Collection")]
    public static void Build()
    {
        List<Material> materials = CollectMaterials();
        List<Shader> shaders = CollectShaders(materials);
        if (shaders.Count == 0)
        {
            Debug.LogWarning("Genesis: no shaders were found, the variant collection was not rebuilt.");
            return;
        }

        List<ShaderEntry> entries = BuildEntries(shaders, materials);
        if (entries.Count == 0)
        {
            Debug.LogWarning("Genesis: no shader variants were found, the variant collection was not rebuilt.");
            return;
        }

        if (!Directory.Exists(CollectionFolder)) Directory.CreateDirectory(CollectionFolder);
        ShaderVariantCollection collection = SaveCollection(entries);
        RegisterInGraphicsSettings(collection);

        Debug.Log("Genesis: shader variant collection rebuilt with " + collection.shaderCount
            + " shaders and " + collection.variantCount + " variants.");
    }

    /// <summary>
    /// Gathers every material the game can put on the screen.
    /// </summary>
    private static List<Material> CollectMaterials()
    {
        var materials = new List<Material>();
        var seen = new HashSet<string>();

        // The materials that live in the project.
        string[] guids = AssetDatabase.FindAssets("t:Material", MaterialFolders);
        for (int i = 0; i < guids.Length; i++)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) continue;
            if (seen.Add(materialPath)) materials.Add(material);
        }

        // The materials the two shipped scenes depend on. The dependencies are
        // read from the asset database, so the scene the developer has open is
        // never touched and no unsaved work is ever lost.
        foreach (string scenePath in new[] { GenesisBuild.BootScenePath, GenesisBuild.ScenePath })
        {
            if (!File.Exists(scenePath)) continue;
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(dependencies[i]);
                if (material == null) continue;
                if (seen.Add(dependencies[i])) materials.Add(material);
            }
        }

        return materials;
    }

    /// <summary>
    /// Gathers every shader the game can put on the screen.
    /// </summary>
    private static List<Shader> CollectShaders(List<Material> materials)
    {
        var found = new List<Shader>();
        var seen = new HashSet<string>();

        for (int i = 0; i < materials.Count; i++)
        {
            Material material = materials[i];
            if (material == null || material.shader == null) continue;
            if (seen.Add(material.shader.name)) found.Add(material.shader);
        }

        // The shaders that nothing in the project points at but the pipeline
        // still needs.
        for (int i = 0; i < CriticalShaderNames.Length; i++)
        {
            Shader shader = Shader.Find(CriticalShaderNames[i]);
            if (shader == null) continue;
            if (seen.Add(shader.name)) found.Add(shader);
        }

        // The shaders that ship with the project itself.
        string[] custom = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Genesis", "Assets/TextMesh Pro" });
        for (int i = 0; i < custom.Length; i++)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(custom[i]));
            if (shader == null) continue;
            if (seen.Add(shader.name)) found.Add(shader);
        }

        return found;
    }

    /// <summary>
    /// Works out the variants of every shader that the game asks for.
    /// </summary>
    private static List<ShaderEntry> BuildEntries(List<Shader> shaders, List<Material> materials)
    {
        var entries = new List<ShaderEntry>();
        var byShader = new Dictionary<Shader, ShaderEntry>();

        for (int i = 0; i < shaders.Count; i++)
        {
            Shader shader = shaders[i];
            string guid;
            long fileId;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out guid, out fileId)) continue;
            if (string.IsNullOrEmpty(guid) || guid.StartsWith("0000000000000000f")) continue;
            var entry = new ShaderEntry { Shader = shader, Guid = guid, FileId = fileId };
            entries.Add(entry);
            byShader[shader] = entry;
        }

        // Every shader gets its plain variant.
        for (int i = 0; i < entries.Count; i++)
        {
            Add(entries[i], null, PassTypeFor(entries[i].Shader));
            Add(entries[i], null, OtherPassTypeFor(entries[i].Shader));
        }

        // And every material adds the variant with the keywords that material
        // asks for. Those are the variants the device has to compile.
        for (int i = 0; i < materials.Count; i++)
        {
            Material material = materials[i];
            if (material == null || material.shader == null) continue;
            ShaderEntry entry;
            if (!byShader.TryGetValue(material.shader, out entry)) continue;
            string[] keywords = KeywordsOf(material);
            if (keywords.Length == 0) continue;
            Add(entry, keywords, PassTypeFor(material.shader));
            Add(entry, keywords, OtherPassTypeFor(material.shader));
        }

        return entries;
    }

    private static void Add(ShaderEntry entry, string[] keywords, PassType passType)
    {
        for (int i = 0; i < entry.Keywords.Count; i++)
        {
            if (entry.PassTypes[i] != passType) continue;
            if (keywords == null && entry.Keywords[i].Length == 0) return;
            if (keywords != null && SameKeywords(entry.Keywords[i], keywords)) return;
        }
        entry.Keywords.Add(keywords ?? new string[0]);
        entry.PassTypes.Add(passType);
    }

    private static bool SameKeywords(string[] a, string[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    /// <summary>
    /// The names of the keywords a material has switched on, both the local
    /// ones of the pipeline and the global ones.
    /// </summary>
    private static string[] KeywordsOf(Material material)
    {
        var names = new List<string>();
        LocalKeyword[] local = material.enabledKeywords;
        if (local != null)
        {
            for (int i = 0; i < local.Length; i++)
            {
                if (!string.IsNullOrEmpty(local[i].name)) names.Add(local[i].name);
            }
        }
        string[] global = material.shaderKeywords;
        if (global != null)
        {
            for (int i = 0; i < global.Length; i++)
            {
                if (!string.IsNullOrEmpty(global[i]) && !names.Contains(global[i])) names.Add(global[i]);
            }
        }
        return names.ToArray();
    }

    /// <summary>
    /// The pipeline shaders are compiled by the scriptable render pipeline,
    /// every other shader by the ordinary pass.
    /// </summary>
    private static PassType PassTypeFor(Shader shader)
    {
        if (shader != null && shader.name.StartsWith("Universal Render Pipeline/"))
            return PassType.ScriptableRenderPipeline;
        return PassType.Normal;
    }

    /// <summary>The other of the two passes a shader can be compiled by.</summary>
    private static PassType OtherPassTypeFor(Shader shader)
    {
        return PassTypeFor(shader) == PassType.ScriptableRenderPipeline
            ? PassType.Normal
            : PassType.ScriptableRenderPipeline;
    }

    /// <summary>
    /// Writes the variants into the collection of the project.
    ///
    /// The collection that is already there is filled again instead of being
    /// replaced, so the identity of the asset never changes. A collection
    /// that is replaced would leave the graphics settings pointing at an
    /// asset that no longer exists, and the graphics settings are read when
    /// the build starts.
    /// </summary>
    private static ShaderVariantCollection SaveCollection(List<ShaderEntry> entries)
    {
        ShaderVariantCollection collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(CollectionPath);
        if (collection == null)
        {
            collection = new ShaderVariantCollection();
            collection.name = CollectionName;
            AssetDatabase.CreateAsset(collection, CollectionPath);
        }
        else collection.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            ShaderEntry entry = entries[i];
            for (int v = 0; v < entry.Keywords.Count; v++)
            {
                try
                {
                    string[] keywords = entry.Keywords[v];
                    if (keywords.Length == 0)
                        collection.Add(new ShaderVariantCollection.ShaderVariant(entry.Shader, entry.PassTypes[v]));
                    else
                        collection.Add(new ShaderVariantCollection.ShaderVariant(entry.Shader, entry.PassTypes[v], keywords));
                }
                catch (System.Exception)
                {
                    // A shader that does not have that pass is not a problem.
                    // It only means the other pass is the one it uses.
                }
            }
        }
        EditorUtility.SetDirty(collection);
        AssetDatabase.SaveAssets();
        return collection;
    }

    /// <summary>
    /// Registers the collection in the graphics settings and gives the warm
    /// up a small budget per frame, so it runs in the background instead of
    /// in one long block that freezes the loading screen.
    /// </summary>
    private static void RegisterInGraphicsSettings(ShaderVariantCollection collection)
    {
        Object[] settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (settings == null || settings.Length == 0) return;
        var serialized = new SerializedObject(settings[0]);

        SerializedProperty preloaded = serialized.FindProperty("m_PreloadedShaders");
        if (preloaded != null)
        {
            bool already = false;
            // A collection that was rebuilt has a new identity, so the old
            // entry is dropped instead of being left behind empty.
            for (int i = 0; i < preloaded.arraySize; i++)
            {
                var element = preloaded.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null)
                {
                    preloaded.DeleteArrayElementAtIndex(i);
                    i--;
                    continue;
                }
                if (element.objectReferenceValue == collection) { already = true; break; }
            }
            if (!already)
            {
                preloaded.InsertArrayElementAtIndex(preloaded.arraySize);
                preloaded.GetArrayElementAtIndex(preloaded.arraySize - 1).objectReferenceValue = collection;
            }
        }

        SetNumber(serialized, "m_PreloadShadersBatchTimeLimit", WarmupBatchMilliseconds);
        SetNumber(serialized, "m_WarmupProgressivelyLimit", WarmupBatchMilliseconds);
        SetNumber(serialized, "m_WarmupAsync", 1);

        serialized.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Writes a number into a setting of the graphics settings.
    ///
    /// The settings are not all the same kind. The warm up switch is a yes
    /// or no, the budgets are numbers, and Unity refuses a number that does
    /// not fit the kind of the setting, so the kind is read first.
    /// </summary>
    private static void SetNumber(SerializedObject serialized, string propertyPath, int value)
    {
        SerializedProperty property = serialized.FindProperty(propertyPath);
        if (property == null) return;
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                property.intValue = value;
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = value != 0;
                break;
            case SerializedPropertyType.Float:
                property.floatValue = value;
                break;
        }
    }
}
