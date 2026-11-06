using System;
using System.IO;
using System.Linq;
using EnchantedUI;
using EnchantedUI.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Genesis.Editor
{
    public static class GenesisEnchantedIntegration
    {
        public const string PrefabPath = "Assets/Genesis/Resources/Genesis/UI/EnchantedConversation.prefab";

        [MenuItem("Genesis/Presentation/Install Game-wide Enchanted Theme")]
        public static void InstallGameTheme()
        {
            const string path = "Assets/Genesis/Resources/Genesis/UI/EnchantedGameTheme.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EnchantedTheme theme = AssetDatabase.LoadAssetAtPath<EnchantedTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<EnchantedTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }
            theme.panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/panel_enchanted.png");
            theme.button = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/button_amethyst.png");
            theme.hexSocket = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/hex_portrait_socket.png");
            theme.blossom = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/ornament_blossom.png");
            theme.progressTrack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/progress_track.png");
            theme.progressFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/progress_fill.png");
            theme.divider = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/divider_gold.png");
            theme.spark = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/icon_spark.png");
            theme.diamond = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/icon_diamond.png");
            theme.check = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/icon_check.png");
            theme.back = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/icon_back.png");
            theme.next = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/icon_next.png");
            theme.titleFont = Resources.Load<Font>("Genesis/Fonts/Almendra-Bold");
            theme.bodyFont = Resources.Load<Font>("Genesis/Fonts/CrimsonText-Regular");
            if (theme.panel == null || theme.button == null || theme.hexSocket == null || theme.titleFont == null || theme.bodyFont == null)
                throw new InvalidOperationException("Game-wide Enchanted artwork or fonts missing.");
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Debug.Log("Game-wide Enchanted theme installed; original scenes and progression unchanged.");
        }

        [MenuItem("Genesis/Presentation/Build Enchanted Conversation")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            EnchantedKitInstaller.BuildKit();
            string source = Directory.GetDirectories("Assets/EnchantedUI/Generated")
                .OrderByDescending(p => p, StringComparer.Ordinal).First() + "/Prefabs/ConversationScreen.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            AssetDatabase.Refresh();
            GameObject root = PrefabUtility.LoadPrefabContents(source.Replace('\\', '/'));
            try
            {
                root.name = "Genesis Enchanted Conversation";
                // Height-based mobile readability; wide windows get a centered column.
                root.GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
                root.GetComponent<Canvas>().sortingOrder = 50;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            InstallGameTheme();
            Validate();
        }

        [MenuItem("Genesis/Presentation/Validate Enchanted Conversation")]
        public static void Validate()
        {
            int checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new InvalidOperationException(message); checks++; };
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            check(prefab != null, "Conversation prefab missing.");
            EnchantedDialogueView view = prefab.GetComponent<EnchantedDialogueView>();
            check(view != null, "View missing.");
            check(prefab.GetComponentInChildren<EnchantedDemoStory>(true) == null, "Demo story must not ship in the production prefab.");
            check(prefab.GetComponentsInChildren<Component>(true).All(c => c != null), "Missing script reference.");
            foreach (Text label in new[] { view.title, view.resource, view.chapter, view.speaker, view.body, view.leftName, view.rightName })
                check(label != null && label.font != null, "Missing editable text/font.");
            check(view.continueButton != null && view.previousButton != null && view.backButton != null, "Navigation references missing.");
            check(view.dialogueScroll != null && view.dialogueScroll.content == view.body.rectTransform, "Dialogue scrolling disconnected.");
            check(prefab.GetComponentInChildren<EnchantedSafeArea>(true) != null, "Safe area missing.");
            check(prefab.GetComponentsInChildren<Button>(true).Length == 6, "Expected three actions and three hex navigation controls.");
            foreach (Image image in prefab.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite == null) continue;
                if (image.sprite.name == "panel_enchanted" || image.sprite.name == "button_amethyst")
                    check(image.type == Image.Type.Sliced && image.pixelsPerUnitMultiplier > 1, "Illustrated frame is not sliced.");
            }
            string[] names = { "panel_enchanted", "button_amethyst" };
            Vector4[] borders = { new Vector4(320, 320, 320, 320), new Vector4(540, 268, 540, 268) };
            for (int i = 0; i < names.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/EnchantedUI/Art/" + names[i] + ".png");
                check(sprite != null && sprite.border == borders[i], "Manifest border mismatch: " + names[i]);
            }
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/enchanted-integration-validation.txt", DateTime.UtcNow.ToString("O") + "\nPASS: " + checks + " prefab/import checks.\nUnity " + Application.unityVersion + "\nDevice execution and visual review are separate checks.\n");
            Debug.Log("Enchanted integration: " + checks + " prefab/import checks passed. No original scene saved.");
        }
    }
}