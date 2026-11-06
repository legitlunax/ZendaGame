using System;
using System.IO;
using Genesis;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GenesisDemoEditor
{
    private const string ScenePath = "Assets/Genesis/Scenes/GenesisDemo.unity";
    static GenesisDemoEditor()
    {
        EditorApplication.delayCall += Connect;
        EditorApplication.update += Poll;
    }

    [MenuItem("Genesis/Connect local MCP")]
    public static async void Connect()
    {
        if (EditorApplication.isCompiling || Application.isBatchMode) return;
        try
        {
            EditorConfigurationCache.Instance.SetUseHttpTransport(true);
            EditorConfigurationCache.Instance.SetHttpBaseUrl("http://127.0.0.1:8080");
            EditorConfigurationCache.Instance.SetHttpTransportScope("local");
            if (!MCPServiceLocator.Bridge.IsRunning)
                Debug.Log("Genesis MCP connected: " + await MCPServiceLocator.Bridge.StartAsync());
        }
        catch (Exception error) { Debug.LogWarning("Genesis MCP: " + error.Message); }
    }

    private static void Poll()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string request = Path.GetFullPath("Tools/MCP/create-demo.request");
        if (!File.Exists(request)) return;
        File.Delete(request);
        CreateDemo();
    }

    [MenuItem("Genesis/Create playtest scene")]
    public static void CreateDemo()
    {
        Directory.CreateDirectory("Assets/Genesis/Scenes");
        if (File.Exists(ScenePath)) { Debug.Log("Demo scene already exists: " + ScenePath); return; }
        Scene previous = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(previous.path))
        {
            Directory.CreateDirectory("Assets/Genesis/PreservedScenes");
            string preserved = AssetDatabase.GenerateUniqueAssetPath("Assets/Genesis/PreservedScenes/BeforeDemo.unity");
            if (!EditorSceneManager.SaveScene(previous, preserved))
                throw new IOException("Could not preserve the unsaved editor scene.");
        }
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        GameObject root = new GameObject("Genesis Playtest");
        root.AddComponent<GenesisDemo>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.CloseScene(scene, true);
        if (previous.IsValid()) SceneManager.SetActiveScene(previous);
        AssetDatabase.Refresh();
        Debug.Log("GENESIS DEMO READY: " + ScenePath);
    }

    [MenuItem("Genesis/Build Windows playtest")]
    public static void BuildDemo()
    {
        if (!File.Exists(ScenePath)) CreateDemo();
        const string materialPath = "Assets/Genesis/Resources/Genesis/DemoUnlit.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Unlit shader unavailable.");
            AssetDatabase.CreateAsset(new Material(shader), materialPath);
            AssetDatabase.SaveAssets();
        }
        Directory.CreateDirectory("Builds/GenesisDemo");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath }, locationPathName = "Builds/GenesisDemo/GenesisDemo.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
        });
        File.WriteAllText("Tools/MCP/build-result.txt", report.summary.result + " errors=" + report.summary.totalErrors);
        Debug.Log("GENESIS BUILD: " + report.summary.result);
    }
}